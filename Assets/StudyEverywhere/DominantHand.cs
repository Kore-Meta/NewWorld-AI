using System;
using UnityEngine;

namespace StudyEverywhere
{
    // The person's dominant hand, as they set it (Right unless they choose Left). It places controls meant for that
    // hand, such as Haven's Explore time pill on the matching armrest; either hand can still use them. Not the watch
    // wrist (WristWatch.Worn) and not any interactor's handedness. Kept on the device, like the other settings.
    public static class DominantHand
    {
        const string Key = "StudyEverywhere.DominantHand";
        public static event Action Changed;

        public static bool Left
        {
            get => PlayerPrefs.GetInt(Key, 0) == 1;
            set
            {
                if (value == Left) return;
                PlayerPrefs.SetInt(Key, value ? 1 : 0); PlayerPrefs.Save();
                Changed?.Invoke();
            }
        }
    }
}
