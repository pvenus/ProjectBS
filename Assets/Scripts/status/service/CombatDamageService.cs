using UnityEngine;
using Status.Dto;
using Status.Resolver;

namespace Status.Service
{
    [System.Serializable]
    public class DamageAmountDto
    {
        public float baseDamage;
        public float flatBonusDamage;
    }

    [System.Serializable]
    public class SkillContextDto
    {
        public string skillId;
        public DamageType damageType = DamageType.Normal;
    }

    [System.Serializable]
    public class ElementContextDto
    {
        public ElementType elementType = ElementType.None;

        // Fire heat 루프용 최소 파라미터
        public float heatCoefficient;
        public float heatGain;
        public bool canTriggerOverheat = true;
    }

    [System.Serializable]
    public class DamageModifierDto
    {
        public bool isCritical;
        public float criticalMultiplier = 1f;
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
        public ElementContextDto element = new ElementContextDto();
        public DamageModifierDto modifiers = new DamageModifierDto();
    }

    [System.Serializable]
    public class DamageResult
    {
        public float baseDamage;
        public float appliedDamage;
        public float bonusDamage;
        public bool triggeredOverheat;
        public bool targetDied;
    }

    /// <summary>
    /// 전투 1회 피해 처리 서비스.
    /// 현재는 평타/기본 Fire 루프(Heat 추뎀 + Overheat 체크) 중심의 최소 구현이다.
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

            CombatStatusMono status = request.target.GetComponent<CombatStatusMono>();
            StatMono stat = request.target.GetComponent<StatMono>();

            float baseDamage = request.damage != null ? request.damage.baseDamage : 0f;
            if (request.damage != null)
            {
                baseDamage += request.damage.flatBonusDamage;
            }

            float bonusDamage = 0f;

            // 1. Heat 기반 Fire 추가 데미지 계산
            if (request.element != null && request.element.elementType == ElementType.Fire && status != null)
            {
                HeatStateDto heatState = new HeatStateDto(
                    status.CurrentHeat,
                    status.MaxHeat,
                    0f,
                    0f
                );

                float bonusBaseDamage = request.damage != null ? request.damage.baseDamage : 0f;
                bonusDamage = HeatResolver.CalculateBasicAttackBonusDamage(
                    heatState,
                    bonusBaseDamage,
                    request.element.heatCoefficient
                );
            }

            // 2. 크리티컬 확장 포인트
            if (request.modifiers != null && request.modifiers.isCritical && request.modifiers.criticalMultiplier > 1f)
            {
                baseDamage *= request.modifiers.criticalMultiplier;
                bonusDamage *= request.modifiers.criticalMultiplier;
            }

            float finalDamage = baseDamage + bonusDamage;
            result.baseDamage = baseDamage;
            result.bonusDamage = bonusDamage;
            result.appliedDamage = finalDamage;

            // 3. 최종 피해 적용 (기본 데미지와 추뎀을 분리해서 호출)
            if (stat != null)
            {
                if (baseDamage > 0f)
                {
                    stat.TakeDamage(baseDamage);
                }

                if (!stat.IsDead && bonusDamage > 0f)
                {
                    stat.TakeDamage(bonusDamage);
                }

                result.targetDied = stat.IsDead;
            }

            // 4. Fire Heat 누적 및 Overheat 처리
            if (request.element != null && request.element.elementType == ElementType.Fire && status != null)
            {
                float heatGain = HeatResolver.CalculateHeatGain(request.element.heatGain);
                status.AddHeat(heatGain);

                if (request.element.canTriggerOverheat && status.CanTriggerOverheatExplosion())
                {
                    result.triggeredOverheat = true;

                    // TODO: OverheatExplosionService 연결
                    // 현재는 최소 루프 확인을 위해 즉시 Heat 초기화만 수행
                    status.ResetHeat();
                }
            }

            return result;
        }
    }
}