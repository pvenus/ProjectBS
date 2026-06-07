using System.Collections.Generic;
using UnityEngine;

namespace Skill
{
    /// <summary>
    /// 범용 스킬 풀 서비스.
    /// SkillPoolRuntimeData를 받아 slotKey 기반으로 RuntimeData 조회/갱신 기능을 제공한다.
    /// 캐릭터 스킬셋은 SkillPoolSlotKeys.BasicAttack / Active1~3 / Passive1 키를 사용한다.
    /// </summary>
    public class SkillPoolService
    {
        private readonly EquipmentSkillResolver resolver;

        public SkillPoolService()
        {
            resolver = new EquipmentSkillResolver();
        }

        public SkillPoolService(EquipmentSkillResolver resolver)
        {
            this.resolver = resolver ?? new EquipmentSkillResolver();
        }

        public void ResolvePool(SkillPoolRuntimeData poolRuntimeData)
        {
            if (poolRuntimeData == null)
            {
                return;
            }

            poolRuntimeData.ResolveAllSkills(resolver);
        }

        public void ClearResolvedRuntimeData(SkillPoolRuntimeData poolRuntimeData)
        {
            if (poolRuntimeData == null)
            {
                return;
            }

            poolRuntimeData.ClearAllRuntimeData();
        }

        public SkillPoolSlotData GetSlot(
            SkillPoolRuntimeData poolRuntimeData,
            int slotIndex)
        {
            if (poolRuntimeData == null)
            {
                return null;
            }

            return poolRuntimeData.GetSlot(slotIndex);
        }

        public SkillPoolSlotData GetEntryBySlot(
            SkillPoolRuntimeData poolRuntimeData,
            int slotIndex)
        {
            return GetSlotByKey(
                poolRuntimeData,
                GetCharacterSlotKey(slotIndex));
        }

        public SkillPoolSlotData GetSlotByKey(
            SkillPoolRuntimeData poolRuntimeData,
            string slotKey)
        {
            if (poolRuntimeData == null)
            {
                return null;
            }

            return poolRuntimeData.GetSlotByKey(slotKey);
        }

        public EquipmentSkillRuntimeData GetRuntimeData(
            SkillPoolRuntimeData poolRuntimeData,
            int slotIndex)
        {
            SkillPoolSlotData slot = GetSlot(poolRuntimeData, slotIndex);
            return ResolveSlotIfNeeded(slot);
        }

        public EquipmentSkillRuntimeData GetRuntimeBySlot(
            SkillPoolRuntimeData poolRuntimeData,
            int slotIndex)
        {
            SkillPoolSlotData slot = GetEntryBySlot(poolRuntimeData, slotIndex);
            return ResolveSlotIfNeeded(slot);
        }

        public EquipmentSkillRuntimeData GetRuntimeByKey(
            SkillPoolRuntimeData poolRuntimeData,
            string slotKey)
        {
            SkillPoolSlotData slot = GetSlotByKey(poolRuntimeData, slotKey);
            return ResolveSlotIfNeeded(slot);
        }

        public EquipmentSkillRuntimeData GetBasicAttackRuntime(
            SkillPoolRuntimeData poolRuntimeData)
        {
            return GetRuntimeByKey(poolRuntimeData, SkillPoolSlotKeys.BasicAttack);
        }

        public bool HasSkill(
            SkillPoolRuntimeData poolRuntimeData,
            int slotIndex)
        {
            SkillPoolSlotData slot = GetEntryBySlot(poolRuntimeData, slotIndex);
            return slot != null && slot.HasSkill;
        }

        public bool HasBasicAttack(SkillPoolRuntimeData poolRuntimeData)
        {
            return poolRuntimeData != null
                && poolRuntimeData.HasSkillByKey(SkillPoolSlotKeys.BasicAttack);
        }

        public bool HasAnyActiveSkill(SkillPoolRuntimeData poolRuntimeData)
        {
            if (poolRuntimeData == null)
            {
                return false;
            }

            return poolRuntimeData.HasSkillByKey(SkillPoolSlotKeys.Active1)
                || poolRuntimeData.HasSkillByKey(SkillPoolSlotKeys.Active2)
                || poolRuntimeData.HasSkillByKey(SkillPoolSlotKeys.Active3);
        }

