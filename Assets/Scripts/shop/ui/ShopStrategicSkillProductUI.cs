using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shop
{
    /// <summary>
    /// 부적 콘셉트의 전략스킬 상품 UI.
    /// ShopProductSO의 등급에 맞춰 부적 배경 스프라이트를 교체한다.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    [AutoBindPrefix("Item")]
    public class ShopStrategicSkillProductUI : ShopProductUIBase
    {
        [Header("Purchase State Visual")]
        [SerializeField, Range(0f, 1f)] private float completedAlpha = 0.2f;

        [Header("Display")]
        [AutoBind] [SerializeField] private Image iconImage;
        [AutoBind] [SerializeField] private TMP_Text nameText;
        [AutoBind] [SerializeField] private TMP_Text descriptionText;
        [AutoBind] [SerializeField] private Image rarityBackgroundImage;

        [Header("Gold Price")]
        [AutoBind] [SerializeField] private Image goldIconImage;
        [AutoBind] [SerializeField] private TMP_Text priceText;

        private CanvasGroup stateCanvasGroup;
        private ShopProductHoverController hoverController;

        protected override ShopProductType ProductType =>
            ShopProductType.StrategicSkillItem;

        protected override void SetProductVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        protected override void PresentProduct(ShopRuntimeItem item)
        {
            if (iconImage != null)
            {
                iconImage.sprite = item.Icon;
                iconImage.enabled = item.Icon != null;
            }

            if (nameText != null)
            {
                nameText.text = item.DisplayName;
            }

            if (descriptionText != null)
            {
                descriptionText.text = item.Description;
            }

            if (goldIconImage != null)
            {
                goldIconImage.enabled = goldIconImage.sprite != null;
            }

            if (priceText != null)
            {
                priceText.text = item.price.ToString();
            }

            PresentRarity(item);
        }

        protected override void PresentPurchaseState(
            ShopProductPurchaseState state,
            bool canAfford)
        {
            bool isCompleted = state == ShopProductPurchaseState.Completed;

            if (stateCanvasGroup == null)
            {
                stateCanvasGroup = GetComponent<CanvasGroup>();
            }

            if (stateCanvasGroup != null)
            {
                stateCanvasGroup.alpha = isCompleted
                    ? completedAlpha
                    : 1f;
                stateCanvasGroup.interactable = !isCompleted;
                stateCanvasGroup.blocksRaycasts = !isCompleted;
            }

            if (hoverController == null)
            {
                hoverController = GetComponent<ShopProductHoverController>();
            }

            if (hoverController != null)
            {
                hoverController.SetInteractionEnabled(!isCompleted);
            }
        }

        private void PresentRarity(ShopRuntimeItem item)
        {
            ShopItemRarity rarity = item.product != null
                ? item.product.Rarity
                : ShopItemRarity.Common;

            Debug.Log(
                $"[PLACEHOLDER] {nameof(ShopStrategicSkillProductUI)}.{nameof(PresentRarity)} " +
                $"called. rarity={rarity}, backgroundBound={rarityBackgroundImage != null}; " +
                "rarity visual mapping is pending.",
                this);
        }
    }
}
