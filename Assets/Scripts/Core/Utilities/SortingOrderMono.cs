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

        private bool appliedMorpgOrder;

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

            int order = CalculateSortingOrder();
            appliedMorpgOrder = Battle.Morpg.MorpgEnvironmentRuntime.OwnsActor(
                GetComponentInParent<Character.CharacterManager>());

            foreach (var spriteRenderer in spriteRenderers)
            {
                if (spriteRenderer == null)
                {
                    continue;
                }

                spriteRenderer.sortingOrder = order;
            }
        }

        private void OnDisable()
        {
            if (!appliedMorpgOrder || spriteRenderers == null) return;
            Transform pivot = sortPivot != null ? sortPivot : transform;
            int legacyOrder = sortingOffset - Mathf.RoundToInt(pivot.position.y * sortingScale);
            foreach (var renderer in spriteRenderers) if (renderer != null) renderer.sortingOrder = legacyOrder;
            appliedMorpgOrder = false;
        }

        public int CalculateSortingOrder()
        {
            Transform pivot = sortPivot != null ? sortPivot : transform;
            int legacyOrder = sortingOffset - Mathf.RoundToInt(pivot.position.y * sortingScale);
            return Battle.Morpg.MorpgEnvironmentRuntime.OwnsActor(GetComponentInParent<Character.CharacterManager>())
                ? Battle.BattlePresentationSortingPolicy.MorpgBodyOrder(legacyOrder) : legacyOrder;
        }
    }
}
