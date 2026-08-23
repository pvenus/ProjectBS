using System.Collections.Generic;
using Presentation;
using Skill;
using Stat;
using UnityEngine;

namespace Effect
{
    public sealed class EffectPresentationResolver
    {
        public EffectPresentationData Resolve(
            EffectEntrySO entry,
            PresentationContext context)
        {
            if (entry == null)
            {
                return EffectPresentationData.Unsupported(
                    provenance: new PresentationProvenanceData(
                        PresentationProvenanceKind.Unknown));
            }

            EffectEntryConstraintPresentationData constraints =
                CreateConstraints(entry);

            EffectSO effect = entry.EffectSO;

            if (effect == null)
            {
                return EffectPresentationData.Unsupported(
                    constraints: constraints,
                    provenance: CreateEntryProvenance(entry));
            }

            PresentationIdentityData identity =
                CreateIdentity(effect);
            string description = ResolveDescription(effect);

            switch (effect.Config)
            {
                case StatModifierEffectConfig config:
                    return CreateSupportedResult(
                        identity,
                        description,
                        null,
                        CreateStatModifier(
                            effect,
                            config.TargetStat,
                            config.ModifierType,
                            config.Value,
                            GetStatModifierUnit(
                                config.ModifierType,
                                PresentationValueUnit.Ratio),
                            "Value"),
                        constraints,
                        effect);

                case ChanceOnHitStatModifierEffectConfig config:
                    if (config.ValueType == StatModifierType.Multiply)
                    {
                        return CreateUnmappedResult(
                            identity,
                            description,
                            constraints,
                            effect);
                    }

                    return CreateSupportedResult(
                        identity,
                        description,
                        CreatePercentActivation(
                            effect,
                            EffectActivationTrigger.OnHit,
                            config.ChancePercent,
                            "ChancePercent"),
                        CreateStatModifier(
                            effect,
                            config.StatType,
                            config.ValueType,
                            config.Value,
                            GetStatModifierUnit(
                                config.ValueType,
                                PresentationValueUnit.Percent),
                            "Value"),
                        constraints,
                        effect);

                case OnHitTimedStatModifierEffectConfig config:
                    if (TryMapDurationControl(
                            config.StatType,
                            out EffectControlKind controlKind))
                    {
                        return CreateSupportedResult(
                            identity,
                            description,
                            CreatePercentActivation(
                                effect,
                                EffectActivationTrigger.OnHit,
                                config.ChancePercent,
                                "ChancePercent"),
                            new ControlPresentationData(
                                controlKind,
                                CreateEffectValue(
                                    effect,
                                    config.Value,
                                    PresentationValueUnit.Seconds,
                                    "Value")),
                            constraints,
                            effect);
                    }

                    return CreateSupportedResult(
                        identity,
                        description,
                        CreatePercentActivation(
                            effect,
                            EffectActivationTrigger.OnHit,
                            config.ChancePercent,
                            "ChancePercent"),
                        CreateStatModifier(
                            effect,
                            config.StatType,
                            config.ModifierType,
                            config.Value,
                            GetStatModifierUnit(
                                config.ModifierType,
                                PresentationValueUnit.Percent),
                            "Value",
                            CreateEffectValue(
                                effect,
                                config.DurationSeconds,
                                PresentationValueUnit.Seconds,
                                "DurationSeconds")),
                        constraints,
                        effect);

                case ChanceOnHealStatModifierEffectConfig config:
                    return CreateSupportedResult(
                        identity,
                        description,
                        CreateRatioActivation(
                            effect,
                            EffectActivationTrigger.OnHeal,
                            config.Chance,
                            "Chance",
                            MapActivationTarget(config.TriggerTargetType)),
                        CreateStatModifier(
                            effect,
                            config.StatType,
                            StatModifierType.Flat,
                            config.Value,
                            PresentationValueUnit.Flat,
                            "Value"),
                        constraints,
                        effect);

                case HealEffectConfig config:
                    return CreateSupportedResult(
                        identity,
                        description,
                        null,
                        CreateHeal(effect, config),
                        constraints,
                        effect);

                case ChanceOnHealCooldownReduceEffectConfig config:
                    return CreateSupportedResult(
                        identity,
                        description,
                        CreateRatioActivation(
                            effect,
                            EffectActivationTrigger.OnHeal,
                            config.Chance,
                            "Chance",
                            MapActivationTarget(config.TriggerTargetType)),
                        CreateCooldownChange(
                            effect,
                            config.ReduceType,
                            config.ReducePercent,
                            config.ReduceSeconds),
                        constraints,
                        effect);

                case CooldownReduceEffectConfig config:
                    return CreateSupportedResult(
                        identity,
                        description,
                        null,
                        CreateCooldownChange(
                            effect,
                            config.ReduceType,
                            config.ReducePercent,
                            config.ReduceSeconds),
                        constraints,
                        effect);

                case KnockbackEffectConfig config:
                    return CreateSupportedResult(
                        identity,
                        description,
                        null,
                        new DisplacementPresentationData(
                            MapDirection(config.DirectionType),
                            CreateEffectValue(
                                effect,
                                config.Force,
                                PresentationValueUnit.Force,
                                "Force")),
                        constraints,
                        effect);

                case OnHitKnockbackDistanceEffectConfig config:
                    return CreateSupportedResult(
                        identity,
                        description,
                        CreatePercentActivation(
                            effect,
                            EffectActivationTrigger.OnHit,
                            config.ChancePercent,
                            "ChancePercent"),
                        new DisplacementPresentationData(
                            MapDistanceDirection(config.DirectionType),
                            CreateEffectValue(
                                effect,
                                config.DistanceMeters,
                                PresentationValueUnit.Meters,
                                "DistanceMeters")),
                        constraints,
                        effect);

                case AttackBleedEffectConfig config:
                    return CreateSupportedResult(
                        identity,
                        description,
                        CreatePercentActivation(
                            effect,
                            EffectActivationTrigger.OnAttack,
                            config.ChancePercent,
                            "ChancePercent"),
                        CreatePeriodicDamage(
                            effect,
                            config.AttackRatioPercent,
                            "AttackRatioPercent",
                            PeriodicDamageRateUnit.PerSecond,
                            duration: constraints.Duration),
                        constraints,
                        effect);

                case OnHitPoisonDotEffectConfig config:
                    return CreateSupportedResult(
                        identity,
                        description,
                        CreatePercentActivation(
                            effect,
                            EffectActivationTrigger.OnHit,
                            config.ChancePercent,
                            "ChancePercent"),
                        CreatePeriodicDamage(
                            effect,
                            config.AttackRatioPercentPerTick,
                            "AttackRatioPercentPerTick",
                            PeriodicDamageRateUnit.PerTick,
                            CreateEffectValue(
                                effect,
                                config.TickIntervalSeconds,
                                PresentationValueUnit.Seconds,
                                "TickIntervalSeconds"),
                            CreateEffectValue(
                                effect,
                                config.DurationSeconds,
                                PresentationValueUnit.Seconds,
                                "DurationSeconds")),
                        constraints,
                        effect);

                case ChanceOnHitSkillEffectConfig config:
                    if (config.SkillSo == null)
                    {
                        return CreateUnmappedResult(
                            identity,
                            description,
                            constraints,
                            effect);
                    }

                    return CreateSupportedResult(
                        identity,
                        description,
                        CreatePercentActivation(
                            effect,
                            EffectActivationTrigger.OnHit,
                            config.Chance,
                            "Chance",
                            requiresCriticalHit: config.RequireCriticalHit),
                        new SkillInvokePresentationData(
                            CreateSkillIdentity(config.SkillSo)),
                        constraints,
                        effect);

                case TauntEffectConfig _:
                    return CreateSupportedResult(
                        identity,
                        description,
                        null,
                        new ControlPresentationData(
                            EffectControlKind.Taunt,
                            CreateEntryValue(
                                entry,
                                entry.Duration,
                                PresentationValueUnit.Seconds,
                                "Duration")),
                        constraints,
                        effect);

                default:
                    return CreateUnmappedResult(
                        identity,
                        description,
                        constraints,
                        effect);
            }
        }

