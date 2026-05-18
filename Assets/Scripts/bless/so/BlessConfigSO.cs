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
        public BlessPoolSO commonPool;

        [Header("Settings")]
        [Tooltip("기본 공용 블레스 선택 개수")]
        public int commonBlessingCount = 1;
    }
}
