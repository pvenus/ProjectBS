using Stat;
using UnityEngine;

namespace Character
{
    public class CharacterDamageService
    {
        public CharacterDamageService()
        {
        }

        public float Heal(
            CharacterManager targetManager,
            float healAmount)
        {
            if (targetManager == null
                || targetManager.RuntimeData == null)
            {
                return 0f;
            }

            if (targetManager.RuntimeData.isDead)
            {
                return 0f;
            }

            if (healAmount <= 0f)
            {
                return 0f;
            }

            float currentHp =
                targetManager.GetStatValue(StatType.Hp);

            float maxHp =
                targetManager.GetStatValue(StatType.MaxHp);

            if (maxHp <= 0f)
            {
                return 0f;
            }

            if (currentHp >= maxHp)
            {
                return 0f;
            }

            float nextHp =
                Mathf.Min(
                    maxHp,
                    currentHp + healAmount);

            float actualHealAmount =
                Mathf.Max(
                    0f,
                    nextHp - currentHp);

            if (actualHealAmount <= 0f)
            {
                return 0f;
            }

            targetManager.SetStat(
                StatType.Hp,
                nextHp);

            return actualHealAmount;
        }

        public float HealByMaxHpPercent(
            CharacterManager targetManager,
            float percent)
        {
            if (targetManager == null
                || targetManager.RuntimeData == null
                || percent <= 0f)
            {
                return 0f;
            }

            float maxHp =
                targetManager.GetStatValue(StatType.MaxHp);

            if (maxHp <= 0f)
            {
                return 0f;
            }

            return Heal(
                targetManager,
                maxHp * (percent / 100f));
        }

        public float ApplyBleedDamagePerSecond(
            CharacterManager targetManager,
            float deltaTime)
        {
            if (targetManager == null
                || targetManager.RuntimeData == null
                || targetManager.RuntimeData.isDead
                || deltaTime <= 0f)
            {
                return 0f;
            }

            float bleedDamagePerSecond =
                targetManager.GetStatValue(StatType.BleedDamagePerSecond);

            if (bleedDamagePerSecond <= 0f)
            {
                return 0f;
            }

            float damage =
                bleedDamagePerSecond * deltaTime;

            if (damage <= 0f)
            {
                return 0f;
            }

            float currentHp =
                targetManager.GetStatValue(StatType.Hp);

            if (currentHp <= 0f)
            {
                return 0f;
            }

            float nextHp =
                Mathf.Max(
                    0f,
                    currentHp - damage);

            float actualDamage =
                Mathf.Max(
                    0f,
                    currentHp - nextHp);

            if (actualDamage <= 0f)
            {
                return 0f;
            }

            targetManager.SetStat(
                StatType.Hp,
                nextHp);

            if (nextHp <= 0f)
            {
                targetManager.RuntimeData.isDead = true;
                targetManager.HandleDeath();
            }

            return actualDamage;
        }

        public float TakeDamage(
            CharacterManager targetManager,
            float damage,
            bool isCritical)
        {
            return TakeDamage(
                targetManager,
                damage,
                isCritical,
                null,
                true);
        }

