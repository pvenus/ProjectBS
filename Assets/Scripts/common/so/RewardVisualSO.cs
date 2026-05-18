

using Stage;
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

        public string displayName;

        [TextArea]
        public string description;

        [Header("Visual")]
        public Sprite icon;

        public Color color = Color.white;

        [Header("Formatting")]
        public string valuePrefix = "+";

        public string valueSuffix;

        public bool hideValue;
    }
}