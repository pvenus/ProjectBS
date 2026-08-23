using System;
using System.Collections.Generic;
using Effect;
using Presentation;
using Skill;

public sealed class SkillPresentationGroupResolver
{
    private const string ActivationGroupKey = "Activation";
    private const string DeliveryGroupKey = "Delivery";
    private const string OutcomeGroupKey = "Outcome";
    private const string SpecialEffectGroupKey = "SpecialEffect";
    private const string LinkedSkillGroupKey = "LinkedSkill";
    private const double NumericEpsilon = 0.0001d;
    private const double UnboundedValueThreshold = 999d;

    public ContentPresentationData Resolve(SkillPresentationData skill)
    {
        return ResolveCore(skill, false);
    }

    public ContentPresentationData ResolveForPlayerDisplay(SkillPresentationData skill)
    {
        return ResolveCore(skill, true);
    }

    private static ContentPresentationData ResolveCore(
        SkillPresentationData skill,
        bool filterDefaults)
    {
        if (skill == null)
        {
            return new ContentPresentationData(
                new PresentationIdentityData(string.Empty, string.Empty),
                string.Empty,
                Array.Empty<string>(),
                Array.Empty<PresentationGroupData>(),
                new PresentationProvenanceData(PresentationProvenanceKind.Unknown),
                ContentPresentationStatus.Unsupported);
        }

        GroupBuilder activation = new(ActivationGroupKey);
        GroupBuilder delivery = new(DeliveryGroupKey);
        GroupBuilder outcome = new(OutcomeGroupKey);
        GroupBuilder specialEffect = new(SpecialEffectGroupKey);
        GroupBuilder linkedSkill = new(LinkedSkillGroupKey);

        AddCastEntries(activation, delivery, skill.Cast, filterDefaults);
        AddBaseProfileEntries(delivery, skill.Projectile, filterDefaults);
        AddMoveEntries(delivery, skill.Projectile, filterDefaults);
        AddHitEntries(
            activation,
            delivery,
            outcome,
            specialEffect,
            linkedSkill,
            skill.Hits,
            filterDefaults);
        AddEffectEntries(
            activation,
            outcome,
            specialEffect,
            linkedSkill,
            skill.SelfEffects);
        AddSpawnEntries(outcome, skill.Spawn, filterDefaults);

        if (filterDefaults)
        {
            activation.FilterForPlayerDisplay();
            delivery.FilterForPlayerDisplay();
            outcome.FilterForPlayerDisplay();
            specialEffect.FilterForPlayerDisplay();
            linkedSkill.FilterForPlayerDisplay();
        }

        List<PresentationGroupData> groups = new();
        AddResolvedGroup(groups, activation);
        AddResolvedGroup(groups, delivery);
        AddResolvedGroup(groups, outcome);
        AddResolvedGroup(groups, specialEffect);
        AddResolvedGroup(groups, linkedSkill);

        return new ContentPresentationData(
            skill.Identity,
            skill.Description,
            ResolveClassificationKeys(skill.Classification, filterDefaults),
            groups,
            skill.Provenance,
            skill.Status);
    }

    private static IReadOnlyList<string> ResolveClassificationKeys(
        SkillClassificationPresentationData classification,
        bool playerDisplay)
    {
        if (classification == null)
        {
            return Array.Empty<string>();
        }

        if (!playerDisplay)
        {
            return classification.Keys;
        }

        List<string> result = new();
        AddPlayerTag(result, classification.SkillType.ToString());
        AddPlayerTag(result, classification.Category.ToString());
        AddPlayerTag(result, classification.TargetType.ToString());
        return result;
    }

    private static void AddPlayerTag(
        ICollection<string> target,
        string token)
    {
        if (PresentationDisplayCatalog.IsPlayerVisibleTag(token))
        {
            target.Add(token);
        }
    }

