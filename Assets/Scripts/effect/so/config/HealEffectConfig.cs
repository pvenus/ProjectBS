

using System;
using UnityEngine;

namespace Effect
{
    [Serializable]
    public class HealEffectConfig : EffectConfig
    {
        [Header("Heal")]
        [SerializeField]
        private bool useMaxHpPercent = true;

        [Range(0f, 10f)]
        [SerializeField]
        private float maxHpPercent;

        [SerializeField]
        private float flatHealAmount;

        [Header("Attack Scaling")]
        [SerializeField]
        private bool useAttackScaling;

        [Range(0f, 20f)]
        [SerializeField]
        private float attackPercentHeal;

        [SerializeField]
        private bool clampToMaxHp = true;

        public bool UseMaxHpPercent => useMaxHpPercent;
        public float MaxHpPercent => maxHpPercent;
        public float FlatHealAmount => flatHealAmount;
        public bool UseAttackScaling => useAttackScaling;
        public float AttackPercentHeal => attackPercentHeal;
        public bool ClampToMaxHp => clampToMaxHp;

#if UNITY_EDITOR
        public void ApplyEditorData(
            bool useMaxHpPercent,
            float maxHpPercent,
            float flatHealAmount,
            bool useAttackScaling,
            float attackPercentHeal,
            bool clampToMaxHp)
        {
            this.useMaxHpPercent = useMaxHpPercent;
            this.maxHpPercent = maxHpPercent;
            this.flatHealAmount = flatHealAmount;
            this.useAttackScaling = useAttackScaling;
            this.attackPercentHeal = attackPercentHeal;
            this.clampToMaxHp = clampToMaxHp;
        }
#endif
    }
}