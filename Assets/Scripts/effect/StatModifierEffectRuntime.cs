using Stat;
using Character;
using UnityEngine;

namespace Effect
{
    public class StatModifierEffectRuntime
        : EffectRuntimeData
    {
        private readonly StatModifierEffectSO effectSO;
        private readonly StatManager statManager;

        private float appliedValue;

        public StatModifierEffectRuntime(
            StatModifierEffectSO effectSO,
            EffectSourceType sourceType,
            string sourceId)
        {
            this.effectSO = effectSO;

            SourceType = sourceType;
            SourceId = sourceId;

            RuntimeId =
                $"{sourceType}_{sourceId}_{effectSO.effectId}";

            statManager = StatManager.Instance;
        }

        public override void OnApply()
        {
            if (effectSO == null
                || statManager == null)
            {
                return;
            }

            float value =
                CalculateModifierValue();

            appliedValue = value;

            statManager.AddStat(
                effectSO.targetStat,
                appliedValue);

            CharacterManager[] characterManagers =
                Object.FindObjectsByType<CharacterManager>(
                    FindObjectsSortMode.None);

            for (int i = 0;
                 i < characterManagers.Length;
                 i++)
            {
                CharacterManager characterManager =
                    characterManagers[i];

                if (characterManager == null)
                {
                    continue;
                }

                characterManager.AddStat(
                    effectSO.targetStat,
                    appliedValue);
            }
        }

        public override void OnRemove()
        {
            if (effectSO == null
                || statManager == null)
            {
                return;
            }

            statManager.AddStat(
                effectSO.targetStat,
                -appliedValue);

            CharacterManager[] characterManagers =
                Object.FindObjectsByType<CharacterManager>(
                    FindObjectsSortMode.None);

            for (int i = 0;
                 i < characterManagers.Length;
                 i++)
            {
                CharacterManager characterManager =
                    characterManagers[i];

                if (characterManager == null)
                {
                    continue;
                }

                characterManager.AddStat(
                    effectSO.targetStat,
                    -appliedValue);
            }
        }

        private float CalculateModifierValue()
        {
            float currentValue =
                statManager.GetStat(
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
    }
}
