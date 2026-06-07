using Character;
using UnityEngine;

namespace Effect
{
    public class HealEffectRuntime
        : EffectRuntimeData
    {
        private readonly HealEffectSO effectSO;
        private readonly CharacterManager targetCharacter;
        private readonly CharacterManager sourceCharacter;

        public HealEffectRuntime(
            HealEffectSO effectSO,
            CharacterManager targetCharacter = null,
            CharacterManager sourceCharacter = null)
        {
            this.effectSO = effectSO;
            this.targetCharacter = targetCharacter;
            this.sourceCharacter = sourceCharacter;

            RuntimeId =
                $"Heal_{effectSO.effectId}_{GetTargetRuntimeId()}";
        }

        public override void OnApply()
        {
            if (effectSO == null || targetCharacter == null)
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
            float result = effectSO.flatHealAmount;

            if (effectSO.useMaxHpPercent)
            {
                float maxHp =
                    targetCharacter.GetStatValue(
                        Stat.StatType.MaxHp);

                result += maxHp * effectSO.maxHpPercent;
            }

            if (effectSO.useAttackScaling &&
                sourceCharacter != null)
            {
                float attack =
                    sourceCharacter.GetStatValue(
                        Stat.StatType.Attack);

                result += attack * effectSO.attackPercentHeal;
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