        private static EffectPresentationData CreateSupportedResult(
            PresentationIdentityData identity,
            string description,
            EffectActivationPresentationData activation,
            EffectOutcomePresentationData outcome,
            EffectEntryConstraintPresentationData constraints,
            EffectSO effect)
        {
            return EffectPresentationData.Supported(
                identity,
                description,
                activation,
                outcome,
                constraints,
                CreateEffectProvenance(
                    effect,
                    PresentationProvenanceKind.AuthoredAsset));
        }

        private static EffectActivationPresentationData CreatePercentActivation(
            EffectSO effect,
            EffectActivationTrigger trigger,
            float chancePercent,
            string sourceField,
            EffectActivationTarget target = EffectActivationTarget.None,
            bool requiresCriticalHit = false)
        {
            return new EffectActivationPresentationData(
                trigger,
                CreateEffectValue(
                    effect,
                    chancePercent,
                    PresentationValueUnit.Percent,
                    sourceField),
                target,
                requiresCriticalHit);
        }

        private static EffectActivationPresentationData CreateRatioActivation(
            EffectSO effect,
            EffectActivationTrigger trigger,
            float chanceRatio,
            string sourceField,
            EffectActivationTarget target = EffectActivationTarget.None)
        {
            return new EffectActivationPresentationData(
                trigger,
                CreateEffectValue(
                    effect,
                    chanceRatio,
                    PresentationValueUnit.Ratio,
                    sourceField),
                target);
        }

