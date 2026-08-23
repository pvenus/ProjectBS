using System;
using System.Collections.Generic;

namespace Presentation
{
    public enum PresentationEntryValueFormatKind
    {
        Default = 0,
        DamageType = 100,
        ControlType = 200,
        DisplacementType = 300,
    }

    public sealed class PresentationEntryDisplayRule
    {
        public string EntryKey { get; }
        public string LabelKey { get; }
        public string TokenKeyPrefix { get; }
        public string ValueFormatKey { get; }
        public PresentationEntryValueFormatKind FormatKind { get; }
        public bool PlayerVisible { get; }
        public bool TokenIsLocalizedText { get; }

        public PresentationEntryDisplayRule(
            string entryKey,
            string labelKey,
            bool playerVisible,
            string tokenKeyPrefix = null,
            string valueFormatKey = null,
            PresentationEntryValueFormatKind formatKind = PresentationEntryValueFormatKind.Default,
            bool tokenIsLocalizedText = false)
        {
            EntryKey = entryKey ?? string.Empty;
            LabelKey = labelKey ?? string.Empty;
            TokenKeyPrefix = tokenKeyPrefix ?? string.Empty;
            ValueFormatKey = valueFormatKey ?? string.Empty;
            FormatKind = formatKind;
            PlayerVisible = playerVisible;
            TokenIsLocalizedText = tokenIsLocalizedText;
        }
    }

    public static class PresentationDisplayCatalog
    {
        private static readonly IReadOnlyDictionary<string, string> GroupLabelKeys =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["Activation"] = "presentation.group.activation",
                ["Delivery"] = "presentation.group.delivery",
                ["Outcome"] = "presentation.group.outcome",
                ["SpecialEffect"] = "presentation.group.special_effect",
                ["LinkedSkill"] = "presentation.group.linked_skill",
                ["StatModifier"] = "presentation.group.stat_modifier",
                ["Heal"] = "presentation.group.heal",
                ["CooldownChange"] = "presentation.group.cooldown_change",
                ["Displacement"] = "presentation.group.displacement",
                ["PeriodicDamage"] = "presentation.group.periodic_damage",
                ["SkillInvoke"] = "presentation.group.skill_invoke",
                ["Control"] = "presentation.group.control",
                ["Bless.Duration"] = "presentation.group.bless_duration",
                ["Bless.Runtime"] = "presentation.group.bless_runtime",
                ["Relic.Runtime"] = "presentation.group.relic_runtime",
                ["Character.Stats"] = "presentation.group.character_stats",
            };

        private static readonly IReadOnlyDictionary<string, PresentationEntryDisplayRule> EntryRules =
            CreateEntryRules();

        private static readonly HashSet<string> PlayerTagTokens = new(StringComparer.Ordinal)
        {
            "Active",
            "Passive",
            "Attack",
            "Support",
            "Defense",
            "Heal",
            "Control",
            "Buff",
            "Debuff",
            "Utility",
            "Self",
            "Ally",
            "Enemy",
            "Party",
            "Point",
        };

        private static readonly HashSet<string> PlayerQualifiedTags = new(StringComparer.Ordinal)
        {
            "Bless.Category.Common",
            "Bless.Category.Offense",
            "Bless.Category.Defense",
            "Bless.Category.Utility",
            "Bless.Category.Support",
            "Bless.Category.Economy",
            "Bless.Category.Survival",
            "Bless.Category.Special",
            "Bless.Duration.Permanent",
            "Bless.Duration.NextBattle",
            "Bless.Duration.BattleCount",
            "Bless.God.Life",
            "Bless.God.War",
            "Bless.God.Greed",
            "Bless.God.Dark",
            "Relic.Rarity.Common",
            "Relic.Rarity.Rare",
            "Relic.Rarity.Epic",
            "Relic.Rarity.Legendary",
            "Character.Type.Npc",
            "Character.Type.Boss",
            "Character.Type.Player",
            "Character.Job.SoldierBase",
            "Character.Job.ArcherBase",
            "Character.Job.ScholarBase",
            "Character.Job.MonkBase",
        };

