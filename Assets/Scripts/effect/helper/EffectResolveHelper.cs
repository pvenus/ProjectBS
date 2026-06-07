using Character;
using UnityEngine;

namespace Effect
{
    public static class EffectResolveHelper
    {
        public static EffectRuntimeData CreateRuntimeData(
            EffectSO effectSo,
            EffectSourceType sourceType,
            string sourceId,
            CharacterManager targetCharacter,
            Transform sourceTransform = null,
            Vector2 projectileDirection = default,
            CharacterManager sourceCharacter = null)
        {
            if (effectSo == null)
            {
                return null;
            }

            if (effectSo is StatModifierEffectSO statModifierEffect)
            {
                if (targetCharacter == null)
                {
                    return null;
                }

                return new StatModifierEffectRuntime(
                    statModifierEffect,
                    sourceType,
                    sourceId,
                    targetCharacter);
            }

            if (effectSo is HealEffectSO healEffect)
            {
                if (targetCharacter == null)
                {
                    return null;
                }

                return healEffect.CreateRuntimeData(
                    targetCharacter);
            }

            if (effectSo is CooldownReduceEffectSO cooldownReduceEffect)
            {
                if (targetCharacter == null)
                {
                    return null;
                }

                return new CooldownReduceEffectRuntime(
                    cooldownReduceEffect,
                    targetCharacter);
            }

            if (effectSo is ChanceOnHitStatModifierEffectSO chanceOnHitStatModifierEffect)
            {
                if (targetCharacter == null)
                {
                    return null;
                }

                return chanceOnHitStatModifierEffect.CreateRuntimeData(
                    targetCharacter);
            }

            if (effectSo is ChanceOnHealStatModifierEffectSO chanceOnHealStatModifierEffect)
            {
                if (targetCharacter == null)
                {
                    return null;
                }

                return new ChanceOnHealStatModifierEffectRuntime(
                    chanceOnHealStatModifierEffect,
                    targetCharacter);
            }

            if (effectSo is ChanceOnHealCooldownReduceEffectSO chanceOnHealCooldownReduceEffect)
            {
                if (targetCharacter == null)
                {
                    return null;
                }

                return chanceOnHealCooldownReduceEffect.CreateRuntimeData(
                    targetCharacter);
            }

            if (effectSo is ChanceOnHitSkillEffectSO chanceOnHitSkillEffect)
            {
                if (targetCharacter == null || sourceCharacter == null)
                {
                    return null;
                }

                return chanceOnHitSkillEffect.CreateRuntimeData(
                    targetCharacter,
                    sourceCharacter);
            }

            if (effectSo is AttackBleedEffectSO attackBleedEffect)
            {
                if (targetCharacter == null || sourceCharacter == null)
                {
                    return null;
                }

                return attackBleedEffect.CreateRuntimeData(
                    targetCharacter,
                    sourceCharacter);
            }

            if (effectSo is KnockbackEffectSO knockbackEffect)
            {
                if (targetCharacter == null)
                {
                    return null;
                }

                return new KnockbackEffectRuntime(
                    knockbackEffect,
                    targetCharacter,
                    sourceTransform,
                    projectileDirection);
            }

            if (effectSo is TauntEffectSO tauntEffect)
            {
                if (targetCharacter == null || sourceTransform == null)
                {
                    return null;
                }

                return new TauntEffectRuntime(
                    tauntEffect,
                    targetCharacter,
                    sourceTransform);
            }

            return null;
        }
    }
}