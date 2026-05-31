using System.Collections.Generic;
using System;
using UnityEngine;

namespace Item
{
    [System.Serializable]
    public class RelicRuntimeData
    {
        [SerializeField]
        private List<RelicEntry> relics = new();

        [SerializeField]
        private int maxRelicCount = 3;

        public IReadOnlyList<RelicEntry> Relics => relics;

        public int MaxRelicCount => maxRelicCount;


        public bool AddRelic(RelicSO relic)
        {
            if (relic == null)
            {
                return false;
            }

            if (HasRelic(relic))
            {
                return false;
            }

            int equippedCount =
                relics.FindAll(x => x != null
                    && x.isEquipped).Count;

            RelicEntry entry = new()
            {
                relic = relic,
                acquiredAt = DateTime.UtcNow.Ticks,
                isEquipped = equippedCount < maxRelicCount,
            };

            relics.Add(entry);
            return true;
        }

        public bool RemoveRelic(RelicSO relic)
        {
            if (relic == null)
            {
                return false;
            }

            RelicEntry entry = FindRelic(relic);

            if (entry == null)
            {
                return false;
            }

            return relics.Remove(entry);
        }

        public bool HasRelic(RelicSO relic)
        {
            return FindRelic(relic) != null;
        }

        public RelicEntry FindRelic(RelicSO relic)
        {
            if (relic == null)
            {
                return null;
            }

            return relics.Find(x => x != null
                && x.relic == relic);
        }

        public void Clear()
        {
            relics.Clear();
        }
    }

    [Serializable]
    public class RelicEntry
    {
        public RelicSO relic;

        public long acquiredAt;

        public bool isEquipped = true;
    }
}