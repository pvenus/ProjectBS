using System;
using Presentation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [AutoBindPrefix("Entry")]
    public sealed class UIContentInfoEntryView : UIComponent
    {
        [AutoBind] [SerializeField] private TMP_Text labelText;
        [AutoBind] [SerializeField] private TMP_Text valueText;
        [AutoBind] [SerializeField] private Button detailButton;

        public void Bind(
            PresentationEntryData entry,
            PresentationTextFormatter formatter,
            Action<string> detailRequested)
        {
            formatter ??= new PresentationTextFormatter();
            string label = formatter.FormatEntryLabel(entry?.Key);
            string value = formatter.FormatEntryValues(entry);
            bool visible = !string.IsNullOrWhiteSpace(label)
                && !string.IsNullOrWhiteSpace(value);
            gameObject.SetActive(visible);
            if (!visible)
            {
                return;
            }

            if (labelText != null)
            {
                labelText.text = label;
            }
            if (valueText != null)
            {
                valueText.text = value;
            }

            if (detailButton == null)
            {
                return;
            }

            detailButton.onClick.RemoveAllListeners();
            bool hasDetail = !string.IsNullOrWhiteSpace(entry?.DetailContentId);
            detailButton.gameObject.SetActive(hasDetail);
            if (hasDetail)
            {
                string contentId = entry.DetailContentId;
                detailButton.onClick.AddListener(() => detailRequested?.Invoke(contentId));
            }
        }
    }
}
