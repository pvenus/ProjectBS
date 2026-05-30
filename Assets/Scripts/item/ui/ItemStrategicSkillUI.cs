using System.Collections.Generic;
using Battle;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
        [SerializeField] private StrategicSkillCostManager costManager;
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
        private EquipmentSkillResolver skillResolver;
        private ProjectileFactory projectileFactory;

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

        public bool TryUseStrategicSkillItem(
            StrategicSkillItemSO strategicSkillItem,
            Vector2 screenPosition)
        {
            if (strategicSkillItem == null)
            {
                return false;
            }

            ResolveReferences();

            if (costManager == null)
            {
                Debug.LogWarning("[ItemStrategicSkillUI] StrategicSkillCostManager is null.", this);
                return false;
            }

            Vector3 worldPosition = ScreenToWorldPosition(screenPosition);

            if (!TryBuildStrategicSkillProjectileData(
                    strategicSkillItem,
                    worldPosition,
                    out ProjectileEntity projectilePrefab,
                    out ProjectileRuntimeData projectileData))
            {
                Debug.LogWarning($"[ItemStrategicSkillUI] Failed to build strategic skill projectile data. item={strategicSkillItem.displayName}", this);
                return false;
            }

            if (!costManager.TrySpend(strategicSkillItem.gaugeCost))
            {
                if (logDebug)
                {
                    Debug.Log($"[ItemStrategicSkillUI] Not enough strategic skill gauge. item={strategicSkillItem.displayName} cost={strategicSkillItem.gaugeCost}", this);
                }

                return false;
            }

            projectileFactory.SpawnOriented(projectilePrefab, projectileData);

            if (logDebug)
            {
                Debug.Log($"[ItemStrategicSkillUI] Strategic skill projectile spawned. item={strategicSkillItem.displayName} prefab={projectilePrefab.name} pos={worldPosition}", this);
            }

            if (logDebug)
            {
                Debug.Log($"[ItemStrategicSkillUI] Strategic skill used. item={strategicSkillItem.displayName} pos={worldPosition}", this);
            }

            return true;
        }

        private bool TryBuildStrategicSkillProjectileData(
            StrategicSkillItemSO strategicSkillItem,
            Vector3 worldPosition,
            out ProjectileEntity projectilePrefab,
            out ProjectileRuntimeData projectileData)
        {
            projectilePrefab = null;
            projectileData = null;

            if (strategicSkillItem == null)
            {
                return false;
            }

            if (strategicSkillItem.skillSo == null)
            {
                Debug.LogWarning($"[ItemStrategicSkillUI] SkillSO is null. item={strategicSkillItem.displayName}", this);
                return false;
            }

            EnsureSkillRuntimeHelpers();

            EquipmentSkillInstanceData instanceData = new EquipmentSkillInstanceData
            {
                equipmentId = strategicSkillItem.skillSo.EquipmentId,
                projectilePrefab = strategicSkillItem.projectilePrefabOverride,
                projectileLifetimeOverride = strategicSkillItem.projectileLifetimeOverride
            };

            EquipmentSkillRuntimeData runtimeData = skillResolver.Resolve(
                strategicSkillItem.skillSo,
                instanceData);

            if (runtimeData == null)
            {
                Debug.LogWarning($"[ItemStrategicSkillUI] RuntimeData is null. item={strategicSkillItem.displayName}", this);
                return false;
            }

            projectilePrefab = strategicSkillItem.projectilePrefabOverride != null
                ? strategicSkillItem.projectilePrefabOverride
                : runtimeData.projectilePrefab;

            if (projectilePrefab == null)
            {
                Debug.LogWarning($"[ItemStrategicSkillUI] Projectile prefab is null. item={strategicSkillItem.displayName} skill={strategicSkillItem.skillSo.name}", this);
                return false;
            }

            Vector2 spawnPosition = worldPosition;
            Vector2 direction = Vector2.up;

            projectileData = skillResolver.ResolveProjectileRuntime(
                runtimeData,
                null,
                null,
                spawnPosition,
                direction);

            if (projectileData == null)
            {
                Debug.LogWarning($"[ItemStrategicSkillUI] ProjectileRuntimeData is null. item={strategicSkillItem.displayName} skill={strategicSkillItem.skillSo.name}", this);
                return false;
            }

            projectileData.projectilePrefab = projectilePrefab;
            projectileData.spawnPosition = spawnPosition;

            if (logDebug)
            {
                Debug.Log($"[ItemStrategicSkillUI] Projectile data built. item={strategicSkillItem.displayName} prefab={projectilePrefab.name} pos={spawnPosition}", this);
            }

            return true;
        }

        private void EnsureSkillRuntimeHelpers()
        {
            skillResolver ??= new EquipmentSkillResolver();
            projectileFactory ??= new ProjectileFactory();
        }

        private Vector3 ScreenToWorldPosition(Vector2 screenPosition)
        {
            Camera cameraToUse = worldCamera != null ? worldCamera : Camera.main;

            if (cameraToUse == null)
            {
                Vector3 fallback = screenPosition;

                if (fixedZ)
                {
                    fallback.z = spawnZ;
                }

                return fallback;
            }

            Vector3 screenPoint = new Vector3(
                screenPosition.x,
                screenPosition.y,
                Mathf.Abs(cameraToUse.transform.position.z));

            Vector3 worldPosition = cameraToUse.ScreenToWorldPoint(screenPoint);

            if (fixedZ)
            {
                worldPosition.z = spawnZ;
            }

            return worldPosition;
        }

        private void ResolveReferences()
        {
            if (costManager == null)
            {
                costManager = StrategicSkillCostManager.Instance;
            }

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