

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Item
{
    [CreateAssetMenu(
        fileName = "ConsumePool",
        menuName = "BS/Item/Consume Pool")]
    public class ConsumePoolSO : ScriptableObject
    {
        [Serializable]
        public class ConsumePoolEntry
        {
            public ConsumeSO consume;

            [Min(1)]
            public int weight = 1;
        }

        [Header("Info")]
        public string poolId;

        public string displayName;

        [TextArea]
        public string description;

        [Header("Pool")]
        public List<ConsumePoolEntry> consumes = new();

        public ConsumeSO GetRandomConsume()
        {
            if (consumes == null
                || consumes.Count == 0)
            {
                return null;
            }

            int totalWeight = 0;

            foreach (ConsumePoolEntry entry in consumes)
            {
                if (entry == null
                    || entry.consume == null)
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

            foreach (ConsumePoolEntry entry in consumes)
            {
                if (entry == null
                    || entry.consume == null)
                {
                    continue;
                }

                currentWeight += Mathf.Max(1, entry.weight);

                if (randomValue < currentWeight)
                {
                    return entry.consume;
                }
            }

            return consumes[0] != null
                ? consumes[0].consume
                : null;
        }
    }
}