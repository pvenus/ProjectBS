using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mission
{
    [Serializable]
    public class MissionRuntimeData
    {
        [SerializeField]
        private List<MissionRuntimeEntry> missions = new();

        public IReadOnlyList<MissionRuntimeEntry> Missions => missions;

        public MissionRuntimeEntry GetMission(
            MissionSO mission)
        {
            if (mission == null)
            {
                return null;
            }

            return missions.Find(x => x.mission == mission);
        }

        public MissionRuntimeEntry ActivateMission(
            MissionSO mission)
        {
            if (mission == null)
            {
                return null;
            }

            MissionRuntimeEntry entry =
                GetMission(mission);

            if (entry != null)
            {
                entry.active = true;
                return entry;
            }

            entry = new MissionRuntimeEntry
            {
                mission = mission,
                active = true,
                currentCount = 0,
                completed = false,
                claimed = false,
                conditions = new List<MissionConditionRuntimeEntry>(),
            };

            IReadOnlyList<MissionConditionData> conditions =
                mission.Conditions;

            if (conditions != null)
            {
                for (int i = 0; i < conditions.Count; i++)
                {
                    MissionConditionData condition =
                        conditions[i];

                    if (condition == null)
                    {
                        continue;
                    }

                    entry.conditions.Add(
                        new MissionConditionRuntimeEntry
                        {
                            progressKey = condition.progressKey,
                            currentCount = 0,
                            completed = false,
                        });
                }
            }

            missions.Add(entry);
            return entry;
        }

        public bool AddProgress(
            MissionSO mission,
            MissionProgressKey key,
            int amount)
        {
            if (mission == null)
            {
                return false;
            }


            MissionRuntimeEntry entry =
                GetMission(mission);

            if (entry == null)
            {
                return false;
            }

            if (!entry.active)
            {
                return false;
            }

            if (entry.completed)
            {
                return false;
            }
            IReadOnlyList<MissionConditionData> conditions =
                mission.Conditions;

            if (conditions == null)
            {
                return false;
            }

            MissionConditionRuntimeEntry runtimeCondition =
                entry.conditions.Find(x => x.progressKey == key);

            if (runtimeCondition == null)
            {
                return false;
            }

            MissionConditionData missionCondition = null;

            for (int i = 0; i < conditions.Count; i++)
            {
                MissionConditionData condition = conditions[i];

                if (condition == null || condition.progressKey != key)
                {
                    continue;
                }

                missionCondition = condition;
                break;
            }

            if (missionCondition == null)
            {
                return false;
            }

            if (missionCondition.progressType
                != MissionProgressType.State
                && amount <= 0)
            {
                return false;
            }

            if (missionCondition.progressType
                == MissionProgressType.Counter)
            {
                if (missionCondition.targetCount <= 0)
                {
                    Debug.LogWarning(
                        $"[MissionRuntimeData] Counter mission requires targetCount > 0. mission={mission.DisplayName}, key={key}");

                    return false;
                }
            }

            if (missionCondition.progressType
                == MissionProgressType.State)
            {
                if (missionCondition.targetValue == 0
                    && missionCondition.compareType
                        != MissionCompareType.Equal)
                {
                    Debug.LogWarning(
                        $"[MissionRuntimeData] State mission requires valid targetValue. mission={mission.DisplayName}, key={key}");
                }
            }

            bool compareSuccess = false;

            bool requiresCompareType =
                missionCondition.progressType
                == MissionProgressType.State;

            if (requiresCompareType
                && missionCondition.compareType
                    == MissionCompareType.None)
            {
                Debug.LogWarning(
                    $"[MissionRuntimeData] CompareType is required for State mission. mission={mission.DisplayName}, key={key}");

                return false;
            }

            switch (missionCondition.compareType)
            {
                case MissionCompareType.None:
                    compareSuccess = true;
                    break;

                case MissionCompareType.GreaterOrEqual:
                    compareSuccess =
                        runtimeCondition.currentCount >= missionCondition.targetValue;
                    break;

                case MissionCompareType.LessOrEqual:
                    compareSuccess =
                        runtimeCondition.currentCount <= missionCondition.targetValue;
                    break;

                case MissionCompareType.Equal:
                    compareSuccess =
                        runtimeCondition.currentCount == missionCondition.targetValue;
                    break;
            }

            if (missionCondition.progressType
                == MissionProgressType.State)
            {
                runtimeCondition.currentCount = amount;
            }
            else
            {
                runtimeCondition.currentCount += amount;
            }

            bool shouldIncreaseCompletedCount =
                compareSuccess
                && !runtimeCondition.lastCompareSuccess;

            if (shouldIncreaseCompletedCount)
            {
                runtimeCondition.completedCount++;

                if (missionCondition.progressType
                    != MissionProgressType.State)
                {
                    runtimeCondition.currentCount = 0;
                }
            }

            runtimeCondition.lastCompareSuccess =
                compareSuccess;

            runtimeCondition.completed =
                runtimeCondition.completedCount >= missionCondition.targetCount;

            entry.currentCount = 0;

            bool allCompleted = true;

            for (int i = 0; i < entry.conditions.Count; i++)
            {
                MissionConditionRuntimeEntry condition =
                    entry.conditions[i];

                if (condition == null)
                {
                    continue;
                }

                entry.currentCount += condition.currentCount;

                if (!condition.completed)
                {
                    allCompleted = false;
                }
            }

            entry.completed = allCompleted;
            return entry.completed;
        }

        public void Clear()
        {
            missions.Clear();
        }
    }

    [Serializable]
    public class MissionRuntimeEntry
    {
        public MissionSO mission;

        public bool active;

        public int currentCount;
        public List<MissionConditionRuntimeEntry> conditions = new();

        public bool completed;

        public bool claimed;
    }

    [Serializable]
    public class MissionConditionRuntimeEntry
    {
        public MissionProgressKey progressKey;

        public int currentCount;

        public int completedCount;

        public bool completed;

        public bool lastCompareSuccess;
    }
}