    private static void AddCastEntries(
        GroupBuilder activation,
        GroupBuilder delivery,
        SkillCastPresentationData cast,
        bool filterDefaults)
    {
        if (cast == null)
        {
            return;
        }

        if (!filterDefaults || cast.TargetingType != TargetingType.None)
        {
            AddToken(activation.Entries, "targetingType", cast.TargetingType.ToString());
        }
        if (filterDefaults)
        {
            AddPositiveValue(activation.Entries, "cooldown", cast.Cooldown);
            AddPositiveValue(activation.Entries, "castTime", cast.CastTime);
            if (UsesRange(cast.TargetingType) && IsBoundedPositive(cast.Range))
            {
                AddValue(activation.Entries, "range", cast.Range);
            }
        }
        else
        {
            AddValue(activation.Entries, "cooldown", cast.Cooldown);
            AddValue(activation.Entries, "castTime", cast.CastTime);
            AddValue(activation.Entries, "range", cast.Range);
        }

        if (!filterDefaults)
        {
            AddValue(delivery.Entries, "burst.count", cast.BurstCount);
            AddValue(delivery.Entries, "burst.interval", cast.BurstInterval);
        }
        else if (cast.BurstCount != null && cast.BurstCount.NumericValue > 1d)
        {
            AddValue(delivery.Entries, "burst.count", cast.BurstCount);
            AddValue(
                delivery.Entries,
                "burst.interval",
                filterDefaults ? PositiveOrNull(cast.BurstInterval) : cast.BurstInterval);
        }

        if (cast.CastMoveType != CastMoveType.None)
        {
            AddToken(delivery.Entries, "castMove.moveType", cast.CastMoveType.ToString());
            AddValue(
                delivery.Entries,
                "castMove.distance",
                filterDefaults ? PositiveOrNull(cast.CastMoveDistance) : cast.CastMoveDistance);
            AddValue(
                delivery.Entries,
                "castMove.duration",
                filterDefaults ? PositiveOrNull(cast.CastMoveDuration) : cast.CastMoveDuration);
        }

    }

    private static void AddBaseProfileEntries(
        GroupBuilder delivery,
        SkillProjectilePresentationData projectile,
        bool filterDefaults)
    {
        if (projectile == null)
        {
            return;
        }

        ICollection<PresentationEntryData> entries = delivery.Entries;
        bool hasArrangementData = IsPositive(projectile.ArrangementValue)
            || IsPositive(projectile.SpreadAngle)
            || IsPositive(projectile.SpawnRadius)
            || IsGreaterThanOne(projectile.Count);
        bool hasSpawnData = IsPositive(projectile.SpawnOffset)
            || IsPositive(projectile.SpawnInterval);
        if (!filterDefaults)
        {
            AddValue(entries, "projectileCount", projectile.Count);
            AddValue(entries, "projectileScale", projectile.Scale);
            AddValue(entries, "projectileColliderRadius", projectile.ColliderRadius);
            AddValue(entries, "projectileLifetime", projectile.Lifetime);
            if (hasArrangementData)
            {
                AddToken(entries, "projectile.arrangement", projectile.Arrangement.ToString());
                AddValue(entries, "projectile.arrangementValue", projectile.ArrangementValue);
                AddValue(entries, "projectile.spreadAngle", projectile.SpreadAngle);
                AddValue(entries, "projectile.radius", projectile.SpawnRadius);
            }
            if (hasSpawnData)
            {
                AddValue(entries, "projectileSpawn.spawnOffset", projectile.SpawnOffset);
                AddValue(entries, "projectileSpawn.interval", projectile.SpawnInterval);
            }
            return;
        }

        AddValue(entries, "projectileCount", GreaterThanOneOrNull(projectile.Count));
        AddValue(entries, "projectileScale", NonDefaultOneOrNull(projectile.Scale));
        AddValue(
            entries,
            "projectileColliderRadius",
            IsBoundedPositive(projectile.ColliderRadius) ? projectile.ColliderRadius : null);
        AddValue(entries, "projectileLifetime", PositiveOrNull(projectile.Lifetime));

        if (hasArrangementData)
        {
            AddToken(entries, "projectile.arrangement", projectile.Arrangement.ToString());
        }
        AddValue(entries, "projectile.arrangementValue", PositiveOrNull(projectile.ArrangementValue));
        AddValue(entries, "projectile.spreadAngle", PositiveOrNull(projectile.SpreadAngle));
        AddValue(entries, "projectile.radius", PositiveOrNull(projectile.SpawnRadius));
        AddValue(entries, "projectileSpawn.spawnOffset", PositiveOrNull(projectile.SpawnOffset));
        AddValue(entries, "projectileSpawn.interval", PositiveOrNull(projectile.SpawnInterval));

    }

