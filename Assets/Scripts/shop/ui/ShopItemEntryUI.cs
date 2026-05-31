

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Currency;

namespace Shop
{
    /// <summary>
    /// 상점 스크롤 목록에 표시되는 개별 상품 UI.
    /// ShopRuntimeItem을 바인딩하고 구매 버튼 클릭을 StageShopManager로 전달한다.
    /// </summary>
    public class ShopItemEntryUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private StageShopManager shopManager;

        [Header("Root")]
        [SerializeField] private GameObject soldOutRoot;
        [SerializeField] private GameObject lockedRoot;

        [Header("Display")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private TMP_Text typeText;
        [SerializeField] private TMP_Text effectText;

        [Header("Action")]
        [SerializeField] private Button buyButton;
        [SerializeField] private TMP_Text buyButtonText;

        [Header("Options")]
        [SerializeField] private string buyLabel = "Buy";
        [SerializeField] private string soldOutLabel = "Sold Out";
        [SerializeField] private string lockedLabel = "Locked";
        [SerializeField] private string cannotAffordLabel = "Need Gold";

        private ShopRuntimeItem runtimeItem;

        public ShopRuntimeItem RuntimeItem => runtimeItem;

        private void Awake()
        {
            if (shopManager == null)
            {
                shopManager = StageShopManager.Instance;
            }
        }

        private void OnEnable()
        {
            if (shopManager == null)
            {
                shopManager = StageShopManager.Instance;
            }
        }

        private void OnDestroy()
        {
            if (buyButton != null)
            {
                buyButton.onClick.RemoveListener(HandleBuyClicked);
            }
        }

        public void Bind(ShopRuntimeItem item, StageShopManager manager = null)
        {
            runtimeItem = item;

            if (manager != null)
            {
                shopManager = manager;
            }
            else if (shopManager == null)
            {
                shopManager = StageShopManager.Instance;
            }

            if (buyButton != null)
            {
                buyButton.onClick.RemoveListener(HandleBuyClicked);
                buyButton.onClick.AddListener(HandleBuyClicked);
            }

            Refresh();
        }

        public void Refresh()
        {
            if (runtimeItem == null)
            {
                SetEmpty();
                return;
            }

            if (iconImage != null)
            {
                iconImage.sprite = runtimeItem.Icon;
                iconImage.enabled = runtimeItem.Icon != null;
            }

            if (nameText != null)
            {
                nameText.text = runtimeItem.DisplayName;
            }

            if (descriptionText != null)
            {
                descriptionText.text = runtimeItem.Description;
            }

            if (priceText != null)
            {
                priceText.text = runtimeItem.price.ToString();
            }

            if (typeText != null)
            {
                typeText.text = runtimeItem.ProductType.ToString();
            }

            if (effectText != null)
            {
                effectText.text = runtimeItem.Description;
            }

            RefreshState();
        }

        private void RefreshState()
        {
            bool isSoldOut = runtimeItem != null && runtimeItem.IsSoldOut;
            bool isLocked = runtimeItem != null && runtimeItem.IsLocked;
            bool canAfford = runtimeItem == null || CurrencyManager.Instance.Gold >= runtimeItem.price;
            bool canBuy = runtimeItem != null && runtimeItem.IsAvailable && canAfford;

            if (soldOutRoot != null)
            {
                soldOutRoot.SetActive(isSoldOut);
            }

            if (lockedRoot != null)
            {
                lockedRoot.SetActive(isLocked);
            }

            if (buyButton != null)
            {
                buyButton.interactable = canBuy;
            }

            if (buyButtonText != null)
            {
                if (isSoldOut)
                {
                    buyButtonText.text = soldOutLabel;
                }
                else if (isLocked)
                {
                    buyButtonText.text = lockedLabel;
                }
                else if (!canAfford)
                {
                    buyButtonText.text = cannotAffordLabel;
                }
                else
                {
                    buyButtonText.text = buyLabel;
                }
            }
        }

        private void SetEmpty()
        {
            if (iconImage != null)
            {
                iconImage.sprite = null;
                iconImage.enabled = false;
            }

            if (nameText != null)
            {
                nameText.text = "Empty";
            }

            if (descriptionText != null)
            {
                descriptionText.text = string.Empty;
            }

            if (priceText != null)
            {
                priceText.text = "-";
            }

            if (typeText != null)
            {
                typeText.text = string.Empty;
            }

            if (effectText != null)
            {
                effectText.text = string.Empty;
            }

            if (soldOutRoot != null)
            {
                soldOutRoot.SetActive(false);
            }

            if (lockedRoot != null)
            {
                lockedRoot.SetActive(false);
            }

            if (buyButton != null)
            {
                buyButton.interactable = false;
            }

            if (buyButtonText != null)
            {
                buyButtonText.text = buyLabel;
            }
        }

        private void HandleBuyClicked()
        {
            if (runtimeItem == null)
            {
                return;
            }

            if (shopManager == null)
            {
                Debug.LogWarning("[ShopItemEntryUI] Buy failed. StageShopManager is null.");
                return;
            }

            shopManager.TryPurchase(runtimeItem.runtimeId);
        }
    }
}