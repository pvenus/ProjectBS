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
        private readonly EffectSO effectSO;
        private readonly ChanceOnHealCooldownReduceEffectConfig config;
        private readonly CharacterManager ownerCharacter;

        public ChanceOnHealCooldownReduceEffectRuntime(
            EffectSO effectSO,
            ChanceOnHealCooldownReduceEffectConfig config,
            CharacterManager ownerCharacter)
        {
            this.effectSO = effectSO;
            this.config = config;
            this.ownerCharacter = ownerCharacter;

            RuntimeId =
                $"ChanceOnHealCooldownReduce_{effectSO.EffectId}_{GetOwnerRuntimeId()}";
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
            if (!IsActive || effectSO == null || config == null || ownerCharacter == null)
            {
                return;
            }

            if (!CanTrigger(healer, healTarget, healAmount))
            {
                return;
            }

            if (config.Chance < 1f && Random.value > config.Chance)
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

            switch (config.TriggerTargetType)
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

            switch (config.ReduceType)
            {
                case CooldownReduceType.Percent:
                    percent = Mathf.Clamp01(config.ReducePercent);
                    break;

                case CooldownReduceType.FlatSeconds:
                    seconds = Mathf.Max(0f, config.ReduceSeconds);
                    break;

                case CooldownReduceType.PercentAndFlat:
                    percent = Mathf.Clamp01(config.ReducePercent);
                    seconds = Mathf.Max(0f, config.ReduceSeconds);
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