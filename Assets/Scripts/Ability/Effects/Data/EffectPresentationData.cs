using System;
using System.Collections.Generic;
using Presentation;
using Stat;

namespace Effect
{
    public enum EffectActivationTrigger
    {
        None = 0,
        OnHit = 100,
        OnHeal = 200,
        OnAttack = 300,
    }

    public enum EffectActivationTarget
    {
        None = 0,
        AnyAlly = 100,
        Self = 200,
        OtherAlly = 300,
        Party = 400,
    }

    public enum EffectOutcomeKind
    {
        StatModifier = 100,
        Heal = 200,
        CooldownChange = 300,
        Displacement = 400,
        PeriodicDamage = 500,
        SkillInvoke = 600,
        Control = 700,
    }

    public enum PeriodicDamageRateUnit
    {
        PerSecond = 100,
        PerTick = 200,
    }

    public enum CooldownChangeKind
    {
        Reduce = 100,
        Increase = 200,
    }

    public enum EffectDisplacementDirection
    {
        PushAwayFromSource = 100,
        PullToSource = 200,
        ProjectileDirection = 300,
        CustomDirection = 400,
    }

    public enum EffectControlKind
    {
        Taunt = 100,
        Stun = 200,
        Root = 300,
    }

    [Serializable]
    public sealed class EffectPresentationData
    {
        public PresentationIdentityData Identity { get; }
        public string Description { get; }
        public EffectActivationPresentationData Activation { get; }
        public EffectOutcomePresentationData Outcome { get; }
        public EffectEntryConstraintPresentationData Constraints { get; }
        public PresentationProvenanceData Provenance { get; }
        public ContentPresentationStatus Status { get; }

        private EffectPresentationData(
            PresentationIdentityData identity,
            string description,
            EffectActivationPresentationData activation,
            EffectOutcomePresentationData outcome,
            EffectEntryConstraintPresentationData constraints,
            PresentationProvenanceData provenance,
            ContentPresentationStatus status)
        {
            Identity = identity ?? new PresentationIdentityData(string.Empty, string.Empty);
            Description = description ?? string.Empty;
            Activation = activation;
            Outcome = outcome;
            Constraints = constraints;
            Provenance = provenance;
            Status = status;
        }

        public static EffectPresentationData Supported(
            PresentationIdentityData identity,
            string description,
            EffectActivationPresentationData activation,
            EffectOutcomePresentationData outcome,
            EffectEntryConstraintPresentationData constraints,
            PresentationProvenanceData provenance)
        {
            if (outcome == null)
            {
                throw new ArgumentNullException(nameof(outcome));
            }

            return new EffectPresentationData(
                identity,
                description,
                activation,
                outcome,
                constraints,
                provenance,
                ContentPresentationStatus.Supported);
        }

        public static EffectPresentationData DescriptionOnly(
            PresentationIdentityData identity,
            string description,
            EffectEntryConstraintPresentationData constraints,
            PresentationProvenanceData provenance)
        {
            return new EffectPresentationData(
                identity,
                description,
                null,
                null,
                constraints,
                provenance,
                ContentPresentationStatus.DescriptionOnly);
        }

        public static EffectPresentationData Unsupported(
            PresentationIdentityData identity = null,
            EffectEntryConstraintPresentationData constraints = null,
            PresentationProvenanceData provenance = null)
        {
            return new EffectPresentationData(
                identity,
                string.Empty,
                null,
                null,
                constraints,
                provenance,
                ContentPresentationStatus.Unsupported);
        }
    }

    [Serializable]
    public sealed class EffectActivationPresentationData
    {
        public EffectActivationTrigger Trigger { get; }
        public PresentationValueData Chance { get; }
        public EffectActivationTarget Target { get; }
        public bool RequiresCriticalHit { get; }

        public EffectActivationPresentationData(
            EffectActivationTrigger trigger,
            PresentationValueData chance = null,
            EffectActivationTarget target = EffectActivationTarget.None,
            bool requiresCriticalHit = false)
        {
            Trigger = trigger;
            Chance = chance;
            Target = target;
            RequiresCriticalHit = requiresCriticalHit;
        }
    }

    [Serializable]
    public sealed class EffectEntryConstraintPresentationData
    {
        public EffectCategoryType Category { get; }
        public EffectLifetimeType Lifetime { get; }
        public PresentationValueData Duration { get; }
        public PresentationValueData MaxApplyCount { get; }

