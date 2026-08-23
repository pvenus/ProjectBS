using System;
using System.Linq;
using System.Text;
using Effect;
using Presentation;
using Skill;
using Stat;
using UnityEditor;
using UnityEngine;

namespace EffectEditor
{
    public static class EffectPresentationStage4SelfTest
    {
        private static readonly string[] ApprovedAssetRoots =
        {
            "Assets/Resources/skill/character/generated",
            "Assets/Resources/skill/json",
        };

        [MenuItem(
            "Tools/ProjectBS/Presentation/Run Effect Mapping Self Test")]
        public static void RunFromMenu()
        {
            int syntheticCount = VerifySyntheticMappings();
            int approvedCount = VerifyApprovedAssets();

            Debug.Log(
                "[EffectPresentationStage4SelfTest] PASS\n"
                + $"Synthetic mapping cases: {syntheticCount}\n"
                + $"Approved EffectEntry assets: {approvedCount}");
        }

        [MenuItem(
            "Assets/ProjectBS/Presentation/Log Selected Effect Entry",
            false,
            2100)]
        public static void LogSelectedEntry()
        {
            EffectEntrySO entry = Selection.activeObject as EffectEntrySO;
            Ensure(entry != null, "Select one EffectEntrySO asset first.");

            EffectPresentationData data =
                new EffectPresentationResolver().Resolve(
                    entry,
                    PresentationContext.Preview);

            Debug.Log(BuildSummary(entry, data), entry);
        }

        [MenuItem(
            "Assets/ProjectBS/Presentation/Log Selected Effect Entry",
            true)]
        private static bool CanLogSelectedEntry()
        {
            return Selection.activeObject is EffectEntrySO;
        }

