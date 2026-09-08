using Character;
using UnityEngine;

namespace Effect
{
    public class EffectResolver
    {
        public EffectEntryRuntime[] ResolveEntries(
            EffectEntrySO[] entries,
            GameObject owner,
            GameObject target,
            EffectCategoryType defaultCategoryType,
            System.Collections.Generic.IReadOnlyList<EffectUpgradeModifierData> effectUpgradeModifiers = null)
        {
            if (entries == null || entries.Length == 0)
            {
                return null;
            }

            CharacterManager sourceCharacter = owner != null
                ? owner.GetComponent<CharacterManager>()
                : null;

            CharacterManager targetCharacter = target != null
                ? target.GetComponent<CharacterManager>()
                : null;

            Transform sourceTransform = owner != null
                ? owner.transform
                : null;

            var results = new System.Collections.Generic.List<EffectEntryRuntime>();

            for (int i = 0; i < entries.Length; i++)
            {
                EffectEntrySO entry = entries[i];

                if (entry == null)
                {
                    continue;
                }

                EffectEntryRuntime resolvedEntry = Resolve(
                    entry,
                    targetCharacter,
                    sourceCharacter,
                    sourceTransform);

                if (resolvedEntry == null)
                {
                    continue;
                }

                resolvedEntry = ResolveSwiftStepSnapshot(
                    entry,
                    resolvedEntry,
                    targetCharacter,
                    effectUpgradeModifiers);

                EffectCategoryType categoryType = resolvedEntry.CategoryType;

                results.Add(new EffectEntryRuntime(
                    resolvedEntry.RuntimeData,
                    resolvedEntry.LifetimeType,
                    categoryType,
                    resolvedEntry.Duration,
                    resolvedEntry.MaxApplyCount));
            }

            return results.Count > 0
                ? results.ToArray()
                : null;
        }

        private static EffectEntryRuntime ResolveSwiftStepSnapshot(
            EffectEntrySO source,
            EffectEntryRuntime resolved,
            CharacterManager target,
            System.Collections.Generic.IReadOnlyList<EffectUpgradeModifierData> modifiers)
        {
            string effectId = source?.EffectSO?.EffectId;
            if (string.IsNullOrEmpty(effectId) ||
                effectId.IndexOf(".active_4.swift_step.effect.self.", System.StringComparison.Ordinal) < 0)
            {
                return resolved;
            }

            if (!(source.EffectSO.Config is StatModifierEffectConfig config) || target == null)
            {
                return null;
            }

            float value = config.Value;
            float duration = source.Duration;
            if (modifiers != null)
            {
                for (int i = 0; i < modifiers.Count; i++)
                {
                    EffectUpgradeModifierData modifier = modifiers[i];
                    if (modifier == null ||
                        !string.Equals(modifier.TargetEffectId, effectId, System.StringComparison.Ordinal))
                        continue;
                    if (modifier.FieldType == EffectModifierFieldType.Value)
                        value = global::UpgradeModifierValueEvaluator.Apply(
                            value,
                            modifier.OperationType,
                            modifier.Value);
                    else if (modifier.FieldType == EffectModifierFieldType.Duration)
                        duration = global::UpgradeModifierValueEvaluator.Apply(
                            duration,
                            modifier.OperationType,
                            modifier.Value);
                    else return null;
                }
            }

            EffectRuntimeData runtime = new StatModifierEffectRuntime(
                source.EffectSO, config, target, value);
            return new EffectEntryRuntime(
                runtime,
                resolved.LifetimeType,
                resolved.CategoryType,
                Mathf.Max(0f, duration),
                resolved.MaxApplyCount);
        }

        public EffectEntryRuntime Resolve(
            EffectEntrySO effectEntrySo,
            CharacterManager targetCharacter,
            CharacterManager sourceCharacter = null,
            Transform sourceTransform = null,
            Vector2 projectileDirection = default)
        {
            if (effectEntrySo == null)
            {
                return null;
            }

            EffectRuntimeData runtimeData = ResolveRuntimeData(
                effectEntrySo.EffectSO,
                effectEntrySo,
                targetCharacter,
                sourceCharacter,
                sourceTransform,
                projectileDirection);

            if (runtimeData == null)
            {
                return null;
            }

            return new EffectEntryRuntime(
                runtimeData,
                effectEntrySo.LifetimeType,
                effectEntrySo.CategoryType,
                effectEntrySo.Duration,
                effectEntrySo.MaxApplyCount);
        }

