using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIFramework.Data
{
    public enum ShrineUIState
    {
        MainSelection,
        GodSelection,
        BlessingSelection,
        Result
    }

    [Serializable]
    public class ShrineUIViewData
    {
        public ShrineUIState state;
        public string title;
        public string description;
        public int currentFaith;
        public List<ShrineGodViewData> selectableGods = new List<ShrineGodViewData>();
        public List<ShrineBlessingViewData> selectableBlessings = new List<ShrineBlessingViewData>();
    }

    [Serializable]
    public class ShrineGodViewData
    {
        public string godId;
        public string displayName;
        public Sprite portrait;
        public string description;
        public int faithLevel;
        public int reputation;
        public bool selectable;
        public string disabledReason;
    }

    [Serializable]
    public class ShrineBlessingViewData
    {
        public string blessingId;
        public string displayName;
        public Sprite icon;
        public string description;
        public int cost;
        public bool selectable;
        public string disabledReason;
    }

    public enum ShrineUIResultType
    {
        SelectEnterFaith,
        SelectHealAndBless,
        SelectGod,
        SelectBlessing,
        Pray,
        Donate,
        Close
    }

    public class ShrineUIResult
    {
        public ShrineUIResultType type;
        public string selectedGodId;
        public string selectedBlessingId;
    }
}
