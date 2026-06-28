using UnityEngine;
using System.Collections.Generic;
using Bless;
using Stat;

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
        private readonly StatManager statManager;

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
            statManager = StatManager.Instance;
        }

        public int Pray(
            ShrineGodType godType)
        {
            return AddFaith(
                godType,
                config != null
                    ? config.PrayFaithGain
                    : 1);
        }

        public int Donate(
            ShrineGodType godType)
        {
            return AddFaith(
                godType,
                config != null
                    ? config.DonateFaithGain
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
                GetFaithLevel(godType);

            AddFaithAffinity(
                godType,
                amount);

            int level =
                GetFaithLevel(godType);

            bool becameLocked =
                TryLockFaith(
                    godType,
                    level);

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
                GetFaithLevel(
                    god.GodType);

            if (currentLevel > 0)
            {
                return false;
            }

            if (god.InitialFaithLevel <= 0)
            {
                return false;
            }

            SetFaithLevel(
                god.GodType,
                god.InitialFaithLevel);

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
                    $"[ShrineFaithService] Faith locked. god={lockedGod.GodType}");
            }

            int currentFaithLevel =
                GetFaithLevel(
                    lockedGod.GodType);

            missionService?.ActivateFaithMission(
                lockedGod,
                currentFaithLevel);

            RemoveOtherGodBlessings(
                lockedGod.GodType);

            RemoveOtherFaithRelics(
                lockedGod.GodType);


        }

        private void RemoveOtherGodBlessings(
            ShrineGodType lockedGodType)
        {
            if (BlessManager.Instance == null)
            {
                return;
            }

            if (config == null
                || config.Gods == null)
            {
                return;
            }

            IReadOnlyList<ShrineGodSO> gods =
                config.Gods;

            List<ShrineGodSO> targetGods = new();

            for (int i = 0; i < gods.Count; i++)
            {
                ShrineGodSO god =
                    gods[i];

                if (god == null)
                {
                    continue;
                }

                if (god.GodType == lockedGodType)
                {
                    continue;
                }

                targetGods.Add(god);
            }

            for (int i = 0; i < targetGods.Count; i++)
            {
                ShrineGodSO targetGod =
                    targetGods[i];

                RemoveBlessingGroups(
                    targetGod,
                    ShrineBlessingGroup.Base);

                RemoveBlessingGroups(
                    targetGod,
                    ShrineBlessingGroup.Enhanced);
            }
        }

        private void RemoveBlessingGroups(
            ShrineGodSO god,
            ShrineBlessingGroup group)
        {
            if (god == null
                || BlessManager.Instance == null)
            {
                return;
            }

            List<BlessSO> blessings =
                new();

            if (god.BlessingPools != null)
            {
                for (int i = 0; i < god.BlessingPools.Count; i++)
                {
                    BlessPoolSO pool =
                        god.BlessingPools[i];

                    if (pool == null
                        || pool.Blessings == null)
                    {
                        continue;
                    }

                    for (int j = 0; j < pool.Blessings.Count; j++)
                    {
                        BlessPoolSO.BlessPoolEntry entry =
                            pool.Blessings[j];

                        if (entry == null
                            || entry.Blessing == null)
                        {
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(entry.Blessing.GroupId))
                        {
                            continue;
                        }

                        if (blessings.Contains(entry.Blessing))
                        {
                            continue;
                        }

                        blessings.Add(entry.Blessing);

                        string targetGroupId =
                            entry.Blessing.GroupId;

                        BlessManager.Instance.RemoveBlesses(
                            x => x != null
                                 && x.source != null
                                 && x.source.GroupId == targetGroupId);
                    }
                }
            }
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

            if (config.Gods == null)
            {
                return;
            }

            IReadOnlyList<ShrineGodSO> gods =
                config.Gods;

            for (int i = 0; i < gods.Count; i++)
            {
                ShrineGodSO god =
                    gods[i];

                if (god == null)
                {
                    continue;
                }

                if (god.GodType == lockedGodType)
                {
                    continue;
                }

                if (god.FaithRelicRewards == null)
                {
                    continue;
                }

                for (int j = 0; j < god.FaithRelicRewards.Count; j++)
                {
                    ShrineFaithRelicReward reward =
                        god.FaithRelicRewards[j];

                    if (reward == null)
                    {
                        continue;
                    }

                    if (reward.RelicReward == null)
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

            if (BlessManager.Instance == null)
            {
                return;
            }

            List<BlessSO> blessings =
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
                BlessSO blessing =
                    blessings[i];

                if (blessing == null)
                {
                    continue;
                }

                BlessManager.Instance.AddBless(
                    blessing,
                    $"faith_{god.GodType}_{faithLevel}");

                if (logDebug)
                {
                    Debug.Log(
                        $"[ShrineFaithService] Enhanced blessing granted. god={god.GodType}, blessing={blessing.BlessingId}, level={faithLevel}");
                }
            }
        }
        public int GetFaithLevel(
            ShrineGodType godType)
        {
            if (statManager == null)
            {
                return 0;
            }

            return Mathf.RoundToInt(
                statManager.GetStat(
                    ConvertFaithLevelStat(godType)));
        }

        public int GetFaithAffinity(
            ShrineGodType godType)
        {
            if (statManager == null)
            {
                return 0;
            }

            return Mathf.RoundToInt(
                statManager.GetStat(
                    ConvertAffinityStat(godType)));
        }

        public void SetFaithLevel(
            ShrineGodType godType,
            int level)
        {
            if (statManager == null)
            {
                return;
            }

            statManager.SetStat(
                ConvertFaithLevelStat(godType),
                level);

        }

        private void AddFaithAffinity(
            ShrineGodType godType,
            int amount)
        {
            if (statManager == null)
            {
                return;
            }

            StatType affinityStat =
                ConvertAffinityStat(godType);

            float currentValue =
                statManager.GetStat(affinityStat);

            float nextValue =
                currentValue + amount;

            statManager.SetStat(
                affinityStat,
                nextValue);

            int nextLevel =
                config != null
                    ? config.CalculateFaithLevel(
                        Mathf.RoundToInt(nextValue))
                    : Mathf.FloorToInt(nextValue / 100f);

            SetFaithLevel(
                godType,
                nextLevel);
        }

        private StatType ConvertFaithLevelStat(
            ShrineGodType godType)
        {
            return godType switch
            {
                ShrineGodType.Life => StatType.LifeFaithLevel,
                ShrineGodType.War => StatType.WarFaithLevel,
                ShrineGodType.Greed => StatType.GreedFaithLevel,
                ShrineGodType.Dark => StatType.DarkFaithLevel,
                _ => StatType.LifeFaithLevel
            };
        }

        private StatType ConvertAffinityStat(
            ShrineGodType godType)
        {
            return godType switch
            {
                ShrineGodType.Life => StatType.LifeAffinity,
                ShrineGodType.War => StatType.WarAffinity,
                ShrineGodType.Greed => StatType.GreedAffinity,
                ShrineGodType.Dark => StatType.DarkAffinity,
                _ => StatType.LifeAffinity
            };
        }

        public void AcceptFaithAscension()
        {
            if (playerRuntimeData == null)
            {
                return;
            }

            if (!playerRuntimeData.HasPendingFaithAscension)
            {
                return;
            }

            ShrineGodType godType =
                playerRuntimeData.PendingFaithGod;

            playerRuntimeData.LockFaith(godType);

            ShrineGodSO god =
                shrineManager.GetGodSO(godType);

            if (god != null)
            {
                HandleFaithLock(god);
            }
        }

        public void RejectFaithAscension()
        {
            if (playerRuntimeData == null)
            {
                return;
            }

            playerRuntimeData.ClearFaithAscensionRequest();
        }

        private bool TryLockFaith(
            ShrineGodType godType,
            int level)
        {
            if (playerRuntimeData == null)
            {
                return false;
            }

            if (playerRuntimeData.HasLockedFaith)
            {
                return false;
            }

            if (playerRuntimeData.HasPendingFaithAscension)
            {
                return false;
            }

            if (level < 5)
            {
                return false;
            }

            playerRuntimeData.RequestFaithAscension(godType);

            shrineManager?.NotifyFaithAscensionRequested(godType);

            if (logDebug)
            {
                Debug.Log(
                    $"[ShrineFaithService] Faith ascension available. god={godType}, level={level}");
            }

            return false;
        }
    }
}