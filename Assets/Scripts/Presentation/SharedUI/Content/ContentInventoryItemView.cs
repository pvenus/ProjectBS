using System;
using Presentation;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [AutoBindPrefix("UI")]
    public sealed class ContentInventoryItemView : UIComponent
    {
        [AutoBind]
        [SerializeField] private UISelectableIconButton selectableIconButton;

        [AutoBind]
        [SerializeField] private Image activeIndicatorImage;

        public ContentInventoryItemData Item { get; private set; }

        public void Bind(
            ContentInventoryItemData item,
            Action<ContentInventoryItemView> onSelected)
        {
            Item = item;

            if (selectableIconButton == null)
            {
                Debug.LogError(
                    "[ContentInventoryItemView] UISelectableIconButton is not assigned.",
                    this);
                return;
            }

            bool isLocked =
                item?.AcquisitionState == ContentAcquisitionState.Locked;
            bool isActive =
                item?.ActivationState == ContentActivationState.Active;

            selectableIconButton.SetIcon(item?.Content?.Identity?.Icon);
            selectableIconButton.SetLocked(isLocked);
            selectableIconButton.SetInteractable(item?.Content != null);
            selectableIconButton.SetSelected(false);
            selectableIconButton.Bind(() => onSelected?.Invoke(this));

            if (activeIndicatorImage != null)
            {
                activeIndicatorImage.gameObject.SetActive(isActive);
            }
        }

        public void SetSelected(bool selected)
        {
            selectableIconButton?.SetSelected(selected);
        }
    }
}