    private static void AddMoveEntries(
        GroupBuilder delivery,
        SkillProjectilePresentationData projectile,
        bool filterDefaults)
    {
        if (projectile == null)
        {
            return;
        }

        ICollection<PresentationEntryData> entries = delivery.Entries;
        if (!filterDefaults || projectile.MoveType != ProjectileMoveType.None)
        {
            AddToken(entries, "moveType", projectile.MoveType.ToString());
        }

        foreach (PresentationEntryData parameter in projectile.MovementParameters)
        {
            PresentationEntryData selected = filterDefaults
                ? FilterNonZeroValues(parameter)
                : parameter;
            if (selected != null)
            {
                entries.Add(selected);
            }
        }

    }

    private static void AddHitEntries(
        GroupBuilder activation,
        GroupBuilder delivery,
        GroupBuilder outcome,
        GroupBuilder specialEffect,
        GroupBuilder linkedSkill,
        IReadOnlyList<SkillHitPresentationData> hits,
        bool filterDefaults)
    {
        if (hits == null)
        {
            return;
        }

        foreach (SkillHitPresentationData hit in hits)
        {
            if (hit == null)
            {
                continue;
            }

            AddValue(activation.Entries, "targetLayerMask", hit.TargetLayerMask);
            if (!filterDefaults)
            {
                if (hit.HasDamage)
                {
                    AddToken(outcome.Entries, "damage.damageType", hit.DamageType.ToString());
                    AddValue(outcome.Entries, "damage.baseDamage", hit.BaseDamage);
                    AddValue(outcome.Entries, "damage.firstHitBaseDamage", hit.FirstHitBaseDamage);
                    AddValue(outcome.Entries, "damage.attackPercentDamage", hit.AttackScaling);
                    AddToken(outcome.Entries, "damage.canCritical", hit.CanCritical ? "True" : "False");
                    AddToken(outcome.Entries, "damage.ignoreDefense", hit.IgnoreDefense ? "True" : "False");
                }
                AddValue(delivery.Entries, "maxHitCount", hit.MaxHitCount);
                AddValue(delivery.Entries, "hitStartTime", hit.HitStartTime);
                AddValue(delivery.Entries, "repeatInterval", hit.RepeatInterval);
                AddValue(delivery.Entries, "split.hitCount", hit.SplitHitCount);
                AddValue(delivery.Entries, "split.hitInterval", hit.SplitHitInterval);

                AddEffectEntries(
                    activation,
                    outcome,
                    specialEffect,
                    linkedSkill,
                    hit.Effects);
                continue;
            }

            bool hasDamage = hit.HasDamage
                && (IsPositive(hit.BaseDamage)
                    || IsPositive(hit.AttackScaling)
                    || IsPositive(hit.FirstHitBaseDamage)
                    || hit.CanCritical
                    || hit.IgnoreDefense);
            if (hasDamage)
            {
                AddToken(outcome.Entries, "damage.damageType", hit.DamageType.ToString());
                AddPositiveValue(outcome.Entries, "damage.baseDamage", hit.BaseDamage);
                AddPositiveValue(outcome.Entries, "damage.firstHitBaseDamage", hit.FirstHitBaseDamage);
                AddPositiveValue(outcome.Entries, "damage.attackPercentDamage", hit.AttackScaling);
                if (hit.CanCritical)
                {
                    AddToken(outcome.Entries, "damage.canCritical", "True");
                }
                if (hit.IgnoreDefense)
                {
                    AddToken(outcome.Entries, "damage.ignoreDefense", "True");
                }
            }

            if (IsBoundedPositive(hit.MaxHitCount))
            {
                AddValue(delivery.Entries, "maxHitCount", hit.MaxHitCount);
            }
            AddPositiveValue(delivery.Entries, "hitStartTime", hit.HitStartTime);
            AddPositiveValue(delivery.Entries, "repeatInterval", hit.RepeatInterval);
            AddValue(delivery.Entries, "split.hitCount", GreaterThanOneOrNull(hit.SplitHitCount));
            AddValue(delivery.Entries, "split.hitInterval", PositiveOrNull(hit.SplitHitInterval));

            AddEffectEntries(
                activation,
                outcome,
                specialEffect,
                linkedSkill,
                hit.Effects);
        }
    }

