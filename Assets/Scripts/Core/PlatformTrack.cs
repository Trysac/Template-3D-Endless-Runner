using System.Collections.Generic;

namespace GauntletRun.Core
{
    /// <summary>
    /// The road: platforms of a fixed length that slide towards the player and a new one that
    /// appears far ahead to continue it.
    /// </summary>
    public sealed class PlatformTrack
    {
        readonly List<float> platforms = new List<float>();
        float timer;

        public PlatformTrack(float length, float spawnZ, float speed)
        {
            Length = length;
            SpawnZ = spawnZ;
            Speed = speed;
        }

        public float Length { get; }
        public float SpawnZ { get; }
        public float Speed { get; }

        /// <summary>The z of every platform spawned so far, oldest first.</summary>
        public IReadOnlyList<float> Platforms => platforms;

        /// <summary>Platforms alternate: connection pieces first, then trap pieces.</summary>
        public static bool IsTrap(int index) => index % 2 == 1;

        /// <summary>The seconds between two platforms.</summary>
        public float Interval => Length / Speed;

        /// <summary>
        /// Moves the road and returns how many platforms were added. A new platform starts
        /// exactly where the last one ends, so the road has no gaps or overlaps at any speed.
        /// </summary>
        public int Tick(float dt)
        {
            for (int i = 0; i < platforms.Count; i++)
            {
                platforms[i] -= Speed * dt;
            }
            int added = 0;
            if (platforms.Count == 0)
            {
                timer += dt;
                if (timer < Interval)
                {
                    return 0;
                }
                platforms.Add(SpawnZ - Speed * (timer - Interval));
                added++;
            }
            while (platforms[platforms.Count - 1] + Length <= SpawnZ)
            {
                platforms.Add(platforms[platforms.Count - 1] + Length);
                added++;
            }
            return added;
        }
    }
}
