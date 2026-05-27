

using UnityEngine;

namespace ShaderDetail
{
    [System.Serializable]
    public class StunBlinkFeature
    {
        [Header("State")]
        public bool enabled = true;

        [Header("Color")]
        public Color stunRimColor = new Color(0.65f, 0.9f, 1f, 1f);

        [Header("Intensity")]
        [Min(0f)] public float stunRimIntensity = 1.25f;
        [Min(0f)] public float stunPulseIntensity = 0.55f;
        [Min(0f)] public float stunSparkleIntensity = 0.15f;

        [Header("Blink")]
        [Min(0.01f)] public float blinkSpeed = 7f;
        [Range(0f, 1f)] public float minBlend = 0.15f;
        [Range(0f, 1f)] public float maxBlend = 1f;
        public AnimationCurve blinkCurve;

        private bool isActive;
        private float timer;

        public bool IsActive => enabled && isActive;

        public void EnsureDefaults()
        {
            blinkSpeed = Mathf.Max(0.01f, blinkSpeed);
            minBlend = Mathf.Clamp01(minBlend);
            maxBlend = Mathf.Clamp01(maxBlend);

            if (maxBlend < minBlend)
            {
                maxBlend = minBlend;
            }

            if (blinkCurve == null || blinkCurve.length == 0)
            {
                blinkCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            }
        }

        public void SetActive(bool active)
        {
            if (!enabled)
            {
                StopImmediate();
                return;
            }

            if (isActive == active)
            {
                return;
            }

            isActive = active;

            if (isActive)
            {
                timer = 0f;
            }
        }

        public void StopImmediate()
        {
            isActive = false;
            timer = 0f;
        }

        public void Tick(float deltaTime)
        {
            if (!IsActive)
            {
                return;
            }

            timer += Mathf.Max(0f, deltaTime);
        }

        public float EvaluateBlink01()
        {
            if (!IsActive)
            {
                return 0f;
            }

            float wave = Mathf.Sin(timer * blinkSpeed) * 0.5f + 0.5f;
            float curveValue = blinkCurve != null
                ? blinkCurve.Evaluate(wave)
                : wave;

            return Mathf.Lerp(
                minBlend,
                maxBlend,
                Mathf.Clamp01(curveValue));
        }

        public Color EvaluateRimColor()
        {
            return stunRimColor;
        }

        public float EvaluateRimIntensity(float baseRimIntensity)
        {
            float blend = EvaluateBlink01();
            return Mathf.Lerp(
                baseRimIntensity,
                stunRimIntensity,
                blend);
        }

        public float EvaluatePulseIntensity(float basePulseIntensity)
        {
            float blend = EvaluateBlink01();
            return Mathf.Lerp(
                basePulseIntensity,
                stunPulseIntensity,
                blend);
        }

        public float EvaluateSparkleIntensity(float baseSparkleIntensity)
        {
            float blend = EvaluateBlink01();
            return Mathf.Lerp(
                baseSparkleIntensity,
                stunSparkleIntensity,
                blend);
        }
    }
}