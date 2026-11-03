namespace StudyEverywhere
{
    // Timing only: independent of tracking hardware so loss/recovery and noisy input are testable.
    // A tap must follow a stable release. Invalid input cancels; it never counts as a release.
    public sealed class HandTapGate
    {
        public bool Touching { get; private set; }
        public bool Consumed { get; private set; }
        public float StartedAt { get; private set; }
        public int Direction { get; private set; }
        bool armed;
        float releasedAt = -1;

        public void Reset()
        {
            Touching = Consumed = armed = false;
            releasedAt = -1;
            Direction = 0;
        }

        public void Consume() => Consumed = true;

        public int Sample(bool valid, bool contact, bool released, int direction, float now,
            float minimumHold, float maximumHold, float releaseDwell)
        {
            if (!valid) { Reset(); return 0; }
            if (!Touching)
            {
                if (released)
                {
                    if (releasedAt < 0) releasedAt = now;
                    if (now - releasedAt >= releaseDwell) armed = true;
                }
                else releasedAt = -1;
                if (armed && contact)
                {
                    armed = false;
                    Touching = true;
                    Consumed = false;
                    StartedAt = now;
                    Direction = direction;
                }
                return 0;
            }
            if (!released) return 0;
            Touching = false;
            releasedAt = now;
            float duration = now - StartedAt;
            return !Consumed && duration >= minimumHold && duration <= maximumHold ? Direction : 0;
        }
    }
}
