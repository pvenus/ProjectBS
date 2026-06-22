using Character;

namespace Effect
{
    public class StatModifierEffectRuntime
        : EffectRuntimeData
    {
        private readonly StatModifierEffectSO effectSO;
        private readonly CharacterManager targetCharacterManager;
        private readonly float? valueOverride;

        private float appliedValue;

        public StatModifierEffectRuntime(
            StatModifierEffectSO effectSO,
            EffectSourceType sourceType,
            string sourceId,
            CharacterManager targetCharacterManager,
            float? valueOverride = null)
        {
            this.effectSO = effectSO;
            this.targetCharacterManager = targetCharacterManager;
            this.valueOverride = valueOverride;

            SourceType = sourceType;
            SourceId = sourceId;

            RuntimeId =
                $"{sourceType}_{sourceId}_{effectSO.effectId}_{GetTargetRuntimeId()}";
        }

        public override void OnApply()
        {
            if (effectSO == null || targetCharacterManager == null)
            {
                return;
            }

            appliedValue = CalculateModifierValue();

            targetCharacterManager.AddStat(
                effectSO.targetStat,
                appliedValue);
        }

        public override void OnRemove()
        {
            if (effectSO == null || targetCharacterManager == null)
            {
                return;
            }

            targetCharacterManager.AddStat(
                effectSO.targetStat,
                -appliedValue);
        }

        private float CalculateModifierValue()
        {
            float currentValue =
                targetCharacterManager.GetStatValue(
                    effectSO.targetStat);
            float resolvedValue = valueOverride ?? effectSO.value;
            return effectSO.modifierType switch
            {
                StatModifierType.Flat
                    => resolvedValue,

                StatModifierType.Percent
                    => currentValue * resolvedValue,

                StatModifierType.Multiply
                    => currentValue * (resolvedValue - 1f),

                _ => resolvedValue
            };
        }

        private string GetTargetRuntimeId()
        {
            return targetCharacterManager != null
                ? targetCharacterManager.GetInstanceID().ToString()
                : "NoTarget";
        }
    }
}