    private static void AddEffectEntries(
        GroupBuilder activation,
        GroupBuilder outcome,
        GroupBuilder specialEffect,
        GroupBuilder linkedSkill,
        IReadOnlyList<SkillEffectPresentationItem> effects)
    {
        if (effects == null)
        {
            return;
        }

        EffectPresentationGroupResolver resolver = new();
        foreach (SkillEffectPresentationItem item in effects)
        {
            EffectPresentationData effect = item?.Effect;
            if (effect == null)
            {
                continue;
            }

            activation.AddEntries(resolver.ResolveActivationEntries(effect));

            GroupBuilder resultGroup = effect.Outcome?.Kind switch
            {
                EffectOutcomeKind.Displacement => specialEffect,
                EffectOutcomeKind.Control => specialEffect,
                EffectOutcomeKind.SkillInvoke => linkedSkill,
                _ => outcome,
            };

            resultGroup.AddEntries(resolver.ResolveOutcomeEntries(effect));
            resultGroup.AddEntries(resolver.ResolveConstraintEntries(effect));
            resultGroup.AddDescription(effect.Description);
        }
    }

    private static void AddSpawnEntries(
        GroupBuilder outcome,
        SkillSpawnPresentationData spawn,
        bool filterDefaults)
    {
        if (spawn == null)
        {
            return;
        }

        ICollection<PresentationEntryData> entries = outcome.Entries;
        if (spawn.CharacterIdentity != null)
        {
            entries.Add(new PresentationEntryData(
                "character",
                new[]
                {
                    PresentationValueData.SemanticToken(
                        string.IsNullOrWhiteSpace(spawn.CharacterIdentity.DisplayName)
                            ? spawn.CharacterIdentity.ContentId
                            : spawn.CharacterIdentity.DisplayName),
                },
                spawn.CharacterIdentity.ContentId));
        }

        if (filterDefaults)
        {
            AddValue(entries, "spawnCount", GreaterThanOneOrNull(spawn.Count));
            AddValue(
                entries,
                "spawnInterval",
                IsGreaterThanOne(spawn.Count) ? PositiveOrNull(spawn.Interval) : null);
            AddValue(entries, "spawnLifeTime", PositiveOrNull(spawn.Lifetime));
        }
        else
        {
            AddValue(entries, "spawnCount", spawn.Count);
            AddValue(entries, "spawnInterval", spawn.Interval);
            AddValue(entries, "spawnLifeTime", spawn.Lifetime);
        }
    }

    private static void AddToken(
        ICollection<PresentationEntryData> entries,
        string key,
        string token)
    {
        AddValue(entries, key, PresentationValueData.SemanticToken(token));
    }

