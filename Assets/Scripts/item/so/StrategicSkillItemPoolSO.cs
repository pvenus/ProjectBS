

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Item
{
    [CreateAssetMenu(
        fileName = "StrategicSkillItemPool",
        menuName = "BS/Item/Strategic Skill Item Pool")]
    public class StrategicSkillItemPoolSO : ScriptableObject
    {
        [Serializable]
        public class StrategicSkillItemPoolEntry
        {
            public StrategicSkillItemSO strategicSkillItem;

            [Min(1)]
            public int weight = 1;
        }

        [Header("Info")]
        public string poolId;

        public string displayName;

        [TextArea]
        public string description;

        [Header("Pool")]
        public List<StrategicSkillItemPoolEntry> strategicSkillItems = new();

        public StrategicSkillItemSO GetRandomStrategicSkillItem()
        {
            if (strategicSkillItems == null
                || strategicSkillItems.Count == 0)
            {
                return null;
            }

            int totalWeight = 0;

            foreach (StrategicSkillItemPoolEntry entry in strategicSkillItems)
            {
                if (entry == null
                    || entry.strategicSkillItem == null)
                {
                    continue;
                }

                totalWeight += Mathf.Max(1, entry.weight);
            }

            if (totalWeight <= 0)
            {
                return null;
            }

            int randomValue = UnityEngine.Random.Range(0, totalWeight);
            int currentWeight = 0;

            foreach (StrategicSkillItemPoolEntry entry in strategicSkillItems)
            {
                if (entry == null
                    || entry.strategicSkillItem == null)
                {
                    continue;
                }

                currentWeight += Mathf.Max(1, entry.weight);

                if (randomValue < currentWeight)
                {
                    return entry.strategicSkillItem;
                }
            }

            return strategicSkillItems[0] != null
                ? strategicSkillItems[0].strategicSkillItem
                : null;
        }
    }
}