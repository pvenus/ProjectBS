using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Battle.UI.BattleStatus
{
    [DisallowMultipleComponent]
    public sealed class BattleProgressView : MonoBehaviour
    {
        [Header("Visibility")]
        [SerializeField] private GameObject visibilityRoot;

        [Header("Replaceable Visuals")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image decorationImage;

        [Header("Progress")]
        [SerializeField] private TMP_Text battleNameText;
        [SerializeField] private TMP_Text waveText;
        [SerializeField] private TMP_Text remainingTimeText;
        [SerializeField] private TMP_Text elapsedTimeText;
        [SerializeField] private TMP_Text remainingEnemyCountText;

        public bool IsVisible =>
            visibilityRoot != null
                ? visibilityRoot.activeSelf
                : gameObject.activeSelf;

        public void Show(BattleProgressViewData data)
        {
            if (data == null)
            {
                Clear();
                Hide();
                return;
            }

            Render(data);
            SetVisible(true);
        }

        public void Hide()
        {
            SetVisible(false);
        }

        public void SetVisible(bool visible)
        {
            GameObject target =
                visibilityRoot != null
                    ? visibilityRoot
                    : gameObject;

            if (target.activeSelf != visible)
            {
                target.SetActive(visible);
            }
        }

        public void Render(BattleProgressViewData data)
        {
            if (data == null)
            {
                Clear();
                return;
            }

            SetText(battleNameText, data.BattleName);
            SetText(waveText, $"WAVE {data.CurrentWave:N0} / {data.TotalWave:N0}");
            SetText(
                remainingTimeText,
                $"REMAIN {FormatDuration(data.RemainingTimeSeconds, true)}");
            SetText(
                elapsedTimeText,
                $"ELAPSED {FormatDuration(data.ElapsedTimeSeconds, false)}");
            SetText(
                remainingEnemyCountText,
                $"ENEMY {data.RemainingEnemyCount:N0}");
        }

        public void Clear()
        {
            SetText(battleNameText, string.Empty);
            SetText(waveText, "WAVE - / -");
            SetText(remainingTimeText, "REMAIN --:--");
            SetText(elapsedTimeText, "ELAPSED --:--");
            SetText(remainingEnemyCountText, "ENEMY -");
        }

        public void SetBackgroundSprite(Sprite sprite)
        {
            if (backgroundImage != null)
            {
                backgroundImage.sprite = sprite;
            }
        }

        public void SetDecorationSprite(Sprite sprite)
        {
            if (decorationImage != null)
            {
                decorationImage.sprite = sprite;
            }
        }

        private static void SetText(TMP_Text target, string value)
        {
            if (target != null)
            {
                target.text = value ?? string.Empty;
            }
        }

        private static string FormatDuration(float seconds, bool roundUp)
        {
            float safeSeconds =
                BattleStatusValueUtility.ToNonNegativeFinite(seconds);

            int totalSeconds =
                roundUp
                    ? Mathf.CeilToInt(safeSeconds)
                    : Mathf.FloorToInt(safeSeconds);

            int minutes = totalSeconds / 60;
            int remainingSeconds = totalSeconds % 60;

            return $"{minutes:00}:{remainingSeconds:00}";
        }
    }
}
