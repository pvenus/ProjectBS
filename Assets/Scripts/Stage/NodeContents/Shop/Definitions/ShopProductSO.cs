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

        public Sprite Icon
        {
            get
            {
                if (rewardData?.Relic != null)
                {
                    return rewardData.Relic.icon;
                }

                if (rewardData?.StrategicSkillItem != null)
                {
                    return rewardData.StrategicSkillItem.icon;
                }

                return null;
            }
        }

        public string DisplayName
        {
            get
            {
                if (rewardData?.Relic != null)
                {
                    return rewardData.Relic.DisplayName;
                }

                if (rewardData?.StrategicSkillItem != null)
                {
                    return rewardData.StrategicSkillItem.DisplayName;
                }

                return productId;
            }
        }

        public string Description
        {
            get
            {
                if (rewardData?.Relic != null)
                {
                    return rewardData.Relic.Description;
                }

                if (rewardData?.StrategicSkillItem != null)
                {
                    return rewardData.StrategicSkillItem.Description;
                }

                return string.Empty;
            }
        }
    }

    [Serializable]
    public class ShopRewardData
    {
        public ShopRewardType rewardType;

        [Header("Reward")]
        public ScriptableObject reward;

        public RelicSO Relic => reward as RelicSO;

        public StrategicSkillItemSO StrategicSkillItem =>
            reward as StrategicSkillItemSO;
    }

    public enum ShopProductType
    {
        None = 0,

        Relic = 100,
        StrategicSkillItem = 200,
        AIFunction = 300,
    }

    public enum ShopRewardType
    {
        None = 0,

        Relic = 100,
        StrategicSkillItem = 200,
        AIFunction = 300,
    }
}
