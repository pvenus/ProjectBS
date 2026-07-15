using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIFramework.Data
{
    [Serializable]
    public class FaithDetailUIViewData
    {
        public List<FaithColumnViewData> faithColumns = new List<FaithColumnViewData>();
    }

    [Serializable]
    public class FaithColumnViewData
    {
        public string faithId;
        public string displayName;
        public Sprite icon;
        public int totalReputation;
        public int currentLevel;
        public int currentLevelReputation;
        public int nextLevelRequiredReputation;
        public float levelProgress01; // 1~10레벨 10칸 게이지로 사용될 수 있음
        public List<FaithNodeViewData> nodes = new List<FaithNodeViewData>(); // 1~10레벨 노드 데이터 리스트
        
        // 현재 적용 중인 분할 효과 리스트
        public List<FaithEffectItemData> currentEffects = new List<FaithEffectItemData>();
        // 다음 레벨에서 해금/강화되는 분할 효과 리스트
        public List<FaithEffectItemData> nextEffects = new List<FaithEffectItemData>();
    }

    [Serializable]
    public class FaithNodeViewData
    {
        public string nodeId;
        public int requiredLevel;
        public bool isUnlocked;
        public Sprite icon;
        public string title;
        public string description;
        public string status; // "Cleared", "Current", "Future", "LockedOut"
    }

    [Serializable]
    public class FaithEffectItemData
    {
        public Sprite icon;
        public string title;       // 예: "기본 축복", "유물", "직업 진화" 등
        public string description; // 효과 상세 설명 및 수치 변화
        public string type;        // "Blessing", "Relic", "Evolution"
    }
}
