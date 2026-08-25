using UnityEngine;

namespace Shop
{
    /// <summary>
    /// 상품 호버 연출의 교체 가능한 기반 컴포넌트.
    /// 입력 처리는 ShopProductHoverController가 담당한다.
    /// </summary>
    public abstract class ShopProductHoverEffectBase : MonoBehaviour
    {
        public abstract void SetHovered(bool hovered);

        public virtual void ResetEffect()
        {
            SetHovered(false);
        }
    }
}
