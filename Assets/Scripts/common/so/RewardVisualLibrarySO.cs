using System;
using System.Collections.Generic;
using UnityEngine;

namespace Common.SO
{
    [CreateAssetMenu(
        fileName = "RewardVisualLibrary",
        menuName = "BS/Common/Reward Visual Library")]
    public class RewardVisualLibrarySO : ScriptableObject
    {
        [Serializable]
        public class RewardVisualEntry
        {
            public Stage.PopupEventRewardType rewardType;
            public string localizationMainKey;
            public Sprite icon;
        }

        [SerializeField]
        private List<RewardVisualEntry> visuals = new();

        public IReadOnlyList<RewardVisualEntry> Visuals => visuals;

        public RewardVisualEntry GetVisual(
            Stage.PopupEventRewardType rewardType)
        {
            return visuals.Find(x => x != null
                && x.rewardType == rewardType);
        }
    }
}