using Character;
using Stat;
using UnityEngine;

namespace Effect
{
    /// <summary>
    /// 회복 이벤트 발생 시 조건에 따라 스탯 버프를 적용하는 런타임.
    ///
    /// 회복 이벤트 연결은 외부에서 OnHeal(...)을 호출하는 방식으로 붙인다.
    /// </summary>
    public class ChanceOnHealStatModifierEffectRuntime : EffectRuntimeData
    {
        private readonly ChanceOnHealStatModifierEffectSO effectSO;
        private readonly CharacterManager ownerCharacter;

        private float appliedValue;

        public ChanceOnHealStatModifierEffectRuntime(
            ChanceOnHealStatModifierEffectSO effectSO,
            CharacterManager ownerCharacter)
        {
            this.effectSO = effectSO;
            this.ownerCharacter = ownerCharacter;

            RuntimeId =
                $"ChanceOnHealStatModifier_{effectSO.effectId}_{GetOwnerRuntimeId()}";
        }

        public override void OnApply()
        {
            IsActive = true;
        }

        public override void OnRemove()
        {
            RemoveAppliedValue();
            IsActive = false;
        }

        public void OnHeal(
            CharacterManager healer,
            CharacterManager healTarget,
            float healAmount)
        {
            if (!IsActive || effectSO == null || ownerCharacter == null)
            {
                return;
            }

            if (!CanTrigger(healer, healTarget, healAmount))
            {
                return;
            }

            if (effectSO.chance < 1f && Random.value > effectSO.chance)
            {
                return;
            }

            ApplyModifier();
        }

        private bool CanTrigger(
            CharacterManager healer,
            CharacterManager healTarget,
            float healAmount)
        {
            if (healTarget == null)
            {
                return false;
            }

            if (healAmount <= 0f)
            {
                return false;
            }

            switch (effectSO.triggerTargetType)
            {
                case HealTriggerTargetType.Self:
                    return healTarget == ownerCharacter;

                case HealTriggerTargetType.OtherAlly:
                    return healTarget != ownerCharacter && IsAlly(healTarget);

                case HealTriggerTargetType.Party:
                case HealTriggerTargetType.AnyAlly:
                    return IsAlly(healTarget);

                default:
                    return false;
            }
        }

        private bool IsAlly(
            CharacterManager target)
        {
            if (target == null || ownerCharacter == null)
            {
                return false;
            }

            return target.gameObject.layer == ownerCharacter.gameObject.layer;
        }

        private void ApplyModifier()
        {
            if (ownerCharacter == null)
            {
                return;
            }

            RemoveAppliedValue();

            float value = ResolveValue();

            ownerCharacter.AddStat(
                effectSO.statType,
                value);

            appliedValue += value;
        }

        private float ResolveValue()
        {
            return effectSO.value;
        }

        private void RemoveAppliedValue()
        {
            if (ownerCharacter == null || Mathf.Approximately(appliedValue, 0f))
            {
                return;
            }

            ownerCharacter.AddStat(
                effectSO.statType,
                -appliedValue);

            appliedValue = 0f;
        }

        private string GetOwnerRuntimeId()
        {
            if (ownerCharacter == null)
            {
                return "Unknown";
            }

            return ownerCharacter.GetInstanceID().ToString();
        }
    }
}