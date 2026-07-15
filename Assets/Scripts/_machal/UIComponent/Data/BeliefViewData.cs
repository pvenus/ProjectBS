using System.Collections.Generic;
using UnityEngine;

namespace UIFramework.Data
{
    public class BeliefItemViewData
    {
        public string godId;
        public string godName;
        public Sprite godIcon;
        public int currentLevel;
        public int currentExp;
        public int maxExpForNextLevel; // 레벨업에 필요한 경험치, 혹은 다음 레벨 경험치
    }

    public class BeliefListViewData
    {
        public List<BeliefItemViewData> beliefs = new List<BeliefItemViewData>();
    }
}
