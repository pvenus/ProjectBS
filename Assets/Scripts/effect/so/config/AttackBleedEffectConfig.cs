

using System;
using UnityEngine;

namespace Effect
{
    [Serializable]
    public class AttackBleedEffectConfig : EffectConfig
    {
        [Header("Chance")]
        [Range(0f, 100f)]
        [SerializeField]
        private float chancePercent = 10f;

        [Header("Bleed")]
        [Tooltip("공격력의 몇 %를 초당 출혈 데미지로 적용할지 설정합니다.")]
        [Min(0f)]
        [SerializeField]
        private float attackRatioPercent = 10f;

        public float ChancePercent => chancePercent;
        public float AttackRatioPercent => attackRatioPercent;

#if UNITY_EDITOR
        public void ApplyEditorData(
            float chancePercent,
            float attackRatioPercent)
        {
            this.chancePercent = chancePercent;
            this.attackRatioPercent = attackRatioPercent;
        }
#endif
    }
}