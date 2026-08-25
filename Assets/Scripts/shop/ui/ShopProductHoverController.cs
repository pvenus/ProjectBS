using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Shop
{
    /// <summary>
    /// 상품의 포인터 진입/이탈을 감지해 연결된 호버 연출에 전달한다.
    /// 연출 교체 시 이 컴포넌트는 유지하고 effect 목록만 바꾼다.
    /// </summary>
    public class ShopProductHoverController : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        [SerializeField] private List<ShopProductHoverEffectBase> effects = new();
        [SerializeField] private bool findEffectsOnSameObjectWhenEmpty = true;

        private bool interactionEnabled = true;

        public bool InteractionEnabled => interactionEnabled;

        private void Awake()
        {
            if (findEffectsOnSameObjectWhenEmpty && effects.Count == 0)
            {
                effects.AddRange(GetComponents<ShopProductHoverEffectBase>());
            }
        }

        private void OnDisable()
        {
            SetHovered(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!interactionEnabled)
            {
                return;
            }

            SetHovered(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SetHovered(false);
        }

        public void SetInteractionEnabled(bool enabled)
        {
            interactionEnabled = enabled;

            if (!interactionEnabled)
            {
                SetHovered(false);
            }
        }

        private void SetHovered(bool hovered)
        {
            foreach (ShopProductHoverEffectBase effect in effects)
            {
                if (effect != null && effect.isActiveAndEnabled)
                {
                    effect.SetHovered(hovered);
                }
            }
        }
    }
}
