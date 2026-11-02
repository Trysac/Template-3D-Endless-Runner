namespace GauntletRun.Core
{
    /// <summary>
    /// The clock of a run: one metre (and its points) every distance interval while the run is
    /// active, the road, and a pickup every pickup interval.
    /// </summary>
    public sealed class RunTimeline
    {
        readonly float distanceInterval;
        readonly float pickupInterval;
        float distanceTimer;
        float pickupTimer;

        public RunTimeline(float distanceInterval, float pickupInterval, PlatformTrack track)
        {
            this.distanceInterval = distanceInterval;
            this.pickupInterval = pickupInterval;
            Track = track;
        }

        public ScoreKeeper Score { get; } = new ScoreKeeper();
        public PlatformTrack Track { get; }
        public bool Active { get; private set; } = true;

        public void End() => Active = false;

        /// <summary>Advances the run; reports the platforms and pickups to spawn this step. An ended run stays still.</summary>
        public void Tick(float dt, out int platformsAdded, out int pickupsAdded)
        {
            platformsAdded = 0;
            pickupsAdded = 0;
            if (!Active)
            {
                return;
            }
            distanceTimer += dt;
            while (distanceTimer >= distanceInterval)
            {
                distanceTimer -= distanceInterval;
                Score.AddDistance(1);
            }
            platformsAdded = Track.Tick(dt);
            pickupTimer += dt;
            while (pickupTimer >= pickupInterval)
            {
                pickupTimer -= pickupInterval;
                pickupsAdded++;
            }
        }
    }
}
