

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
            Vector2 projectileDirection = default)
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

                return healEffect.CreateRuntimeData(targetCharacter);
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

            return null;
        }
    }
}