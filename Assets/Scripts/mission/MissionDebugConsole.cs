using System.Collections.Generic;
using UnityEngine;

namespace Mission
{
    /// <summary>
    /// Mission 디버그/테스트용 콘솔.
    /// Runtime 상태 확인 및 강제 진행 테스트를 지원한다.
    /// </summary>
    public class MissionDebugConsole : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField]
        private MissionManager missionManager;

        [Header("Debug")]
        [SerializeField]
        private bool logDebug = true;

        [ContextMenu("Mission/Print Runtime Missions")]
        public void PrintRuntimeMissions()
        {
            if (missionManager == null)
            {
                Debug.LogWarning(
                    "[MissionDebugConsole] MissionManager is null.");

                return;
            }

            IReadOnlyList<MissionRuntimeEntry> missions =
                missionManager.RuntimeData.Missions;

            Debug.Log(
                $"[MissionDebugConsole] Runtime Mission Count = {missions.Count}");

            for (int i = 0; i < missions.Count; i++)
            {
                MissionRuntimeEntry entry = missions[i];

                if (entry == null)
                {
                    continue;
                }

                MissionSO mission = entry.mission;

                string missionName =
                    mission != null
                        ? mission.DisplayName
                        : "NULL";

                IReadOnlyList<MissionConditionData> conditions =
                    mission != null
                        ? mission.Conditions
                        : null;

                string conditionText = string.Empty;

                if (conditions != null)
                {
                    for (int j = 0; j < conditions.Count; j++)
                    {
                        MissionConditionData condition =
                            conditions[j];

                        if (condition == null)
                        {
                            continue;
                        }

                        if (!string.IsNullOrWhiteSpace(conditionText))
                        {
                            conditionText += " | ";
                        }

                        conditionText +=
                            $"{condition.progressKey}:{condition.targetValue} x{condition.targetCount} ({condition.compareType})";
                    }
                }

                Debug.Log(
                    $"[MissionDebugConsole] mission={missionName}, active={entry.active}, completed={entry.completed}, claimed={entry.claimed}, conditions=[{conditionText}]");
            }
        }

        [ContextMenu("Mission/Test Shop Progress")]
        public void TestShopProgress()
        {
            AddProgress(
                MissionProgressKey.ShopVisit,
                1);
        }

        [ContextMenu("Mission/Test Dead Member Win")]
        public void TestDeadMemberWin()
        {
            AddProgress(
                MissionProgressKey.BattleWinWithDeadPartyMember,
                1);
        }

        [ContextMenu("Mission/Test Chaos Event")]
        public void TestChaosEvent()
        {
            AddProgress(
                MissionProgressKey.EventClear,
                1);
        }

        public void AddProgress(
            MissionProgressKey key,
            int amount)
        {
            if (missionManager == null)
            {
                return;
            }

            missionManager.NotifyProgress(
                key,
                amount);

            if (logDebug)
            {
                Debug.Log(
                    $"[MissionDebugConsole] Progress Added. key={key}, amount={amount}");
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F7))
            {
                PrintRuntimeMissions();
            }
        }
    }
}