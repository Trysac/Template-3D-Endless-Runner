using System.Collections.Generic;

namespace GauntletRun.Core
{
    public enum ObstacleKind
    {
        /// <summary>A low barricade: jump over it.</summary>
        Hurdle,
        /// <summary>A spiked beam at head height: slide under it.</summary>
        Beam,
        /// <summary>A stack of crates as tall as the corridor: change lanes.</summary>
        Wall,
        /// <summary>A pendulum axe swinging across its lane: dodge it or time it.</summary>
        Axe,
    }

    public enum PickupKind
    {
        Coin,
        Gem,
        Shield,
        Magnet,
        Double,
    }

    public sealed class Obstacle
    {
        public Obstacle(int id, ObstacleKind kind, int lane, float z, float phase = 0f)
        {
            Id = id;
            Kind = kind;
            Lane = lane;
            Z = z;
            Phase = phase;
        }

        public int Id { get; }
        public ObstacleKind Kind { get; }
        public int Lane { get; }
        public float Z { get; }
        /// <summary>Where in its swing an axe starts, in radians.</summary>
        public float Phase { get; }
        /// <summary>True once a shielded runner has smashed through it.</summary>
        public bool Smashed { get; internal set; }

        /// <summary>True when no jump or slide gets past it in its lane.</summary>
        public bool BlocksLane => Kind == ObstacleKind.Wall || Kind == ObstacleKind.Axe;

        public float Depth => Kind == ObstacleKind.Wall ? 1.2f : Kind == ObstacleKind.Axe ? 0.6f : 0.5f;
    }

    public sealed class Pickup
    {
        public Pickup(int id, PickupKind kind, int lane, float z, float y)
        {
            Id = id;
            Kind = kind;
            X = Tuning.LaneX(lane);
            Y = y;
            Z = z;
        }

        public int Id { get; }
        public PickupKind Kind { get; }
        public float X { get; internal set; }
        public float Y { get; internal set; }
        public float Z { get; internal set; }
        /// <summary>True while the magnet draws it towards the runner.</summary>
        public bool Pulled { get; internal set; }
        public bool Taken { get; internal set; }
    }

    /// <summary>Where the obstacles and pickups of a run come from.</summary>
    public interface ITrackSource
    {
        /// <summary>Appends everything up to <paramref name="upTo"/> metres that has not been handed out yet.</summary>
        void Fill(float upTo, List<Obstacle> obstacles, List<Pickup> pickups);
    }
}
