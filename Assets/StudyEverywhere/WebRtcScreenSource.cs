using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Newtonsoft.Json.Linq;
using Unity.WebRTC;
using UnityEngine;

namespace StudyEverywhere
{
    // The real IScreenSource: every display of a Windows PC running the New World Desktop app, over the local network
    // (docs/monitor-connection/protocol.md). Pairing listens for the desktop app's UDP beacons and opens a signalling
    // connection to each PC heard; the one where the user types the headset's code and approves becomes the session.
    // The video is WebRTC, H.264 on the Quest's hardware decoder, one stream per display, host candidates only: nothing
    // leaves the LAN and there is no server in the path. The code also signs both sides' DTLS fingerprints, so no other
    // device on the network can slip into the middle.
    public sealed class WebRtcScreenSource : MonoBehaviour, IScreenSource
    {
        public const int BeaconPort = 47800;
        public float codeLifetime = 120f;
        [Tooltip("Seconds between stats polls; each poll that sees new decoded frames raises Frame.")]
        public float framePoll = 0.25f;

        public event Action<string, float> CodeIssued;
        public event Action Approved;
        public event Action<SharedDisplay> DisplayAdded;
        public event Action<int> DisplayRemoved;
        public event Action<int> Frame;
        public event Action<string> Ended;

        // One signalling socket to one PC. Reading runs on its own thread; lines are handed to the main thread.
        sealed class Link
        {
            public IPEndPoint endpoint;
            public TcpClient tcp;
            public NetworkStream stream;
            public volatile bool closed;
            public float lastHeard, lastPing;

            public void Send(JObject message)
            {
                if (closed || stream == null) return;
                try
                {
                    var bytes = Encoding.UTF8.GetBytes(message.ToString(Newtonsoft.Json.Formatting.None) + "\n");
                    stream.Write(bytes, 0, bytes.Length);
                }
                catch (Exception) { closed = true; }
            }

            public void Close()
            {
                closed = true;
                try { tcp?.Close(); } catch (Exception) { }
            }
        }

        sealed class Display
        {
            public SharedDisplay info;
            public string mid;
            public VideoStreamTrack track;
            public bool announced;
            public bool removed;   // stopped on either side; its stream stays in the connection and "Share again" reuses it
            public long decoded;
        }

        enum Phase { Idle, Pairing, Connecting, Streaming }

        Phase phase;
        string code;
        byte[] nonce;
        UdpClient beacons;
        Thread beaconThread;
        readonly ConcurrentQueue<Action> mainThread = new ConcurrentQueue<Action>();
        readonly Dictionary<string, Link> links = new Dictionary<string, Link>();   // by PC address, while pairing
        Link session;
        RTCPeerConnection pc;
        readonly Dictionary<int, Display> displays = new Dictionary<int, Display>();
        readonly Dictionary<string, int> midToDisplay = new Dictionary<string, int>();
        Coroutine webrtcUpdate, polling;
        AndroidJavaObject multicastLock;

        public void Pair()
        {
            Close(null);
            phase = Phase.Pairing;
            using (var rng = RandomNumberGenerator.Create()) { nonce = new byte[16]; rng.GetBytes(nonce); }
            code = SecureCode();
            ListenForBeacons();
            CodeIssued?.Invoke(code, codeLifetime);
        }

        public void Cancel() => Close(null);

        public void Stop()
        {
            session?.Send(new JObject { ["type"] = "stop", ["reason"] = "Sharing was stopped in the headset." });
            Close(null);
        }

        public void StopDisplay(int id)
        {
            session?.Send(new JObject { ["type"] = "stop-display", ["id"] = id });
            if (displays.TryGetValue(id, out var d)) { d.announced = false; d.removed = true; }
        }

        // ---- Travelling between places ----------------------------------------------------------------------------
        // One source for the whole app, kept across scene loads, so sharing carries on into the next place that has a
        // workspace. A workspace that goes away (the player left its place) lets it go; if no workspace takes it back
        // within a few seconds, sharing stops, so the computer is never captured with nothing showing it.

