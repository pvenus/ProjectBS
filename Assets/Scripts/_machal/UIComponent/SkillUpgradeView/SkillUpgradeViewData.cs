using System;
using System.Collections.Generic;
using Presentation;
using UnityEngine;

namespace UIFramework.Data
{
    /// <summary>
    /// SkillUpgradeView 에 주입되는 최상위 데이터 (UI 전용).
    /// </summary>
    [Serializable]
    public class SkillUpgradeViewData
    {
        public string title;
        public List<SkillUpgradeOptionData> options = new();
    }

    /// <summary>
    /// UISkillUpgradeOptionCard(또는 UISkillUpgradeButton)에 주입되는 개별 옵션 데이터 (UI 전용).
    /// </summary>
    [Serializable]
    public class SkillUpgradeOptionData
    {
        public Sprite characterPortrait;
        public string characterName;
        public int currentLevel;
        public int nextLevel;
        public string statComparisonText;
        public ContentPresentationData content;
    }
}
