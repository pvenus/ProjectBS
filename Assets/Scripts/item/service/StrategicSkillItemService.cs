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
        private readonly StrategicSkillCostManager costManager = StrategicSkillCostManager.Instance;

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

            if (costManager == null)
            {
                Debug.LogWarning("[StrategicSkillItemService] StrategicSkillCostManager is null.", logContext);
                return false;
            }

            if (!costManager.TrySpend(strategicSkillItem.gaugeCost))
            {
                if (logDebug)
                {
                    Debug.Log($"[StrategicSkillItemService] Not enough strategic skill gauge. item={strategicSkillItem.DisplayName} cost={strategicSkillItem.gaugeCost}", logContext);
                }

                return false;
            }

            SkillUseHelper.UseSkill(new SkillUseContext
            {
                Runtime = runtimeData,
                Caster = ItemManager.Instance.transform,
                Target = null,
                UsePoint = true,
                TargetPoint = worldPosition,
                CoroutineRunner = ItemManager.Instance
            });

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
                return false;
            }

            if (strategicSkillItem.skillSo == null)
            {
                Debug.LogWarning($"[StrategicSkillItemService] SkillSO is null. item={strategicSkillItem.DisplayName}", logContext);
                return false;
            }

            EquipmentSkillInstanceData instanceData = new EquipmentSkillInstanceData
            {
                equipmentId = strategicSkillItem.skillSo.EquipmentId,
            };

            runtimeData = skillResolver.Resolve(
                strategicSkillItem.skillSo,
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

        public void Clear()
        {
            if (ownedItems.Count <= 0)
                return;

            ownedItems.Clear();
            OnStrategicSkillItemsChanged?.Invoke();
        }
    }
}