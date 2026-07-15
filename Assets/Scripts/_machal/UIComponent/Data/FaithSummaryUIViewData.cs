using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIFramework.Data
{
    [Serializable]
    public class FaithSummaryUIViewData
    {
        public string title;
        public List<FaithSummaryItemViewData> items = new List<FaithSummaryItemViewData>();
    }

    [Serializable]
    public class FaithSummaryItemViewData
    {
        public string faithId;
        public string displayName;
        public Sprite icon;
        public int totalReputation;
        public int currentLevel;
        public int nextLevel;
        public int currentLevelReputation;
        public int nextLevelRequiredReputation;
        public float progress01;
        public bool isMaxLevel;
        public string tooltip;
    }
}
