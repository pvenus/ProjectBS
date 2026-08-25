using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIFramework.Test
{
    [Serializable]
    public class FaithDetailMockData
    {
        public List<FaithLevelThresholdData> levelThresholds = new List<FaithLevelThresholdData>();
        public List<FaithDetailEntryMockData> faithEntries = new List<FaithDetailEntryMockData>();
    }

    [Serializable]
    public class FaithLevelThresholdData
    {
        public int level;
        public int requiredTotalReputation;
    }

    [Serializable]
    public class FaithDetailEntryMockData
    {
        public string faithId;
        public string displayName;
        public Sprite icon;
        public int totalReputation;
        public List<FaithNodeRewardMockData> nodes = new List<FaithNodeRewardMockData>();
    }

    [Serializable]
    public class FaithNodeRewardMockData
    {
        public string nodeId;
        public int requiredLevel;
        public Sprite activeIcon;
        public Sprite inactiveIcon;
        public string title;
        [TextArea] public string description;
    }
}
