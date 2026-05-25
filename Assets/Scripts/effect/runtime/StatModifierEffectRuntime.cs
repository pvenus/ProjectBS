using Character;

namespace Effect
{
    public class StatModifierEffectRuntime
        : EffectRuntimeData
    {
        private readonly StatModifierEffectSO effectSO;
        private readonly CharacterManager targetCharacterManager;

        private float appliedValue;

        public StatModifierEffectRuntime(
            StatModifierEffectSO effectSO,
            EffectSourceType sourceType,
            string sourceId,
            CharacterManager targetCharacterManager)
        {
            this.effectSO = effectSO;
            this.targetCharacterManager = targetCharacterManager;

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

            return effectSO.modifierType switch
            {
                StatModifierType.Flat
                    => effectSO.value,

                StatModifierType.Percent
                    => currentValue * effectSO.value,

                StatModifierType.Multiply
                    => currentValue * (effectSO.value - 1f),

                _ => effectSO.value
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
