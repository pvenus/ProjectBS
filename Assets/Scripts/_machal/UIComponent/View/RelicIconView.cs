using UnityEngine;
using UnityEngine.UI;
using UIFramework.Data;

namespace UIFramework.View
{
    [AutoBindPrefix("UI")]
    public class RelicIconView : UIComponent
    {
        [AutoBind]
        [SerializeField] private Image iconImage;
        
        [AutoBind]
        [SerializeField] private Transform decoImage; // 신유물 전용 장식 (Deco) 이미지 루트

        public void Bind(RelicItemViewData data)
        {
            if (data == null) return;

            if (iconImage != null)
            {
                iconImage.sprite = data.icon;
                iconImage.enabled = data.icon != null;
            }

            // 신유물(God Relic)인 경우에만 Deco 오브젝트 활성화
            if (decoImage != null)
            {
                decoImage.gameObject.SetActive(data.type == RelicType.God);
            }
        }
    }
}
