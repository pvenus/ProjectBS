using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EquipmentSkillLoadoutEntry
{
    [Header("Source")]
    [SerializeField] private EquipmentSkillSO skillSo;

    [Header("Instance")]
    [SerializeField] private EquipmentGrade currentGrade = EquipmentGrade.Common;
    [SerializeField, Min(1)] private int currentRuneSlotCount = 1;
    [SerializeField] private ElementType mainElement = ElementType.None;
    [SerializeField] private List<ElementType> subElements = new();
    [SerializeField] private ProjectileEntity projectilePrefabOverride;
    [SerializeField] private float projectileLifetimeOverride = -1f;

    [System.NonSerialized] private EquipmentSkillRuntimeData runtimeData;

    public EquipmentSkillSO SkillSo => skillSo;
    public EquipmentGrade CurrentGrade => currentGrade;
    public int CurrentRuneSlotCount => currentRuneSlotCount;
    public ElementType MainElement => mainElement;
    public IReadOnlyList<ElementType> SubElements => subElements;
    public ProjectileEntity ProjectilePrefabOverride => projectilePrefabOverride;
    public float ProjectileLifetimeOverride => projectileLifetimeOverride;
    public EquipmentSkillRuntimeData RuntimeData => runtimeData;
    public bool HasSkill => skillSo != null;

    public EquipmentSkillInstanceData BuildInstanceData()
    {
        return new EquipmentSkillInstanceData
        {
            equipmentId = skillSo != null ? skillSo.EquipmentId : string.Empty,
            currentGrade = currentGrade,
            currentRuneSlotCount = Mathf.Max(1, currentRuneSlotCount),
            mainElement = mainElement,
            subElements = subElements != null ? new List<ElementType>(subElements) : new List<ElementType>(),
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

public class SkillLoadoutMono : MonoBehaviour
{
    [Header("Loadout")]
    [SerializeField] private EquipmentSkillLoadoutEntry basicAttack = new();
    [SerializeField] private EquipmentSkillLoadoutEntry skill1 = new();
    [SerializeField] private EquipmentSkillLoadoutEntry skill2 = new();
    [SerializeField] private EquipmentSkillLoadoutEntry skill3 = new();

    private EquipmentSkillResolver resolver;

    public EquipmentSkillLoadoutEntry BasicAttack => basicAttack;
    public EquipmentSkillLoadoutEntry Skill1 => skill1;
    public EquipmentSkillLoadoutEntry Skill2 => skill2;
    public EquipmentSkillLoadoutEntry Skill3 => skill3;

    private void Awake()
    {
        resolver = new EquipmentSkillResolver();
        ResolveAllSkills();
    }

    public int ActiveSkillCount
    {
        get
        {
            int count = 0;
            if (skill1 != null && skill1.HasSkill) count++;
            if (skill2 != null && skill2.HasSkill) count++;
            if (skill3 != null && skill3.HasSkill) count++;
            return count;
        }
    }

    public EquipmentSkillLoadoutEntry GetEntryBySlot(int slotIndex)
    {
        switch (slotIndex)
        {
            case 0: return basicAttack;
            case 1: return skill1;
            case 2: return skill2;
            case 3: return skill3;
            default: return null;
        }
    }

    public EquipmentSkillRuntimeData GetRuntimeBySlot(int slotIndex)
    {
        EquipmentSkillLoadoutEntry entry = GetEntryBySlot(slotIndex);
        return entry != null ? entry.RuntimeData : null;
    }

    public EquipmentSkillRuntimeData GetBasicAttackRuntime()
    {
        return basicAttack != null ? basicAttack.RuntimeData : null;
    }

    public EquipmentSkillLoadoutEntry[] GetActiveEntries()
    {
        List<EquipmentSkillLoadoutEntry> skills = new List<EquipmentSkillLoadoutEntry>(3);

        if (skill1 != null && skill1.HasSkill) skills.Add(skill1);
        if (skill2 != null && skill2.HasSkill) skills.Add(skill2);
        if (skill3 != null && skill3.HasSkill) skills.Add(skill3);

        return skills.ToArray();
    }

    public EquipmentSkillRuntimeData[] GetActiveRuntimes()
    {
        List<EquipmentSkillRuntimeData> skills = new List<EquipmentSkillRuntimeData>(3);

        if (skill1 != null && skill1.RuntimeData != null) skills.Add(skill1.RuntimeData);
        if (skill2 != null && skill2.RuntimeData != null) skills.Add(skill2.RuntimeData);
        if (skill3 != null && skill3.RuntimeData != null) skills.Add(skill3.RuntimeData);

        return skills.ToArray();
    }

    public EquipmentSkillLoadoutEntry[] GetAllEntries()
    {
        List<EquipmentSkillLoadoutEntry> skills = new List<EquipmentSkillLoadoutEntry>(4);

        if (basicAttack != null && basicAttack.HasSkill) skills.Add(basicAttack);
        if (skill1 != null && skill1.HasSkill) skills.Add(skill1);
        if (skill2 != null && skill2.HasSkill) skills.Add(skill2);
        if (skill3 != null && skill3.HasSkill) skills.Add(skill3);

        return skills.ToArray();
    }

    public EquipmentSkillRuntimeData[] GetAllRuntimes()
    {
        List<EquipmentSkillRuntimeData> skills = new List<EquipmentSkillRuntimeData>(4);

        if (basicAttack != null && basicAttack.RuntimeData != null) skills.Add(basicAttack.RuntimeData);
        if (skill1 != null && skill1.RuntimeData != null) skills.Add(skill1.RuntimeData);
        if (skill2 != null && skill2.RuntimeData != null) skills.Add(skill2.RuntimeData);
        if (skill3 != null && skill3.RuntimeData != null) skills.Add(skill3.RuntimeData);

        return skills.ToArray();
    }

    public bool HasAnyActiveSkill()
    {
        return (skill1 != null && skill1.HasSkill)
            || (skill2 != null && skill2.HasSkill)
            || (skill3 != null && skill3.HasSkill);
    }

    public bool HasBasicAttack()
    {
        return basicAttack != null && basicAttack.HasSkill;
    }

    public void ResolveAllSkills()
    {
        if (resolver == null)
        {
            resolver = new EquipmentSkillResolver();
        }

        basicAttack?.ResolveRuntime(resolver);
        skill1?.ResolveRuntime(resolver);
        skill2?.ResolveRuntime(resolver);
        skill3?.ResolveRuntime(resolver);
    }

    public void RefreshSlotRuntime(int slotIndex)
    {
        if (resolver == null)
        {
            resolver = new EquipmentSkillResolver();
        }

        EquipmentSkillLoadoutEntry entry = GetEntryBySlot(slotIndex);
        entry?.ResolveRuntime(resolver);
    }

    public void ClearAllRuntimeData()
    {
        basicAttack?.ClearRuntime();
        skill1?.ClearRuntime();
        skill2?.ClearRuntime();
        skill3?.ClearRuntime();
    }
}
