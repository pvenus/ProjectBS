using Character;
using Stat;
using UnityEngine;

namespace Effect
{
    public class AttackBleedEffectRuntime : EffectRuntimeData
    {
        private readonly EffectSO effectSO;
        private readonly AttackBleedEffectConfig config;

        private readonly CharacterManager targetCharacter;

        private readonly CharacterManager sourceCharacter;

        private float appliedBleedDamagePerSecond;

        private bool isApplied;

        public AttackBleedEffectRuntime(
            EffectSO effectSO,
            AttackBleedEffectConfig config,
            CharacterManager targetCharacter,
            CharacterManager sourceCharacter)
        {
            this.effectSO = effectSO;
            this.config = config;
            this.targetCharacter = targetCharacter;
            this.sourceCharacter = sourceCharacter;

            RuntimeId =
                $"AttackBleed_{effectSO.EffectId}_{GetTargetRuntimeId()}";
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
                || config == null
                || targetCharacter == null
                || sourceCharacter == null)
            {
                IsActive = false;
                return;
            }

            IsActive = true;
        }

        public void OnHit(
            CharacterDamageRequest request,
            CharacterDamageResult result)
        {
            if (effectSO == null
                || config == null
                || targetCharacter == null
                || sourceCharacter == null
                || request == null
                || request.attacker == null
                || request.target == null)
            {
                return;
            }

            if (request.attacker.GetComponent<CharacterManager>() != sourceCharacter
                || request.target.GetComponent<CharacterManager>() != targetCharacter)
            {
                return;
            }

            if (config.ChancePercent <= 0f)
            {
                return;
            }

            float randomValue =
                Random.Range(0f, 100f);

            if (randomValue > config.ChancePercent)
            {
                return;
            }

            float attack =
                sourceCharacter.GetStatValue(StatType.Attack);

            appliedBleedDamagePerSecond =
                attack * (config.AttackRatioPercent / 100f);

            if (appliedBleedDamagePerSecond <= 0f)
            {
                return;
            }

            if (isApplied)
            {
                targetCharacter.AddStat(
                    StatType.BleedDamagePerSecond,
                    -appliedBleedDamagePerSecond);
            }

            targetCharacter.AddStat(
                StatType.BleedDamagePerSecond,
                appliedBleedDamagePerSecond);

            isApplied = true;
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
            isApplied = false;
        }
    }
}
