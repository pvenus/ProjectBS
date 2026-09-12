using System;
using System.Collections.Generic;
using UnityEngine;
using Skills.Dto;
using Skills.Dto.Move;
using Skills.Move.Config;
using Skill;
using Effect;
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
            resolvedRendererScale = equipmentSo.BaseProfileSo != null
                ? equipmentSo.BaseProfileSo.RendererScale
                : 1f,

            visualContext = BuildVisualContext(equipmentSo),
            upgradeRuntimeData = upgradeRuntimeData,
            comboProfile = equipmentSo.ComboProfile
            ,resolvedMouse3Profile = ResolveMouse3Profile(equipmentSo, resolvedStatModifiers)
        };
    }

    private ResolvedMouse3SkillProfile ResolveMouse3Profile(
        EquipmentSkillSO equipmentSo, List<SkillStatModifierData> modifiers)
    {
        Mouse3SkillProfile source = equipmentSo?.Mouse3Profile;
        if (source == null || !source.Enabled) return null;
        return new ResolvedMouse3SkillProfile
        {
            crowdControlKind = source.CrowdControlKind,
            distance = statResolver.ResolveStat(equipmentSo, SkillStatModifierType.Mouse3Distance, modifiers),
            duration = statResolver.ResolveStat(equipmentSo, SkillStatModifierType.Mouse3CrowdControlDuration, modifiers),
            stopRadius = source.StopRadius,
            collisionSafe = source.CollisionSafe,
            normalRatio = statResolver.ResolveStat(equipmentSo, SkillStatModifierType.Mouse3NormalRatio, modifiers),
            eliteRatio = statResolver.ResolveStat(equipmentSo, SkillStatModifierType.Mouse3EliteRatio, modifiers),
            bossRatio = statResolver.ResolveStat(equipmentSo, SkillStatModifierType.Mouse3BossRatio, modifiers),
            bossHardCap = statResolver.ResolveStat(equipmentSo, SkillStatModifierType.Mouse3BossHardCap, modifiers),
            fanAngle = source.FanAngle > 0f
                ? statResolver.ResolveStat(equipmentSo, SkillStatModifierType.Mouse3FanAngle, modifiers)
                : 0f,
            burstCount = statResolver.ResolveBurstCount(equipmentSo, modifiers),
            burstInterval = statResolver.ResolveBurstInterval(equipmentSo, modifiers),
            nextBasicForwardRatio = statResolver.ResolveStat(equipmentSo, SkillStatModifierType.Mouse3NextBasicForwardRatio, modifiers),
            nextBasicRangeRatio = statResolver.ResolveStat(equipmentSo, SkillStatModifierType.Mouse3NextBasicRangeRatio, modifiers),
            nextBasicInputGrace = statResolver.ResolveStat(equipmentSo, SkillStatModifierType.Mouse3NextBasicInputGrace, modifiers),
            gatherDistance = statResolver.ResolveStat(equipmentSo, SkillStatModifierType.Mouse3GatherDistance, modifiers),
            gatherDuration = statResolver.ResolveStat(equipmentSo, SkillStatModifierType.Mouse3GatherDuration, modifiers),
            stunNormalDuration = statResolver.ResolveStat(equipmentSo, SkillStatModifierType.Mouse3StunNormalDuration, modifiers),
            stunEliteDuration = statResolver.ResolveStat(equipmentSo, SkillStatModifierType.Mouse3StunEliteDuration, modifiers),
            stunBossDuration = statResolver.ResolveStat(equipmentSo, SkillStatModifierType.Mouse3StunBossDuration, modifiers)
        };
    }

    public ProjectileRuntimeData[] ResolveProjectileRuntime(
        EquipmentSkillRuntimeData runtime,
        GameObject owner,
        GameObject target,
        Vector2 spawnPosition,
        Vector2 direction,
        Vector2? explicitTargetPosition = null,
        int selectedHitIndex = -1,
        AnimationClip visualClipOverride = null,
        SkillAnimationVfxProfileSO animationVfxProfileOverride = null,
        SpritePresentationCalibrationProfileSO presentationCalibration = null,
        string comboToken = null,
        bool? criticalOverride = null,
        float damageWeight = 1f,
        int comboIndex = -1,
        SkillHitSO hitOverride = null,
        bool suppressVisual = false,
        float minimumVisualLifetime = 0f,SkillAimMode? manualAimMode=null,
        float comboGatherDistance=0f,float comboGatherDuration=0f,
        float comboGatherStopRadius=0f,float comboGatherBossHardCap=0f)
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

        if(manualAimMode.HasValue)
        {
            switch(manualAimMode.Value)
            {
                case SkillAimMode.Direction:targetingType=TargetingType.Directional;break;
                case SkillAimMode.Target:targetingType=TargetingType.AutoTarget;break;
                case SkillAimMode.GroundPoint:targetingType=TargetingType.Position;break;
                case SkillAimMode.Self:targetingType=TargetingType.Self;break;
                default:return Array.Empty<ProjectileRuntimeData>();
            }
        }

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

        baseProjectileData.orientManualPresentation=manualAimMode==SkillAimMode.Direction;
        baseProjectileData.controlPoint = resolvedTargetPosition;
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
                selectedHitIndex,
                hitOverride);

        float resolvedDamageWeight = Mathf.Clamp01(damageWeight);
        if (resolvedDamageWeight < 1f)
        {
            for (int i = 0; i < hitRuntimes.Length; i++)
            {
                SkillDamageProfileDto profile = hitRuntimes[i].damageProfile;
                if (profile == null)
                {
                    continue;
                }

                profile.baseDamage *= resolvedDamageWeight;
                profile.firstHitBaseDamage *= resolvedDamageWeight;
                profile.attackDamagePercent *= resolvedDamageWeight;
            }
        }

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
            projectileData.visualClipOverride = visualClipOverride;
            projectileData.animationVfxProfileOverride = animationVfxProfileOverride;
            projectileData.presentationCalibration = presentationCalibration;
            projectileData.comboToken = comboToken;
            projectileData.comboIndex = comboIndex >= 0 ? comboIndex : selectedHitIndex;
            projectileData.useCriticalOverride = criticalOverride.HasValue;
            projectileData.criticalOverride = criticalOverride.GetValueOrDefault();
            projectileData.suppressVisual = suppressVisual;
            projectileData.minimumVisualLifetime = Mathf.Max(0f, minimumVisualLifetime);
            projectileData.comboGatherDistance = Mathf.Max(0f, comboGatherDistance);
            projectileData.comboGatherDuration = Mathf.Max(0f, comboGatherDuration);
            projectileData.comboGatherStopRadius = Mathf.Max(0f, comboGatherStopRadius);
            projectileData.comboGatherBossHardCap = Mathf.Max(0f, comboGatherBossHardCap);
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
            resolvedLevel = Mathf.Clamp(runtime.resolvedLevel, 1, 5),
            resolvedMouse3Profile = runtime.resolvedMouse3Profile,
            target = target,
            spawnPosition = resolvedSpawnPosition,
            direction = direction,
            lifetime = statResolver.ResolveProjectileLifetime(runtime, resolvedStatModifiers),
            projectileCount = Mathf.Max(1, runtime.resolvedProjectileCount),
            projectileSpreadAngle = Mathf.Max(0f, runtime.resolvedProjectileSpreadAngle),
            projectileArrangementValue = Mathf.Max(0f, runtime.resolvedProjectileArrangementValue),
            projectileScale = Mathf.Max(0.01f, runtime.resolvedProjectileScale),
            rendererScale = EquipmentBaseProfileSO.NormalizeRendererScale(runtime.resolvedRendererScale),
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
            resolvedLevel = source.resolvedLevel,
            resolvedMouse3Profile = source.resolvedMouse3Profile,
            target = source.target,
            spawnPosition = source.spawnPosition,
            direction = source.direction,
            controlPoint = source.controlPoint,
            orientManualPresentation=source.orientManualPresentation,
            lifetime = source.lifetime,
            projectileCount = source.projectileCount,
            projectileSpreadAngle = source.projectileSpreadAngle,
            projectileArrangementValue = source.projectileArrangementValue,
            projectileScale = source.projectileScale,
            rendererScale = EquipmentBaseProfileSO.NormalizeRendererScale(source.rendererScale),
            projectileSpawnInterval = source.projectileSpawnInterval,
            projectileSpawnRadius = source.projectileSpawnRadius,
            moveRuntime = source.moveRuntime,
            damageProfile = source.damageProfile,
            visualContext = source.visualContext,
            effectRuntimeSet = source.effectRuntimeSet,
            spawnSkillSo = source.spawnSkillSo,
            projectileVisualType = source.projectileVisualType,
            sortingRelation = source.sortingRelation,
            material = source.material,
            color = source.color,
            useAnimatorTriggers = source.useAnimatorTriggers,
            visualClipOverride = source.visualClipOverride,
            animationVfxProfileOverride = source.animationVfxProfileOverride,
            minimumVisualLifetime = source.minimumVisualLifetime,
            presentationCalibration = source.presentationCalibration,
            comboToken = source.comboToken,
            comboIndex = source.comboIndex,
            useCriticalOverride = source.useCriticalOverride,
            criticalOverride = source.criticalOverride,
            comboGatherDistance = source.comboGatherDistance,
            comboGatherDuration = source.comboGatherDuration,
            comboGatherStopRadius = source.comboGatherStopRadius,
            comboGatherBossHardCap = source.comboGatherBossHardCap
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
            useHitWindow = hitSo.UseHitWindow,
            hitStartTime = Mathf.Max(0f, hitSo.HitStartTime),
            hitDuration = Mathf.Max(0f, hitSo.HitDuration),
            deactivateAfterFirstHit = hitSo.DeactivateAfterFirstHit,
            targetLayerMask = hitSo.TargetLayerMask,
            damageProfile = resolvedDamageProfile,
            spawnSkill = hitSo.SpawnSkill,
            buffEffectEntries = hitSo.BuffEffects,
            debuffEffectEntries = hitSo.DebuffEffects,
            effectUpgradeModifiers = effectUpgradeModifiers != null
                ? new List<EffectUpgradeModifierData>(effectUpgradeModifiers)
                : null,
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
        int selectedHitIndex = -1,
        SkillHitSO hitOverride = null)
    {
        EquipmentSkillSO equipmentSo = runtime?.sourceEquipment;
        SkillHitSO[] hitSos = hitOverride != null
            ? new[] { hitOverride }
            : equipmentSo != null
            ? equipmentSo.HitSos
            : null;

        if (hitSos == null || hitSos.Length == 0)
        {
            return Array.Empty<ResolvedHitRuntimeData>();
        }

        List<ResolvedHitRuntimeData> results = new();

        for (int i = 0; i < hitSos.Length; i++)
        {
            if (hitOverride == null && selectedHitIndex >= 0 && i != selectedHitIndex)
            {
                continue;
            }
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
                statResolver.ResolveHitMaxHitCount(
                    hitSo,
                    resolvedStatModifiers);

            SkillProjectileHitDto hitDto = CreateHitDto(
                hitSo,
                damageProfile,
                resolvedMaxHitCount,
                runtime.upgradeRuntimeData?.effectModifiers);

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

        bool definesDamage = hitSo.BaseDamage > Mathf.Epsilon
            || hitSo.FirstHitBaseDamage > Mathf.Epsilon
            || hitSo.AttackPercentDamage > Mathf.Epsilon;

        if (!definesDamage)
        {
            // Effect-only hits are represented by an empty embedded damage
            // block in the SO. Keep them damage-free even when the skill has
            // damage upgrade modifiers intended for another hit entry.
            return null;
        }

        SkillDamageProfileDto dto = new SkillDamageProfileDto
        {
            damageType = statResolver.GetDamageType(hitSo),
            baseDamage = statResolver.ResolveHitDamageStat(
                hitSo,
                SkillStatModifierType.BaseDamage,
                resolvedStatModifiers),
            attackDamagePercent = statResolver.ResolveHitDamageStat(
                hitSo,
                SkillStatModifierType.AttackPercentDamage,
                resolvedStatModifiers),
            firstHitBaseDamage = Mathf.Max(0f, hitSo.FirstHitBaseDamage),
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
        projectileData.suppressVisual = projectileData.suppressVisual ||
            projectileData.projectileVisualType == ProjectileVisualType.None;
        projectileData.sortingRelation = baseVisual != null
            ? baseVisual.SortingRelation
            : SkillSortingRelation.SameAsOwner;

        projectileData.material = null;
        projectileData.color = Color.white;
        projectileData.useAnimatorTriggers = !projectileData.suppressVisual &&
                                             (baseVisual == null ||
                                             baseVisual.AnimationClips == null ||
                                             baseVisual.AnimationClips.Length == 0);
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
