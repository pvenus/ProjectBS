using System.Collections.Generic;
using UnityEngine;

namespace Stage.UI
{
    /// <summary>
    /// 신전 노드 전용 비주얼 관리 컴포넌트
    /// </summary>
    public class UINodeSlot_Shrine : UINodeSlot
    {
        [Header("Shrine Custom Sprites")]
        [SerializeField] private List<Sprite> shrineSprites = new();

        /// <summary>
        /// 신전에 사용할 이미지 스프라이트 리스트를 설정합니다.
        /// </summary>
        public void InitializeShrine(List<Sprite> sprites)
        {
            if (sprites != null)
            {
                shrineSprites = new List<Sprite>(sprites);
            }
        }

        public override void SetNodeData(RoundNode node)
        {
            base.SetNodeData(node);

            if (shrineSprites != null && shrineSprites.Count > 0)
            {
                // nodeId의 해시값을 사용하여 결정적 랜덤 인덱스 선택
                int hash = node.nodeId.GetHashCode();
                int index = Mathf.Abs(hash) % shrineSprites.Count;
                Sprite selectedSprite = shrineSprites[index];

                if (iconImage != null)
                {
                    iconImage.sprite = selectedSprite;
                    iconImage.enabled = selectedSprite != null;
                }
            }
        }
    }
}
