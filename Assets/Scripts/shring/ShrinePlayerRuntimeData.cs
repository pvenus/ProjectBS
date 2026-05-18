using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shrine
{
    [Serializable]
    public class ShrinePlayerRuntimeData
    {
        [Header("Faith Levels")]
        [SerializeField] private List<ShrineFaithEntry> faithEntries = new();

        [Header("Visited Gods")]
        [SerializeField] private List<ShrineGodType> visitedGods = new();

        [Header("Unlocked Gods")]
        [SerializeField] private List<ShrineGodType> unlockedGods = new();

        [Header("Locked Faith")]
        [SerializeField] private bool hasLockedFaith;
        [SerializeField] private ShrineGodType lockedGod = ShrineGodType.None;

        public IReadOnlyList<ShrineFaithEntry> FaithEntries => faithEntries;
        public IReadOnlyList<ShrineGodType> VisitedGods => visitedGods;
        public IReadOnlyList<ShrineGodType> UnlockedGods => unlockedGods;

        public bool HasLockedFaith => hasLockedFaith;
        public ShrineGodType LockedGod => lockedGod;

        public void RemoveFaiths(
            Predicate<ShrineFaithEntry> match)
        {
            faithEntries.RemoveAll(match);
        }

        public void Reset()
        {
            faithEntries.Clear();
            visitedGods.Clear();
            unlockedGods.Clear();
            hasLockedFaith = false;
            lockedGod = ShrineGodType.None;
        }

        public int GetFaithLevel(ShrineGodType godType)
        {
            ShrineFaithEntry entry = GetFaithEntry(godType);
            return entry?.faithLevel ?? 0;
        }

        public void SetFaithLevel(
            ShrineGodType godType,
            int level)
        {
            if (godType == ShrineGodType.None)
            {
                return;
            }

            if (level < 0)
            {
                level = 0;
            }

            ShrineFaithEntry entry =
                GetOrCreateFaithEntry(godType);

            entry.faithLevel = level;

            RegisterVisitedGod(godType);

            if (!hasLockedFaith
                && entry.faithLevel >= 5)
            {
                LockFaith(godType);
            }
        }

        public bool AddFaith(ShrineGodType godType, int amount)
        {
            if (godType == ShrineGodType.None)
            {
                return false;
            }

            if (amount <= 0)
            {
                return false;
            }

            ShrineFaithEntry entry = GetOrCreateFaithEntry(godType);
            entry.faithLevel += amount;
            bool becameLocked = false;

            if (!hasLockedFaith && entry.faithLevel >= 5)
            {
                LockFaith(godType);
                becameLocked = true;
            }

            RegisterVisitedGod(godType);

            return becameLocked;
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

        public bool IsGodUnlocked(ShrineGodType godType)
        {
            if (godType == ShrineGodType.None)
            {
                return false;
            }

            return unlockedGods.Contains(godType);
        }

        public bool UnlockGod(ShrineGodType godType)
        {
            if (godType == ShrineGodType.None)
            {
                return false;
            }

            if (unlockedGods.Contains(godType))
            {
                return false;
            }

            unlockedGods.Add(godType);
            return true;
        }

        public bool CanSelectGod(ShrineGodType godType)
        {
            if (godType == ShrineGodType.None)
            {
                return false;
            }

            if (unlockedGods.Count > 0
                && !unlockedGods.Contains(godType))
            {
                return false;
            }

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
        }


        private ShrineFaithEntry GetFaithEntry(ShrineGodType godType)
        {
            return faithEntries.Find(x => x.godType == godType);
        }

        public ShrineFaithEntry GetOrCreateFaithEntry(ShrineGodType godType)
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