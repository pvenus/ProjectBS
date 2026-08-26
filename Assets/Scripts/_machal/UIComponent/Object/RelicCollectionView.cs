using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Item;
using Item.UI;
using UIFramework.Data;

[AutoBindPrefix("UI")]
public class RelicCollectionView : UIView
{
    [Header("UI Components")]
    [AutoBind] [SerializeField] private TMP_Text titleText;
    [AutoBind] [SerializeField] private TMP_Text countText;
    [AutoBind] [SerializeField] private UIGridScrollWidget gridRoot;

    [Header("Content Info")]
    [SerializeField] private RelicContentInfoPresenter contentInfoPresenter;

    [Header("Prefabs")]
    [SerializeField] private GameObject relicGridItemPrefab;

    [Header("Relic Source")]
    [SerializeField] private List<RelicSO> relicList = new();

    public IReadOnlyList<RelicSO> RelicList => relicList;

    private Action<RelicCollectionResult> onResultCallback;
    private RelicGridItemView selectedItemView;
    private RelicCollectionViewData currentData;

    private void Awake()
    {
        ResolveContentInfoPresenter();
        Hide();
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        ResolveContentInfoPresenter();
    }
#endif

    public void Show(RelicCollectionViewData data, Action<RelicCollectionResult> onResult)
    {
        onResultCallback = onResult;
        currentData = data;

        HideContentInfo();
        base.Show();
        Refresh(data);
    }

    public override void Show()
    {
        ShowConfiguredRelics();
    }

