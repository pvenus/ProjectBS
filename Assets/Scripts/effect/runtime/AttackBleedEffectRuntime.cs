

using Character;
using Stat;
using UnityEngine;

namespace Effect
{
    public class AttackBleedEffectRuntime : EffectRuntimeData
    {
        private readonly AttackBleedEffectSO effectSO;

        private readonly CharacterManager targetCharacter;

        private readonly CharacterManager sourceCharacter;

        private float appliedBleedDamagePerSecond;

        public AttackBleedEffectRuntime(
            AttackBleedEffectSO effectSO,
            CharacterManager targetCharacter,
            CharacterManager sourceCharacter)
        {
            this.effectSO = effectSO;
            this.targetCharacter = targetCharacter;
            this.sourceCharacter = sourceCharacter;

            RuntimeId =
                $"AttackBleed_{effectSO.effectId}_{GetTargetRuntimeId()}";
        }

        private string GetTargetRuntimeId()
        {
            if (targetCharacter == null)
            {
                return "None";
            }

            return targetCharacter.GetInstanceID().ToString();
        }

        public override void OnApply()
        {
            if (effectSO == null
                || targetCharacter == null
                || sourceCharacter == null)
            {
                return;
            }

            if (effectSO.chancePercent <= 0f)
            {
                return;
            }

            float randomValue =
                Random.Range(0f, 100f);

            if (randomValue > effectSO.chancePercent)
            {
                return;
            }

            float attack =
                sourceCharacter.GetStatValue(StatType.Attack);

            appliedBleedDamagePerSecond =
                attack * (effectSO.attackRatioPercent / 100f);

            if (appliedBleedDamagePerSecond <= 0f)
            {
                return;
            }

            targetCharacter.AddStat(
                StatType.BleedDamagePerSecond,
                appliedBleedDamagePerSecond);
        }

        public override void OnRemove()
        {
            if (targetCharacter == null || appliedBleedDamagePerSecond <= 0f)
            {
                return;
            }

            targetCharacter.AddStat(
                StatType.BleedDamagePerSecond,
                -appliedBleedDamagePerSecond);

            appliedBleedDamagePerSecond = 0f;
        }
    }
}