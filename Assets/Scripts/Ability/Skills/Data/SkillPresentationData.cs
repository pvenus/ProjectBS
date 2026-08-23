using System;
using System.Collections.Generic;
using Effect;
using Presentation;

namespace Skill
{
    public enum SkillEffectSourceKind
    {
        SelfEffects = 100,
        BuffEffects = 200,
        DebuffEffects = 300,
    }

    [Serializable]
    public sealed class SkillPresentationData
    {
        private readonly SkillHitPresentationData[] hits;
        private readonly SkillEffectPresentationItem[] selfEffects;

        public PresentationIdentityData Identity { get; }
        public string Description { get; }
        public SkillClassificationPresentationData Classification { get; }
        public SkillCastPresentationData Cast { get; }
        public SkillProjectilePresentationData Projectile { get; }
        public IReadOnlyList<SkillHitPresentationData> Hits => hits;
        public IReadOnlyList<SkillEffectPresentationItem> SelfEffects => selfEffects;
        public SkillSpawnPresentationData Spawn { get; }
        public PresentationProvenanceData Provenance { get; }
        public ContentPresentationStatus Status { get; }

        public SkillPresentationData(
            PresentationIdentityData identity,
            string description,
            SkillClassificationPresentationData classification,
            SkillCastPresentationData cast,
            SkillProjectilePresentationData projectile,
            IReadOnlyList<SkillHitPresentationData> hits,
            IReadOnlyList<SkillEffectPresentationItem> selfEffects,
            SkillSpawnPresentationData spawn,
            PresentationProvenanceData provenance,
            ContentPresentationStatus status)
        {
            Identity = identity ?? new PresentationIdentityData(string.Empty, string.Empty);
            Description = description ?? string.Empty;
            Classification = classification;
            Cast = cast;
            Projectile = projectile;
            this.hits = Copy(hits);
            this.selfEffects = Copy(selfEffects);
            Spawn = spawn;
            Provenance = provenance;
            Status = status;
        }

        private static T[] Copy<T>(IReadOnlyList<T> source)
        {
            if (source == null || source.Count == 0)
            {
                return Array.Empty<T>();
            }

            T[] result = new T[source.Count];
            for (int index = 0; index < source.Count; index++)
            {
                result[index] = source[index];
            }

            return result;
        }
    }

    [Serializable]
    public sealed class SkillCastPresentationData
    {
        public TargetingType TargetingType { get; }
        public PresentationValueData Cooldown { get; }
        public PresentationValueData CastTime { get; }
        public PresentationValueData Range { get; }
        public PresentationValueData BurstCount { get; }
        public PresentationValueData BurstInterval { get; }
        public CastMoveType CastMoveType { get; }
        public PresentationValueData CastMoveDistance { get; }
        public PresentationValueData CastMoveDuration { get; }

        public SkillCastPresentationData(
            TargetingType targetingType,
            PresentationValueData cooldown,
            PresentationValueData castTime,
            PresentationValueData range,
            PresentationValueData burstCount,
            PresentationValueData burstInterval,
            CastMoveType castMoveType,
            PresentationValueData castMoveDistance,
            PresentationValueData castMoveDuration)
        {
            TargetingType = targetingType;
            Cooldown = cooldown;
            CastTime = castTime;
            Range = range;
            BurstCount = burstCount;
            BurstInterval = burstInterval;
            CastMoveType = castMoveType;
            CastMoveDistance = castMoveDistance;
            CastMoveDuration = castMoveDuration;
        }
    }

    [Serializable]
    public sealed class SkillProjectilePresentationData
    {
        private readonly PresentationEntryData[] movementParameters;

        public ProjectileMoveType MoveType { get; }
        public ProjectileArrangementType Arrangement { get; }
        public PresentationValueData Count { get; }
        public PresentationValueData Scale { get; }
        public PresentationValueData ColliderRadius { get; }
        public PresentationValueData Lifetime { get; }
        public PresentationValueData SpreadAngle { get; }
        public PresentationValueData ArrangementValue { get; }
        public PresentationValueData SpawnOffset { get; }
        public PresentationValueData SpawnInterval { get; }
        public PresentationValueData SpawnRadius { get; }
        public IReadOnlyList<PresentationEntryData> MovementParameters => movementParameters;

