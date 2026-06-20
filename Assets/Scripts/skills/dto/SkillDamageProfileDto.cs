using System;
using UnityEngine;
using Skill;
namespace Skills.Dto
{
    /// <summary>
    /// 스킬이 전투 시스템에 전달할 데미지 프로필 DTO.
    /// 스킬/평타/투사체 등이 공통 형태로 DamageRequest를 만들 수 있도록 한다.
    /// </summary>
    [Serializable]
    public class SkillDamageProfileDto
    {
        [Header("Skill")]
        public string skillId;
        public DamageType damageType = DamageType.Normal;

        [Header("Damage")]
        public float baseDamage;
        public float firstHitBaseDamage;
        public float attackDamagePercent = 1f;

        [Header("Critical / Modifiers")]
        public bool canCritical;
        public bool ignoreDefense;
    }
}