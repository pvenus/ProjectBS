using System;
using Presentation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [AutoBindPrefix("Info")]
    public sealed class UIContentInfoView : UIView
    {
        [Header("Identity")]
        [AutoBind] [SerializeField] private Image iconImage;
        [AutoBind] [SerializeField] private TMP_Text nameText;
        [AutoBind] [SerializeField] private RectTransform tagRoot;

        [Header("Body")]
        [AutoBind] [SerializeField] private TMP_Text descriptionText;
        [AutoBind] [SerializeField] private ScrollRect scrollRect;
        [AutoBind] [SerializeField] private RectTransform groupRoot;
        [AutoBind] [SerializeField] private TMP_Text statusText;

        [Header("Templates")]
        [SerializeField] private UIContentInfoTagView tagPrefab;
        [SerializeField] private UIContentInfoGroupView groupPrefab;

        private PresentationTextFormatter formatter = new();

        public event Action<string> DetailRequested;

        public void SetFormatter(PresentationTextFormatter value)
        {
            formatter = value ?? new PresentationTextFormatter();
        }

        public void Bind(ContentPresentationData content)
        {
            ClearChildren(tagRoot);
            ClearChildren(groupRoot);

            if (content == null)
            {
                BindEmpty();
                return;
            }

            if (nameText != null)
            {
                nameText.text = content.Identity?.DisplayName ?? string.Empty;
            }

            if (iconImage != null)
            {
                iconImage.sprite = content.Identity?.Icon;
                iconImage.enabled = iconImage.sprite != null;
            }

            SetOptionalText(descriptionText, content.Description);
            SetStatus(content.Status);
            BindTags(content);
            BindGroups(content);
            RefreshScrollLayout();
        }

        private void BindTags(ContentPresentationData content)
        {
            if (tagRoot == null || tagPrefab == null)
            {
                return;
            }

            foreach (string key in content.ClassificationKeys)
            {
                string text = formatter.FormatClassification(key);
                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }

                UIContentInfoTagView tag = Instantiate(tagPrefab, tagRoot);
                tag.Bind(text);
            }
        }

        private void BindGroups(ContentPresentationData content)
        {
            if (groupRoot == null || groupPrefab == null)
            {
                return;
            }

            foreach (PresentationGroupData groupData in content.Groups)
            {
                if (groupData == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(
                        formatter.FormatGroupLabel(groupData.Key)))
                {
                    continue;
                }

                UIContentInfoGroupView group = Instantiate(groupPrefab, groupRoot);
                group.Bind(groupData, formatter, OnDetailRequested);
            }
        }

        private void SetStatus(ContentPresentationStatus status)
        {
            if (statusText == null)
            {
                return;
            }

            bool visible = status != ContentPresentationStatus.Supported;
            statusText.gameObject.SetActive(visible);
            statusText.text = visible ? formatter.FormatLabel(status.ToString()) : string.Empty;
        }

        private void BindEmpty()
        {
            if (nameText != null)
            {
                nameText.text = string.Empty;
            }
            if (iconImage != null)
            {
                iconImage.sprite = null;
                iconImage.enabled = false;
            }
            SetOptionalText(descriptionText, string.Empty);
            if (statusText != null)
            {
                statusText.gameObject.SetActive(true);
                statusText.text = formatter.FormatLabel(ContentPresentationStatus.Unsupported.ToString());
            }
        }

        private void OnDetailRequested(string contentId)
        {
            DetailRequested?.Invoke(contentId);
        }

        private void RefreshScrollLayout()
        {
            if (groupRoot != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(groupRoot);
            }

            Canvas.ForceUpdateCanvases();

            if (scrollRect == null)
            {
                return;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.transform as RectTransform);
            Canvas.ForceUpdateCanvases();
            scrollRect.StopMovement();
            scrollRect.verticalNormalizedPosition = 1f;
        }

        private static void SetOptionalText(TMP_Text target, string value)
        {
            if (target == null)
            {
                return;
            }

            bool visible = !string.IsNullOrWhiteSpace(value);
            target.gameObject.SetActive(visible);
            target.text = visible ? value : string.Empty;
        }

        private static void ClearChildren(RectTransform root)
        {
            if (root == null)
            {
                return;
            }

            for (int index = root.childCount - 1; index >= 0; index--)
            {
                GameObject child = root.GetChild(index).gameObject;
                child.SetActive(false);
                Destroy(child);
            }
        }
    }
}
