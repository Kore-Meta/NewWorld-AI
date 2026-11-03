using System.Collections.Generic;

namespace StudyEverywhere
{
    // Who has the music's attention. A Play or Listen button that starts a sound claims it, and releases it when the
    // sound stops, ends, is interrupted or its object is put away; while any claim is held the place's background music
    // (ParisMusic) fades out, and it fades back in only after the last claim is released. Owners that were destroyed
    // without releasing are dropped, so a lost claim never keeps the music down.
    public static class MusicFocus
    {
        static readonly HashSet<object> owners = new HashSet<object>();

        public static void Claim(object owner) { if (owner != null) owners.Add(owner); }
        public static void Release(object owner) { if (owner != null) owners.Remove(owner); }
        public static void Set(object owner, bool claimed) { if (claimed) Claim(owner); else Release(owner); }

        public static bool Busy
        {
            get
            {
                if (owners.Count == 0) return false;
                owners.RemoveWhere(o => o is UnityEngine.Object u && !u);   // destroyed without releasing
                return owners.Count > 0;
            }
        }
    }
}
