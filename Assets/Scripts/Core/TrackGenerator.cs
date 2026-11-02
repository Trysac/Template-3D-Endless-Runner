using System;
using System.Collections.Generic;

namespace GauntletRun.Core
{
    /// <summary>
    /// Lays the road out of patterns, one after another, picking harder ones as the distance
    /// grows and stretching them so the time between rows stays fair at higher speeds. The same
    /// seed always builds the same road.
    /// </summary>
    public sealed class TrackGenerator : ITrackSource
    {
        static readonly PickupKind[] PowerUps = { PickupKind.Shield, PickupKind.Magnet, PickupKind.Double };

        readonly Random random;
        float nextZ;
        int nextId = 1;

        public TrackGenerator(int seed, float start = Tuning.ClearStart)
        {
            random = new Random(seed);
            nextZ = start;
        }

        /// <summary>The names of the patterns laid so far, in order.</summary>
        public List<string> Laid { get; } = new List<string>();

        /// <summary>Where each laid pattern begins and ends, in order.</summary>
        public List<(float Start, float End)> Spans { get; } = new List<(float, float)>();

        public void Fill(float upTo, List<Obstacle> obstacles, List<Pickup> pickups)
        {
            while (nextZ < upTo)
            {
                int tier = Difficulty.TierAt(nextZ);
                Pattern pattern = Pick(tier);
                bool mirror = random.Next(2) == 1;
                float stretch = Difficulty.Stretch(nextZ);
                float z = nextZ;
                for (int row = 0; row < pattern.Rows.Length; row++)
                {
                    z = nextZ + row * pattern.Spacing * stretch;
                    for (int lane = 0; lane < Tuning.LaneCount; lane++)
                    {
                        char cell = pattern.Rows[row][mirror ? Tuning.LaneCount - 1 - lane : lane];
                        Place(cell, lane, z, obstacles, pickups);
                    }
                }
                Laid.Add(pattern.Name);
                float end = z + Difficulty.PatternGap(tier) * stretch;
                Spans.Add((nextZ, end));
                nextZ = end;
            }
        }

        Pattern Pick(int tier)
        {
            // Patterns of the current tier are three times as likely as older ones.
            int total = 0;
            foreach (Pattern pattern in Patterns.All)
            {
                total += Weight(pattern, tier);
            }
            int roll = random.Next(total);
            foreach (Pattern pattern in Patterns.All)
            {
                roll -= Weight(pattern, tier);
                if (roll < 0)
                {
                    return pattern;
                }
            }
            return Patterns.All[0];
        }

        static int Weight(Pattern pattern, int tier) => pattern.MinTier > tier ? 0 : pattern.MinTier == tier ? 3 : 1;

        void Place(char cell, int lane, float z, List<Obstacle> obstacles, List<Pickup> pickups)
        {
            switch (cell)
            {
                case 'H':
                case 'h':
                    obstacles.Add(new Obstacle(nextId++, ObstacleKind.Hurdle, lane, z));
                    if (cell == 'h')
                    {
                        pickups.Add(new Pickup(nextId++, PickupKind.Coin, lane, z - 2f, 1.7f));
                        pickups.Add(new Pickup(nextId++, PickupKind.Coin, lane, z, 2.3f));
                        pickups.Add(new Pickup(nextId++, PickupKind.Coin, lane, z + 2f, 1.7f));
                    }
                    break;
                case 'B':
                case 'b':
                    obstacles.Add(new Obstacle(nextId++, ObstacleKind.Beam, lane, z));
                    if (cell == 'b')
                    {
                        pickups.Add(new Pickup(nextId++, PickupKind.Coin, lane, z - 1.5f, 0.45f));
                        pickups.Add(new Pickup(nextId++, PickupKind.Coin, lane, z, 0.45f));
                        pickups.Add(new Pickup(nextId++, PickupKind.Coin, lane, z + 1.5f, 0.45f));
                    }
                    break;
                case 'W':
                    obstacles.Add(new Obstacle(nextId++, ObstacleKind.Wall, lane, z));
                    break;
                case 'A':
                    obstacles.Add(new Obstacle(nextId++, ObstacleKind.Axe, lane, z, (float)(random.NextDouble() * Math.PI * 2)));
                    break;
                case 'c':
                    pickups.Add(new Pickup(nextId++, PickupKind.Coin, lane, z, 0.8f));
                    break;
                case 'g':
                    pickups.Add(new Pickup(nextId++, PickupKind.Gem, lane, z, 0.9f));
                    break;
                case '*':
                    pickups.Add(new Pickup(nextId++, PowerUps[random.Next(PowerUps.Length)], lane, z, 1f));
                    break;
            }
        }
    }

    /// <summary>A fixed piece of road written like a pattern, then nothing: for tests and staged scenes.</summary>
    public sealed class ScriptedTrack : ITrackSource
    {
        readonly List<Obstacle> obstacles = new List<Obstacle>();
        readonly List<Pickup> pickups = new List<Pickup>();
        int handedObstacles;
        int handedPickups;

        /// <param name="start">Where the first row is.</param>
        /// <param name="spacing">Metres between rows.</param>
        /// <param name="rows">Rows near to far as in <see cref="Pattern"/>.</param>
        /// <param name="powerUp">What a <c>*</c> cell becomes.</param>
        /// <param name="axePhase">Where every axe starts its swing, in radians.</param>
        public ScriptedTrack(float start, float spacing, string rows, PickupKind powerUp = PickupKind.Shield, float axePhase = 0f)
        {
            int id = 1;
            string[] lines = rows.Split('|');
            for (int row = 0; row < lines.Length; row++)
            {
                float z = start + row * spacing;
                for (int lane = 0; lane < lines[row].Length; lane++)
                {
                    switch (lines[row][lane])
                    {
                        case 'H': obstacles.Add(new Obstacle(id++, ObstacleKind.Hurdle, lane, z)); break;
                        case 'B': obstacles.Add(new Obstacle(id++, ObstacleKind.Beam, lane, z)); break;
                        case 'W': obstacles.Add(new Obstacle(id++, ObstacleKind.Wall, lane, z)); break;
                        case 'A': obstacles.Add(new Obstacle(id++, ObstacleKind.Axe, lane, z, axePhase)); break;
                        case 'c': pickups.Add(new Pickup(id++, PickupKind.Coin, lane, z, 0.8f)); break;
                        case 'g': pickups.Add(new Pickup(id++, PickupKind.Gem, lane, z, 0.9f)); break;
                        case '*': pickups.Add(new Pickup(id++, powerUp, lane, z, 1f)); break;
                    }
                }
            }
        }

        public void Fill(float upTo, List<Obstacle> obstacleList, List<Pickup> pickupList)
        {
            while (handedObstacles < obstacles.Count && obstacles[handedObstacles].Z <= upTo)
            {
                obstacleList.Add(obstacles[handedObstacles++]);
            }
            while (handedPickups < pickups.Count && pickups[handedPickups].Z <= upTo)
            {
                pickupList.Add(pickups[handedPickups++]);
            }
        }
    }
}
