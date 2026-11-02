using UnityEngine;

namespace GauntletRun.Game
{
    /// <summary>A chase camera behind the knight that widens with speed, shakes on impacts and can swing round for the shop.</summary>
    public sealed class CameraRig : MonoBehaviour
    {
        const float BaseFov = 58f;

        Camera view;
        Vector3 position;
        Vector3 look;
        float fov = BaseFov;
        float shake;
        bool placed;

        public Camera View => view != null ? view : view = GetComponent<Camera>();

        public void Shake(float amount) => shake = Mathf.Max(shake, amount);

        /// <summary>Jumps straight to the target instead of easing (after a new run starts).</summary>
        public void Snap() => placed = false;

        /// <summary>Follows a knight at <paramref name="knight"/> running at <paramref name="speed"/> m/s.</summary>
        public void Chase(Vector3 knight, float speed, float dt)
        {
            var targetPosition = new Vector3(knight.x * 0.55f, 3.5f + knight.y * 0.35f, knight.z - 6.6f);
            var targetLook = new Vector3(knight.x * 0.75f, 1.3f + knight.y * 0.25f, knight.z + 7f);
            Move(targetPosition, targetLook, BaseFov + Mathf.Clamp01((speed - 10f) / 14f) * 10f, dt, 9f);
        }

        /// <summary>A front view of the knight on the left of the screen (the shop).</summary>
        public void Showcase(Vector3 knight, float dt)
        {
            var targetPosition = knight + new Vector3(-0.9f, 1.55f, 4.4f);
            var targetLook = knight + new Vector3(-1.45f, 1.05f, 0f);
            Move(targetPosition, targetLook, 42f, dt, 5f);
        }

        void Move(Vector3 targetPosition, Vector3 targetLook, float targetFov, float dt, float sharpness)
        {
            float blend = placed ? 1f - Mathf.Exp(-sharpness * dt) : 1f;
            placed = true;
            position = Vector3.Lerp(position, targetPosition, blend);
            look = Vector3.Lerp(look, targetLook, blend);
            fov = Mathf.Lerp(fov, targetFov, blend * 0.5f + (blend >= 1f ? 0.5f : 0f));
            shake = Mathf.MoveTowards(shake, 0f, dt * 2.5f);
            Vector3 jitter = shake > 0f ? Random.insideUnitSphere * shake * 0.25f : Vector3.zero;
            transform.position = position + jitter;
            transform.rotation = Quaternion.LookRotation(look - position);
            View.fieldOfView = fov;
        }
    }
}
