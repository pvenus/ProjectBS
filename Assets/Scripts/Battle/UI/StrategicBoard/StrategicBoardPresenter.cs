using System.Collections.Generic;
using Battle;
using Item;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Battle.UI.StrategicBoard
{
    /// <summary>
    /// ItemManager 및 StrategicSkillCostManager의 런타임 데이터를 StrategicBoardView에 바인딩하고,
    /// 슬롯의 실행 요청 이벤트를 ItemManager의 스킬 발동 API로 중계하는 Presenter 컴포넌트.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class StrategicBoardPresenter : MonoBehaviour
    {
        [Header("View Reference")]
        [SerializeField] private StrategicBoardView boardView;

        [Header("Camera")]
        [SerializeField] private Camera worldCamera;

        [Header("Debug")]
        [SerializeField] private bool logDebug;

        private ItemManager subscribedItemManager;
        private StrategicSkillCostManager subscribedCostManager;
        private readonly List<StrategicSkillSlotView> boundSlots = new();

        public StrategicBoardView BoardView => boardView;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();
            SubscribeEvents();
            Rebind();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        private void ResolveReferences()
        {
            if (boardView == null)
            {
                boardView = GetComponent<StrategicBoardView>();
            }

            if (worldCamera == null)
            {
                worldCamera = Camera.main;
            }
        }

        [ContextMenu("Rebind")]
        public void Rebind()
        {
            if (boardView == null)
            {
                return;
            }

            IReadOnlyList<StrategicSkillSlotView> slots = boardView.Slots;
            if (slots == null || slots.Count == 0)
            {
                return;
            }

            ItemManager itemManager = ItemManager.Instance;
            StrategicSkillItemRuntimeData runtimeData = itemManager != null ? itemManager.StrategicSkillItemRuntimeData : null;
            IReadOnlyList<StrategicSkillItemRuntimeData.StrategicSkillItemEntry> entries = runtimeData?.StrategicSkillItems;

            int entryIndex = 0;
            int entryCount = entries?.Count ?? 0;

            for (int i = 0; i < slots.Count; i++)
            {
                StrategicSkillSlotView slot = slots[i];
                if (slot == null)
                {
                    continue;
                }

                // Find next valid owned item
                StrategicSkillItemSO itemSO = null;
                while (entryIndex < entryCount)
                {
                    StrategicSkillItemRuntimeData.StrategicSkillItemEntry entry = entries[entryIndex++];
                    if (entry != null && entry.Owned && entry.StrategicSkillItem != null)
                    {
                        itemSO = entry.StrategicSkillItem;
                        break;
                    }
                }

                if (itemSO != null)
                {
                    slot.SetContent(itemSO.icon, itemSO.gaugeCost, itemSO);
                    if (logDebug)
                    {
                        Debug.Log($"[StrategicBoardPresenter] Bound slot {slot.SlotId} with item {itemSO.DisplayName} (cost: {itemSO.gaugeCost}).", this);
                    }
                }
                else
                {
                    slot.ClearContent();
                }
            }

            StrategicSkillCostManager costManager = StrategicSkillCostManager.Instance;
            int currentGauge = costManager != null ? costManager.CurrentGauge : 0;
            boardView.RefreshSlotResourceStates(currentGauge);
        }

        private void SubscribeEvents()
        {
            UnsubscribeEvents();

            // 1. Subscribe to Slot Execution Events
            if (boardView != null && boardView.Slots != null)
            {
                foreach (StrategicSkillSlotView slot in boardView.Slots)
                {
                    if (slot != null)
                    {
                        slot.ExecutionRequested += HandleSlotExecutionRequested;
                        boundSlots.Add(slot);
                    }
                }
            }

            // 2. Subscribe to ItemManager Events
            subscribedItemManager = ItemManager.Instance;
            if (subscribedItemManager != null)
            {
                subscribedItemManager.OnStrategicSkillItemAdded += HandleItemChanged;
                subscribedItemManager.OnStrategicSkillItemRemoved += HandleItemChanged;
            }

            // 3. Subscribe to CostManager Events
            subscribedCostManager = StrategicSkillCostManager.Instance;
            if (subscribedCostManager != null)
            {
                subscribedCostManager.OnGaugeChanged += HandleGaugeChanged;
            }
        }

        private void UnsubscribeEvents()
        {
            foreach (StrategicSkillSlotView slot in boundSlots)
            {
                if (slot != null)
                {
                    slot.ExecutionRequested -= HandleSlotExecutionRequested;
                }
            }
            boundSlots.Clear();

            if (subscribedItemManager != null)
            {
                subscribedItemManager.OnStrategicSkillItemAdded -= HandleItemChanged;
                subscribedItemManager.OnStrategicSkillItemRemoved -= HandleItemChanged;
                subscribedItemManager = null;
            }

            if (subscribedCostManager != null)
            {
                subscribedCostManager.OnGaugeChanged -= HandleGaugeChanged;
                subscribedCostManager = null;
            }
        }

        private void HandleSlotExecutionRequested(StrategicSkillSlotView slotView, Object payload, PointerEventData eventData)
        {
            if (payload is not StrategicSkillItemSO strategicSkillItem)
            {
                if (logDebug)
                {
                    Debug.LogWarning("[StrategicBoardPresenter] ExecutionRequested received with invalid payload.", this);
                }
                return;
            }

            if (worldCamera == null)
            {
                worldCamera = Camera.main;
            }

            ItemManager itemManager = ItemManager.Instance;
            if (itemManager == null)
            {
                Debug.LogError("[StrategicBoardPresenter] ItemManager.Instance is null when trying to execute strategic skill.", this);
                return;
            }

            if (logDebug)
            {
                Debug.Log($"[StrategicBoardPresenter] Requesting execution for {strategicSkillItem.DisplayName} at screen pos {eventData.position}.", this);
            }

            itemManager.TryUseStrategicSkillItemFromScreenPosition(
                strategicSkillItem,
                eventData.position,
                worldCamera,
                logDebug,
                this);
        }

        private void HandleGaugeChanged(int currentGauge, int maxGauge)
        {
            if (boardView != null)
            {
                boardView.RefreshSlotResourceStates(currentGauge);
            }
        }

        private void HandleItemChanged(StrategicSkillItemSO _)
        {
            Rebind();
        }
    }
}
