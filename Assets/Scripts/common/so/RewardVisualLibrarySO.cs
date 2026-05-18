

using System.Collections.Generic;
using UnityEngine;

namespace Common.SO
{
    [CreateAssetMenu(
        fileName = "RewardVisualLibrary",
        menuName = "BS/Common/Reward Visual Library")]
    public class RewardVisualLibrarySO : ScriptableObject
    {
        [SerializeField]
        private List<RewardVisualSO> visuals = new();

        public IReadOnlyList<RewardVisualSO> Visuals => visuals;

        public RewardVisualSO GetVisual(
            Stage.PopupEventRewardType rewardType)
        {
            return visuals.Find(x => x != null
                && x.rewardType == rewardType);
        }
    }
}