        public SkillProjectilePresentationData(
            ProjectileMoveType moveType,
            ProjectileArrangementType arrangement,
            PresentationValueData count,
            PresentationValueData scale,
            PresentationValueData colliderRadius,
            PresentationValueData lifetime,
            PresentationValueData spreadAngle,
            PresentationValueData arrangementValue,
            PresentationValueData spawnOffset,
            PresentationValueData spawnInterval,
            PresentationValueData spawnRadius,
            IReadOnlyList<PresentationEntryData> movementParameters)
        {
            MoveType = moveType;
            Arrangement = arrangement;
            Count = count;
            Scale = scale;
            ColliderRadius = colliderRadius;
            Lifetime = lifetime;
            SpreadAngle = spreadAngle;
            ArrangementValue = arrangementValue;
            SpawnOffset = spawnOffset;
            SpawnInterval = spawnInterval;
            SpawnRadius = spawnRadius;
            this.movementParameters = Copy(movementParameters);
        }

        private static PresentationEntryData[] Copy(IReadOnlyList<PresentationEntryData> source)
        {
            if (source == null || source.Count == 0)
            {
                return Array.Empty<PresentationEntryData>();
            }

            PresentationEntryData[] result = new PresentationEntryData[source.Count];
            for (int index = 0; index < source.Count; index++)
            {
                result[index] = source[index];
            }

            return result;
        }
    }

    [Serializable]
    public sealed class SkillHitPresentationData
    {
        private readonly SkillEffectPresentationItem[] effects;

        public string HitId { get; }
        public bool HasDamage { get; }
        public PresentationValueData TargetLayerMask { get; }
        public DamageType DamageType { get; }
        public PresentationValueData BaseDamage { get; }
        public PresentationValueData FirstHitBaseDamage { get; }
        public PresentationValueData AttackScaling { get; }
        public bool CanCritical { get; }
        public bool IgnoreDefense { get; }
        public PresentationValueData MaxHitCount { get; }
        public PresentationValueData HitStartTime { get; }
        public PresentationValueData RepeatInterval { get; }
        public PresentationValueData SplitHitCount { get; }
        public PresentationValueData SplitHitInterval { get; }
        public IReadOnlyList<SkillEffectPresentationItem> Effects => effects;

        public SkillHitPresentationData(
            string hitId,
            bool hasDamage,
            PresentationValueData targetLayerMask,
            DamageType damageType,
            PresentationValueData baseDamage,
            PresentationValueData firstHitBaseDamage,
            PresentationValueData attackScaling,
            bool canCritical,
            bool ignoreDefense,
            PresentationValueData maxHitCount,
            PresentationValueData hitStartTime,
            PresentationValueData repeatInterval,
            PresentationValueData splitHitCount,
            PresentationValueData splitHitInterval,
            IReadOnlyList<SkillEffectPresentationItem> effects)
        {
            HitId = hitId ?? string.Empty;
            HasDamage = hasDamage;
            TargetLayerMask = targetLayerMask;
            DamageType = damageType;
            BaseDamage = baseDamage;
            FirstHitBaseDamage = firstHitBaseDamage;
            AttackScaling = attackScaling;
            CanCritical = canCritical;
            IgnoreDefense = ignoreDefense;
            MaxHitCount = maxHitCount;
            HitStartTime = hitStartTime;
            RepeatInterval = repeatInterval;
            SplitHitCount = splitHitCount;
            SplitHitInterval = splitHitInterval;
            this.effects = Copy(effects);
        }

        private static SkillEffectPresentationItem[] Copy(
            IReadOnlyList<SkillEffectPresentationItem> source)
        {
            if (source == null || source.Count == 0)
            {
                return Array.Empty<SkillEffectPresentationItem>();
            }

            SkillEffectPresentationItem[] result = new SkillEffectPresentationItem[source.Count];
            for (int index = 0; index < source.Count; index++)
            {
                result[index] = source[index];
            }

            return result;
        }
    }

    [Serializable]
    public sealed class SkillEffectPresentationItem
    {
        public SkillEffectSourceKind SourceKind { get; }
        public string HitId { get; }
        public EffectPresentationData Effect { get; }

        public SkillEffectPresentationItem(
            SkillEffectSourceKind sourceKind,
            string hitId,
            EffectPresentationData effect)
        {
            SourceKind = sourceKind;
            HitId = hitId ?? string.Empty;
            Effect = effect;
        }
    }

    [Serializable]
    public sealed class SkillSpawnPresentationData
    {
        public PresentationIdentityData CharacterIdentity { get; }
        public PresentationValueData Count { get; }
        public PresentationValueData Interval { get; }
        public PresentationValueData Lifetime { get; }

        public SkillSpawnPresentationData(
            PresentationIdentityData characterIdentity,
            PresentationValueData count,
            PresentationValueData interval,
            PresentationValueData lifetime)
        {
            CharacterIdentity = characterIdentity;
            Count = count;
            Interval = interval;
            Lifetime = lifetime;
        }
    }
}
