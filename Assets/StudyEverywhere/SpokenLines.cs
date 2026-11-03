using System.Collections.Generic;
using System.Text;
using Meta.WitAi.TTS.Integrations;
using Meta.WitAi.TTS.Utilities;

namespace StudyEverywhere
{
    // What the AI characters (Orbit, Hori, Calypso) share for speaking through the Voice SDK's text-to-speech.
    public static class SpokenLines
    {
        // Wit's text-to-speech takes at most 250 characters a request (longer text was cut off), so a reply goes to the
        // speaker sentence by sentence, joined up to that length, and is played as one queue.
        public static IEnumerable<string> Sentences(string text, int max = 240)
        {
            var part = new StringBuilder();
            foreach (var raw in System.Text.RegularExpressions.Regex.Split(text.Trim(), @"(?<=[.!?…])\s+"))
            {
                var s = raw.Trim(); if (s.Length == 0) continue;
                if (part.Length > 0 && part.Length + 1 + s.Length > max) { yield return part.ToString(); part.Clear(); }
                while (s.Length > max)   // one very long sentence: break it at a space
                {
                    int cut = s.LastIndexOf(' ', max); if (cut < max / 2) cut = max;
                    if (part.Length > 0) { yield return part.ToString(); part.Clear(); }
                    yield return s.Substring(0, cut).Trim(); s = s.Substring(cut).Trim();
                }
                if (part.Length > 0) part.Append(' ');
                part.Append(s);
            }
            if (part.Length > 0) yield return part.ToString();
        }

        // A character's voice: a Wit preset voice at a pitch.
        public static void Voice(TTSSpeaker speaker, string voiceName, int pitch)
            => speaker.SetVoiceOverride(new TTSWitVoiceSettings { voice = voiceName, pitch = pitch });
    }
}