        static WebRtcScreenSource shared;
        public static WebRtcScreenSource Shared
        {
            get
            {
                if (shared) return shared;
                var go = new GameObject("Screen sharing");
                DontDestroyOnLoad(go);
                return shared = go.AddComponent<WebRtcScreenSource>();
            }
        }

        [Tooltip("Seconds sharing waits for the next place's workspace before it stops.")]
        public float releaseGrace = 8f;
        object holder;
        float releasedAt = -1;

        public bool IsSharing => phase == Phase.Connecting || phase == Phase.Streaming;
        public IEnumerable<SharedDisplay> ShownDisplays => displays.Values.Where(d => d.announced && !d.removed).Select(d => d.info);

        public void Claim(object owner) { holder = owner; releasedAt = -1; }

        public void Release(object owner)
        {
            if (holder != owner) return;
            holder = null;
            releasedAt = phase == Phase.Idle ? -1 : Time.unscaledTime;
        }

        static string SecureCode()
        {
            var bytes = new byte[4];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(bytes);
            return (BitConverter.ToUInt32(bytes, 0) % 1000000).ToString("000000");
        }

        // ---- Discovery -------------------------------------------------------------------------------------------

        void ListenForBeacons()
        {
            AcquireMulticastLock();
            try
            {
                beacons = new UdpClient();
                beacons.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                beacons.EnableBroadcast = true;
                beacons.Client.Bind(new IPEndPoint(IPAddress.Any, BeaconPort));
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Screens] Can't listen for computers: " + e.Message);
                return;
            }
            var socket = beacons;
            beaconThread = new Thread(() =>
            {
                var from = new IPEndPoint(IPAddress.Any, 0);
                while (true)
                {
                    byte[] data;
                    try { data = socket.Receive(ref from); }
                    catch (Exception) { return; }   // closed
                    var sender = from.Address;
                    var text = Encoding.UTF8.GetString(data);
                    mainThread.Enqueue(() => OnBeacon(sender, text));
                }
            }) { IsBackground = true, Name = "New World beacons" };
            beaconThread.Start();
        }

        void OnBeacon(IPAddress address, string text)
        {
            if (phase != Phase.Pairing) return;
            JObject beacon;
            try { beacon = JObject.Parse(text); } catch (Exception) { return; }
            if ((string)beacon["app"] != "newworld-desktop" || (int?)beacon["v"] != 1) return;
            int port = (int?)beacon["port"] ?? 47801;
            string key = address + ":" + port;
            if (links.ContainsKey(key)) return;
            Debug.Log($"[Screens] Found {(string)beacon["name"]} at {key}");
            var link = new Link { endpoint = new IPEndPoint(address, port), lastHeard = Time.time, lastPing = Time.time };
            links[key] = link;
            Open(link);
        }

        // Connects on a thread, says hello, then reads lines until the socket closes.
        void Open(Link link)
        {
            var hello = new JObject
            {
                ["type"] = "hello", ["v"] = 1, ["device"] = DeviceName(), ["nonce"] = Hex(nonce),
                ["proof"] = Mac("hello:" + Hex(nonce)),
            };
            new Thread(() =>
            {
                try
                {
                    link.tcp = new TcpClient { NoDelay = true };
                    link.tcp.Connect(link.endpoint);
                    link.stream = link.tcp.GetStream();
                    link.Send(hello);
                    var buffer = new byte[64 * 1024];
                    var line = new StringBuilder();
                    var decoder = Encoding.UTF8.GetDecoder();
                    var chars = new char[buffer.Length];
                    while (!link.closed)
                    {
                        int n = link.stream.Read(buffer, 0, buffer.Length);
                        if (n <= 0) break;
                        int c = decoder.GetChars(buffer, 0, n, chars, 0);
                        for (int i = 0; i < c; i++)
                        {
                            if (chars[i] != '\n') { line.Append(chars[i]); continue; }
                            var message = line.ToString();
                            line.Clear();
                            if (message.Length > 0) mainThread.Enqueue(() => OnMessage(link, message));
                        }
                    }
                }
                catch (Exception e)
                {
                    if (!link.closed) Debug.Log($"[Screens] {link.endpoint}: {e.Message}");
                }
                if (!link.closed) mainThread.Enqueue(() => OnClosed(link));
                link.closed = true;
            }) { IsBackground = true, Name = "New World link " + link.endpoint }.Start();
        }

