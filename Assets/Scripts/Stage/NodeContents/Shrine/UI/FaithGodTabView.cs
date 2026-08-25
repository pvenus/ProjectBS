using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shrine.UI
{
    [AutoBindPrefix("FaithTab")]
    public sealed class FaithGodTabView : UIComponent
    {
        [AutoBind] [SerializeField] private Button button;
        [AutoBind] [SerializeField] private Image iconImage;
        [AutoBind] [SerializeField] private TMP_Text nameText;
        [AutoBind] [SerializeField] private TMP_Text levelText;
        [AutoBind] [SerializeField] private Image selectedFrameImage;
        [AutoBind] [SerializeField] private RectTransform lockedMark;
        [AutoBind] [SerializeField] private RectTransform inactiveMark;

        private Action onSelected;

        private void Awake()
        {
            if (button != null)
            {
                button.onClick.AddListener(HandleClicked);
            }
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(HandleClicked);
            }
        }

        public void Bind(
            Sprite icon,
            string displayName,
            string levelLabel,
            bool selected,
            bool locked,
            bool active,
            Action selectedCallback)
        {
            onSelected = selectedCallback;

            if (iconImage != null)
            {
                iconImage.sprite = icon;
                iconImage.enabled = icon != null;
            }

            if (nameText != null)
            {
                nameText.text = displayName ?? string.Empty;
            }

            if (levelText != null)
            {
                levelText.text = levelLabel ?? string.Empty;
            }

            if (selectedFrameImage != null)
            {
                selectedFrameImage.gameObject.SetActive(selected);
            }

            if (lockedMark != null)
            {
                lockedMark.gameObject.SetActive(locked);
            }

            if (inactiveMark != null)
            {
                inactiveMark.gameObject.SetActive(!active);
            }

            if (button != null)
            {
                button.interactable = true;
            }
        }

        public void SetSelected(bool selected)
        {
            if (selectedFrameImage != null)
            {
                selectedFrameImage.gameObject.SetActive(selected);
            }
        }

        private void HandleClicked()
        {
            onSelected?.Invoke();
        }
    }
}
