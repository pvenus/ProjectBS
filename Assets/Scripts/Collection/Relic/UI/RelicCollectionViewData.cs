using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIFramework.Data
{
    [Serializable]
    public class RelicCollectionViewData
    {
        public int ownedCount;
        public int totalCount;
        public List<RelicCollectionItemViewData> relics = new List<RelicCollectionItemViewData>();
    }

    [Serializable]
    public class RelicCollectionItemViewData
    {
        public string relicId;
        public string displayName;
        public Sprite icon;
        public Sprite lockedSilhouetteIcon;
        public string description;
        public bool isUnlocked;
    }

    public enum RelicCollectionResultType
    {
        SelectRelic,
        Close
    }

    public struct RelicCollectionResult
    {
        public RelicCollectionResultType type;
        public string relicId;
    }
}
