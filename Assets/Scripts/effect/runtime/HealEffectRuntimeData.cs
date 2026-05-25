using Character;
using UnityEngine;

namespace Effect
{
    public class HealEffectRuntime
        : EffectRuntimeData
    {
        private readonly HealEffectSO effectSO;
        private readonly CharacterManager targetCharacter;

        public HealEffectRuntime(
            HealEffectSO effectSO,
            CharacterManager targetCharacter = null)
        {
            this.effectSO = effectSO;
            this.targetCharacter = targetCharacter;

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

            targetCharacter.AddStat(
                Stat.StatType.Hp,
                healAmount);

            if (effectSO.clampToMaxHp)
            {
                ClampHpToMaxHp();
            }
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

            return Mathf.Max(0f, result);
        }

        private void ClampHpToMaxHp()
        {
            float hp =
                targetCharacter.GetStatValue(
                    Stat.StatType.Hp);

            float maxHp =
                targetCharacter.GetStatValue(
                    Stat.StatType.MaxHp);

            if (hp <= maxHp)
            {
                return;
            }

            targetCharacter.SetStat(
                Stat.StatType.Hp,
                maxHp);
        }

        private string GetTargetRuntimeId()
        {
            return targetCharacter != null
                ? targetCharacter.GetInstanceID().ToString()
                : "NoTarget";
        }
    }
}