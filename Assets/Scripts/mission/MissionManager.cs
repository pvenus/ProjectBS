using System;
using System.Collections.Generic;
using UnityEngine;
using Shrine;

namespace Mission
{
    public class MissionManager : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField]
        private List<MissionSO> missionDatabase = new();

        [Header("Runtime")]
        [SerializeField]
        private MissionRuntimeData runtimeData = new();

        [SerializeField]
        private ShrineManager shrineManager;

        [Header("Debug")]
        [SerializeField]
        private bool logDebug = true;

        public MissionRuntimeData RuntimeData => runtimeData;

        public event Action<MissionSO> OnMissionActivated;

        public event Action<MissionSO, int, int> OnMissionProgressed;

        public event Action<MissionSO> OnMissionCompleted;

        public MissionRuntimeEntry ActivateMission(
            MissionSO mission)
        {
            if (mission == null)
            {
                return null;
            }

            MissionRuntimeEntry entry =
                runtimeData.ActivateMission(mission);

            if (entry == null)
            {
                return null;
            }

            if (logDebug)
            {
                Debug.Log(
                    $"[MissionManager] Mission activated. mission={mission.displayName}");
            }

            OnMissionActivated?.Invoke(mission);
            return entry;
        }

        public void NotifyProgress(
            MissionProgressKey key,
            int amount = 1)
        {
            if (key == MissionProgressKey.None)
            {
                return;
            }

            bool hasStateCondition = false;

            IReadOnlyList<MissionRuntimeEntry> stateCheckMissions =
                runtimeData.Missions;

            for (int i = 0; i < stateCheckMissions.Count; i++)
            {
                MissionRuntimeEntry missionEntry =
                    stateCheckMissions[i];

                if (missionEntry == null
                    || missionEntry.mission == null
                    || missionEntry.mission.conditions == null)
                {
                    continue;
                }

                for (int j = 0;
                     j < missionEntry.mission.conditions.Count;
                     j++)
                {
                    MissionConditionData condition =
                        missionEntry.mission.conditions[j];

                    if (condition == null)
                    {
                        continue;
                    }

                    if (condition.progressKey != key)
                    {
                        continue;
                    }

                    if (condition.progressType
                        == MissionProgressType.State)
                    {
                        hasStateCondition = true;
                        break;
                    }
                }

                if (hasStateCondition)
                {
                    break;
                }
            }

            if (!hasStateCondition
                && amount <= 0)
            {
                return;
            }

            IReadOnlyList<MissionRuntimeEntry> missions =
                runtimeData.Missions;

            for (int i = 0; i < missions.Count; i++)
            {
                MissionRuntimeEntry entry = missions[i];

                if (entry == null)
                {
                    continue;
                }

                if (!entry.active)
                {
                    continue;
                }

                if (entry.completed)
                {
                    continue;
                }

                MissionSO mission = entry.mission;

                if (mission == null)
                {
                    continue;
                }

                if (mission.conditions == null
                    || mission.conditions.Count <= 0)
                {
                    continue;
                }

                bool hasMatchedCondition = false;

                for (int j = 0; j < mission.conditions.Count; j++)
                {
                    MissionConditionData condition =
                        mission.conditions[j];

                    if (condition == null)
                    {
                        continue;
                    }

                    if (condition.progressKey != key)
                    {
                        continue;
                    }

                    hasMatchedCondition = true;
                    break;
                }

                if (!hasMatchedCondition)
                {
                    continue;
                }

                bool completed =
                    runtimeData.AddProgress(
                        mission,
                        key,
                        amount);

                OnMissionProgressed?.Invoke(
                    mission,
                    entry.currentCount,
                    mission.GetTotalTargetCount());

                if (logDebug)
                {
                    Debug.Log(
                        $"[MissionManager] Mission progressed. mission={mission.displayName}, progress={entry.currentCount}/{mission.GetTotalTargetCount()}");
                }

                if (!completed)
                {
                    continue;
                }

                if (logDebug)
                {
                    Debug.Log(
                        $"[MissionManager] Mission completed. mission={mission.displayName}");
                }

                GiveReward(mission);

                OnMissionCompleted?.Invoke(mission);
            }
        }

        private void GiveReward(
            MissionSO mission)
        {
            if (mission == null)
            {
                return;
            }

            switch (mission.rewardType)
            {
                case MissionRewardType.Faith:
                {
                    if (shrineManager == null)
                    {
                        return;
                    }

                    ShrineGodType lockedGod =
                        shrineManager.CurrentShrine
                            .LockedGod;

                    if (lockedGod == ShrineGodType.None)
                    {
                        return;
                    }

                    bool becameLocked =
                        shrineManager.CurrentShrine
                            .AddFaith(
                                lockedGod,
                                mission.rewardFaith);

                    if (logDebug)
                    {
                        Debug.Log(
                            $"[MissionManager] Faith reward granted. god={lockedGod}, amount={mission.rewardFaith}, locked={becameLocked}");
                    }

                    break;
                }

                case MissionRewardType.UnlockGod:
                {
                    if (shrineManager == null)
                    {
                        return;
                    }

                    if (mission.unlockGodType == ShrineGodType.None)
                    {
                        return;
                    }

                    shrineManager.CurrentShrine
                        .availableGods
                        .Add(mission.unlockGodType);

                    if (logDebug)
                    {
                        Debug.Log(
                            $"[MissionManager] God unlocked. god={mission.unlockGodType}");
                    }

                    break;
                }
            }
        }

        public MissionSO GetMission(
            string missionId)
        {
            if (string.IsNullOrWhiteSpace(missionId))
            {
                return null;
            }

            return missionDatabase.Find(
                x => x != null && x.missionId == missionId);
        }
    }
}