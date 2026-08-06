using System;
using Stat;
using UnityEngine;

namespace Effect
{
    [Serializable]
    public class OnHitTimedStatModifierEffectConfig : EffectConfig
    {
        [SerializeField] private float chancePercent;
        [SerializeField] private StatType statType = StatType.None;
        [SerializeField] private StatModifierType modifierType = StatModifierType.Flat;
        [SerializeField] private float value;
        [SerializeField] private float durationSeconds;

        public float ChancePercent => chancePercent;
        public StatType StatType => statType;
        public StatModifierType ModifierType => modifierType;
        public float Value => value;
        public float DurationSeconds => durationSeconds;

        public void ApplyEditorData(
            float chancePercent,
            StatType statType,
            StatModifierType modifierType,
            float value,
            float durationSeconds)
        {
            this.chancePercent = chancePercent;
            this.statType = statType;
            this.modifierType = modifierType;
            this.value = value;
            this.durationSeconds = durationSeconds;
        }
    }
}
