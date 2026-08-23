using System;
using System.Collections.Generic;
using Presentation;

namespace Effect
{
    public sealed class EffectPresentationGroupResolver
    {
        public PresentationGroupData Resolve(EffectPresentationData effect)
        {
            if (effect == null)
            {
                return null;
            }

            List<PresentationEntryData> entries = new();
            entries.AddRange(ResolveActivationEntries(effect));
            entries.AddRange(ResolveOutcomeEntries(effect));
            entries.AddRange(ResolveConstraintEntries(effect));

            if (entries.Count == 0 && string.IsNullOrWhiteSpace(effect.Description))
            {
                return null;
            }

            return new PresentationGroupData(
                GetOutcomeGroupKey(effect.Outcome),
                entries,
                effect.Description,
                effect.Identity?.ContentId);
        }

        public PresentationGroupData ResolveForPlayerDisplay(
            EffectPresentationData effect)
        {
            PresentationGroupData group = Resolve(effect);
            if (group == null)
            {
                return null;
            }

            List<PresentationEntryData> entries = new();
            foreach (PresentationEntryData entry in group.Entries)
            {
                if (entry != null
                    && PresentationDisplayCatalog.IsPlayerVisibleEntry(entry.Key))
                {
                    entries.Add(entry);
                }
            }

            if (entries.Count == 0
                && string.IsNullOrWhiteSpace(group.Description))
            {
                return null;
            }

            return new PresentationGroupData(
                group.Key,
                entries,
                group.Description,
                group.SourceContentId);
        }

        public IReadOnlyList<PresentationEntryData> ResolveActivationEntries(
            EffectPresentationData effect)
        {
            List<PresentationEntryData> entries = new();
            AddActivation(entries, effect?.Activation);
            return entries;
        }

        public IReadOnlyList<PresentationEntryData> ResolveOutcomeEntries(
            EffectPresentationData effect)
        {
            List<PresentationEntryData> entries = new();
            AddOutcome(entries, effect?.Outcome);

            if (effect != null
                && effect.Status != ContentPresentationStatus.Supported)
            {
                AddToken(entries, "status", effect.Status.ToString());
            }

            return entries;
        }

        public IReadOnlyList<PresentationEntryData> ResolveConstraintEntries(
            EffectPresentationData effect)
        {
            List<PresentationEntryData> entries = new();
            AddConstraints(entries, effect?.Constraints, effect?.Outcome);
            return entries;
        }

        private static void AddActivation(
            ICollection<PresentationEntryData> entries,
            EffectActivationPresentationData activation)
        {
            if (activation == null)
            {
                return;
            }

            AddToken(entries, "Activation.Trigger", activation.Trigger.ToString());
            AddSourceValue(entries, "Activation", "Chance", activation.Chance);

            if (activation.Target != EffectActivationTarget.None)
            {
                AddToken(entries, "Activation.Target", activation.Target.ToString());
            }

            if (activation.RequiresCriticalHit)
            {
                AddToken(entries, "Activation.RequiresCriticalHit", "True");
            }
        }

