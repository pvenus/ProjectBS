using System;
using System.Collections.Generic;
using UnityEngine;

namespace SKill
{
    /// <summary>
    /// 범용 스킬 풀에서 사용하는 슬롯 키 모음.
    /// SkillPoolRuntimeData 자체는 이 키를 강제하지 않으며,
    /// 캐릭터/전략스킬/상점/보상 등 사용하는 쪽에서 필요한 키만 선택해 쓴다.
    /// </summary>
    public static class SkillPoolSlotKeys
    {
        public const string BasicAttack = "basic_attack";
        public const string Active1 = "active_1";
        public const string Active2 = "active_2";
        public const string Active3 = "active_3";

        public const string Strategic1 = "strategic_1";
        public const string Strategic2 = "strategic_2";
        public const string Strategic3 = "strategic_3";
        public const string Strategic4 = "strategic_4";
    }

    [Serializable]
    public class SkillPoolSlotData
    {
        [Header("Slot")]
        [SerializeField] private string slotKey;

        [Header("Skill")]
        [SerializeField] private EquipmentSkillSO skillSo;

        [Header("Instance")]
        [SerializeField] private EquipmentGrade currentGrade = EquipmentGrade.Common;
        [SerializeField, Min(1)] private int currentRuneSlotCount = 1;
        [SerializeField] private ElementType mainElement = ElementType.None;
        [SerializeField] private List<ElementType> subElements = new();

        [Header("Override")]
        [SerializeField] private ProjectileEntity projectilePrefabOverride;
        [SerializeField] private float projectileLifetimeOverride = -1f;

        [NonSerialized] private EquipmentSkillRuntimeData runtimeData;

        public string SlotKey => slotKey;
        public EquipmentSkillSO SkillSo => skillSo;
        public EquipmentGrade CurrentGrade => currentGrade;
        public int CurrentRuneSlotCount => currentRuneSlotCount;
        public ElementType MainElement => mainElement;
        public IReadOnlyList<ElementType> SubElements => subElements;
        public ProjectileEntity ProjectilePrefabOverride => projectilePrefabOverride;
        public float ProjectileLifetimeOverride => projectileLifetimeOverride;
        public EquipmentSkillRuntimeData RuntimeData => runtimeData;
        public bool HasSkill => skillSo != null;

        public bool IsSlotKey(string key)
        {
            return string.Equals(slotKey, key, StringComparison.Ordinal);
        }

        public EquipmentSkillInstanceData BuildInstanceData()
        {
            return new EquipmentSkillInstanceData
            {
                equipmentId = skillSo != null ? skillSo.EquipmentId : string.Empty,
                currentGrade = currentGrade,
                currentRuneSlotCount = Mathf.Max(1, currentRuneSlotCount),
                mainElement = mainElement,
                subElements = subElements != null
                    ? new List<ElementType>(subElements)
                    : new List<ElementType>(),
                projectilePrefab = projectilePrefabOverride,
                projectileLifetimeOverride = projectileLifetimeOverride
            };
        }

        public EquipmentSkillRuntimeData ResolveRuntime(EquipmentSkillResolver resolver)
        {
            if (resolver == null || skillSo == null)
            {
                runtimeData = null;
                return null;
            }

            runtimeData = resolver.Resolve(skillSo, BuildInstanceData());
            return runtimeData;
        }

        public void ClearRuntime()
        {
            runtimeData = null;
        }
    }

    [Serializable]
    public class SkillPoolRuntimeData
    {
        [Header("Skill Pool")]
        [SerializeField] private List<SkillPoolSlotData> slots = new();

        public IReadOnlyList<SkillPoolSlotData> Slots => slots;

        public SkillPoolSlotData GetSlot(int index)
        {
            if (slots == null || index < 0 || index >= slots.Count)
            {
                return null;
            }

            return slots[index];
        }

        public SkillPoolSlotData GetSlotByKey(string slotKey)
        {
            if (slots == null || string.IsNullOrWhiteSpace(slotKey))
            {
                return null;
            }

            return slots.Find(slot => slot != null && slot.IsSlotKey(slotKey));
        }

        public EquipmentSkillRuntimeData GetRuntimeByKey(string slotKey)
        {
            return GetSlotByKey(slotKey)?.RuntimeData;
        }

        public List<SkillPoolSlotData> GetEntriesByKeys(params string[] slotKeys)
        {
            List<SkillPoolSlotData> result = new List<SkillPoolSlotData>();

            if (slotKeys == null)
            {
                return result;
            }

            foreach (string slotKey in slotKeys)
            {
                AddIfHasSkill(result, GetSlotByKey(slotKey));
            }

            return result;
        }

        public List<EquipmentSkillRuntimeData> GetRuntimesByKeys(params string[] slotKeys)
        {
            List<EquipmentSkillRuntimeData> result = new List<EquipmentSkillRuntimeData>();

            if (slotKeys == null)
            {
                return result;
            }

            foreach (string slotKey in slotKeys)
            {
                AddIfRuntimeExists(result, GetSlotByKey(slotKey));
            }

            return result;
        }

        public bool HasSkillByKey(string slotKey)
        {
            SkillPoolSlotData slot = GetSlotByKey(slotKey);
            return slot != null && slot.HasSkill;
        }

        public void ResolveAllSkills(EquipmentSkillResolver resolver)
        {
            if (slots == null)
            {
                return;
            }

            foreach (SkillPoolSlotData slot in slots)
            {
                slot?.ResolveRuntime(resolver);
            }
        }

        public void RefreshSlotRuntime(int slotIndex, EquipmentSkillResolver resolver)
        {
            GetSlot(slotIndex)?.ResolveRuntime(resolver);
        }

        public void RefreshSlotRuntimeByKey(string slotKey, EquipmentSkillResolver resolver)
        {
            GetSlotByKey(slotKey)?.ResolveRuntime(resolver);
        }

        public void ClearAllRuntimeData()
        {
            if (slots == null)
            {
                return;
            }

            foreach (SkillPoolSlotData slot in slots)
            {
                slot?.ClearRuntime();
            }
        }

        public void AddSlot(SkillPoolSlotData slot)
        {
            if (slot == null)
            {
                return;
            }

            slots.Add(slot);
        }

        public void RemoveSlot(SkillPoolSlotData slot)
        {
            if (slot == null)
            {
                return;
            }

            slots.Remove(slot);
        }

        public void ClearSlots()
        {
            slots.Clear();
        }

        private void AddIfHasSkill(List<SkillPoolSlotData> result, SkillPoolSlotData slot)
        {
            if (result == null || slot == null || !slot.HasSkill)
            {
                return;
            }

            result.Add(slot);
        }

        private void AddIfRuntimeExists(List<EquipmentSkillRuntimeData> result, SkillPoolSlotData slot)
        {
            if (result == null || slot == null || slot.RuntimeData == null)
            {
                return;
            }

            result.Add(slot.RuntimeData);
        }
    }
}