        private static StatModifierPresentationData CreateStatModifier(
            EffectSO effect,
            StatType stat,
            StatModifierType operation,
            float value,
            PresentationValueUnit unit,
            string sourceField,
            PresentationValueData duration = null)
        {
            return new StatModifierPresentationData(
                stat,
                operation,
                CreateEffectValue(
                    effect,
                    value,
                    unit,
                    sourceField),
                duration);
        }

        private static PresentationValueUnit GetStatModifierUnit(
            StatModifierType operation,
            PresentationValueUnit percentUnit)
        {
            return operation switch
            {
                StatModifierType.Flat => PresentationValueUnit.Flat,
                StatModifierType.Percent => percentUnit,
                StatModifierType.Multiply => PresentationValueUnit.Ratio,
                _ => PresentationValueUnit.None,
            };
        }

        private static HealPresentationData CreateHeal(
            EffectSO effect,
            HealEffectConfig config)
        {
            PresentationValueData maximumHealthRatio = config.UseMaxHpPercent
                ? CreateEffectValue(
                    effect,
                    config.MaxHpPercent,
                    PresentationValueUnit.Ratio,
                    "MaxHpPercent")
                : null;
            PresentationValueData flatAmount = !Mathf.Approximately(config.FlatHealAmount, 0f)
                ? CreateEffectValue(
                    effect,
                    config.FlatHealAmount,
                    PresentationValueUnit.Flat,
                    "FlatHealAmount")
                : null;
            PresentationValueData attackRatio = config.UseAttackScaling
                ? CreateEffectValue(
                    effect,
                    config.AttackPercentHeal,
                    PresentationValueUnit.Ratio,
                    "AttackPercentHeal")
                : null;

            return new HealPresentationData(
                maximumHealthRatio,
                flatAmount,
                attackRatio,
                clampToMaximumHealth: true);
        }