        private static void AddOutcome(
            ICollection<PresentationEntryData> entries,
            EffectOutcomePresentationData outcome)
        {
            switch (outcome)
            {
                case StatModifierPresentationData stat:
                    AddToken(entries, "StatModifier.Stat", stat.Stat.ToString());
                    AddToken(entries, "StatModifier.Operation", stat.Operation.ToString());
                    AddSourceValue(entries, "StatModifier", "Value", stat.Value);
                    AddSourceValue(entries, "StatModifier", "Duration", stat.Duration);
                    break;

                case HealPresentationData heal:
                    AddSourceValue(entries, "Heal", "MaximumHealthRatio", heal.MaximumHealthRatio);
                    AddSourceValue(entries, "Heal", "FlatAmount", heal.FlatAmount);
                    AddSourceValue(entries, "Heal", "AttackRatio", heal.AttackRatio);
                    if (heal.ClampToMaximumHealth)
                    {
                        AddToken(entries, "Heal.ClampToMaximumHealth", "True");
                    }
                    break;

                case PeriodicDamagePresentationData damage:
                    AddSourceValue(entries, "PeriodicDamage", "AttackRatio", damage.AttackRatio);
                    AddToken(entries, "PeriodicDamage.RateUnit", damage.RateUnit.ToString());
                    AddSourceValue(entries, "PeriodicDamage", "Interval", damage.Interval);
                    AddSourceValue(entries, "PeriodicDamage", "Duration", damage.Duration);
                    break;

                case CooldownChangePresentationData cooldown:
                    AddToken(entries, "CooldownChange.Kind", cooldown.ChangeKind.ToString());
                    AddSourceValue(entries, "CooldownChange", "Ratio", cooldown.Ratio);
                    AddSourceValue(entries, "CooldownChange", "FlatSeconds", cooldown.FlatSeconds);
                    break;

                case DisplacementPresentationData displacement:
                    AddToken(entries, "Displacement.Direction", displacement.Direction.ToString());
                    AddSourceValue(entries, "Displacement", "Magnitude", displacement.Magnitude);
                    break;

                case SkillInvokePresentationData invoke when invoke.SkillIdentity != null:
                    entries.Add(new PresentationEntryData(
                        "SkillInvoke.Skill",
                        new[]
                        {
                            PresentationValueData.SemanticToken(
                                string.IsNullOrWhiteSpace(invoke.SkillIdentity.DisplayName)
                                    ? invoke.SkillIdentity.ContentId
                                    : invoke.SkillIdentity.DisplayName),
                        },
                        invoke.SkillIdentity.ContentId));
                    AddSourceValue(entries, "SkillInvoke", "Range", invoke.Range);
                    break;

                case ControlPresentationData control:
                    AddToken(entries, "Control.Kind", control.ControlKind.ToString());
                    AddSourceValue(entries, "Control", "Duration", control.Duration);
                    break;
            }
        }

        private static void AddConstraints(
            ICollection<PresentationEntryData> entries,
            EffectEntryConstraintPresentationData constraints,
            EffectOutcomePresentationData outcome)
        {
            if (constraints == null)
            {
                return;
            }

            AddToken(entries, "categoryType", constraints.Category.ToString());
            AddToken(entries, "lifetimeType", constraints.Lifetime.ToString());
            if (!UsesConstraintDuration(outcome, constraints.Duration))
            {
                AddSourceValue(entries, string.Empty, "duration", constraints.Duration);
            }
            AddSourceValue(entries, string.Empty, "maxApplyCount", constraints.MaxApplyCount);
        }

        private static bool UsesConstraintDuration(
            EffectOutcomePresentationData outcome,
            PresentationValueData constraintDuration)
        {
            if (constraintDuration == null)
            {
                return false;
            }

            return outcome switch
            {
                ControlPresentationData control =>
                    IsSameSourceValue(control.Duration, constraintDuration),
                PeriodicDamagePresentationData damage =>
                    IsSameSourceValue(damage.Duration, constraintDuration),
                _ => false,
            };
        }

        private static bool IsSameSourceValue(
            PresentationValueData left,
            PresentationValueData right)
        {
            if (left == null || right == null)
            {
                return false;
            }

            return left.Kind == right.Kind
                && left.Unit == right.Unit
                && Math.Abs(left.NumericValue - right.NumericValue) < 0.0001d
                && string.Equals(
                    left.Provenance?.SourcePath,
                    right.Provenance?.SourcePath,
                    StringComparison.Ordinal)
                && string.Equals(
                    left.Provenance?.SourceField,
                    right.Provenance?.SourceField,
                    StringComparison.Ordinal);
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

        private static void AddSourceValue(
            ICollection<PresentationEntryData> entries,
            string normalizedGroup,
            string fallbackField,
            PresentationValueData value)
        {
            if (value == null)
            {
                return;
            }

            string sourceField = value.Provenance?.SourceField;
            string field = string.IsNullOrWhiteSpace(sourceField)
                ? fallbackField
                : ToJsonFieldPath(sourceField);
            string key = string.IsNullOrWhiteSpace(normalizedGroup)
                ? field
                : $"{normalizedGroup}.{field}";
            AddValue(entries, key, value);
        }

        private static string ToJsonFieldPath(string sourceField)
        {
            if (string.IsNullOrWhiteSpace(sourceField))
            {
                return string.Empty;
            }

            string[] segments = sourceField.Split('.');
            string field = segments[^1];
            if (field.Length == 0)
            {
                return field;
            }

            return char.ToLowerInvariant(field[0]) + field.Substring(1);
        }

        private static string GetOutcomeGroupKey(EffectOutcomePresentationData outcome)
        {
            return outcome?.Kind.ToString() ?? "Unsupported";
        }
    }
}
