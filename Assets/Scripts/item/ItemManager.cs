

using System;
using UnityEngine;

namespace Item
{
    /// <summary>
    /// 공용 Item 시스템 매니저.
    ///
    /// 현재는 Relic(Runtime) 관리 중심으로 구성되어 있으며,
    /// 이후 Equipment / Consumable / Currency 등으로 확장 가능하다.
    /// </summary>
    public class ItemManager : MonoBehaviour
    {
        public static ItemManager Instance { get; private set; }

        [Header("Runtime Data")]
        [SerializeField]
        private RelicRuntimeData relicRuntimeData = new();

        [Header("Debug")]
        [SerializeField]
        private bool logDebug;

        public RelicRuntimeData RelicRuntimeData => relicRuntimeData;

        public event Action<RelicSO> OnRelicAdded;

        public event Action<RelicSO> OnRelicRemoved;

        private void Awake()
        {
            if (Instance != null
                && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (relicRuntimeData == null)
            {
                relicRuntimeData = new RelicRuntimeData();
            }
        }

        public bool AddRelic(RelicSO relic)
        {
            if (relic == null)
            {
                return false;
            }

            bool added =
                relicRuntimeData.AddRelic(relic);

            if (!added)
            {
                return false;
            }

            if (logDebug)
            {
                Debug.Log(
                    $"[ItemManager] Relic added. relic={relic.displayName}");
            }

            OnRelicAdded?.Invoke(relic);
            return true;
        }

        public bool RemoveRelic(RelicSO relic)
        {
            if (relic == null)
            {
                return false;
            }

            bool removed =
                relicRuntimeData.RemoveRelic(relic);

            if (!removed)
            {
                return false;
            }

            if (logDebug)
            {
                Debug.Log(
                    $"[ItemManager] Relic removed. relic={relic.displayName}");
            }

            OnRelicRemoved?.Invoke(relic);
            return true;
        }

        public bool HasRelic(RelicSO relic)
        {
            if (relic == null)
            {
                return false;
            }

            return relicRuntimeData.HasRelic(relic);
        }

        public void ClearRelics()
        {
            relicRuntimeData.Clear();

            if (logDebug)
            {
                Debug.Log(
                    "[ItemManager] All relics cleared.");
            }
        }
    }
}