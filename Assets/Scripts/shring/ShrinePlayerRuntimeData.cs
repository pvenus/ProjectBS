using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shrine
{
    [Serializable]
    public class ShrinePlayerRuntimeData
    {
        [Header("Common Blessing")]
        [SerializeField] private ShrineBlessingSO commonBlessing;

        [Header("Faith Levels")]
        [SerializeField] private List<ShrineFaithEntry> faithEntries = new();

        [Header("Visited Gods")]
        [SerializeField] private List<ShrineGodType> visitedGods = new();

        [Header("Locked Faith")]
        [SerializeField] private bool hasLockedFaith;
        [SerializeField] private ShrineGodType lockedGod = ShrineGodType.None;

        public ShrineBlessingSO CommonBlessing => commonBlessing;
        public IReadOnlyList<ShrineFaithEntry> FaithEntries => faithEntries;
        public IReadOnlyList<ShrineGodType> VisitedGods => visitedGods;

        public bool HasLockedFaith => hasLockedFaith;
        public ShrineGodType LockedGod => lockedGod;

        public void Reset()
        {
            commonBlessing = null;
            faithEntries.Clear();
            visitedGods.Clear();
            hasLockedFaith = false;
            lockedGod = ShrineGodType.None;
        }

        public bool HasBlessing(ShrineBlessingSO blessing)
        {
            if (blessing == null)
            {
                return false;
            }

            return commonBlessing == blessing;
        }

        public bool AddBlessing(ShrineBlessingSO blessing)
        {
            if (blessing == null)
            {
                return false;
            }

            if (blessing.godType != ShrineGodType.None)
            {
                return true;
            }

            commonBlessing = blessing;
            return true;
        }

        public bool RemoveBlessing(ShrineBlessingSO blessing)
        {
            if (blessing == null)
            {
                return false;
            }

            if (commonBlessing != blessing)
            {
                return false;
            }

            commonBlessing = null;
            return true;
        }

        public int GetFaithLevel(ShrineGodType godType)
        {
            ShrineFaithEntry entry = GetFaithEntry(godType);
            return entry?.faithLevel ?? 0;
        }

        public void AddFaith(ShrineGodType godType, int amount)
        {
            if (godType == ShrineGodType.None)
            {
                return;
            }

            if (amount <= 0)
            {
                return;
            }

            ShrineFaithEntry entry = GetOrCreateFaithEntry(godType);
            entry.faithLevel += amount;
            if (entry.faithLevel >= 5)
            {
                LockFaith(godType);
            }

            RegisterVisitedGod(godType);
        }

        public bool HasVisitedGod(ShrineGodType godType)
        {
            return visitedGods.Contains(godType);
        }

        public void RegisterVisitedGod(ShrineGodType godType)
        {
            if (godType == ShrineGodType.None)
            {
                return;
            }

            if (visitedGods.Contains(godType))
            {
                return;
            }

            visitedGods.Add(godType);
        }

        public bool CanSelectGod(ShrineGodType godType)
        {
            if (!hasLockedFaith)
            {
                return true;
            }

            return lockedGod == godType;
        }

        private void LockFaith(ShrineGodType godType)
        {
            hasLockedFaith = true;
            lockedGod = godType;

            RemoveOtherFaiths(godType);
        }

        private void RemoveOtherFaiths(ShrineGodType targetGod)
        {
            for (int i = faithEntries.Count - 1; i >= 0; i--)
            {
                ShrineFaithEntry entry = faithEntries[i];
                if (entry == null)
                {
                    continue;
                }

                if (entry.godType == targetGod)
                {
                    continue;
                }

                faithEntries.RemoveAt(i);
            }
        }

        public ShrineBlessingSO GetActiveGodBlessing(
            ShrineGodType godType,
            List<ShrineBlessingSO> blessingPool)
        {
            if (godType == ShrineGodType.None)
            {
                return null;
            }

            if (blessingPool == null || blessingPool.Count == 0)
            {
                return null;
            }

            int faithLevel = GetFaithLevel(godType);
            ShrineBlessingSO result = null;

            foreach (ShrineBlessingSO blessing in blessingPool)
            {
                if (blessing == null)
                {
                    continue;
                }

                if (blessing.godType != godType)
                {
                    continue;
                }

                if (blessing.progressionStep != faithLevel)
                {
                    continue;
                }

                if (result == null)
                {
                    result = blessing;
                    continue;
                }

                if (blessing.progressionStep > result.progressionStep)
                {
                    result = blessing;
                }
            }

            return result;
        }

        private ShrineFaithEntry GetFaithEntry(ShrineGodType godType)
        {
            return faithEntries.Find(x => x.godType == godType);
        }

        private ShrineFaithEntry GetOrCreateFaithEntry(ShrineGodType godType)
        {
            ShrineFaithEntry entry = GetFaithEntry(godType);
            if (entry != null)
            {
                return entry;
            }

            entry = new ShrineFaithEntry(godType);
            faithEntries.Add(entry);
            return entry;
        }
    }

    [Serializable]
    public class ShrineFaithEntry
    {
        public ShrineGodType godType;
        public int faithLevel;

        public ShrineFaithEntry(ShrineGodType godType)
        {
            this.godType = godType;
            faithLevel = 0;
        }
    }
}