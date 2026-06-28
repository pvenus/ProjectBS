using System;
using System.Collections.Generic;
using UnityEngine;
using Skills.Dto;
using Skills.Dto.Move;
using Skills.Move.Config;
using Skill;
using Effect;
using Effect.Helper;
/// <summary>
/// 장비 스킬 런타임 조립용 최상위 Resolver.
/// 세부 계산은 Rune / Upgrade / Stat / Reflection helper에 위임한다.
/// </summary>
public class EquipmentSkillResolver
{
    private readonly EquipmentRuneResolver runeResolver = new EquipmentRuneResolver();
    private readonly EquipmentUpgradeResolver upgradeResolver = new EquipmentUpgradeResolver();
    private readonly EquipmentStatResolver statResolver = new EquipmentStatResolver();
    private readonly EffectResolver effectResolver = new EffectResolver();

    public EquipmentSkillRuntimeData Resolve(EquipmentSkillSO equipmentSo, EquipmentSkillInstanceData instanceData)
    {
        if (equipmentSo == null)
        {
            Debug.LogError("EquipmentSkillResolver.Resolve failed: equipmentSo is null.");
            return null;
        }
        EquipmentUpgradeRuntimeData upgradeRuntimeData = upgradeResolver.Resolve(equipmentSo, instanceData);

        List<SkillStatModifierData> resolvedStatModifiers = statResolver.CombineStatModifiers(
            upgradeResolver.ExtractModifiers(upgradeRuntimeData));

        return new EquipmentSkillRuntimeData
        {
            sourceEquipment = equipmentSo,
            instanceData = instanceData,

            resolvedLevel = instanceData != null
                ? Mathf.Max(1, instanceData.currentLevel)
                : 1,
            resolvedRange = statResolver.ResolveStat(
                equipmentSo,
                SkillStatModifierType.Range,
                resolvedStatModifiers),
            resolvedBurstCount = statResolver.ResolveBurstCount(equipmentSo, resolvedStatModifiers),
            resolvedBurstInterval = statResolver.ResolveBurstInterval(equipmentSo, resolvedStatModifiers),
            resolvedProjectileCount = statResolver.ResolveProjectileCount(equipmentSo, resolvedStatModifiers),
            resolvedProjectileSpreadAngle = statResolver.ResolveProjectileSpreadAngle(equipmentSo, resolvedStatModifiers),
            resolvedProjectileArrangementValue = statResolver.GetProjectileArrangementValue(equipmentSo),
            resolvedProjectileScale = statResolver.ResolveProjectileScale(equipmentSo, resolvedStatModifiers),

            visualContext = BuildVisualContext(equipmentSo),
            upgradeRuntimeData = upgradeRuntimeData
        };
    }

    public ProjectileRuntimeData[] ResolveProjectileRuntime(
        EquipmentSkillRuntimeData runtime,
        GameObject owner,
        GameObject target,
        Vector2 spawnPosition,
        Vector2 direction,
        Vector2? explicitTargetPosition = null)
    {
        if (runtime == null)
        {
            Debug.LogError("EquipmentSkillResolver.ResolveProjectileRuntime failed: runtime is null.");
            return Array.Empty<ProjectileRuntimeData>();
        }

        List<SkillStatModifierData> resolvedStatModifiers = ResolveStatModifiers(runtime);
        Vector2 resolvedSpawnPosition = ResolveProjectileSpawnPosition(runtime, spawnPosition, direction);

        EquipmentSkillSO equipmentSo = runtime.sourceEquipment;
        SkillCastSO castSo = equipmentSo != null
            ? equipmentSo.CastSo
            : null;

        TargetingType targetingType = castSo != null
            ? castSo.TargetingType
            : TargetingType.None;

        float castRange = statResolver.ResolveStat(
            equipmentSo,
            SkillStatModifierType.Range,
            resolvedStatModifiers);

        Vector2 resolvedTargetPosition = ResolveProjectileTargetPosition(
            targetingType,
            target,
            resolvedSpawnPosition,
            direction,
            castRange,
            explicitTargetPosition);

        ProjectileRuntimeData baseProjectileData = CreateBaseProjectileRuntimeData(
            runtime,
            owner,
            target,
            resolvedSpawnPosition,
            direction,
            targetingType,
            resolvedStatModifiers);

        baseProjectileData.projectileSpawnInterval =
            statResolver.ResolveStat(
                equipmentSo,
                SkillStatModifierType.ProjectileSpawnInterval,
                resolvedStatModifiers);

        baseProjectileData.projectileSpawnRadius =
            statResolver.ResolveStat(
                equipmentSo,
                SkillStatModifierType.ProjectileSpawnRadius,
                resolvedStatModifiers);

        baseProjectileData.moveRuntime = CreateMoveRuntimeDto(
            equipmentSo != null ? equipmentSo.MoveSo : null,
            target,
            targetingType,
            resolvedSpawnPosition,
            direction,
            resolvedTargetPosition);

        baseProjectileData.spawnSkillSo = equipmentSo != null
            ? equipmentSo.SpawnSkillSo
            : null;

        ResolvedHitRuntimeData[] hitRuntimes =
            CreateHitRuntimeDatas(
                runtime,
                resolvedStatModifiers,
                owner,
                target);

        if (hitRuntimes == null || hitRuntimes.Length == 0)
        {
            baseProjectileData.hit = null;
            baseProjectileData.damageProfile = null;
            ResolveProjectileVisualRuntime(runtime, baseProjectileData);
            return new[] { baseProjectileData };
        }

        ProjectileRuntimeData[] projectileDatas = new ProjectileRuntimeData[hitRuntimes.Length];

        for (int i = 0; i < hitRuntimes.Length; i++)
        {
            ProjectileRuntimeData projectileData = CloneProjectileRuntimeData(baseProjectileData);
            projectileData.hit = hitRuntimes[i].hit;
            projectileData.damageProfile = hitRuntimes[i].damageProfile;
            ResolveProjectileVisualRuntime(runtime, projectileData);
            projectileDatas[i] = projectileData;
        }

        return projectileDatas;
    }

