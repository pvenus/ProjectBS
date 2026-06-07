using System;
using System.Collections.Generic;
using UnityEngine;
using Skills.Dto;
using Skill;
/// <summary>
/// 장비 스킬 런타임 조립용 최상위 Resolver.
/// 세부 계산은 Rune / Upgrade / Stat / Reflection helper에 위임한다.
/// </summary>
public class EquipmentSkillResolver
{
    private readonly EquipmentRuneResolver runeResolver = new EquipmentRuneResolver();
    private readonly EquipmentUpgradeResolver upgradeResolver = new EquipmentUpgradeResolver();
    private readonly EquipmentStatResolver statResolver = new EquipmentStatResolver();

    public EquipmentSkillRuntimeData Resolve(EquipmentSkillSO equipmentSo, EquipmentSkillInstanceData instanceData)
    {
        if (equipmentSo == null)
        {
            Debug.LogError("EquipmentSkillResolver.Resolve failed: equipmentSo is null.");
            return null;
        }

        EquipmentGrade resolvedGrade = instanceData != null
            ? instanceData.currentGrade
            : EquipmentGrade.Common;

        int resolvedRuneSlotCount = instanceData != null && instanceData.currentRuneSlotCount > 0
            ? instanceData.currentRuneSlotCount
            : 1;

        RuneRuntimeSetData runeRuntimeSet = runeResolver.Resolve(instanceData);
        EffectRuntimeSetData effectRuntimeSet = runeResolver.ResolveEffectRuntimeSet(runeRuntimeSet);
        EquipmentUpgradeRuntimeData upgradeRuntimeData = upgradeResolver.Resolve(equipmentSo, instanceData);

        List<SkillStatModifierRuntimeData> resolvedStatModifiers = statResolver.CombineStatModifiers(
            runeResolver.ExtractModifiers(runeRuntimeSet),
            upgradeResolver.ExtractModifiers(upgradeRuntimeData));

        AttackArchetype attackArchetype = GetAttackArchetype(equipmentSo);
        SkillType skillType = GetSkillType(equipmentSo);
        bool skipAttackAnimation = GetSkipAttackAnimation(equipmentSo);

        return new EquipmentSkillRuntimeData
        {
            sourceEquipment = equipmentSo,
            instanceData = instanceData,

            skillType = skillType,
            attackArchetype = attackArchetype,
            skipAttackAnimation = skipAttackAnimation,
            resolvedGrade = resolvedGrade,
            resolvedRuneSlotCount = Mathf.Max(1, resolvedRuneSlotCount),
            resolvedProjectileCount = statResolver.ResolveProjectileCount(equipmentSo, resolvedStatModifiers),
            resolvedProjectileSpreadAngle = statResolver.ResolveProjectileSpreadAngle(equipmentSo, resolvedStatModifiers),
            resolvedProjectileScale = statResolver.ResolveProjectileScale(equipmentSo, resolvedStatModifiers),

            castSo = equipmentSo.CastSo,
            hitSos = equipmentSo.HitSos,
            moveSo = equipmentSo.MoveSo,
            spawnSkillSo = equipmentSo.SpawnSkillSo,
            visualSetSo = equipmentSo.VisualSetSo,

            visualContext = BuildVisualContext(equipmentSo, attackArchetype, resolvedGrade),
            runeRuntimeSet = runeRuntimeSet,
            effectRuntimeSet = effectRuntimeSet,
            upgradeRuntimeData = upgradeRuntimeData,

            projectilePrefab = instanceData != null && instanceData.projectilePrefab != null
                ? instanceData.projectilePrefab
                : GetProjectilePrefab(equipmentSo)
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

        List<SkillStatModifierRuntimeData> resolvedStatModifiers = ResolveStatModifiers(runtime);
        Vector2 resolvedSpawnPosition = ResolveProjectileSpawnPosition(runtime, spawnPosition, direction);

        TargetingType targetingType = runtime.castSo != null
            ? runtime.castSo.TargetingType
            : TargetingType.None;

        float castRange = runtime.castSo != null
            ? runtime.castSo.Range
            : 0f;

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
            statResolver.GetProjectileSpawnInterval(runtime.sourceEquipment);

        baseProjectileData.projectileSpawnRadius =
            statResolver.GetProjectileSpawnRadius(runtime.sourceEquipment);
        baseProjectileData.move = CreateMoveDto(
            runtime.moveSo,
            target,
            targetingType,
            resolvedSpawnPosition,
            direction,
            resolvedTargetPosition);

        baseProjectileData.spawnSkillSo = runtime.spawnSkillSo;

        if (baseProjectileData.move != null)
        {
            baseProjectileData.move.maxProjectileCount = baseProjectileData.projectileCount;
        }

        ResolvedHitRuntimeData[] hitRuntimes =
            CreateHitRuntimeDatas(runtime, resolvedStatModifiers);

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
        List<SkillStatModifierRuntimeData> resolvedStatModifiers)
    {
        return new ProjectileRuntimeData
        {
            owner = owner,
            target = target,
            spawnPosition = resolvedSpawnPosition,
            direction = direction,
            targetingType = targetingType,
            lifetime = statResolver.ResolveProjectileLifetime(runtime, resolvedStatModifiers),
            projectilePrefab = runtime.projectilePrefab,
            projectileCount = Mathf.Max(1, runtime.resolvedProjectileCount),
            projectileSpreadAngle = Mathf.Max(0f, runtime.resolvedProjectileSpreadAngle),
            projectileScale = Mathf.Max(0.01f, runtime.resolvedProjectileScale),
            visualContext = runtime.visualContext,
            effectRuntimeSet = runtime.effectRuntimeSet,
            spawnSkillSo = runtime.spawnSkillSo
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
            target = source.target,
            spawnPosition = source.spawnPosition,
            direction = source.direction,
            targetingType = source.targetingType,
            lifetime = source.lifetime,
            projectilePrefab = source.projectilePrefab,
            projectileCount = source.projectileCount,
            projectileSpreadAngle = source.projectileSpreadAngle,
            projectileScale = source.projectileScale,
            projectileSpawnInterval = source.projectileSpawnInterval,
            projectileSpawnRadius = source.projectileSpawnRadius,
            move = source.move,
            damageProfile = source.damageProfile,
            visualContext = source.visualContext,
            effectRuntimeSet = source.effectRuntimeSet,
            spawnSkillSo = source.spawnSkillSo,
            spawnClip = source.spawnClip,
            hitClip = source.hitClip,
            despawnClip = source.despawnClip,
            sprite = source.sprite,
            material = source.material,
            color = source.color,
            useAnimatorTriggers = source.useAnimatorTriggers
        };
    }

    private List<SkillStatModifierRuntimeData> ResolveStatModifiers(EquipmentSkillRuntimeData runtime)
    {
        if (runtime == null)
        {
            return new List<SkillStatModifierRuntimeData>();
        }

        return statResolver.CombineStatModifiers(
            runeResolver.ExtractModifiers(runtime.runeRuntimeSet),
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

    private SkillProjectileMoveDto CreateMoveDto(
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

        return moveSo.CreateDto(targetTransform, startPosition, targetPosition);
    }


    private class ResolvedHitRuntimeData
    {
        public SkillProjectileHitDto hit;
        public SkillDamageProfileDto damageProfile;
    }

    private ResolvedHitRuntimeData[] CreateHitRuntimeDatas(
        EquipmentSkillRuntimeData runtime,
        List<SkillStatModifierRuntimeData> resolvedStatModifiers)
    {
        if (runtime?.hitSos == null || runtime.hitSos.Length == 0)
        {
            return Array.Empty<ResolvedHitRuntimeData>();
        }

        List<ResolvedHitRuntimeData> results = new();

        for (int i = 0; i < runtime.hitSos.Length; i++)
        {
            SkillHitSO hitSo = runtime.hitSos[i];

            if (hitSo == null)
            {
                continue;
            }

            SkillDamageProfileDto damageProfile =
                CreateDamageProfileDto(
                    runtime,
                    hitSo,
                    resolvedStatModifiers);

            SkillProjectileHitDto hitDto =
                ReflectionHelper.TryInvokeCreateDto<SkillProjectileHitDto>(
                    hitSo,
                    new object[] { damageProfile, 0f });

            if (hitDto == null)
            {
                hitDto = ReflectionHelper.TryInvokeCreateDto<SkillProjectileHitDto>(
                    hitSo,
                    new object[] { damageProfile });
            }

            if (hitDto == null)
            {
                hitDto = ReflectionHelper.TryInvokeCreateDto<SkillProjectileHitDto>(hitSo);
            }

            if (hitDto != null)
            {
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
        IEnumerable<SkillStatModifierRuntimeData> resolvedStatModifiers)
    {
        if (runtime == null)
        {
            return null;
        }

        SkillDamageSO damageSo = statResolver.GetDamageSo(hitSo);

        SkillDamageProfileDto dto =
            ReflectionHelper.TryInvokeCreateDto<SkillDamageProfileDto>(damageSo);

        if (dto == null)
        {
            dto = new SkillDamageProfileDto();
        }

        EquipmentSkillSO equipmentSo = runtime.sourceEquipment;

        dto.damageType = statResolver.GetDamageType(damageSo);
        dto.baseDamage = statResolver.ResolveBaseDamage(
            damageSo,
            resolvedStatModifiers);
        dto.attackDamagePercent = statResolver.GetAttackPercentDamage(damageSo);
        dto.canCritical = statResolver.GetCanCritical(damageSo);
        dto.ignoreDefense = statResolver.GetIgnoreDefense(damageSo);

        dto.projectileSpawnInterval =
            statResolver.GetProjectileSpawnInterval(equipmentSo);
        dto.projectileSpawnRadius =
            statResolver.GetProjectileSpawnRadius(equipmentSo);
        return dto;
    }

    private void ResolveProjectileVisualRuntime(EquipmentSkillRuntimeData runtime, ProjectileRuntimeData projectileData)
    {
        if (runtime == null || projectileData == null)
        {
            return;
        }

        SkillVisualSetSO visualSet = runtime.visualSetSo;
        if (visualSet == null)
        {
            return;
        }

        BaseVisualSO baseVisual = visualSet.BaseVisualSo;

        projectileData.spawnClip = baseVisual != null
            ? baseVisual.ProjectileLoopClip
            : null;

        projectileData.hitClip = baseVisual != null
            ? baseVisual.HitClip
            : null;

        projectileData.despawnClip = null;

        projectileData.sprite = baseVisual != null
            ? baseVisual.BaseSprite
            : null;

        projectileData.material = null;
        projectileData.color = Color.white;
        projectileData.useAnimatorTriggers = projectileData.spawnClip == null
            && projectileData.hitClip == null
            && projectileData.despawnClip == null;
    }

    private ResolvedVisualContextDto BuildVisualContext(
        EquipmentSkillSO equipmentSo,
        AttackArchetype archetype,
        EquipmentGrade grade)
    {
        return new ResolvedVisualContextDto
        {
            attackArchetype = archetype,
            equipmentGrade = grade,
            baseVisualId = equipmentSo.VisualSetSo != null && equipmentSo.VisualSetSo.BaseVisualSo != null
                ? equipmentSo.VisualSetSo.BaseVisualSo.VisualId
                : string.Empty,
            mainVisualId = string.Empty
        };
    }

    private bool GetSkipAttackAnimation(EquipmentSkillSO equipmentSo)
    {
        return equipmentSo != null &&
               equipmentSo.BaseProfileSo != null &&
               equipmentSo.BaseProfileSo.SkipAttackAnimation;
    }

    private SkillType GetSkillType(EquipmentSkillSO equipmentSo)
    {
        if (equipmentSo != null && equipmentSo.BaseProfileSo != null)
        {
            return equipmentSo.BaseProfileSo.SkillType;
        }

        return SkillType.Active;
    }

    private AttackArchetype GetAttackArchetype(EquipmentSkillSO equipmentSo)
    {
        if (equipmentSo != null && equipmentSo.BaseProfileSo != null)
        {
            return equipmentSo.BaseProfileSo.AttackArchetype;
        }

        return AttackArchetype.Melee;
    }

    private ProjectileEntity GetProjectilePrefab(EquipmentSkillSO equipmentSo)
    {
        if (equipmentSo != null && equipmentSo.BaseProfileSo != null)
        {
            return equipmentSo.BaseProfileSo.ProjectilePrefab;
        }

        return null;
    }
}