        // ---- Signalling ------------------------------------------------------------------------------------------

        void OnMessage(Link link, string text)
        {
            if (link.closed) return;
            link.lastHeard = Time.time;
            JObject m;
            try { m = JObject.Parse(text); } catch (Exception) { return; }
            string type = (string)m["type"];
            if (session != null && link != session) return;
            switch (type)
            {
                case "ping": link.Send(new JObject { ["type"] = "pong" }); break;
                case "declined": Close((string)m["reason"] ?? "Sharing was declined on the computer."); break;
                case "approved": OnApproved(link, m); break;
                case "offer": if (link == session) StartCoroutine(Answer(m, signed: true)); break;
                case "display-added":
                    if (link != session) break;
                    Expect(m["display"] as JObject);
                    StartCoroutine(Answer(m, signed: false));
                    break;
                case "display-removed":
                    int gone = (int?)m["id"] ?? 0;
                    if (displays.TryGetValue(gone, out var d) && !d.removed)
                    {
                        bool shown = d.announced;
                        d.announced = false; d.removed = true;
                        if (shown) DisplayRemoved?.Invoke(gone);
                    }
                    break;
                case "stop": Close((string)m["reason"] ?? "Sharing was stopped on the computer."); break;
            }
        }

        void OnClosed(Link link)
        {
            if (link == session) { Close("Lost the connection to the computer."); return; }
            foreach (var key in links.Where(p => p.Value == link).Select(p => p.Key).ToList()) links.Remove(key);
        }

        // The computer accepted the code and the user approved: this PC is the session, every other one is let go.
        void OnApproved(Link link, JObject m)
        {
            if (phase != Phase.Pairing) return;
            session = link;
            foreach (var other in links.Values) if (other != link) other.Close();
            links.Clear();
            StopListening();
            phase = Phase.Connecting;
            displays.Clear();
            foreach (var d in (m["displays"] as JArray) ?? new JArray()) Expect(d as JObject);
            webrtcUpdate ??= StartCoroutine(WebRTC.Update());
            Approved?.Invoke();
        }

        void Expect(JObject d)
        {
            if (d == null) return;
            int id = (int?)d["id"] ?? 0;
            if (id <= 0) return;
            var info = new SharedDisplay
            {
                id = id, name = (string)d["name"] ?? "Display " + id, primary = (bool?)d["primary"] ?? false,
                layout = new RectInt((int?)d["x"] ?? 0, (int?)d["y"] ?? 0, (int?)d["width"] ?? 1920, (int?)d["height"] ?? 1080),
                sizeMm = new Vector2((float?)d["widthMm"] ?? 0, (float?)d["heightMm"] ?? 0),
            };
            if (displays.TryGetValue(id, out var known))
            {
                if (!known.removed) return;   // display-added repeats the displays already shown
                info.texture = known.info.texture;   // shared again: same stream, panel reopens once the answer is sent
                known.info = info;
                known.removed = false;
                return;
            }
            displays[id] = new Display { info = info };
        }

        // After a renegotiation, a display shared again whose stream already has a picture is shown at once (its
        // track was set up the first time, so no new OnTrack comes).
        void AnnounceReady()
        {
            foreach (var d in displays.Values.ToList())
            {
                if (d.announced || d.removed || d.track == null || d.track.Texture == null) continue;
                d.info.texture = d.track.Texture;
                d.announced = true;
                DisplayAdded?.Invoke(d.info);
            }
        }

