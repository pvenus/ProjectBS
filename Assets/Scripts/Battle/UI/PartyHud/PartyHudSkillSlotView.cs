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

        public PartyHudSkillSlotData Data { get; private set; }

        private PartyHudSkillState currentState;

        private void Awake()
        {
            ConfigureCooldownFill();
            HideStateOverlays();
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
            HideStateOverlays();

            if (state == PartyHudSkillState.Passive)
            {
                SetCooldown(0f, 0f);
            }
        }

        private void HideStateOverlays()
        {
            if (stateOverlayImage != null && stateOverlayImage.gameObject.activeSelf)
            {
                stateOverlayImage.gameObject.SetActive(false);
            }

            if (stateText != null && stateText.gameObject.activeSelf)
            {
                stateText.gameObject.SetActive(false);
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