    private ProjectileRuntimeData CreateBaseProjectileRuntimeData(
        EquipmentSkillRuntimeData runtime,
        GameObject owner,
        GameObject target,
        Vector2 resolvedSpawnPosition,
        Vector2 direction,
        TargetingType targetingType,
        List<SkillStatModifierData> resolvedStatModifiers)
    {
        EquipmentSkillSO equipmentSo = runtime?.sourceEquipment;
        return new ProjectileRuntimeData
        {
            owner = owner,
            sourceEquipment = equipmentSo,
            target = target,
            spawnPosition = resolvedSpawnPosition,
            direction = direction,
            lifetime = statResolver.ResolveProjectileLifetime(runtime, resolvedStatModifiers),
            projectileCount = Mathf.Max(1, runtime.resolvedProjectileCount),
            projectileSpreadAngle = Mathf.Max(0f, runtime.resolvedProjectileSpreadAngle),
            projectileArrangementValue = Mathf.Max(0f, runtime.resolvedProjectileArrangementValue),
            projectileScale = Mathf.Max(0.01f, runtime.resolvedProjectileScale),
            visualContext = runtime.visualContext,
            spawnSkillSo = equipmentSo != null
                ? equipmentSo.SpawnSkillSo
                : null
        };
    }

    private ProjectileRuntimeData CloneProjectileRuntimeData(
        ProjectileRuntimeData source)
    {
        if (source == null)
        {
            return null;
        }

        return new ProjectileRuntimeData
        {
            owner = source.owner,
            sourceEquipment = source.sourceEquipment,
            target = source.target,
            spawnPosition = source.spawnPosition,
            direction = source.direction,
            lifetime = source.lifetime,
            projectileCount = source.projectileCount,
            projectileSpreadAngle = source.projectileSpreadAngle,
            projectileArrangementValue = source.projectileArrangementValue,
            projectileScale = source.projectileScale,
            projectileSpawnInterval = source.projectileSpawnInterval,
            projectileSpawnRadius = source.projectileSpawnRadius,
            moveRuntime = source.moveRuntime,
            damageProfile = source.damageProfile,
            visualContext = source.visualContext,
            effectRuntimeSet = source.effectRuntimeSet,
            spawnSkillSo = source.spawnSkillSo,
            projectileVisualType = source.projectileVisualType,
            material = source.material,
            color = source.color,
            useAnimatorTriggers = source.useAnimatorTriggers
        };
    }

    private List<SkillStatModifierData> ResolveStatModifiers(EquipmentSkillRuntimeData runtime)
    {
        if (runtime == null)
        {
            return new List<SkillStatModifierData>();
        }

        return statResolver.CombineStatModifiers(
            upgradeResolver.ExtractModifiers(runtime.upgradeRuntimeData));
    }