        public static bool TryGetEntryRule(
            string entryKey,
            out PresentationEntryDisplayRule rule)
        {
            if (string.IsNullOrWhiteSpace(entryKey))
            {
                rule = null;
                return false;
            }

            return EntryRules.TryGetValue(entryKey, out rule);
        }

        public static bool IsPlayerVisibleEntry(string entryKey)
        {
            return TryGetEntryRule(entryKey, out PresentationEntryDisplayRule rule)
                && rule.PlayerVisible;
        }

        public static string GetGroupLabelKey(string groupKey)
        {
            return !string.IsNullOrWhiteSpace(groupKey)
                && GroupLabelKeys.TryGetValue(groupKey, out string labelKey)
                    ? labelKey
                    : string.Empty;
        }

        public static string GetEntryLabelKey(string entryKey)
        {
            return TryGetEntryRule(entryKey, out PresentationEntryDisplayRule rule)
                ? rule.LabelKey
                : string.Empty;
        }

        public static string GetEntryTokenKey(
            string entryKey,
            string token)
        {
            if (string.IsNullOrWhiteSpace(token)
                || !TryGetEntryRule(entryKey, out PresentationEntryDisplayRule rule)
                || rule.TokenIsLocalizedText)
            {
                return string.Empty;
            }

            return string.IsNullOrWhiteSpace(rule.TokenKeyPrefix)
                ? $"presentation.token.{token}"
                : $"{rule.TokenKeyPrefix}.{token}";
        }

        public static bool IsEntryTokenLocalizedText(string entryKey)
        {
            return TryGetEntryRule(entryKey, out PresentationEntryDisplayRule rule)
                && rule.TokenIsLocalizedText;
        }

        public static string GetValueFormatKey(string entryKey)
        {
            return TryGetEntryRule(entryKey, out PresentationEntryDisplayRule rule)
                ? rule.ValueFormatKey
                : string.Empty;
        }

        public static bool IsPlayerVisibleTag(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            return PlayerTagTokens.Contains(token)
                || PlayerQualifiedTags.Contains(token);
        }

        public static string GetTagLabelKey(string token)
        {
            if (!IsPlayerVisibleTag(token))
            {
                return string.Empty;
            }

            int separator = token.LastIndexOf('.');
            string value = separator >= 0 && separator + 1 < token.Length
                ? token.Substring(separator + 1)
                : token;
            return $"presentation.tag.{value}";
        }

