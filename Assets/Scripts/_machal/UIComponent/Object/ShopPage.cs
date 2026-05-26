using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Shop;

[AutoBindPrefix("Shop")]
public class ShopPage : UIPage
{
    [Header("UI Components")]
    [AutoBind] [SerializeField] private GameObject panelRoot;
    [AutoBind] [SerializeField] private TMP_Text titleText;
    [AutoBind] [SerializeField] private TMP_Text goldText;

    [Header("Categories (AutoBind 가능하도록 Shop_ 접두사 권장)")]
    [AutoBind] [SerializeField] private ShopCategoryPanel relic;
    [AutoBind] [SerializeField] private ShopCategoryPanel consumable;
    [AutoBind] [SerializeField] private ShopCategoryPanel tactic;

    [Header("Carousel Animation")]
    [SerializeField] private float verticalSpacing = 350f;     // 비활성 패널이 위아래로 떨어지는 거리
    [SerializeField] private float inactiveScale = 0.85f;      // 비활성 패널의 축소 비율
    [SerializeField] private float inactiveAlpha = 0.6f;       // 비활성 패널의 투명도
    [SerializeField] private float lerpSpeed = 10f;            // 애니메이션 속도

    private int currentCategoryIndex = 1; // 0: 위, 1: 중앙(기본), 2: 아래
    private ShopCategoryPanel[] categories;
    private RectTransform[] categoryRects;
    private CanvasGroup[] categoryCanvasGroups;

    [Header("Tooltip (Shop_TooltipWidget 등)")]
    [AutoBind] [SerializeField] private UITooltipWidget tooltipWidget;

    [Header("Manager (비워두면 자동 탐색)")]
    [SerializeField] private StageShopManager shopManager;

    private void Awake()
    {
        HideTooltip();
        if (panelRoot == null)
        {
            Debug.LogWarning("[ShopPage] PanelRoot가 할당되지 않았습니다. 현재 스크립트가 있는 오브젝트 자식으로 UI 패널을 구성하고 할당해주세요.");
        }

        // 카테고리 배열 초기화 (위치 및 스케일 제어용)
        categories = new ShopCategoryPanel[] { relic, consumable, tactic };
        categoryRects = new RectTransform[3];
        categoryCanvasGroups = new CanvasGroup[3];

        for (int i = 0; i < 3; i++)
        {
            if (categories[i] != null)
            {
                categoryRects[i] = categories[i].GetComponent<RectTransform>();
                categoryCanvasGroups[i] = categories[i].GetComponent<CanvasGroup>();
                if (categoryCanvasGroups[i] == null)
                {
                    categoryCanvasGroups[i] = categories[i].gameObject.AddComponent<CanvasGroup>();
                }
            }
        }

        Hide();
    }

    private void Start()
    {
        if (shopManager == null)
        {
            shopManager = StageShopManager.Instance;
            if (shopManager == null)
                shopManager = Object.FindFirstObjectByType<StageShopManager>();
        }

        Subscribe();
    }

    private void OnEnable()
    {
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        if (shopManager == null) return;

        // 중복 구독 방지
        Unsubscribe();

        shopManager.OnShopOpened += HandleShopOpened;
        shopManager.OnShopClosed += HandleShopClosed;
        shopManager.OnShopRefreshed += HandleShopRefreshed;
        shopManager.OnGoldChanged += HandleGoldChanged;
        shopManager.OnItemPurchased += HandleItemPurchased;

        if (shopManager.CurrentShop != null && shopManager.CurrentShop.isOpened)
        {
            ShowShop(shopManager.CurrentShop);
        }
    }

    private void Unsubscribe()
    {
        if (shopManager == null) return;

        shopManager.OnShopOpened -= HandleShopOpened;
        shopManager.OnShopClosed -= HandleShopClosed;
        shopManager.OnShopRefreshed -= HandleShopRefreshed;
        shopManager.OnGoldChanged -= HandleGoldChanged;
        shopManager.OnItemPurchased -= HandleItemPurchased;
    }

    public override void Show()
    {
        if (panelRoot != null) panelRoot.SetActive(true);
        else base.Show();
    }

