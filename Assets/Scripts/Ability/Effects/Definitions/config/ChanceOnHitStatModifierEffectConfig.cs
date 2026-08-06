

using System;
using Stat;
using UnityEngine;

namespace Effect
{
    [Serializable]
    public class ChanceOnHitStatModifierEffectConfig : EffectConfig
    {
        [Header("Chance")]
        [Range(0f, 100f)]
        [SerializeField]
        private float chancePercent = 10f;

        [Header("Stat Modifier")]
        [SerializeField]
        private StatType statType = StatType.Attack;

        [SerializeField]
        private StatModifierType valueType = StatModifierType.Percent;

        [SerializeField]
        private float value = -15f;

        public float ChancePercent => chancePercent;
        public StatType StatType => statType;
        public StatModifierType ValueType => valueType;
        public float Value => value;

#if UNITY_EDITOR
        public void ApplyEditorData(
            float chancePercent,
            StatType statType,
            StatModifierType valueType,
            float value)
        {
            this.chancePercent = chancePercent;
            this.statType = statType;
            this.valueType = valueType;
            this.value = value;
        }
#endif
    }
}