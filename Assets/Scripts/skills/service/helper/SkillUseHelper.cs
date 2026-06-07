using Character;
using Effect;
using Effect.Helper;
using Skill;
using UnityEngine;

namespace Skill.Service.Helper
{
    /// <summary>
    /// 스킬 사용 시 공통 실행 로직을 모은 헬퍼.
    ///
    /// ActiveSkillService 같은 외부 스킬 실행 코드는 이 객체를 통해
    /// 투사체 생성과 시전자 자기 효과 적용을 처리한다.
    /// ProjectileEntity 내부 컴포넌트 동작은 ProjectileEntity.Initialize 이후 각 컴포넌트가 담당한다.
    /// </summary>
    public static class SkillUseHelper
    {
        private static ProjectileFactory projectileFactory;
        private static readonly EquipmentSkillResolver skillResolver = new();

        private static ProjectileFactory GetProjectileFactory()
        {
            if (projectileFactory == null)
            {
                projectileFactory = new ProjectileFactory();
            }

            return projectileFactory;
        }

        public static bool FireProjectiles(
            EquipmentSkillRuntimeData runtime,
            ProjectileRuntimeData[] projectileDatas,
            Vector2 targetPoint,
            bool usePoint)
        {
            ProjectileFactory factory = GetProjectileFactory();

            if (factory == null || runtime == null)
            {
                return false;
            }

            if (projectileDatas == null || projectileDatas.Length == 0)
            {
                return false;
            }

            bool firedAny = false;

            for (int i = 0; i < projectileDatas.Length; i++)
            {
                ProjectileRuntimeData projectileData = projectileDatas[i];

                if (projectileData == null)
                {
                    continue;
                }

                if (usePoint && projectileData.move != null)
                {
                    projectileData.move.targetPosition = targetPoint;
                }

                ProjectileEntity projectilePrefab = ResolveProjectilePrefab(
                    runtime,
                    projectileData);

                if (projectilePrefab == null)
                {
                    continue;
                }

                ProjectileEntity projectile = factory.SpawnOriented(
                    projectilePrefab,
                    projectileData);

                if (projectile != null)
                {
                    firedAny = true;
                }
            }

            return firedAny;
        }

        public static void ApplyCastSelfEffects(
            EquipmentSkillRuntimeData runtime,
            GameObject caster)
        {
            if (runtime == null || runtime.castSo == null || caster == null)
            {
                return;
            }

            SkillProjectileHitEffectEntry[] selfEffects = runtime.castSo.SelfEffects;

            if (selfEffects == null || selfEffects.Length == 0)
            {
                return;
            }

            EffectManager effectManager = ResolveEffectManager(caster);

            CharacterManager casterCharacter = caster.GetComponent<CharacterManager>();

            if (casterCharacter == null)
            {
                casterCharacter = caster.GetComponentInParent<CharacterManager>();
            }

            if (casterCharacter == null)
            {
                casterCharacter = caster.GetComponentInChildren<CharacterManager>();
            }

            if (effectManager == null)
            {
                return;
            }

            for (int i = 0; i < selfEffects.Length; i++)
            {
                SkillProjectileHitEffectEntry entry = selfEffects[i];

                if (entry == null || entry.effectSo == null)
                {
                    continue;
                }

                string sourceId = runtime.sourceEquipment != null
                    ? runtime.sourceEquipment.EquipmentId
                    : "Skill";

                EffectApplyHelper.ApplyEffect(
                    effectManager,
                    casterCharacter,
                    entry.effectSo,
                    EffectSourceType.Skill,
                    sourceId,
                    entry.lifetimeType,
                    entry.duration,
                    entry.categoryType,
                    ResolveEffectSourceTransform(entry.effectSo, caster),
                    Vector2.zero);
            }
        }

        public static bool UseSkillProjectilesAndSelfEffects(
            EquipmentSkillRuntimeData runtime,
            GameObject caster,
            ProjectileRuntimeData[] projectileDatas,
            Vector2 targetPoint,
            bool usePoint)
        {
            ApplyCastSelfEffects(
                runtime,
                caster);

            return FireProjectiles(
                runtime,
                projectileDatas,
                targetPoint,
                usePoint);
        }

