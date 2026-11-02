using GauntletRun.Core;
using UnityEngine;

namespace GauntletRun.Game
{
    /// <summary>Places and animates the knight from the core runner: run cycle, jump tuck, slide, crash.</summary>
    public sealed class KnightView
    {
        readonly KnightRig rig;
        readonly Transform bubble;
        float cycle;
        float fallen;
        float lean;
        int skin = -1;

        public KnightView(Transform parent, Material bubbleMaterial)
        {
            rig = Models.Knight(parent);
            bubble = Shapes.Part("Shield bubble", rig.Root, PrimitiveType.Sphere, bubbleMaterial, new Vector3(0f, 0.95f, 0f),
                Vector3.zero, new Vector3(1.6f, 2.2f, 1.6f), shadows: false);
            bubble.gameObject.SetActive(false);
        }

        public Transform Root => rig.Root;
        public bool ShieldShown => bubble.gameObject.activeSelf;
        public int Skin => skin;

        public void Wear(int index)
        {
            if (index == skin)
            {
                return;
            }
            skin = index;
            Models.Paint(rig, Skins.All[index]);
        }

        /// <summary>Follows the runner.</summary>
        public void Sync(Run run, float dt, float time)
        {
            Runner r = run.Runner;
            rig.Root.localPosition = new Vector3(r.X, r.Y, r.Z);
            float sideways = Tuning.LaneX(r.Lane) - r.X;
            lean = Mathf.MoveTowards(lean, Mathf.Clamp(sideways * 12f, -16f, 16f), dt * 160f);
            rig.Root.localEulerAngles = new Vector3(0f, -lean * 0.6f, lean);

            bool shielded = run.Shielded && run.Alive;
            bubble.gameObject.SetActive(shielded);
            if (shielded)
            {
                float pulse = 1f + Mathf.Sin(time * 8f) * 0.04f;
                bool blinking = run.ShieldLeft < 2.5f && Mathf.Repeat(time * 6f, 1f) < 0.4f;
                bubble.localScale = new Vector3(1.6f, 2.2f, 1.6f) * (blinking ? 0.94f : pulse);
            }

            if (!run.Alive)
            {
                // Knocked flat on the back, arms flung out.
                fallen = Mathf.MoveTowards(fallen, 1f, dt * 4f);
                rig.Body.localEulerAngles = new Vector3(-82f * Ease(fallen), 0f, 0f);
                rig.Body.localPosition = new Vector3(0f, 0.25f * Ease(fallen), -0.6f * Ease(fallen));
                Pose(-20f, 20f, -150f, -150f, 70f);
                return;
            }
            fallen = 0f;
            rig.Body.localPosition = Vector3.zero;

            if (r.Sliding)
            {
                // Feet first, leaning back.
                rig.Body.localEulerAngles = new Vector3(-62f, 0f, 0f);
                rig.Body.localPosition = new Vector3(0f, 0.05f, 0.35f);
                Pose(-80f, -70f, -40f, -50f, 80f);
            }
            else if (!r.Grounded)
            {
                // Tucked legs, arms up.
                rig.Body.localEulerAngles = new Vector3(8f, 0f, 0f);
                float rise = Mathf.Clamp(r.VerticalSpeed / Tuning.JumpSpeed, -1f, 1f);
                Pose(-55f, -20f, -110f + rise * 20f, -70f, 45f);
            }
            else
            {
                cycle += dt * run.Speed * 0.75f;
                float swing = Mathf.Sin(cycle);
                rig.Body.localEulerAngles = new Vector3(10f + run.Speed * 0.3f, 0f, 0f);
                rig.Body.localPosition = new Vector3(0f, Mathf.Abs(Mathf.Cos(cycle)) * 0.07f, 0f);
                Pose(swing * 45f, -swing * 45f, -swing * 40f - 10f, swing * 40f - 30f, 20f + Mathf.Abs(swing) * 15f);
            }
        }

        /// <summary>Standing still, facing the camera in the shop.</summary>
        public void Idle(Vector3 position, float time)
        {
            rig.Root.localPosition = position;
            rig.Root.localEulerAngles = new Vector3(0f, -15f + Mathf.Sin(time * 0.6f) * 20f, 0f);
            rig.Body.localEulerAngles = Vector3.zero;
            rig.Body.localPosition = new Vector3(0f, Mathf.Sin(time * 2f) * 0.015f, 0f);
            bubble.gameObject.SetActive(false);
            Pose(0f, 0f, -8f, -25f, 8f + Mathf.Sin(time * 1.7f) * 4f);
        }

        void Pose(float leftLeg, float rightLeg, float leftArm, float rightArm, float cape)
        {
            rig.LeftLeg.localEulerAngles = new Vector3(leftLeg, 0f, 0f);
            rig.RightLeg.localEulerAngles = new Vector3(rightLeg, 0f, 0f);
            rig.LeftArm.localEulerAngles = new Vector3(leftArm, 0f, -8f);
            rig.RightArm.localEulerAngles = new Vector3(rightArm, 0f, 8f);
            rig.Cape.localEulerAngles = new Vector3(cape, 0f, 0f);
        }

        static float Ease(float t) => t * t * (3f - 2f * t);
    }
}
