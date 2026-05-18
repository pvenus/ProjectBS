using UnityEngine;

namespace Item
{
    [CreateAssetMenu(
        fileName = "ConsumeSO",
        menuName = "Game/Item/Consume Item")]
    public class ConsumeSO : ScriptableObject
    {
        [Header("Identity")]
        public string consumeId;

        public string displayName;

        [TextArea]
        public string description;

        public Sprite icon;

        [Header("Ownership")]
        [Tooltip("한 번 획득하면 영구적으로 사용 가능한지 여부")]
        public bool permanentOwned = true;

        [Header("Battle Usage")]
        [Tooltip("전투 1회 기준 사용 횟수 제한 여부")]
        public bool useLimitPerBattle;

        [Min(0)]
        public int maxUseCountPerBattle = 1;

        [Header("Shop")]
        [Min(0)]
        public int defaultPrice = 100;

        [Min(0)]
        public int weight = 100;

        [Header("Tags")]
        public string[] tags;
    }
}
