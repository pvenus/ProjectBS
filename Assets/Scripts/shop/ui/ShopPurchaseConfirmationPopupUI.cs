using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shop
{
    /// <summary>
    /// 상품 구매 전 확인을 받는 선택형 팝업.
    /// ShopFixedUI의 구매 방식을 ConfirmationPopup으로 설정했을 때 사용한다.
    /// </summary>
    [AutoBindPrefix("Confirm")]
    public class ShopPurchaseConfirmationPopupUI : UIComponent
    {
        [Header("Root")]
        [AutoBind] [SerializeField] private RectTransform panelRoot;

        [Header("Product")]
        [AutoBind] [SerializeField] private Image iconImage;
        [AutoBind] [SerializeField] private TMP_Text nameText;
        [AutoBind] [SerializeField] private TMP_Text descriptionText;

        [Header("Gold Price")]
        [AutoBind] [SerializeField] private Image goldIconImage;
        [AutoBind] [SerializeField] private TMP_Text priceText;

        [Header("Action")]
        [AutoBind] [SerializeField] private Button confirmButton;
        [AutoBind] [SerializeField] private Button cancelButton;

        private ShopRuntimeItem runtimeItem;
        private Action<ShopRuntimeItem> confirmAction;

        private void Awake()
        {
            RegisterButtons();
            Hide();
        }

        private void OnDestroy()
        {
            UnregisterButtons();
        }

        public void Show(
            ShopRuntimeItem item,
            Action<ShopRuntimeItem> onConfirmed)
        {
            if (item == null)
            {
                Hide();
                return;
            }

            runtimeItem = item;
            confirmAction = onConfirmed;

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

            if (priceText != null)
            {
                priceText.text = item.price.ToString();
            }

            if (goldIconImage != null)
            {
                goldIconImage.enabled = goldIconImage.sprite != null;
            }

            SetVisible(true);
        }

        public void Hide()
        {
            runtimeItem = null;
            confirmAction = null;
            SetVisible(false);
        }

        private void RegisterButtons()
        {
            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveListener(HandleConfirmClicked);
                confirmButton.onClick.AddListener(HandleConfirmClicked);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.RemoveListener(Hide);
                cancelButton.onClick.AddListener(Hide);
            }
        }

        private void UnregisterButtons()
        {
            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveListener(HandleConfirmClicked);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.RemoveListener(Hide);
            }
        }

        private void HandleConfirmClicked()
        {
            ShopRuntimeItem item = runtimeItem;
            Action<ShopRuntimeItem> action = confirmAction;
            Hide();
            action?.Invoke(item);
        }

        private void SetVisible(bool visible)
        {
            if (panelRoot != null && panelRoot.gameObject != gameObject)
            {
                panelRoot.gameObject.SetActive(visible);
            }
        }
    }
}
