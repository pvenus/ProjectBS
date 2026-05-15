using UnityEngine;
using Item;
using Shrine;
using System;
using System.Collections.Generic;

namespace Mission
{
    [CreateAssetMenu(
        fileName = "MissionSO",
        menuName = "Game/Mission/Mission SO")]
    public class MissionSO : ScriptableObject
    {
        [Header("Identity")]
        public string missionId;

        public string displayName;

        [TextArea]
        public string description;

        [Header("UI")]
        public Sprite icon;

        public Color themeColor = Color.white;

        [Header("Progress")]
        public List<MissionConditionData> conditions = new();

        [Header("Reward")]
        public MissionRewardType rewardType = MissionRewardType.None;

        public int rewardGold;

        public int rewardFaith;

        public RelicSO rewardRelic;

        [Tooltip("해금되는 신")]
        public ShrineGodType unlockGodType = ShrineGodType.None;

        [Tooltip("해금되는 컨텐츠 ID")]
        public string unlockContentId;

        [Header("Flags")]
        public bool hidden;

        public bool repeatable;

        public int GetTotalTargetCount()
        {
            if (conditions == null
                || conditions.Count <= 0)
            {
                return 0;
            }

            int total = 0;

            for (int i = 0; i < conditions.Count; i++)
            {
                MissionConditionData condition =
                    conditions[i];

                if (condition == null)
                {
                    continue;
                }

                total += condition.targetCount;
            }

            return total;
        }
    }

    [Serializable]
    public class MissionConditionData
    {
        public MissionProgressType progressType = MissionProgressType.Counter;

        public MissionProgressKey progressKey = MissionProgressKey.None;

        [Header("Content")]
        [Tooltip("컨텐츠 태그 기반 조건")]
        public List<string> contentTags = new();

        public MissionCompareType compareType = MissionCompareType.GreaterOrEqual;

        public int targetValue = 1;

        [Min(1)]
        public int targetCount = 1;
    }

    public enum MissionProgressType
    {
        Counter = 0,
        State = 1,
    }

    public enum MissionCompareType
    {
        None = 0,

        GreaterOrEqual = 100,
        LessOrEqual = 200,
        Equal = 300,
    }

    public enum MissionProgressKey
    {
        None = 0,

        ShopVisit = 100,
        ShopPurchase = 120,
        Donate = 150,

        BattleWin = 200,
        MonsterKill = 220,
        EliteKill = 240,
        BossKillInTimeLimit = 260,

        EventClear = 300,

        PartyAverageHpPercentAfterBattle = 400,
        AllPartyHpPercentAfterBattle = 450,

        Reputation = 500,
        GoldOwned = 550,
        RelicOwned = 560,

        BattleWinWithDeadPartyMember = 600,
    }
    public enum MissionRewardType
    {
        None = 0,

        Gold = 100,
        Faith = 200,
        Relic = 300,

        UnlockGod = 1000,
        UnlockDungeon = 1100,
        UnlockEvent = 1200,
        UnlockNpc = 1300,
        UnlockShop = 1400,
    }
}