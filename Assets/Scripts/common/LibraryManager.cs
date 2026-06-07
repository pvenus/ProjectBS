using Common.SO;
using Stage;
using UnityEngine;

namespace Common
{
    public class LibraryManager : MonoBehaviour
    {
        public static LibraryManager Instance { get; private set; }

        [Header("Reward")]
        [SerializeField]
        private RewardVisualLibrarySO rewardVisualLibrary;

        [Header("Stage Node")]
        [SerializeField]
        private NodeTypeIconLibrarySO nodeTypeIconLibrary;

        [Header("Debug")]
        [SerializeField]
        private bool logDebug;

        public RewardVisualLibrarySO RewardVisualLibrary =>
            rewardVisualLibrary;

        public Sprite GetNodeTypeIcon(RoundNodeType nodeType)
        {
            if (nodeTypeIconLibrary == null)
            {
                return null;
            }

            Sprite icon = nodeTypeIconLibrary.GetIcon(nodeType);

            if (icon == null && logDebug)
            {
                Debug.LogWarning(
                    $"[LibraryManager] NodeType icon not found. nodeType={nodeType}");
            }

            return icon;
        }

        private void Awake()
        {
            if (Instance != null
                && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (logDebug)
            {
                Debug.Log("[LibraryManager] Initialized.");
            }
        }

        public RewardVisualLibrarySO.RewardVisualEntry GetRewardVisual(
            Stage.PopupEventRewardType rewardType)
        {
            if (rewardVisualLibrary == null)
            {
                Debug.LogWarning(
                    "[LibraryManager] RewardVisualLibrary is null.");

                return null;
            }

            return rewardVisualLibrary.GetVisual(rewardType);
        }
    }
}