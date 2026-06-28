using System.Collections.Generic;
using UnityEngine;
using Bless;
using Item;
using Mission;
using String;

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
        [SerializeField] private string godId;
        [SerializeField] private ShrineGodType godType = ShrineGodType.None;

        [Header("Display")]
        [SerializeField] private Sprite icon;
        [SerializeField] private Color themeColor = Color.white;

        [Header("Faith")]
        [Tooltip("신 선택 시 시작 신앙 레벨")]
        [Range(0, 10)]
        [SerializeField] private int initialFaithLevel = 1;

        [Tooltip("신앙 고정 레벨")]
        [Range(1, 10)]
        [SerializeField] private int lockFaithLevel = 5;

        [Range(1, 10)]
        [SerializeField] private int successorFaithLevel = 10;

        [Header("Blessing Pools")]
        [Tooltip("신 전용 축복 풀")]
        [SerializeField] private List<BlessPoolSO> blessingPools = new();

        [Header("Faith Rewards")]
        [Tooltip("특정 신앙 레벨 도달 시 지급되는 유물")]
        [SerializeField] private List<ShrineFaithRelicReward> faithRelicRewards = new();

        [Header("Mission")]
        [Tooltip("이 신을 해금하는 공용 미션 목록")]
        [SerializeField] private List<MissionSO> unlockMissions = new();

        [Tooltip("Faith Lock 이후 활성화 되는 공용 미션 목록")]
        [SerializeField] private List<MissionSO> faithMissions = new();

        public string LocalizationMainKey => godId;

        public string DisplayName =>
            StringManager.Instance.Get(
                LocalizationMainKey,
                "name");

        public string Description =>
            StringManager.Instance.Get(
                LocalizationMainKey,
                "desc");

        public string GodId => godId;
        public ShrineGodType GodType => godType;
        public Sprite Icon => icon;
        public Color ThemeColor => themeColor;
        public int InitialFaithLevel => initialFaithLevel;
        public int LockFaithLevel => lockFaithLevel;
        public int SuccessorFaithLevel => successorFaithLevel;
        public IReadOnlyList<BlessPoolSO> BlessingPools => blessingPools;
        public IReadOnlyList<ShrineFaithRelicReward> FaithRelicRewards => faithRelicRewards;
        public IReadOnlyList<MissionSO> UnlockMissions => unlockMissions;
        public IReadOnlyList<MissionSO> FaithMissions => faithMissions;

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
                    || pool.Blessings == null)
                {
                    continue;
                }

                foreach (BlessPoolSO.BlessPoolEntry entry in pool.Blessings)
                {
                    if (entry == null
                        || entry.Blessing == null)
                    {
                        continue;
                    }

                    if (entry.ProgressionStep != faithLevel)
                    {
                        continue;
                    }

                    if (result.Contains(entry.Blessing))
                    {
                        continue;
                    }

                    result.Add(entry.Blessing);
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

                if (reward.FaithLevel != faithLevel)
                {
                    continue;
                }

                return reward.RelicReward;
            }

            return null;
        }
    }

    [System.Serializable]
    public class ShrineFaithRelicReward
    {
        [SerializeField] private int faithLevel = 1;

        [SerializeField] private RelicSO relicReward;

        public int FaithLevel => faithLevel;
        public RelicSO RelicReward => relicReward;
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