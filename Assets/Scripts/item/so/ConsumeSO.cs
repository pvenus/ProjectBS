using Effect;
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

        [Header("Cost Pool")]
        [Tooltip("클래시 로얄 엘릭서처럼 소비 아이템 사용 시 필요한 코스트")]
        [Min(0)]
        public int cost = 1;

        [Tooltip("전투 중 코스트만 충분하면 반복 사용 가능한지 여부")]
        public bool reusable = true;

        [Header("Effect")]
        [Tooltip("사용 시 적용할 이펙트 목록. 단발 스탯 변경, 버프, 디버프 모두 이곳에 연결")]
        public EffectSO[] effects;

        [Tooltip("이 아이템으로 적용되는 이펙트의 기본 수명 타입")]
        public EffectLifetimeType lifetimeType = EffectLifetimeType.CombatTimed;

        [Tooltip("Timed / CombatTimed 타입일 때 지속 시간. 0 이하이면 EffectSO 자체 값 또는 즉시형 처리 기준으로 사용")]
        public float duration = 0f;

        [Header("Target")]
        [Tooltip("아군에게 적용되는 아이템인지 여부")]
        public bool targetAlly = true;

        [Tooltip("적에게 적용되는 아이템인지 여부")]
        public bool targetEnemy;

        [Header("Shop")]
        [Min(0)]
        public int defaultPrice = 100;

        [Min(0)]
        public int weight = 100;

        [Header("Tags")]
        public string[] tags;
    }
}
