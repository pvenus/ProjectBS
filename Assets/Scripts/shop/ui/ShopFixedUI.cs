using System.Collections.Generic;
using System.Linq;
using Currency;
using TMPro;
using UnityEngine;

namespace Shop
{
    public enum ShopPurchaseClickMode
    {
        DirectPurchase = 0,
        ConfirmationPopup = 100,
    }

    /// <summary>
    /// Coordinates shop data and dynamically created product views.
    /// </summary>
    [AutoBindPrefix("Shop")]
    public class ShopFixedUI : UIComponent
    {
        public const int ProductCountPerCategory = 3;

        [Header("Manager")]
        [SerializeField] private StageShopManager shopManager;

        [Header("Purchase Interaction")]
        [SerializeField] private ShopPurchaseClickMode purchaseClickMode =
            ShopPurchaseClickMode.DirectPurchase;
        [SerializeField] private ShopPurchaseConfirmationPopupUI purchaseConfirmationPopup;

        [Header("Currency")]
        [AutoBind] [SerializeField] private TMP_Text goldText;

        [Header("Dynamic Product Roots")]
        [AutoBind] [SerializeField] private RectTransform strategicProductsRoot;
        [AutoBind] [SerializeField] private RectTransform relicProductsRoot;

        [Header("Dynamic Product Prefabs")]
        [SerializeField] private ShopStrategicSkillProductUI strategicProductPrefab;
        [SerializeField] private ShopRelicProductUI relicProductPrefab;

        private readonly List<ShopStrategicSkillProductUI> strategicProductViews = new();
        private readonly List<ShopRelicProductUI> relicProductViews = new();
        private CurrencyManager subscribedCurrencyManager;

        public IReadOnlyList<ShopStrategicSkillProductUI> StrategicProductViews =>
            strategicProductViews;

        public IReadOnlyList<ShopRelicProductUI> RelicProductViews =>
            relicProductViews;

        private void Awake()
        {
            ResolveManager();
            ValidateDynamicProductSetup();
        }

