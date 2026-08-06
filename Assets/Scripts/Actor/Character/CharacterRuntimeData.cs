using System;
using System.Collections.Generic;
using Stat;
using UnityEngine;
using Skill;

namespace Character
{
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