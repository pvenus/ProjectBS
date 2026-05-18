using System.Collections.Generic;
using UnityEngine;
using Bless;
using Item;
using Mission;

namespace Shrine
{
    /// <summary>
    /// 신 정의 데이터.
    /// 신앙 단계별 보상, 전용 축복, 테마 등을 관리한다.
    /// </summary>
    [CreateAssetMenu(menuName = "Shrine/Shrine God")]
    public class ShrineGodSO : ScriptableObject
    {
        [Header("Identity")]
        public string godId;
        public string godName;
        public ShrineGodType godType = ShrineGodType.None;

        [TextArea]
        public string description;

        [Header("Display")]
        public Sprite icon;
        public Color themeColor = Color.white;

        [Header("Faith")]
        [Tooltip("신 선택 시 시작 신앙 레벨")]
        [Range(0, 10)]
        public int initialFaithLevel = 1;

        [Tooltip("신앙 고정 레벨")]
        [Range(1, 10)]
        public int lockFaithLevel = 5;

        [Range(1, 10)]
        public int successorFaithLevel = 10;

        [Header("Blessing Pools")]
        [Tooltip("신 전용 축복 풀")]
        public List<BlessPoolSO> blessingPools = new();

        [Header("Faith Rewards")]
        [Tooltip("특정 신앙 레벨 도달 시 지급되는 유물")]
        public List<ShrineFaithRelicReward> faithRelicRewards = new();

        [Header("Mission")]
        [Tooltip("이 신을 해금하는 공용 미션 목록")]
        public List<MissionSO> unlockMissions = new();

        [Tooltip("Faith Lock 이후 활성화 되는 공용 미션 목록")]
        public List<MissionSO> faithMissions = new();

        public string DisplayName => string.IsNullOrWhiteSpace(godName)
            ? name
            : godName;

        public bool HasBlessingPools => blessingPools != null && blessingPools.Count > 0;

        public bool HasEnhancedBlessings =>
            blessingPools != null
            && blessingPools.Count > 0;

        public List<BlessSO> GetAvailableBlessings(
            int faithLevel,
            ShrineBlessingGroup blessingGroup = ShrineBlessingGroup.Base)
        {
            List<BlessSO> result = new();

            if (blessingPools == null)
            {
                return result;
            }

            foreach (BlessPoolSO pool in blessingPools)
            {
                if (pool == null
                    || pool.blessings == null)
                {
                    continue;
                }

                foreach (BlessPoolSO.BlessPoolEntry entry in pool.blessings)
                {
                    if (entry == null
                        || entry.blessing == null)
                    {
                        continue;
                    }

                    if (entry.progressionStep != faithLevel)
                    {
                        continue;
                    }

                    if (result.Contains(entry.blessing))
                    {
                        continue;
                    }

                    result.Add(entry.blessing);
                }
            }

            return result;
        }

        public RelicSO GetFaithRelicReward(int faithLevel)
        {
            if (faithRelicRewards == null)
            {
                return null;
            }

            for (int i = 0; i < faithRelicRewards.Count; i++)
            {
                ShrineFaithRelicReward reward =
                    faithRelicRewards[i];

                if (reward == null)
                {
                    continue;
                }

                if (reward.faithLevel != faithLevel)
                {
                    continue;
                }

                return reward.relicReward;
            }

            return null;
        }
    }

    [System.Serializable]
    public class ShrineFaithRelicReward
    {
        [Range(1, 10)]
        public int faithLevel = 1;

        public RelicSO relicReward;
    }

    public enum ShrineBlessingGroup
    {
        Base = 0,
        Enhanced = 1
    }

    public enum ShrineRewardType
    {
        None = 0,

        Blessing,
        Gold,
        Heal,
        Unlock,
        Passive,
        Relic
    }
}