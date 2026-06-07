using System;
using System.Collections.Generic;
using UnityEngine;
using Skill;
/// <summary>
/// 플레이어가 실제로 보유한 장비 1개의 인스턴스 데이터.
/// 같은 EquipmentSkillSO를 가진 장비라도 각각 별도 인스턴스로 관리한다.
/// UI에서는 이 데이터 하나를 장비 카드 하나로 표시하면 된다.
/// </summary>
[Serializable]
public class OwnedEquipmentData
{
    [Header("Identity")]
    [SerializeField] private string instanceId;
    [SerializeField] private EquipmentSkillSO equipmentSo;

    [Header("State")]
    [SerializeField] private bool isLocked;

    [Header("Equip")]
    [SerializeField] private bool isEquipped;
    [SerializeField] private int equippedSlotIndex = -1;

    [Header("Runes")]
    [SerializeField] private List<RuneSO> equippedRunes = new();

    [Header("Runtime Overrides")]
    [SerializeField] private ProjectileEntity projectilePrefabOverride;
    [SerializeField] private float projectileLifetimeOverride = -1f;

    public string InstanceId => instanceId;
    public EquipmentSkillSO EquipmentSo => equipmentSo;
    public bool IsLocked => isLocked;
    public bool IsEquipped => isEquipped;
    public int EquippedSlotIndex => equippedSlotIndex;
    public IReadOnlyList<RuneSO> EquippedRunes => equippedRunes;
    public ProjectileEntity ProjectilePrefabOverride => projectilePrefabOverride;
    public float ProjectileLifetimeOverride => projectileLifetimeOverride;

    public bool HasEquipment => equipmentSo != null;
    public string EquipmentId => equipmentSo != null ? equipmentSo.EquipmentId : string.Empty;
    public string DisplayName => equipmentSo != null ? equipmentSo.DisplayName : string.Empty;

    public static OwnedEquipmentData Create(EquipmentSkillSO source)
    {
        return new OwnedEquipmentData
        {
            instanceId = Guid.NewGuid().ToString("N"),
            equipmentSo = source,
            isLocked = false,
            isEquipped = false,
            equippedSlotIndex = -1,
            equippedRunes = new List<RuneSO>(),
            projectilePrefabOverride = null,
            projectileLifetimeOverride = -1f
        };
    }

    public void SetEquipment(EquipmentSkillSO source)
    {
        equipmentSo = source;
    }

    public void SetInstanceId(string id)
    {
        instanceId = string.IsNullOrWhiteSpace(id)
            ? Guid.NewGuid().ToString("N")
            : id;
    }

    public void SetLocked(bool locked)
    {
        isLocked = locked;
    }

    public void EquipToSlot(int slotIndex)
    {
        isEquipped = true;
        equippedSlotIndex = slotIndex;
    }

    public void Unequip()
    {
        isEquipped = false;
        equippedSlotIndex = -1;
    }

    public void SetProjectilePrefabOverride(ProjectileEntity prefab)
    {
        projectilePrefabOverride = prefab;
    }

    public void SetProjectileLifetimeOverride(float lifetime)
    {
        projectileLifetimeOverride = lifetime;
    }

    public bool CanUseAsUpgradeMaterial(string targetEquipmentId)
    {
        if (!HasEquipment)
        {
            return false;
        }

        if (isLocked || isEquipped)
        {
            return false;
        }

        if (EquipmentId != targetEquipmentId)
        {
            return false;
        }

        return true;
    }

    public bool AddRune(RuneSO rune, int maxRuneSlotCount)
    {
        if (rune == null)
        {
            return false;
        }

        if (equippedRunes == null)
        {
            equippedRunes = new List<RuneSO>();
        }

        if (equippedRunes.Count >= Mathf.Max(0, maxRuneSlotCount))
        {
            return false;
        }

        equippedRunes.Add(rune);
        return true;
    }

    public bool RemoveRune(RuneSO rune)
    {
        if (rune == null || equippedRunes == null)
        {
            return false;
        }

        return equippedRunes.Remove(rune);
    }

    public void ClearRunes()
    {
        equippedRunes?.Clear();
    }

    public EquipmentSkillInstanceData ToInstanceData()
    {
        return new EquipmentSkillInstanceData
        {
            equipmentId = EquipmentId,
            equippedRunes = equippedRunes != null
                ? new List<RuneSO>(equippedRunes)
                : new List<RuneSO>(),
            projectilePrefab = projectilePrefabOverride,
            projectileLifetimeOverride = projectileLifetimeOverride
        };
    }
}
