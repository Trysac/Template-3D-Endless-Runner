namespace GauntletRun.Core
{
    /// <summary>Every number that shapes a run. Distances are metres, times seconds.</summary>
    public static class Tuning
    {
        public const int LaneCount = 3;
        public const float LaneWidth = 2.2f;
        /// <summary>Sideways speed while changing lanes.</summary>
        public const float LaneSwitchSpeed = 18f;

        public const float JumpSpeed = 10f;
        public const float Gravity = 32f;
        /// <summary>Downward speed when sliding in the air (a quick drop to the floor).</summary>
        public const float DropSpeed = 20f;
        public const float SlideTime = 0.65f;
        /// <summary>A jump or slide pressed this long before it is possible still happens.</summary>
        public const float InputBuffer = 0.15f;

        public const float RunnerHalfWidth = 0.35f;
        public const float RunnerHalfDepth = 0.3f;
        public const float StandHeight = 1.8f;
        public const float SlideHeight = 0.8f;

        public const float StartSpeed = 10f;
        public const float MaxSpeed = 24f;
        /// <summary>The distance at which the speed is halfway from the start to the top speed.</summary>
        public const float SpeedHalfDistance = 1600f;

        public const float ShieldTime = 15f;
        public const float MagnetTime = 10f;
        public const float DoubleTime = 10f;
        /// <summary>Grace after the shield breaks, so one crash cannot cost two hits.</summary>
        public const float GraceTime = 0.25f;
        public const float MagnetReach = 9f;
        public const float MagnetPull = 30f;

        public const int CoinPoints = 10;
        public const int GemPoints = 100;
        /// <summary>A gem is worth this many coins in the wallet.</summary>
        public const int GemCoins = 10;
        public const float MilestoneEvery = 500f;

        /// <summary>The empty road before the first obstacle.</summary>
        public const float ClearStart = 40f;
        /// <summary>How far ahead of the runner the road is generated.</summary>
        public const float LookAhead = 140f;
        /// <summary>Items this far behind the runner are dropped.</summary>
        public const float KeepBehind = 12f;

        public static float LaneX(int lane) => (lane - 1) * LaneWidth;
    }
}
