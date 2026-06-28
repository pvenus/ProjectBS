

using System;
using Skill;
using UnityEngine;

namespace Effect
{
    [Serializable]
    public class ChanceOnHitSkillEffectConfig : EffectConfig
    {
        [Header("Trigger")]
        [SerializeField]
        private float chance = 1f;

        [SerializeField]
        private bool requireCriticalHit;

        [Header("Skill")]
        [SerializeField]
        private EquipmentSkillSO skillSo;

        [Tooltip("0 이하면 기본 타겟 사용")]
        [SerializeField]
        private float rangeOverride = -1f;

        public float Chance => chance;
        public bool RequireCriticalHit => requireCriticalHit;
        public EquipmentSkillSO SkillSo => skillSo;
        public float RangeOverride => rangeOverride;

#if UNITY_EDITOR
        public void ApplyEditorData(
            float chance,
            bool requireCriticalHit,
            EquipmentSkillSO skillSo,
            float rangeOverride)
        {
            this.chance = chance;
            this.requireCriticalHit = requireCriticalHit;
            this.skillSo = skillSo;
            this.rangeOverride = rangeOverride;
        }
#endif
    }
}