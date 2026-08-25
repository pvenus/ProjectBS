using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shop
{
    /// <summary>
    /// 큰 유물 이미지, 이름, 설명을 표시하는 유물 상품 UI.
    /// 실제 UI 참조와 가격/클릭 처리는 ShopProductUIBase가 담당한다.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    [AutoBindPrefix("Item")]
    public class ShopRelicProductUI : ShopProductUIBase
    {
        [Header("Purchase State Visual")]
        [SerializeField, Range(0f, 1f)] private float completedAlpha = 0.2f;

        [Header("Display")]
        [AutoBind] [SerializeField] private Image iconImage;
        [AutoBind] [SerializeField] private Image iconShadowImage;
        [AutoBind] [SerializeField] private TMP_Text nameText;
        [AutoBind] [SerializeField] private TMP_Text descriptionText;

        [Header("Gold Price")]
        [AutoBind] [SerializeField] private Image goldIconImage;
        [AutoBind] [SerializeField] private TMP_Text priceText;

        private CanvasGroup stateCanvasGroup;
        private ShopProductHoverController hoverController;

        protected override ShopProductType ProductType =>
            ShopProductType.Relic;

        protected override void SetProductVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        protected override void PresentProduct(ShopRuntimeItem item)
        {
            Sprite icon = item.Icon;

            if (iconImage != null)
            {
                iconImage.sprite = icon;
                iconImage.enabled = icon != null;
            }

            if (iconShadowImage != null)
            {
                iconShadowImage.sprite = icon;
                iconShadowImage.enabled = icon != null;
            }

            if (descriptionText != null)
            {
                descriptionText.text = item.Description;
            }

            if (nameText != null)
            {
                nameText.text = item.DisplayName;
            }

            if (goldIconImage != null)
            {
                goldIconImage.enabled = goldIconImage.sprite != null;
            }

            if (priceText != null)
            {
                priceText.text = item.price.ToString();
            }
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
    }
}
