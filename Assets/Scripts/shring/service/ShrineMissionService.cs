using System.Collections.Generic;
using Mission;
using UnityEngine;

namespace Shrine
{
    /// <summary>
    /// Shrine 전용 Mission 연결 서비스.
    /// Unlock Mission 등록 및 Faith Mission 활성화를 담당한다.
    /// </summary>
    public class ShrineMissionService
    {
        private readonly ShrineConfigSO config;

        private readonly MissionManager missionManager;

        private readonly bool logDebug;

        public ShrineMissionService(
            ShrineConfigSO config,
            MissionManager missionManager,
            bool logDebug)
        {
            this.config = config;
            this.missionManager = missionManager;
            this.logDebug = logDebug;
        }

        /// <summary>
        /// Shrine Config에 등록된 Unlock Mission들을 활성화한다.
        /// </summary>
        public void RegisterUnlockMissions()
        {
            if (config == null)
            {
                return;
            }

            if (missionManager == null)
            {
                return;
            }

            IReadOnlyList<ShrineGodSO> gods =
                config.Gods;

            if (gods == null)
            {
                return;
            }

            for (int i = 0; i < gods.Count; i++)
            {
                ShrineGodSO god =
                    gods[i];

                if (god == null)
                {
                    continue;
                }

                IReadOnlyList<MissionSO> unlockMissions =
                    god.UnlockMissions;

                if (unlockMissions == null
                    || unlockMissions.Count <= 0)
                {
                    continue;
                }

                foreach (MissionSO mission in unlockMissions)
                {
                    if (mission == null)
                    {
                        continue;
                    }

                    missionManager.ActivateMission(
                        mission);

                    if (logDebug)
                    {
                        Debug.Log(
                            $"[ShrineMissionService] Unlock mission registered. god={god.GodType}, mission={mission.DisplayName}");
                    }
                }
            }
        }

        /// <summary>
        /// 특정 신의 Faith Mission을 활성화한다.
        /// </summary>
        public void ActivateFaithMission(
            ShrineGodSO god,
            int faithLevel)
        {
            if (god == null)
            {
                return;
            }

            IReadOnlyList<MissionSO> faithMissions =
                god.FaithMissions;

            if (faithMissions == null
                || faithMissions.Count <= 0)
            {
                return;
            }

            if (faithLevel < 5)
            {
                if (logDebug)
                {
                    Debug.Log(
                        $"[ShrineMissionService] Faith mission skipped. god={god.GodType}, level={faithLevel}");
                }

                return;
            }

            if (missionManager == null)
            {
                return;
            }

            foreach (MissionSO mission in faithMissions)
            {
                if (mission == null)
                {
                    continue;
                }

                missionManager.ActivateMission(
                    mission);

                if (logDebug)
                {
                    Debug.Log(
                        $"[ShrineMissionService] Faith mission activated. god={god.GodType}, level={faithLevel}, mission={mission.DisplayName}");
                }
            }
        }
    }
}