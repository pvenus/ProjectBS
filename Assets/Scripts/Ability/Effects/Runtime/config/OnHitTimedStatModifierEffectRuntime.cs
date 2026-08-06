using Character;
using Stat;
using UnityEngine;

namespace Effect
{
    public class OnHitTimedStatModifierEffectRuntime : EffectRuntimeData
    {
        private readonly EffectSO effectSO;
        private readonly OnHitTimedStatModifierEffectConfig config;
        private readonly CharacterManager sourceCharacter;

        public OnHitTimedStatModifierEffectRuntime(
            EffectSO effectSO,
            OnHitTimedStatModifierEffectConfig config,
            CharacterManager sourceCharacter)
        {
            this.effectSO = effectSO;
            this.config = config;
            this.sourceCharacter = sourceCharacter;
            RuntimeId = $"OnHitTimedStatModifier_{effectSO.EffectId}_{GetSourceRuntimeId()}";
        }

        public override void OnApply()
        {
            IsActive = effectSO != null
                && config != null
                && sourceCharacter != null;
        }

        public void OnHit(
            CharacterDamageRequest request,
            CharacterDamageResult result)
        {
            if (!IsActive
                || request == null
                || request.attacker == null
                || request.target == null
                || request.attacker.GetComponent<CharacterManager>() != sourceCharacter)
            {
                return;
            }

            CharacterManager target =
                request.target.GetComponent<CharacterManager>()
                ?? request.target.GetComponentInParent<CharacterManager>();

            if (target == null || target.RuntimeData == null || target.RuntimeData.isDead)
            {
                return;
            }

            float chance = Mathf.Clamp(config.ChancePercent, 0f, 100f);
            if (chance <= 0f || Random.Range(0f, 100f) > chance)
            {
                return;
            }

            EffectManager targetEffectManager =
                target.GetComponent<EffectManager>()
                ?? target.GetComponentInChildren<EffectManager>()
                ?? target.GetComponentInParent<EffectManager>();

            if (targetEffectManager == null)
            {
                return;
            }

            TargetTimedStatModifierEffectRuntime targetRuntime =
                new TargetTimedStatModifierEffectRuntime(
                    effectSO,
                    config,
                    target,
                    sourceCharacter);

            targetEffectManager.AddEffect(
                targetRuntime,
                EffectLifetimeType.Timed,
                config.DurationSeconds,
                EffectCategoryType.Debuff);
        }

        private string GetSourceRuntimeId()
        {
            return sourceCharacter != null
                ? sourceCharacter.GetInstanceID().ToString()
                : "NoSource";
        }
    }

    public class TargetTimedStatModifierEffectRuntime : EffectRuntimeData
    {
        private readonly EffectSO effectSO;
        private readonly OnHitTimedStatModifierEffectConfig config;
        private readonly CharacterManager targetCharacter;
        private readonly CharacterManager sourceCharacter;

        private float appliedValue;

        public TargetTimedStatModifierEffectRuntime(
            EffectSO effectSO,
            OnHitTimedStatModifierEffectConfig config,
            CharacterManager targetCharacter,
            CharacterManager sourceCharacter)
        {
            this.effectSO = effectSO;
            this.config = config;
            this.targetCharacter = targetCharacter;
            this.sourceCharacter = sourceCharacter;
            RuntimeId =
                $"TargetTimedStatModifier_{effectSO.EffectId}_{GetSourceRuntimeId()}_{GetTargetRuntimeId()}";
        }

        public override void OnApply()
        {
            if (effectSO == null
                || config == null
                || targetCharacter == null)
            {
                IsActive = false;
                return;
            }

            if (IsDurationStat(config.StatType))
            {
                float current = targetCharacter.GetStatValue(config.StatType);
                targetCharacter.SetStat(
                    config.StatType,
                    Mathf.Max(current, Mathf.Max(0f, config.Value)));
                return;
            }

            appliedValue = CalculateModifierValue();
            if (Mathf.Approximately(appliedValue, 0f))
            {
                return;
            }

            targetCharacter.AddStat(
                config.StatType,
                appliedValue);
        }

        public override void OnRemove()
        {
            if (targetCharacter == null
                || IsDurationStat(config.StatType)
                || Mathf.Approximately(appliedValue, 0f))
            {
                return;
            }

            targetCharacter.AddStat(
                config.StatType,
                -appliedValue);

            appliedValue = 0f;
        }

        private float CalculateModifierValue()
        {
            float currentValue =
                targetCharacter.GetStatValue(config.StatType);

            return config.ModifierType switch
            {
                StatModifierType.Flat => config.Value,
                StatModifierType.Percent => currentValue * (config.Value / 100f),
                StatModifierType.Multiply => currentValue * (config.Value - 1f),
                _ => config.Value
            };
        }

        private static bool IsDurationStat(StatType statType)
        {
            return statType == StatType.StunDuration
                || statType == StatType.RootDuration;
        }

        private string GetSourceRuntimeId()
        {
            return sourceCharacter != null
                ? sourceCharacter.GetInstanceID().ToString()
                : "NoSource";
        }

        private string GetTargetRuntimeId()
        {
            return targetCharacter != null
                ? targetCharacter.GetInstanceID().ToString()
                : "NoTarget";
        }
    }
}
