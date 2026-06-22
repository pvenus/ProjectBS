

using System.Collections.Generic;
using Effect;
using Skill;
using Skills.Dto;
using UnityEngine;

namespace Effect.Helper
{
    /// <summary>
    /// Skill upgrade로 생성된 effect modifier를 hit/self effect entry에 반영하는 공용 헬퍼.
    /// EffectSO 원본은 수정하지 않고, entry override 값만 채운다.
    /// </summary>
    public static class EffectUpgradeApplyHelper
    {
        public static SkillProjectileHitEffectEntry CreateResolvedEntry(
            SkillProjectileHitEffectEntry source,
            IReadOnlyList<EffectUpgradeModifierData> modifiers)
        {
            if (source == null)
            {
                return null;
            }

            SkillProjectileHitEffectEntry resolved = CopyEntry(source);
            ApplyModifiers(
                resolved,
                modifiers);

            return resolved;
        }

        public static SkillProjectileHitEffectEntry CopyEntry(
            SkillProjectileHitEffectEntry source)
        {
            if (source == null)
            {
                return null;
            }

            return new SkillProjectileHitEffectEntry
            {
                effectSo = source.effectSo,
                lifetimeType = source.lifetimeType,
                categoryType = source.categoryType,
                duration = source.duration,
                maxApplyCount = Mathf.Max(0, source.maxApplyCount),
                hasValueOverride = source.hasValueOverride,
                valueOverride = source.valueOverride
            };
        }

        public static void ApplyModifiers(
            SkillProjectileHitEffectEntry entry,
            IReadOnlyList<EffectUpgradeModifierData> modifiers)
        {
            if (entry == null ||
                entry.effectSo == null ||
                modifiers == null ||
                modifiers.Count == 0)
            {
                return;
            }

            string effectId = entry.effectSo.effectId;

            for (int i = 0; i < modifiers.Count; i++)
            {
                EffectUpgradeModifierData modifier = modifiers[i];
                if (modifier == null ||
                    string.IsNullOrWhiteSpace(modifier.effectId) ||
                    modifier.effectId != effectId)
                {
                    continue;
                }

                ApplyModifier(entry, modifier);
            }
        }

        private static void ApplyModifier(
            SkillProjectileHitEffectEntry entry,
            EffectUpgradeModifierData modifier)
        {
            switch (modifier.fieldType)
            {
                case EffectModifierFieldType.Value:
                    entry.valueOverride = ApplyModifierValue(
                        ResolveEffectBaseValue(entry.effectSo),
                        modifier);
                    entry.hasValueOverride = true;
                    break;

                case EffectModifierFieldType.Duration:
                    entry.duration = ApplyModifierValue(
                        entry.duration,
                        modifier);
                    break;

                case EffectModifierFieldType.MaxApplyCount:
                    entry.maxApplyCount = Mathf.RoundToInt(
                        ApplyModifierValue(
                            entry.maxApplyCount,
                            modifier));
                    break;
            }
        }

        private static float ResolveEffectBaseValue(EffectSO effectSo)
        {
            if (effectSo == null)
            {
                return 0f;
            }

            switch (effectSo)
            {
                case StatModifierEffectSO statModifierEffectSo:
                    return statModifierEffectSo.value;

                default:
                    return 0f;
            }
        }

        private static float ApplyModifierValue(
            float currentValue,
            EffectUpgradeModifierData modifier)
        {
            switch (modifier.operationType)
            {
                case SkillStatModifierOperationType.Flat:
                    return currentValue + modifier.value;

                case SkillStatModifierOperationType.Percent:
                    return currentValue * (1f + modifier.value);

                case SkillStatModifierOperationType.Override:
                    return modifier.value;

                default:
                    return currentValue;
            }
        }
    }
}