

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어가 보유한 장비 인스턴스 목록을 관리한다.
/// 같은 EquipmentSkillSO라도 각각 별도 OwnedEquipmentData로 보관하며,
/// 동일 장비/동일 등급 3개를 소비해 대상 장비를 다음 등급으로 승급시킨다.
/// </summary>
public class EquipmentInventoryMono : MonoBehaviour
{
    private const int UpgradeMaterialCount = 2;

    [Header("Inventory")]
    [SerializeField] private List<OwnedEquipmentData> equipments = new();

    public IReadOnlyList<OwnedEquipmentData> Equipments => equipments;

    public OwnedEquipmentData Acquire(EquipmentSkillSO equipmentSo)
    {
        return Acquire(equipmentSo, ResolveBaseGrade(equipmentSo));
    }

    public OwnedEquipmentData Acquire(EquipmentSkillSO equipmentSo, EquipmentGrade grade)
    {
        if (equipmentSo == null)
        {
            Debug.LogWarning("EquipmentInventoryMono.Acquire failed: equipmentSo is null.", this);
            return null;
        }

        OwnedEquipmentData owned = OwnedEquipmentData.Create(equipmentSo, grade);
        equipments.Add(owned);
        return owned;
    }

    public bool Remove(string instanceId)
    {
        OwnedEquipmentData equipment = Find(instanceId);
        if (equipment == null)
        {
            return false;
        }

        if (equipment.IsEquipped || equipment.IsLocked)
        {
            return false;
        }

        return equipments.Remove(equipment);
    }

    public OwnedEquipmentData Find(string instanceId)
    {
        if (string.IsNullOrWhiteSpace(instanceId) || equipments == null)
        {
            return null;
        }

        for (int i = 0; i < equipments.Count; i++)
        {
            OwnedEquipmentData equipment = equipments[i];
            if (equipment != null && equipment.InstanceId == instanceId)
            {
                return equipment;
            }
        }

        return null;
    }

    public List<OwnedEquipmentData> FindSameEquipmentAndGrade(string equipmentId, EquipmentGrade grade)
    {
        var result = new List<OwnedEquipmentData>();

        if (string.IsNullOrWhiteSpace(equipmentId) || equipments == null)
        {
            return result;
        }

        for (int i = 0; i < equipments.Count; i++)
        {
            OwnedEquipmentData equipment = equipments[i];
            if (equipment == null || !equipment.HasEquipment)
            {
                continue;
            }

            if (equipment.EquipmentId == equipmentId && equipment.CurrentGrade == grade)
            {
                result.Add(equipment);
            }
        }

        return result;
    }

    public List<OwnedEquipmentData> FindUpgradeMaterials(OwnedEquipmentData target)
    {
        var result = new List<OwnedEquipmentData>();

        if (target == null || !target.HasEquipment || equipments == null)
        {
            return result;
        }

        for (int i = 0; i < equipments.Count; i++)
        {
            OwnedEquipmentData equipment = equipments[i];
            if (equipment == null || equipment == target)
            {
                continue;
            }

            if (!equipment.CanUseAsUpgradeMaterial(target.EquipmentId, target.CurrentGrade))
            {
                continue;
            }

            result.Add(equipment);

            if (result.Count >= UpgradeMaterialCount)
            {
                break;
            }
        }

        return result;
    }

    public bool CanUpgrade(string targetInstanceId)
    {
        OwnedEquipmentData target = Find(targetInstanceId);
        return CanUpgrade(target);
    }

    public bool CanUpgrade(OwnedEquipmentData target)
    {
        if (target == null || !target.HasEquipment)
        {
            return false;
        }

        if (target.IsLocked)
        {
            return false;
        }

        if (!TryGetNextGrade(target.CurrentGrade, out _))
        {
            return false;
        }

        return FindUpgradeMaterials(target).Count >= UpgradeMaterialCount;
    }

    public bool TryUpgrade(string targetInstanceId)
    {
        OwnedEquipmentData target = Find(targetInstanceId);
        return TryUpgrade(target);
    }

