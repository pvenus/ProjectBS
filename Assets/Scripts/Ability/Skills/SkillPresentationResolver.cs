using System;
using System.Collections.Generic;
using Character;
using Effect;
using Presentation;
using Skill;
using Skills.Move.Config;
using UnityEngine;

public sealed class SkillPresentationResolver
{
    private const string StrategicSkillPrefix = "skill.strategic.";
    private const string StrategicItemPrefix = "item.strategic.";

    private readonly EffectPresentationResolver effectResolver = new();
    private readonly EquipmentStatResolver statResolver = new();

    public SkillPresentationData Resolve(
        EquipmentSkillSO skill,
        PresentationContext context)
    {
        return ResolveCore(skill, null, context ?? PresentationContext.Preview);
    }

    public SkillPresentationData Resolve(
        EquipmentSkillRuntimeData runtime,
        PresentationContext context)
    {
        if (runtime == null || runtime.sourceEquipment == null)
        {
            return CreateUnsupported();
        }

        return ResolveCore(
            runtime.sourceEquipment,
            runtime,
            context ?? PresentationContext.Runtime);
    }

    public ContentPresentationData ResolveLegacyEffect(
        SkillEffectSO effect)
    {
        if (effect == null)
        {
            return new ContentPresentationData(
                new PresentationIdentityData(string.Empty, string.Empty),
                string.Empty,
                Array.Empty<string>(),
                Array.Empty<PresentationGroupData>(),
                new PresentationProvenanceData(PresentationProvenanceKind.Unknown),
                ContentPresentationStatus.Unsupported);
        }

        PresentationIdentityData identity = new(
            effect.EffectId,
            string.IsNullOrWhiteSpace(effect.DisplayName)
                ? effect.name
                : effect.DisplayName);

        bool hasDescription = !string.IsNullOrWhiteSpace(effect.Description);
        return new ContentPresentationData(
            identity,
            hasDescription ? effect.Description : string.Empty,
            CopyTags(effect.Tags),
            Array.Empty<PresentationGroupData>(),
            new PresentationProvenanceData(
                hasDescription
                    ? PresentationProvenanceKind.AuthoredDescriptionFallback
                    : PresentationProvenanceKind.AuthoredAsset,
                effect.EffectId,
                sourceField: hasDescription ? "Description" : null),
            hasDescription
                ? ContentPresentationStatus.DescriptionOnly
                : ContentPresentationStatus.Unsupported);
    }

    private SkillPresentationData ResolveCore(
        EquipmentSkillSO skill,
        EquipmentSkillRuntimeData runtime,
        PresentationContext context)
    {
        if (skill == null)
        {
            return CreateUnsupported();
        }

        bool useRuntime = runtime != null
            && context.Mode == PresentationContextMode.Runtime;
        PresentationProvenanceData rootProvenance = CreateProvenance(
            skill,
            useRuntime
                ? PresentationProvenanceKind.RuntimeResolved
                : PresentationProvenanceKind.AuthoredAsset);

        return new SkillPresentationData(
            CreateIdentity(skill),
            ResolveDescription(skill),
            CreateClassification(skill),
            CreateCast(skill, runtime, useRuntime),
            CreateProjectile(skill, runtime, useRuntime),
            CreateHits(skill, runtime, context, useRuntime),
            CreateSelfEffects(skill, context),
            CreateSpawn(skill),
            rootProvenance,
            ContentPresentationStatus.Supported);
    }

    private SkillClassificationPresentationData CreateClassification(
        EquipmentSkillSO skill)
    {
        EquipmentBaseProfileSO profile = skill.BaseProfileSo;
        return profile == null
            ? null
            : new SkillClassificationPresentationData(
                profile.SkillType,
                profile.SkillComponentType,
                profile.Category,
                profile.TargetType,
                profile.TacticalNeed);
    }

