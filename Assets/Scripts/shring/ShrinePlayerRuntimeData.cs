using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shrine
{
    [Serializable]
    public class ShrinePlayerRuntimeData
    {
        [Header("Visited Gods")]
        [SerializeField] private List<ShrineGodType> visitedGods = new();

        [Header("Unlocked Gods")]
        [SerializeField] private List<ShrineGodType> unlockedGods = new();

        [Header("Locked Faith")]
        [SerializeField] private bool hasLockedFaith;
        [SerializeField] private ShrineGodType lockedGod = ShrineGodType.None;

        [Header("Pending Faith Ascension")]
        [SerializeField] private bool hasPendingFaithAscension;
        [SerializeField] private ShrineGodType pendingFaithGod = ShrineGodType.None;

        public IReadOnlyList<ShrineGodType> VisitedGods => visitedGods;
        public IReadOnlyList<ShrineGodType> UnlockedGods => unlockedGods;

        public bool HasLockedFaith => hasLockedFaith;
        public ShrineGodType LockedGod => lockedGod;

        public bool HasPendingFaithAscension => hasPendingFaithAscension;
        public ShrineGodType PendingFaithGod => pendingFaithGod;

        public void Reset()
        {
            visitedGods.Clear();
            unlockedGods.Clear();
            hasLockedFaith = false;
            lockedGod = ShrineGodType.None;
            hasPendingFaithAscension = false;
            pendingFaithGod = ShrineGodType.None;
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

        public void RequestFaithAscension(ShrineGodType godType)
        {
            if (godType == ShrineGodType.None)
            {
                return;
            }

            hasPendingFaithAscension = true;
            pendingFaithGod = godType;
        }

        public void ClearFaithAscensionRequest()
        {
            hasPendingFaithAscension = false;
            pendingFaithGod = ShrineGodType.None;
        }

        public void LockFaith(ShrineGodType godType)
        {
            ClearFaithAscensionRequest();
            hasLockedFaith = true;
            lockedGod = godType;
        }
    }
}