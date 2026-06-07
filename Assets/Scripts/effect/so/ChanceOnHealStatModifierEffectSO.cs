using Stat;
using UnityEngine;

namespace Effect
{
    /// <summary>
    /// 회복 이벤트가 발생했을 때 조건에 따라 스탯 버프를 적용하는 효과 정의.
    ///
    /// 예)
    /// - 내가 회복을 받을 때 공격력 증가
    /// - 나를 제외한 아군이 회복을 받을 때 방어 증가
    /// - 파티 회복이 발생했을 때 쿨타임 감소/스탯 증가
    /// </summary>
    [CreateAssetMenu(
        fileName = "effect.chance_on_heal_stat_modifier",
        menuName = "Effect/Chance On Heal Stat Modifier")]
    public class ChanceOnHealStatModifierEffectSO : EffectSO
    {
        [Header("Heal Trigger")]
        [Range(0f, 1f)]
        public float chance = 1f;

        public HealTriggerTargetType triggerTargetType = HealTriggerTargetType.AnyAlly;


        [Header("Stat Modifier")]
        public StatType statType = StatType.Attack;

        public StatModifierType valueType = StatModifierType.Flat;

        [Tooltip("스탯에 더할 값. Percent 타입이면 퍼센트 값으로 해석한다.")]
        public float value;
    }
}