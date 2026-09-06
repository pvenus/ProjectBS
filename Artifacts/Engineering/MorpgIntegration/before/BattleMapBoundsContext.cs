using UnityEngine;

namespace Battle
{
    /// <summary>Battle-local, non-persistent bounds authority. Inactive means legacy behavior.</summary>
    public static class BattleMapBoundsContext
    {
        public const float LargeMapTargetSearchMultiplier = 2f;
        public const float CameraVisualPadding = .15f;
        private static Rect arena;
        private static float movementInset;

        public static bool IsActive { get; private set; }
        public static Rect Arena => arena;

        public static void Activate(Vector2 size, float inset)
        {
            arena = new Rect(-size * .5f, size);
            movementInset = Mathf.Max(0f, inset);
            IsActive = size.x > 0f && size.y > 0f;
        }

        public static void Clear()
        {
            IsActive = false;
            arena = default;
            movementInset = 0f;
        }

        public static Vector2 ClampActorCenter(Vector2 point, float extraInset = 0f)
        {
            if (!IsActive) return point;
            float inset = movementInset + Mathf.Max(0f, extraInset);
            return new Vector2(
                Mathf.Clamp(point.x, arena.xMin + inset, arena.xMax - inset),
                Mathf.Clamp(point.y, arena.yMin + inset, arena.yMax - inset));
        }

        public static Vector2 ClampCameraCenter(Vector2 point, Camera camera)
        {
            if (!IsActive || camera == null || !camera.orthographic) return point;
            float halfH = camera.orthographicSize + CameraVisualPadding;
            float halfW = camera.orthographicSize * Mathf.Max(.0001f, camera.aspect) + CameraVisualPadding;
            return new Vector2(
                ClampAxis(point.x, arena.xMin + halfW, arena.xMax - halfW),
                ClampAxis(point.y, arena.yMin + halfH, arena.yMax - halfH));
        }

        public static float ResolveTargetSearchRadius(float legacyRadius) =>
            Mathf.Max(0f, legacyRadius) * (IsActive ? LargeMapTargetSearchMultiplier : 1f);

        private static float ClampAxis(float value, float min, float max) =>
            min <= max ? Mathf.Clamp(value, min, max) : (min + max) * .5f;
    }
}
