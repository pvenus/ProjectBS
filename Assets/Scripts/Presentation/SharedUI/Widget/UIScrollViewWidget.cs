using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 에디터에서 컴포넌트를 붙이는 즉시 ScrollRect 구조(Viewport, Content)를 자동 생성하고,
/// Inspector에서 옵션(방향, 간격 등)을 바꾸면 즉시 하위 객체들에 반영하는 스마트 위젯입니다.
/// </summary>
[RequireComponent(typeof(ScrollRect))]
public class UIScrollViewWidget : UIComponent
{
    public enum ScrollDirection
    {
        Horizontal,
        Vertical,
        Both
    }

    [Header("Scroll Options")]
    [SerializeField] private ScrollDirection direction = ScrollDirection.Horizontal;
    [SerializeField] private float spacing = 10f;
    [SerializeField] private RectOffset padding = new RectOffset();

    [Header("References")]
    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform content;

    private ScrollRect scrollRect;

    public Transform ContentRoot => content != null ? content : (GetComponent<ScrollRect>().content);

    private void Awake()
    {
        if (Application.isPlaying)
        {
            scrollRect = GetComponent<ScrollRect>();
        }
    }

    public void ResetScrollPosition()
    {
        if (scrollRect == null) scrollRect = GetComponent<ScrollRect>();
        if (scrollRect == null) return;
        
        if (scrollRect.horizontal) scrollRect.horizontalNormalizedPosition = 0f;
        if (scrollRect.vertical) scrollRect.verticalNormalizedPosition = 1f;
    }

#if UNITY_EDITOR
    private void Reset()
    {
        GenerateHierarchy();
    }

    protected override void OnValidate()
    {
        base.OnValidate();

        if (Application.isPlaying) return;

        // OnValidate 내부에서 구조 파괴/생성(DestroyImmediate 등)을 안전하게 처리하기 위해 delayCall 사용
        EditorApplication.delayCall += () =>
        {
            if (this == null) return;
            UpdateScrollSettings();
        };
    }

    [ContextMenu("Force Regenerate Hierarchy")]
    private void GenerateHierarchy()
    {
        scrollRect = GetComponent<ScrollRect>();
        if (scrollRect == null) scrollRect = gameObject.AddComponent<ScrollRect>();

        // 1. Viewport 생성
        if (viewport == null)
        {
            Transform existingViewport = transform.Find("Viewport");
            if (existingViewport != null) 
            {
                viewport = existingViewport.GetComponent<RectTransform>();
            }
            else
            {
                GameObject vpObj = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
                vpObj.transform.SetParent(transform, false);
                viewport = vpObj.GetComponent<RectTransform>();
                
                viewport.anchorMin = Vector2.zero;
                viewport.anchorMax = Vector2.one;
                viewport.sizeDelta = Vector2.zero;
                
                vpObj.GetComponent<Mask>().showMaskGraphic = false;
                
                Image img = vpObj.GetComponent<Image>();
                img.color = new Color(1, 1, 1, 0.01f); // Mask를 위한 투명 이미지
            }
        }

        // 2. Content 생성
        if (content == null && viewport != null)
        {
            Transform existingContent = viewport.Find("Content");
            if (existingContent != null) 
            {
                content = existingContent.GetComponent<RectTransform>();
            }
            else
            {
                GameObject ctObj = new GameObject("Content", typeof(RectTransform));
                ctObj.transform.SetParent(viewport, false);
                content = ctObj.GetComponent<RectTransform>();
                
                content.anchorMin = new Vector2(0, 1);
                content.anchorMax = new Vector2(0, 1);
                content.pivot = new Vector2(0, 1);
                content.sizeDelta = Vector2.zero;
            }
        }

        scrollRect.viewport = viewport;
        scrollRect.content = content;

        UpdateScrollSettings();
    }

    private void UpdateScrollSettings()
    {
        if (scrollRect == null) scrollRect = GetComponent<ScrollRect>();
        if (scrollRect == null || content == null) return;

        // 1. 스크롤 허용 방향 갱신
        scrollRect.horizontal = (direction == ScrollDirection.Horizontal || direction == ScrollDirection.Both);
        scrollRect.vertical = (direction == ScrollDirection.Vertical || direction == ScrollDirection.Both);

        // 2. ContentSizeFitter 동적 세팅
        ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
        if (fitter == null) fitter = content.gameObject.AddComponent<ContentSizeFitter>();
        
        fitter.horizontalFit = scrollRect.horizontal ? ContentSizeFitter.FitMode.PreferredSize : ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = scrollRect.vertical ? ContentSizeFitter.FitMode.PreferredSize : ContentSizeFitter.FitMode.Unconstrained;

        // 3. LayoutGroup 동적 교체 및 설정
        if (direction == ScrollDirection.Horizontal)
        {
            var vg = content.GetComponent<VerticalLayoutGroup>();
            if (vg != null) DestroyImmediate(vg);
            
            var hg = content.GetComponent<HorizontalLayoutGroup>();
            if (hg == null) hg = content.gameObject.AddComponent<HorizontalLayoutGroup>();
            
            hg.spacing = spacing;
            hg.padding = padding;
            hg.childControlHeight = false;
            hg.childControlWidth = false;
            hg.childForceExpandHeight = false;
            hg.childForceExpandWidth = false;
        }
        else if (direction == ScrollDirection.Vertical)
        {
            var hg = content.GetComponent<HorizontalLayoutGroup>();
            if (hg != null) DestroyImmediate(hg);
            
            var vg = content.GetComponent<VerticalLayoutGroup>();
            if (vg == null) vg = content.gameObject.AddComponent<VerticalLayoutGroup>();
            
            vg.spacing = spacing;
            vg.padding = padding;
            vg.childControlHeight = false;
            vg.childControlWidth = false;
            vg.childForceExpandHeight = false;
            vg.childForceExpandWidth = false;
        }
        else
        {
            var vg = content.GetComponent<VerticalLayoutGroup>();
            if (vg != null) DestroyImmediate(vg);
            var hg = content.GetComponent<HorizontalLayoutGroup>();
            if (hg != null) DestroyImmediate(hg);
        }
        
        EditorUtility.SetDirty(content.gameObject);
        EditorUtility.SetDirty(this);
    }
#endif
}