    private Vector2 ResolveProjectileSpawnPosition(
        EquipmentSkillRuntimeData runtime,
        Vector2 origin,
        Vector2 direction)
    {
        if (runtime == null || runtime.sourceEquipment == null)
        {
            return origin;
        }

        float spawnOffset = statResolver.GetProjectileSpawnOffset(runtime.sourceEquipment);
        if (Mathf.Abs(spawnOffset) <= 0.0001f)
        {
            return origin;
        }

        Vector2 forward = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;
        return origin + (forward * spawnOffset);
    }

    private Vector2 ResolveProjectileTargetPosition(
        TargetingType targetingType,
        GameObject target,
        Vector2 startPosition,
        Vector2 direction,
        float range,
        Vector2? explicitTargetPosition)
    {
        Vector2 forward = direction.sqrMagnitude > 0.0001f
            ? direction.normalized
            : Vector2.right;

        float distance = range > 0.0001f
            ? range
            : 1f;

        switch (targetingType)
        {
            case TargetingType.AutoTarget:
                if (target != null)
                {
                    return target.transform.position;
                }

                return explicitTargetPosition ?? startPosition + forward * distance;

            case TargetingType.AutoTargetDirection:
            case TargetingType.Directional:
                return startPosition + forward * distance;

            case TargetingType.Position:
                if (explicitTargetPosition.HasValue)
                {
                    return explicitTargetPosition.Value;
                }

                if (target != null)
                {
                    return target.transform.position;
                }

                return startPosition + forward * distance;

            default:
                return explicitTargetPosition ?? startPosition + forward * distance;
        }
    }

    private SkillMoveRuntimeDto CreateMoveRuntimeDto(
        SkillMoveSO moveSo,
        GameObject target,
        TargetingType targetingType,
        Vector2 startPosition,
        Vector2 direction,
        Vector2 targetPosition)
    {
        if (moveSo == null)
        {
            return null;
        }

        Transform targetTransform = targetingType == TargetingType.AutoTarget && target != null
            ? target.transform
            : null;

        SkillMoveConfig moveConfig =
            moveSo.Config ?? CreateDefaultMoveConfig(moveSo.MoveType);

        if (moveConfig == null)
        {
            return null;
        }

        SkillMoveRuntimeDto runtimeDto = moveConfig.CreateMoveDto(
            targetTransform,
            startPosition,
            targetPosition);

        if (runtimeDto != null)
        {
            runtimeDto.applyDirectionRotation = moveSo.ApplyDirectionRotation;
            runtimeDto.rotationOffset = moveSo.RotationOffset;
        }

        return runtimeDto;
    }

    private static SkillMoveConfig CreateDefaultMoveConfig(
        ProjectileMoveType moveType)
    {
        return moveType switch
        {
            ProjectileMoveType.Linear => new LinearMoveConfig(),
            ProjectileMoveType.Hover => new HoverMoveConfig(),
            ProjectileMoveType.Warp => new WarpMoveConfig(),
            ProjectileMoveType.Homing => new HomingMoveConfig(),
            ProjectileMoveType.Orbit => new OrbitMoveConfig(),
            _ => null
        };
    }


    private SkillProjectileHitDto CreateHitDto(
        SkillHitSO hitSo,
        SkillDamageProfileDto resolvedDamageProfile,
        GameObject owner,
        GameObject target,
        int resolvedMaxHitCount,
        IReadOnlyList<EffectUpgradeModifierData> effectUpgradeModifiers = null)
    {
        if (hitSo == null)
        {
            return null;
        }

        return new SkillProjectileHitDto
        {
            maxHitCount = Mathf.Max(1, resolvedMaxHitCount),
            ignoreSameRoot = hitSo.IgnoreSameRoot,
            useRepeatInterval = hitSo.UseRepeatInterval,
            repeatInterval = Mathf.Max(0f, hitSo.RepeatInterval),
            hitStartTime = Mathf.Max(0f, hitSo.HitStartTime),
            deactivateAfterFirstHit = hitSo.DeactivateAfterFirstHit,
            targetLayerMask = hitSo.TargetLayerMask,
            damageProfile = resolvedDamageProfile,
            spawnSkill = hitSo.SpawnSkill,
            buffEffects = effectResolver.ResolveEntries(
                hitSo.BuffEffects,
                owner,
                target,
                EffectCategoryType.Buff,
                effectUpgradeModifiers),
            debuffEffects = effectResolver.ResolveEntries(
                hitSo.DebuffEffects,
                owner,
                target,
                EffectCategoryType.Debuff,
                effectUpgradeModifiers),
            splitHitCount = Mathf.Max(1, hitSo.SplitHitCount),
            splitHitInterval = Mathf.Max(0f, hitSo.SplitHitInterval),
        };
    }


