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
        private static Rect? actorZone;

        internal static void SetActorZone(Rect zone) => actorZone = zone;

        public static bool IsActive { get; private set; }
        public static Rect Arena => arena;

        public static void Activate(Vector2 size, float inset)
        {
            Activate(new Rect(-size * .5f, size), inset);
        }

        public static void Activate(Rect bounds, float inset)
        {
            arena = bounds;
            actorZone = null;
            movementInset = Mathf.Max(0f, inset);
            IsActive = bounds.width > 0f && bounds.height > 0f;
        }

        public static void Clear()
        {
            IsActive = false;
            actorZone = null;
            arena = default;
            movementInset = 0f;
        }

        public static Vector2 ClampActorCenter(Vector2 point, float extraInset = 0f)
        {
            if (!IsActive) return point;
            if(Morpg.MorpgEnvironmentRuntime.Active!=null)return Morpg.MorpgEnvironmentRuntime.Active.ClampActorCenter(point,extraInset);
            float inset = (actorZone.HasValue ? 0f : movementInset) + Mathf.Max(0f, extraInset);
            Rect bounds = actorZone ?? arena;
            return new Vector2(
                Mathf.Clamp(point.x, bounds.xMin + inset, bounds.xMax - inset),
                Mathf.Clamp(point.y, bounds.yMin + inset, bounds.yMax - inset));
        }

        public static Vector2 SweepActorStep(Vector2 from, Vector2 to, Rigidbody2D body = null)
        {
            var environment = Morpg.MorpgEnvironmentRuntime.Active;
            return environment != null
                ? environment.Sweep(from, to, Morpg.MorpgEnvironmentRuntime.ActorRadius(body))
                : ClampActorCenter(to);
        }

        public static Vector2 ClampCameraCenter(Vector2 point, Camera camera)
        {
            if (!IsActive || camera == null || !camera.orthographic) return point;
            if (Morpg.MorpgEnvironmentRuntime.Active != null)
                return Morpg.MorpgEnvironmentRuntime.Active.ClampCamera(point);
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
