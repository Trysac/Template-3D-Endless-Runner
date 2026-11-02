using System;

namespace GauntletRun.Core
{
    /// <summary>
    /// A simple bot: it keeps to the lane with the most open road ahead, jumps hurdles and
    /// slides under beams. It plays the title screen's attract run and, in the tests, proves the
    /// generated road can always be survived.
    /// </summary>
    public static class Autopilot
    {
        public static RunInput Decide(Run run)
        {
            Runner r = run.Runner;
            float speed = run.Speed;
            var input = new RunInput();

            float[] clear = new float[Tuning.LaneCount];
            for (int lane = 0; lane < Tuning.LaneCount; lane++)
            {
                clear[lane] = ClearAhead(run, lane);
            }
            float needed = speed * 0.9f + 3f;
            if (clear[r.Lane] < needed)
            {
                bool urgent = clear[r.Lane] < speed * 0.3f;
                int best = r.Lane;
                for (int lane = 0; lane < Tuning.LaneCount; lane++)
                {
                    if (lane == r.Lane || clear[lane] <= clear[best])
                    {
                        continue;
                    }
                    // Do not step in front of a hurdle or beam too close to jump or slide.
                    float action = ActionGap(run, lane);
                    if (!urgent && action > -1.5f && action < speed * 0.3f)
                    {
                        continue;
                    }
                    bool crossesMiddle = Math.Abs(lane - r.Lane) == 2;
                    if (crossesMiddle && clear[1] < speed * 0.35f + 1.5f)
                    {
                        continue;
                    }
                    best = lane;
                }
                input.Steer = Math.Sign(best - r.Lane);
            }

            // Jump or slide for whatever is coming in the lanes the knight overlaps.
            float nearestHurdle = float.MaxValue, nearestBeam = float.MaxValue;
            foreach (Obstacle o in run.Obstacles)
            {
                if (o.Smashed || o.BlocksLane || !Overlaps(r, o.Lane))
                {
                    continue;
                }
                float gap = o.Z - o.Depth / 2f - (r.Z + Tuning.RunnerHalfDepth);
                if (gap < -o.Depth - Tuning.RunnerHalfDepth * 2f)
                {
                    continue;
                }
                if (o.Kind == ObstacleKind.Hurdle)
                {
                    nearestHurdle = Math.Min(nearestHurdle, gap);
                }
                else
                {
                    nearestBeam = Math.Min(nearestBeam, gap);
                }
            }
            if (nearestHurdle <= speed * 0.2f && nearestHurdle > -0.5f)
            {
                input.Jump = r.Grounded;
            }
            else if (nearestBeam <= speed * 0.12f && !r.Sliding)
            {
                input.Slide = true;
            }
            return input;
        }

        /// <summary>The gap to the nearest hurdle or beam ahead in a lane (negative while passing it).</summary>
        static float ActionGap(Run run, int lane)
        {
            Runner r = run.Runner;
            float gap = float.MaxValue;
            foreach (Obstacle o in run.Obstacles)
            {
                if (o.Lane != lane || o.BlocksLane || o.Smashed)
                {
                    continue;
                }
                float near = o.Z - o.Depth / 2f - (r.Z + Tuning.RunnerHalfDepth);
                if (near > -o.Depth - Tuning.RunnerHalfDepth * 2f)
                {
                    gap = Math.Min(gap, near);
                }
            }
            return gap;
        }

        /// <summary>The open road ahead in a lane before something that cannot be jumped or slid.</summary>
        public static float ClearAhead(Run run, int lane)
        {
            Runner r = run.Runner;
            float clear = float.MaxValue;
            foreach (Obstacle o in run.Obstacles)
            {
                if (o.Lane != lane || !o.BlocksLane || o.Smashed)
                {
                    continue;
                }
                float reach = o.Depth / 2f;
                if (o.Z + reach < r.Z - Tuning.RunnerHalfDepth)
                {
                    continue;
                }
                clear = Math.Min(clear, o.Z - reach - (r.Z + Tuning.RunnerHalfDepth));
            }
            return clear;
        }

        static bool Overlaps(Runner r, int lane)
        {
            float laneX = Tuning.LaneX(lane);
            float half = Tuning.LaneWidth / 2f;
            return r.Lane == lane || (r.X + Tuning.RunnerHalfWidth > laneX - half && r.X - Tuning.RunnerHalfWidth < laneX + half);
        }
    }
}