    private SkillCastPresentationData CreateCast(
        EquipmentSkillSO skill,
        EquipmentSkillRuntimeData runtime,
        bool useRuntime)
    {
        SkillCastSO cast = skill.CastSo;
        if (cast == null)
        {
            return null;
        }

        CastMoveProfile move = cast.CastMove;
        return new SkillCastPresentationData(
            cast.TargetingType,
            Number(skill, cast.Cooldown, PresentationValueUnit.Seconds, "CastSo.Cooldown"),
            Number(skill, cast.CastTime, PresentationValueUnit.Seconds, "CastSo.CastTime"),
            useRuntime
                ? RuntimeNumber(skill, runtime.resolvedRange, PresentationValueUnit.Meters, "resolvedRange")
                : Number(skill, cast.Range, PresentationValueUnit.Meters, "CastSo.Range"),
            useRuntime
                ? RuntimeNumber(skill, runtime.resolvedBurstCount, PresentationValueUnit.Count, "resolvedBurstCount")
                : Number(skill, cast.BurstCount, PresentationValueUnit.Count, "CastSo.BurstCount"),
            useRuntime
                ? RuntimeNumber(skill, runtime.resolvedBurstInterval, PresentationValueUnit.Seconds, "resolvedBurstInterval")
                : Number(skill, cast.BurstInterval, PresentationValueUnit.Seconds, "CastSo.BurstInterval"),
            move != null ? move.MoveType : CastMoveType.None,
            move != null && move.MoveType != CastMoveType.None
                ? Number(skill, move.Distance, PresentationValueUnit.Meters, "CastSo.CastMove.Distance")
                : null,
            move != null && move.MoveType != CastMoveType.None
                ? Number(skill, move.Duration, PresentationValueUnit.Seconds, "CastSo.CastMove.Duration")
                : null);
    }

    private SkillProjectilePresentationData CreateProjectile(
        EquipmentSkillSO skill,
        EquipmentSkillRuntimeData runtime,
        bool useRuntime)
    {
        EquipmentBaseProfileSO profile = skill.BaseProfileSo;
        if (profile == null || profile.SkillComponentType != SkillComponentType.Projectile)
        {
            return null;
        }

        SkillMoveSO move = skill.MoveSo;
        ProjectileMoveType moveType = move != null
            ? move.MoveType
            : ProjectileMoveType.None;

        return new SkillProjectilePresentationData(
            moveType,
            profile.ProjectileArrangement,
            useRuntime
                ? RuntimeNumber(skill, runtime.resolvedProjectileCount, PresentationValueUnit.Count, "resolvedProjectileCount")
                : Number(skill, profile.ProjectileCount, PresentationValueUnit.Count, "BaseProfileSo.ProjectileCount"),
            useRuntime
                ? RuntimeNumber(skill, runtime.resolvedProjectileScale, PresentationValueUnit.Ratio, "resolvedProjectileScale")
                : Number(skill, profile.ProjectileScale, PresentationValueUnit.Ratio, "BaseProfileSo.ProjectileScale"),
            Number(skill, profile.ProjectileColliderRadius, PresentationValueUnit.Meters, "BaseProfileSo.ProjectileColliderRadius"),
            Number(skill, profile.ProjectileLifetime, PresentationValueUnit.Seconds, "BaseProfileSo.ProjectileLifetime"),
            useRuntime
                ? RuntimeNumber(skill, runtime.resolvedProjectileSpreadAngle, PresentationValueUnit.Degrees, "resolvedProjectileSpreadAngle")
                : Number(skill, profile.ProjectileSpreadAngle, PresentationValueUnit.Degrees, "BaseProfileSo.ProjectileSpreadAngle"),
            useRuntime
                ? RuntimeNumber(skill, runtime.resolvedProjectileArrangementValue, GetArrangementUnit(profile.ProjectileArrangement), "resolvedProjectileArrangementValue")
                : Number(skill, profile.ProjectileArrangementValue, GetArrangementUnit(profile.ProjectileArrangement), "BaseProfileSo.ProjectileArrangementValue"),
            Number(skill, profile.ProjectileSpawnOffset, PresentationValueUnit.Meters, "BaseProfileSo.ProjectileSpawnOffset"),
            Number(skill, profile.ProjectileSpawnInterval, PresentationValueUnit.Seconds, "BaseProfileSo.ProjectileSpawnInterval"),
            Number(skill, profile.ProjectileSpawnRadius, PresentationValueUnit.Meters, "BaseProfileSo.ProjectileSpawnRadius"),
            CreateMovementParameters(skill, move));
    }

