using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIFramework.Test
{
    [Serializable]
    public class FaithSummaryMockData
    {
        // Reusing FaithLevelThresholdData from FaithDetailMockData
        public List<FaithLevelThresholdData> levelThresholds = new List<FaithLevelThresholdData>();
        public List<FaithSummaryEntryMockData> faithEntries = new List<FaithSummaryEntryMockData>();
    }

    [Serializable]
    public class FaithSummaryEntryMockData
    {
        public string faithId;
        public string displayName;
        public Sprite icon;
        public int totalReputation;
        [TextArea] public string tooltip;
    }
}
