using System;

namespace GauntletRun.Core
{
    /// <summary>The difficulty curve: speed rises towards a cap and new patterns unlock in tiers.</summary>
    public static class Difficulty
    {
        /// <summary>The distance at which each tier starts (tier 0 at the start line).</summary>
        public static readonly float[] TierStarts = { 0f, 300f, 800f, 1500f, 2500f };

        public static int MaxTier => TierStarts.Length - 1;

        /// <summary>Running speed at a distance: rises quickly at first, then eases towards the cap.</summary>
        public static float SpeedAt(float distance)
        {
            float d = Math.Max(0f, distance);
            return Tuning.StartSpeed + (Tuning.MaxSpeed - Tuning.StartSpeed) * d / (d + Tuning.SpeedHalfDistance);
        }

        public static int TierAt(float distance)
        {
            int tier = 0;
            for (int i = 1; i < TierStarts.Length; i++)
            {
                if (distance >= TierStarts[i])
                {
                    tier = i;
                }
            }
            return tier;
        }

        /// <summary>
        /// How much to stretch a pattern so the time between its rows stays about the same
        /// as the runner speeds up (never shorter than at the start).
        /// </summary>
        public static float Stretch(float distance) => Math.Max(1f, SpeedAt(distance) / Tuning.StartSpeed * 0.9f);

        /// <summary>Empty road between two patterns, shrinking as the tier rises.</summary>
        public static float PatternGap(int tier) => 16f - 2f * Math.Min(tier, MaxTier);
    }
}
