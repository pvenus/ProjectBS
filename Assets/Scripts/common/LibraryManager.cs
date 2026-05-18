

using Common.SO;
using UnityEngine;

namespace Common
{
    public class LibraryManager : MonoBehaviour
    {
        public static LibraryManager Instance { get; private set; }

        [Header("Reward")]
        [SerializeField]
        private RewardVisualLibrarySO rewardVisualLibrary;

        [Header("Debug")]
        [SerializeField]
        private bool logDebug;

        public RewardVisualLibrarySO RewardVisualLibrary =>
            rewardVisualLibrary;

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

        public RewardVisualSO GetRewardVisual(
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