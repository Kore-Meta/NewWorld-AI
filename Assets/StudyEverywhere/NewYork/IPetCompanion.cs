using UnityEngine;

namespace StudyEverywhere
{
    // A café pet that talks through PetVoiceLink (New York's Bubble, Seoul's Hori): what the link needs from its body.
    public interface IPetCompanion
    {
        string DisplayName { get; }
        Transform transform { get; }
        CatBubble Bubble { get; }
        AudioSource Speech { get; set; }   // its speaking voice: the mouth follows its sound
        void Attend(float seconds = 12);   // spoken to: wake, turn to the person, keep looking while they talk
        string Describe();                 // what it is doing, for the relay (never the person's words)
        void Do(string command);           // one action from the relay
        string Unreachable { get; }        // said in its bubble when the relay can't be reached
        string UnreachableAction { get; }  // and done
    }
}
