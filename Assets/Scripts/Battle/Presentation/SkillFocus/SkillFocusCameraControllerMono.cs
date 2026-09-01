using UnityEngine;

namespace Battle.Presentation.SkillFocus
{
    [DefaultExecutionOrder(10000)]
    [DisallowMultipleComponent]
    public sealed class SkillFocusCameraControllerMono : MonoBehaviour
    {
        private Camera targetCamera;
        private bool active;
        private Vector3 appliedOffset;
        private SkillFocusCalibration calibration;
        private Vector2 fixedAxis;
        private float elapsed;
        private float startedAt;
        private bool preserveInitialPeakUntilRender;

        public bool IsActive => active;

        public bool TryPlay(Camera camera, SkillFocusCalibration value, Vector2 axis)
        {
            RestoreImmediate();
            targetCamera = camera;
            active = targetCamera != null && targetCamera.orthographic;
            if (!active) return false;
            calibration = value;
            fixedAxis = axis.sqrMagnitude > .0001f ? axis.normalized : Vector2.right;
            elapsed = 0f;
            startedAt = Time.unscaledTime;
            preserveInitialPeakUntilRender = true;
            appliedOffset = Vector3.zero;
            ApplyPixelOffset(ResolvePeakPixels(calibration.AmplitudePixels, ResolveRenderHeight(targetCamera)));
            return true;
        }

        public void RestoreImmediate()
        {
            if (targetCamera != null) targetCamera.transform.position -= appliedOffset;
            targetCamera = null;
            appliedOffset = Vector3.zero;
            active = false;
            elapsed = 0f;
            startedAt = 0f;
            preserveInitialPeakUntilRender = false;
        }

        private void LateUpdate()
        {
            if (!active || targetCamera == null) return;
            if (preserveInitialPeakUntilRender)
            {
                preserveInitialPeakUntilRender = false;
                return;
            }
            targetCamera.transform.position -= appliedOffset;
            appliedOffset = Vector3.zero;
            elapsed = Mathf.Max(0f, Time.unscaledTime - startedAt);
            if (elapsed >= calibration.Duration)
            {
                RestoreImmediate();
                return;
            }

            float displacement = EvaluateNormalizedWaveform(elapsed / calibration.Duration, calibration)
                * ResolvePeakPixels(calibration.AmplitudePixels, ResolveRenderHeight(targetCamera));
            ApplyPixelOffset(displacement);
        }

        private void ApplyPixelOffset(float pixels)
        {
            float renderHeight = ResolveRenderHeight(targetCamera);
            float worldPerPixel = (2f * targetCamera.orthographicSize) / renderHeight;
            float worldDisplacement = pixels * worldPerPixel;
            appliedOffset = (targetCamera.transform.right * (fixedAxis.x * worldDisplacement))
                + (targetCamera.transform.up * (fixedAxis.y * worldDisplacement));
            targetCamera.transform.position += appliedOffset;
        }

        public static float ResolvePeakPixels(float basePixelsAt1080, float renderHeight)
        {
            float heightScale = Mathf.Clamp(renderHeight / 1080f, .75f, 1.2f);
            return Mathf.Min(Mathf.Max(0f, basePixelsAt1080) * heightScale, 6f);
        }

        public static float ResolveRenderHeight(Camera camera)
        {
            if (camera == null) return 1f;
            if (camera.targetTexture != null) return Mathf.Max(1f, camera.targetTexture.height);
            if (camera.scaledPixelHeight > 0) return camera.scaledPixelHeight;
            return Mathf.Max(1f, camera.pixelRect.height);
        }

        public static float EvaluateNormalizedWaveform(float normalizedTime, SkillFocusCalibration value)
        {
            float u = Mathf.Clamp01(normalizedTime);
            if (u >= 1f || value.Duration <= 0f || value.Cycles <= 0f) return 0f;
            float decay = Mathf.Pow(1f - u, 1.35f);
            float carrier = Mathf.Sin(Mathf.PI * 2f * value.Cycles * u);
            return Mathf.Clamp(decay * carrier * value.Normalization, -1f, 1f);
        }

        private void OnDisable() => RestoreImmediate();
        private void OnDestroy() => RestoreImmediate();
    }
}
