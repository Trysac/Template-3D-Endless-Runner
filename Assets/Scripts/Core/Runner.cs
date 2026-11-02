using System;

namespace GauntletRun.Core
{
    /// <summary>One frame of player intent. Presses are one-shot; the runner buffers them briefly.</summary>
    public struct RunInput
    {
        /// <summary>-1 to move a lane left, +1 to move a lane right, 0 to stay.</summary>
        public int Steer;
        public bool Jump;
        public bool Slide;

        public static RunInput None => default;
        public static RunInput Left => new RunInput { Steer = -1 };
        public static RunInput Right => new RunInput { Steer = 1 };
        public static RunInput JumpPress => new RunInput { Jump = true };
        public static RunInput SlidePress => new RunInput { Slide = true };
    }

    /// <summary>The knight's body: lane, sideways position, jump arc and slide.</summary>
    public sealed class Runner
    {
        float jumpBuffer;
        float slideBuffer;

        /// <summary>The lane the runner is in or heading to (0 = left, 2 = right).</summary>
        public int Lane { get; private set; } = 1;
        public float X { get; private set; }
        /// <summary>Height of the feet above the floor.</summary>
        public float Y { get; private set; }
        public float Z { get; internal set; }
        public float VerticalSpeed { get; private set; }
        public bool Grounded { get; private set; } = true;
        public float SlideLeft { get; private set; }
        public bool Sliding => SlideLeft > 0f;
        public float Height => Sliding ? Tuning.SlideHeight : Tuning.StandHeight;

        /// <summary>Applies the input and gravity for one step; returns what happened.</summary>
        internal RunnerChange Step(RunInput input, float dt)
        {
            RunnerChange change = RunnerChange.None;
            if (input.Steer != 0)
            {
                int lane = Math.Max(0, Math.Min(Tuning.LaneCount - 1, Lane + Math.Sign(input.Steer)));
                if (lane != Lane)
                {
                    Lane = lane;
                    change |= RunnerChange.LaneChange;
                }
            }
            if (input.Jump)
            {
                jumpBuffer = Tuning.InputBuffer;
            }
            if (input.Slide)
            {
                slideBuffer = Tuning.InputBuffer;
            }

            if (jumpBuffer > 0f && Grounded)
            {
                VerticalSpeed = Tuning.JumpSpeed;
                Grounded = false;
                SlideLeft = 0f;
                jumpBuffer = 0f;
                slideBuffer = 0f;
                change |= RunnerChange.Jump;
            }
            if (slideBuffer > 0f)
            {
                if (Grounded)
                {
                    SlideLeft = Tuning.SlideTime;
                    slideBuffer = 0f;
                    change |= RunnerChange.Slide;
                }
                else if (VerticalSpeed > -Tuning.DropSpeed)
                {
                    // Sliding in the air drops the knight to the floor; the slide starts on landing.
                    VerticalSpeed = -Tuning.DropSpeed;
                }
            }
            jumpBuffer = Math.Max(0f, jumpBuffer - dt);
            slideBuffer = Math.Max(0f, slideBuffer - dt);

            if (!Grounded)
            {
                VerticalSpeed -= Tuning.Gravity * dt;
                Y += VerticalSpeed * dt;
                if (Y <= 0f)
                {
                    Y = 0f;
                    VerticalSpeed = 0f;
                    Grounded = true;
                    change |= RunnerChange.Land;
                    if (slideBuffer > 0f)
                    {
                        SlideLeft = Tuning.SlideTime;
                        slideBuffer = 0f;
                        change |= RunnerChange.Slide;
                    }
                }
            }
            SlideLeft = Math.Max(0f, SlideLeft - dt);

            float target = Tuning.LaneX(Lane);
            float step = Tuning.LaneSwitchSpeed * dt;
            X = Math.Abs(target - X) <= step ? target : X + Math.Sign(target - X) * step;
            return change;
        }
    }

    [Flags]
    internal enum RunnerChange
    {
        None = 0,
        LaneChange = 1,
        Jump = 2,
        Land = 4,
        Slide = 8,
    }
}
