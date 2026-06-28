

using System;
using UnityEngine;

namespace Effect
{
    [Serializable]
    public class CooldownReduceEffectConfig : EffectConfig
    {
        [SerializeField]
        private CooldownReduceType reduceType = CooldownReduceType.Percent;

        [SerializeField]
        private float reducePercent = 0.2f;

        [SerializeField]
        private float reduceSeconds;

        public CooldownReduceType ReduceType => reduceType;
        public float ReducePercent => reducePercent;
        public float ReduceSeconds => reduceSeconds;

        public void ApplyEditorData(
            CooldownReduceType reduceType,
            float reducePercent,
            float reduceSeconds)
        {
            this.reduceType = reduceType;
            this.reducePercent = reducePercent;
            this.reduceSeconds = reduceSeconds;
        }
    }
}