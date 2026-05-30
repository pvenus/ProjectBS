using System;
using Stat;
using UnityEngine;

namespace Character
{
    public class CharacterStatService
    {
        public event Action<StatType, float> OnStatChanged;

        private readonly CharacterRuntimeData runtimeData;
        private float appliedMissingHpAttackBonus;

        public CharacterStatService(CharacterRuntimeData runtimeData)
        {
            this.runtimeData = runtimeData;
        }

        public float GetStat(StatType statType)
        {
            if (runtimeData == null)
            {
                return 0f;
            }

            return runtimeData.GetStatValue(statType);
        }

        public bool HasStat(StatType statType)
        {
            return FindStat(statType) != null;
        }

        public StatEntry FindStat(StatType statType)
        {
            if (runtimeData == null || runtimeData.stats == null)
            {
                return null;
            }

            for (int i = 0;
                 i < runtimeData.stats.Count;
                 i++)
            {
                StatEntry entry = runtimeData.stats[i];

                if (entry == null)
                {
                    continue;
                }

                if (entry.statType == statType)
                {
                    return entry;
                }
            }

            return null;
        }

        public void AddStat(
            StatType statType,
            float value)
        {
            if (runtimeData == null)
            {
                Debug.LogError(
                    "[CharacterStatService] RuntimeData is null.");

                return;
            }

            StatEntry entry = FindStat(statType);

            if (entry == null)
            {
                runtimeData.stats.Add(
                    new StatEntry
                    {
                        statType = statType,
                        value = value
                    });
            }
            else
            {
                entry.value += value;
            }

            RefreshFinalStats();

            OnStatChanged?.Invoke(
                statType,
                GetStat(statType));
        }

        public void SetStat(
            StatType statType,
            float value)
        {
            if (runtimeData == null)
            {
                Debug.LogError(
                    "[CharacterStatService] RuntimeData is null.");

                return;
            }

            StatEntry entry = FindStat(statType);

            if (entry == null)
            {
                runtimeData.stats.Add(
                    new StatEntry
                    {
                        statType = statType,
                        value = value
                    });
            }
            else
            {
                entry.value = value;
            }

            RefreshFinalStats();

            OnStatChanged?.Invoke(
                statType,
                GetStat(statType));
        }

        public void RemoveStat(StatType statType)
        {
            if (runtimeData == null || runtimeData.stats == null)
            {
                return;
            }

            runtimeData.stats.RemoveAll(
                x => x != null && x.statType == statType);

            RefreshFinalStats();

            OnStatChanged?.Invoke(
                statType,
                GetStat(statType));
        }

        public void RefreshFinalStats()
        {
            if (runtimeData == null)
            {
                return;
            }

            runtimeData.finalStats.Clear();

            if (runtimeData.stats == null)
            {
                return;
            }

            MergeNonFormulaStatsToFinalStats();

            SetCalculatedFinalStat(
                StatType.MaxHp,
                CalculateWithPercent(
                    StatType.MaxHp,
                    StatType.MaxHpPercent));

            SetCalculatedFinalStat(
                StatType.Attack,
                CalculateWithPercent(
                    StatType.Attack,
                    StatType.AttackPercent));

            ApplyMissingHpDerivedDamageStats();

            SetCalculatedFinalStat(
                StatType.AttackSpeed,
                CalculateWithPercent(
                    StatType.AttackSpeed,
                    StatType.AttackSpeedPercent));

            SetCalculatedFinalStat(
                StatType.MoveSpeed,
                CalculateWithPercent(
                    StatType.MoveSpeed,
                    StatType.MoveSpeedPercent));

            SetCalculatedFinalStat(
                StatType.Defense,
                CalculateWithPercent(
                    StatType.Defense,
                    StatType.DefensePercent));

            SetCalculatedFinalStat(
                StatType.GoldGain,
                CalculateWithPercent(
                    StatType.GoldGain,
                    StatType.GoldGainPercent));

            SetCalculatedFinalStat(
                StatType.ExpGain,
                CalculateWithPercent(
                    StatType.ExpGain,
                    StatType.ExpGainPercent));

            SetCalculatedFinalStat(
                StatType.RelicDropRate,
                CalculateWithPercent(
                    StatType.RelicDropRate,
                    StatType.RelicDropRatePercent));

            SetCalculatedFinalStat(
                StatType.Shield,
                CalculateWithPercent(
                    StatType.Shield,
                    StatType.ShieldPercent));

            SetCalculatedFinalStat(
                StatType.SkillRange,
                CalculateWithPercent(
                    StatType.SkillRange,
                    StatType.SkillRangePercent));

            SetCalculatedFinalStat(
                StatType.CooldownReduction,
                CalculateWithPercent(
                    StatType.CooldownReduction,
                    StatType.CooldownReductionPercent));

            ClampCurrentHpToMaxHp();
        }

        public void RefreshMissingHpDerivedDamageStats()
        {
            RefreshFinalStats();
        }