    private IReadOnlyList<PresentationEntryData> CreateMovementParameters(
        EquipmentSkillSO skill,
        SkillMoveSO move)
    {
        if (move?.Config == null)
        {
            return Array.Empty<PresentationEntryData>();
        }

        List<PresentationEntryData> entries = new();
        switch (move.Config)
        {
            case LinearMoveConfig linear:
                AddEntry(entries, "config.speed",
                    Number(skill, linear.speed, PresentationValueUnit.MetersPerSecond, "MoveSo.Config.speed"));
                break;

            case HomingMoveConfig homing:
                AddEntry(entries, "config.speed",
                    Number(skill, homing.speed, PresentationValueUnit.MetersPerSecond, "MoveSo.Config.speed"));
                AddEntry(entries, "config.turnSpeed",
                    Number(skill, homing.turnSpeed, PresentationValueUnit.DegreesPerSecond, "MoveSo.Config.turnSpeed"));
                break;

            case OrbitMoveConfig orbit:
                AddEntry(entries, "config.orbitRadius",
                    Number(skill, orbit.orbitRadius, PresentationValueUnit.Meters, "MoveSo.Config.orbitRadius"));
                AddEntry(entries, "config.orbitAngularSpeed",
                    Number(skill, orbit.orbitAngularSpeed, PresentationValueUnit.DegreesPerSecond, "MoveSo.Config.orbitAngularSpeed"));
                AddEntry(entries, "config.clockwise",
                    PresentationValueData.SemanticToken(
                        orbit.clockwise ? "Clockwise" : "CounterClockwise",
                        CreateProvenance(skill, PresentationProvenanceKind.AuthoredAsset, "MoveSo.Config.clockwise")));
                break;

            case HoverMoveConfig hover:
                AddEntry(entries, "config.followOffset.x",
                    Number(skill, hover.followOffset.x, PresentationValueUnit.Meters, "MoveSo.Config.followOffset.x"));
                AddEntry(entries, "config.followOffset.y",
                    Number(skill, hover.followOffset.y, PresentationValueUnit.Meters, "MoveSo.Config.followOffset.y"));
                break;

            case WarpMoveConfig _:
                break;
        }

        return entries;
    }

    private IReadOnlyList<SkillHitPresentationData> CreateHits(
        EquipmentSkillSO skill,
        EquipmentSkillRuntimeData runtime,
        PresentationContext context,
        bool useRuntime)
    {
        SkillHitSO[] sources = skill.HitSos;
        if (sources == null || sources.Length == 0)
        {
            return Array.Empty<SkillHitPresentationData>();
        }

        IEnumerable<SkillStatModifierData> modifiers = useRuntime
            ? runtime.upgradeRuntimeData?.statModifiers
            : null;
        List<SkillHitPresentationData> results = new();

        for (int sourceIndex = 0; sourceIndex < sources.Length; sourceIndex++)
        {
            SkillHitSO hit = sources[sourceIndex];
            if (hit == null)
            {
                continue;
            }

            float baseDamage = useRuntime
                ? statResolver.ResolveStat(skill, SkillStatModifierType.BaseDamage, modifiers)
                : hit.BaseDamage;
            float attackScaling = useRuntime
                ? statResolver.ResolveStat(skill, SkillStatModifierType.AttackPercentDamage, modifiers)
                : hit.AttackPercentDamage;
            int maxHitCount = useRuntime
                ? statResolver.ResolveIntStat(skill, SkillStatModifierType.MaxHitCount, modifiers)
                : hit.MaxHitCount;
            bool hasDamage = !Mathf.Approximately(hit.BaseDamage, 0f)
                || !Mathf.Approximately(hit.FirstHitBaseDamage, 0f)
                || !Mathf.Approximately(hit.AttackPercentDamage, 0f)
                || hit.CanCritical
                || hit.IgnoreDefense;
            bool exposeSplit = hit.UseSplitMultiHitDamage;

            results.Add(new SkillHitPresentationData(
                hit.HitId,
                hasDamage,
                LayerMaskToken(
                    skill,
                    hit.TargetLayerMask,
                    $"HitSos[{sourceIndex}].TargetLayerMask"),
                hit.DamageType,
                useRuntime
                    ? RuntimeNumber(skill, baseDamage, PresentationValueUnit.Flat, $"Hit[{sourceIndex}].resolvedBaseDamage")
                    : Number(skill, baseDamage, PresentationValueUnit.Flat, $"HitSos[{sourceIndex}].BaseDamage"),
                !useRuntime && !Mathf.Approximately(hit.FirstHitBaseDamage, 0f)
                    ? Number(skill, hit.FirstHitBaseDamage, PresentationValueUnit.Flat, $"HitSos[{sourceIndex}].FirstHitBaseDamage")
                    : null,
                useRuntime
                    ? RuntimeNumber(skill, attackScaling, PresentationValueUnit.Percent, $"Hit[{sourceIndex}].resolvedAttackDamagePercent")
                    : Number(skill, attackScaling, PresentationValueUnit.Percent, $"HitSos[{sourceIndex}].AttackPercentDamage"),
                hit.CanCritical,
                hit.IgnoreDefense,
                useRuntime
                    ? RuntimeNumber(skill, maxHitCount, PresentationValueUnit.Count, $"Hit[{sourceIndex}].resolvedMaxHitCount")
                    : Number(skill, maxHitCount, PresentationValueUnit.Count, $"HitSos[{sourceIndex}].MaxHitCount"),
                Number(skill, hit.HitStartTime, PresentationValueUnit.Seconds, $"HitSos[{sourceIndex}].HitStartTime"),
                hit.UseRepeatInterval
                    ? Number(skill, hit.RepeatInterval, PresentationValueUnit.Seconds, $"HitSos[{sourceIndex}].RepeatInterval")
                    : null,
                exposeSplit
                    ? Number(skill, hit.SplitHitCount, PresentationValueUnit.Count, $"HitSos[{sourceIndex}].SplitHitCount")
                    : null,
                exposeSplit
                    ? Number(skill, hit.SplitHitInterval, PresentationValueUnit.Seconds, $"HitSos[{sourceIndex}].SplitHitInterval")
                    : null,
                CreateHitEffects(hit, context)));
        }

        return results;
    }

