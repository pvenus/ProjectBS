using System;
using System.Collections.Generic;

using UnityEngine;

namespace Stage
{
    [CreateAssetMenu(
        fileName = "EventPool",
        menuName = "Stage/Event Pool")]
    public class EventPoolSO : ScriptableObject
    {
        [Header("Info")]
        public string poolId;
        public string displayName;

        [Header("Entries")]
        public List<EventPoolEntry> entries = new();

        public List<EventPoolEntry> GetAvailableEntries(
            int currentDepth)
        {
            List<EventPoolEntry> result = new();

            foreach (EventPoolEntry entry in entries)
            {
                if (entry == null)
                {
                    continue;
                }

                if (entry.node == null)
                {
                    continue;
                }

                if (currentDepth < entry.minDepth)
                {
                    continue;
                }

                if (entry.maxDepth > 0
                    && currentDepth > entry.maxDepth)
                {
                    continue;
                }

                result.Add(entry);
            }

            return result;
        }
    }

    [Serializable]
    public class EventPoolEntry
    {
        public RoundNodeSO node;

        [Header("Info")]
        public string entryId;

        [Range(0.01f, 100f)]
        public float weight = 1f;

        [Header("Runtime")]
        public bool oneShot;
        public int cooldownRounds;

        [Header("Depth")]
        public int minDepth;
        public int maxDepth;

        [Header("Tags")]
        public List<string> tags = new();
    }
}
