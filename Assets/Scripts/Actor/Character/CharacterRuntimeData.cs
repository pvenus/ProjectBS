using System;
using System.Collections.Generic;
using Stat;
using UnityEngine;
using Skill;

namespace Character
{
    public enum CharacterSkillLevelCasResult
    {
        Applied = 0,
        MissingTarget = 10,
        DuplicateTarget = 20,
        StaleLevel = 30
    }

    public enum CharacterVitalCasResult
    {
        Applied = 0,
        MissingCurrentHp = 10,
        DuplicateCurrentHp = 20,
        MissingMaxHp = 30,
        DuplicateMaxHp = 40,
        InvalidValue = 50,
        MirrorMismatch = 60,
        StaleCurrentHp = 70
    }

    [Serializable]
    public class CharacterRuntimeData
    {
        [Header("Definition")]
        public CharacterSO characterSO;

        [Header("Progression")]
        public bool isDead;

        [Header("Skill Progression")]
        public List<EquipmentSkillInstanceData> skillInstances = new();

        [Header("Runtime Stats")]
        public List<StatEntry> stats = new();

        [Header("Final Runtime Stats")]
        public List<StatEntry> finalStats = new();

        public EquipmentSkillInstanceData GetSkillInstance(string equipmentId)
        {
            if (string.IsNullOrWhiteSpace(equipmentId))
            {
                return null;
            }

            for (int i = 0; i < skillInstances.Count; i++)
            {
                EquipmentSkillInstanceData instance = skillInstances[i];
                if (instance == null)
                {
                    continue;
                }

                if (instance.equipmentId == equipmentId)
                {
                    return instance;
                }
            }

            return null;
        }

        public int CountSkillInstances(string equipmentId)
        {
            if (string.IsNullOrWhiteSpace(equipmentId) || skillInstances == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < skillInstances.Count; i++)
            {
                EquipmentSkillInstanceData instance = skillInstances[i];
                if (instance != null
                    && string.Equals(instance.equipmentId, equipmentId, StringComparison.Ordinal))
                {
                    count++;
                }
            }

            return count;
        }

        public CharacterSkillLevelCasResult TryCompareExchangeSkillLevel(
            string equipmentId,
            int expectedLevel,
            int replacementLevel)
        {
            if (string.IsNullOrWhiteSpace(equipmentId) || skillInstances == null)
            {
                return CharacterSkillLevelCasResult.MissingTarget;
            }

            EquipmentSkillInstanceData target = null;
            int matchCount = 0;
            for (int i = 0; i < skillInstances.Count; i++)
            {
                EquipmentSkillInstanceData instance = skillInstances[i];
                if (instance == null
                    || !string.Equals(instance.equipmentId, equipmentId, StringComparison.Ordinal))
                {
                    continue;
                }

                target = instance;
                matchCount++;
            }

            if (matchCount == 0)
            {
                return CharacterSkillLevelCasResult.MissingTarget;
            }

            if (matchCount != 1)
            {
                return CharacterSkillLevelCasResult.DuplicateTarget;
            }

            int currentLevel = Mathf.Max(1, target.currentLevel);
            if (currentLevel != expectedLevel)
            {
                return CharacterSkillLevelCasResult.StaleLevel;
            }

            target.currentLevel = replacementLevel;
            target.upgradeLevel = Mathf.Max(0, replacementLevel - 1);
            return CharacterSkillLevelCasResult.Applied;
        }

        public CharacterVitalCasResult TryReadExactVitalStats(
            out float currentHp,
            out float maxHp)
        {
            currentHp = 0;
            maxHp = 0;
            CharacterVitalCasResult resolved = TryResolveExactVitalEntries(
                out StatEntry rawCurrent,
                out StatEntry finalCurrent,
                out StatEntry rawMax,
                out StatEntry finalMax);
            if (resolved != CharacterVitalCasResult.Applied)
            {
                return resolved;
            }

            if (!TryValidCurrentHp(rawCurrent.value, out currentHp)
                || !TryValidCurrentHp(finalCurrent.value, out float mirroredCurrent)
                || !TryValidMaxHp(rawMax.value, out maxHp)
                || !TryValidMaxHp(finalMax.value, out float mirroredMax))
            {
                currentHp = 0;
                maxHp = 0;
                return CharacterVitalCasResult.InvalidValue;
            }

            if (!Progression.CanonicalFloatBits.AreEqual(currentHp, mirroredCurrent)
                || !Progression.CanonicalFloatBits.AreEqual(maxHp, mirroredMax))
            {
                currentHp = 0;
                maxHp = 0;
                return CharacterVitalCasResult.MirrorMismatch;
            }

            return CharacterVitalCasResult.Applied;
        }