        public static bool UseSkillProjectilesAndSelfEffects(
            EquipmentSkillRuntimeData runtime,
            Transform caster,
            Transform target,
            bool usePoint,
            Vector2 targetPoint)
        {
            if (runtime == null || caster == null)
            {
                return false;
            }

            Vector2 resolvedTargetPoint = ResolveTargetPoint(
                runtime,
                caster,
                target,
                usePoint,
                targetPoint);

            Vector2 spawnPosition = caster.position;
            Vector2 direction = ResolveDirection(
                spawnPosition,
                caster,
                target,
                true,
                resolvedTargetPoint);

            return UseSkillProjectilesAndSelfEffects(
                runtime,
                caster.gameObject,
                target != null ? target.gameObject : null,
                spawnPosition,
                direction,
                resolvedTargetPoint,
                usePoint);
        }

        public static bool UseSkillProjectilesAndSelfEffects(
            EquipmentSkillRuntimeData runtime,
            GameObject caster,
            GameObject target,
            Vector2 spawnPosition,
            Vector2 direction,
            Vector2 targetPoint,
            bool usePoint)
        {
            if (runtime == null || caster == null)
            {
                return false;
            }

            ProjectileRuntimeData[] projectileDatas =
                skillResolver.ResolveProjectileRuntime(
                    runtime,
                    caster,
                    target,
                    spawnPosition,
                    direction,
                    targetPoint);

            return UseSkillProjectilesAndSelfEffects(
                runtime,
                caster,
                projectileDatas,
                targetPoint,
                usePoint);
        }

        private static Vector2 ResolveTargetPoint(
            EquipmentSkillRuntimeData runtime,
            Transform caster,
            Transform target,
            bool usePoint,
            Vector2 targetPoint)
        {
            if (runtime == null || runtime.castSo == null)
            {
                return usePoint
                    ? targetPoint
                    : caster != null
                        ? (Vector2)caster.position
                        : Vector2.zero;
            }

            TargetingType targetingType = runtime.castSo.TargetingType;

            if (targetingType == TargetingType.Self ||
                targetingType == TargetingType.None)
            {
                return caster != null
                    ? (Vector2)caster.position
                    : Vector2.zero;
            }

            if (usePoint)
            {
                return targetPoint;
            }

            if (target != null)
            {
                return target.position;
            }

            return caster != null
                ? (Vector2)caster.position
                : Vector2.zero;
        }

        private static Vector2 ResolveDirection(
            Vector2 spawnPosition,
            Transform caster,
            Transform target,
            bool usePoint,
            Vector2 targetPoint)
        {
            Vector2 direction = caster != null
                ? (Vector2)caster.right
                : Vector2.right;

            if (target != null)
            {
                Vector2 toTarget = (Vector2)target.position - spawnPosition;

                if (toTarget.sqrMagnitude > 0.0001f)
                {
                    direction = toTarget.normalized;
                }
            }
            else if (usePoint)
            {
                Vector2 toPoint = targetPoint - spawnPosition;

                if (toPoint.sqrMagnitude > 0.0001f)
                {
                    direction = toPoint.normalized;
                }
            }

            return direction;
        }

        private static ProjectileEntity ResolveProjectilePrefab(
            EquipmentSkillRuntimeData runtime,
            ProjectileRuntimeData projectileData)
        {
            if (projectileData != null && projectileData.projectilePrefab != null)
            {
                return projectileData.projectilePrefab;
            }

            return runtime != null
                ? runtime.projectilePrefab
                : null;
        }

        private static EffectManager ResolveEffectManager(GameObject caster)
        {
            if (caster == null)
            {
                return null;
            }

            EffectManager effectManager = caster.GetComponent<EffectManager>();

            if (effectManager != null)
            {
                return effectManager;
            }

            effectManager = caster.GetComponentInParent<EffectManager>();

            if (effectManager != null)
            {
                return effectManager;
            }

            return caster.GetComponentInChildren<EffectManager>();
        }

        private static Transform ResolveEffectSourceTransform(
            EffectSO effectSo,
            GameObject fallback)
        {
            if (effectSo == null)
            {
                return fallback != null
                    ? fallback.transform
                    : null;
            }

            return fallback != null
                ? fallback.transform
                : null;
        }
    }
}