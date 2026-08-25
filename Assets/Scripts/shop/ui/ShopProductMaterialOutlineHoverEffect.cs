using UnityEngine;

namespace Shop
{
    /// <summary>
    /// UI Graphic의 머티리얼 프로퍼티를 바꿔 호버 아웃라인을 표시한다.
    /// 원본 공유 머티리얼을 수정하지 않고 Graphic별 런타임 복사본을 사용한다.
    /// </summary>
    public class ShopProductMaterialOutlineHoverEffect : ShopProductHoverEffectBase
    {
        [SerializeField] private string effectKey = "ShopProductOutline";

        public string EffectKey => effectKey;

        public override void SetHovered(bool hovered)
        {
            ApplyMaterialEffect(hovered);
        }

        /// <summary>
        /// Reserved implementation point for future material or shader behavior.
        /// </summary>
        protected virtual void ApplyMaterialEffect(bool hovered)
        {
        }
    }
}
