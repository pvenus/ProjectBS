using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shop
{
    /// <summary>
    /// 상점 팝업 UI.
    /// StageShopManager 이벤트를 구독해서 상점 열기/닫기/갱신을 화면에 반영한다.
    /// ScrollView Content 아래에 ShopItemEntryUI 프리팹을 생성한다.
    /// </summary>
    public class ShopPopupUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private StageShopManager shopManager;

        [Header("Root")]
        [SerializeField] private GameObject panelRoot;

        [Header("Header")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text goldText;
        [SerializeField] private TMP_Text shopTypeText;

        [Header("List")]
        [SerializeField] private RectTransform contentRoot;
        [SerializeField] private ShopItemEntryUI itemEntryPrefab;

        [Header("Layout")]
        [SerializeField] private bool forcePositionOffset = true;

        [SerializeField] private Vector2 itemOffset = new Vector2(0f, -180f);

        [Header("Buttons")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button refreshButton;

        [Header("Options")]
        [SerializeField] private bool hideOnAwake = true;
        [SerializeField] private bool rebuildOnRefresh = true;

        private readonly List<ShopItemEntryUI> spawnedEntries = new();

        private void Awake()
        {
            if (shopManager == null)
            {
                shopManager = StageShopManager.Instance;
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(HandleCloseClicked);
                closeButton.onClick.AddListener(HandleCloseClicked);
            }

            if (refreshButton != null)
            {
                refreshButton.onClick.RemoveListener(HandleRefreshClicked);
                refreshButton.onClick.AddListener(HandleRefreshClicked);
            }

            if (hideOnAwake)
            {
                Hide();
            }
        }

        private void OnEnable()
        {
            if (shopManager == null)
            {
                shopManager = StageShopManager.Instance;
            }

            Subscribe();

            if (shopManager != null && shopManager.CurrentShop != null && shopManager.CurrentShop.isOpened)
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
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(HandleCloseClicked);
            }

            if (refreshButton != null)
            {
                refreshButton.onClick.RemoveListener(HandleRefreshClicked);
            }
        }

        public void Show(ShopRuntimeData shop)
        {
            if (shop == null)
            {
                Hide();
                return;
            }

            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }
            else
            {
                Debug.LogWarning("[ShopPopupUI] panelRoot is not assigned. UI controller object will stay active.");
            }

            RefreshHeader(shop);
            BuildItems(shop);
        }

        public void Hide()
        {
            ClearItems();

            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
                return;
            }

            Debug.LogWarning("[ShopPopupUI] panelRoot is not assigned. ShopPopupUI object will stay active so event subscription can work.");
        }

        public void Refresh()
        {
            if (shopManager == null || shopManager.CurrentShop == null)
            {
                Hide();
                return;
            }

            RefreshHeader(shopManager.CurrentShop);

            if (rebuildOnRefresh)
            {
                BuildItems(shopManager.CurrentShop);
            }
            else
            {
                RefreshEntries();
            }
        }

        private void RefreshHeader(ShopRuntimeData shop)
        {
            if (shop == null)
            {
                return;
            }

            if (titleText != null)
            {
                titleText.text = string.IsNullOrWhiteSpace(shop.shopName) ? "Shop" : shop.shopName;
            }

            if (goldText != null)
            {
                int gold = shopManager != null ? shopManager.CurrentGold : 0;
                goldText.text = gold.ToString();
            }

            if (shopTypeText != null)
            {
                shopTypeText.text = shop.shopType.ToString();
            }
        }

        private void BuildItems(ShopRuntimeData shop)
        {
            ClearItems();

            if (shop == null || shop.items == null)
            {
                return;
            }

            if (contentRoot == null)
            {
                Debug.LogWarning("[ShopPopupUI] contentRoot is not assigned.");
                return;
            }

            if (itemEntryPrefab == null)
            {
                Debug.LogWarning("[ShopPopupUI] itemEntryPrefab is not assigned.");
                return;
            }

            int i = 0;
            foreach (ShopRuntimeItem item in shop.items)
            {
                if (item == null)
                {
                    continue;
                }

                ShopItemEntryUI entry = Instantiate(itemEntryPrefab, contentRoot);

                if (forcePositionOffset)
                {
                    RectTransform rectTransform =
                        entry.GetComponent<RectTransform>();

                    if (rectTransform != null)
                    {
                        rectTransform.anchoredPosition =
                            new Vector2(
                                itemOffset.x * i,
                                itemOffset.y * i);
                    }
                }

                entry.Bind(item, shopManager);
                spawnedEntries.Add(entry);
                i++;
            }
        }

        private void RefreshEntries()
        {
            foreach (ShopItemEntryUI entry in spawnedEntries)
            {
                if (entry != null)
                {
                    entry.Refresh();
                }
            }
        }

        private void ClearItems()
        {
            foreach (ShopItemEntryUI entry in spawnedEntries)
            {
                if (entry != null)
                {
                    Destroy(entry.gameObject);
                }
            }

            spawnedEntries.Clear();
        }

        private void Subscribe()
        {
            if (shopManager == null)
            {
                return;
            }

            shopManager.OnShopOpened -= HandleShopOpened;
            shopManager.OnShopClosed -= HandleShopClosed;
            shopManager.OnShopRefreshed -= HandleShopRefreshed;
            shopManager.OnGoldChanged -= HandleGoldChanged;
            shopManager.OnItemPurchased -= HandleItemPurchased;

            shopManager.OnShopOpened += HandleShopOpened;
            shopManager.OnShopClosed += HandleShopClosed;
            shopManager.OnShopRefreshed += HandleShopRefreshed;
            shopManager.OnGoldChanged += HandleGoldChanged;
            shopManager.OnItemPurchased += HandleItemPurchased;
        }

        private void Unsubscribe()
        {
            if (shopManager == null)
            {
                return;
            }

            shopManager.OnShopOpened -= HandleShopOpened;
            shopManager.OnShopClosed -= HandleShopClosed;
            shopManager.OnShopRefreshed -= HandleShopRefreshed;
            shopManager.OnGoldChanged -= HandleGoldChanged;
            shopManager.OnItemPurchased -= HandleItemPurchased;
        }

        private void HandleShopOpened(ShopRuntimeData shop)
        {
            Show(shop);
        }

        private void HandleShopClosed(ShopRuntimeData shop)
        {
            Hide();
        }

        private void HandleShopRefreshed(ShopRuntimeData shop)
        {
            if (shop == null || !shop.isOpened)
            {
                Hide();
                return;
            }

            Refresh();
        }

        private void HandleGoldChanged(int gold)
        {
            if (goldText != null)
            {
                goldText.text = gold.ToString();
            }

            RefreshEntries();
        }

        private void HandleItemPurchased(ShopRuntimeItem item)
        {
            RefreshEntries();
        }

        private void HandleCloseClicked()
        {
            if (shopManager != null)
            {
                shopManager.CloseShop();
            }
            else
            {
                Hide();
            }
        }

        private void HandleRefreshClicked()
        {
            if (shopManager != null)
            {
                shopManager.RefreshDefaultShop();
            }
        }
    }
}