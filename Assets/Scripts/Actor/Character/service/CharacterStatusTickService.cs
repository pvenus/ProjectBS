using Character;
using Stat;
using UnityEngine;

namespace Character.Service
{
    /// <summary>
    /// CharacterManager.Update()에서 처리하던 주기성 상태 갱신 로직을 담당한다.
    /// - StunDuration 감소
    /// - RootDuration 감소
    /// - HpRegen / HpRegenMaxHpPercent 회복
    /// - BleedDamage tick
    /// - Missing HP 기반 파생 스탯 갱신
    /// </summary>
    public class CharacterStatusTickService
    {
        private float hpRegenTimer;
        private float bleedTimer;
        private float appliedEliteApproachMoveSpeedBonus;
        private float appliedLowHpAttackBonus;
        private float appliedLowHpDefenseBonus;
        private float appliedSurroundedAttackPercent;
        private float appliedSurroundedDamageReductionPercent;

        public void Reset()
        {
            hpRegenTimer = 0f;
            bleedTimer = 0f;
            appliedEliteApproachMoveSpeedBonus = 0f;
            appliedLowHpAttackBonus = 0f;
            appliedLowHpDefenseBonus = 0f;
            appliedSurroundedAttackPercent = 0f;
            appliedSurroundedDamageReductionPercent = 0f;
        }

        public void Tick(
            CharacterManager characterManager,
            CharacterRuntimeData runtimeData,
            CharacterStatService statService,
            CharacterDamageService damageService,
            float deltaTime,
            float hpRegenTickInterval,
            float bleedTickInterval)
        {
            if (characterManager == null
                || runtimeData == null
                || runtimeData.isDead)
            {
                return;
            }

            UpdateDurationStat(
                characterManager,
                StatType.StunDuration,
                deltaTime);

            UpdateDurationStat(
                characterManager,
                StatType.RootDuration,
                deltaTime);

            UpdateHpRegen(
                characterManager,
                damageService,
                deltaTime,
                hpRegenTickInterval);

            UpdateBleedDamage(
                characterManager,
                damageService,
                deltaTime,
                bleedTickInterval);

            UpdateMissingHpDerivedDamageStats(
                runtimeData,
                statService);

            UpdateEliteApproachMoveSpeed(
                characterManager);
            UpdateLowHpAttackBonus(
                characterManager);
            UpdateLowHpDefenseBonus(
                characterManager);
            UpdateSurroundedBonuses(
                characterManager);
        }

        private void UpdateDurationStat(
            CharacterManager characterManager,
            StatType statType,
            float deltaTime)
        {
            if (characterManager == null || deltaTime <= 0f)
            {
                return;
            }

            float duration =
                characterManager.GetStatValue(statType);

            if (duration <= 0f)
            {
                return;
            }

            duration -= deltaTime;

            if (duration < 0f)
            {
                duration = 0f;
            }

            characterManager.SetStat(
                statType,
                duration);
        }

        private void UpdateHpRegen(
            CharacterManager characterManager,
            CharacterDamageService damageService,
            float deltaTime,
            float tickInterval)
        {
            if (characterManager == null
                || damageService == null
                || deltaTime <= 0f)
            {
                return;
            }

            hpRegenTimer -= deltaTime;

            float interval =
                Mathf.Max(0.05f, tickInterval);

            if (hpRegenTimer > 0f)
            {
                return;
            }

            hpRegenTimer = interval;

            float flatHpRegenPerSecond =
                characterManager.GetStatValue(StatType.HpRegen);

            float maxHpRegenPercentPerSecond =
                characterManager.GetStatValue(StatType.HpRegenMaxHpPercent);

            if (flatHpRegenPerSecond <= 0f &&
                maxHpRegenPercentPerSecond <= 0f)
            {
                return;
            }

            float maxHp =
                characterManager.GetStatValue(StatType.MaxHp);

            float percentHpRegenPerSecond =
                maxHp > 0f
                    ? maxHp * maxHpRegenPercentPerSecond * 0.01f
                    : 0f;

            float healAmount =
                (flatHpRegenPerSecond + percentHpRegenPerSecond) * interval;

            if (healAmount <= 0f)
            {
                return;
            }

            damageService.Heal(
                characterManager,
                healAmount);
        }