        private static int VerifySyntheticMappings()
        {
            int count = 0;

            Verify(
                CreateStatModifier(),
                data =>
                {
                    StatModifierPresentationData outcome =
                        RequireOutcome<StatModifierPresentationData>(data);
                    Ensure(outcome.Operation == StatModifierType.Percent,
                        "StatModifier operation changed.");
                    EnsureValue(outcome.Value, 0.2d, PresentationValueUnit.Ratio,
                        "StatModifier ratio");
                });
            count++;

            Verify(
                CreateChanceOnHitStatModifier(),
                data =>
                {
                    EnsureActivation(data, EffectActivationTrigger.OnHit, 50d);
                    StatModifierPresentationData outcome =
                        RequireOutcome<StatModifierPresentationData>(data);
                    EnsureValue(outcome.Value, -15d, PresentationValueUnit.Percent,
                        "ChanceOnHit StatModifier value");
                });
            count++;

            Verify(
                CreateOnHitTimedStatModifier(),
                data =>
                {
                    EnsureActivation(data, EffectActivationTrigger.OnHit, 75d);
                    StatModifierPresentationData outcome =
                        RequireOutcome<StatModifierPresentationData>(data);
                    EnsureValue(outcome.Duration, 3d, PresentationValueUnit.Seconds,
                        "Timed StatModifier duration");
                });
            count++;

            Verify(
                CreateOnHitTimedControl(StatType.StunDuration, 2.25f),
                data =>
                {
                    EnsureActivation(data, EffectActivationTrigger.OnHit, 75d);
                    ControlPresentationData outcome =
                        RequireOutcome<ControlPresentationData>(data);
                    Ensure(outcome.ControlKind == EffectControlKind.Stun,
                        "Stun duration stat must normalize as Control(Stun).");
                    EnsureValue(outcome.Duration, 2.25d, PresentationValueUnit.Seconds,
                        "Stun duration");
                });
            count++;

            Verify(
                CreateOnHitTimedControl(StatType.RootDuration, 1.5f),
                data =>
                {
                    EnsureActivation(data, EffectActivationTrigger.OnHit, 75d);
                    ControlPresentationData outcome =
                        RequireOutcome<ControlPresentationData>(data);
                    Ensure(outcome.ControlKind == EffectControlKind.Root,
                        "Root duration stat must normalize as Control(Root).");
                    EnsureValue(outcome.Duration, 1.5d, PresentationValueUnit.Seconds,
                        "Root duration");
                });
            count++;

            Verify(
                CreateChanceOnHealStatModifier(),
                data =>
                {
                    EnsureActivation(
                        data,
                        EffectActivationTrigger.OnHeal,
                        0.25d,
                        PresentationValueUnit.Ratio,
                        EffectActivationTarget.Self);
                    StatModifierPresentationData outcome =
                        RequireOutcome<StatModifierPresentationData>(data);
                    Ensure(outcome.Operation == StatModifierType.Flat,
                        "ChanceOnHeal must reflect the runtime flat AddStat behavior.");
                    EnsureValue(outcome.Value, 12d, PresentationValueUnit.Flat,
                        "ChanceOnHeal StatModifier value");
                });
            count++;

            Verify(
                CreateHeal(),
                data =>
                {
                    HealPresentationData outcome =
                        RequireOutcome<HealPresentationData>(data);
                    Ensure(outcome.MaximumHealthRatio != null
                        && outcome.FlatAmount != null
                        && outcome.AttackRatio != null,
                        "Heal source fields must remain separately available.");
                    Ensure(outcome.ClampToMaximumHealth,
                        "Runtime heal clamp must remain visible.");
                });
            count++;

            Verify(
                CreateChanceOnHealCooldown(),
                data =>
                {
                    EnsureActivation(
                        data,
                        EffectActivationTrigger.OnHeal,
                        0.5d,
                        PresentationValueUnit.Ratio,
                        EffectActivationTarget.OtherAlly);
                    CooldownChangePresentationData outcome =
                        RequireOutcome<CooldownChangePresentationData>(data);
                    EnsureValue(outcome.Ratio, 0.2d, PresentationValueUnit.Ratio,
                        "Heal-trigger cooldown ratio");
                    EnsureValue(outcome.FlatSeconds, 1.5d, PresentationValueUnit.Seconds,
                        "Heal-trigger cooldown seconds");
                });
            count++;

            Verify(
                CreateCooldown(),
                data =>
                {
                    CooldownChangePresentationData outcome =
                        RequireOutcome<CooldownChangePresentationData>(data);
                    Ensure(outcome.Ratio == null,
                        "Flat cooldown mapping invented a ratio.");
                    EnsureValue(outcome.FlatSeconds, 2d, PresentationValueUnit.Seconds,
                        "Cooldown seconds");
                });
            count++;

            Verify(
                CreateKnockback(),
                data =>
                {
                    DisplacementPresentationData outcome =
                        RequireOutcome<DisplacementPresentationData>(data);
                    Ensure(outcome.Direction == EffectDisplacementDirection.PullToSource,
                        "Knockback direction changed.");
                    EnsureValue(outcome.Magnitude, 6d, PresentationValueUnit.Force,
                        "Knockback force");
                });
            count++;

            Verify(
                CreateOnHitDistance(),
                data =>
                {
                    EnsureActivation(data, EffectActivationTrigger.OnHit, 80d);
                    DisplacementPresentationData outcome =
                        RequireOutcome<DisplacementPresentationData>(data);
                    Ensure(outcome.Direction == EffectDisplacementDirection.PushAwayFromSource,
                        "Distance displacement must match the runtime's effective direction set.");
                    EnsureValue(outcome.Magnitude, 4d, PresentationValueUnit.Meters,
                        "Displacement distance");
                });
            count++;

            Verify(
                CreateAttackBleed(),
                data =>
                {
                    EnsureActivation(data, EffectActivationTrigger.OnAttack, 30d);
                    PeriodicDamagePresentationData outcome =
                        RequireOutcome<PeriodicDamagePresentationData>(data);
                    Ensure(outcome.RateUnit == PeriodicDamageRateUnit.PerSecond,
                        "Attack bleed PeriodicDamage changed.");
                    EnsureValue(outcome.Duration, 4d, PresentationValueUnit.Seconds,
                        "Attack bleed duration");
                },
                EffectLifetimeType.Timed,
                4f);
            count++;

            Verify(
                CreatePoison(),
                data =>
                {
                    EnsureActivation(data, EffectActivationTrigger.OnHit, 40d);
                    PeriodicDamagePresentationData outcome =
                        RequireOutcome<PeriodicDamagePresentationData>(data);
                    Ensure(outcome.RateUnit == PeriodicDamageRateUnit.PerTick,
                        "Poison rate unit changed.");
                    EnsureValue(outcome.Interval, 0.01d, PresentationValueUnit.Seconds,
                        "Poison authored interval");
                    EnsureValue(outcome.Duration, 5d, PresentationValueUnit.Seconds,
                        "Poison duration");
                });
            count++;

            EquipmentSkillSO skill = ScriptableObject.CreateInstance<EquipmentSkillSO>();
            try
            {
                SerializedObject serializedSkill = new(skill);
                serializedSkill.FindProperty("equipmentId").stringValue = "test.skill.invoke";
                serializedSkill.ApplyModifiedPropertiesWithoutUndo();

                Verify(
                    CreateChanceOnHitSkill(skill),
                    data =>
                    {
                        EnsureActivation(data, EffectActivationTrigger.OnHit, 60d);
                        Ensure(data.Activation.RequiresCriticalHit,
                            "Skill invoke critical condition changed.");
                        SkillInvokePresentationData outcome =
                            RequireOutcome<SkillInvokePresentationData>(data);
                        Ensure(outcome.SkillIdentity.ContentId == "test.skill.invoke",
                            "Invoked skill identity changed.");
                        Ensure(outcome.Range == null,
                            "Unused RangeOverride must not be presented as active.");
                    });
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(skill);
            }
            count++;

            Verify(
                new TauntEffectConfig(),
                data =>
                {
                    ControlPresentationData outcome =
                        RequireOutcome<ControlPresentationData>(data);
                    Ensure(outcome.ControlKind == EffectControlKind.Taunt,
                        "Taunt control kind changed.");
                    EnsureValue(outcome.Duration, 2.5d, PresentationValueUnit.Seconds,
                        "Taunt duration");
                },
                EffectLifetimeType.Instant,
                2.5f);
            count++;

            return count;
        }

