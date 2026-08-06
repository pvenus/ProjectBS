

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Item
{
    [CreateAssetMenu(
        fileName = "RelicPool",
        menuName = "BS/Item/Relic Pool")]
    public class RelicPoolSO : ScriptableObject
    {
        [Serializable]
        public class RelicPoolEntry
        {
            public RelicSO relic;

            [Min(1)]
            public int weight = 1;
        }

        [Header("Info")]
        public string poolId;

        public string displayName;

        [TextArea]
        public string description;

        [Header("Visual")]
        public Sprite icon;

        [Header("Pool")]
        public List<RelicPoolEntry> relics = new();

        public RelicSO GetRandomRelic()
        {
            if (relics == null
                || relics.Count == 0)
            {
                return null;
            }

            int totalWeight = 0;

            foreach (RelicPoolEntry entry in relics)
            {
                if (entry == null
                    || entry.relic == null)
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

            foreach (RelicPoolEntry entry in relics)
            {
                if (entry == null
                    || entry.relic == null)
                {
                    continue;
                }

                currentWeight += Mathf.Max(1, entry.weight);

                if (randomValue < currentWeight)
                {
                    return entry.relic;
                }
            }

            return relics[0] != null
                ? relics[0].relic
                : null;
        }
    }
}