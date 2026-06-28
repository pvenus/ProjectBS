

using System;
using Stat;
using UnityEngine;

namespace Effect
{
    [Serializable]
    public class StatModifierEffectConfig : EffectConfig
    {
        [Header("Stat Modifier")]
        [SerializeField]
        private StatType targetStat = StatType.None;

        [SerializeField]
        private StatModifierType modifierType = StatModifierType.Flat;

        [SerializeField]
        private float value;

        public StatType TargetStat => targetStat;
        public StatModifierType ModifierType => modifierType;
        public float Value => value;

        public void ApplyEditorData(
            StatType targetStat,
            StatModifierType modifierType,
            float value)
        {
            this.targetStat = targetStat;
            this.modifierType = modifierType;
            this.value = value;
        }
    }
}