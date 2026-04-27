using System;
using System.Collections.Generic;
using UnityEngine;
using Skills.Dto;

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
            : GetBaseGrade(equipmentSo);

        int resolvedRuneSlotCount = instanceData != null && instanceData.currentRuneSlotCount > 0
            ? instanceData.currentRuneSlotCount
            : GetBaseRuneSlotCount(equipmentSo);

        ElementType mainElement = instanceData != null
            ? instanceData.mainElement
            : ElementType.None;

        ElementType[] subElements = instanceData != null && instanceData.subElements != null
            ? instanceData.subElements.ToArray()
            : Array.Empty<ElementType>();

        RuneRuntimeSetData runeRuntimeSet = runeResolver.Resolve(instanceData);
        EffectRuntimeSetData effectRuntimeSet = runeResolver.ResolveEffectRuntimeSet(runeRuntimeSet);
        EquipmentUpgradeRuntimeData upgradeRuntimeData = upgradeResolver.Resolve(equipmentSo, instanceData);

        List<SkillStatModifierRuntimeData> resolvedStatModifiers = statResolver.CombineStatModifiers(
            runeResolver.ExtractModifiers(runeRuntimeSet),
            upgradeResolver.ExtractModifiers(upgradeRuntimeData));

        AttackArchetype attackArchetype = GetAttackArchetype(equipmentSo);

        return new EquipmentSkillRuntimeData
        {
            sourceEquipment = equipmentSo,
            instanceData = instanceData,

            attackArchetype = attackArchetype,
            resolvedGrade = resolvedGrade,
            resolvedRuneSlotCount = Mathf.Max(1, resolvedRuneSlotCount),
            resolvedProjectileCount = statResolver.ResolveProjectileCount(equipmentSo, resolvedStatModifiers),
            resolvedProjectileScale = statResolver.ResolveProjectileScale(equipmentSo, resolvedStatModifiers),

            mainElement = mainElement,
            subElements = subElements,

            castSo = equipmentSo.CastSo,
            damageSo = equipmentSo.DamageSo,
            hitSo = equipmentSo.HitSo,
            moveSo = equipmentSo.MoveSo,
            visualSetSo = equipmentSo.VisualSetSo,

            visualContext = BuildVisualContext(equipmentSo, attackArchetype, resolvedGrade, mainElement, subElements),
            runeRuntimeSet = runeRuntimeSet,
            effectRuntimeSet = effectRuntimeSet,
            upgradeRuntimeData = upgradeRuntimeData,

            projectilePrefab = instanceData != null && instanceData.projectilePrefab != null
                ? instanceData.projectilePrefab
                : GetProjectilePrefab(equipmentSo)
        };
    }

    public ProjectileRuntimeData ResolveProjectileRuntime(
        EquipmentSkillRuntimeData runtime,
        GameObject owner,
        GameObject target,
        Vector2 spawnPosition,
        Vector2 direction)
    {
        if (runtime == null)
        {
            Debug.LogError("EquipmentSkillResolver.ResolveProjectileRuntime failed: runtime is null.");
            return null;
        }

        List<SkillStatModifierRuntimeData> resolvedStatModifiers = ResolveStatModifiers(runtime);
        Vector2 resolvedSpawnPosition = ResolveProjectileSpawnPosition(runtime, spawnPosition, direction);

        var projectileData = new ProjectileRuntimeData
        {
            owner = owner,
            target = target,
            spawnPosition = resolvedSpawnPosition,
            direction = direction,
            lifetime = statResolver.ResolveProjectileLifetime(runtime, resolvedStatModifiers),
            projectilePrefab = runtime.projectilePrefab,
            projectileCount = Mathf.Max(1, runtime.resolvedProjectileCount),
            projectileScale = Mathf.Max(0.01f, runtime.resolvedProjectileScale),
            visualContext = runtime.visualContext,
            effectRuntimeSet = runtime.effectRuntimeSet
        };

        projectileData.move = CreateMoveDto(runtime.moveSo, target, resolvedSpawnPosition, direction);
        projectileData.hit = CreateHitDto(runtime.hitSo);
        projectileData.damageProfile = CreateDamageProfileDto(runtime, resolvedStatModifiers);

        ResolveProjectileVisualRuntime(runtime, projectileData);
        return projectileData;
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

    private SkillProjectileMoveDto CreateMoveDto(
        SkillMoveSO moveSo,
        GameObject target,
        Vector2 startPosition,
        Vector2 direction)
    {
        if (moveSo == null)
        {
            return null;
        }

        Transform targetTransform = target != null ? target.transform : null;
        Vector2 targetPosition = targetTransform != null
            ? (Vector2)targetTransform.position
            : startPosition + (direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right);

        return moveSo.CreateDto(targetTransform, startPosition, targetPosition);
    }

    private SkillProjectileHitDto CreateHitDto(SkillHitSO hitSo)
    {
        SkillProjectileHitDto dto = ReflectionHelper.TryInvokeCreateDto<SkillProjectileHitDto>(hitSo);
        if (dto != null)
        {
            return dto;
        }

        return ReflectionHelper.TryInvokeCreateDto<SkillProjectileHitDto>(hitSo, new object[] { null });
    }

    private SkillDamageProfileDto CreateDamageProfileDto(
        EquipmentSkillRuntimeData runtime,
        IEnumerable<SkillStatModifierRuntimeData> resolvedStatModifiers)
    {
        if (runtime == null)
        {
            return null;
        }

        SkillDamageProfileDto dto = ReflectionHelper.TryInvokeCreateDto<SkillDamageProfileDto>(runtime.damageSo);
        if (dto == null)
        {
            dto = new SkillDamageProfileDto();
        }

        EquipmentSkillSO equipmentSo = runtime.sourceEquipment;
        dto.baseDamage = statResolver.ResolveBaseDamage(equipmentSo, resolvedStatModifiers);
        dto.flatBonusDamage = statResolver.GetFlatBonusDamage(equipmentSo);
        dto.criticalMultiplier = statResolver.GetCriticalMultiplier(equipmentSo);

        ReflectionHelper.WriteMemberObject(dto, "damageType", statResolver.GetDamageType(equipmentSo));
        ReflectionHelper.WriteMemberObject(dto, "canCritical", statResolver.GetCanCritical(equipmentSo));
        ReflectionHelper.WriteMemberObject(dto, "ignoreDefense", statResolver.GetIgnoreDefense(equipmentSo));

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
        MainElementVisualEntry mainEntry = ResolveMainElementVisualEntry(runtime);

        projectileData.spawnClip = mainEntry != null && mainEntry.ProjectileClipOverride != null
            ? mainEntry.ProjectileClipOverride
            : baseVisual != null ? baseVisual.ProjectileLoopClip : null;

        projectileData.hitClip = baseVisual != null ? baseVisual.HitClip : null;
        projectileData.despawnClip = null;

        projectileData.sprite = mainEntry != null && mainEntry.MainSprite != null
            ? mainEntry.MainSprite
            : baseVisual != null ? baseVisual.BaseSprite : null;

        projectileData.material = mainEntry != null ? mainEntry.MainMaterial : null;
        projectileData.color = Color.white;
        projectileData.useAnimatorTriggers = projectileData.spawnClip == null
            && projectileData.hitClip == null
            && projectileData.despawnClip == null;
    }

    private MainElementVisualEntry ResolveMainElementVisualEntry(EquipmentSkillRuntimeData runtime)
    {
        if (runtime == null || runtime.mainElement == ElementType.None)
        {
            return null;
        }

        SkillVisualSetSO visualSet = runtime.visualSetSo;
        if (visualSet == null || visualSet.MainElementVisualLibrarySo == null)
        {
            return null;
        }

        MainElementVisualGroupSO group = visualSet.MainElementVisualLibrarySo.GetGroup(runtime.mainElement);
        return group != null ? group.Get(runtime.resolvedGrade) : null;
    }

    private ResolvedVisualContextDto BuildVisualContext(
        EquipmentSkillSO equipmentSo,
        AttackArchetype archetype,
        EquipmentGrade grade,
        ElementType mainElement,
        ElementType[] subElements)
    {
        return new ResolvedVisualContextDto
        {
            attackArchetype = archetype,
            equipmentGrade = grade,
            mainElement = mainElement,
            subElements = subElements ?? Array.Empty<ElementType>(),
            baseVisualId = equipmentSo.VisualSetSo != null && equipmentSo.VisualSetSo.BaseVisualSo != null
                ? equipmentSo.VisualSetSo.BaseVisualSo.VisualId
                : string.Empty,
            mainVisualId = BuildMainVisualId(archetype, mainElement, grade)
        };
    }

    private string BuildMainVisualId(AttackArchetype archetype, ElementType element, EquipmentGrade grade)
    {
        if (element == ElementType.None)
        {
            return string.Empty;
        }

        return $"{archetype}_{element}_{grade}";
    }

    private EquipmentGrade GetBaseGrade(EquipmentSkillSO equipmentSo)
    {
        if (equipmentSo != null && equipmentSo.BaseProfileSo != null)
        {
            return equipmentSo.BaseProfileSo.BaseGrade;
        }

        return EquipmentGrade.Common;
    }

    private int GetBaseRuneSlotCount(EquipmentSkillSO equipmentSo)
    {
        if (equipmentSo != null && equipmentSo.BaseProfileSo != null)
        {
            return Mathf.Max(1, equipmentSo.BaseProfileSo.BaseRuneSlotCount);
        }

        return 1;
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
