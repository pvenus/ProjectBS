

using System.Collections.Generic;
using UnityEngine;

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
        [Range(1, 10)]
        public int lockFaithLevel = 5;

        [Range(1, 10)]
        public int successorFaithLevel = 10;

        [Tooltip("이 신의 전용 축복 풀")]
        public List<ShrineBlessingSO> exclusiveBlessings = new();

        [Header("Stage Rewards")]
        public List<ShrineFaithReward> stageRewards = new();

        [Header("Mission")]
        public ShrineMissionType missionType = ShrineMissionType.None;

        [TextArea]
        public string missionDescription;

        [Header("Flavor")]
        [TextArea]
        public string introductionText;

        [TextArea]
        public string devotionText;

        [TextArea]
        public string successorText;

        public string DisplayName => string.IsNullOrWhiteSpace(godName)
            ? name
            : godName;

        public bool HasExclusiveBlessings => exclusiveBlessings != null && exclusiveBlessings.Count > 0;

        public ShrineFaithReward GetReward(int faithLevel)
        {
            if (stageRewards == null)
            {
                return null;
            }

            foreach (ShrineFaithReward reward in stageRewards)
            {
                if (reward == null)
                {
                    continue;
                }

                if (reward.faithLevel == faithLevel)
                {
                    return reward;
                }
            }

            return null;
        }

        public bool HasReward(int faithLevel)
        {
            return GetReward(faithLevel) != null;
        }

        public List<ShrineBlessingSO> GetAvailableBlessings(int faithLevel)
        {
            List<ShrineBlessingSO> result = new();

            if (exclusiveBlessings == null)
            {
                return result;
            }

            foreach (ShrineBlessingSO blessing in exclusiveBlessings)
            {
                if (blessing == null)
                {
                    continue;
                }

                if (blessing.requiredFaithLevel > faithLevel)
                {
                    continue;
                }

                result.Add(blessing);
            }

            return result;
        }
    }

    [System.Serializable]
    public class ShrineFaithReward
    {
        [Range(1, 10)]
        public int faithLevel = 1;

        [TextArea]
        public string rewardDescription;

        [Header("Reward")]
        public ShrineRewardType rewardType = ShrineRewardType.None;

        public ShrineBlessingSO blessingReward;

        public int goldReward;

        public int healPercent;

        public string unlockId;
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