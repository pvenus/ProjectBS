using System;
using UnityEngine;

namespace Effect
{
    [Serializable]
    public class OnHitPoisonDotEffectConfig : EffectConfig
    {
        [SerializeField] private float chancePercent;
        [SerializeField] private float attackRatioPercentPerTick;
        [SerializeField] private float tickIntervalSeconds = 1f;
        [SerializeField] private float durationSeconds;

        public float ChancePercent => chancePercent;
        public float AttackRatioPercentPerTick => attackRatioPercentPerTick;
        public float TickIntervalSeconds => tickIntervalSeconds;
        public float DurationSeconds => durationSeconds;

        public void ApplyEditorData(
            float chancePercent,
            float attackRatioPercentPerTick,
            float tickIntervalSeconds,
            float durationSeconds)
        {
            this.chancePercent = chancePercent;
            this.attackRatioPercentPerTick = attackRatioPercentPerTick;
            this.tickIntervalSeconds = tickIntervalSeconds;
            this.durationSeconds = durationSeconds;
        }
    }
}