        public CharacterVitalCasResult TryCompareExchangeCurrentHp(
            float expectedCurrentHp,
            float replacementCurrentHp)
        {
            if (!TryValidCurrentHp(expectedCurrentHp, out _)
                || !TryValidCurrentHp(replacementCurrentHp, out _))
            {
                return CharacterVitalCasResult.InvalidValue;
            }

            CharacterVitalCasResult resolved = TryResolveExactVitalEntries(
                out StatEntry rawCurrent,
                out StatEntry finalCurrent,
                out StatEntry rawMax,
                out StatEntry finalMax);
            if (resolved != CharacterVitalCasResult.Applied)
            {
                return resolved;
            }

            if (!TryValidCurrentHp(rawCurrent.value, out float rawValue)
                || !TryValidCurrentHp(finalCurrent.value, out float finalValue)
                || !TryValidMaxHp(rawMax.value, out float rawMaxValue)
                || !TryValidMaxHp(finalMax.value, out float finalMaxValue))
            {
                return CharacterVitalCasResult.InvalidValue;
            }

            if (!Progression.CanonicalFloatBits.AreEqual(rawValue, finalValue)
                || !Progression.CanonicalFloatBits.AreEqual(rawMaxValue, finalMaxValue))
            {
                return CharacterVitalCasResult.MirrorMismatch;
            }

            if (!Progression.CanonicalFloatBits.AreEqual(rawValue, expectedCurrentHp))
            {
                return CharacterVitalCasResult.StaleCurrentHp;
            }

            rawCurrent.value = replacementCurrentHp;
            finalCurrent.value = replacementCurrentHp;
            return CharacterVitalCasResult.Applied;
        }

        private CharacterVitalCasResult TryResolveExactVitalEntries(
            out StatEntry rawCurrent,
            out StatEntry finalCurrent,
            out StatEntry rawMax,
            out StatEntry finalMax)
        {
            rawCurrent = null;
            finalCurrent = null;
            rawMax = null;
            finalMax = null;
            int rawCurrentCount = CountStatEntries(stats, StatType.Hp, out rawCurrent);
            int finalCurrentCount = CountStatEntries(finalStats, StatType.Hp, out finalCurrent);
            int rawMaxCount = CountStatEntries(stats, StatType.MaxHp, out rawMax);
            int finalMaxCount = CountStatEntries(finalStats, StatType.MaxHp, out finalMax);
            if (rawCurrentCount == 0 || finalCurrentCount == 0)
            {
                return CharacterVitalCasResult.MissingCurrentHp;
            }

            if (rawCurrentCount != 1 || finalCurrentCount != 1)
            {
                return CharacterVitalCasResult.DuplicateCurrentHp;
            }

            if (rawMaxCount == 0 || finalMaxCount == 0)
            {
                return CharacterVitalCasResult.MissingMaxHp;
            }

            return rawMaxCount == 1 && finalMaxCount == 1
                ? CharacterVitalCasResult.Applied
                : CharacterVitalCasResult.DuplicateMaxHp;
        }

        private static int CountStatEntries(
            List<StatEntry> entries,
            StatType statType,
            out StatEntry match)
        {
            match = null;
            if (entries == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < entries.Count; i++)
            {
                StatEntry entry = entries[i];
                if (entry != null && entry.statType == statType)
                {
                    match = entry;
                    count++;
                }
            }

            return count;
        }

        private static bool TryValidCurrentHp(float value, out float result)
        {
            result = 0f;
            if (!Progression.CanonicalFloatBits.IsFinite(value)
                || Progression.CanonicalFloatBits.IsNegativeZero(value)
                || value <= 0f
                )
            {
                return false;
            }
            result = value;
            return true;
        }

        private static bool TryValidMaxHp(float value, out float result)
        {
            result = 0f;
            if (!Progression.CanonicalFloatBits.IsFinite(value)
                || Progression.CanonicalFloatBits.IsNegativeZero(value)
                || Progression.CanonicalFloatBits.IsPositiveSubnormal(value)
                || value < 1f)
                return false;
            result = value;
            return true;
        }

        public EquipmentSkillInstanceData GetOrCreateSkillInstance(string equipmentId)
        {
            EquipmentSkillInstanceData instance = GetSkillInstance(equipmentId);
            if (instance != null)
            {
                return instance;
            }

            instance = new EquipmentSkillInstanceData
            {
                equipmentId = equipmentId,
                currentLevel = 1,
                upgradeLevel = 0
            };

            skillInstances.Add(instance);
            return instance;
        }

        public void SetSkillLevel(string equipmentId, int level)
        {
            EquipmentSkillInstanceData instance = GetOrCreateSkillInstance(equipmentId);
            instance.currentLevel = Mathf.Max(1, level);
            instance.upgradeLevel = Mathf.Max(0, instance.currentLevel - 1);
        }

        public int GetSkillLevel(string equipmentId)
        {
            EquipmentSkillInstanceData instance = GetSkillInstance(equipmentId);
            return instance == null
                ? 1
                : Mathf.Max(1, instance.currentLevel);
        }

        public float GetStatValue(StatType statType)
        {
            for (int i = 0;
                 i < finalStats.Count;
                 i++)
            {
                if (finalStats[i].statType != statType)
                {
                    continue;
                }

                return finalStats[i].value;
            }

            return 0f;
        }
    }
}
