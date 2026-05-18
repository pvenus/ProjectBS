using System;
using UnityEngine;
using Item;

namespace Shop
{
    [CreateAssetMenu(
        fileName = "ShopProductSO",
        menuName = "Game/Shop/Shop Product")]
    public class ShopProductSO : ScriptableObject
    {
        [Header("Identity")]
        public string productId;

        public string displayName;

        [TextArea]
        public string description;

        public Sprite icon;

        [Header("Shop")]
        public ShopProductType productType;

        [Min(0)]
        public int price = 10;

        [Min(0)]
        public int weight = 1;

        public bool uniqueProduct;

        [Header("Tags")]
        public string[] tags;

        [Header("Reward")]
        public ShopRewardData rewardData;
    }

    [Serializable]
    public class ShopRewardData
    {
        public ShopRewardType rewardType;

        [Header("Relic")]
        public RelicSO relic;

        [Header("Consumable")]
        public ScriptableObject consumable;

        [Header("AI")]
        public ScriptableObject aiFunction;
    }

    public enum ShopProductType
    {
        None = 0,

        Relic = 100,
        Consumable = 200,
        AIFunction = 300,
    }

    public enum ShopRewardType
    {
        None = 0,

        Relic = 100,
        Consumable = 200,
        AIFunction = 300,
    }
}
