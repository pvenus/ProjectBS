using Stat;
using UnityEngine;

namespace Character
{
    public class CharacterDamageService
    {
        public CharacterDamageService()
        {
        }

        public void TakeDamage(
            CharacterManager targetManager,
            float damage,
            bool isCritical)
        {
            if (targetManager == null
                || targetManager.RuntimeData == null)
            {
                return;
            }

            if (targetManager.RuntimeData.isDead)
            {
                return;
            }

            if (damage <= 0f)
            {
                return;
            }

            float defensePercent =
                targetManager.GetStatValue(StatType.Defense);

              if (defensePercent > 0f)
            {
                damage -= damage * (defensePercent / 100f);
            }

            damage = Mathf.Max(1f, damage);

            targetManager.PlayDamagePresentation(
                damage,
                isCritical);

            float remainingDamage = damage;

            float currentShield =
                targetManager.GetStatValue(StatType.Shield);

            if (currentShield > 0f)
            {
                float nextShield =
                    Mathf.Max(0f, currentShield - remainingDamage);

                remainingDamage =
                    Mathf.Max(0f, remainingDamage - currentShield);

                targetManager.SetStat(
                    StatType.Shield,
                    nextShield);
            }

            if (remainingDamage > 0f)
            {
                float currentHp =
                    targetManager.GetStatValue(StatType.Hp);

                currentHp -= remainingDamage;

                if (currentHp <= 0f)
                {
                    currentHp = 0f;
                    targetManager.RuntimeData.isDead = true;
                }

                targetManager.SetStat(
                    StatType.Hp,
                    currentHp);
            }

            if (targetManager.RuntimeData.isDead)
            {
                targetManager.HandleDeath();
            }
        }

        public CharacterDamageResult Apply(CharacterDamageRequest request)
        {
            CharacterDamageResult result = new();

            if (request == null
                || request.target == null)
            {
                return result;
            }

            CharacterManager targetManager =
                ResolveCharacterManager(request.target);

            if (targetManager == null)
            {
                return result;
            }

            CharacterManager attackerManager =
                ResolveAttackerManager(request);

            float baseDamage =
                CalculateBaseDamage(
                    request,
                    attackerManager);

            baseDamage =
                ApplyBossDamageBonus(
                    baseDamage,
                    attackerManager,
                    targetManager);

            bool isCritical =
                RollCritical(attackerManager);

            float finalDamage =
                ApplyCriticalDamage(
                    baseDamage,
                    attackerManager,
                    isCritical);

            result.damage = finalDamage;
            result.isCritical = isCritical;

            if (finalDamage > 0f)
            {
                TakeDamage(
                    targetManager,
                    finalDamage,
                    isCritical);
            }

            result.targetDied =
                targetManager.RuntimeData != null
                && targetManager.RuntimeData.isDead;

            return result;
        }

        private float CalculateBaseDamage(
            CharacterDamageRequest request,
            CharacterManager attackerManager)
        {
            float attackerAttack =
                attackerManager != null
                    ? attackerManager.GetStatValue(StatType.Attack)
                    : 0f;

            return attackerAttack
                   * (request.attackDamagePercent / 100f)
                   + request.flatBonusDamage;
        }

        private bool RollCritical(CharacterManager attackerManager)
        {
            if (attackerManager == null)
            {
                return false;
            }

            float critChance =
                attackerManager.GetStatValue(StatType.CritChance);

            if (critChance <= 0f)
            {
                return false;
            }

            return Random.value <= critChance / 100f;
        }

        private float ApplyCriticalDamage(
            float baseDamage,
            CharacterManager attackerManager,
            bool isCritical)
        {
            if (!isCritical || attackerManager == null)
            {
                return baseDamage;
            }

            float critDamage =
                attackerManager.GetStatValue(StatType.CritDamage);

            if (critDamage <= 0f)
            {
                return baseDamage;
            }

            return baseDamage
                   + baseDamage * (critDamage / 100f);
        }

        private CharacterManager ResolveAttackerManager(CharacterDamageRequest request)
        {
            if (request == null || request.attacker == null)
            {
                return null;
            }

            return ResolveCharacterManager(request.attacker);
        }

        private CharacterManager ResolveCharacterManager(GameObject target)
        {
            if (target == null)
            {
                return null;
            }

            CharacterManager characterManager =
                target.GetComponent<CharacterManager>();

            if (characterManager == null)
            {
                characterManager =
                    target.GetComponentInParent<CharacterManager>();
            }

            return characterManager;
        }

        private float ApplyBossDamageBonus(
            float baseDamage,
            CharacterManager attackerManager,
            CharacterManager targetManager)
        {
            if (baseDamage <= 0f
                || attackerManager == null
                || targetManager == null
                || targetManager.RuntimeData == null
                || targetManager.RuntimeData.characterSO == null)
            {
                return baseDamage;
            }

            if (targetManager.RuntimeData.characterSO.characterType != CharacterType.Boss)
            {
                return baseDamage;
            }

            float bossDamagePercent =
                attackerManager.GetStatValue(StatType.BossDamagePercent);

            if (bossDamagePercent <= 0f)
            {
                return baseDamage;
            }

            return baseDamage
                   + baseDamage * (bossDamagePercent / 100f);
        }
    }
}