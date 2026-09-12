using System.Collections;
using Character;

namespace Effect
{
    public class StatModifierEffectRuntime
        : EffectRuntimeData
    {
        private readonly EffectSO effectSO;
        private readonly StatModifierEffectConfig config;
        private readonly CharacterManager targetCharacterManager;
        private readonly float? valueOverride;

        private float appliedValue;

        public StatModifierEffectRuntime(
            EffectSO effectSO,
            StatModifierEffectConfig config,
            CharacterManager targetCharacterManager,
            float? valueOverride = null)
        {
            this.effectSO = effectSO;
            this.config = config;
            this.targetCharacterManager = targetCharacterManager;
            this.valueOverride = valueOverride;

            RuntimeId =
                $"StatModifier_{effectSO.EffectId}_{GetTargetRuntimeId()}";
        }

        public override void OnApply()
        {
            if (effectSO == null || config == null || targetCharacterManager == null)
            {
                return;
            }

            if (config.TargetStat == Stat.StatType.RootDuration)
            {
                float requested = UnityEngine.Mathf.Clamp(
                    valueOverride ?? config.Value,
                    0f,
                    config.RootHardCap);
                requested = Skill.Mouse3CrowdControlPolicy.ResolveDuration(
                    targetCharacterManager, requested);
                targetCharacterManager.StartCoroutine(ApplyRootAfterDisplacement(requested));
                appliedValue = 0f;
                return;
            }

            appliedValue = CalculateModifierValue();

            targetCharacterManager.AddStat(
                config.TargetStat,
                appliedValue);
        }

        public override void OnRemove()
        {
            if (effectSO == null || config == null || targetCharacterManager == null
                || config.TargetStat == Stat.StatType.RootDuration)
            {
                return;
            }

            targetCharacterManager.AddStat(
                config.TargetStat,
                -appliedValue);
        }

        private float CalculateModifierValue()
        {
            float currentValue =
                targetCharacterManager.GetStatValue(
                    config.TargetStat);
            float resolvedValue = valueOverride ?? config.Value;
            return config.ModifierType switch
            {
                StatModifierType.Flat
                    => resolvedValue,

                StatModifierType.Percent
                    => currentValue * resolvedValue,

                StatModifierType.Multiply
                    => currentValue * (resolvedValue - 1f),

                _ => resolvedValue
            };
        }

        private IEnumerator ApplyRootAfterDisplacement(float requested)
        {
            MovementMono movement = targetCharacterManager != null
                ? targetCharacterManager.GetComponent<MovementMono>()
                  ?? targetCharacterManager.GetComponentInParent<MovementMono>()
                  ?? targetCharacterManager.GetComponentInChildren<MovementMono>()
                : null;

            while (targetCharacterManager != null
                   && targetCharacterManager.isActiveAndEnabled
                   && movement != null
                   && movement.IsKnockingBack())
            {
                yield return null;
            }

            if (targetCharacterManager == null || !targetCharacterManager.isActiveAndEnabled)
            {
                yield break;
            }

            float current = targetCharacterManager.GetStatValue(config.TargetStat);
            targetCharacterManager.SetStat(
                config.TargetStat,
                UnityEngine.Mathf.Max(current, requested));
        }

        private string GetTargetRuntimeId()
        {
            return targetCharacterManager != null
                ? targetCharacterManager.GetInstanceID().ToString()
                : "NoTarget";
        }
    }
}