        // Sets the computer's offer, answers with H.264 first (the hardware decoder) after gathering every candidate.
        IEnumerator Answer(JObject m, bool signed)
        {
            string sdp = (string)m["sdp"];
            if (string.IsNullOrEmpty(sdp)) yield break;
            if (signed && !Equal((string)m["mac"], Mac("offer:" + Fingerprint(sdp))))
            {
                Close("The connection could not be verified. Retry for a new code.");
                yield break;
            }
            if (m["mids"] is JObject mids)
                foreach (var p in mids) midToDisplay[p.Key] = (int)p.Value;

            if (pc == null)
            {
                var config = new RTCConfiguration { iceServers = new RTCIceServer[0] };   // host candidates only: the stream stays on the LAN
                pc = new RTCPeerConnection(ref config);
                pc.OnTrack = e => { if (e.Track is VideoStreamTrack v) OnTrack(v, e.Transceiver.Mid); };
                pc.OnConnectionStateChange = s =>
                {
                    Debug.Log("[Screens] Connection " + s);
                    if (s == RTCPeerConnectionState.Failed) mainThread.Enqueue(() => Close("Couldn't reach the computer. Check it is on the same Wi-Fi as the headset."));
                };
            }

            var remote = new RTCSessionDescription { type = RTCSdpType.Offer, sdp = sdp };
            var setRemote = pc.SetRemoteDescription(ref remote);
            yield return setRemote;
            if (setRemote.IsError) { Close("The computer's stream could not be set up: " + setRemote.Error.message); yield break; }

            var codecs = RTCRtpReceiver.GetCapabilities(TrackKind.Video).codecs;
            var ordered = codecs.Where(c => c.mimeType == "video/H264").Concat(codecs.Where(c => c.mimeType != "video/H264")).ToArray();
            foreach (var t in pc.GetTransceivers()) t.SetCodecPreferences(ordered);

            var create = pc.CreateAnswer();
            yield return create;
            if (create.IsError) { Close("The stream could not be answered: " + create.Error.message); yield break; }
            var local = create.Desc;
            var setLocal = pc.SetLocalDescription(ref local);
            yield return setLocal;
            if (setLocal.IsError) { Close("The stream could not be answered: " + setLocal.Error.message); yield break; }

            float until = Time.time + 3f;
            while (pc != null && pc.GatheringState != RTCIceGatheringState.Complete && Time.time < until) yield return null;
            if (pc == null || session == null) yield break;

            string answer = pc.LocalDescription.sdp;
            var reply = new JObject { ["type"] = "answer", ["sdp"] = answer };
            if (signed) { reply["mac"] = Mac("answer:" + Fingerprint(answer)); code = null; }   // the code is spent
            session.Send(reply);
            if (phase == Phase.Connecting) phase = Phase.Streaming;
            AnnounceReady();
            polling ??= StartCoroutine(PollFrames());
        }

        void OnTrack(VideoStreamTrack track, string mid)
        {
            if (mid == null || !midToDisplay.TryGetValue(mid, out int id) || !displays.TryGetValue(id, out var d)) { track.Dispose(); return; }
            d.mid = mid;
            d.track = track;
            track.OnVideoReceived += texture =>
            {
                d.info.texture = texture;
                if (d.announced || d.removed) return;
                d.announced = true;
                DisplayAdded?.Invoke(d.info);
            };
        }

        // com.unity.webrtc only reports a track's texture once, so liveness is polled. Screen capture only sends a frame
        // when the screen changes (a still desktop can be ~1 fps or less), so a display counts as live while the
        // connection is up, and new decoded frames also count; a dropped network stops both and the panel pauses.
        IEnumerator PollFrames()
        {
            var wait = new WaitForSeconds(framePoll);
            while (pc != null)
            {
                yield return wait;
                if (pc == null) break;
                if (pc.ConnectionState == RTCPeerConnectionState.Connected)
                {
                    foreach (var d in displays.Values.Where(d => d.announced).ToList()) Frame?.Invoke(d.info.id);
                    continue;
                }
                var op = pc.GetStats();
                yield return op;
                if (op.IsError || pc == null) continue;
                using (var report = op.Value)
                    foreach (var s in report.Stats.Values)
                    {
                        if (s.Type != RTCStatsType.InboundRtp) continue;
                        var dict = s.Dict;
                        if (!dict.TryGetValue("mid", out var mid) || mid == null) continue;
                        if (!midToDisplay.TryGetValue(mid.ToString(), out int id) || !displays.TryGetValue(id, out var d) || !d.announced) continue;
                        long decoded = dict.TryGetValue("framesDecoded", out var f) && f != null ? Convert.ToInt64(f) : 0;
                        if (decoded > d.decoded) { d.decoded = decoded; Frame?.Invoke(id); }
                    }
            }
            polling = null;
        }

