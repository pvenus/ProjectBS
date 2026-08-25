using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Battle.UI.StrategicBoard
{
    /// <summary>
    /// Displays the shared strategic resource as a radial gauge and numeric values.
    /// </summary>
    public sealed class StrategicGaugeView : MonoBehaviour
    {
        [Header("Independent Visual Layers")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image fillImage;
        [SerializeField] private Image frameImage;

        [Header("Labels")]
        [SerializeField] private TMP_Text currentMaxText;
        [SerializeField] private TMP_Text chargePerSecondText;
        [SerializeField] private string chargeFormat = "+{0:0.##}/s";

        [Header("Gauge Animation")]
        [SerializeField] private bool animateIncreases = true;
        [SerializeField, Min(0f)] private float increaseTweenDuration = 0.25f;

        private Coroutine gaugeTweenCoroutine;
        private float displayedGaugeValue;
        private bool hasDisplayedGaugeValue;

        public int CurrentValue { get; private set; }
        public int MaxValue { get; private set; }
        public float ChargePerSecond { get; private set; }
        public float NormalizedValue => MaxValue > 0
            ? Mathf.Clamp01((float)CurrentValue / MaxValue)
            : 0f;
        public float DisplayedNormalizedValue => MaxValue > 0
            ? Mathf.Clamp01(displayedGaugeValue / MaxValue)
            : 0f;
        public bool AnimateIncreases => animateIncreases;
        public float IncreaseTweenDuration => increaseTweenDuration;
        public bool IsGaugeTweenRunning => gaugeTweenCoroutine != null;

        private void Awake()
        {
            ConfigureRadialFill();
            ApplyDisplayedGauge(CurrentValue);
            SetChargePerSecond(ChargePerSecond);
        }

        private void OnDisable()
        {
            CancelGaugeTween(true);
        }

        private void OnDestroy()
        {
            CancelGaugeTween(true);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ConfigureRadialFill();
            increaseTweenDuration = Mathf.Max(0f, increaseTweenDuration);

            if (!Application.isPlaying)
            {
                ApplyDisplayedGauge(CurrentValue);
                SetChargePerSecond(ChargePerSecond);
            }
        }
#endif

        public void SetGauge(int current, int max)
        {
            SetGaugeImmediate(current, max);
        }

        public void SetGaugeImmediate(int current, int max)
        {
            SetGaugeInternal(current, max, false);
        }

        public void SetGaugeAnimated(int current, int max)
        {
            SetGaugeInternal(current, max, true);
        }

        private void SetGaugeInternal(int current, int max, bool allowAnimation)
        {
            int nextMaxValue = Mathf.Max(0, max);
            int nextCurrentValue = nextMaxValue > 0
                ? Mathf.Clamp(current, 0, nextMaxValue)
                : 0;
            float startDisplayedValue = hasDisplayedGaugeValue
                ? displayedGaugeValue
                : CurrentValue;
            bool maxChanged = MaxValue != nextMaxValue;

            MaxValue = nextMaxValue;
            CurrentValue = nextCurrentValue;

            CancelGaugeTween(false);

            bool shouldAnimateIncrease = allowAnimation &&
                animateIncreases &&
                !maxChanged &&
                CurrentValue > startDisplayedValue &&
                increaseTweenDuration > Mathf.Epsilon &&
                isActiveAndEnabled;

            if (!shouldAnimateIncrease)
            {
                ApplyDisplayedGauge(CurrentValue);
                return;
            }

            gaugeTweenCoroutine = StartCoroutine(
                AnimateGaugeIncrease(startDisplayedValue, CurrentValue));
        }

        public void SetChargePerSecond(float amount)
        {
            ChargePerSecond = amount;

            if (chargePerSecondText != null)
            {
                chargePerSecondText.text = string.Format(chargeFormat, amount);
            }
        }

        private void ConfigureRadialFill()
        {
            if (fillImage == null)
            {
                return;
            }

            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Radial360;
            fillImage.fillOrigin = (int)Image.Origin360.Top;
            fillImage.fillClockwise = true;
        }

        private IEnumerator AnimateGaugeIncrease(float from, float to)
        {
            float elapsed = 0f;

            while (elapsed < increaseTweenDuration)
            {
                elapsed += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(elapsed / increaseTweenDuration);
                float easedTime = normalizedTime * normalizedTime * (3f - 2f * normalizedTime);
                ApplyDisplayedGauge(Mathf.Lerp(from, to, easedTime));
                yield return null;
            }

            ApplyDisplayedGauge(to);
            gaugeTweenCoroutine = null;
        }

        private void ApplyDisplayedGauge(float value)
        {
            displayedGaugeValue = MaxValue > 0
                ? Mathf.Clamp(value, 0f, MaxValue)
                : 0f;
            hasDisplayedGaugeValue = true;

            if (fillImage != null)
            {
                fillImage.fillAmount = DisplayedNormalizedValue;
            }

            if (currentMaxText != null)
            {
                currentMaxText.text = $"{Mathf.RoundToInt(displayedGaugeValue)} / {MaxValue}";
            }
        }

        private void CancelGaugeTween(bool snapToTarget)
        {
            if (gaugeTweenCoroutine != null)
            {
                StopCoroutine(gaugeTweenCoroutine);
                gaugeTweenCoroutine = null;
            }

            if (snapToTarget)
            {
                ApplyDisplayedGauge(CurrentValue);
            }
        }
    }
}
