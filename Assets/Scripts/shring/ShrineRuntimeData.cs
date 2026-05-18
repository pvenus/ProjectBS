using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Bless;

namespace Shrine
{
    /// <summary>
    /// 현재 열린 신전의 런타임 상태 데이터.
    /// UI와 ShrineManager가 공유하는 진행 상태를 담는다.
    /// </summary>
    [Serializable]
    public class ShrineRuntimeData
    {
        [Header("Identity")]
        public string shrineId;
        public string shrineName;

        [Header("Flow")]
        public ShrineFlowState flowState = ShrineFlowState.None;
        public ShrineActionType selectedAction = ShrineActionType.None;
        public ShrineGodType selectedGod = ShrineGodType.None;

        [Header("Runtime Flags")]
        public bool isOpened;
        public bool isCompleted;
        public bool healApplied;
        public bool blessingSelected;
        public bool faithActionApplied;

        [Header("Blessing Candidates")]
        public List<BlessRuntimeData.BlessEntry> blessingCandidates = new();
        public BlessRuntimeData.BlessEntry selectedBlessing;

        [Header("Available Gods")]
        public List<ShrineGodType> availableGods = new();

        [Header("Faith")]
        public List<ShrineFaithEntry> faithEntries = new();
        public bool hasLockedFaith;
        public ShrineGodType lockedGod = ShrineGodType.None;

        [Header("Debug")]
        public int seed;
        public string generatedFromConfigId;

        public bool HasBlessingCandidates => blessingCandidates != null && blessingCandidates.Count > 0;
        public bool HasSelectedBlessing => selectedBlessing != null;
        public bool HasSelectedGod => selectedGod != ShrineGodType.None;

        public bool HasLockedFaith => hasLockedFaith;
        public ShrineGodType LockedGod => lockedGod;

        public ShrineRuntimeData()
        {
        }

        public ShrineRuntimeData(string shrineId, string shrineName)
        {
            this.shrineId = shrineId;
            this.shrineName = shrineName;
        }

        public void Open()
        {
            isOpened = true;
            isCompleted = false;
            flowState = ShrineFlowState.MainSelection;
        }

        public void Complete()
        {
            isCompleted = true;
            isOpened = false;
            flowState = ShrineFlowState.Complete;
        }

        public void ResetFlow()
        {
            flowState = ShrineFlowState.None;
            selectedAction = ShrineActionType.None;
            selectedGod = ShrineGodType.None;
            isOpened = false;
            isCompleted = false;
            healApplied = false;
            blessingSelected = false;
            faithActionApplied = false;
            selectedBlessing = null;
            blessingCandidates.Clear();
            availableGods.Clear();
            hasLockedFaith = false;
            lockedGod = ShrineGodType.None;
            faithEntries.Clear();
        }

        public void SetAction(ShrineActionType actionType)
        {
            selectedAction = actionType;

            switch (actionType)
            {
                case ShrineActionType.HealAndBless:
                    flowState = ShrineFlowState.BlessingSelection;
                    break;
                case ShrineActionType.Pray:
                case ShrineActionType.Donate:
                    flowState = ShrineFlowState.GodSelection;
                    break;
                case ShrineActionType.Leave:
                    flowState = ShrineFlowState.Complete;
                    break;
                default:
                    flowState = ShrineFlowState.MainSelection;
                    break;
            }
        }

        public void SetAvailableGods(IEnumerable<ShrineGodType> gods)
        {
            availableGods.Clear();

            if (gods == null)
            {
                return;
            }

            foreach (ShrineGodType god in gods)
            {
                if (god == ShrineGodType.None)
                {
                    continue;
                }

                if (!availableGods.Contains(god))
                {
                    availableGods.Add(god);
                }
            }
        }

        public bool IsGodAvailable(ShrineGodType godType)
        {
            return godType != ShrineGodType.None && availableGods.Contains(godType);
        }