        private static IReadOnlyDictionary<string, PresentationEntryDisplayRule> CreateEntryRules()
        {
            Dictionary<string, PresentationEntryDisplayRule> rules =
                new(StringComparer.Ordinal);

            Add(rules, "targetingType", "presentation.entry.targeting", true,
                "presentation.targeting");
            Add(rules, "cooldown", "presentation.entry.cooldown", true);
            Add(rules, "castTime", "presentation.entry.cast_time", true);
            Add(rules, "range", "presentation.entry.range", true);
            Add(rules, "burst.count", "presentation.entry.burst_count", true);
            Add(rules, "burst.interval", "presentation.entry.burst_interval", true);
            Add(rules, "castMove.moveType", "presentation.entry.cast_move_type", true,
                "presentation.cast_move");
            Add(rules, "castMove.distance", "presentation.entry.cast_move_distance", true);
            Add(rules, "castMove.duration", "presentation.entry.cast_move_duration", true);
            Add(rules, "projectileCount", "presentation.entry.projectile_count", true);
            Add(rules, "projectileColliderRadius", "presentation.entry.effect_range", true);
            Add(rules, "projectileLifetime", "presentation.entry.duration", true);
            Add(rules, "projectile.arrangement", "presentation.entry.projectile_arrangement", true,
                "presentation.arrangement");
            Add(rules, "projectile.spreadAngle", "presentation.entry.spread_angle", true);
            Add(rules, "projectile.radius", "presentation.entry.arrangement_radius", true);
            Add(rules, "projectileSpawn.interval", "presentation.entry.projectile_spawn_interval", true);
            Add(rules, "maxHitCount", "presentation.entry.max_hit_count", true);
            Add(rules, "repeatInterval", "presentation.entry.repeat_interval", true);
            Add(rules, "split.hitCount", "presentation.entry.split_hit_count", true);
            Add(rules, "split.hitInterval", "presentation.entry.split_hit_interval", true);
            Add(rules, "damage.damageType", "presentation.entry.damage_type", true,
                "presentation.damage", "presentation.format.damage_type",
                PresentationEntryValueFormatKind.DamageType);
            Add(rules, "damage.baseDamage", "presentation.entry.base_damage", true);
            Add(rules, "damage.firstHitBaseDamage", "presentation.entry.first_hit_damage", true);
            Add(rules, "damage.attackPercentDamage", "presentation.entry.attack_scaling", true);
            Add(rules, "damage.canCritical", "presentation.entry.can_critical", true,
                "presentation.boolean");
            Add(rules, "damage.ignoreDefense", "presentation.entry.ignore_defense", true,
                "presentation.boolean");
            Add(rules, "character", "presentation.entry.summoned_character", true,
                tokenIsLocalizedText: true);
            Add(rules, "spawnCount", "presentation.entry.spawn_count", true);
            Add(rules, "spawnInterval", "presentation.entry.spawn_interval", true);
            Add(rules, "spawnLifeTime", "presentation.entry.spawn_lifetime", true);

            Add(rules, "Activation.Trigger", "presentation.entry.activation_trigger", true,
                "presentation.trigger");
            Add(rules, "Activation.chance", "presentation.entry.activation_chance", true);
            Add(rules, "Activation.chancePercent", "presentation.entry.activation_chance", true);
            Add(rules, "Activation.Target", "presentation.entry.activation_target", true,
                "presentation.target");
            Add(rules, "Activation.RequiresCriticalHit", "presentation.entry.critical_condition", true,
                "presentation.boolean");
            Add(rules, "StatModifier.Stat", "presentation.entry.stat", true,
                "presentation.stat");
            Add(rules, "StatModifier.Operation", "presentation.entry.operation", true,
                "presentation.operation");
            Add(rules, "StatModifier.value", "presentation.entry.modifier_value", true);
            Add(rules, "StatModifier.durationSeconds", "presentation.entry.duration", true);
            Add(rules, "Heal.maxHpPercent", "presentation.entry.max_health_ratio", true);
            Add(rules, "Heal.flatHealAmount", "presentation.entry.heal_amount", true);
            Add(rules, "Heal.attackPercentHeal", "presentation.entry.attack_scaling", true);
            Add(rules, "CooldownChange.Kind", "presentation.entry.cooldown_change_type", true,
                "presentation.cooldown_change");
            Add(rules, "CooldownChange.reducePercent", "presentation.entry.cooldown_reduction_ratio", true);
            Add(rules, "CooldownChange.reduceSeconds", "presentation.entry.cooldown_reduction_time", true);
            Add(rules, "Displacement.Direction", "presentation.entry.displacement_type", true,
                "presentation.displacement", "presentation.format.displacement_type",
                PresentationEntryValueFormatKind.DisplacementType);
            Add(rules, "Displacement.force", "presentation.entry.effect_magnitude", true);
            Add(rules, "Displacement.distanceMeters", "presentation.entry.effect_distance", true);
            Add(rules, "PeriodicDamage.attackRatioPercent", "presentation.entry.attack_scaling", true);
            Add(rules, "PeriodicDamage.attackRatioPercentPerTick", "presentation.entry.attack_scaling_per_tick", true);
            Add(rules, "PeriodicDamage.RateUnit", "presentation.entry.periodic_rate", true,
                "presentation.periodic_rate");
            Add(rules, "PeriodicDamage.tickIntervalSeconds", "presentation.entry.interval", true);
            Add(rules, "PeriodicDamage.durationSeconds", "presentation.entry.duration", true);
            Add(rules, "SkillInvoke.Skill", "presentation.entry.linked_skill", true,
                tokenIsLocalizedText: true);
            Add(rules, "SkillInvoke.Range", "presentation.entry.effect_range", true);
            Add(rules, "Control.Kind", "presentation.entry.control_type", true,
                "presentation.control", "presentation.format.control_type",
                PresentationEntryValueFormatKind.ControlType);
            Add(rules, "Control.value", "presentation.entry.duration", true);
            Add(rules, "Control.duration", "presentation.entry.duration", true);
            Add(rules, "duration", "presentation.entry.duration", true);
            Add(rules, "maxApplyCount", "presentation.entry.max_apply_count", true);
            Add(rules, "Bless.Duration.Battles", "presentation.entry.bless_duration_battles", true);
            Add(rules, "Bless.Runtime.Level", "presentation.entry.bless_level", true);
            Add(rules, "Bless.Runtime.RemainingBattles", "presentation.entry.remaining_battles", true);

            Add(rules, "Character.Stat.Attack", "presentation.stat.Attack", true);
            Add(rules, "Character.Stat.Defense", "presentation.stat.Defense", true);
            Add(rules, "Character.Stat.MaxHp", "presentation.stat.MaxHp", true);
            Add(rules, "Character.Stat.AttackSpeed", "presentation.stat.AttackSpeed", true,
                valueFormatKey: "presentation.format.multiplier");
            Add(rules, "Character.Stat.CritChance", "presentation.stat.CritChance", true);
            Add(rules, "Character.Stat.CritDamage", "presentation.stat.CritDamage", true);
            Add(rules, "Character.Stat.MoveSpeed", "presentation.stat.MoveSpeed", true);

            Add(rules, "projectileScale", "presentation.internal.projectile_scale", false);
            Add(rules, "projectile.arrangementValue", "presentation.internal.arrangement_value", false);
            Add(rules, "projectileSpawn.spawnOffset", "presentation.internal.spawn_offset", false);
            Add(rules, "moveType", "presentation.internal.move_type", false);
            Add(rules, "config.speed", "presentation.internal.move_speed", false);
            Add(rules, "config.turnSpeed", "presentation.internal.turn_speed", false);
            Add(rules, "config.orbitRadius", "presentation.internal.orbit_radius", false);
            Add(rules, "config.orbitAngularSpeed", "presentation.internal.orbit_speed", false);
            Add(rules, "config.clockwise", "presentation.internal.orbit_direction", false);
            Add(rules, "config.followOffset.x", "presentation.internal.follow_offset_x", false);
            Add(rules, "config.followOffset.y", "presentation.internal.follow_offset_y", false);
            Add(rules, "targetLayerMask", "presentation.internal.target_layer", false);
            Add(rules, "hitStartTime", "presentation.internal.hit_start_time", false);
            Add(rules, "Heal.ClampToMaximumHealth", "presentation.internal.heal_clamp", false);
            Add(rules, "categoryType", "presentation.internal.effect_category", false);
            Add(rules, "lifetimeType", "presentation.internal.effect_lifetime", false);
            Add(rules, "status", "presentation.internal.resolution_status", false);
            Add(rules, "Bless.Runtime.Equipped", "presentation.internal.equipped", false);
            Add(rules, "Bless.Runtime.Locked", "presentation.internal.locked", false);
            Add(rules, "Relic.Runtime.Equipped", "presentation.internal.equipped", false);
            Add(rules, "Relic.Runtime.HasOwner", "presentation.internal.has_owner", false);

            return rules;
        }

        private static void Add(
            IDictionary<string, PresentationEntryDisplayRule> rules,
            string entryKey,
            string labelKey,
            bool playerVisible,
            string tokenKeyPrefix = null,
            string valueFormatKey = null,
            PresentationEntryValueFormatKind formatKind = PresentationEntryValueFormatKind.Default,
            bool tokenIsLocalizedText = false)
        {
            rules.Add(
                entryKey,
                new PresentationEntryDisplayRule(
                    entryKey,
                    labelKey,
                    playerVisible,
                    tokenKeyPrefix,
                    valueFormatKey,
                    formatKind,
                    tokenIsLocalizedText));
        }
    }
}