        private void ApplyMissingHpDerivedDamageStats()
        {
            if (runtimeData == null || runtimeData.stats == null)
            {
                return;
            }

            float maxHp = GetStat(StatType.MaxHp);
            float currentHp = GetRuntimeStatSum(StatType.Hp);

            if (maxHp <= 0f)
            {
                ApplyMissingHpAttackBonusDiff(0f);
                return;
            }

            float missingHp = Mathf.Max(
                0f,
                maxHp - currentHp);

            float missingHpRatio = Mathf.Clamp01(
                missingHp / maxHp);

            float missingHpPercent =
                missingHpRatio * 100f;

            float missingHpAttackPercent =
                GetRuntimeStatSum(StatType.MissingHpAttackPercent);

            float missingHpFinalDamageAmplify =
                GetRuntimeStatSum(StatType.MissingHpFinalDamageAmplify);

            float amplifiedMissingHpAttackPercent =
                missingHpAttackPercent
                + missingHpAttackPercent * (missingHpFinalDamageAmplify / 100f);

            float baseAttack =
                Mathf.Max(
                    0f,
                    GetRuntimeStatSum(StatType.Attack));

            float nextAttackBonus =
                baseAttack
                * (amplifiedMissingHpAttackPercent * missingHpPercent / 100f);

            ApplyMissingHpAttackBonusDiff(nextAttackBonus);
        }

        private void ApplyMissingHpAttackBonusDiff(float nextAttackBonus)
        {
            float diff = nextAttackBonus - appliedMissingHpAttackBonus;

            if (Mathf.Abs(diff) <= 0.0001f)
            {
                return;
            }

            AddOrMergeFinalStat(
                StatType.Attack,
                diff);

            appliedMissingHpAttackBonus = nextAttackBonus;

            OnStatChanged?.Invoke(
                StatType.Attack,
                GetStat(StatType.Attack));
        }

        private void ClampCurrentHpToMaxHp()
        {
            float maxHp = GetStat(StatType.MaxHp);
            float currentHp = GetStat(StatType.Hp);

            if (currentHp <= maxHp)
            {
                return;
            }

            SetStat(
                StatType.Hp,
                maxHp);
        }

        private void MergeNonFormulaStatsToFinalStats()
        {
            for (int i = 0;
                 i < runtimeData.stats.Count;
                 i++)
            {
                StatEntry entry = runtimeData.stats[i];

                if (entry == null)
                {
                    continue;
                }

                if (IsFormulaStat(entry.statType))
                {
                    continue;
                }

                AddOrMergeFinalStat(
                    entry.statType,
                    entry.value);
            }
        }

        private float CalculateWithPercent(
            StatType baseStatType,
            StatType percentStatType)
        {
            float baseValue =
                GetRuntimeStatSum(baseStatType);

            float percentValue =
                GetRuntimeStatSum(percentStatType);

            return baseValue
                   + baseValue * (percentValue / 100f);
        }

        private float GetRuntimeStatSum(StatType statType)
        {
            if (runtimeData == null || runtimeData.stats == null)
            {
                return 0f;
            }

            float value = 0f;

            for (int i = 0;
                 i < runtimeData.stats.Count;
                 i++)
            {
                StatEntry entry = runtimeData.stats[i];

                if (entry == null)
                {
                    continue;
                }

                if (entry.statType != statType)
                {
                    continue;
                }

                value += entry.value;
            }

            return value;
        }

        private void SetCalculatedFinalStat(
            StatType statType,
            float value)
        {
            runtimeData.finalStats.RemoveAll(
                x => x != null && x.statType == statType);

            runtimeData.finalStats.Add(
                new StatEntry
                {
                    statType = statType,
                    value = value
                });
        }

        private bool IsFormulaStat(StatType statType)
        {
            return statType == StatType.MaxHp
                   || statType == StatType.MaxHpPercent
                   || statType == StatType.Attack
                   || statType == StatType.AttackPercent
                   || statType == StatType.AttackSpeed
                   || statType == StatType.AttackSpeedPercent
                   || statType == StatType.MoveSpeed
                   || statType == StatType.MoveSpeedPercent
                   || statType == StatType.Defense
                   || statType == StatType.DefensePercent
                   || statType == StatType.GoldGain
                   || statType == StatType.GoldGainPercent
                   || statType == StatType.ExpGain
                   || statType == StatType.ExpGainPercent
                   || statType == StatType.RelicDropRate
                   || statType == StatType.RelicDropRatePercent
                   || statType == StatType.Shield
                   || statType == StatType.ShieldPercent
                   || statType == StatType.CooldownReduction
                   || statType == StatType.CooldownReductionPercent
                   || statType == StatType.MissingHpAttackPercent
                   || statType == StatType.MissingHpFinalDamageAmplify
                   || statType == StatType.SkillRange
                   || statType == StatType.SkillRangePercent;
        }

        private void AddOrMergeFinalStat(
            StatType statType,
            float value)
        {
            for (int i = 0;
                 i < runtimeData.finalStats.Count;
                 i++)
            {
                StatEntry entry = runtimeData.finalStats[i];

                if (entry == null)
                {
                    continue;
                }

                if (entry.statType != statType)
                {
                    continue;
                }

                entry.value += value;
                return;
            }

            runtimeData.finalStats.Add(
                new StatEntry
                {
                    statType = statType,
                    value = value
                });
        }
    }
}