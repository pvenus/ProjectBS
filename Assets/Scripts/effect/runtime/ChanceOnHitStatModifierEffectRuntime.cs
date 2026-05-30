using Character;
using Stat;
using UnityEngine;

namespace Effect
{
    public class ChanceOnHitStatModifierEffectRuntime : EffectRuntimeData
    {
        private readonly ChanceOnHitStatModifierEffectSO effectSO;

        private readonly CharacterManager targetCharacter;

        private bool isApplied;

        private float appliedValue;

        public ChanceOnHitStatModifierEffectRuntime(
            ChanceOnHitStatModifierEffectSO effectSO,
            CharacterManager targetCharacter)
        {
            this.effectSO = effectSO;
            this.targetCharacter = targetCharacter;

            RuntimeId =
                $"ChanceOnHitStatModifier_{effectSO.effectId}_{GetTargetRuntimeId()}";
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
            if (effectSO == null || targetCharacter == null)
            {
                IsActive = false;
                return;
            }

            if (effectSO.chancePercent <= 0f)
            {
                IsActive = false;
                return;
            }

            float randomValue =
                Random.Range(0f, 100f);

            if (randomValue > effectSO.chancePercent)
            {
                IsActive = false;
                return;
            }

            float currentValue =
                targetCharacter.GetStatValue(effectSO.statType);

            appliedValue =
                ResolveAppliedValue(currentValue);

            if (Mathf.Approximately(appliedValue, 0f))
            {
                IsActive = false;
                return;
            }

            targetCharacter.AddStat(
                effectSO.statType,
                appliedValue);

            isApplied = true;
        }

        public override void OnRemove()
        {
            if (!isApplied
                || effectSO == null
                || targetCharacter == null
                || Mathf.Approximately(appliedValue, 0f))
            {
                return;
            }

            float currentValue =
                targetCharacter.GetStatValue(effectSO.statType);

            targetCharacter.AddStat(
                effectSO.statType,
                -appliedValue);

            isApplied = false;
            appliedValue = 0f;
        }

        private float ResolveAppliedValue(
            float currentValue)
        {
            switch (effectSO.valueType)
            {
                case ChanceOnHitStatModifierValueType.Flat:
                    return effectSO.value;

                case ChanceOnHitStatModifierValueType.PercentOfCurrentValue:
                    return currentValue * effectSO.value;

                default:
                    return 0f;
            }
        }
    }
}