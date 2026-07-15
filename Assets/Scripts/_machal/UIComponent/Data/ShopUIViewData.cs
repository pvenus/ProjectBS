using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIFramework.Data
{
    [Serializable]
    public class ShopUIViewData
    {
        public string shopTitle;
        public string shopDescription;
        public int currentGold;
        public List<ShopItemViewData> items = new List<ShopItemViewData>();
    }

    [Serializable]
    public class ShopItemViewData
    {
        public string itemId;
        public string categoryId; // relic, consumable, tactic
        public string displayName;
        public Sprite icon;
        public string description;
        public int price;
        public bool soldOut;
        public bool affordable;
        public string disabledReason;
    }

    public enum ShopUIResultType
    {
        PurchaseRequested,
        Close
    }

    public class ShopUIResult
    {
        public ShopUIResultType type;
        public string itemId;
    }
}
