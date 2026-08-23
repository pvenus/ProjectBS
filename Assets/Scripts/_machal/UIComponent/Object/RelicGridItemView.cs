using System;
using UnityEngine;
using UIFramework.Data;

[AutoBindPrefix("UI")]
public class RelicGridItemView : UIComponent
{
    [AutoBind] [SerializeField] private UISelectableIconButton selectableIconButton;

    private RelicCollectionItemViewData viewData;

    public void Bind(RelicCollectionItemViewData data, Action<RelicCollectionItemViewData> onSelected)
    {
        viewData = data;

        if (selectableIconButton != null)
        {
            if (data.isUnlocked)
            {
                selectableIconButton.SetIcon(data.icon);
                selectableIconButton.SetInteractable(true);
                selectableIconButton.SetLocked(false);
            }
            else
            {
                selectableIconButton.SetIcon(data.lockedSilhouetteIcon);
                selectableIconButton.SetInteractable(false); // 잠긴 유물은 클릭 무시
                selectableIconButton.SetLocked(true);
            }

            selectableIconButton.SetSelected(false);

            selectableIconButton.Bind(() =>
            {
                if (data.isUnlocked)
                {
                    onSelected?.Invoke(data);
                }
                else
                {
                    Debug.Log($"[RelicGridItemView] 잠긴 유물 클릭 무시됨: {data.relicId}");
                }
            });
        }
    }

    public void SetSelected(bool selected)
    {
        if (selectableIconButton != null)
        {
            selectableIconButton.SetSelected(selected);
        }
    }

    public string GetRelicId()
    {
        return viewData != null ? viewData.relicId : string.Empty;
    }
}