    private IReadOnlyList<SkillEffectPresentationItem> CreateSelfEffects(
        EquipmentSkillSO skill,
        PresentationContext context)
    {
        return CreateEffects(
            skill.CastSo != null ? skill.CastSo.SelfEffects : null,
            SkillEffectSourceKind.SelfEffects,
            string.Empty,
            context);
    }

    private IReadOnlyList<SkillEffectPresentationItem> CreateHitEffects(
        SkillHitSO hit,
        PresentationContext context)
    {
        List<SkillEffectPresentationItem> result = new();
        result.AddRange(CreateEffects(hit.BuffEffects, SkillEffectSourceKind.BuffEffects, hit.HitId, context));
        result.AddRange(CreateEffects(hit.DebuffEffects, SkillEffectSourceKind.DebuffEffects, hit.HitId, context));
        return result;
    }

    private IReadOnlyList<SkillEffectPresentationItem> CreateEffects(
        EffectEntrySO[] entries,
        SkillEffectSourceKind sourceKind,
        string hitId,
        PresentationContext context)
    {
        if (entries == null || entries.Length == 0)
        {
            return Array.Empty<SkillEffectPresentationItem>();
        }

        List<SkillEffectPresentationItem> results = new(entries.Length);
        foreach (EffectEntrySO entry in entries)
        {
            if (entry == null)
            {
                continue;
            }

            results.Add(new SkillEffectPresentationItem(
                sourceKind,
                hitId,
                effectResolver.Resolve(entry, context)));
        }

        return results;
    }

    private SkillSpawnPresentationData CreateSpawn(EquipmentSkillSO skill)
    {
        SpawnSkillSO spawn = skill.SpawnSkillSo;
        if (spawn == null)
        {
            return null;
        }

        CharacterSO character = spawn.CharacterSO;
        PresentationIdentityData identity = character != null
            ? new PresentationIdentityData(
                character.CharacterId,
                ResolveCharacterName(character))
            : null;

        return new SkillSpawnPresentationData(
            identity,
            Number(skill, spawn.SpawnCount, PresentationValueUnit.Count, "SpawnSkillSo.SpawnCount"),
            Number(skill, spawn.SpawnInterval, PresentationValueUnit.Seconds, "SpawnSkillSo.SpawnInterval"),
            Number(skill, spawn.SpawnLifeTime, PresentationValueUnit.Seconds, "SpawnSkillSo.SpawnLifeTime"));
    }

    private static SkillPresentationData CreateUnsupported()
    {
        return new SkillPresentationData(
            new PresentationIdentityData(string.Empty, string.Empty),
            string.Empty,
            null,
            null,
            null,
            Array.Empty<SkillHitPresentationData>(),
            Array.Empty<SkillEffectPresentationItem>(),
            null,
            new PresentationProvenanceData(PresentationProvenanceKind.Unknown),
            ContentPresentationStatus.Unsupported);
    }

