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

        public int CurrentValue { get; private set; }
        public int MaxValue { get; private set; }
        public float ChargePerSecond { get; private set; }
        public float NormalizedValue => MaxValue > 0
            ? Mathf.Clamp01((float)CurrentValue / MaxValue)
            : 0f;

        private void Awake()
        {
            ConfigureRadialFill();
            RefreshVisuals();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ConfigureRadialFill();
            RefreshVisuals();
        }
#endif

        public void SetGauge(int current, int max)
        {
            MaxValue = Mathf.Max(0, max);
            CurrentValue = MaxValue > 0
                ? Mathf.Clamp(current, 0, MaxValue)
                : 0;
            RefreshGaugeVisuals();
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

        private void RefreshVisuals()
        {
            RefreshGaugeVisuals();
            SetChargePerSecond(ChargePerSecond);
        }

        private void RefreshGaugeVisuals()
        {
            if (fillImage != null)
            {
                fillImage.fillAmount = NormalizedValue;
            }

            if (currentMaxText != null)
            {
                currentMaxText.text = $"{CurrentValue} / {MaxValue}";
            }
        }
    }
}
