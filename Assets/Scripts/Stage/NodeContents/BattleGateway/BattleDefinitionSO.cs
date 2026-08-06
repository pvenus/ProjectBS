using UnityEngine;
using System;
using System.Collections.Generic;
using Shrine;

namespace Stage
{
    [CreateAssetMenu(
        fileName = "BattleDefinition",
        menuName = "BS/Stage/Battle Definition")]
    public class BattleDefinitionSO : ScriptableObject
    {
        [Serializable]
        public class BattleRewardData
        {
            public BattleRewardType rewardType;

            public int value;

            public ShrineGodType godType;

            public ScriptableObject targetData;
        }

        [Header("Info")]
        public string battleId;

        public string displayName;

        [TextArea]
        public string description;

        [Header("Visual")]
        public Sprite previewIcon;

        public Color themeColor = Color.white;

        [Header("Battle")]
        public bool isBossBattle;

        public bool lockRetreat;

        public bool autoStartBattle = true;

        [Header("Enemy")]
        public ScriptableObject enemyGroup;

        [Header("Reward")]
        public List<BattleRewardData> clearRewards = new();
    }

    public enum BattleRewardType
    {
        None = 0,

        Gold = 100,
        Exp = 200,

        Faith = 1000,

        Relic = 2000,
        Consume = 2100,
        ConsumePool = 2150,

        Blessing = 3000,
    }
}