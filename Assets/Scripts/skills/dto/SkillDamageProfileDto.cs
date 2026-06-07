using System;
using UnityEngine;
using Status.Service;
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
        public float attackDamagePercent = 1f;
        public float projectileSpawnInterval;
        public float projectileSpawnRadius;

        [Header("Critical / Modifiers")]
        public bool canCritical;
        public bool ignoreDefense;

        /// <summary>
        /// 현재 프로필 기준으로 DamageRequest를 생성한다.
        /// </summary>
        public DamageRequest CreateRequest(GameObject attacker, GameObject target, Vector2 hitPoint)
        {
            return new DamageRequest
            {
                attacker = attacker,
                target = target,
                hitPoint = hitPoint,
                damage = new DamageAmountDto
                {
                    baseDamage = baseDamage,
                    attackDamagePercent = attackDamagePercent
                },
                skill = new SkillContextDto
                {
                    skillId = skillId,
                    damageType = damageType
                },
                modifiers = new DamageModifierDto
                {
                    isCritical = false,
                    ignoreDefense = ignoreDefense
                }
            };
        }

        /// <summary>
        /// 크리티컬 여부를 외부에서 결정한 뒤 DamageRequest를 생성한다.
        /// </summary>
        public DamageRequest CreateRequest(GameObject attacker, GameObject target, Vector2 hitPoint, bool isCritical)
        {
            DamageRequest request = CreateRequest(attacker, target, hitPoint);

            if (request.modifiers != null)
            {
                request.modifiers.isCritical = canCritical && isCritical;
            }

            return request;
        }

        /// <summary>
        /// 기본 공격용 빠른 생성 헬퍼.
        /// </summary>
        public static SkillDamageProfileDto CreateBasicAttack(
            string skillId,
            float baseDamage,
            float attackDamagePercent)
        {
            return new SkillDamageProfileDto
            {
                skillId = skillId,
                damageType = DamageType.Normal,
                baseDamage = baseDamage,
                attackDamagePercent = attackDamagePercent
            };
        }
    }
}