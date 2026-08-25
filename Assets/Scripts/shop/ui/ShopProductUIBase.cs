using System;
using UnityEngine.EventSystems;

namespace Shop
{
    public enum ShopProductPurchaseState
    {
        Unavailable = 0,
        Available = 100,
        Completed = 200,
    }

    /// <summary>
    /// 상점 상품 프리팹의 공통 표시와 구매 요청을 담당한다.
    /// 실제 구매는 ShopFixedUI가 StageShopManager에 전달한다.
    /// </summary>
    public abstract class ShopProductUIBase : UIComponent, IPointerClickHandler
    {
        private ShopRuntimeItem runtimeItem;
        private bool canAfford;

        public ShopRuntimeItem RuntimeItem => runtimeItem;
        public bool HasItem => runtimeItem != null;
        public ShopProductPurchaseState PurchaseState { get; private set; } =
            ShopProductPurchaseState.Unavailable;
        public bool IsPurchaseAvailable =>
            PurchaseState == ShopProductPurchaseState.Available;
        public bool IsPurchaseCompleted =>
            PurchaseState == ShopProductPurchaseState.Completed;
        public bool CanAfford => canAfford;
        public bool IsClickable => IsPurchaseAvailable && CanAfford;

        public event Action<ShopProductUIBase> PurchaseRequested;

        protected abstract ShopProductType ProductType { get; }
        protected abstract void SetProductVisible(bool visible);
        protected abstract void PresentProduct(ShopRuntimeItem item);
        protected abstract void PresentPurchaseState(
            ShopProductPurchaseState state,
            bool canAfford);

        public bool Bind(ShopRuntimeItem item, int currentGold)
        {
            if (item == null || item.ProductType != ProductType)
            {
                Clear();
                return false;
            }

            runtimeItem = item;
            SetProductVisible(true);
            PresentProduct(item);
            RefreshState(currentGold);
            return true;
        }

        public void Refresh(int currentGold)
        {
            if (runtimeItem == null)
            {
                return;
            }

            PresentProduct(runtimeItem);
            RefreshState(currentGold);
        }

        public void Clear()
        {
            runtimeItem = null;
            canAfford = false;
            PurchaseState = ShopProductPurchaseState.Unavailable;
            PresentPurchaseState(PurchaseState, canAfford);
            SetProductVisible(false);
        }

        private void RefreshState(int currentGold)
        {
            bool isSoldOut = runtimeItem.IsSoldOut;
            canAfford = currentGold >= runtimeItem.price;
            PurchaseState = isSoldOut
                ? ShopProductPurchaseState.Completed
                : runtimeItem.IsAvailable
                    ? ShopProductPurchaseState.Available
                    : ShopProductPurchaseState.Unavailable;

            PresentPurchaseState(PurchaseState, canAfford);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left
                || runtimeItem == null
                || !IsClickable)
            {
                return;
            }

            PurchaseRequested?.Invoke(this);
        }
    }
}
