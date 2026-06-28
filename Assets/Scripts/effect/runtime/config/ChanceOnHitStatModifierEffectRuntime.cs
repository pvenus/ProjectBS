using Character;
using Stat;
using UnityEngine;

namespace Effect
{
    public class ChanceOnHitStatModifierEffectRuntime : EffectRuntimeData
    {
        private readonly EffectSO effectSO;
        private readonly ChanceOnHitStatModifierEffectConfig config;

        private readonly CharacterManager targetCharacter;

        private bool isApplied;

        private float appliedValue;

        public ChanceOnHitStatModifierEffectRuntime(
            EffectSO effectSO,
            ChanceOnHitStatModifierEffectConfig config,
            CharacterManager targetCharacter)
        {
            this.effectSO = effectSO;
            this.config = config;
            this.targetCharacter = targetCharacter;

            RuntimeId =
                $"ChanceOnHitStatModifier_{effectSO.EffectId}_{GetTargetRuntimeId()}";
        }

        private string GetTargetRuntimeId()
        {
            if (targetCharacter == null)
            {
                return "None";
            }

            return targetCharacter.GetInstanceID().ToString();
        }

        public override void OnApply()
        {
            if (effectSO == null || config == null || targetCharacter == null)
            {
                IsActive = false;
                return;
            }

            IsActive = true;
        }

        public void OnHit(
            CharacterDamageRequest request,
            CharacterDamageResult result)
        {
            if (!IsActive
                || effectSO == null
                || config == null
                || targetCharacter == null
                || request == null)
            {
                return;
            }

            if (request.target.GetComponent<CharacterManager>() != targetCharacter)
            {
                return;
            }

            float randomValue = Random.Range(0f, 100f);

            if (randomValue > config.ChancePercent)
            {
                return;
            }

            if (isApplied && !Mathf.Approximately(appliedValue, 0f))
            {
                targetCharacter.AddStat(
                    config.StatType,
                    -appliedValue);
            }

            float currentValue =
                targetCharacter.GetStatValue(config.StatType);

            appliedValue =
                ResolveAppliedValue(currentValue);

            if (Mathf.Approximately(appliedValue, 0f))
            {
                return;
            }

            targetCharacter.AddStat(
                config.StatType,
                appliedValue);

            isApplied = true;
        }

        public override void OnRemove()
        {
            if (!isApplied
                || effectSO == null
                || config == null
                || targetCharacter == null
                || Mathf.Approximately(appliedValue, 0f))
            {
                return;
            }

            targetCharacter.AddStat(
                config.StatType,
                -appliedValue);

            isApplied = false;
            appliedValue = 0f;
        }

        private float ResolveAppliedValue(
            float currentValue)
        {
            switch (config.ValueType)
            {
                case StatModifierType.Flat:
                    return config.Value;

                case StatModifierType.Percent:
                    return currentValue * (config.Value / 100f);

                default:
                    return 0f;
            }
        }
    }
}