        public bool HasPassiveSkill(SkillPoolRuntimeData poolRuntimeData)
        {
            return poolRuntimeData != null
                && poolRuntimeData.HasSkillByKey(SkillPoolSlotKeys.Passive1);
        }

        public bool CanUseSlot(
            SkillPoolRuntimeData poolRuntimeData,
            int slotIndex)
        {
            return GetRuntimeBySlot(poolRuntimeData, slotIndex) != null;
        }

        public bool TryGetRuntimeData(
            SkillPoolRuntimeData poolRuntimeData,
            int slotIndex,
            out EquipmentSkillRuntimeData runtimeData)
        {
            runtimeData = GetRuntimeBySlot(poolRuntimeData, slotIndex);
            return runtimeData != null;
        }

        public List<SkillPoolSlotData> GetActiveEntries(
            SkillPoolRuntimeData poolRuntimeData)
        {
            if (poolRuntimeData == null)
            {
                return new List<SkillPoolSlotData>();
            }

            return poolRuntimeData.GetEntriesByKeys(
                SkillPoolSlotKeys.Active1,
                SkillPoolSlotKeys.Active2,
                SkillPoolSlotKeys.Active3);
        }

        public List<SkillPoolSlotData> GetPassiveEntries(
            SkillPoolRuntimeData poolRuntimeData)
        {
            if (poolRuntimeData == null)
            {
                return new List<SkillPoolSlotData>();
            }

            return poolRuntimeData.GetEntriesByKeys(
                SkillPoolSlotKeys.Passive1);
        }

        public List<EquipmentSkillRuntimeData> GetActiveRuntimes(
            SkillPoolRuntimeData poolRuntimeData)
        {
            List<EquipmentSkillRuntimeData> result = new List<EquipmentSkillRuntimeData>(3);

            AddRuntimeIfExists(result, GetRuntimeByKey(poolRuntimeData, SkillPoolSlotKeys.Active1));
            AddRuntimeIfExists(result, GetRuntimeByKey(poolRuntimeData, SkillPoolSlotKeys.Active2));
            AddRuntimeIfExists(result, GetRuntimeByKey(poolRuntimeData, SkillPoolSlotKeys.Active3));

            return result;
        }

        public List<EquipmentSkillRuntimeData> GetPassiveRuntimes(
            SkillPoolRuntimeData poolRuntimeData)
        {
            List<EquipmentSkillRuntimeData> result = new List<EquipmentSkillRuntimeData>(1);

            AddRuntimeIfExists(result, GetRuntimeByKey(poolRuntimeData, SkillPoolSlotKeys.Passive1));

            return result;
        }

        public List<SkillPoolSlotData> GetAllEntries(
            SkillPoolRuntimeData poolRuntimeData)
        {
            if (poolRuntimeData == null)
            {
                return new List<SkillPoolSlotData>();
            }

            return poolRuntimeData.GetEntriesByKeys(
                SkillPoolSlotKeys.BasicAttack,
                SkillPoolSlotKeys.Active1,
                SkillPoolSlotKeys.Active2,
                SkillPoolSlotKeys.Active3,
                SkillPoolSlotKeys.Passive1);
        }

        public List<EquipmentSkillRuntimeData> GetAllRuntimes(
            SkillPoolRuntimeData poolRuntimeData)
        {
            List<EquipmentSkillRuntimeData> result = new List<EquipmentSkillRuntimeData>(5);

            AddRuntimeIfExists(result, GetRuntimeByKey(poolRuntimeData, SkillPoolSlotKeys.BasicAttack));
            AddRuntimeIfExists(result, GetRuntimeByKey(poolRuntimeData, SkillPoolSlotKeys.Active1));
            AddRuntimeIfExists(result, GetRuntimeByKey(poolRuntimeData, SkillPoolSlotKeys.Active2));
            AddRuntimeIfExists(result, GetRuntimeByKey(poolRuntimeData, SkillPoolSlotKeys.Active3));
            AddRuntimeIfExists(result, GetRuntimeByKey(poolRuntimeData, SkillPoolSlotKeys.Passive1));

            return result;
        }

        public List<EquipmentSkillRuntimeData> GetAllRuntimeData(
            SkillPoolRuntimeData poolRuntimeData)
        {
            return GetAllRuntimes(poolRuntimeData);
        }