    public bool TryUpgrade(OwnedEquipmentData target)
    {
        if (!CanUpgrade(target))
        {
            return false;
        }

        if (!TryGetNextGrade(target.CurrentGrade, out EquipmentGrade nextGrade))
        {
            return false;
        }

        List<OwnedEquipmentData> materials = FindUpgradeMaterials(target);
        if (materials.Count < UpgradeMaterialCount)
        {
            return false;
        }

        for (int i = 0; i < materials.Count; i++)
        {
            equipments.Remove(materials[i]);
        }

        target.UpgradeGrade(nextGrade);
        return true;
    }

    public bool TryUpgrade(string targetInstanceId, IReadOnlyList<string> materialInstanceIds)
    {
        OwnedEquipmentData target = Find(targetInstanceId);
        if (target == null || materialInstanceIds == null || materialInstanceIds.Count < UpgradeMaterialCount)
        {
            return false;
        }

        if (!CanUpgrade(target))
        {
            return false;
        }

        if (!TryGetNextGrade(target.CurrentGrade, out EquipmentGrade nextGrade))
        {
            return false;
        }

        var materials = new List<OwnedEquipmentData>();
        for (int i = 0; i < materialInstanceIds.Count; i++)
        {
            OwnedEquipmentData material = Find(materialInstanceIds[i]);
            if (material == null || material == target)
            {
                return false;
            }

            if (!material.CanUseAsUpgradeMaterial(target.EquipmentId, target.CurrentGrade))
            {
                return false;
            }

            if (materials.Contains(material))
            {
                return false;
            }

            materials.Add(material);

            if (materials.Count >= UpgradeMaterialCount)
            {
                break;
            }
        }

        if (materials.Count < UpgradeMaterialCount)
        {
            return false;
        }

        for (int i = 0; i < materials.Count; i++)
        {
            equipments.Remove(materials[i]);
        }

        target.UpgradeGrade(nextGrade);
        return true;
    }

    public bool Equip(string instanceId, int slotIndex)
    {
        OwnedEquipmentData equipment = Find(instanceId);
        if (equipment == null || equipment.IsLocked)
        {
            return false;
        }

        UnequipSlot(slotIndex);
        equipment.EquipToSlot(slotIndex);
        return true;
    }

    public bool Unequip(string instanceId)
    {
        OwnedEquipmentData equipment = Find(instanceId);
        if (equipment == null)
        {
            return false;
        }

        equipment.Unequip();
        return true;
    }

    public void UnequipSlot(int slotIndex)
    {
        if (equipments == null)
        {
            return;
        }

        for (int i = 0; i < equipments.Count; i++)
        {
            OwnedEquipmentData equipment = equipments[i];
            if (equipment != null && equipment.IsEquipped && equipment.EquippedSlotIndex == slotIndex)
            {
                equipment.Unequip();
            }
        }
    }

    public OwnedEquipmentData GetEquipped(int slotIndex)
    {
        if (equipments == null)
        {
            return null;
        }

        for (int i = 0; i < equipments.Count; i++)
        {
            OwnedEquipmentData equipment = equipments[i];
            if (equipment != null && equipment.IsEquipped && equipment.EquippedSlotIndex == slotIndex)
            {
                return equipment;
            }
        }

        return null;
    }

    public List<OwnedEquipmentData> GetEquippedItems()
    {
        var result = new List<OwnedEquipmentData>();

        if (equipments == null)
        {
            return result;
        }

        for (int i = 0; i < equipments.Count; i++)
        {
            OwnedEquipmentData equipment = equipments[i];
            if (equipment != null && equipment.IsEquipped)
            {
                result.Add(equipment);
            }
        }

        return result;
    }

    public void Clear()
    {
        equipments.Clear();
    }

    private EquipmentGrade ResolveBaseGrade(EquipmentSkillSO equipmentSo)
    {
        if (equipmentSo != null && equipmentSo.BaseProfileSo != null)
        {
            return equipmentSo.BaseProfileSo.BaseGrade;
        }

        return EquipmentGrade.Common;
    }

    private bool TryGetNextGrade(EquipmentGrade current, out EquipmentGrade next)
    {
        switch (current)
        {
            case EquipmentGrade.Common:
                next = EquipmentGrade.Rare;
                return true;
            case EquipmentGrade.Rare:
                next = EquipmentGrade.Epic;
                return true;
            case EquipmentGrade.Epic:
                next = EquipmentGrade.Legendary;
                return true;
            default:
                next = current;
                return false;
        }
    }
}