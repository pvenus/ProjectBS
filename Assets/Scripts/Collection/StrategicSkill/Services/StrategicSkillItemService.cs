using Battle;
using Skill;
using Skill.Service.Helper;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Item.Service
{
    public class StrategicSkillItemService
    {
        private readonly EquipmentSkillResolver skillResolver = new();

        private readonly List<StrategicSkillItemSO> ownedItems = new();

        public event Action<StrategicSkillItemSO> OnStrategicSkillItemAdded;
        public event Action<StrategicSkillItemSO> OnStrategicSkillItemRemoved;
        public event Action OnStrategicSkillItemsChanged;

        public IReadOnlyList<StrategicSkillItemSO> OwnedItems => ownedItems;

        public bool Add(StrategicSkillItemSO item)
        {
            if (item == null)
                return false;

            if (ownedItems.Contains(item))
                return false;

            ownedItems.Add(item);
            OnStrategicSkillItemAdded?.Invoke(item);
            OnStrategicSkillItemsChanged?.Invoke();
            return true;
        }

        public bool Remove(StrategicSkillItemSO item)
        {
            if (item == null)
                return false;

            if (!ownedItems.Remove(item))
                return false;

            OnStrategicSkillItemRemoved?.Invoke(item);
            OnStrategicSkillItemsChanged?.Invoke();
            return true;
        }

        public bool Has(StrategicSkillItemSO item)
        {
            return item != null
                   && ownedItems.Contains(item);
        }

        public bool Has(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
                return false;

            for (int i = 0; i < ownedItems.Count; i++)
            {
                StrategicSkillItemSO item = ownedItems[i];
                if (item != null && item.strategicSkillItemId == itemId)
                    return true;
            }

            return false;
        }

        public bool TryUseFromScreenPosition(
            StrategicSkillItemSO strategicSkillItem,
            Vector2 screenPosition,
            Camera worldCamera,
            bool logDebug = false,
            UnityEngine.Object logContext = null)
        {
            if (worldCamera == null)
            {
                Debug.LogWarning("[StrategicSkillItemService] World camera is null.", logContext);
                return false;
            }

            Vector3 screenPoint = new Vector3(
                screenPosition.x,
                screenPosition.y,
                Mathf.Abs(worldCamera.transform.position.z));

            Vector3 worldPosition = worldCamera.ScreenToWorldPoint(screenPoint);

            return TryUse(
                strategicSkillItem,
                worldPosition,
                logDebug,
                logContext);
        }

        public bool TryUse(
            StrategicSkillItemSO strategicSkillItem,
            Vector3 worldPosition,
            bool logDebug = false,
            UnityEngine.Object logContext = null)
        {
            if (!TryBuildRuntimeData(
                    strategicSkillItem,
                    logDebug,
                    logContext,
                    out EquipmentSkillRuntimeData runtimeData))
            {
                return false;
            }

            StrategicSkillCostManager costManager = StrategicSkillCostManager.Instance;
            if (costManager == null)
            {
                Debug.LogWarning(
                    $"[StrategicSkillItemService] Execution failed: StrategicSkillCostManager.Instance is null. " +
                    $"item={strategicSkillItem.strategicSkillItemId}",
                    logContext);
                return false;
            }

            if (!costManager.CanSpend(strategicSkillItem.gaugeCost))
            {
                if (logDebug)
                {
                    Debug.Log(
                        $"[StrategicSkillItemService] Execution failed: insufficient gauge. " +
                        $"item={strategicSkillItem.strategicSkillItemId} " +
                        $"cost={strategicSkillItem.gaugeCost} current={costManager.CurrentGauge}",
                        logContext);
                }

                return false;
            }

            ItemManager itemManager = ItemManager.Instance;
            if (itemManager == null)
            {
                Debug.LogWarning(
                    $"[StrategicSkillItemService] Execution failed: ItemManager.Instance is null. " +
                    $"item={strategicSkillItem.strategicSkillItemId}",
                    logContext);
                return false;
            }

            bool executed = SkillUseHelper.UseSkill(new SkillUseContext
            {
                Runtime = runtimeData,
                Caster = itemManager.transform,
                Target = null,
                UsePoint = true,
                TargetPoint = worldPosition,
                CoroutineRunner = itemManager
            });

            if (!executed)
            {
                Debug.LogWarning(
                    $"[StrategicSkillItemService] Execution failed: SkillUseHelper returned false. " +
                    $"item={strategicSkillItem.strategicSkillItemId} pos={worldPosition}",
                    logContext);
                return false;
            }

            if (!costManager.TrySpend(strategicSkillItem.gaugeCost))
            {
                Debug.LogError(
                    $"[StrategicSkillItemService] Skill executed but gauge spend unexpectedly failed. " +
                    $"item={strategicSkillItem.strategicSkillItemId} " +
                    $"cost={strategicSkillItem.gaugeCost} current={costManager.CurrentGauge}",
                    logContext);
                return false;
            }

            if (logDebug)
            {
                Debug.Log($"[StrategicSkillItemService] Strategic skill executed. item={strategicSkillItem.DisplayName} pos={worldPosition}", logContext);
            }

            return true;
        }

        private bool TryBuildRuntimeData(
            StrategicSkillItemSO strategicSkillItem,
            bool logDebug,
            UnityEngine.Object logContext,
            out EquipmentSkillRuntimeData runtimeData)
        {
            runtimeData = null;

            if (strategicSkillItem == null)
            {
                Debug.LogWarning(
                    "[StrategicSkillItemService] Execution failed: StrategicSkillItemSO is null.",
                    logContext);
                return false;
            }

            EquipmentSkillSO skillSo = strategicSkillItem.skillSo;

            if (skillSo == null)
            {
                Debug.LogWarning(
                    $"[StrategicSkillItemService] EquipmentSkillSO is not assigned. " +
                    $"item={strategicSkillItem.strategicSkillItemId}",
                    logContext);
                return false;
            }

            if (!ValidateSkillDefinition(skillSo, strategicSkillItem, logContext))
            {
                return false;
            }

            EquipmentSkillInstanceData instanceData = new EquipmentSkillInstanceData
            {
                equipmentId = skillSo.EquipmentId,
            };

            runtimeData = skillResolver.Resolve(
                skillSo,
                instanceData);

            if (runtimeData == null)
            {
                Debug.LogWarning($"[StrategicSkillItemService] RuntimeData is null. item={strategicSkillItem.DisplayName}", logContext);
                return false;
            }

            if (logDebug)
            {
                Debug.Log($"[StrategicSkillItemService] Skill runtime data built. item={strategicSkillItem.DisplayName}", logContext);
            }

            return true;
        }

        private static bool ValidateSkillDefinition(
            EquipmentSkillSO skillSo,
            StrategicSkillItemSO strategicSkillItem,
            UnityEngine.Object logContext)
        {
            if (skillSo.BaseProfileSo == null)
            {
                Debug.LogWarning(
                    $"[StrategicSkillItemService] Execution failed: BaseProfileSo is null. " +
                    $"item={strategicSkillItem.strategicSkillItemId}",
                    logContext);
                return false;
            }

            if (skillSo.CastSo == null)
            {
                Debug.LogWarning(
                    $"[StrategicSkillItemService] Execution failed: CastSo is null. " +
                    $"item={strategicSkillItem.strategicSkillItemId}",
                    logContext);
                return false;
            }

            if (skillSo.BaseProfileSo.SkillComponentType == SkillComponentType.Spawn)
            {
                if (skillSo.SpawnSkillSo != null)
                {
                    return true;
                }

                Debug.LogWarning(
                    $"[StrategicSkillItemService] Execution failed: SpawnSkillSo is null for a spawn skill. " +
                    $"item={strategicSkillItem.strategicSkillItemId}",
                    logContext);
                return false;
            }

            if (skillSo.MoveSo == null)
            {
                Debug.LogWarning(
                    $"[StrategicSkillItemService] Execution failed: MoveSo is null. " +
                    $"item={strategicSkillItem.strategicSkillItemId}",
                    logContext);
                return false;
            }

            SkillHitSO[] hitSos = skillSo.HitSos;
            if (hitSos == null || hitSos.Length == 0)
            {
                Debug.LogWarning(
                    $"[StrategicSkillItemService] Execution failed: HitSos is empty. " +
                    $"item={strategicSkillItem.strategicSkillItemId}",
                    logContext);
                return false;
            }

            for (int i = 0; i < hitSos.Length; i++)
            {
                if (hitSos[i] != null)
                {
                    return true;
                }
            }

            Debug.LogWarning(
                $"[StrategicSkillItemService] Execution failed: HitSos contains no valid SkillHitSO. " +
                $"item={strategicSkillItem.strategicSkillItemId}",
                logContext);
            return false;
        }

        public void Clear()
        {
            if (ownedItems.Count <= 0)
                return;

            ownedItems.Clear();
            OnStrategicSkillItemsChanged?.Invoke();
        }
    }
}
