

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Shop
{
    /// <summary>
    /// 현재 열린 상점의 런타임 데이터.
    /// 어떤 아이템이 생성되었는지, 구매 상태가 어떤지, 상점이 열려있는지 등을 관리한다.
    /// </summary>
    [Serializable]
    public class ShopRuntimeData
    {
        [Header("Identity")]
        public string shopId;
        public string shopName;
        public ShopType shopType = ShopType.Normal;

        [Header("Runtime")]
        public List<ShopRuntimeItem> items = new();
        public bool isOpened;
        public bool isClosed;

        [Header("Debug")]
        public string generatedFromPoolId;
        public int seed;

        public int ItemCount => items?.Count ?? 0;
        public bool HasItems => items != null && items.Count > 0;

        public ShopRuntimeData()
        {
        }

        public ShopRuntimeData(string shopId, string shopName, ShopType shopType = ShopType.Normal)
        {
            this.shopId = shopId;
            this.shopName = shopName;
            this.shopType = shopType;
        }

        public void Open()
        {
            isOpened = true;
            isClosed = false;
        }

        public void Close()
        {
            isOpened = false;
            isClosed = true;
        }

        public void Clear()
        {
            items.Clear();
            isOpened = false;
            isClosed = false;
        }

        public void AddItem(ShopRuntimeItem item)
        {
            if (item == null)
            {
                Debug.LogWarning("[ShopRuntimeData] Cannot add null item.");
                return;
            }

            if (string.IsNullOrWhiteSpace(item.runtimeId))
            {
                item.runtimeId = CreateFallbackRuntimeId(item);
            }

            items.Add(item);
        }

        public void AddItems(IEnumerable<ShopRuntimeItem> runtimeItems)
        {
            if (runtimeItems == null)
            {
                return;
            }

            foreach (ShopRuntimeItem item in runtimeItems)
            {
                AddItem(item);
            }
        }

        public ShopRuntimeItem GetItem(string runtimeId)
        {
            if (string.IsNullOrWhiteSpace(runtimeId) || items == null)
            {
                return null;
            }

            return items.FirstOrDefault(x => x != null && x.runtimeId == runtimeId);
        }

        public ShopRuntimeItem GetItemBySlot(int slotIndex)
        {
            if (items == null)
            {
                return null;
            }

            return items.FirstOrDefault(x => x != null && x.slotIndex == slotIndex);
        }

        public List<ShopRuntimeItem> GetAvailableItems()
        {
            if (items == null)
            {
                return new List<ShopRuntimeItem>();
            }

            return items.Where(x => x != null && x.IsAvailable).ToList();
        }

        public List<ShopRuntimeItem> GetSoldOutItems()
        {
            if (items == null)
            {
                return new List<ShopRuntimeItem>();
            }

            return items.Where(x => x != null && x.IsSoldOut).ToList();
        }

        public bool CanPurchase(string runtimeId, int currentGold)
        {
            ShopRuntimeItem item = GetItem(runtimeId);
            return item != null && item.CanPurchase(currentGold);
        }

        public bool TryPurchase(string runtimeId, int currentGold, out ShopRuntimeItem purchasedItem)
        {
            purchasedItem = null;

            ShopRuntimeItem item = GetItem(runtimeId);
            if (item == null)
            {
                return false;
            }

            if (!item.CanPurchase(currentGold))
            {
                return false;
            }

            item.MarkPurchased();
            purchasedItem = item;
            return true;
        }

        public bool HasPurchased(string runtimeId)
        {
            ShopRuntimeItem item = GetItem(runtimeId);
            return item != null && item.isPurchased;
        }

        public int GetTotalPriceOfAvailableItems()
        {
            int total = 0;

            foreach (ShopRuntimeItem item in GetAvailableItems())
            {
                total += item.price;
            }

            return total;
        }

        private string CreateFallbackRuntimeId(ShopRuntimeItem item)
        {
            string itemId = item.item != null && !string.IsNullOrWhiteSpace(item.item.itemId)
                ? item.item.itemId
                : "empty";

            return $"shop_runtime_{item.slotIndex}_{itemId}_{Guid.NewGuid().ToString("N")[..8]}";
        }
    }
}