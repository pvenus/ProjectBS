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
        public List<ShopProductSO> products = new();

        [Header("Generation")]
        [Tooltip("중복 허용 여부")]
        public bool allowDuplicate = false;

        [Tooltip("비활성 아이템 포함 여부")]
        public bool includeDisabledEntries = false;

        public bool HasEntries => products != null && products.Count > 0;

        public List<ShopProductSO> GetAvailableProducts()
        {
            List<ShopProductSO> result = new();

            if (products == null)
            {
                return result;
            }

            foreach (ShopProductSO product in products)
            {
                if (product == null)
                {
                    continue;
                }

                result.Add(product);
            }

            return result;
        }
    }
}