        private static int VerifyApprovedAssets()
        {
            string[] guids = AssetDatabase.FindAssets(
                "t:EffectEntrySO",
                ApprovedAssetRoots);
            Ensure(guids.Length > 0, "No approved EffectEntrySO assets were found.");

            EffectPresentationResolver resolver = new();
            int verified = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                EffectEntrySO entry =
                    AssetDatabase.LoadAssetAtPath<EffectEntrySO>(path);
                Ensure(entry != null, $"Could not load EffectEntrySO: {path}");

                EffectPresentationData data =
                    resolver.Resolve(entry, PresentationContext.Preview);
                Ensure(
                    data.Status == ContentPresentationStatus.Supported,
                    $"Approved asset did not resolve as Supported: {path}, {data.Status}");
                verified++;
            }

            return verified;
        }

        private static void Verify(
            EffectConfig config,
            Action<EffectPresentationData> assertion,
            EffectLifetimeType lifetime = EffectLifetimeType.Instant,
            float duration = 0f)
        {
            EffectSO effect = ScriptableObject.CreateInstance<EffectSO>();
            EffectEntrySO entry = ScriptableObject.CreateInstance<EffectEntrySO>();

            try
            {
                effect.ApplyEditorData(
                    $"test.effect.{config.GetType().Name}",
                    null,
                    config);
                entry.ApplyEditorData(
                    effect,
                    lifetime,
                    EffectCategoryType.Buff,
                    duration,
                    1,
                    false,
                    0f);

                EffectPresentationData data =
                    new EffectPresentationResolver().Resolve(
                        entry,
                        PresentationContext.Preview);
                Ensure(data.Status == ContentPresentationStatus.Supported,
                    $"{config.GetType().Name} did not resolve as Supported.");
                assertion(data);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(entry);
                UnityEngine.Object.DestroyImmediate(effect);
            }
        }

        private static T RequireOutcome<T>(EffectPresentationData data)
            where T : EffectOutcomePresentationData
        {
            Ensure(data.Outcome is T,
                $"Expected {typeof(T).Name}, got {data.Outcome?.GetType().Name ?? "null"}.");
            return (T)data.Outcome;
        }

