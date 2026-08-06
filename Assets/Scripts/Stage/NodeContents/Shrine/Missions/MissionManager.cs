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
                    $"[MissionManager] Mission activated. mission={mission.DisplayName}");
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
                    || missionEntry.mission.Conditions == null)
                {
                    continue;
                }

                IReadOnlyList<MissionConditionData> entryConditions =
                    missionEntry.mission.Conditions;

                for (int j = 0;
                     j < entryConditions.Count;
                     j++)
                {
                    MissionConditionData condition =
                        entryConditions[j];

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

                IReadOnlyList<MissionConditionData> missionConditions =
                    mission.Conditions;

                if (missionConditions == null
                    || missionConditions.Count <= 0)
                {
                    continue;
                }

                bool hasMatchedCondition = false;

                for (int j = 0; j < missionConditions.Count; j++)
                {
                    MissionConditionData condition =
                        missionConditions[j];

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
                        $"[MissionManager] Mission progressed. mission={mission.DisplayName}, progress={entry.currentCount}/{mission.GetTotalTargetCount()}");
                }

                if (!completed)
                {
                    continue;
                }

                if (logDebug)
                {
                    Debug.Log(
                        $"[MissionManager] Mission completed. mission={mission.DisplayName}");
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

            switch (mission.RewardType)
            {
                case MissionRewardType.Faith:
                {
                    if (shrineManager == null)
                    {
                        return;
                    }

                    ShrineGodType lockedGod =
                        shrineManager.PlayerRuntimeData
                            .LockedGod;

                    if (lockedGod == ShrineGodType.None)
                    {
                        return;
                    }

                    int faithLevel =
                        shrineManager.AddFaith(
                            lockedGod,
                            mission.RewardFaith);

                    if (logDebug)
                    {
                        Debug.Log(
                            $"[MissionManager] Faith reward granted. god={lockedGod}, amount={mission.RewardFaith}, level={faithLevel}");
                    }

                    break;
                }

                case MissionRewardType.UnlockGod:
                {
                    if (shrineManager == null)
                    {
                        return;
                    }

                    if (mission.UnlockGodType == ShrineGodType.None)
                    {
                        return;
                    }

                    shrineManager.CurrentShrine
                        .availableGods
                        .Add(mission.UnlockGodType);

                    if (logDebug)
                    {
                        Debug.Log(
                            $"[MissionManager] God unlocked. god={mission.UnlockGodType}");
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
                x => x != null && x.MissionId == missionId);
        }
    }
}