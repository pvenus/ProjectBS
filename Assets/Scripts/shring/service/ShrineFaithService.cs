using UnityEngine;
using System.Collections.Generic;

namespace Shrine
{
    /// <summary>
    /// Shrine Faith 관련 처리 전담 서비스.
    /// Faith 증가 / Lock / Reward / Mission 연결 등을 담당한다.
    /// </summary>
    public class ShrineFaithService
    {
        private readonly ShrineManager shrineManager;

        private readonly ShrinePlayerRuntimeData playerRuntimeData;

        private readonly ShrineConfigSO config;

        private readonly ShrineRewardService rewardService;

        private readonly ShrineMissionService missionService;

        private readonly bool logDebug;

        public ShrineFaithService(
            ShrineManager shrineManager,
            ShrinePlayerRuntimeData playerRuntimeData,
            ShrineConfigSO config,
            ShrineRewardService rewardService,
            ShrineMissionService missionService,
            bool logDebug)
        {
            this.shrineManager = shrineManager;
            this.playerRuntimeData = playerRuntimeData;
            this.config = config;
            this.rewardService = rewardService;
            this.missionService = missionService;
            this.logDebug = logDebug;
        }

        public int Pray(
            ShrineGodType godType)
        {
            return AddFaith(
                godType,
                config != null
                    ? config.prayFaithGain
                    : 1);
        }

        public int Donate(
            ShrineGodType godType)
        {
            return AddFaith(
                godType,
                config != null
                    ? config.donateFaithGain
                    : 2);
        }

        public int AddFaith(
            ShrineGodType godType,
            int amount)
        {
            if (playerRuntimeData == null)
            {
                return 0;
            }

            if (godType == ShrineGodType.None)
            {
                return 0;
            }

            ShrineGodSO god =
                shrineManager.GetGodSO(godType);

            if (god == null)
            {
                return 0;
            }

            bool appliedInitialFaith =
                EnsureInitialFaith(god);

            if (appliedInitialFaith && logDebug)
            {
                Debug.Log(
                    $"[ShrineFaithService] Initial faith applied. god={godType}");
            }

            int previousLevel =
                playerRuntimeData.GetFaithLevel(godType);

            bool becameLocked =
                playerRuntimeData.AddFaith(
                    godType,
                    amount);

            int level =
                playerRuntimeData.GetFaithLevel(godType);

            GiveFaithLevelRewards(
                god,
                previousLevel,
                level);

            if (becameLocked)
            {
                HandleFaithLock(god);
            }

            if (logDebug)
            {
                Debug.Log(
                    $"[ShrineFaithService] Faith added. god={godType}, amount={amount}, level={level}");
            }

            return level;
        }

        private bool EnsureInitialFaith(
            ShrineGodSO god)
        {
            if (god == null)
            {
                return false;
            }

            int currentLevel =
                playerRuntimeData.GetFaithLevel(
                    god.godType);

            if (currentLevel > 0)
            {
                return false;
            }

            if (god.initialFaithLevel <= 0)
            {
                return false;
            }

            playerRuntimeData.SetFaithLevel(
                god.godType,
                god.initialFaithLevel);

            return true;
        }

        private void HandleFaithLock(
            ShrineGodSO lockedGod)
        {
            if (lockedGod == null)
            {
                return;
            }

            if (logDebug)
            {
                Debug.Log(
                    $"[ShrineFaithService] Faith locked. god={lockedGod.godType}");
            }

            int currentFaithLevel =
                playerRuntimeData.GetFaithLevel(
                    lockedGod.godType);

            missionService?.ActivateFaithMission(
                lockedGod,
                currentFaithLevel);

            RemoveOtherFaithRelics(
                lockedGod.godType);
        }

        private void RemoveOtherFaithRelics(
            ShrineGodType lockedGodType)
        {
            if (config == null)
            {
                return;
            }

            if (rewardService == null)
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

                if (god.godType == lockedGodType)
                {
                    continue;
                }

                if (god.faithRelicRewards == null)
                {
                    continue;
                }

                for (int j = 0; j < god.faithRelicRewards.Count; j++)
                {
                    ShrineFaithRelicReward reward =
                        god.faithRelicRewards[j];

                    if (reward == null)
                    {
                        continue;
                    }

                    if (reward.relicReward == null)
                    {
                        continue;
                    }

                    rewardService.RemoveFaithRelics(god);
                    break;
                }
            }
        }

        private void GiveFaithLevelRewards(
            ShrineGodSO god,
            int previousLevel,
            int currentLevel)
        {
            if (god == null)
            {
                return;
            }

            if (rewardService == null)
            {
                return;
            }

            if (currentLevel <= previousLevel)
            {
                return;
            }

            for (int level = previousLevel + 1;
                 level <= currentLevel;
                 level++)
            {
                rewardService.GiveFaithRelicReward(
                    god,
                    level);

                GiveEnhancedBlessings(
                    god,
                    level);
            }
        }

        private void GiveEnhancedBlessings(
            ShrineGodSO god,
            int faithLevel)
        {
            if (god == null)
            {
                return;
            }

            if (playerRuntimeData == null)
            {
                return;
            }

            List<ShrineBlessingSO> blessings =
                god.GetAvailableBlessings(
                    faithLevel,
                    ShrineBlessingGroup.Enhanced);

            if (blessings == null
                || blessings.Count <= 0)
            {
                return;
            }

            for (int i = 0; i < blessings.Count; i++)
            {
                ShrineBlessingSO blessing =
                    blessings[i];

                if (blessing == null)
                {
                    continue;
                }

                playerRuntimeData.AddEnhancedBlessing(
                    blessing);

                if (logDebug)
                {
                    Debug.Log(
                        $"[ShrineFaithService] Enhanced blessing granted. god={god.godType}, blessing={blessing.name}, level={faithLevel}");
                }
            }
        }
    }
}