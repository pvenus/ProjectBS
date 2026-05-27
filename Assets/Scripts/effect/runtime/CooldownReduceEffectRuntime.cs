using Character;
using UnityEngine;

namespace Effect
{
    public class CooldownReduceEffectRuntime
        : EffectRuntimeData
    {
        private readonly CooldownReduceEffectSO effectSO;
        private readonly CharacterManager targetCharacter;

        public CooldownReduceEffectRuntime(
            CooldownReduceEffectSO effectSO,
            CharacterManager targetCharacter = null)
        {
            this.effectSO = effectSO;
            this.targetCharacter = targetCharacter;

            RuntimeId =
                $"CooldownReduce_{effectSO.effectId}_{GetTargetRuntimeId()}";
        }

        public override void OnApply()
        {
            if (effectSO == null || targetCharacter == null)
            {
                return;
            }

            SkillExecutorMono skillExecutor =
                ResolveSkillExecutor(targetCharacter);

            if (skillExecutor == null)
            {
                return;
            }

            TryReduceCooldowns(skillExecutor);
        }

        public override void OnRemove()
        {
            // Instant Cooldown Reduce Effect
        }

        private void TryReduceCooldowns(
            SkillExecutorMono skillExecutor)
        {
            if (skillExecutor == null)
            {
                return;
            }

            float percent = ShouldUsePercent()
                ? Mathf.Clamp01(effectSO.reducePercent)
                : 0f;

            float seconds = ShouldUseFlatSeconds()
                ? Mathf.Max(0f, effectSO.reduceSeconds)
                : 0f;

            skillExecutor.ReduceAllCooldowns(
                percent,
                seconds);
        }

        private bool ShouldUsePercent()
        {
            return effectSO.reduceType == CooldownReduceType.Percent
                || effectSO.reduceType == CooldownReduceType.PercentAndFlat;
        }

        private bool ShouldUseFlatSeconds()
        {
            return effectSO.reduceType == CooldownReduceType.FlatSeconds
                || effectSO.reduceType == CooldownReduceType.PercentAndFlat;
        }

        private SkillExecutorMono ResolveSkillExecutor(
            CharacterManager characterManager)
        {
            if (characterManager == null)
            {
                return null;
            }

            SkillExecutorMono executor =
                characterManager.GetComponent<SkillExecutorMono>();

            if (executor != null)
            {
                return executor;
            }

            executor = characterManager.GetComponentInChildren<SkillExecutorMono>();

            if (executor != null)
            {
                return executor;
            }

            return characterManager.GetComponentInParent<SkillExecutorMono>();
        }

        private string GetTargetRuntimeId()
        {
            return targetCharacter != null
                ? targetCharacter.GetInstanceID().ToString()
                : "NoTarget";
        }
    }
}