    private static void AddValue(
        ICollection<PresentationEntryData> entries,
        string key,
        PresentationValueData value)
    {
        if (value != null)
        {
            entries.Add(new PresentationEntryData(key, new[] { value }));
        }
    }

    private static void AddPositiveValue(
        ICollection<PresentationEntryData> entries,
        string key,
        PresentationValueData value)
    {
        AddValue(entries, key, PositiveOrNull(value));
    }

    private static PresentationEntryData FilterNonZeroValues(
        PresentationEntryData entry)
    {
        if (entry == null)
        {
            return null;
        }

        List<PresentationValueData> values = new();
        foreach (PresentationValueData value in entry.Values)
        {
            if (value == null)
            {
                continue;
            }

            if (value.Kind != PresentationValueKind.Number || IsNonZero(value))
            {
                values.Add(value);
            }
        }

        return values.Count > 0
            ? new PresentationEntryData(entry.Key, values, entry.DetailContentId)
            : null;
    }

    private static PresentationValueData PositiveOrNull(PresentationValueData value)
    {
        return IsPositive(value) ? value : null;
    }

    private static PresentationValueData GreaterThanOneOrNull(PresentationValueData value)
    {
        return IsGreaterThanOne(value) ? value : null;
    }

    private static PresentationValueData NonDefaultOneOrNull(PresentationValueData value)
    {
        return value != null
            && value.Kind == PresentationValueKind.Number
            && value.NumericValue > NumericEpsilon
            && Math.Abs(value.NumericValue - 1d) > NumericEpsilon
                ? value
                : null;
    }

    private static bool IsPositive(PresentationValueData value)
    {
        return value != null
            && value.Kind == PresentationValueKind.Number
            && value.NumericValue > NumericEpsilon;
    }

    private static bool IsNonZero(PresentationValueData value)
    {
        return value != null
            && value.Kind == PresentationValueKind.Number
            && Math.Abs(value.NumericValue) > NumericEpsilon;
    }

    private static bool IsGreaterThanOne(PresentationValueData value)
    {
        return value != null
            && value.Kind == PresentationValueKind.Number
            && value.NumericValue > 1d + NumericEpsilon;
    }

    private static bool IsBoundedPositive(PresentationValueData value)
    {
        return IsPositive(value) && value.NumericValue < UnboundedValueThreshold;
    }

    private static bool UsesRange(TargetingType targetingType)
    {
        return targetingType == TargetingType.AutoTarget
            || targetingType == TargetingType.AutoTargetDirection
            || targetingType == TargetingType.Directional
            || targetingType == TargetingType.Position;
    }

    private static void AddResolvedGroup(
        ICollection<PresentationGroupData> groups,
        GroupBuilder builder)
    {
        PresentationGroupData group = builder?.Build();
        if (group == null)
        {
            return;
        }

        groups.Add(group);
    }

    private sealed class GroupBuilder
    {
        private readonly List<string> descriptions = new();

        public string Key { get; }
        public List<PresentationEntryData> Entries { get; } = new();

        public GroupBuilder(string key)
        {
            Key = key ?? string.Empty;
        }

        public void AddEntries(IReadOnlyList<PresentationEntryData> entries)
        {
            if (entries == null)
            {
                return;
            }

            for (int index = 0; index < entries.Count; index++)
            {
                PresentationEntryData entry = entries[index];
                if (entry != null)
                {
                    Entries.Add(entry);
                }
            }
        }

        public void AddDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description)
                || descriptions.Contains(description))
            {
                return;
            }

            descriptions.Add(description);
        }

        public void FilterForPlayerDisplay()
        {
            Entries.RemoveAll(entry =>
                entry == null
                || !PresentationDisplayCatalog.IsPlayerVisibleEntry(entry.Key));
        }

        public PresentationGroupData Build()
        {
            string description = string.Join("\n", descriptions);
            return Entries.Count == 0 && string.IsNullOrWhiteSpace(description)
                ? null
                : new PresentationGroupData(Key, Entries, description);
        }
    }
}
