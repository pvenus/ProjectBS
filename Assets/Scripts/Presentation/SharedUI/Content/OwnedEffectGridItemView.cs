using System;
using Presentation;
using UnityEngine;

[AutoBindPrefix("UI")]
public sealed class OwnedEffectGridItemView : UIComponent
{
    [AutoBind]
    [SerializeField] private UISelectableIconButton selectableIconButton;

    public OwnedEffectInventoryItemData Item { get; private set; }

    public void Bind(
        OwnedEffectInventoryItemData item,
        Action<OwnedEffectGridItemView> onSelected)
    {
        Item = item;

        if (selectableIconButton == null)
        {
            Debug.LogError(
                "[OwnedEffectGridItemView] UISelectableIconButton is not assigned.",
                this);
            return;
        }

        selectableIconButton.SetIcon(item?.Content?.Identity?.Icon);
        selectableIconButton.SetLocked(false);
        selectableIconButton.SetInteractable(item?.Content != null);
        selectableIconButton.SetSelected(false);
        selectableIconButton.Bind(() => onSelected?.Invoke(this));
    }

    public void SetSelected(bool selected)
    {
        selectableIconButton?.SetSelected(selected);
    }
}
