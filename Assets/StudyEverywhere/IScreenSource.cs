using System;
using UnityEngine;

namespace StudyEverywhere
{
    // One display the computer is sharing. Layout is the display's rectangle on the Windows desktop in logical
    // (DPI-scaled) units, y down, so the headset can arrange panels the way the monitors stand on the desk.
    public struct SharedDisplay
    {
        public int id;
        public string name;
        public bool primary;
        public RectInt layout;
        public Texture texture;   // frames are drawn into this texture
    }

    // Transport for the workspace computer connection: every display of one Windows computer over the local network,
    // one encrypted stream each. The real one (WebRTC from the New World desktop app) and SimulatedScreenSource
    // both implement it. Events report what the computer did; calls made from the headset raise none.
    // Implementations raise every event on the main thread.
    public interface IScreenSource
    {
        // Pairing started: a short-lived code for the user to enter in the desktop app, valid for this many seconds.
        event Action<string, float> CodeIssued;
        // The user approved capture on the computer and chose which displays to share; the streams are being set up.
        event Action Approved;
        // A display's stream is up: once per shared display after approval, and later for a newly attached monitor
        // once the user has confirmed it on the computer (never silently).
        event Action<SharedDisplay> DisplayAdded;
        // A display stopped on the computer's side: unplugged, or unshared there.
        event Action<int> DisplayRemoved;
        // A new frame landed in that display's texture. Silence means its stream has stalled.
        event Action<int> Frame;
        // The far side or the network ended the whole session (declined, stopped on the computer, lost), with a reason for the user.
        event Action<string> Ended;

        void Pair();                  // request a pairing code and wait for approval
        void Cancel();                // abandon pairing or connecting
        void Stop();                  // stop sharing everything; the computer stops capturing
        void StopDisplay(int id);     // stop sharing one display
    }
}
