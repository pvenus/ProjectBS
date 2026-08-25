using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Battle.UI.PartyHud
{
    [DisallowMultipleComponent]
    public sealed class PartyHudSkillSlotView : MonoBehaviour
    {
        [Header("Images")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image cooldownFillImage;
        [SerializeField] private Image stateOverlayImage;

        [Header("Labels")]
        [SerializeField] private TMP_Text cooldownText;
        [SerializeField] private TMP_Text stateText;

        [Header("State Colors")]
        [SerializeField] private Color availableColor = new Color(0.24f, 0.78f, 0.42f, 0.9f);
        [SerializeField] private Color unavailableColor = new Color(0.65f, 0.18f, 0.18f, 0.82f);
        [SerializeField] private Color passiveColor = new Color(0.24f, 0.48f, 0.82f, 0.82f);

        public PartyHudSkillSlotData Data { get; private set; }

        private PartyHudSkillState currentState;

        private void Awake()
        {
            ConfigureCooldownFill();
        }

        private void OnValidate()
        {
            ConfigureCooldownFill();
        }

        public void Render(PartyHudSkillSlotData data)
        {
            Data = data;
            gameObject.SetActive(data != null);

            if (data == null)
            {
                return;
            }

            SetIcon(data.Icon);
            SetState(data.State);
            SetCooldown(
                data.CooldownRemainingSeconds,
                data.CooldownDurationSeconds);
        }

        public void SetIcon(Sprite icon)
        {
            if (iconImage == null)
            {
                return;
            }

            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
        }

        public void SetCooldown(float remainingSeconds, float durationSeconds)
        {
            float remaining = Mathf.Max(0f, remainingSeconds);
            float duration = Mathf.Max(0f, durationSeconds);
            bool hasCooldown =
                gameObject.activeSelf
                && currentState != PartyHudSkillState.Passive
                && remaining > 0f;

            if (cooldownFillImage != null)
            {
                ConfigureCooldownFill();
                cooldownFillImage.gameObject.SetActive(hasCooldown);
                cooldownFillImage.fillAmount =
                    duration <= 0f
                        ? (hasCooldown ? 1f : 0f)
                        : Mathf.Clamp01(remaining / duration);
            }

            if (cooldownText != null)
            {
                cooldownText.gameObject.SetActive(hasCooldown);
                cooldownText.text =
                    hasCooldown
                        ? Mathf.CeilToInt(remaining).ToString()
                        : string.Empty;
            }
        }

        public void SetState(PartyHudSkillState state)
        {
            currentState = state;

            string label;
            Color color;
            bool showOverlay;

            switch (state)
            {
                case PartyHudSkillState.Unavailable:
                    label = "LOCKED";
                    color = unavailableColor;
                    showOverlay = true;
                    break;
                case PartyHudSkillState.Passive:
                    label = "PASSIVE";
                    color = passiveColor;
                    showOverlay = true;
                    break;
                default:
                    label = "READY";
                    color = availableColor;
                    showOverlay = false;
                    break;
            }

            if (stateOverlayImage != null)
            {
                stateOverlayImage.gameObject.SetActive(showOverlay);
                stateOverlayImage.color = color;
            }

            if (stateText != null)
            {
                stateText.text = label;
                stateText.color = color;
            }

            if (state == PartyHudSkillState.Passive)
            {
                SetCooldown(0f, 0f);
            }
        }

        private void ConfigureCooldownFill()
        {
            if (cooldownFillImage == null)
            {
                return;
            }

            cooldownFillImage.type = Image.Type.Filled;
            cooldownFillImage.fillMethod = Image.FillMethod.Radial360;
            cooldownFillImage.fillOrigin = (int)Image.Origin360.Top;
            cooldownFillImage.fillClockwise = true;
        }
    }
}
