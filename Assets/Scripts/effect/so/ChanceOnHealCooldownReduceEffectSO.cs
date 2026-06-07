

using Character;
using UnityEngine;

namespace Effect
{
    /// <summary>
    /// 회복 이벤트가 발생했을 때 조건에 따라 스킬 쿨타임을 감소시키는 효과 정의.
    ///
    /// value/percent를 각각 적용할 수 있도록 기존 CooldownReduceEffectSO와 동일한
    /// CooldownReduceType, reducePercent, reduceSeconds 구조를 사용한다.
    /// </summary>
    [CreateAssetMenu(
        fileName = "effect.chance_on_heal_cooldown_reduce",
        menuName = "Effect/Chance On Heal Cooldown Reduce")]
    public class ChanceOnHealCooldownReduceEffectSO : EffectSO
    {
        [Header("Heal Trigger")]
        [Range(0f, 1f)]
        public float chance = 1f;

        public HealTriggerTargetType triggerTargetType = HealTriggerTargetType.AnyAlly;

        [Header("Cooldown Reduce")]
        public CooldownReduceType reduceType = CooldownReduceType.FlatSeconds;

        [Range(0f, 1f)]
        public float reducePercent;

        [Min(0f)]
        public float reduceSeconds = 1f;

        public ChanceOnHealCooldownReduceEffectRuntime CreateRuntimeData(
            CharacterManager ownerCharacter)
        {
            if (ownerCharacter == null)
            {
                return null;
            }

            return new ChanceOnHealCooldownReduceEffectRuntime(
                this,
                ownerCharacter);
        }
    }
}