    private static PresentationIdentityData CreateIdentity(EquipmentSkillSO skill)
    {
        string strategicItemKey = GetStrategicItemLocalizationKey(
            skill.LocalizationMainKey);
        string displayName = string.IsNullOrWhiteSpace(strategicItemKey)
            ? PresentationLocalizedTextResolver.ResolveName(
                skill.name,
                skill.LocalizationMainKey)
            : PresentationLocalizedTextResolver.ResolveName(
                skill.name,
                strategicItemKey,
                skill.LocalizationMainKey);
        return new PresentationIdentityData(skill.EquipmentId, displayName, skill.Icon);
    }

    private static string ResolveDescription(EquipmentSkillSO skill)
    {
        if (skill == null)
        {
            return string.Empty;
        }

        if (!skill.LocalizationMainKey.StartsWith(
                StrategicSkillPrefix,
                StringComparison.Ordinal))
        {
            return PresentationLocalizedTextResolver.ResolveRequired(
                "desc",
                skill.LocalizationMainKey);
        }

        string strategicItemKey = GetStrategicItemLocalizationKey(
            skill.LocalizationMainKey);
        return PresentationLocalizedTextResolver.ResolveRequired(
            "desc",
            skill.LocalizationMainKey,
            strategicItemKey);
    }

    private static string ResolveCharacterName(CharacterSO character)
    {
        return PresentationLocalizedTextResolver.ResolveName(
            character.name,
            character.LocalizationMainKey);
    }

    private static string GetStrategicItemLocalizationKey(string skillKey)
    {
        return !string.IsNullOrWhiteSpace(skillKey)
            && skillKey.StartsWith(StrategicSkillPrefix, StringComparison.Ordinal)
                ? StrategicItemPrefix
                    + skillKey.Substring(StrategicSkillPrefix.Length)
                : string.Empty;
    }

    private static PresentationValueUnit GetArrangementUnit(
        ProjectileArrangementType arrangement)
    {
        return arrangement == ProjectileArrangementType.Spread
            ? PresentationValueUnit.Degrees
            : PresentationValueUnit.Meters;
    }

    private static void AddEntry(
        ICollection<PresentationEntryData> entries,
        string key,
        PresentationValueData value)
    {
        if (value != null)
        {
            entries.Add(new PresentationEntryData(key, new[] { value }));
        }
    }

    private static PresentationValueData Number(
        EquipmentSkillSO skill,
        double value,
        PresentationValueUnit unit,
        string sourceField)
    {
        return PresentationValueData.Number(
            value,
            unit,
            CreateProvenance(
                skill,
                PresentationProvenanceKind.AuthoredAsset,
                sourceField));
    }

    private static PresentationValueData RuntimeNumber(
        EquipmentSkillSO skill,
        double value,
        PresentationValueUnit unit,
        string sourceField)
    {
        return PresentationValueData.Number(
            value,
            unit,
            CreateProvenance(
                skill,
                PresentationProvenanceKind.RuntimeResolved,
                sourceField));
    }

    private static PresentationValueData LayerMaskToken(
        EquipmentSkillSO skill,
        LayerMask mask,
        string sourceField)
    {
        List<string> layerNames = new();
        int value = mask.value;
        for (int layer = 0; layer < 32; layer++)
        {
            if ((value & (1 << layer)) == 0)
            {
                continue;
            }

            string layerName = LayerMask.LayerToName(layer);
            if (!string.IsNullOrWhiteSpace(layerName))
            {
                layerNames.Add(layerName);
            }
        }

        return layerNames.Count == 0
            ? null
            : PresentationValueData.SemanticToken(
                string.Join(", ", layerNames),
                CreateProvenance(
                    skill,
                    PresentationProvenanceKind.AuthoredAsset,
                    sourceField));
    }

    private static PresentationProvenanceData CreateProvenance(
        EquipmentSkillSO skill,
        PresentationProvenanceKind kind,
        string sourceField = null)
    {
        return new PresentationProvenanceData(
            kind,
            skill != null ? skill.EquipmentId : string.Empty,
            sourceField: sourceField);
    }

    private static IReadOnlyList<string> CopyTags(IReadOnlyList<string> tags)
    {
        if (tags == null || tags.Count == 0)
        {
            return Array.Empty<string>();
        }

        List<string> result = new();
        foreach (string tag in tags)
        {
            if (!string.IsNullOrWhiteSpace(tag))
            {
                result.Add(tag);
            }
        }

        return result;
    }
}