        public List<SkillPoolSlotData> GetUsableSlots(
            SkillPoolRuntimeData poolRuntimeData)
        {
            List<SkillPoolSlotData> result = new List<SkillPoolSlotData>();

            if (poolRuntimeData == null)
            {
                return result;
            }

            AddSlotIfRuntimeExists(result, GetSlotByKey(poolRuntimeData, SkillPoolSlotKeys.BasicAttack));
            AddSlotIfRuntimeExists(result, GetSlotByKey(poolRuntimeData, SkillPoolSlotKeys.Active1));
            AddSlotIfRuntimeExists(result, GetSlotByKey(poolRuntimeData, SkillPoolSlotKeys.Active2));
            AddSlotIfRuntimeExists(result, GetSlotByKey(poolRuntimeData, SkillPoolSlotKeys.Active3));
            AddSlotIfRuntimeExists(result, GetSlotByKey(poolRuntimeData, SkillPoolSlotKeys.Passive1));

            return result;
        }

        public bool TryResolveSlot(
            SkillPoolRuntimeData poolRuntimeData,
            int slotIndex,
            out SkillPoolSlotData slot)
        {
            slot = GetEntryBySlot(poolRuntimeData, slotIndex);

            if (slot == null || !slot.HasSkill)
            {
                return false;
            }

            return slot.ResolveRuntime(resolver) != null;
        }

        public void RefreshSlotRuntime(
            SkillPoolRuntimeData poolRuntimeData,
            int slotIndex)
        {
            if (poolRuntimeData == null)
            {
                return;
            }

            poolRuntimeData.RefreshSlotRuntimeByKey(
                GetCharacterSlotKey(slotIndex),
                resolver);
        }

        public void DebugLogPool(
            SkillPoolRuntimeData poolRuntimeData)
        {
            if (poolRuntimeData == null || poolRuntimeData.Slots == null)
            {
                Debug.Log("[SkillPoolService] Pool is null or empty.");
                return;
            }

            DebugLogSlot(GetSlotByKey(poolRuntimeData, SkillPoolSlotKeys.BasicAttack), "BasicAttack");
            DebugLogSlot(GetSlotByKey(poolRuntimeData, SkillPoolSlotKeys.Active1), "Active1");
            DebugLogSlot(GetSlotByKey(poolRuntimeData, SkillPoolSlotKeys.Active2), "Active2");
            DebugLogSlot(GetSlotByKey(poolRuntimeData, SkillPoolSlotKeys.Active3), "Active3");
            DebugLogSlot(GetSlotByKey(poolRuntimeData, SkillPoolSlotKeys.Passive1), "Passive1");
        }

        private EquipmentSkillRuntimeData ResolveSlotIfNeeded(SkillPoolSlotData slot)
        {
            if (slot == null || !slot.HasSkill)
            {
                return null;
            }

            if (slot.RuntimeData == null)
            {
                return slot.ResolveRuntime(resolver);
            }

            return slot.RuntimeData;
        }

        private void AddRuntimeIfExists(
            List<EquipmentSkillRuntimeData> result,
            EquipmentSkillRuntimeData runtimeData)
        {
            if (result == null || runtimeData == null)
            {
                return;
            }

            result.Add(runtimeData);
        }

        private void AddSlotIfRuntimeExists(
            List<SkillPoolSlotData> result,
            SkillPoolSlotData slot)
        {
            if (result == null || slot == null)
            {
                return;
            }

            if (ResolveSlotIfNeeded(slot) == null)
            {
                return;
            }

            result.Add(slot);
        }

        private void DebugLogSlot(SkillPoolSlotData slot, string label)
        {
            if (slot == null || slot.SkillSo == null)
            {
                Debug.Log($"[SkillPoolService] {label}: Empty");
                return;
            }

            Debug.Log($"[SkillPoolService] {label}: skill={slot.SkillSo.name} resolved={slot.RuntimeData != null}");
        }

        private string GetCharacterSlotKey(int slotIndex)
        {
            switch (slotIndex)
            {
                case 0:
                    return SkillPoolSlotKeys.BasicAttack;
                case 1:
                    return SkillPoolSlotKeys.Active1;
                case 2:
                    return SkillPoolSlotKeys.Active2;
                case 3:
                    return SkillPoolSlotKeys.Active3;
                case 4:
                    return SkillPoolSlotKeys.Passive1;
                default:
                    return null;
            }
        }
    }
}