        private void UpdateBleedDamage(
            CharacterManager characterManager,
            CharacterDamageService damageService,
            float deltaTime,
            float tickInterval)
        {
            if (characterManager == null
                || damageService == null
                || deltaTime <= 0f)
            {
                return;
            }

            bleedTimer -= deltaTime;

            float interval =
                Mathf.Max(0.05f, tickInterval);

            if (bleedTimer > 0f)
            {
                return;
            }

            bleedTimer = interval;

            damageService.ApplyBleedDamagePerSecond(
                characterManager,
                interval);
        }

        private void UpdateMissingHpDerivedDamageStats(
            CharacterRuntimeData runtimeData,
            CharacterStatService statService)
        {
            if (runtimeData == null
                || runtimeData.isDead
                || statService == null)
            {
                return;
            }

            statService.RefreshMissingHpDerivedDamageStats();
        }

        private void UpdateEliteApproachMoveSpeed(
            CharacterManager characterManager)
        {
            if (characterManager == null)
            {
                return;
            }

            float bonusPercent =
                characterManager.GetStatValue(
                    StatType.EliteApproachMoveSpeedPercent);

            float radius =
                characterManager.GetStatValue(
                    StatType.EliteApproachRadius);

            if (radius <= 0f)
            {
                radius = 3f;
            }

            float currentBonus =
                appliedEliteApproachMoveSpeedBonus;

            bool hasEliteNearby = IsEliteNearby(
                characterManager,
                radius);

            float targetBonus =
                hasEliteNearby
                    ? bonusPercent
                    : 0f;

            float diff = targetBonus - currentBonus;

            if (Mathf.Abs(diff) <= 0.001f)
            {
                return;
            }

            characterManager.AddStat(
                StatType.MoveSpeedPercent,
                diff);

            appliedEliteApproachMoveSpeedBonus =
                targetBonus;
        }

        private void UpdateLowHpAttackBonus(
            CharacterManager characterManager)
        {
            if (characterManager == null)
            {
                return;
            }

            float bonusPercent =
                characterManager.GetStatValue(
                    StatType.LowHpAttackBonus);

            float currentHp =
                characterManager.GetStatValue(
                    StatType.Hp);

            float maxHp =
                characterManager.GetStatValue(
                    StatType.MaxHp);

            if (maxHp <= 0f)
            {
                return;
            }

            bool isLowHp =
                currentHp <= maxHp * 0.5f;

            float targetBonus =
                isLowHp
                    ? bonusPercent
                    : 0f;

            float diff =
                targetBonus - appliedLowHpAttackBonus;

            if (Mathf.Abs(diff) <= 0.001f)
            {
                return;
            }

            characterManager.AddStat(
                StatType.AttackPercent,
                diff);

            appliedLowHpAttackBonus =
                targetBonus;
        }

        private void UpdateLowHpDefenseBonus(
            CharacterManager characterManager)
        {
            if (characterManager == null)
            {
                return;
            }

            float bonusPercent =
                characterManager.GetStatValue(
                    StatType.LowHpDefenseBonus);

            float currentHp =
                characterManager.GetStatValue(
                    StatType.Hp);

            float maxHp =
                characterManager.GetStatValue(
                    StatType.MaxHp);

            if (maxHp <= 0f)
            {
                return;
            }

            bool isLowHp =
                currentHp <= maxHp * 0.3f;

            float targetBonus =
                isLowHp
                    ? bonusPercent
                    : 0f;

            float diff =
                targetBonus - appliedLowHpDefenseBonus;

            if (Mathf.Abs(diff) <= 0.001f)
            {
                return;
            }

            characterManager.AddStat(
                StatType.Defense,
                diff);

            appliedLowHpDefenseBonus =
                targetBonus;
        }

