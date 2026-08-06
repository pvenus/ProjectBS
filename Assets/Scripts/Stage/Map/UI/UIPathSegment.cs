using System.Collections.Generic;
using UnityEngine;

namespace Stage.UI
{
    /// <summary>
    /// 패스 세그먼트 하위에 비주얼 표현용 프리팹을 무작위로 생성하는 컴포넌트.
    /// </summary>
    public class UIPathSegment : MonoBehaviour
    {
        [Header("Visual Prefabs")]
        [SerializeField] private List<GameObject> segmentPrefabs = new();

        private GameObject spawnedVisualObject;

        /// <summary>
        /// 전달된 프리팹 리스트(우선) 또는 자체 segmentPrefabs 중 하나를 무작위로 하위에 생성합니다.
        /// </summary>
        public void ApplyRandomVisual(List<GameObject> overridePrefabs = null, System.Random rng = null)
        {
            List<GameObject> prefabs = (overridePrefabs != null && overridePrefabs.Count > 0)
                ? overridePrefabs
                : segmentPrefabs;

            if (prefabs == null || prefabs.Count == 0) return;

            if (spawnedVisualObject != null)
            {
                Destroy(spawnedVisualObject);
                spawnedVisualObject = null;
            }

            int index = (rng != null) ? rng.Next(prefabs.Count) : Random.Range(0, prefabs.Count);
            GameObject prefab = prefabs[index];
            if (prefab == null) return;

            spawnedVisualObject = Instantiate(prefab, transform, false);

            if (spawnedVisualObject.transform is RectTransform rect)
            {
                rect.anchoredPosition = Vector2.zero;
                rect.localScale = Vector3.one;
                rect.localRotation = Quaternion.identity;
            }
            else
            {
                spawnedVisualObject.transform.localPosition = Vector3.zero;
                spawnedVisualObject.transform.localScale = Vector3.one;
                spawnedVisualObject.transform.localRotation = Quaternion.identity;
            }
        }
    }
}
