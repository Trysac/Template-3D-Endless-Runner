namespace GauntletRun.Core
{
    /// <summary>Free sideways movement on the road, kept between -limit and +limit.</summary>
    public static class LateralMovement
    {
        /// <summary>The new x after steering with <paramref name="input"/> (-1..1) for <paramref name="dt"/> seconds.</summary>
        public static float Step(float x, float input, float dt, float speed, float limit)
        {
            float next = x + input * dt * speed;
            return next > limit ? limit : next < -limit ? -limit : next;
        }
    }
}
