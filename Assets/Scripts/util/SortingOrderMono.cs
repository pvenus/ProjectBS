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
                spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
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
