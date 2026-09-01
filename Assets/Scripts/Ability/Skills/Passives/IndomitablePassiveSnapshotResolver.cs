using System;
using System.Collections.Generic;
using Effect;
using Skill;

namespace Character.Skill
{
    public static class IndomitablePassiveSnapshotResolver
    {
        public const string Revision = "indomitable.v2";
        public const float DamageReductionContributionCap = 20f;
        public const float EffectiveDefenseCap = 75f;

        private sealed class GradeContract
        {
            public readonly int Cap;
            public readonly float BaseAttack;
            public readonly float BaseDamageReduction;
            public readonly string AttackEffectId;
            public readonly string DamageReductionEffectId;
            public readonly float[] AttackDeltaByLevel;
            public readonly float[] DamageReductionDeltaByLevel;

            public GradeContract(
                int cap,
                float baseAttack,
                float baseDamageReduction,
                string attackEffectId,
                string damageReductionEffectId,
                float[] attackDeltaByLevel,
                float[] damageReductionDeltaByLevel)
            {
                Cap = cap;
                BaseAttack = baseAttack;
                BaseDamageReduction = baseDamageReduction;
                AttackEffectId = attackEffectId;
                DamageReductionEffectId = damageReductionEffectId;
                AttackDeltaByLevel = attackDeltaByLevel;
                DamageReductionDeltaByLevel = damageReductionDeltaByLevel;
            }
        }

        private static readonly float[] AttackDeltas =
            { 0f, 2f, 0f, 2f, 1f, 2f, 0f, 2f, 0f, 3f, 2f, 0f, 2f, 0f, 3f };

        private static readonly float[] DamageReductionDeltas =
            { 0f, 0f, 1f, 0f, 0f, 0f, 1f, 0f, 1f, 0f, 0f, 1f, 0f, 1f, 1f };

        private static readonly Dictionary<string, GradeContract> Contracts =
            new(StringComparer.Ordinal)
            {
                ["skill.character.seojin.1.passive_1.indomitable"] = CreateContract(1, 5, 20f, 10f, "self"),
                ["skill.character.seojin.2.passive_1.indomitable"] = CreateContract(2, 10, 23f, 11f, "buff"),
                ["skill.character.seojin.3.passive_1.indomitable"] = CreateContract(3, 15, 26f, 12f, "buff")
            };

        public static bool IsIndomitable(EquipmentSkillRuntimeData runtime)
        {
            string equipmentId = runtime?.sourceEquipment?.EquipmentId;
            return !string.IsNullOrEmpty(equipmentId) && Contracts.ContainsKey(equipmentId);
        }

        public static bool TryResolve(
            EquipmentSkillRuntimeData runtime,
            out ResolvedConditionalPassiveSnapshot snapshot,
            out string error)
        {
            snapshot = null;
            error = null;

            string equipmentId = runtime?.sourceEquipment?.EquipmentId;
            if (string.IsNullOrEmpty(equipmentId) || !Contracts.TryGetValue(equipmentId, out GradeContract contract))
            {
                error = "InvalidIndomitablePassiveDefinition: unsupported equipment.";
                return false;
            }

            int level = runtime.resolvedLevel > 0
                ? runtime.resolvedLevel
                : runtime.instanceData != null ? runtime.instanceData.currentLevel : 1;

            if (level < 1 || level > contract.Cap)
            {
                error = $"InvalidIndomitablePassiveDefinition: level={level} cap={contract.Cap}.";
                return false;
            }

            float attackDelta = 0f;
            float damageReductionDelta = 0f;
            IReadOnlyList<EffectUpgradeModifierData> modifiers =
                runtime.upgradeRuntimeData?.effectModifiers;

            if (modifiers != null)
            {
                for (int i = 0; i < modifiers.Count; i++)
                {
                    EffectUpgradeModifierData modifier = modifiers[i];
                    if (!TryAccumulateModifier(
                            modifier,
                            contract,
                            ref attackDelta,
                            ref damageReductionDelta,
                            out error))
                    {
                        return false;
                    }
                }
            }

            float expectedAttackDelta = SumThrough(contract.AttackDeltaByLevel, level);
            float expectedDamageReductionDelta = SumThrough(contract.DamageReductionDeltaByLevel, level);
            if (!Approximately(attackDelta, expectedAttackDelta) ||
                !Approximately(damageReductionDelta, expectedDamageReductionDelta))
            {
                error = "InvalidIndomitablePassiveDefinition: cumulative prefix mismatch.";
                return false;
            }

            float attack = contract.BaseAttack + attackDelta;
            float damageReduction = contract.BaseDamageReduction + damageReductionDelta;
            if (!IsFinite(attack) || !IsFinite(damageReduction) ||
                attack < 0f || attack > 45f ||
                damageReduction < 0f || damageReduction > DamageReductionContributionCap)
            {
                error = "InvalidIndomitablePassiveDefinition: resolved value outside caps.";
                return false;
            }

            snapshot = new ResolvedConditionalPassiveSnapshot(
                equipmentId,
                level,
                Revision,
                attack,
                damageReduction);
            return true;
        }

        private static bool TryAccumulateModifier(
            EffectUpgradeModifierData modifier,
            GradeContract contract,
            ref float attackDelta,
            ref float damageReductionDelta,
            out string error)
        {
            error = null;
            if (modifier == null ||
                modifier.FieldType != EffectModifierFieldType.Value ||
                modifier.OperationType != SkillStatModifierOperationType.Flat ||
                !IsFinite(modifier.Value) || modifier.Value < 0f)
            {
                error = "InvalidIndomitablePassiveDefinition: modifier allow-list violation.";
                return false;
            }

            if (string.Equals(modifier.TargetEffectId, contract.AttackEffectId, StringComparison.Ordinal))
            {
                attackDelta += modifier.Value;
                return true;
            }

            if (string.Equals(modifier.TargetEffectId, contract.DamageReductionEffectId, StringComparison.Ordinal))
            {
                damageReductionDelta += modifier.Value;
                return true;
            }

            error = "InvalidIndomitablePassiveDefinition: target effect mismatch.";
            return false;
        }

        private static GradeContract CreateContract(
            int grade,
            int cap,
            float baseAttack,
            float baseDamageReduction,
            string topology)
        {
            string prefix = $"skill.character.seojin.{grade}.passive_1.indomitable.effect.{topology}.";
            return new GradeContract(
                cap,
                baseAttack,
                baseDamageReduction,
                prefix + "1",
                prefix + "2",
                AttackDeltas,
                DamageReductionDeltas);
        }

        private static float SumThrough(float[] values, int level)
        {
            float result = 0f;
            for (int i = 0; i < level && i < values.Length; i++)
            {
                result += values[i];
            }
            return result;
        }

        private static bool Approximately(float left, float right) =>
            Math.Abs(left - right) <= 0.0001f;

        private static bool IsFinite(float value) =>
            !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