        public EffectRuntimeData ResolveRuntimeData(
            EffectSO effectSo,
            EffectEntrySO effectEntrySo,
            CharacterManager targetCharacter,
            CharacterManager sourceCharacter = null,
            Transform sourceTransform = null,
            Vector2 projectileDirection = default)
        {
            if (effectSo == null)
            {
                return null;
            }

            switch (effectSo.Config)
            {
                case StatModifierEffectConfig statModifierConfig:
                    if (targetCharacter == null)
                    {
                        return null;
                    }

                    return new StatModifierEffectRuntime(
                        effectSo,
                        statModifierConfig,
                        targetCharacter);

                case HealEffectConfig healConfig:
                    return new HealEffectRuntime(
                        effectSo,
                        healConfig,
                        targetCharacter,
                        sourceCharacter);

                case KnockbackEffectConfig knockbackConfig:
                    if (targetCharacter == null)
                    {
                        return null;
                    }

                    return new KnockbackEffectRuntime(
                        effectSo,
                        knockbackConfig,
                        targetCharacter,
                        sourceTransform,
                        projectileDirection);

                case CooldownReduceEffectConfig cooldownReduceConfig:
                    if (targetCharacter == null)
                    {
                        return null;
                    }

                    return new CooldownReduceEffectRuntime(
                        effectSo,
                        cooldownReduceConfig,
                        targetCharacter);

                case ChanceOnHitSkillEffectConfig chanceOnHitSkillConfig:
                    if (targetCharacter == null || sourceCharacter == null)
                    {
                        return null;
                    }

                    return new ChanceOnHitSkillEffectRuntime(
                        effectSo,
                        chanceOnHitSkillConfig,
                        targetCharacter,
                        sourceCharacter);

                case ChanceOnHitStatModifierEffectConfig chanceOnHitStatModifierConfig:
                    if (targetCharacter == null)
                    {
                        return null;
                    }

                    return new ChanceOnHitStatModifierEffectRuntime(
                        effectSo,
                        chanceOnHitStatModifierConfig,
                        targetCharacter,
                        sourceCharacter);

                case ChanceOnHealStatModifierEffectConfig chanceOnHealStatModifierConfig:
                    if (targetCharacter == null)
                    {
                        return null;
                    }

                    return new ChanceOnHealStatModifierEffectRuntime(
                        effectSo,
                        chanceOnHealStatModifierConfig,
                        targetCharacter);

                case ChanceOnHealCooldownReduceEffectConfig chanceOnHealCooldownReduceConfig:
                    if (targetCharacter == null)
                    {
                        return null;
                    }

                    return new ChanceOnHealCooldownReduceEffectRuntime(
                        effectSo,
                        chanceOnHealCooldownReduceConfig,
                        targetCharacter);

                case AttackBleedEffectConfig attackBleedConfig:
                    if (targetCharacter == null || sourceCharacter == null)
                    {
                        return null;
                    }

                    return new AttackBleedEffectRuntime(
                        effectSo,
                        attackBleedConfig,
                        targetCharacter,
                        sourceCharacter);

                case TauntEffectConfig tauntConfig:
                    if (effectEntrySo == null)
                    {
                        return null;
                    }

                    return new TauntEffectRuntime(
                        effectSo,
                        tauntConfig,
                        targetCharacter,
                        sourceTransform,
                        effectEntrySo.Duration);

                case OnHitKnockbackDistanceEffectConfig onHitKnockbackDistanceConfig:
                    if (sourceCharacter == null)
                    {
                        return null;
                    }

                    return new OnHitKnockbackDistanceEffectRuntime(
                        effectSo,
                        onHitKnockbackDistanceConfig,
                        sourceCharacter);

                case OnHitTimedStatModifierEffectConfig onHitTimedStatModifierConfig:
                    if (sourceCharacter == null)
                    {
                        return null;
                    }

                    return new OnHitTimedStatModifierEffectRuntime(
                        effectSo,
                        onHitTimedStatModifierConfig,
                        sourceCharacter);

                case OnHitPoisonDotEffectConfig onHitPoisonDotConfig:
                    if (sourceCharacter == null)
                    {
                        return null;
                    }

                    return new OnHitPoisonDotEffectRuntime(
                        effectSo,
                        onHitPoisonDotConfig,
                        sourceCharacter);

                default:
                    return null;
            }
        }
    }
}
