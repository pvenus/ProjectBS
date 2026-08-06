using System.Collections.Generic;
using UnityEngine;

namespace Shrine
{
    /// <summary>
    /// 신전 시스템 전체 설정 데이터.
    /// 회복량, 축복 후보 개수, 기도/기부 수치, 기부 비용, 기본 신 목록 등을 정의한다.
    /// </summary>
    [CreateAssetMenu(menuName = "Shrine/Shrine Config")]
    public class ShrineConfigSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField]
        private string configId = "default_shrine_config";
        [SerializeField]
        private string configName = "Default Shrine Config";

        [Header("Heal And Bless")]
        [Tooltip("회복과 축복 선택 시 파티 최대 체력 대비 회복 비율")]
        [SerializeField, Range(0f, 1f)]
        private float partyHealRatio = 0.4f;

        [Tooltip("회복과 축복 선택 시 표시할 축복 후보 개수")]
        [SerializeField, Min(1)]
        private int blessingCandidateCount = 3;

        [Tooltip("동일 축복 후보 중복 허용 여부")]
        [SerializeField]
        private bool allowDuplicateBlessingCandidates = false;

        [Header("Faith")]
        [Tooltip("기도하기 선택 시 증가하는 신앙심")]
        [SerializeField, Min(0)]
        private int prayFaithGain = 1;

        [Tooltip("기부하기 선택 시 증가하는 신앙심")]
        [SerializeField, Min(0)]
        private int donateFaithGain = 2;

        [Header("Faith Level")]
        [Tooltip("레벨 별 필요 경험치")]
        [SerializeField]
        private List<int> faithLevelRequiredExp = new()
        {
            100,
            125,
            150,
            200,
            250,
            325,
            400,
            500,
            650,
            800
        };

        [Tooltip("기본적으로 선택 가능한 신 목록")]
        [SerializeField]
        private List<ShrineGodType> defaultAvailableGods = new()
        {
            ShrineGodType.Life,
            ShrineGodType.War
        };

        [Tooltip("신 데이터 목록")]
        [SerializeField]
        private List<ShrineGodSO> gods = new();

        [Header("Donation Cost")]
        [SerializeField]
        private List<ShrineDonationCostRule> donationCostRules = new()
        {
            new ShrineDonationCostRule(1, 4, 100),
            new ShrineDonationCostRule(5, 6, 200),
            new ShrineDonationCostRule(7, 8, 350),
            new ShrineDonationCostRule(9, 9, 500)
        };

        [Header("Faith Stage Thresholds")]
        [SerializeField, Range(1, 10)]
        private int influenceLevel = 3;

        [SerializeField, Range(1, 10)]
        private int faithLockLevel = 5;

        [SerializeField, Range(1, 10)]
        private int devotedLevel = 7;

        [SerializeField, Range(1, 10)]
        private int favorLevel = 9;

        [SerializeField, Range(1, 10)]
        private int successorLevel = 10;

        [Header("Debug")]
        [SerializeField]
        private bool useFixedSeed = false;
        [SerializeField]
        private int seed = 0;

        public string ConfigId => configId;
        public string ConfigName => configName;
        public float PartyHealRatio => partyHealRatio;
        public int BlessingCandidateCount => Mathf.Max(1, blessingCandidateCount);
        public bool AllowDuplicateBlessingCandidates => allowDuplicateBlessingCandidates;
        public int PrayFaithGain => Mathf.Max(0, prayFaithGain);
        public int DonateFaithGain => Mathf.Max(0, donateFaithGain);
        public IReadOnlyList<int> FaithLevelRequiredExp => faithLevelRequiredExp;
        public IReadOnlyList<ShrineGodType> DefaultAvailableGods => defaultAvailableGods;
        public IReadOnlyList<ShrineGodSO> Gods => gods;
        public IReadOnlyList<ShrineDonationCostRule> DonationCostRules => donationCostRules;
        public int InfluenceLevel => influenceLevel;
        public int FaithLockLevel => faithLockLevel;
        public int DevotedLevel => devotedLevel;
        public int FavorLevel => favorLevel;
        public int SuccessorLevel => successorLevel;
        public bool UseFixedSeed => useFixedSeed;
        public int Seed => seed;

        public int GetRequiredFaithExp(int level)
        {
            if (faithLevelRequiredExp == null
                || faithLevelRequiredExp.Count <= 0)
            {
                return 0;
            }

            int normalizedLevel =
                Mathf.Clamp(
                    level,
                    1,
                    faithLevelRequiredExp.Count);

            return Mathf.Max(
                0,
                faithLevelRequiredExp[normalizedLevel - 1]);
        }

        public int CalculateFaithLevel(int affinity)
        {
            if (affinity <= 0)
            {
                return 0;
            }

            if (faithLevelRequiredExp == null
                || faithLevelRequiredExp.Count <= 0)
            {
                return 0;
            }

            int level = 0;
            int remainingExp = affinity;

            while (level < faithLevelRequiredExp.Count)
            {
                int requiredExp =
                    GetRequiredFaithExp(level + 1);

                if (remainingExp < requiredExp)
                {
                    break;
                }

                remainingExp -= requiredExp;
                level++;
            }

            return Mathf.Clamp(
                level,
                0,
                faithLevelRequiredExp.Count);
        }

        public int GetDonationCost(int currentFaithLevel)
        {
            int normalizedLevel = Mathf.Clamp(currentFaithLevel, 1, 10);

            foreach (ShrineDonationCostRule rule in DonationCostRules)
            {
                if (rule == null)
                {
                    continue;
                }

                if (rule.Contains(normalizedLevel))
                {
                    return rule.Cost;
                }
            }

            return 0;
        }

        public bool IsDefaultGodAvailable(ShrineGodType godType)
        {
            return godType != ShrineGodType.None && defaultAvailableGods.Contains(godType);
        }

        public List<ShrineGodType> GetDefaultAvailableGods()
        {
            List<ShrineGodType> result = new();

            foreach (ShrineGodType godType in defaultAvailableGods)
            {
                if (godType == ShrineGodType.None)
                {
                    continue;
                }

                if (!result.Contains(godType))
                {
                    result.Add(godType);
                }
            }

            return result;
        }

        public ShrineGodSO GetGod(ShrineGodType godType)
        {
            if (godType == ShrineGodType.None)
            {
                return null;
            }

            return gods.Find(x => x != null && x.GodType == godType);
        }
    }

    [System.Serializable]
    public class ShrineDonationCostRule
    {
        [SerializeField, Range(1, 10)]
        private int minFaithLevel = 1;

        [SerializeField, Range(1, 10)]
        private int maxFaithLevel = 1;

        [SerializeField, Min(0)]
        private int cost = 100;

        public int MinFaithLevel => Mathf.Clamp(minFaithLevel, 1, 10);
        public int MaxFaithLevel => Mathf.Clamp(maxFaithLevel, MinFaithLevel, 10);
        public int Cost => Mathf.Max(0, cost);

        public ShrineDonationCostRule()
        {
        }

        public ShrineDonationCostRule(int minFaithLevel, int maxFaithLevel, int cost)
        {
            this.minFaithLevel = Mathf.Clamp(minFaithLevel, 1, 10);
            this.maxFaithLevel = Mathf.Clamp(maxFaithLevel, this.minFaithLevel, 10);
            this.cost = Mathf.Max(0, cost);
        }

        public bool Contains(int faithLevel)
        {
            int normalizedLevel = Mathf.Clamp(faithLevel, 1, 10);
            return normalizedLevel >= MinFaithLevel && normalizedLevel <= MaxFaithLevel;
        }
    }
}