        private static void EnsureActivation(
            EffectPresentationData data,
            EffectActivationTrigger trigger,
            double chancePercent,
            PresentationValueUnit unit = PresentationValueUnit.Percent,
            EffectActivationTarget target = EffectActivationTarget.None)
        {
            Ensure(data.Activation != null, "Activation is missing.");
            Ensure(data.Activation.Trigger == trigger,
                $"Expected trigger {trigger}, got {data.Activation.Trigger}.");
            Ensure(data.Activation.Target == target,
                $"Expected target {target}, got {data.Activation.Target}.");
            EnsureValue(
                data.Activation.Chance,
                chancePercent,
                unit,
                "Activation chance");
        }

        private static void EnsureValue(
            PresentationValueData value,
            double expected,
            PresentationValueUnit unit,
            string label)
        {
            Ensure(value != null, $"{label} is missing.");
            Ensure(Math.Abs(value.NumericValue - expected) < 0.0001d,
                $"{label} expected {expected}, got {value.NumericValue}.");
            Ensure(value.Unit == unit,
                $"{label} expected unit {unit}, got {value.Unit}.");
        }

        private static string BuildSummary(
            EffectEntrySO entry,
            EffectPresentationData data)
        {
            StringBuilder builder = new();
            builder.AppendLine("[EffectPresentation] Selected Entry");
            builder.AppendLine($"Asset: {AssetDatabase.GetAssetPath(entry)}");
            builder.AppendLine($"Status: {data.Status}");
            builder.AppendLine($"ContentId: {data.Identity.ContentId}");
            builder.AppendLine($"Activation: {DescribeActivation(data.Activation)}");
            builder.AppendLine($"Outcome: {DescribeOutcome(data.Outcome)}");
            builder.AppendLine(
                $"Constraints: {data.Constraints.Category}, "
                + $"{data.Constraints.Lifetime}, "
                + $"duration={DescribeValue(data.Constraints.Duration)}, "
                + $"maxApply={DescribeValue(data.Constraints.MaxApplyCount)}");
            return builder.ToString();
        }

        private static string DescribeActivation(
            EffectActivationPresentationData activation)
        {
            if (activation == null)
            {
                return "None";
            }

            return $"{activation.Trigger}, "
                + $"chance={DescribeValue(activation.Chance)}, "
                + $"target={activation.Target}, "
                + $"critical={activation.RequiresCriticalHit}";
        }

        private static string DescribeOutcome(
            EffectOutcomePresentationData outcome)
        {
            if (outcome == null)
            {
                return "None";
            }

            switch (outcome)
            {
                case StatModifierPresentationData stat:
                    return $"StatModifier({stat.Stat}, {stat.Operation}, "
                        + $"{DescribeValue(stat.Value)}, duration={DescribeValue(stat.Duration)})";
                case HealPresentationData heal:
                    return $"Heal(maxHp={DescribeValue(heal.MaximumHealthRatio)}, "
                        + $"flat={DescribeValue(heal.FlatAmount)}, "
                        + $"attack={DescribeValue(heal.AttackRatio)}, "
                        + $"clamp={heal.ClampToMaximumHealth})";
                case PeriodicDamagePresentationData damage:
                    return $"PeriodicDamage(attack={DescribeValue(damage.AttackRatio)}, "
                        + $"rate={damage.RateUnit}, "
                        + $"interval={DescribeValue(damage.Interval)}, "
                        + $"duration={DescribeValue(damage.Duration)})";
                case CooldownChangePresentationData cooldown:
                    return $"CooldownChange({cooldown.ChangeKind}, "
                        + $"ratio={DescribeValue(cooldown.Ratio)}, "
                        + $"seconds={DescribeValue(cooldown.FlatSeconds)})";
                case DisplacementPresentationData displacement:
                    return $"Displacement({displacement.Direction}, "
                        + $"{DescribeValue(displacement.Magnitude)})";
                case SkillInvokePresentationData skill:
                    return $"SkillInvoke({skill.SkillIdentity.ContentId})";
                case ControlPresentationData control:
                    return $"Control({control.ControlKind}, "
                        + $"duration={DescribeValue(control.Duration)})";
                default:
                    return outcome.Kind.ToString();
            }
        }