        private static CooldownChangePresentationData CreateCooldownChange(
            EffectSO effect,
            CooldownReduceType reduceType,
            float reducePercent,
            float reduceSeconds)
        {
            bool usesRatio = reduceType == CooldownReduceType.Percent
                || reduceType == CooldownReduceType.PercentAndFlat;
            bool usesSeconds = reduceType == CooldownReduceType.FlatSeconds
                || reduceType == CooldownReduceType.PercentAndFlat;

            PresentationValueData ratio = usesRatio
                ? CreateEffectValue(
                    effect,
                    reducePercent,
                    PresentationValueUnit.Ratio,
                    "ReducePercent")
                : null;

            PresentationValueData seconds = usesSeconds
                ? CreateEffectValue(
                    effect,
                    reduceSeconds,
                    PresentationValueUnit.Seconds,
                    "ReduceSeconds")
                : null;

            return new CooldownChangePresentationData(
                CooldownChangeKind.Reduce,
                ratio,
                seconds);
        }

        private static PeriodicDamagePresentationData CreatePeriodicDamage(
            EffectSO effect,
            float attackRatioPercent,
            string sourceField,
            PeriodicDamageRateUnit rateUnit,
            PresentationValueData interval = null,
            PresentationValueData duration = null)
        {
            return new PeriodicDamagePresentationData(
                CreateEffectValue(
                    effect,
                    attackRatioPercent,
                    PresentationValueUnit.Percent,
                    sourceField),
                rateUnit,
                interval,
                duration);
        }

        private static EffectActivationTarget MapActivationTarget(
            HealTriggerTargetType target)
        {
            return target switch
            {
                HealTriggerTargetType.AnyAlly => EffectActivationTarget.AnyAlly,
                HealTriggerTargetType.Self => EffectActivationTarget.Self,
                HealTriggerTargetType.OtherAlly => EffectActivationTarget.OtherAlly,
                HealTriggerTargetType.Party => EffectActivationTarget.Party,
                _ => EffectActivationTarget.None,
            };
        }

        private static EffectDisplacementDirection MapDirection(
            KnockbackDirectionType direction)
        {
            return direction switch
            {
                KnockbackDirectionType.PushAwayFromSource =>
                    EffectDisplacementDirection.PushAwayFromSource,
                KnockbackDirectionType.PullToSource =>
                    EffectDisplacementDirection.PullToSource,
                KnockbackDirectionType.ProjectileDirection =>
                    EffectDisplacementDirection.ProjectileDirection,
                KnockbackDirectionType.CustomDirection =>
                    EffectDisplacementDirection.CustomDirection,
                _ => EffectDisplacementDirection.PushAwayFromSource,
            };
        }

        private static EffectDisplacementDirection MapDistanceDirection(
            KnockbackDirectionType direction)
        {
            return direction == KnockbackDirectionType.PullToSource
                ? EffectDisplacementDirection.PullToSource
                : EffectDisplacementDirection.PushAwayFromSource;
        }

        private static bool TryMapDurationControl(
            StatType stat,
            out EffectControlKind controlKind)
        {
            switch (stat)
            {
                case StatType.StunDuration:
                    controlKind = EffectControlKind.Stun;
                    return true;
                case StatType.RootDuration:
                    controlKind = EffectControlKind.Root;
                    return true;
                default:
                    controlKind = default;
                    return false;
            }
        }

        private static EffectPresentationData CreateUnmappedResult(
            PresentationIdentityData identity,
            string description,
            EffectEntryConstraintPresentationData constraints,
            EffectSO effect)
        {
            if (!string.IsNullOrWhiteSpace(description))
            {
                return EffectPresentationData.DescriptionOnly(
                    identity,
                    description,
                    constraints,
                    CreateEffectProvenance(
                        effect,
                        PresentationProvenanceKind.AuthoredDescriptionFallback,
                        "Description"));
            }

            return EffectPresentationData.Unsupported(
                identity,
                constraints,
                CreateEffectProvenance(
                    effect,
                    PresentationProvenanceKind.AuthoredAsset));
        }

