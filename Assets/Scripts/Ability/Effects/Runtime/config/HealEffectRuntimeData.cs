using Character;
using UnityEngine;

namespace Effect
{
    public class HealEffectRuntime
        : EffectRuntimeData
    {
        private readonly EffectSO effectSO;
        private readonly HealEffectConfig config;
        private readonly CharacterManager targetCharacter;
        private readonly CharacterManager sourceCharacter;

        public HealEffectRuntime(
            EffectSO effectSO,
            HealEffectConfig config,
            CharacterManager targetCharacter = null,
            CharacterManager sourceCharacter = null)
        {
            this.effectSO = effectSO;
            this.config = config;
            this.targetCharacter = targetCharacter;
            this.sourceCharacter = sourceCharacter;

            RuntimeId =
                $"Heal_{effectSO.EffectId}_{GetTargetRuntimeId()}";
        }

        public override void OnApply()
        {
            if (effectSO == null || config == null || targetCharacter == null)
            {
                return;
            }

            float healAmount = CalculateHealAmount();

            if (healAmount <= 0f)
            {
                return;
            }

            targetCharacter.Heal(healAmount);
        }

        public override void OnRemove()
        {
            // Instant Heal Effect
        }

        private float CalculateHealAmount()
        {
            float result = config.FlatHealAmount;

            if (config.UseMaxHpPercent)
            {
                float maxHp =
                    targetCharacter.GetStatValue(
                        Stat.StatType.MaxHp);

                result += maxHp * config.MaxHpPercent;
            }

            if (config.UseAttackScaling &&
                sourceCharacter != null)
            {
                float attack =
                    sourceCharacter.GetStatValue(
                        Stat.StatType.Attack);

                result += attack * config.AttackPercentHeal;
            }

            return Mathf.Max(0f, result);
        }
        private string GetTargetRuntimeId()
        {
            return targetCharacter != null
                ? targetCharacter.GetInstanceID().ToString()
                : "NoTarget";
        }
    }
}