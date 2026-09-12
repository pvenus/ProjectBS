

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

        [SerializeField, Min(0f)]
        private float rootHardCap = 0.2f;

        public StatType TargetStat => targetStat;
        public StatModifierType ModifierType => modifierType;
        public float Value => value;
        public float RootHardCap => rootHardCap > 0f ? rootHardCap : 0.2f;

        public void ApplyEditorData(
            StatType targetStat,
            StatModifierType modifierType,
            float value,
            float rootHardCap = 0.2f)
        {
            this.targetStat = targetStat;
            this.modifierType = modifierType;
            this.value = value;
            this.rootHardCap = Mathf.Max(0f, rootHardCap);
        }
    }
}