        private static PresentationIdentityData CreateIdentity(
            EffectSO effect)
        {
            string displayName = PresentationLocalizedTextResolver.ResolveName(
                effect.name,
                effect.LocalizationMainKey);

            return new PresentationIdentityData(
                effect.EffectId,
                displayName,
                effect.Icon);
        }

        private static PresentationIdentityData CreateSkillIdentity(
            EquipmentSkillSO skill)
        {
            if (skill == null)
            {
                return new PresentationIdentityData(
                    string.Empty,
                    string.Empty);
            }

            string displayName = PresentationLocalizedTextResolver.ResolveName(
                skill.name,
                GetStrategicItemLocalizationKey(skill.LocalizationMainKey),
                skill.LocalizationMainKey);

            return new PresentationIdentityData(
                skill.EquipmentId,
                displayName,
                skill.Icon);
        }

        private static string GetStrategicItemLocalizationKey(string skillKey)
        {
            const string skillPrefix = "skill.strategic.";
            const string itemPrefix = "item.strategic.";
            return !string.IsNullOrWhiteSpace(skillKey)
                && skillKey.StartsWith(skillPrefix, System.StringComparison.Ordinal)
                    ? itemPrefix + skillKey.Substring(skillPrefix.Length)
                    : string.Empty;
        }

        private static string ResolveDescription(EffectSO effect)
        {
            if (effect == null)
            {
                return string.Empty;
            }

            return PresentationLocalizedTextResolver.ResolveRequired(
                "desc",
                effect.LocalizationMainKey);
        }

        private static EffectEntryConstraintPresentationData CreateConstraints(
            EffectEntrySO entry)
        {
            PresentationValueData duration =
                UsesEntryDuration(entry.LifetimeType)
                    ? PresentationValueData.Number(
                        entry.Duration,
                        PresentationValueUnit.Seconds,
                        CreateEntryProvenance(entry, "Duration"))
                    : null;

            PresentationValueData maxApplyCount =
                PresentationValueData.Number(
                    entry.MaxApplyCount,
                    PresentationValueUnit.Count,
                    CreateEntryProvenance(entry, "MaxApplyCount"));

            return new EffectEntryConstraintPresentationData(
                entry.CategoryType,
                entry.LifetimeType,
                duration,
                maxApplyCount);
        }

        private static bool UsesEntryDuration(
            EffectLifetimeType lifetime)
        {
            return lifetime == EffectLifetimeType.Timed
                || lifetime == EffectLifetimeType.CombatTimed;
        }

        private static PresentationProvenanceData CreateEntryProvenance(
            EffectEntrySO entry,
            string sourceField = null)
        {
            return new PresentationProvenanceData(
                PresentationProvenanceKind.AuthoredAsset,
                entry != null ? entry.name : string.Empty,
                sourceField: sourceField);
        }

        private static PresentationValueData CreateEffectValue(
            EffectSO effect,
            double value,
            PresentationValueUnit unit,
            string sourceField)
        {
            return PresentationValueData.Number(
                value,
                unit,
                CreateEffectProvenance(
                    effect,
                    PresentationProvenanceKind.AuthoredAsset,
                    sourceField));
        }

        private static PresentationValueData CreateEntryValue(
            EffectEntrySO entry,
            double value,
            PresentationValueUnit unit,
            string sourceField)
        {
            return PresentationValueData.Number(
                value,
                unit,
                CreateEntryProvenance(entry, sourceField));
        }

        private static PresentationProvenanceData CreateEffectProvenance(
            EffectSO effect,
            PresentationProvenanceKind kind,
            string sourceField = null)
        {
            return new PresentationProvenanceData(
                kind,
                effect != null ? effect.EffectId : string.Empty,
                sourceField: sourceField);
        }
    }
}
