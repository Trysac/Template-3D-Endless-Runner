using System;
using System.Collections.Generic;

namespace GauntletRun.Core
{
    public enum RunEventKind
    {
        LaneChange,
        Jump,
        Land,
        Slide,
        Coin,
        Gem,
        PowerUp,
        ShieldBreak,
        Milestone,
        Death,
    }

    /// <summary>Something the view should show or play; read and cleared every frame.</summary>
    public struct RunEvent
    {
        public RunEventKind Kind;
        public float X;
        public float Y;
        public float Z;
        public PickupKind Pickup;
        public ObstacleKind Obstacle;
        /// <summary>The milestone distance, or the id of the obstacle or pickup involved.</summary>
        public int Value;
    }

    /// <summary>
    /// One run down the gauntlet: the knight, the road around it, power-ups and the score. Plain
    /// maths in fixed steps; nothing here knows about Unity.
    /// </summary>
    public sealed class Run
    {
        /// <summary>The axe hangs from a beam and swings across its own lane.</summary>
        public const float AxeArm = 3f;
        public const float AxePivot = 3.8f;
        public const float AxeSwing = 0.5f;
        public const float AxePeriod = 1.6f;

        readonly ITrackSource track;
        readonly float? fixedSpeed;
        readonly List<Obstacle> obstacles = new List<Obstacle>();
        readonly List<Pickup> pickups = new List<Pickup>();
        double points;
        float nextMilestone = Tuning.MilestoneEvery;

        /// <param name="fixedSpeed">Run at this speed instead of following the difficulty curve.</param>
        public Run(ITrackSource track, float? fixedSpeed = null)
        {
            this.track = track;
            this.fixedSpeed = fixedSpeed;
            Speed = fixedSpeed ?? Difficulty.SpeedAt(0f);
            track.Fill(Tuning.LookAhead, obstacles, pickups);
        }

        public Runner Runner { get; } = new Runner();
        public float Distance => Runner.Z;
        public float Speed { get; private set; }
        public float Time { get; private set; }
        public bool Alive { get; private set; } = true;
        public ObstacleKind? KilledBy { get; private set; }

        public int Score => (int)points;
        public int Coins { get; private set; }
        public int Gems { get; private set; }
        /// <summary>What the run adds to the wallet: every coin, and each gem as several.</summary>
        public int Earned => Coins + Gems * Tuning.GemCoins;
        public int Jumps { get; private set; }

        public float ShieldLeft { get; private set; }
        public float MagnetLeft { get; private set; }
        public float DoubleLeft { get; private set; }
        public float GraceLeft { get; private set; }
        public bool Shielded => ShieldLeft > 0f;
        public int Multiplier => DoubleLeft > 0f ? 2 : 1;

        public IReadOnlyList<Obstacle> Obstacles => obstacles;
        public IReadOnlyList<Pickup> Pickups => pickups;
        public List<RunEvent> Events { get; } = new List<RunEvent>();

        /// <summary>Advances the run by one step. A finished run stands still.</summary>
        public void Step(RunInput input, float dt)
        {
            if (!Alive)
            {
                return;
            }
            Time += dt;
            Speed = fixedSpeed ?? Difficulty.SpeedAt(Distance);
            RunnerChange change = Runner.Step(input, dt);
            float travelled = Speed * dt;
            Runner.Z += travelled;
            points += travelled * Multiplier;
            Report(change);

            while (Distance >= nextMilestone)
            {
                Add(RunEventKind.Milestone, value: (int)nextMilestone);
                nextMilestone += Tuning.MilestoneEvery;
            }

            ShieldLeft = Math.Max(0f, ShieldLeft - dt);
            MagnetLeft = Math.Max(0f, MagnetLeft - dt);
            DoubleLeft = Math.Max(0f, DoubleLeft - dt);
            GraceLeft = Math.Max(0f, GraceLeft - dt);

            track.Fill(Distance + Tuning.LookAhead, obstacles, pickups);
            Collide();
            Collect(dt);
            obstacles.RemoveAll(o => o.Z + 3f < Distance - Tuning.KeepBehind);
            pickups.RemoveAll(p => p.Taken || p.Z < Distance - Tuning.KeepBehind);
        }

        /// <summary>Gives a power-up as if it had been picked up (used by staged scenes).</summary>
        public void Grant(PickupKind kind)
        {
            switch (kind)
            {
                case PickupKind.Shield: ShieldLeft = Tuning.ShieldTime; break;
                case PickupKind.Magnet: MagnetLeft = Tuning.MagnetTime; break;
                case PickupKind.Double: DoubleLeft = Tuning.DoubleTime; break;
            }
        }

        /// <summary>The swing angle of an axe right now, in radians (0 = hanging straight down).</summary>
        public float AxeAngle(Obstacle axe) =>
            AxeSwing * (float)Math.Sin(Time * 2.0 * Math.PI / AxePeriod + axe.Phase);

        /// <summary>The blade's centre across the road and its height.</summary>
        public void AxeBlade(Obstacle axe, out float x, out float y)
        {
            float angle = AxeAngle(axe);
            x = Tuning.LaneX(axe.Lane) + AxeArm * (float)Math.Sin(angle);
            y = AxePivot - AxeArm * (float)Math.Cos(angle);
        }

