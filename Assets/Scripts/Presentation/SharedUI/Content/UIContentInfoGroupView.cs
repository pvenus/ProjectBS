using System;
using Presentation;
using TMPro;
using UnityEngine;

namespace UI
{
    [AutoBindPrefix("Group")]
    public sealed class UIContentInfoGroupView : UIComponent
    {
        [AutoBind] [SerializeField] private TMP_Text titleText;
        [AutoBind] [SerializeField] private TMP_Text descriptionText;
        [AutoBind] [SerializeField] private RectTransform entryRoot;
        [SerializeField] private UIContentInfoEntryView entryPrefab;

        public void Bind(
            PresentationGroupData group,
            PresentationTextFormatter formatter,
            Action<string> detailRequested)
        {
            formatter ??= new PresentationTextFormatter();
            ClearChildren();

            if (titleText != null)
            {
                titleText.text = formatter.FormatGroupLabel(group?.Key);
            }

            if (descriptionText != null)
            {
                bool visible = !string.IsNullOrWhiteSpace(group?.Description);
                descriptionText.gameObject.SetActive(visible);
                descriptionText.text = visible ? group.Description : string.Empty;
            }

            if (group == null || entryRoot == null || entryPrefab == null)
            {
                return;
            }

            foreach (PresentationEntryData entryData in group.Entries)
            {
                if (entryData == null)
                {
                    continue;
                }

                UIContentInfoEntryView entry = Instantiate(entryPrefab, entryRoot);
                entry.Bind(entryData, formatter, detailRequested);
            }
        }

        private void ClearChildren()
        {
            if (entryRoot == null)
            {
                return;
            }

            for (int index = entryRoot.childCount - 1; index >= 0; index--)
            {
                Destroy(entryRoot.GetChild(index).gameObject);
            }
        }
    }
}