    public override void Hide()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
        else base.Hide();
    }

    private void HandleShopOpened(ShopRuntimeData shop) => ShowShop(shop);
    private void HandleShopClosed(ShopRuntimeData shop) => Hide();
    
    private void HandleShopRefreshed(ShopRuntimeData shop)
    {
        if (shop == null || !shop.isOpened)
        {
            Hide();
            return;
        }
        ShowShop(shop);
    }

    private void HandleGoldChanged(int gold)
    {
        if (goldText != null) goldText.text = gold.ToString();
        RefreshAllCards();
    }

    private void HandleItemPurchased(ShopRuntimeItem item)
    {
        RefreshAllCards();
    }

    private void RefreshAllCards()
    {
        if (relic != null) relic.RefreshCards();
        if (consumable != null) consumable.RefreshCards();
        if (tactic != null) tactic.RefreshCards();
    }

    /// <summary>
    /// 실제 상점 데이터를 받아 각 카테고리별로 아이템을 분류하고 UI를 생성합니다.
    /// </summary>
    public void ShowShop(ShopRuntimeData shop)
    {
        if (shop == null)
        {
            Hide();
            return;
        }

        Show(); // 오버라이드된 Show() 호출하여 패널 활성화

        if (titleText != null) 
            titleText.text = string.IsNullOrWhiteSpace(shop.shopName) ? "상점" : shop.shopName;

        if (goldText != null && shopManager != null)
            goldText.text = shopManager.CurrentGold.ToString();

        List<ShopRuntimeItem> relics = new List<ShopRuntimeItem>();
        List<ShopRuntimeItem> consumables = new List<ShopRuntimeItem>();
        List<ShopRuntimeItem> tactics = new List<ShopRuntimeItem>();

        if (shop.items != null && !string.IsNullOrWhiteSpace(shop.generatedFromPoolId))
        {
            string[] poolIds = shop.generatedFromPoolId.Split(',');

            foreach (var item in shop.items)
            {
                if (item == null) continue;

                // 기존 ShopPopupUI의 로직을 그대로 이식하여, ProductType이 아닌 생성된 Pool 순서대로 카테고리를 분류합니다.
                string targetPool = item.generatedFromPoolId;
                
                int poolIndex = -1;
                for (int i = 0; i < poolIds.Length; i++)
                {
                    if (poolIds[i] == targetPool)
                    {
                        poolIndex = i;
                        break;
                    }
                }

                if (poolIndex == 0) relics.Add(item);
                else if (poolIndex == 1) consumables.Add(item);
                else if (poolIndex == 2) tactics.Add(item);
                else relics.Add(item); // 매칭 실패 시 기본으로 첫 번째에 배치
            }
        }

        if (relic != null) 
            relic.Bind("유물", relics, OnItemClicked, OnItemHoverEnter, OnItemHoverExit);
            
        if (consumable != null) 
            consumable.Bind("소모품", consumables, OnItemClicked, OnItemHoverEnter, OnItemHoverExit);
            
        if (tactic != null) 
            tactic.Bind("전술", tactics, OnItemClicked, OnItemHoverEnter, OnItemHoverExit);
    }

    private void OnItemClicked(ShopItemCard card)
    {
        if (card.RuntimeItem == null || shopManager == null) return;
        
        Debug.Log($"[ShopPage] 아이템 구매 시도: {card.RuntimeItem.DisplayName}");
        shopManager.TryPurchase(card.RuntimeItem.runtimeId);
    }

    private void OnItemHoverEnter(ShopItemCard card)
    {
        if (tooltipWidget != null)
        {
            tooltipWidget.Show(card.GetTooltipDescription(), Input.mousePosition);
        }
    }

    private void OnItemHoverExit(ShopItemCard card)
    {
        if (tooltipWidget != null)
        {
            tooltipWidget.Hide();
        }
    }

    private void HideTooltip()
    {
        if (tooltipWidget != null)
        {
            tooltipWidget.Hide();
        }
    }

    private void Update()
    {
        if (panelRoot == null || !panelRoot.activeSelf) return;

        HandleScrollInput();
        UpdateCarouselAnimation();
    }

    private void HandleScrollInput()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (scroll > 0f)
        {
            // 위로 스크롤 (이전 카테고리)
            currentCategoryIndex = Mathf.Max(0, currentCategoryIndex - 1);
        }
        else if (scroll < 0f)
        {
            // 아래로 스크롤 (다음 카테고리)
            currentCategoryIndex = Mathf.Min(2, currentCategoryIndex + 1);
        }
    }

    private void UpdateCarouselAnimation()
    {
        if (categories == null || categoryRects == null) return;

        for (int i = 0; i < 3; i++)
        {
            if (categoryRects[i] == null) continue;

            // 목표 상태 계산
            float targetY = (i - currentCategoryIndex) * -verticalSpacing; 
            // i < currentIndex 면 양수(위로), i > currentIndex 면 음수(아래로) 배치

            float targetScale = (i == currentCategoryIndex) ? 1f : inactiveScale;
            float targetAlpha = (i == currentCategoryIndex) ? 1f : inactiveAlpha;

            // 서서히 이동, 축소/확대, 투명도 조절
            categoryRects[i].anchoredPosition = Vector2.Lerp(
                categoryRects[i].anchoredPosition, 
                new Vector2(categoryRects[i].anchoredPosition.x, targetY), 
                Time.deltaTime * lerpSpeed);

            categoryRects[i].localScale = Vector3.Lerp(
                categoryRects[i].localScale, 
                new Vector3(targetScale, targetScale, 1f), 
                Time.deltaTime * lerpSpeed);

            if (categoryCanvasGroups[i] != null)
            {
                categoryCanvasGroups[i].alpha = Mathf.Lerp(
                    categoryCanvasGroups[i].alpha, 
                    targetAlpha, 
                    Time.deltaTime * lerpSpeed);
                
                // 선택되지 않은 패널은 클릭 방지
                categoryCanvasGroups[i].interactable = (i == currentCategoryIndex);
                categoryCanvasGroups[i].blocksRaycasts = (i == currentCategoryIndex);
            }

            // 하이어라키 순서 조정 (중앙 패널이 항상 맨 앞으로 오도록)
            if (i == currentCategoryIndex)
            {
                categoryRects[i].SetAsLastSibling();
            }
        }
    }
}
