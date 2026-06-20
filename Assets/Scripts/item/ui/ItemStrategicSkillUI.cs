using System.Collections.Generic;
using Item.Service;
using TMPro;
using UnityEngine;
using Battle;

namespace Item.UI
{
    /// <summary>
    /// ItemManager가 보유한 전략 스킬 아이템을 전투 UI 슬롯으로 표시하고,
    /// 클래시 로얄처럼 슬롯을 드래그해 놓은 위치에 전략 스킬 프리팹을 생성한다.
    /// 실제 피해/이펙트/범위 판정은 생성된 프리팹 내부 스킬 로직에서 처리한다.
    /// </summary>
    public class ItemStrategicSkillUI : MonoBehaviour
    {
        [Header("Source")]
        // Removed StrategicSkillCostManager costManager field as per refactor instructions.

        [SerializeField] private Camera worldCamera;

        [Header("Slot UI")]
        [SerializeField] private Transform slotRoot;
        [SerializeField] private ItemStrategicSkillSlotUI slotPrefab;
        [SerializeField] private bool rebuildOnEnable = true;

        [Header("Spawn")]
        [SerializeField] private Transform spawnParent;
        [SerializeField] private bool fixedZ = true;
        [SerializeField] private float spawnZ = 0f;

        [Header("Debug")]
        [SerializeField] private bool logDebug;

        private readonly List<ItemStrategicSkillSlotUI> spawnedSlots = new();

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();
            SubscribeItemManager();

            if (rebuildOnEnable)
            {
                Rebuild();
            }
        }

        private void OnDisable()
        {
            UnsubscribeItemManager();
        }

        [ContextMenu("Rebuild")]
        public void Rebuild()
        {
            ClearSlots();

            ItemManager itemManager = ItemManager.Instance;

            if (itemManager == null)
            {
                Debug.LogWarning("[ItemStrategicSkillUI] ItemManager is null.", this);
                return;
            }

            if (slotRoot == null || slotPrefab == null)
            {
                Debug.LogWarning("[ItemStrategicSkillUI] SlotRoot or SlotPrefab is null.", this);
                return;
            }

            StrategicSkillItemRuntimeData runtimeData = itemManager.StrategicSkillItemRuntimeData;

            if (runtimeData == null || runtimeData.StrategicSkillItems == null)
            {
                return;
            }

            foreach (StrategicSkillItemRuntimeData.StrategicSkillItemEntry entry in runtimeData.StrategicSkillItems)
            {
                if (entry == null || !entry.Owned || entry.StrategicSkillItem == null)
                {
                    continue;
                }

                ItemStrategicSkillSlotUI slot = Instantiate(slotPrefab, slotRoot);
                slot.gameObject.SetActive(true);
                slot.Initialize(this, entry.StrategicSkillItem);
                spawnedSlots.Add(slot);
            }
        }
        private void ResolveReferences()
        {
            // Removed costManager resolution as per refactor instructions.

            if (worldCamera == null)
            {
                worldCamera = Camera.main;
            }
        }

        private void SubscribeItemManager()
        {
            ItemManager itemManager = ItemManager.Instance;

            if (itemManager == null)
            {
                return;
            }

            itemManager.OnStrategicSkillItemAdded -= HandleStrategicSkillItemChanged;
            itemManager.OnStrategicSkillItemRemoved -= HandleStrategicSkillItemChanged;
            itemManager.OnStrategicSkillItemAdded += HandleStrategicSkillItemChanged;
            itemManager.OnStrategicSkillItemRemoved += HandleStrategicSkillItemChanged;
        }

        private void UnsubscribeItemManager()
        {
            ItemManager itemManager = ItemManager.Instance;

            if (itemManager == null)
            {
                return;
            }

            itemManager.OnStrategicSkillItemAdded -= HandleStrategicSkillItemChanged;
            itemManager.OnStrategicSkillItemRemoved -= HandleStrategicSkillItemChanged;
        }

        private void HandleStrategicSkillItemChanged(StrategicSkillItemSO _)
        {
            Rebuild();
        }

        private void ClearSlots()
        {
            for (int i = spawnedSlots.Count - 1; i >= 0; i--)
            {
                ItemStrategicSkillSlotUI slot = spawnedSlots[i];

                if (slot == null)
                {
                    continue;
                }

                Destroy(slot.gameObject);
            }

            spawnedSlots.Clear();
        }
    }

}