        private float TakeDamage(
            CharacterManager targetManager,
            float damage,
            bool isCritical,
            CharacterManager attackerManager,
            bool allowReflect)
        {
            if (targetManager == null
                || targetManager.RuntimeData == null)
            {
                return 0f;
            }

            if (targetManager.RuntimeData.isDead)
            {
                return 0f;
            }

            if (damage <= 0f)
            {
                return 0f;
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
            float actualDamage = 0f;

            float currentShield =
                targetManager.GetStatValue(StatType.Shield);

            if (currentShield > 0f)
            {
                float shieldDamage =
                    Mathf.Min(
                        currentShield,
                        remainingDamage);

                float nextShield =
                    Mathf.Max(0f, currentShield - remainingDamage);

                remainingDamage =
                    Mathf.Max(0f, remainingDamage - currentShield);

                actualDamage += shieldDamage;

                targetManager.SetStat(
                    StatType.Shield,
                    nextShield);
            }

            if (remainingDamage > 0f)
            {
                float currentHp =
                    targetManager.GetStatValue(StatType.Hp);

                float hpDamage =
                    Mathf.Min(
                        currentHp,
                        remainingDamage);

                currentHp -= remainingDamage;

                if (currentHp <= 0f)
                {
                    currentHp = 0f;
                    targetManager.RuntimeData.isDead = true;
                }

                actualDamage += hpDamage;

                targetManager.SetStat(
                    StatType.Hp,
                    currentHp);
            }

            if (allowReflect)
            {
                ApplyReflectDamage(
                    targetManager,
                    attackerManager,
                    actualDamage);
            }

            if (targetManager.RuntimeData.isDead)
            {
                targetManager.RegisterLastHitAttacker(attackerManager);
                targetManager.HandleDeath();
            }

            return actualDamage;
        }

        public static CharacterDamageResult ApplyWithoutOwner(
            CharacterDamageRequest request)
        {
            CharacterDamageResult result = new();

            if (request == null
                || request.target == null)
            {
                return result;
            }

            CharacterManager targetManager =
                ResolveCharacterManagerStatic(request.target);

            if (targetManager == null)
            {
                return result;
            }

            float finalDamage =
                Mathf.Max(0f, request.baseDamage);

            result.damage = finalDamage;
            result.isCritical = false;

            if (finalDamage > 0f)
            {
                new CharacterDamageService().TakeDamage(
                    targetManager,
                    finalDamage,
                    false);
            }

            result.targetDied =
                targetManager.RuntimeData != null
                && targetManager.RuntimeData.isDead;

            return result;
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

            finalDamage =
                ApplyFinalDamageAmplify(
                    finalDamage,
                    attackerManager);

            result.damage = finalDamage;
            result.isCritical = isCritical;

            if (finalDamage > 0f)
            {
                TakeDamage(
                    targetManager,
                    finalDamage,
                    isCritical,
                    attackerManager,
                    true);

                ApplyLifeSteal(
                    attackerManager,
                    finalDamage);
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

            return request.baseDamage
                   + attackerAttack * request.attackDamagePercent/100f;
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

        private float ApplyFinalDamageAmplify(
            float damage,
            CharacterManager attackerManager)
        {
            if (damage <= 0f || attackerManager == null)
            {
                return damage;
            }

            float finalDamageAmplify =
                attackerManager.GetStatValue(StatType.FinalDamageAmplify);

            if (finalDamageAmplify <= 0f)
            {
                return damage;
            }

            return damage
                   + damage * (finalDamageAmplify / 100f);
        }

        private void ApplyReflectDamage(
            CharacterManager defenderManager,
            CharacterManager attackerManager,
            float receivedDamage)
        {
            if (defenderManager == null
                || defenderManager.RuntimeData == null
                || attackerManager == null
                || attackerManager.RuntimeData == null
                || attackerManager.RuntimeData.isDead
                || receivedDamage <= 0f)
            {
                return;
            }

            if (defenderManager == attackerManager)
            {
                return;
            }

            float reflectPercent =
                defenderManager.GetStatValue(StatType.ReflectDamagePercent);

            if (reflectPercent <= 0f)
            {
                return;
            }

            float reflectDamage =
                receivedDamage * (reflectPercent / 100f);

            if (reflectDamage <= 0f)
            {
                return;
            }

            TakeDamage(
                attackerManager,
                reflectDamage,
                false,
                defenderManager,
                false);
        }

        private void ApplyLifeSteal(
            CharacterManager attackerManager,
            float damage)
        {
            if (attackerManager == null
                || attackerManager.RuntimeData == null
                || attackerManager.RuntimeData.isDead
                || damage <= 0f)
            {
                return;
            }

            float lifeStealPercent =
                attackerManager.GetStatValue(StatType.LifeSteal);

            lifeStealPercent +=
                attackerManager.GetStatValue(StatType.LifeStealPercent);

            if (lifeStealPercent <= 0f)
            {
                return;
            }

            float healAmount =
                damage * (lifeStealPercent / 100f);

            if (healAmount <= 0f)
            {
                return;
            }

            Heal(
                attackerManager,
                healAmount);
        }

        private CharacterManager ResolveAttackerManager(CharacterDamageRequest request)
        {
            if (request == null || request.attacker == null)
            {
                return null;
            }

            return ResolveCharacterManager(request.attacker);
        }

        private static CharacterManager ResolveCharacterManagerStatic(GameObject target)
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