        private void UpdateSurroundedBonuses(
            CharacterManager characterManager)
        {
            if (characterManager == null)
            {
                return;
            }

            bool isSurrounded = IsSurroundedByEnemies(
                characterManager,
                2f,
                10);

            UpdateSurroundedAttackPercent(
                characterManager,
                isSurrounded);

            UpdateSurroundedDamageReductionPercent(
                characterManager,
                isSurrounded);
        }

        private void UpdateSurroundedAttackPercent(
            CharacterManager characterManager,
            bool isSurrounded)
        {
            float bonusPercent =
                characterManager.GetStatValue(
                    StatType.SurroundedAttackPercent);

            float targetBonus =
                isSurrounded
                    ? bonusPercent
                    : 0f;

            float diff =
                targetBonus - appliedSurroundedAttackPercent;

            if (Mathf.Abs(diff) <= 0.001f)
            {
                return;
            }

            characterManager.AddStat(
                StatType.AttackPercent,
                diff);

            appliedSurroundedAttackPercent =
                targetBonus;
        }

        private void UpdateSurroundedDamageReductionPercent(
            CharacterManager characterManager,
            bool isSurrounded)
        {
            float bonusPercent =
                characterManager.GetStatValue(
                    StatType.SurroundedDamageReductionPercent);

            float targetBonus =
                isSurrounded
                    ? bonusPercent
                    : 0f;

            float diff =
                targetBonus - appliedSurroundedDamageReductionPercent;

            if (Mathf.Abs(diff) <= 0.001f)
            {
                return;
            }

            characterManager.AddStat(
                StatType.Defense,
                diff);

            appliedSurroundedDamageReductionPercent =
                targetBonus;
        }

        private bool IsSurroundedByEnemies(
            CharacterManager source,
            float radius,
            int requiredEnemyCount)
        {
            if (source == null ||
                source.RuntimeData == null ||
                source.RuntimeData.characterSO == null)
            {
                return false;
            }

            CharacterManager[] characters =
                Object.FindObjectsByType<CharacterManager>(
                    FindObjectsSortMode.None);

            Vector3 position = source.transform.position;
            CharacterType sourceType =
                source.RuntimeData.characterSO.CharacterType;

            int enemyCount = 0;

            for (int i = 0; i < characters.Length; i++)
            {
                CharacterManager target = characters[i];

                if (target == null || target == source)
                {
                    continue;
                }

                if (target.RuntimeData == null ||
                    target.RuntimeData.isDead ||
                    target.RuntimeData.characterSO == null)
                {
                    continue;
                }

                if (!IsEnemyCharacterType(
                        sourceType,
                        target.RuntimeData.characterSO.CharacterType))
                {
                    continue;
                }

                float distance = Vector3.Distance(
                    position,
                    target.transform.position);

                if (distance > radius)
                {
                    continue;
                }

                enemyCount++;

                if (enemyCount >= requiredEnemyCount)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsEnemyCharacterType(
            CharacterType sourceType,
            CharacterType targetType)
        {
            if (sourceType == CharacterType.Player)
            {
                return targetType != CharacterType.Player;
            }

            return targetType == CharacterType.Player;
        }

        private bool IsEliteNearby(
            CharacterManager source,
            float radius)
        {
            CharacterManager[] characters =
                Object.FindObjectsByType<CharacterManager>(
                    FindObjectsSortMode.None);

            Vector3 position = source.transform.position;

            for (int i = 0; i < characters.Length; i++)
            {
                CharacterManager target = characters[i];

                if (target == null || target == source)
                {
                    continue;
                }

                if (target.RuntimeData == null
                    || target.RuntimeData.characterSO == null)
                {
                    continue;
                }

                if (target.RuntimeData.characterSO.CharacterType
                    != CharacterType.Boss)
                {
                    continue;
                }

                float distance = Vector3.Distance(
                    position,
                    target.transform.position);

                if (distance <= radius)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
