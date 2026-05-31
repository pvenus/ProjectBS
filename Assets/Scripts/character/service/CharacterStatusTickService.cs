using Character;
using Stat;
using UnityEngine;

namespace Character.Service
{
    /// <summary>
    /// CharacterManager.Update()에서 처리하던 주기성 상태 갱신 로직을 담당한다.
    /// - StunDuration 감소
    /// - RootDuration 감소
    /// - HpRegen 회복
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

        public void Reset()
        {
            hpRegenTimer = 0f;
            bleedTimer = 0f;
            appliedEliteApproachMoveSpeedBonus = 0f;
            appliedLowHpAttackBonus = 0f;
            appliedLowHpDefenseBonus = 0f;
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

            float hpRegenPerSecond =
                characterManager.GetStatValue(StatType.HpRegen);

            if (hpRegenPerSecond <= 0f)
            {
                return;
            }

            float healAmount =
                hpRegenPerSecond * interval;

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

            float currentBonus =
                appliedEliteApproachMoveSpeedBonus;

            bool hasEliteNearby = IsEliteNearby(
                characterManager,
                3f);

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

                if (target.RuntimeData.characterSO.characterType
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