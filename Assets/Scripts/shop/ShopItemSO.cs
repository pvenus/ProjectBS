using System.Collections.Generic;
using UnityEngine;

namespace Shop
{
    /// <summary>
    /// 상점에서 판매 가능한 아이템 정의 데이터.
    /// 현재는 장비/소모품/유물 등 최종 타입이 확정되지 않았기 때문에,
    /// 상점 생성 및 구매 플로우 검증에 필요한 최소 정보만 가진다.
    /// </summary>
    [CreateAssetMenu(menuName = "Shop/Shop Item")]
    public class ShopItemSO : ScriptableObject
    {
        [Header("Identity")]
        public string itemId;
        public string itemName;

        [TextArea]
        public string description;

        [Header("Display")]
        public Sprite icon;

        [Header("Shop")]
        [Min(0)]
        public int basePrice = 100;

        public ShopItemRarity rarity = ShopItemRarity.Common;
        public ShopItemCategory category = ShopItemCategory.Generic;

        [Header("Flow Only")]
        [Tooltip("실제 효과 시스템이 붙기 전까지 구매 후 처리 방식을 구분하기 위한 임시 타입")]
        public ShopItemUseMode useMode = ShopItemUseMode.None;

        [Tooltip("디버그/임시 표시용 효과 설명")]
        public string effectLabel;

        [Tooltip("디버그/임시 효과 값. 실제 효과 시스템 연결 전까지 테스트용으로만 사용")]
        public int effectValue;

        [Header("Generation Tags")]
        [Tooltip("상점 풀/생성 규칙에서 필터링에 사용할 태그")]
        public List<string> tags = new();

        public string DisplayName => string.IsNullOrWhiteSpace(itemName) ? name : itemName;

        public bool HasTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                return false;
            }

            return tags != null && tags.Contains(tag);
        }
    }

}
