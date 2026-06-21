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

        public void ResolvePool(
            SkillPoolRuntimeData poolRuntimeData,
            Character.CharacterRuntimeData characterRuntimeData)
        {
            if (poolRuntimeData == null)
            {
                return;
            }

            poolRuntimeData.ResolveAllSkills(
                resolver,
                characterRuntimeData);
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
            int slotIndex,
            Character.CharacterRuntimeData characterRuntimeData)
        {
            SkillPoolSlotData slot = GetSlot(poolRuntimeData, slotIndex);
            return ResolveSlotIfNeeded(
                slot,
                characterRuntimeData);
        }

        public EquipmentSkillRuntimeData GetRuntimeBySlot(
            SkillPoolRuntimeData poolRuntimeData,
            int slotIndex,
            Character.CharacterRuntimeData characterRuntimeData)
        {
            SkillPoolSlotData slot = GetEntryBySlot(poolRuntimeData, slotIndex);
            return ResolveSlotIfNeeded(
                slot,
                characterRuntimeData);
        }

        public EquipmentSkillRuntimeData GetRuntimeByKey(
            SkillPoolRuntimeData poolRuntimeData,
            string slotKey,
            Character.CharacterRuntimeData characterRuntimeData)
        {
            SkillPoolSlotData slot = GetSlotByKey(poolRuntimeData, slotKey);
            return ResolveSlotIfNeeded(
                slot,
                characterRuntimeData);
        }

        public EquipmentSkillRuntimeData GetBasicAttackRuntime(
            SkillPoolRuntimeData poolRuntimeData,
            Character.CharacterRuntimeData characterRuntimeData)
        {
            return GetRuntimeByKey(
                poolRuntimeData,
                SkillPoolSlotKeys.BasicAttack,
                characterRuntimeData);
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
            int slotIndex,
            Character.CharacterRuntimeData characterRuntimeData)
        {
            return GetRuntimeBySlot(
                poolRuntimeData,
                slotIndex,
                characterRuntimeData) != null;
        }

        public bool TryGetRuntimeData(
            SkillPoolRuntimeData poolRuntimeData,
            int slotIndex,
            Character.CharacterRuntimeData characterRuntimeData,
            out EquipmentSkillRuntimeData runtimeData)
        {
            runtimeData = GetRuntimeBySlot(
                poolRuntimeData,
                slotIndex,
                characterRuntimeData);
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
            SkillPoolRuntimeData poolRuntimeData,
            Character.CharacterRuntimeData characterRuntimeData)
        {
            List<EquipmentSkillRuntimeData> result = new List<EquipmentSkillRuntimeData>(3);

            AddRuntimeIfExists(result, GetRuntimeByKey(poolRuntimeData, SkillPoolSlotKeys.Active1, characterRuntimeData));
            AddRuntimeIfExists(result, GetRuntimeByKey(poolRuntimeData, SkillPoolSlotKeys.Active2, characterRuntimeData));
            AddRuntimeIfExists(result, GetRuntimeByKey(poolRuntimeData, SkillPoolSlotKeys.Active3, characterRuntimeData));

            return result;
        }

        public List<EquipmentSkillRuntimeData> GetPassiveRuntimes(
            SkillPoolRuntimeData poolRuntimeData,
            Character.CharacterRuntimeData characterRuntimeData)
        {
            List<EquipmentSkillRuntimeData> result = new List<EquipmentSkillRuntimeData>(1);

            AddRuntimeIfExists(result, GetRuntimeByKey(poolRuntimeData, SkillPoolSlotKeys.Passive1, characterRuntimeData));

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
            SkillPoolRuntimeData poolRuntimeData,
            Character.CharacterRuntimeData characterRuntimeData)
        {
            List<EquipmentSkillRuntimeData> result = new List<EquipmentSkillRuntimeData>(5);

            AddRuntimeIfExists(result, GetRuntimeByKey(poolRuntimeData, SkillPoolSlotKeys.BasicAttack, characterRuntimeData));
            AddRuntimeIfExists(result, GetRuntimeByKey(poolRuntimeData, SkillPoolSlotKeys.Active1, characterRuntimeData));
            AddRuntimeIfExists(result, GetRuntimeByKey(poolRuntimeData, SkillPoolSlotKeys.Active2, characterRuntimeData));
            AddRuntimeIfExists(result, GetRuntimeByKey(poolRuntimeData, SkillPoolSlotKeys.Active3, characterRuntimeData));
            AddRuntimeIfExists(result, GetRuntimeByKey(poolRuntimeData, SkillPoolSlotKeys.Passive1, characterRuntimeData));

            return result;
        }

        public List<EquipmentSkillRuntimeData> GetAllRuntimeData(
            SkillPoolRuntimeData poolRuntimeData,
            Character.CharacterRuntimeData characterRuntimeData)
        {
            return GetAllRuntimes(
                poolRuntimeData,
                characterRuntimeData);
        }

        public List<SkillPoolSlotData> GetUsableSlots(
            SkillPoolRuntimeData poolRuntimeData,
            Character.CharacterRuntimeData characterRuntimeData)
        {
            List<SkillPoolSlotData> result = new List<SkillPoolSlotData>();

            if (poolRuntimeData == null)
            {
                return result;
            }

            AddSlotIfRuntimeExists(result, GetSlotByKey(poolRuntimeData, SkillPoolSlotKeys.BasicAttack), characterRuntimeData);
            AddSlotIfRuntimeExists(result, GetSlotByKey(poolRuntimeData, SkillPoolSlotKeys.Active1), characterRuntimeData);
            AddSlotIfRuntimeExists(result, GetSlotByKey(poolRuntimeData, SkillPoolSlotKeys.Active2), characterRuntimeData);
            AddSlotIfRuntimeExists(result, GetSlotByKey(poolRuntimeData, SkillPoolSlotKeys.Active3), characterRuntimeData);
            AddSlotIfRuntimeExists(result, GetSlotByKey(poolRuntimeData, SkillPoolSlotKeys.Passive1), characterRuntimeData);

            return result;
        }

        public bool TryResolveSlot(
            SkillPoolRuntimeData poolRuntimeData,
            int slotIndex,
            Character.CharacterRuntimeData characterRuntimeData,
            out SkillPoolSlotData slot)
        {
            slot = GetEntryBySlot(poolRuntimeData, slotIndex);

            if (slot == null || !slot.HasSkill)
            {
                return false;
            }

            EquipmentSkillInstanceData instanceData =
                characterRuntimeData?.GetOrCreateSkillInstance(slot.SkillSo.EquipmentId);

            return slot.ResolveRuntime(
                resolver,
                instanceData) != null;
        }

        public void RefreshSlotRuntime(
            SkillPoolRuntimeData poolRuntimeData,
            int slotIndex,
            Character.CharacterRuntimeData characterRuntimeData)
        {
            if (poolRuntimeData == null)
            {
                return;
            }

            poolRuntimeData.RefreshSlotRuntimeByKey(
                GetCharacterSlotKey(slotIndex),
                resolver,
                characterRuntimeData);
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

        private EquipmentSkillRuntimeData ResolveSlotIfNeeded(
            SkillPoolSlotData slot,
            Character.CharacterRuntimeData characterRuntimeData)
        {
            if (slot == null || !slot.HasSkill)
            {
                return null;
            }

            if (slot.RuntimeData == null)
            {
                EquipmentSkillInstanceData instanceData =
                    characterRuntimeData?.GetOrCreateSkillInstance(slot.SkillSo.EquipmentId);

                return slot.ResolveRuntime(
                    resolver,
                    instanceData);
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
            SkillPoolSlotData slot,
            Character.CharacterRuntimeData characterRuntimeData)
        {
            if (result == null || slot == null)
            {
                return;
            }

            if (ResolveSlotIfNeeded(slot, characterRuntimeData) == null)
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