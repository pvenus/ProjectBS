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
        public string configId = "default_shrine_config";
        public string configName = "Default Shrine Config";

        [Header("Heal And Bless")]
        [Tooltip("회복과 축복 선택 시 파티 최대 체력 대비 회복 비율")]
        [Range(0f, 1f)]
        public float partyHealRatio = 0.4f;

        [Tooltip("회복과 축복 선택 시 표시할 축복 후보 개수")]
        [Min(1)]
        public int blessingCandidateCount = 3;


        [Tooltip("동일 축복 후보 중복 허용 여부")]
        public bool allowDuplicateBlessingCandidates = false;

        [Header("Faith")]
        [Tooltip("기도하기 선택 시 증가하는 신앙심")]
        [Min(0)]
        public int prayFaithGain = 1;

        [Tooltip("기부하기 선택 시 증가하는 신앙심")]
        [Min(0)]
        public int donateFaithGain = 2;

        [Tooltip("기본적으로 선택 가능한 신 목록")]
        public List<ShrineGodType> defaultAvailableGods = new()
        {
            ShrineGodType.Life,
            ShrineGodType.War
        };

        [Tooltip("신 데이터 목록")]
        public List<ShrineGodSO> gods = new();

        [Header("Donation Cost")]
        public List<ShrineDonationCostRule> donationCostRules = new()
        {
            new ShrineDonationCostRule(1, 4, 100),
            new ShrineDonationCostRule(5, 6, 200),
            new ShrineDonationCostRule(7, 8, 350),
            new ShrineDonationCostRule(9, 9, 500)
        };

        [Header("Faith Stage Thresholds")]
        [Range(1, 10)]
        public int influenceLevel = 3;

        [Range(1, 10)]
        public int faithLockLevel = 5;

        [Range(1, 10)]
        public int devotedLevel = 7;

        [Range(1, 10)]
        public int favorLevel = 9;

        [Range(1, 10)]
        public int successorLevel = 10;

        [Header("Debug")]
        public bool useFixedSeed = false;
        public int seed = 0;

        public int GetDonationCost(int currentFaithLevel)
        {
            int normalizedLevel = Mathf.Clamp(currentFaithLevel, 1, 10);

            foreach (ShrineDonationCostRule rule in donationCostRules)
            {
                if (rule == null)
                {
                    continue;
                }

                if (rule.Contains(normalizedLevel))
                {
                    return Mathf.Max(0, rule.cost);
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

            return gods.Find(x => x != null && x.godType == godType);
        }
    }

    [System.Serializable]
    public class ShrineDonationCostRule
    {
        [Range(1, 10)]
        public int minFaithLevel = 1;

        [Range(1, 10)]
        public int maxFaithLevel = 1;

        [Min(0)]
        public int cost = 100;

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
            return normalizedLevel >= minFaithLevel && normalizedLevel <= maxFaithLevel;
        }
    }
}