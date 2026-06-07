using Stage;
using String;
using UnityEngine;

namespace Common.SO
{
    [CreateAssetMenu(
        fileName = "RewardVisual",
        menuName = "BS/Common/Reward Visual")]
    public class RewardVisualSO : ScriptableObject
    {
        [Header("Info")]
        public PopupEventRewardType rewardType;

        public string localizationMainKey;

        public string DisplayName =>
            StringManager.Instance.Get(
                localizationMainKey,
                "title");

        public string Description =>
            StringManager.Instance.Get(
                localizationMainKey,
                "description");

        [Header("Visual")]
        public Sprite icon;
    }
}