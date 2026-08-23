using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[AutoBindPrefix("UI")]
public class UIGridScrollWidget : UIComponent
{
    public enum ScrollDirection
    {
        Vertical,
        Horizontal
    }

    [AutoBind] [SerializeField] private ScrollRect scrollRect;
    [AutoBind] [SerializeField] private RectTransform viewport;
    [AutoBind] [SerializeField] private RectTransform content;
    [AutoBind] [SerializeField] private GridLayoutGroup gridLayoutGroup;
    [AutoBind] [SerializeField] private ContentSizeFitter contentSizeFitter;

    [Header("Settings")]
    [SerializeField] private ScrollDirection scrollDirection = ScrollDirection.Vertical;
    [SerializeField] private Vector2 cellSize = new Vector2(100, 100);
    [SerializeField] private Vector2 spacing = new Vector2(10, 10);
    [SerializeField] private RectOffset padding;
    [SerializeField] private bool useFlexibleConstraint = true;
    [SerializeField] private int constraintCount = 4;

    private readonly List<GameObject> spawnedItems = new List<GameObject>();

    private void Awake()
    {
        ApplySettings();
    }

    public void ApplySettings()
    {
        if (content == null && scrollRect != null)
        {
            content = scrollRect.content;
        }

        if (content != null)
        {
            if (gridLayoutGroup == null) gridLayoutGroup = content.GetComponent<GridLayoutGroup>();
            if (contentSizeFitter == null) contentSizeFitter = content.GetComponent<ContentSizeFitter>();
        }

        if (gridLayoutGroup != null)
        {
            gridLayoutGroup.cellSize = cellSize;
            gridLayoutGroup.spacing = spacing;
            if (padding != null) gridLayoutGroup.padding = padding;

            if (useFlexibleConstraint)
            {
                gridLayoutGroup.constraint = GridLayoutGroup.Constraint.Flexible;
            }
            else
            {
                if (scrollDirection == ScrollDirection.Vertical)
                {
                    gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                    gridLayoutGroup.constraintCount = constraintCount;
                }
                else
                {
                    gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedRowCount;
                    gridLayoutGroup.constraintCount = constraintCount;
                }
            }
        }

        if (contentSizeFitter != null)
        {
            if (scrollDirection == ScrollDirection.Vertical)
            {
                contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            }
            else
            {
                contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
                contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
        }

        if (scrollRect != null)
        {
            scrollRect.vertical = scrollDirection == ScrollDirection.Vertical;
            scrollRect.horizontal = scrollDirection == ScrollDirection.Horizontal;
        }
    }

    /// <summary>
    /// 데이터를 바탕으로 그리드에 아이템을 생성하고 바인딩합니다. (초기화 시 기존 아이템 삭제)
    /// </summary>
    public void SetItems<TData>(IEnumerable<TData> dataList, GameObject prefab, Action<GameObject, TData, int> bindAction)
    {
        ApplySettings();
        ClearItems();

        if (dataList == null || prefab == null || content == null)
            return;

        int index = 0;
        foreach (var data in dataList)
        {
            GameObject instance = Instantiate(prefab, content);
            spawnedItems.Add(instance);

            bindAction?.Invoke(instance, data, index);
            index++;
        }

        RefreshLayout();
    }

    public void AddItem(GameObject item)
    {
        if (item != null && content != null)
        {
            item.transform.SetParent(content, false);
            spawnedItems.Add(item);
        }
    }

    public void ClearItems()
    {
        foreach (var item in spawnedItems)
        {
            if (item != null)
            {
                Destroy(item);
            }
        }
        spawnedItems.Clear();
    }

    public void RefreshLayout()
    {
        if (scrollRect != null)
        {
            // Canvas 강제 업데이트를 통해 레이아웃 갱신
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 1f;
            scrollRect.horizontalNormalizedPosition = 0f;
        }
    }

    [ContextMenu("Ensure Structure")]
    public void EnsureStructure()
    {
        if (scrollRect == null)
        {
            scrollRect = GetComponent<ScrollRect>();
            if (scrollRect == null)
                scrollRect = gameObject.AddComponent<ScrollRect>();
        }

        if (viewport == null && scrollRect.viewport != null)
        {
            viewport = scrollRect.viewport;
        }

        if (viewport == null)
        {
            Transform vp = transform.Find("Viewport");
            if (vp == null)
            {
                GameObject vpGo = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
                vp = vpGo.transform;
                vp.SetParent(transform, false);

                RectTransform vpRt = vp.GetComponent<RectTransform>();
                vpRt.anchorMin = Vector2.zero;
                vpRt.anchorMax = Vector2.one;
                vpRt.sizeDelta = Vector2.zero;

                Image vpImg = vp.GetComponent<Image>();
                vpImg.color = new Color(1,1,1,0.01f);
                vp.GetComponent<Mask>().showMaskGraphic = false;
            }
            viewport = vp.GetComponent<RectTransform>();
            scrollRect.viewport = viewport;
        }

        if (content == null && scrollRect.content != null)
        {
            content = scrollRect.content;
        }

        if (content == null)
        {
            Transform ct = viewport.Find("Content");
            if (ct == null)
            {
                GameObject ctGo = new GameObject("Content", typeof(RectTransform));
                ct = ctGo.transform;
                ct.SetParent(viewport, false);

                RectTransform ctRt = ct.GetComponent<RectTransform>();
                ctRt.anchorMin = new Vector2(0, 1);
                ctRt.anchorMax = new Vector2(1, 1);
                ctRt.pivot = new Vector2(0, 1);
                ctRt.sizeDelta = new Vector2(0, 0); // 높이는 Fitter가 조절
            }
            content = ct.GetComponent<RectTransform>();
            scrollRect.content = content;
        }

        gridLayoutGroup = content.GetComponent<GridLayoutGroup>();
        if (gridLayoutGroup == null) gridLayoutGroup = content.gameObject.AddComponent<GridLayoutGroup>();

        contentSizeFitter = content.GetComponent<ContentSizeFitter>();
        if (contentSizeFitter == null) contentSizeFitter = content.gameObject.AddComponent<ContentSizeFitter>();

        // 속성 세팅
        gridLayoutGroup.cellSize = cellSize;
        gridLayoutGroup.spacing = spacing;
        if (padding != null) gridLayoutGroup.padding = padding;

        if (scrollDirection == ScrollDirection.Vertical)
        {
            scrollRect.vertical = true;
            scrollRect.horizontal = false;

            if (useFlexibleConstraint)
            {
                gridLayoutGroup.constraint = GridLayoutGroup.Constraint.Flexible;
            }
            else
            {
                gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayoutGroup.constraintCount = constraintCount;
            }

            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        }
        else
        {
            scrollRect.vertical = false;
            scrollRect.horizontal = true;

            if (useFlexibleConstraint)
            {
                gridLayoutGroup.constraint = GridLayoutGroup.Constraint.Flexible;
            }
            else
            {
                gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedRowCount;
                gridLayoutGroup.constraintCount = constraintCount;
            }

            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            RectTransform ctRt = content.GetComponent<RectTransform>();
            ctRt.anchorMin = new Vector2(0, 0);
            ctRt.anchorMax = new Vector2(0, 1);
            ctRt.pivot = new Vector2(0, 0.5f);
        }

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(gameObject);
#endif
    }
}
