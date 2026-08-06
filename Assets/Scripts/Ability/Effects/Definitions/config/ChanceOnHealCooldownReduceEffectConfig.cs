

using System;
using UnityEngine;

namespace Effect
{
    [Serializable]
    public class ChanceOnHealCooldownReduceEffectConfig : EffectConfig
    {
        [Header("Heal Trigger")]
        [Range(0f, 1f)]
        [SerializeField] private float chance = 1f;

        [SerializeField] private HealTriggerTargetType triggerTargetType = HealTriggerTargetType.AnyAlly;

        [Header("Cooldown Reduce")]
        [SerializeField] private CooldownReduceType reduceType = CooldownReduceType.FlatSeconds;

        [Range(0f, 1f)]
        [SerializeField] private float reducePercent;

        [Min(0f)]
        [SerializeField] private float reduceSeconds = 1f;

        public float Chance => chance;
        public HealTriggerTargetType TriggerTargetType => triggerTargetType;
        public CooldownReduceType ReduceType => reduceType;
        public float ReducePercent => reducePercent;
        public float ReduceSeconds => reduceSeconds;

        public void ApplyEditorData(
            float chance,
            HealTriggerTargetType triggerTargetType,
            CooldownReduceType reduceType,
            float reducePercent,
            float reduceSeconds)
        {
            this.chance = chance;
            this.triggerTargetType = triggerTargetType;
            this.reduceType = reduceType;
            this.reducePercent = reducePercent;
            this.reduceSeconds = reduceSeconds;
        }
    }
}