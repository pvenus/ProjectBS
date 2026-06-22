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
                ? Mathf.Clamp01(effectSO.reducePercent)
                : 0f;

            float seconds = ShouldUseFlatSeconds()
                ? Mathf.Max(0f, effectSO.reduceSeconds)
                : 0f;

            skillManager.ReduceAllCooldowns(
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
