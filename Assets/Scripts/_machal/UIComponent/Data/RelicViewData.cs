using System;
using System.Collections.Generic;
using UnityEngine;
using Item;

namespace UIFramework.Data
{
    [Serializable]
    public class RelicListViewData
    {
        public List<RelicItemViewData> godRelics = new List<RelicItemViewData>();
        public List<RelicItemViewData> commonRelics = new List<RelicItemViewData>();
    }

    [Serializable]
    public class RelicItemViewData
    {
        public string id;
        public string name;
        public string description;
        public Sprite icon;
        public RelicType type;
        public RelicRarity rarity;
        public bool isNew;
        public bool isLocked;
        public int count;
    }

    public enum RelicType
    {
        Common,
        God
    }
}
