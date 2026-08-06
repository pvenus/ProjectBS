using UnityEngine;
using UnityEngine.UI;

[AutoBindPrefix("Carousel")]
public class UICarouselWidget : UIComponent
{
    public enum CarouselDirection
    {
        Vertical,
        Horizontal
    }

    [Header("Carousel Settings")]
    [SerializeField] private CarouselDirection scrollInputDirection = CarouselDirection.Vertical;
    [Tooltip("기본적인 아이템 간의 간격 (가로면 X, 세로면 Y 값을 조절하세요)")]
    [SerializeField] private Vector2 itemSpacing = new Vector2(0f, -350f);
    [Tooltip("중앙에서 멀어질수록 추가로 적용되는 곡선 오프셋 (C자 커브 등을 만들 때 사용)")]
    [SerializeField] private Vector2 curveOffset = new Vector2(0f, 0f);
    [SerializeField] private float inactiveScale = 0.8f;       // 비활성 패널의 축소 비율
    [SerializeField] private float inactiveDarken = 0.65f;     // 비활성 패널이 어두워지는 정도
    [SerializeField] private float lerpSpeed = 12f;            // 애니메이션 속도
    [SerializeField] private float scrollCooldownDuration = 0.3f; // 스크롤 무시 쿨다운 시간

    private int currentIndex = 0;
    private float scrollCooldown = 0f;

    private RectTransform[] items;
    private CanvasGroup[] canvasGroups;
    private Image[] darkenOverlays;
    private int[] diffs;

    /// <summary>
    /// 대상 RectTransform 배열을 받아 3D 회전목마 형식으로 초기화합니다.
    /// </summary>
    public void Initialize(RectTransform[] newItems, int startingIndex = 0)
    {
        if (newItems == null || newItems.Length == 0) return;

        items = newItems;
        canvasGroups = new CanvasGroup[items.Length];
        darkenOverlays = new Image[items.Length];
        diffs = new int[items.Length];
        currentIndex = startingIndex;

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null)
            {
                // 강제로 중앙 정렬 (앵커를 0.5, 0.5로 맞추어 Y좌표 0이 무조건 화면 중앙이 되도록)
                Vector2 originalSize = items[i].rect.size;
                items[i].anchorMin = new Vector2(0.5f, 0.5f);
                items[i].anchorMax = new Vector2(0.5f, 0.5f);
                items[i].sizeDelta = originalSize;

                canvasGroups[i] = items[i].GetComponent<CanvasGroup>();
                if (canvasGroups[i] == null)
                {
                    canvasGroups[i] = items[i].gameObject.AddComponent<CanvasGroup>();
                }
                canvasGroups[i].alpha = 1f;

                // 어둡게 만들기 위한 검은색 오버레이 이미지 동적 생성
                GameObject overlay = new GameObject("DarkenOverlay");
                overlay.transform.SetParent(items[i].transform, false);
                overlay.transform.SetAsLastSibling(); // 맨 앞으로
                
                RectTransform overlayRect = overlay.AddComponent<RectTransform>();
                overlayRect.anchorMin = Vector2.zero;
                overlayRect.anchorMax = Vector2.one;
                overlayRect.sizeDelta = Vector2.zero; // 부모 크기에 꽉 차게
                
                Image img = overlay.AddComponent<Image>();
                img.color = new Color(0f, 0f, 0f, 0f);
                img.raycastTarget = false; // 클릭 방해 안 함
                
                darkenOverlays[i] = img;
            }
        }
    }

    private void Update()
    {
        if (items == null || items.Length == 0) return;

        HandleScrollInput();
        UpdateCarouselAnimation();
    }

    private void HandleScrollInput()
    {
        if (scrollCooldown > 0f)
        {
            scrollCooldown -= Time.deltaTime;
        }

        float scroll = 0f;
        if (scrollInputDirection == CarouselDirection.Vertical)
        {
            scroll = Input.mouseScrollDelta.y;
        }
        else
        {
            // 가로 방향일 경우 마우스 휠 X(지원하는 마우스) 또는 Y를 모두 인식
            scroll = Input.mouseScrollDelta.x != 0f ? Input.mouseScrollDelta.x : Input.mouseScrollDelta.y;
        }

        if (scroll != 0f && scrollCooldown <= 0f)
        {
            if (scroll > 0f)
            {
                // 위로 스크롤 (이전 아이템)
                currentIndex--;
                if (currentIndex < 0) currentIndex = items.Length - 1;
            }
            else if (scroll < 0f)
            {
                // 아래로 스크롤 (다음 아이템)
                currentIndex++;
                if (currentIndex >= items.Length) currentIndex = 0;
            }
            
            scrollCooldown = scrollCooldownDuration;
        }
    }

    private void UpdateCarouselAnimation()
    {
        // 3개일 경우 half=1, 5개일 경우 half=2 가 됨
        int half = items.Length / 2;

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null) continue;

            // 무한 회전을 위한 상대적 위치(인덱스 차이) 계산
            int diff = i - currentIndex;
            
            // 차이가 절반보다 크면 반대편으로 넘김 (무한 루프)
            if (diff > half) diff -= items.Length;
            if (diff < -half) diff += items.Length;

            diffs[i] = diff;

            // 목표 상태 계산 (선형 간격 + 절대값 기반 커브 간격)
            Vector2 targetPos = (itemSpacing * diff) + (curveOffset * Mathf.Abs(diff));

            float targetScale = (diff == 0) ? 1f : inactiveScale;
            float targetDarkenAlpha = (diff == 0) ? 0f : inactiveDarken;

            // 서서히 이동, 축소/확대
            items[i].anchoredPosition = Vector2.Lerp(
                items[i].anchoredPosition, 
                targetPos, 
                Time.deltaTime * lerpSpeed);

            items[i].localScale = Vector3.Lerp(
                items[i].localScale, 
                new Vector3(targetScale, targetScale, 1f), 
                Time.deltaTime * lerpSpeed);

            // 어둡게 만드는 오버레이 투명도 조절
            if (darkenOverlays[i] != null)
            {
                Color c = darkenOverlays[i].color;
                c.a = Mathf.Lerp(c.a, targetDarkenAlpha, Time.deltaTime * lerpSpeed);
                darkenOverlays[i].color = c;
            }

            if (canvasGroups[i] != null)
            {
                // 중앙 패널만 상호작용 가능
                canvasGroups[i].interactable = (diff == 0);
                canvasGroups[i].blocksRaycasts = (diff == 0);
            }
        }

        // 하이어라키 렌더링 순서 조정 (거리가 먼 것부터 먼저 그리고, 중앙 패널을 가장 마지막에 그림)
        for (int d = half; d >= 0; d--)
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] != null && Mathf.Abs(diffs[i]) == d)
                {
                    items[i].SetAsLastSibling();
                }
            }
        }
    }
}
