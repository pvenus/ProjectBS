using Character;
using UnityEngine;

namespace Effect
{
    public class CooldownReduceEffectRuntime
        : EffectRuntimeData
    {
        private readonly EffectSO effectSO;
        private readonly CooldownReduceEffectConfig config;
        private readonly CharacterManager targetCharacter;

        public CooldownReduceEffectRuntime(
            EffectSO effectSO,
            CooldownReduceEffectConfig config,
            CharacterManager targetCharacter = null)
        {
            this.effectSO = effectSO;
            this.config = config;
            this.targetCharacter = targetCharacter;

            RuntimeId =
                $"CooldownReduce_{effectSO.EffectId}_{GetTargetRuntimeId()}";
        }

        public override void OnApply()
        {
            if (effectSO == null || config == null || targetCharacter == null)
            {
                return;
            }

            CharacterSkillManager skillManager =
                ResolveCharacterSkillManager(targetCharacter);

            if (skillManager == null)
            {
                return;
            }

            TryReduceCooldowns(skillManager);
        }

        public override void OnRemove()
        {
            // Instant Cooldown Reduce Effect
        }

        private void TryReduceCooldowns(
            CharacterSkillManager skillManager)
        {
            if (skillManager == null)
            {
                return;
            }

            float percent = ShouldUsePercent()
                ? Mathf.Clamp01(config.ReducePercent)
                : 0f;

            float seconds = ShouldUseFlatSeconds()
                ? Mathf.Max(0f, config.ReduceSeconds)
                : 0f;

            skillManager.ReduceAllCooldowns(
                percent,
                seconds);
        }

        private bool ShouldUsePercent()
        {
            return config.ReduceType == CooldownReduceType.Percent
                || config.ReduceType == CooldownReduceType.PercentAndFlat;
        }

        private bool ShouldUseFlatSeconds()
        {
            return config.ReduceType == CooldownReduceType.FlatSeconds
                || config.ReduceType == CooldownReduceType.PercentAndFlat;
        }

        private CharacterSkillManager ResolveCharacterSkillManager(
            CharacterManager characterManager)
        {
            if (characterManager == null)
            {
                return null;
            }

            CharacterSkillManager skillManager =
                characterManager.GetComponent<CharacterSkillManager>();

            if (skillManager != null)
            {
                return skillManager;
            }

            skillManager = characterManager.GetComponentInChildren<CharacterSkillManager>();

            if (skillManager != null)
            {
                return skillManager;
            }

            return characterManager.GetComponentInParent<CharacterSkillManager>();
        }

        private string GetTargetRuntimeId()
        {
            return targetCharacter != null
                ? targetCharacter.GetInstanceID().ToString()
                : "NoTarget";
        }
    }
}