    [ContextMenu("Show Configured Relics")]
    public void ShowConfiguredRelics()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning(
                "[RelicCollectionView] Enter Play Mode before showing the configured relic list.",
                this);
            return;
        }

        ShowRelics(relicList);
    }

    public void SetRelics(
        IReadOnlyList<RelicSO> relics,
        bool refresh = true)
    {
        List<RelicSO> values = new();

        if (relics != null)
        {
            for (int i = 0; i < relics.Count; i++)
            {
                RelicSO relic = relics[i];
                if (relic != null)
                {
                    values.Add(relic);
                }
            }
        }

        relicList.Clear();
        relicList.AddRange(values);

        if (!refresh || !Application.isPlaying)
        {
            return;
        }

        if (gameObject.activeInHierarchy)
        {
            RefreshRelics(relicList);
        }
        else
        {
            ShowRelics(relicList);
        }
    }

    public void ShowRelics(
        IReadOnlyList<RelicSO> relics,
        Action<RelicCollectionResult> onResult = null)
    {
        ShowRelics(
            relics,
            null,
            null,
            onResult);
    }

    public void ShowRelics(
        IReadOnlyList<RelicSO> relics,
        IReadOnlyCollection<string> unlockedRelicIds,
        Sprite lockedSilhouetteIcon,
        Action<RelicCollectionResult> onResult = null)
    {
        Show(
            BuildCollectionData(
                relics,
                unlockedRelicIds,
                lockedSilhouetteIcon),
            onResult);
    }

    public override void Hide()
    {
        onResultCallback = null;
        selectedItemView = null;

        HideContentInfo();

        base.Hide();
    }

    public void Refresh(RelicCollectionViewData data)
    {
        currentData = data;

        if (data == null) return;

        if (titleText != null)
        {
            titleText.text = "유물 도감";
        }

        if (countText != null)
        {
            countText.text = data.totalCount > 0
                ? $"{data.ownedCount} / {data.totalCount}"
                : "보유 유물 0개";
        }

        if (gridRoot != null && relicGridItemPrefab != null)
        {
			gridRoot.SetItems<RelicCollectionItemViewData>(
                data.relics,
                relicGridItemPrefab,
                (go, itemData, index) =>
                {
                    var itemView = go.GetComponent<RelicGridItemView>();
                    if (itemView != null)
                    {
                        itemView.Bind(itemData, OnRelicSelected);
                    }
                }
            );
        }

        ClearSelection();
    }

    public void RefreshRelics(
        IReadOnlyList<RelicSO> relics)
    {
        RefreshRelics(
            relics,
            null,
            null);
    }

    public void RefreshRelics(
        IReadOnlyList<RelicSO> relics,
        IReadOnlyCollection<string> unlockedRelicIds,
        Sprite lockedSilhouetteIcon)
    {
        Refresh(
            BuildCollectionData(
                relics,
                unlockedRelicIds,
                lockedSilhouetteIcon));
    }

    private static RelicCollectionViewData BuildCollectionData(
        IReadOnlyList<RelicSO> relics,
        IReadOnlyCollection<string> unlockedRelicIds,
        Sprite lockedSilhouetteIcon)
    {
        RelicCollectionViewData data = new();
        if (relics == null)
        {
            return data;
        }

        AddRelicsByCategory(
            data,
            relics,
            unlockedRelicIds,
            lockedSilhouetteIcon,
            true);
        AddRelicsByCategory(
            data,
            relics,
            unlockedRelicIds,
            lockedSilhouetteIcon,
            false);

        return data;
    }

    private static void AddRelicsByCategory(
        RelicCollectionViewData data,
        IReadOnlyList<RelicSO> relics,
        IReadOnlyCollection<string> unlockedRelicIds,
        Sprite lockedSilhouetteIcon,
        bool addGodRelics)
    {
        for (int i = 0; i < relics.Count; i++)
        {
            RelicSO relic = relics[i];
            if (relic == null)
            {
                continue;
            }

            bool isGodRelic =
                !string.IsNullOrEmpty(relic.relicId)
                && relic.relicId.Contains(".god.");
            if (isGodRelic != addGodRelics)
            {
                continue;
            }

            bool isUnlocked =
                unlockedRelicIds == null
                || ContainsRelicId(
                    unlockedRelicIds,
                    relic.relicId);

            data.totalCount++;
            if (isUnlocked)
            {
                data.ownedCount++;
            }

            data.relics.Add(new RelicCollectionItemViewData
            {
                sourceRelic = relic,
                relicId = relic.relicId,
                displayName = relic.DisplayName,
                description = relic.Description,
                icon = relic.icon,
                lockedSilhouetteIcon = lockedSilhouetteIcon,
                isUnlocked = isUnlocked,
            });
        }
    }

    private static bool ContainsRelicId(
        IReadOnlyCollection<string> relicIds,
        string relicId)
    {
        foreach (string candidate in relicIds)
        {
            if (string.Equals(
                candidate,
                relicId,
                StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private void OnRelicSelected(RelicCollectionItemViewData selectedData)
    {
        if (selectedData == null || !selectedData.isUnlocked) return;

        bool isAlreadySelected = selectedItemView != null && selectedItemView.GetRelicId() == selectedData.relicId;

        // 기존 선택 해제
        if (selectedItemView != null)
        {
            selectedItemView.SetSelected(false);
            selectedItemView = null;
        }

        // 이미 선택된 유물을 다시 클릭했다면 토글하여 해제 후 정보창 닫기
        if (isAlreadySelected)
        {
            HideContentInfo();
            return;
        }

        // 새로운 선택 찾아서 갱신
        if (gridRoot != null)
        {
            var itemViews = gridRoot.GetComponentsInChildren<RelicGridItemView>();
            foreach (var view in itemViews)
            {
                if (view.GetRelicId() == selectedData.relicId)
                {
                    selectedItemView = view;
                    selectedItemView.SetSelected(true);
                    break;
                }
            }
        }

        ShowContentInfo(selectedData);

        // 콜백 호출
        onResultCallback?.Invoke(new RelicCollectionResult
        {
            type = RelicCollectionResultType.SelectRelic,
            relicId = selectedData.relicId
        });
    }

    public void ClearSelection()
    {
        if (selectedItemView != null)
        {
            selectedItemView.SetSelected(false);
            selectedItemView = null;
        }

        HideContentInfo();
    }

    private void ShowContentInfo(RelicCollectionItemViewData selectedData)
    {
        ResolveContentInfoPresenter();

        if (contentInfoPresenter == null)
        {
            Debug.LogError(
                "[RelicCollectionView] RelicContentInfoPresenter was not found.",
                this);
            return;
        }

        if (selectedData.sourceRelic == null)
        {
            HideContentInfo();
            Debug.LogWarning(
                $"[RelicCollectionView] RelicSO is missing for '{selectedData.relicId}'.",
                this);
            return;
        }

        contentInfoPresenter.ShowRelic(selectedData.sourceRelic);
    }

    private void HideContentInfo()
    {
        ResolveContentInfoPresenter();
        contentInfoPresenter?.HidePresentation();
    }

    private void ResolveContentInfoPresenter()
    {
        if (contentInfoPresenter == null)
        {
            contentInfoPresenter =
                GetComponentInChildren<RelicContentInfoPresenter>(true);
        }
    }

    public void CloseView()
    {
        onResultCallback?.Invoke(new RelicCollectionResult
        {
            type = RelicCollectionResultType.Close,
            relicId = string.Empty
        });
        Hide();
    }
}
