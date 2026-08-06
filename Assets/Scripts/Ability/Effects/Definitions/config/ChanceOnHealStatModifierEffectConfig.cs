using System;
using Stat;
using UnityEngine;

namespace Effect
{
    [Serializable]
    public class ChanceOnHealStatModifierEffectConfig : EffectConfig
    {
        [Header("Heal Trigger")]
        [Range(0f, 1f)]
        [SerializeField]
        private float chance = 1f;

        [SerializeField]
        private HealTriggerTargetType triggerTargetType = HealTriggerTargetType.AnyAlly;

        [Header("Stat Modifier")]
        [SerializeField]
        private StatType statType = StatType.Attack;

        [SerializeField]
        private StatModifierType valueType = StatModifierType.Flat;

        [Tooltip("스탯에 더할 값. Percent 타입이면 퍼센트 값으로 해석한다.")]
        [SerializeField]
        private float value;

        public float Chance => chance;
        public HealTriggerTargetType TriggerTargetType => triggerTargetType;
        public StatType StatType => statType;
        public StatModifierType ValueType => valueType;
        public float Value => value;

#if UNITY_EDITOR
        public void ApplyEditorData(
            float chance,
            HealTriggerTargetType triggerTargetType,
            StatType statType,
            StatModifierType valueType,
            float value)
        {
            this.chance = chance;
            this.triggerTargetType = triggerTargetType;
            this.statType = statType;
            this.valueType = valueType;
            this.value = value;
        }
#endif
    }
}
