

using Character;
using UnityEngine;

namespace Effect
{
    /// <summary>
    /// 회복 이벤트가 발생했을 때 조건에 따라 스킬 쿨타임을 감소시키는 런타임.
    ///
    /// 실제 이벤트 연결은 EffectManager가 CharacterManager.OnAnyHealed를 받아
    /// OnHeal(...)을 호출하는 방식으로 처리한다.
    /// </summary>
    public class ChanceOnHealCooldownReduceEffectRuntime : EffectRuntimeData
    {
        private readonly ChanceOnHealCooldownReduceEffectSO effectSO;
        private readonly CharacterManager ownerCharacter;

        public ChanceOnHealCooldownReduceEffectRuntime(
            ChanceOnHealCooldownReduceEffectSO effectSO,
            CharacterManager ownerCharacter)
        {
            this.effectSO = effectSO;
            this.ownerCharacter = ownerCharacter;

            RuntimeId =
                $"ChanceOnHealCooldownReduce_{effectSO.effectId}_{GetOwnerRuntimeId()}";
        }

        public override void OnApply()
        {
            IsActive = true;
        }

        public override void OnRemove()
        {
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

            ReduceCooldowns();
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

        private void ReduceCooldowns()
        {
            SkillExecutorMono skillExecutor = ResolveSkillExecutor();

            if (skillExecutor == null)
            {
                return;
            }

            float percent = 0f;
            float seconds = 0f;

            switch (effectSO.reduceType)
            {
                case CooldownReduceType.Percent:
                    percent = Mathf.Clamp01(effectSO.reducePercent);
                    break;

                case CooldownReduceType.FlatSeconds:
                    seconds = Mathf.Max(0f, effectSO.reduceSeconds);
                    break;

                case CooldownReduceType.PercentAndFlat:
                    percent = Mathf.Clamp01(effectSO.reducePercent);
                    seconds = Mathf.Max(0f, effectSO.reduceSeconds);
                    break;
            }

            if (percent <= 0f && seconds <= 0f)
            {
                return;
            }

            skillExecutor.ReduceAllCooldowns(
                percent,
                seconds);
        }

        private SkillExecutorMono ResolveSkillExecutor()
        {
            if (ownerCharacter == null)
            {
                return null;
            }

            SkillExecutorMono skillExecutor =
                ownerCharacter.GetComponent<SkillExecutorMono>();

            if (skillExecutor != null)
            {
                return skillExecutor;
            }

            skillExecutor = ownerCharacter.GetComponentInParent<SkillExecutorMono>();

            if (skillExecutor != null)
            {
                return skillExecutor;
            }

            return ownerCharacter.GetComponentInChildren<SkillExecutorMono>();
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