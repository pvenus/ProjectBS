using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Shrine
{
    /// <summary>
    /// 플레이어의 신앙 진행 상태를 저장하는 런타임 데이터.
    /// 각 신의 신앙 레벨과 현재 확정 상태를 관리한다.
    /// </summary>
    [Serializable]
    public class FaithRuntimeData
    {
        [Header("Faith")]
        public List<FaithEntry> faithEntries = new();

        [Header("State")]
        public ShrineGodType lockedGod = ShrineGodType.None;
        public bool faithLocked;

        public bool HasLockedFaith => faithLocked && lockedGod != ShrineGodType.None;
        public bool HasAnyFaith => faithEntries.Any(x => x != null && x.faithLevel > 0);

        public FaithRuntimeData()
        {
            InitializeDefaults();
        }

        public void InitializeDefaults()
        {
            if (faithEntries.Count > 0)
            {
                return;
            }

            foreach (ShrineGodType godType in Enum.GetValues(typeof(ShrineGodType)))
            {
                if (godType == ShrineGodType.None)
                {
                    continue;
                }

                faithEntries.Add(new FaithEntry(godType));
            }
        }

        public FaithEntry GetEntry(ShrineGodType godType)
        {
            return faithEntries.FirstOrDefault(x => x != null && x.godType == godType);
        }

        public int GetFaithLevel(ShrineGodType godType)
        {
            FaithEntry entry = GetEntry(godType);
            return entry != null ? entry.faithLevel : 0;
        }

        public FaithStageState GetFaithState(ShrineGodType godType)
        {
            FaithEntry entry = GetEntry(godType);
            return entry != null ? entry.state : FaithStageState.None;
        }

        public bool CanIncreaseFaith(ShrineGodType godType)
        {
            if (godType == ShrineGodType.None)
            {
                return false;
            }

            if (!HasLockedFaith)
            {
                return true;
            }

            return lockedGod == godType;
        }

        public bool TryIncreaseFaith(ShrineGodType godType, int amount = 1)
        {
            if (!CanIncreaseFaith(godType))
            {
                Debug.LogWarning($"[FaithRuntimeData] Cannot increase faith. god={godType}, lockedGod={lockedGod}");
                return false;
            }

            FaithEntry entry = GetEntry(godType);
            if (entry == null)
            {
                entry = new FaithEntry(godType);
                faithEntries.Add(entry);
            }

            entry.AddFaith(amount);
            UpdateFaithState(entry);
            CheckFaithLock(entry);
            return true;
        }

        public void SetFaithLevel(ShrineGodType godType, int level)
        {
            FaithEntry entry = GetEntry(godType);
            if (entry == null)
            {
                entry = new FaithEntry(godType);
                faithEntries.Add(entry);
            }

            entry.faithLevel = Mathf.Clamp(level, 0, 10);
            UpdateFaithState(entry);
            CheckFaithLock(entry);
        }

        public bool IsMixedFaith()
        {
            int activeFaithCount = faithEntries.Count(x => x != null && x.faithLevel > 0);
            return activeFaithCount >= 2;
        }

        public List<FaithEntry> GetActiveFaiths()
        {
            return faithEntries
                .Where(x => x != null && x.faithLevel > 0)
                .OrderByDescending(x => x.faithLevel)
                .ToList();
        }

        public ShrineGodType GetHighestFaithGod()
        {
            FaithEntry highest = faithEntries
                .Where(x => x != null)
                .OrderByDescending(x => x.faithLevel)
                .FirstOrDefault();

            return highest != null ? highest.godType : ShrineGodType.None;
        }

        public void ResetFaith()
        {
            foreach (FaithEntry entry in faithEntries)
            {
                if (entry == null)
                {
                    continue;
                }

                entry.faithLevel = 0;
                entry.state = FaithStageState.Normal;
            }

            faithLocked = false;
            lockedGod = ShrineGodType.None;
        }

        private void UpdateFaithState(FaithEntry entry)
        {
            if (entry == null)
            {
                return;
            }

            int level = entry.faithLevel;

            if (level >= 10)
            {
                entry.state = FaithStageState.Successor;
            }
            else if (level >= 7)
            {
                entry.state = FaithStageState.Devoted;
            }
            else if (level >= 5)
            {
                entry.state = FaithStageState.Locked;
            }
            else if (level >= 1)
            {
                entry.state = FaithStageState.Influenced;
            }
            else
            {
                entry.state = FaithStageState.Normal;
            }
        }

        private void CheckFaithLock(FaithEntry entry)
        {
            if (entry == null)
            {
                return;
            }

            if (entry.faithLevel < 5)
            {
                return;
            }

            if (!faithLocked)
            {
                faithLocked = true;
                lockedGod = entry.godType;
            }
        }
    }

    [Serializable]
    public class FaithEntry
    {
        [Header("Identity")]
        public ShrineGodType godType = ShrineGodType.None;

        [Header("Faith")]
        [Range(0, 10)]
        public int faithLevel;

        public FaithStageState state = FaithStageState.Normal;

        public FaithEntry()
        {
        }

        public FaithEntry(ShrineGodType godType)
        {
            this.godType = godType;
            faithLevel = 0;
            state = FaithStageState.Normal;
        }

        public void AddFaith(int amount)
        {
            faithLevel = Mathf.Clamp(faithLevel + Mathf.Max(0, amount), 0, 10);
        }

        public void RemoveFaith(int amount)
        {
            faithLevel = Mathf.Clamp(faithLevel - Mathf.Max(0, amount), 0, 10);
        }
    }
}