    private class ResolvedHitRuntimeData
    {
        public SkillProjectileHitDto hit;
        public SkillDamageProfileDto damageProfile;
    }

    private ResolvedHitRuntimeData[] CreateHitRuntimeDatas(
        EquipmentSkillRuntimeData runtime,
        List<SkillStatModifierData> resolvedStatModifiers,
        GameObject owner,
        GameObject target)
    {
        EquipmentSkillSO equipmentSo = runtime?.sourceEquipment;
        SkillHitSO[] hitSos = equipmentSo != null
            ? equipmentSo.HitSos
            : null;

        if (hitSos == null || hitSos.Length == 0)
        {
            return Array.Empty<ResolvedHitRuntimeData>();
        }

        List<ResolvedHitRuntimeData> results = new();

        for (int i = 0; i < hitSos.Length; i++)
        {
            SkillHitSO hitSo = hitSos[i];

            if (hitSo == null)
            {
                continue;
            }
            SkillDamageProfileDto damageProfile =
                CreateDamageProfileDto(
                    runtime,
                    hitSo,
                    resolvedStatModifiers);

            float projectileColliderRadius =
                statResolver.ResolveStat(
                    equipmentSo,
                    SkillStatModifierType.ProjectileColliderRadius,
                    resolvedStatModifiers);

            int resolvedMaxHitCount =
                statResolver.ResolveIntStat(
                    equipmentSo,
                    SkillStatModifierType.MaxHitCount,
                    resolvedStatModifiers);

            SkillProjectileHitDto hitDto = CreateHitDto(
                hitSo,
                damageProfile,
                owner,
                target,
                resolvedMaxHitCount,
                null);

            if (hitDto != null)
            {
                hitDto.projectileColliderRadius = projectileColliderRadius;

                results.Add(new ResolvedHitRuntimeData
                {
                    hit = hitDto,
                    damageProfile = damageProfile
                });
            }
        }

        return results.ToArray();
    }

    private SkillDamageProfileDto CreateDamageProfileDto(
        EquipmentSkillRuntimeData runtime,
        SkillHitSO hitSo,
        IEnumerable<SkillStatModifierData> resolvedStatModifiers)
    {
        if (runtime == null || hitSo == null)
        {
            return null;
        }

        EquipmentSkillSO equipmentSo = runtime.sourceEquipment;

        SkillDamageProfileDto dto = new SkillDamageProfileDto
        {
            damageType = statResolver.GetDamageType(hitSo),
            baseDamage = equipmentSo != null
                ? statResolver.ResolveStat(
                    equipmentSo,
                    SkillStatModifierType.BaseDamage,
                    resolvedStatModifiers)
                : statResolver.GetBaseDamage(hitSo),
            attackDamagePercent = equipmentSo != null
                ? statResolver.ResolveStat(
                    equipmentSo,
                    SkillStatModifierType.AttackPercentDamage,
                    resolvedStatModifiers)
                : statResolver.GetAttackPercentDamage(hitSo),
            canCritical = statResolver.GetCanCritical(hitSo),
            ignoreDefense = statResolver.GetIgnoreDefense(hitSo)
        };

        return dto;
    }

    private void ResolveProjectileVisualRuntime(EquipmentSkillRuntimeData runtime, ProjectileRuntimeData projectileData)
    {
        if (runtime == null || projectileData == null)
        {
            return;
        }

        BaseVisualSO baseVisual = runtime.sourceEquipment != null
            ? runtime.sourceEquipment.BaseVisualSo
            : null;

        projectileData.projectileVisualType = baseVisual != null
            ? baseVisual.ProjectileVisualType
            : ProjectileVisualType.Default;

        projectileData.material = null;
        projectileData.color = Color.white;
        projectileData.useAnimatorTriggers = baseVisual == null ||
                                             baseVisual.AnimationClips == null ||
                                             baseVisual.AnimationClips.Length == 0;
    }

    private ResolvedVisualContextDto BuildVisualContext(
        EquipmentSkillSO equipmentSo)
    {
        BaseVisualSO baseVisual = equipmentSo != null
            ? equipmentSo.BaseVisualSo
            : null;

        return new ResolvedVisualContextDto
        {
            baseVisualId = baseVisual != null
                ? baseVisual.VisualId
                : string.Empty,
            mainVisualId = string.Empty
        };
    }
}