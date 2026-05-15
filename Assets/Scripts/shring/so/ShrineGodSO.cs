using System.Collections.Generic;
using UnityEngine;
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

        [Tooltip("이 신의 전용 축복 풀")]
        public List<ShrineBlessingSO> exclusiveBlessings = new();

        [Header("Enhanced Blessings")]
        [Tooltip("강화 블레스 전용 풀 (5 / 7 단계 전용)")]
        public List<ShrineBlessingSO> enhancedBlessings = new();

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

        public bool HasExclusiveBlessings => exclusiveBlessings != null && exclusiveBlessings.Count > 0;

        public bool HasEnhancedBlessings => enhancedBlessings != null && enhancedBlessings.Count > 0;

        public List<ShrineBlessingSO> GetAvailableBlessings(
            int faithLevel,
            ShrineBlessingGroup blessingGroup = ShrineBlessingGroup.Base)
        {
            List<ShrineBlessingSO> result = new();

            List<ShrineBlessingSO> source =
                blessingGroup == ShrineBlessingGroup.Enhanced
                    ? enhancedBlessings
                    : exclusiveBlessings;

            if (source == null)
            {
                return result;
            }

            foreach (ShrineBlessingSO blessing in source)
            {
                if (blessing == null)
                {
                    continue;
                }

                if (blessing.progressionStep != faithLevel)
                {
                    continue;
                }

                result.Add(blessing);
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