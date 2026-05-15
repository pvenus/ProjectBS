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

            if (config.gods == null)
            {
                return;
            }

            for (int i = 0; i < config.gods.Count; i++)
            {
                ShrineGodSO god =
                    config.gods[i];

                if (god == null)
                {
                    continue;
                }

                if (god.unlockMissions == null
                    || god.unlockMissions.Count <= 0)
                {
                    continue;
                }

                foreach (MissionSO mission in god.unlockMissions)
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
                            $"[ShrineMissionService] Unlock mission registered. god={god.godType}, mission={mission.displayName}");
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

            if (god.faithMissions == null
                || god.faithMissions.Count <= 0)
            {
                return;
            }

            if (faithLevel < 5)
            {
                if (logDebug)
                {
                    Debug.Log(
                        $"[ShrineMissionService] Faith mission skipped. god={god.godType}, level={faithLevel}");
                }

                return;
            }

            if (missionManager == null)
            {
                return;
            }

            foreach (MissionSO mission in god.faithMissions)
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
                        $"[ShrineMissionService] Faith mission activated. god={god.godType}, level={faithLevel}, mission={mission.displayName}");
                }
            }
        }
    }
}