        public EffectEntryConstraintPresentationData(
            EffectCategoryType category,
            EffectLifetimeType lifetime,
            PresentationValueData duration,
            PresentationValueData maxApplyCount)
        {
            Category = category;
            Lifetime = lifetime;
            Duration = duration;
            MaxApplyCount = maxApplyCount;
        }
    }

    [Serializable]
    public abstract class EffectOutcomePresentationData
    {
        public EffectOutcomeKind Kind { get; }

        protected EffectOutcomePresentationData(EffectOutcomeKind kind)
        {
            Kind = kind;
        }
    }

    [Serializable]
    public sealed class StatModifierPresentationData
        : EffectOutcomePresentationData
    {
        public StatType Stat { get; }
        public StatModifierType Operation { get; }
        public PresentationValueData Value { get; }
        public PresentationValueData Duration { get; }

        public StatModifierPresentationData(
            StatType stat,
            StatModifierType operation,
            PresentationValueData value,
            PresentationValueData duration = null)
            : base(EffectOutcomeKind.StatModifier)
        {
            Stat = stat;
            Operation = operation;
            Value = value;
            Duration = duration;
        }
    }

    [Serializable]
    public sealed class HealPresentationData
        : EffectOutcomePresentationData
    {
        public PresentationValueData MaximumHealthRatio { get; }
        public PresentationValueData FlatAmount { get; }
        public PresentationValueData AttackRatio { get; }
        public bool ClampToMaximumHealth { get; }

        public HealPresentationData(
            PresentationValueData maximumHealthRatio,
            PresentationValueData flatAmount,
            PresentationValueData attackRatio,
            bool clampToMaximumHealth = false)
            : base(EffectOutcomeKind.Heal)
        {
            MaximumHealthRatio = maximumHealthRatio;
            FlatAmount = flatAmount;
            AttackRatio = attackRatio;
            ClampToMaximumHealth = clampToMaximumHealth;
        }
    }

    [Serializable]
    public sealed class PeriodicDamagePresentationData
        : EffectOutcomePresentationData
    {
        public PresentationValueData AttackRatio { get; }
        public PeriodicDamageRateUnit RateUnit { get; }
        public PresentationValueData Interval { get; }
        public PresentationValueData Duration { get; }

        public PeriodicDamagePresentationData(
            PresentationValueData attackRatio,
            PeriodicDamageRateUnit rateUnit,
            PresentationValueData interval = null,
            PresentationValueData duration = null)
            : base(EffectOutcomeKind.PeriodicDamage)
        {
            AttackRatio = attackRatio;
            RateUnit = rateUnit;
            Interval = interval;
            Duration = duration;
        }
    }

    [Serializable]
    public sealed class CooldownChangePresentationData
        : EffectOutcomePresentationData
    {
        public CooldownChangeKind ChangeKind { get; }
        public PresentationValueData Ratio { get; }
        public PresentationValueData FlatSeconds { get; }

        public CooldownChangePresentationData(
            CooldownChangeKind changeKind,
            PresentationValueData ratio,
            PresentationValueData flatSeconds)
            : base(EffectOutcomeKind.CooldownChange)
        {
            ChangeKind = changeKind;
            Ratio = ratio;
            FlatSeconds = flatSeconds;
        }
    }

    [Serializable]
    public sealed class DisplacementPresentationData
        : EffectOutcomePresentationData
    {
        public EffectDisplacementDirection Direction { get; }
        public PresentationValueData Magnitude { get; }

        public DisplacementPresentationData(
            EffectDisplacementDirection direction,
            PresentationValueData magnitude)
            : base(EffectOutcomeKind.Displacement)
        {
            Direction = direction;
            Magnitude = magnitude;
        }
    }

    [Serializable]
    public sealed class SkillInvokePresentationData
        : EffectOutcomePresentationData
    {
        public PresentationIdentityData SkillIdentity { get; }
        public PresentationValueData Range { get; }

        public SkillInvokePresentationData(
            PresentationIdentityData skillIdentity,
            PresentationValueData range = null)
            : base(EffectOutcomeKind.SkillInvoke)
        {
            SkillIdentity = skillIdentity;
            Range = range;
        }
    }

    [Serializable]
    public sealed class ControlPresentationData
        : EffectOutcomePresentationData
    {
        public EffectControlKind ControlKind { get; }
        public PresentationValueData Duration { get; }

        public ControlPresentationData(
            EffectControlKind controlKind,
            PresentationValueData duration = null)
            : base(EffectOutcomeKind.Control)
        {
            ControlKind = controlKind;
            Duration = duration;
        }
    }
}
