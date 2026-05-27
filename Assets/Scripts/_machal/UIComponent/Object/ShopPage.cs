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

    [Header("Carousel Widget (Shop_CarouselWidget 등)")]
    [AutoBind] [SerializeField] private UICarouselWidget carouselWidget;

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

        // 캐러셀 위젯 초기화 (하드코딩하지 않고 하위의 모든 카테고리 패널을 자동 수집)
        if (carouselWidget != null)
        {
            ShopCategoryPanel[] allPanels = GetComponentsInChildren<ShopCategoryPanel>(true);
            
            // AutoBind 필드로 선언된 3개 외에 사용자가 추가한 카테고리도 모두 포함되도록 리스트화
            List<RectTransform> panelList = new List<RectTransform>();
            foreach (var panel in allPanels)
            {
                if (panel != null)
                {
                    panelList.Add(panel.GetComponent<RectTransform>());
                }
            }
            
            carouselWidget.Initialize(panelList.ToArray(), 1);
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
}
