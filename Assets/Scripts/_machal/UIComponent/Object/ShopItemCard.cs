using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Shop;
using Currency;

[AutoBindPrefix("Item")]
public class ShopItemCard : UIComponent, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI Components")]
    [AutoBind] [SerializeField] private Image iconImage;
    [AutoBind] [SerializeField] private TMP_Text nameText;
    [AutoBind] [SerializeField] private TMP_Text descriptionText;
    [AutoBind] [SerializeField] private TMP_Text priceText;
    [AutoBind] [SerializeField] private TMP_Text typeText;
    [AutoBind] [SerializeField] private TMP_Text effectText;
    
    [Header("Action UI (Optional)")]
    [AutoBind] [SerializeField] private Button buyButton;
    [AutoBind] [SerializeField] private TMP_Text buyButtonText;
    
    [Header("State UI")]
    [AutoBind] [SerializeField] private GameObject soldOutRoot;
    [AutoBind] [SerializeField] private GameObject lockedRoot;

    [Header("Button Labels")]
    [SerializeField] private string buyLabel = "Buy";
    [SerializeField] private string soldOutLabel = "Sold Out";
    [SerializeField] private string lockedLabel = "Locked";
    [SerializeField] private string cannotAffordLabel = "Need Gold";

    public event Action<ShopItemCard> OnClicked;
    public event Action<ShopItemCard> OnHoverEnter;
    public event Action<ShopItemCard> OnHoverExit;

    private ShopRuntimeItem runtimeItem;
    public ShopRuntimeItem RuntimeItem => runtimeItem;

    private void Awake()
    {
        if (buyButton != null)
        {
            buyButton.onClick.RemoveListener(HandleBuyClicked);
            buyButton.onClick.AddListener(HandleBuyClicked);
        }
    }

    public void Bind(ShopRuntimeItem item)
    {
        runtimeItem = item;
        Refresh();
    }

    public void Refresh()
    {
        if (runtimeItem == null) return;
        
        if (nameText != null) nameText.text = runtimeItem.DisplayName;
        if (descriptionText != null) descriptionText.text = runtimeItem.Description;
        if (priceText != null) priceText.text = runtimeItem.price.ToString();
        if (typeText != null) typeText.text = runtimeItem.ProductType.ToString();
        if (effectText != null) effectText.text = runtimeItem.Description;
        
        if (iconImage != null)
        {
            iconImage.sprite = runtimeItem.Icon;
            iconImage.enabled = runtimeItem.Icon != null;
        }

        bool isSoldOut = runtimeItem.IsSoldOut;
        bool isLocked = runtimeItem.IsLocked;
        
        bool canAfford = CurrencyManager.Instance.Gold >= runtimeItem.price;
        bool canBuy = runtimeItem.IsAvailable && canAfford;

        if (soldOutRoot != null) soldOutRoot.SetActive(isSoldOut);
        if (lockedRoot != null) lockedRoot.SetActive(isLocked);

        if (buyButton != null) buyButton.interactable = canBuy;
        if (buyButtonText != null)
        {
            if (isSoldOut) buyButtonText.text = soldOutLabel;
            else if (isLocked) buyButtonText.text = lockedLabel;
            else if (!canAfford) buyButtonText.text = cannotAffordLabel;
            else buyButtonText.text = buyLabel;
        }
    }

    private void HandleBuyClicked()
    {
        OnClicked?.Invoke(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClicked?.Invoke(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnHoverEnter?.Invoke(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnHoverExit?.Invoke(this);
    }

    public string GetTooltipDescription() => runtimeItem != null ? runtimeItem.Description : string.Empty;
    public string GetItemId() => runtimeItem != null ? runtimeItem.runtimeId : string.Empty;
}
