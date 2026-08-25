using Presentation;
using TMPro;
using UI;
using UnityEngine;

namespace Shrine.UI
{
    [AutoBindPrefix("FaithEffectCard")]
    public sealed class FaithLevelEffectCardView : UIComponent
    {
        [AutoBind] [SerializeField] private TMP_Text stateText;
        [AutoBind] [SerializeField] private TMP_Text levelText;
        [AutoBind] [SerializeField] private UIContentInfoView contentView;
        [AutoBind] [SerializeField] private TMP_Text emptyStateText;

        public UIContentInfoView ContentView => contentView;

        public void Bind(
            int level,
            string stateLocalizationKey,
            ContentPresentationData content)
        {
            SetHeader(level, stateLocalizationKey);

            if (contentView != null)
            {
                contentView.gameObject.SetActive(content != null);
                if (content != null)
                {
                    contentView.Bind(content);
                }
            }

            SetEmptyState(
                content == null,
                "presentation.faith.effect_data.pending");
        }

        public void BindNoNextLevel(int currentLevel)
        {
            SetHeader(
                currentLevel,
                "presentation.faith.next_level_effects");

            if (contentView != null)
            {
                contentView.gameObject.SetActive(false);
            }

            SetEmptyState(
                true,
                "presentation.faith.next_level.none");
        }

        public void ClearCard(string emptyStateLocalizationKey)
        {
            if (contentView != null)
            {
                contentView.gameObject.SetActive(false);
            }

            SetEmptyState(true, emptyStateLocalizationKey);
        }

        private void SetHeader(
            int level,
            string stateLocalizationKey)
        {
            if (stateText != null)
            {
                stateText.text = ResolveLabel(stateLocalizationKey);
            }

            if (levelText != null)
            {
                string format = ResolveLabel("presentation.faith.level_format");
                levelText.text = format.Contains("{0}")
                    ? string.Format(format, level)
                    : $"{format} {level}";
            }
        }

        private void SetEmptyState(
            bool visible,
            string localizationKey)
        {
            if (emptyStateText == null)
            {
                return;
            }

            emptyStateText.gameObject.SetActive(visible);
            emptyStateText.text = visible
                ? ResolveLabel(localizationKey)
                : string.Empty;
        }

        private static string ResolveLabel(string localizationKey)
        {
            string resolved =
                PresentationLocalizedTextResolver.ResolveLabel(localizationKey);
            return string.IsNullOrWhiteSpace(resolved)
                ? localizationKey ?? string.Empty
                : resolved;
        }
    }
}
