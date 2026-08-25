using UnityEngine;

namespace Util
{
    [DisallowMultipleComponent]
    public class SortingOrderMono : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer[] spriteRenderers;
        [SerializeField] private Transform sortPivot;
        [SerializeField] private int sortingOffset;
        [SerializeField, Min(1)] private int sortingScale = 100;

        private void Awake()
        {
            if (spriteRenderers == null || spriteRenderers.Length == 0)
            {
                var allRenderers = GetComponentsInChildren<SpriteRenderer>(true);
                var validRenderers = new System.Collections.Generic.List<SpriteRenderer>();

                foreach (var sr in allRenderers)
                {
                    if (sr == null) continue;

                    // HUD나 스킬 슬롯, 오라 등 독자적인 SortingOrder를 관리하는 자식 컴포넌트는 제외
                    if (sr.GetComponentInParent<Character.UI.CharacterSkillCooldownSlot>() != null ||
                        sr.GetComponentInParent<Party.UI.CharacterBattleHudUI>() != null ||
                        sr.GetComponentInParent<Battle.Presentation.BattleCharacterAuraView>() != null)
                    {
                        continue;
                    }

                    validRenderers.Add(sr);
                }

                spriteRenderers = validRenderers.ToArray();
            }

            if (sortPivot == null)
            {
                sortPivot = transform;
            }
        }

        private void LateUpdate()
        {
            UpdateSortingOrder();
        }

        public void UpdateSortingOrder()
        {
            if (spriteRenderers == null || spriteRenderers.Length == 0)
            {
                return;
            }

            float y = sortPivot.position.y;
            int order = sortingOffset - Mathf.RoundToInt(y * sortingScale);

            foreach (var spriteRenderer in spriteRenderers)
            {
                if (spriteRenderer == null)
                {
                    continue;
                }

                spriteRenderer.sortingOrder = order;
            }
        }
    }
}