        private static string DescribeValue(PresentationValueData value)
        {
            return value == null
                ? "None"
                : $"{value.NumericValue:0.####} {value.Unit}";
        }

        private static StatModifierEffectConfig CreateStatModifier()
        {
            StatModifierEffectConfig config = new();
            config.ApplyEditorData(
                StatType.Attack,
                StatModifierType.Percent,
                0.2f);
            return config;
        }

        private static ChanceOnHitStatModifierEffectConfig CreateChanceOnHitStatModifier()
        {
            ChanceOnHitStatModifierEffectConfig config = new();
            config.ApplyEditorData(
                50f,
                StatType.MoveSpeed,
                StatModifierType.Percent,
                -15f);
            return config;
        }

        private static OnHitTimedStatModifierEffectConfig CreateOnHitTimedStatModifier()
        {
            OnHitTimedStatModifierEffectConfig config = new();
            config.ApplyEditorData(
                75f,
                StatType.MoveSpeed,
                StatModifierType.Percent,
                -20f,
                3f);
            return config;
        }

        private static OnHitTimedStatModifierEffectConfig CreateOnHitTimedControl(
            StatType statType,
            float duration)
        {
            OnHitTimedStatModifierEffectConfig config = new();
            config.ApplyEditorData(
                75f,
                statType,
                StatModifierType.Flat,
                duration,
                duration);
            return config;
        }

        private static ChanceOnHealStatModifierEffectConfig CreateChanceOnHealStatModifier()
        {
            ChanceOnHealStatModifierEffectConfig config = new();
            config.ApplyEditorData(
                0.25f,
                HealTriggerTargetType.Self,
                StatType.Attack,
                StatModifierType.Percent,
                12f);
            return config;
        }

        private static HealEffectConfig CreateHeal()
        {
            HealEffectConfig config = new();
            config.ApplyEditorData(
                true,
                0.1f,
                5f,
                true,
                0.2f,
                false);
            return config;
        }

        private static ChanceOnHealCooldownReduceEffectConfig CreateChanceOnHealCooldown()
        {
            ChanceOnHealCooldownReduceEffectConfig config = new();
            config.ApplyEditorData(
                0.5f,
                HealTriggerTargetType.OtherAlly,
                CooldownReduceType.PercentAndFlat,
                0.2f,
                1.5f);
            return config;
        }

        private static CooldownReduceEffectConfig CreateCooldown()
        {
            CooldownReduceEffectConfig config = new();
            config.ApplyEditorData(
                CooldownReduceType.FlatSeconds,
                0.5f,
                2f);
            return config;
        }

        private static KnockbackEffectConfig CreateKnockback()
        {
            KnockbackEffectConfig config = new();
            config.ApplyEditorData(
                6f,
                KnockbackDirectionType.PullToSource,
                Vector2.up,
                true,
                true);
            return config;
        }

        private static OnHitKnockbackDistanceEffectConfig CreateOnHitDistance()
        {
            OnHitKnockbackDistanceEffectConfig config = new();
            config.ApplyEditorData(
                80f,
                4f,
                KnockbackDirectionType.CustomDirection);
            return config;
        }

        private static AttackBleedEffectConfig CreateAttackBleed()
        {
            AttackBleedEffectConfig config = new();
            config.ApplyEditorData(30f, 25f);
            return config;
        }

        private static OnHitPoisonDotEffectConfig CreatePoison()
        {
            OnHitPoisonDotEffectConfig config = new();
            config.ApplyEditorData(40f, 15f, 0.01f, 5f);
            return config;
        }

        private static ChanceOnHitSkillEffectConfig CreateChanceOnHitSkill(
            EquipmentSkillSO skill)
        {
            ChanceOnHitSkillEffectConfig config = new();
            config.ApplyEditorData(60f, true, skill, 7f);
            return config;
        }

        private static void Ensure(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
