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
        public float Interval => Length / Speed - 0.2f;

        /// <summary>Moves the road and returns how many platforms were added.</summary>
        public int Tick(float dt)
        {
            for (int i = 0; i < platforms.Count; i++)
            {
                platforms[i] -= Speed * dt;
            }
            int added = 0;
            timer += dt;
            while (timer >= Interval)
            {
                timer -= Interval;
                platforms.Add(SpawnZ - Speed * timer);
                added++;
            }
            return added;
        }
    }
}