        void Update()
        {
            while (mainThread.TryDequeue(out var work)) work();
            if (releasedAt >= 0 && holder == null && Time.unscaledTime - releasedAt > releaseGrace)
            {
                releasedAt = -1;
                if (phase != Phase.Idle) { Debug.Log("[Screens] No workspace here; sharing stopped."); Stop(); }
            }
            if (session == null) return;
            if (Time.time - session.lastPing > 5f) { session.lastPing = Time.time; session.Send(new JObject { ["type"] = "ping" }); }
            if (Time.time - session.lastHeard > 15f) Close("Lost the connection to the computer.");
        }

        // ---- Teardown --------------------------------------------------------------------------------------------

        // Ends everything; with a reason, the user is told why (Ended), without one it was asked for here.
        void Close(string reason)
        {
            bool wasActive = phase != Phase.Idle;
            phase = Phase.Idle;
            code = null;
            StopListening();
            foreach (var link in links.Values) link.Close();
            links.Clear();
            session?.Close();
            session = null;
            if (polling != null) { StopCoroutine(polling); polling = null; }
            foreach (var d in displays.Values) d.track?.Dispose();
            displays.Clear();
            midToDisplay.Clear();
            if (pc != null) { pc.OnTrack = null; pc.OnConnectionStateChange = null; pc.Close(); pc.Dispose(); pc = null; }
            if (wasActive && reason != null) Ended?.Invoke(reason);
        }

        void StopListening()
        {
            try { beacons?.Close(); } catch (Exception) { }
            beacons = null;
            beaconThread = null;
            ReleaseMulticastLock();
        }

        void OnDestroy()
        {
            Close(null);
            if (webrtcUpdate != null) StopCoroutine(webrtcUpdate);
        }

        // ---- Helpers ---------------------------------------------------------------------------------------------

        string Mac(string message)
        {
            using (var h = new HMACSHA256(Encoding.UTF8.GetBytes(code ?? "")))
                return Hex(h.ComputeHash(Encoding.UTF8.GetBytes(message)));
        }

        static string Fingerprint(string sdp)
        {
            foreach (var raw in sdp.Split('\n'))
            {
                var line = raw.Trim();
                if (line.StartsWith("a=fingerprint:", StringComparison.Ordinal)) return line.Substring("a=fingerprint:".Length).Trim();
            }
            return "";
        }

        static string Hex(byte[] bytes)
        {
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        // Constant-time comparison, so a wrong mac reveals nothing about the right one.
        static bool Equal(string a, string b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }

        static string DeviceName()
        {
            var model = SystemInfo.deviceModel ?? "";
            return model.Contains("Quest") ? model.Replace("Oculus", "Meta") : "Meta Quest";
        }

        // Some Wi-Fi drivers drop broadcast packets unless an app holds a multicast lock.
        void AcquireMulticastLock()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var wifi = activity.Call<AndroidJavaObject>("getSystemService", "wifi"))
                {
                    multicastLock = wifi.Call<AndroidJavaObject>("createMulticastLock", "newworld-screens");
                    multicastLock.Call("acquire");
                }
            }
            catch (Exception e) { Debug.LogWarning("[Screens] No multicast lock: " + e.Message); multicastLock = null; }
#endif
        }

        void ReleaseMulticastLock()
        {
            if (multicastLock == null) return;
            try { multicastLock.Call("release"); } catch (Exception) { }
            multicastLock.Dispose();
            multicastLock = null;
        }
    }
}
