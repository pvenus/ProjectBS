using UnityEngine;

namespace Bless
{
    [CreateAssetMenu(
        fileName = "BlessConfig",
        menuName = "Bless/Bless Config")]
    public class BlessConfigSO : ScriptableObject
    {
        [Header("Common Pools")]
        [Tooltip("공용 블레스 풀")]
        [SerializeField]
        private BlessPoolSO commonPool;

        [Header("Settings")]
        [Tooltip("기본 공용 블레스 선택 개수")]
        [SerializeField]
        private int commonBlessingCount = 1;

        public BlessPoolSO CommonPool => commonPool;
        public int CommonBlessingCount => Mathf.Max(1, commonBlessingCount);
    }
}
