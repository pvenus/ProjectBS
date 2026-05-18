using System;
using UnityEngine;

namespace Shop
{
    /// <summary>
    /// 실제 상점에 생성된 판매 상품 데이터.
    /// ShopItemSO는 원본 정의이고, ShopRuntimeItem은 이번 상점에서의 가격/상태/구매 여부를 가진다.
    /// </summary>
    [Serializable]
    public class ShopRuntimeItem
    {
        [Header("Identity")]
        public string runtimeId;
        public int slotIndex;

        [Header("Source")]
        public ShopProductSO product;

        [Header("Runtime Shop Data")]
        [Min(0)]
        public int price;
        public ShopItemState state = ShopItemState.Available;
        public bool isPurchased;

        [Header("Debug")]
        public string generatedFromPoolId;

        public string ProductId => product != null ? product.productId : string.Empty;
        public string DisplayName => product != null ? product.displayName : "Empty";
        public string Description => product != null ? product.description : string.Empty;
        public Sprite Icon => product != null ? product.icon : null;
        public ShopProductType ProductType => product != null ? product.productType : ShopProductType.None;

        public bool IsAvailable => state == ShopItemState.Available && !isPurchased && product != null;
        public bool IsSoldOut => state == ShopItemState.SoldOut || isPurchased;
        public bool IsLocked => state == ShopItemState.Locked;

        public ShopRuntimeItem()
        {
        }

        public ShopRuntimeItem(ShopProductSO product, int price, int slotIndex, string generatedFromPoolId = null)
        {
            this.product = product;
            this.price = Mathf.Max(0, price);
            this.slotIndex = slotIndex;
            this.generatedFromPoolId = generatedFromPoolId;
            runtimeId = CreateRuntimeId(product, slotIndex);
            state = product == null ? ShopItemState.None : ShopItemState.Available;
            isPurchased = false;
        }

        public bool CanPurchase(int currentGold)
        {
            return IsAvailable && currentGold >= price;
        }

        public bool TryPurchase(int currentGold)
        {
            if (!CanPurchase(currentGold))
            {
                return false;
            }

            MarkPurchased();
            return true;
        }

        public void MarkPurchased()
        {
            isPurchased = true;
            state = ShopItemState.SoldOut;
        }

        public void SetLocked()
        {
            if (IsSoldOut)
            {
                return;
            }

            state = ShopItemState.Locked;
        }

        public void SetAvailable()
        {
            if (IsSoldOut || product == null)
            {
                return;
            }

            state = ShopItemState.Available;
        }

        public void SetPrice(int newPrice)
        {
            price = Mathf.Max(0, newPrice);
        }

        private static string CreateRuntimeId(ShopProductSO product, int slotIndex)
        {
            string itemId = product != null && !string.IsNullOrWhiteSpace(product.productId)
                ? product.productId
                : "empty";

            return $"shop_item_{slotIndex}_{itemId}_{Guid.NewGuid().ToString("N")[..8]}";
        }
    }
}