        private void OnEnable()
        {
            ResolveManager();
            Subscribe();

            if (shopManager != null
                && shopManager.CurrentShop != null
                && shopManager.CurrentShop.isOpened)
            {
                Show(shopManager.CurrentShop);
            }
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void OnDestroy()
        {
            Unsubscribe();
            ClearProductViews();
        }

        public void Show(ShopRuntimeData shop)
        {
            if (shop == null)
            {
                Hide();
                return;
            }

            SetShopVisible(true);
            PresentShopHeader(shop);

            int currentGold = GetCurrentGold();
            RefreshGold(currentGold);

            List<ShopRuntimeItem> items = shop.groups == null
                ? new List<ShopRuntimeItem>()
                : shop.groups
                    .Where(group => group?.items != null)
                    .SelectMany(group => group.items)
                    .Where(item => item != null)
                    .OrderBy(item => item.slotIndex)
                    .ToList();

            RebuildProductViews(items, currentGold);
        }

        public void Refresh()
        {
            if (shopManager == null
                || shopManager.CurrentShop == null
                || !shopManager.CurrentShop.isOpened)
            {
                Hide();
                return;
            }

            Show(shopManager.CurrentShop);
        }

        public void Hide()
        {
            ClearProductViews();
            SetShopVisible(false);
        }

        private void RebuildProductViews(
            IReadOnlyList<ShopRuntimeItem> items,
            int currentGold)
        {
            ClearProductViews();

            List<ShopRuntimeItem> strategicItems = items
                .Where(item => item.ProductType == ShopProductType.StrategicSkillItem)
                .Take(ProductCountPerCategory)
                .ToList();

            List<ShopRuntimeItem> relicItems = items
                .Where(item => item.ProductType == ShopProductType.Relic)
                .Take(ProductCountPerCategory)
                .ToList();

            BuildProductViews(
                strategicProductPrefab,
                strategicProductsRoot,
                strategicItems,
                strategicProductViews,
                currentGold);

            BuildProductViews(
                relicProductPrefab,
                relicProductsRoot,
                relicItems,
                relicProductViews,
                currentGold);
        }

        private void BuildProductViews<T>(
            T productPrefab,
            RectTransform productRoot,
            IReadOnlyList<ShopRuntimeItem> items,
            List<T> createdViews,
            int currentGold)
            where T : ShopProductUIBase
        {
            if (productPrefab == null || productRoot == null)
            {
                Debug.LogWarning(
                    $"[{nameof(ShopFixedUI)}] Cannot build {typeof(T).Name} views because " +
                    "the prefab or product root is missing.",
                    this);
                return;
            }

            foreach (ShopRuntimeItem item in items)
            {
                if (item == null)
                {
                    continue;
                }

                T productView = Instantiate(productPrefab, productRoot);
                if (!productView.Bind(item, currentGold))
                {
                    Destroy(productView.gameObject);
                    continue;
                }

                productView.PurchaseRequested += HandlePurchaseRequested;
                createdViews.Add(productView);
            }
        }

        private void ClearProductViews()
        {
            ClearProductViews(strategicProductViews);
            ClearProductViews(relicProductViews);
        }

        private void ClearProductViews<T>(List<T> productViews)
            where T : ShopProductUIBase
        {
            foreach (T productView in productViews)
            {
                if (productView == null)
                {
                    continue;
                }

                productView.PurchaseRequested -= HandlePurchaseRequested;
                productView.Clear();
                Destroy(productView.gameObject);
            }

            productViews.Clear();
        }

        private void RefreshProductViews(int currentGold)
        {
            RefreshProductViews(strategicProductViews, currentGold);
            RefreshProductViews(relicProductViews, currentGold);
        }

        private static void RefreshProductViews<T>(
            IEnumerable<T> productViews,
            int currentGold)
            where T : ShopProductUIBase
        {
            foreach (T productView in productViews)
            {
                if (productView != null)
                {
                    productView.Refresh(currentGold);
                }
            }
        }

        private void HandlePurchaseRequested(ShopProductUIBase productUI)
        {
            if (shopManager == null || productUI?.RuntimeItem == null)
            {
                Debug.LogWarning(
                    "[ShopFixedUI] Purchase request ignored because the manager or product is missing.",
                    this);
                return;
            }

            if (purchaseClickMode == ShopPurchaseClickMode.DirectPurchase)
            {
                TryPurchase(productUI.RuntimeItem);
                return;
            }

            if (purchaseConfirmationPopup == null)
            {
                Debug.Log(
                    $"[PLACEHOLDER] {nameof(ShopFixedUI)}.{nameof(HandlePurchaseRequested)} called. " +
                    "Purchase confirmation popup is not connected yet.",
                    this);
                return;
            }

            purchaseConfirmationPopup.Show(
                productUI.RuntimeItem,
                TryPurchase);
        }

        private void TryPurchase(ShopRuntimeItem item)
        {
            if (shopManager == null || item == null)
            {
                return;
            }

            shopManager.TryPurchase(item.runtimeId);
        }

        private void ResolveManager()
        {
            if (shopManager != null)
            {
                return;
            }

            shopManager = StageShopManager.Instance;
            if (shopManager == null)
            {
                shopManager = FindFirstObjectByType<StageShopManager>();
            }
        }

        private void Subscribe()
        {
            if (shopManager == null)
            {
                return;
            }

            Unsubscribe();
            shopManager.OnShopOpened += HandleShopOpened;
            shopManager.OnShopClosed += HandleShopClosed;
            shopManager.OnShopRefreshed += HandleShopRefreshed;
            shopManager.OnItemPurchased += HandleItemPurchased;

            subscribedCurrencyManager = CurrencyManager.Instance;
            if (subscribedCurrencyManager != null)
            {
                subscribedCurrencyManager.OnGoldChanged += HandleGoldChanged;
            }
        }

        private void Unsubscribe()
        {
            if (shopManager != null)
            {
                shopManager.OnShopOpened -= HandleShopOpened;
                shopManager.OnShopClosed -= HandleShopClosed;
                shopManager.OnShopRefreshed -= HandleShopRefreshed;
                shopManager.OnItemPurchased -= HandleItemPurchased;
            }

            if (subscribedCurrencyManager != null)
            {
                subscribedCurrencyManager.OnGoldChanged -= HandleGoldChanged;
                subscribedCurrencyManager = null;
            }
        }

        private void HandleShopOpened(ShopRuntimeData shop) => Show(shop);
        private void HandleShopClosed(ShopRuntimeData shop) => Hide();

        private void HandleShopRefreshed(ShopRuntimeData shop)
        {
            if (shop == null || !shop.isOpened)
            {
                Hide();
                return;
            }

            Show(shop);
        }

        private void HandleGoldChanged(int gold)
        {
            RefreshGold(gold);
            RefreshProductViews(gold);
        }

        private void HandleItemPurchased(ShopRuntimeItem item)
        {
            RefreshProductViews(GetCurrentGold());
        }

        private void PresentShopHeader(ShopRuntimeData shop)
        {
            Debug.Log(
                $"[PLACEHOLDER] {nameof(ShopFixedUI)}.{nameof(PresentShopHeader)} called. " +
                $"shopName={shop.shopName}; header UI is not present in Shop_Fixed yet.",
                this);
        }

        private void RefreshGold(int gold)
        {
            if (goldText != null)
            {
                goldText.text = gold.ToString();
            }
        }

        private void SetShopVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        private static int GetCurrentGold()
        {
            return CurrencyManager.Instance != null
                ? CurrencyManager.Instance.Gold
                : 0;
        }

        private void ValidateDynamicProductSetup()
        {
            if (strategicProductsRoot == null
                || relicProductsRoot == null
                || strategicProductPrefab == null
                || relicProductPrefab == null)
            {
                Debug.LogWarning(
                    "[ShopFixedUI] Dynamic product roots or prefab references are missing.",
                    this);
            }
        }
    }
}