        public void SelectGod(ShrineGodType godType)
        {
            if (!IsGodAvailable(godType))
            {
                Debug.LogWarning($"[ShrineRuntimeData] God is not available. god={godType}");
                return;
            }

            selectedGod = godType;
            flowState = ShrineFlowState.FaithActionSelection;
        }

        public void SetBlessingCandidates(IEnumerable<BlessRuntimeData.BlessEntry> candidates)
        {
            blessingCandidates.Clear();
            selectedBlessing = null;
            blessingSelected = false;

            if (candidates == null)
            {
                return;
            }

            int index = 0;
            foreach (BlessRuntimeData.BlessEntry candidate in candidates)
            {
                if (candidate == null)
                {
                    continue;
                }

                candidate.slotIndex = index;
                blessingCandidates.Add(candidate);
                index++;
            }
        }

        public BlessRuntimeData.BlessEntry GetBlessingCandidate(string runtimeId)
        {
            if (string.IsNullOrWhiteSpace(runtimeId))
            {
                return null;
            }

            return blessingCandidates.FirstOrDefault(x => x != null && x.runtimeId == runtimeId);
        }

        public BlessRuntimeData.BlessEntry GetBlessingCandidateBySlot(int slotIndex)
        {
            return blessingCandidates.FirstOrDefault(x => x != null && x.slotIndex == slotIndex);
        }

        public bool SelectBlessing(string runtimeId)
        {
            BlessRuntimeData.BlessEntry blessing = GetBlessingCandidate(runtimeId);
            if (blessing == null)
            {
                Debug.LogWarning($"[ShrineRuntimeData] Blessing candidate not found. runtimeId={runtimeId}");
                return false;
            }

            selectedBlessing = blessing;
            blessingSelected = true;
            flowState = ShrineFlowState.Reward;
            return true;
        }

        public bool SelectBlessingBySlot(int slotIndex)
        {
            BlessRuntimeData.BlessEntry blessing = GetBlessingCandidateBySlot(slotIndex);
            if (blessing == null)
            {
                Debug.LogWarning($"[ShrineRuntimeData] Blessing candidate not found. slotIndex={slotIndex}");
                return false;
            }

            selectedBlessing = blessing;
            blessingSelected = true;
            flowState = ShrineFlowState.Reward;
            return true;
        }

        public void MarkHealApplied()
        {
            healApplied = true;
        }

        public void MarkFaithActionApplied()
        {
            faithActionApplied = true;
            flowState = ShrineFlowState.Reward;
        }

        public int GetFaithLevel(ShrineGodType godType)
        {
            if (godType == ShrineGodType.None)
            {
                return 0;
            }

            for (int i = 0; i < faithEntries.Count; i++)
            {
                ShrineFaithEntry entry = faithEntries[i];

                if (entry == null)
                {
                    continue;
                }

                if (entry.godType != godType)
                {
                    continue;
                }

                return entry.faithLevel;
            }

            return 0;
        }

        public ShrineFaithEntry GetOrCreateFaithEntry(
            ShrineGodType godType)
        {
            for (int i = 0; i < faithEntries.Count; i++)
            {
                ShrineFaithEntry entry = faithEntries[i];

                if (entry == null)
                {
                    continue;
                }

                if (entry.godType == godType)
                {
                    return entry;
                }
            }

            ShrineFaithEntry created = new ShrineFaithEntry(godType)
            {
                faithLevel = 0
            };

            faithEntries.Add(created);
            return created;
        }

        public bool AddFaith(
            ShrineGodType godType,
            int amount)
        {
            if (godType == ShrineGodType.None)
            {
                return false;
            }

            ShrineFaithEntry entry =
                GetOrCreateFaithEntry(godType);

            entry.faithLevel += amount;

            if (!hasLockedFaith
                && entry.faithLevel >= 5)
            {
                hasLockedFaith = true;
                lockedGod = godType;
                return true;
            }

            return false;
        }
    }
}