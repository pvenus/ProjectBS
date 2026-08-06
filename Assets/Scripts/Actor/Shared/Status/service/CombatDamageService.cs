using UnityEngine;
using Character;
using Stat;
using Skill;

namespace Status.Service
{
    [System.Serializable]
    public class DamageAmountDto
    {
        public float baseDamage;
        public float attackDamagePercent = 1f;
        public float flatBonusDamage;
    }

    [System.Serializable]
    public class SkillContextDto
    {
        public string skillId;
        public DamageType damageType = DamageType.Normal;
    }

    [System.Serializable]
    public class DamageModifierDto
    {
        public bool isCritical;
        public bool ignoreDefense;
    }

    [System.Serializable]
    public class DamageRequest
    {
        public GameObject attacker;
        public GameObject target;
        public Vector2 hitPoint;

        public DamageAmountDto damage = new DamageAmountDto();
        public SkillContextDto skill = new SkillContextDto();
        public DamageModifierDto modifiers = new DamageModifierDto();
    }

    [System.Serializable]
    public class DamageResult
    {
        public float baseDamage;
        public float appliedDamage;
        public float bonusDamage;
        public bool targetDied;
    }

    /// <summary>
    /// 전투 1회 피해 처리 서비스.
    /// 최종 피해는 baseDamage + 공격력 * attackPercentDamage + flatBonusDamage 기준으로 계산한다.
    /// </summary>
    public class CombatDamageService
    {
        public DamageResult Apply(DamageRequest request)
        {
            var result = new DamageResult();

            if (request == null || request.target == null)
            {
                return result;
            }

            CharacterManager characterManager =
                request.target.GetComponent<CharacterManager>();

            if (characterManager == null)
            {
                characterManager =
                    request.target.GetComponentInParent<CharacterManager>();
            }

            float attackerAttack =
                GetAttackerAttack(request);

            float baseDamage = 0f;

            if (request.damage != null)
            {
                baseDamage =
                    request.damage.baseDamage
                    + attackerAttack * request.damage.attackDamagePercent
                    + request.damage.flatBonusDamage;
            }

            float bonusDamage = 0f;

            // 크리티컬 확장 포인트.
            // 현재 criticalMultiplier는 DamageSO에서 제거했으므로 별도 계산하지 않는다.
            if (request.modifiers != null && request.modifiers.isCritical)
            {
                // TODO: 치명타 배율을 별도 시스템으로 분리할 경우 여기서 적용.
            }

            float finalDamage = baseDamage + bonusDamage;
            result.baseDamage = baseDamage;
            result.bonusDamage = bonusDamage;
            result.appliedDamage = finalDamage;

            // 3. 최종 피해 적용 (기본 데미지와 추뎀을 분리해서 호출)
            if (characterManager != null)
            {
                if (baseDamage > 0f)
                {
                    characterManager.TakeDamage(baseDamage);
                }

                bool isDead =
                    characterManager.RuntimeData != null
                    && characterManager.RuntimeData.isDead;

                if (!isDead && bonusDamage > 0f)
                {
                    characterManager.TakeDamage(bonusDamage);
                }

                result.targetDied =
                    characterManager.RuntimeData != null
                    && characterManager.RuntimeData.isDead;
            }

            return result;
        }

        private float GetAttackerAttack(DamageRequest request)
        {
            if (request == null || request.attacker == null)
            {
                return 0f;
            }

            CharacterManager attackerManager =
                request.attacker.GetComponent<CharacterManager>();

            if (attackerManager == null)
            {
                attackerManager =
                    request.attacker.GetComponentInParent<CharacterManager>();
            }

            if (attackerManager == null)
            {
                return 0f;
            }

            return attackerManager.GetStatValue(StatType.Attack);
        }
    }
}