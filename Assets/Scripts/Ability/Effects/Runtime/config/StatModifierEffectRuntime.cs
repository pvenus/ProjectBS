using Character;

namespace Effect
{
    public class StatModifierEffectRuntime
        : EffectRuntimeData
    {
        private readonly EffectSO effectSO;
        private readonly StatModifierEffectConfig config;
        private readonly CharacterManager targetCharacterManager;
        private readonly float? valueOverride;

        private float appliedValue;

        public StatModifierEffectRuntime(
            EffectSO effectSO,
            StatModifierEffectConfig config,
            CharacterManager targetCharacterManager,
            float? valueOverride = null)
        {
            this.effectSO = effectSO;
            this.config = config;
            this.targetCharacterManager = targetCharacterManager;
            this.valueOverride = valueOverride;

            RuntimeId =
                $"StatModifier_{effectSO.EffectId}_{GetTargetRuntimeId()}";
        }

        public override void OnApply()
        {
            if (effectSO == null || config == null || targetCharacterManager == null)
            {
                return;
            }

            appliedValue = CalculateModifierValue();

            targetCharacterManager.AddStat(
                config.TargetStat,
                appliedValue);
        }

        public override void OnRemove()
        {
            if (effectSO == null || config == null || targetCharacterManager == null)
            {
                return;
            }

            targetCharacterManager.AddStat(
                config.TargetStat,
                -appliedValue);
        }

        private float CalculateModifierValue()
        {
            float currentValue =
                targetCharacterManager.GetStatValue(
                    config.TargetStat);
            float resolvedValue = valueOverride ?? config.Value;
            return config.ModifierType switch
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