        void Report(RunnerChange change)
        {
            if ((change & RunnerChange.LaneChange) != 0) Add(RunEventKind.LaneChange);
            if ((change & RunnerChange.Jump) != 0)
            {
                Jumps++;
                Add(RunEventKind.Jump);
            }
            if ((change & RunnerChange.Land) != 0) Add(RunEventKind.Land);
            if ((change & RunnerChange.Slide) != 0) Add(RunEventKind.Slide);
        }

        void Collide()
        {
            Runner r = Runner;
            float left = r.X - Tuning.RunnerHalfWidth;
            float right = r.X + Tuning.RunnerHalfWidth;
            float back = r.Z - Tuning.RunnerHalfDepth;
            float front = r.Z + Tuning.RunnerHalfDepth;
            float bottom = r.Y;
            float top = r.Y + r.Height;
            foreach (Obstacle o in obstacles)
            {
                if (o.Smashed)
                {
                    continue;
                }
                float zMin = o.Z - o.Depth / 2f;
                float zMax = o.Z + o.Depth / 2f;
                if (front <= zMin || back >= zMax)
                {
                    continue;
                }
                float xMin, xMax, yMin, yMax;
                if (o.Kind == ObstacleKind.Axe)
                {
                    AxeBlade(o, out float bladeX, out float bladeY);
                    xMin = bladeX - 0.35f;
                    xMax = bladeX + 0.35f;
                    yMin = bladeY - 0.5f;
                    yMax = bladeY + 0.5f;
                }
                else
                {
                    float laneX = Tuning.LaneX(o.Lane);
                    xMin = laneX - (Tuning.LaneWidth / 2f - 0.1f);
                    xMax = laneX + (Tuning.LaneWidth / 2f - 0.1f);
                    yMin = o.Kind == ObstacleKind.Beam ? 1f : 0f;
                    yMax = o.Kind == ObstacleKind.Hurdle ? 0.9f : 3f;
                }
                if (right <= xMin || left >= xMax || top <= yMin || bottom >= yMax)
                {
                    continue;
                }
                Hit(o);
                if (!Alive)
                {
                    return;
                }
            }
        }

        void Hit(Obstacle o)
        {
            if (GraceLeft > 0f)
            {
                return;
            }
            if (Shielded)
            {
                ShieldLeft = 0f;
                GraceLeft = Tuning.GraceTime;
                o.Smashed = true;
                Add(RunEventKind.ShieldBreak, obstacle: o.Kind, value: o.Id, z: o.Z, x: Tuning.LaneX(o.Lane));
                return;
            }
            Alive = false;
            KilledBy = o.Kind;
            Add(RunEventKind.Death, obstacle: o.Kind, value: o.Id);
        }

        void Collect(float dt)
        {
            Runner r = Runner;
            float centreY = r.Y + r.Height * 0.5f;
            foreach (Pickup p in pickups)
            {
                if (p.Taken)
                {
                    continue;
                }
                bool treasure = p.Kind == PickupKind.Coin || p.Kind == PickupKind.Gem;
                if (treasure && MagnetLeft > 0f && !p.Pulled && p.Z > r.Z - 1f && p.Z < r.Z + Tuning.MagnetReach)
                {
                    p.Pulled = true;
                }
                bool touching;
                if (p.Pulled)
                {
                    float dx = r.X - p.X, dy = centreY - p.Y, dz = r.Z - p.Z;
                    float distance = (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
                    float move = (Tuning.MagnetPull + Speed) * dt;
                    if (distance <= move || distance < 0.6f)
                    {
                        p.X = r.X;
                        p.Y = centreY;
                        p.Z = r.Z;
                        touching = true;
                    }
                    else
                    {
                        p.X += dx / distance * move;
                        p.Y += dy / distance * move;
                        p.Z += dz / distance * move;
                        touching = false;
                    }
                }
                else
                {
                    touching = Math.Abs(p.X - r.X) < Tuning.RunnerHalfWidth + 0.45f &&
                               Math.Abs(p.Z - r.Z) < Tuning.RunnerHalfDepth + 0.5f &&
                               p.Y > r.Y - 0.4f && p.Y < r.Y + r.Height + 0.25f;
                }
                if (touching)
                {
                    Take(p);
                }
            }
        }

        void Take(Pickup p)
        {
            p.Taken = true;
            switch (p.Kind)
            {
                case PickupKind.Coin:
                    Coins++;
                    points += Tuning.CoinPoints * Multiplier;
                    Add(RunEventKind.Coin, PickupKind.Coin, p);
                    break;
                case PickupKind.Gem:
                    Gems++;
                    points += Tuning.GemPoints * Multiplier;
                    Add(RunEventKind.Gem, PickupKind.Gem, p);
                    break;
                default:
                    Grant(p.Kind);
                    Add(RunEventKind.PowerUp, p.Kind, p);
                    break;
            }
        }

        void Add(RunEventKind kind, PickupKind pickup, Pickup p) =>
            Events.Add(new RunEvent { Kind = kind, Pickup = pickup, Value = p.Id, X = p.X, Y = p.Y, Z = p.Z });

        void Add(RunEventKind kind, ObstacleKind obstacle = default, int value = 0, float? x = null, float? z = null) =>
            Events.Add(new RunEvent
            {
                Kind = kind,
                Obstacle = obstacle,
                Value = value,
                X = x ?? Runner.X,
                Y = Runner.Y,
                Z = z ?? Runner.Z,
            });
    }
}
