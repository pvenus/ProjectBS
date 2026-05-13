

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shop
{
    /// <summary>
    /// 로그라이크 상점 생성에 사용하는 아이템 풀.
    /// 실제 상점 생성 시 이 풀에서 조건/가중치 기반으로 아이템을 선택한다.
    /// </summary>
    [CreateAssetMenu(menuName = "Shop/Shop Item Pool")]
    public class ShopItemPoolSO : ScriptableObject
    {
        [Header("Identity")]
        public string poolId;
        public string poolName;

        [TextArea]
        public string description;

        [Header("Pool")]
        public List<ShopItemEntry> entries = new();

        [Header("Generation")]
        [Tooltip("중복 허용 여부")]
        public bool allowDuplicate = false;

        [Tooltip("비활성 아이템 포함 여부")]
        public bool includeDisabledEntries = false;

        public bool HasEntries => entries != null && entries.Count > 0;

        public List<ShopItemEntry> GetAvailableEntries()
        {
            List<ShopItemEntry> result = new();

            if (entries == null)
            {
                return result;
            }

            foreach (ShopItemEntry entry in entries)
            {
                if (entry == null)
                {
                    continue;
                }

                if (!includeDisabledEntries && !entry.enabled)
                {
                    continue;
                }

                if (entry.item == null)
                {
                    continue;
                }

                result.Add(entry);
            }

            return result;
        }
    }

    [Serializable]
    public class ShopItemEntry
    {
        [Header("Item")]
        public ShopItemSO item;

        [Header("Weight")]
        [Min(0)]
        public int weight = 100;

        [Header("Price")]
        public bool overridePrice;

        [Min(0)]
        public int price;

        [Header("Generation")]
        public bool enabled = true;

        [Tooltip("특정 상점 타입 전용 여부")]
        public ShopType shopType = ShopType.Normal;

        [Tooltip("추가 태그 필터")]
        public List<string> tags = new();

        public int GetPrice()
        {
            if (overridePrice)
            {
                return Mathf.Max(0, price);
            }

            if (item == null)
            {
                return 0;
            }

            return Mathf.Max(0, item.basePrice);
        }
    }
}