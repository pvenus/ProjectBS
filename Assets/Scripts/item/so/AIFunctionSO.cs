using UnityEngine;

namespace Item
{
    [CreateAssetMenu(
        fileName = "AIFunctionSO",
        menuName = "Game/Item/AI Function")]
    public class AIFunctionSO : ScriptableObject
    {
        [Header("Identity")]
        public string functionId;

        public string displayName;

        [TextArea]
        public string description;

        public Sprite icon;

        [Header("Equip")]
        [Tooltip("공용 AI 슬롯에 동시에 장착 가능한 최대 수")]
        [Min(1)]
        public int maxEquipCount = 1;

        [Tooltip("동일 기능 중복 장착 허용 여부")]
        public bool allowDuplicateEquip;

        [Header("Target")]
        [Tooltip("효과 적용 대상 범위")]
        public AIFunctionTargetScope targetScope =
            AIFunctionTargetScope.AllParty;

        [Header("Shop")]
        [Min(0)]
        public int defaultPrice = 100;

        [Min(0)]
        public int weight = 100;

        [Header("Tags")]
        public string[] tags;
    }

    public enum AIFunctionTargetScope
    {
        None = 0,

        Self = 100,
        SinglePartyMember = 200,
        AllParty = 300,
        LowestHpPartyMember = 400,
        RandomPartyMember = 500,
    }
}
