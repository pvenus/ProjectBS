using UnityEngine;
using SKill;

public class SkillLoadoutMono : MonoBehaviour
{
    [Header("Loadout")]
    [SerializeField] private SkillPoolRuntimeData skillPool = new();

    private SkillPoolService skillPoolService;

    public SkillPoolRuntimeData SkillPool => skillPool;
    public SkillPoolSlotData BasicAttack => skillPool?.GetSlotByKey(SkillPoolSlotKeys.BasicAttack);
    public SkillPoolSlotData Skill1 => skillPool?.GetSlotByKey(SkillPoolSlotKeys.Active1);
    public SkillPoolSlotData Skill2 => skillPool?.GetSlotByKey(SkillPoolSlotKeys.Active2);
    public SkillPoolSlotData Skill3 => skillPool?.GetSlotByKey(SkillPoolSlotKeys.Active3);

    private void Awake()
    {
        skillPoolService = new SkillPoolService();
        ResolveAllSkills();
    }

    public int ActiveSkillCount
    {
        get
        {
            int count = 0;

            if (skillPool != null && skillPool.HasSkillByKey(SkillPoolSlotKeys.Active1))
            {
                count++;
            }

            if (skillPool != null && skillPool.HasSkillByKey(SkillPoolSlotKeys.Active2))
            {
                count++;
            }

            if (skillPool != null && skillPool.HasSkillByKey(SkillPoolSlotKeys.Active3))
            {
                count++;
            }

            return count;
        }
    }

    public SkillPoolSlotData GetEntryBySlot(int slotIndex)
    {
        EnsureSkillPoolService();
        return skillPoolService.GetEntryBySlot(skillPool, slotIndex);
    }

    public EquipmentSkillRuntimeData GetRuntimeBySlot(int slotIndex)
    {
        EnsureSkillPoolService();
        return skillPoolService.GetRuntimeBySlot(skillPool, slotIndex);
    }

    public EquipmentSkillRuntimeData GetBasicAttackRuntime()
    {
        EnsureSkillPoolService();
        return skillPoolService.GetBasicAttackRuntime(skillPool);
    }

    public SkillPoolSlotData[] GetActiveEntries()
    {
        EnsureSkillPoolService();
        return skillPoolService.GetActiveEntries(skillPool).ToArray();
    }

    public EquipmentSkillRuntimeData[] GetActiveRuntimes()
    {
        EnsureSkillPoolService();
        return skillPoolService.GetActiveRuntimes(skillPool).ToArray();
    }

    public SkillPoolSlotData[] GetAllEntries()
    {
        EnsureSkillPoolService();
        return skillPoolService.GetAllEntries(skillPool).ToArray();
    }

    public EquipmentSkillRuntimeData[] GetAllRuntimes()
    {
        EnsureSkillPoolService();
        return skillPoolService.GetAllRuntimes(skillPool).ToArray();
    }

    public bool HasAnyActiveSkill()
    {
        EnsureSkillPoolService();
        return skillPoolService.HasAnyActiveSkill(skillPool);
    }

    public bool HasBasicAttack()
    {
        EnsureSkillPoolService();
        return skillPoolService.HasBasicAttack(skillPool);
    }

    public void ResolveAllSkills()
    {
        EnsureSkillPoolService();
        skillPoolService.ResolvePool(skillPool);
    }

    public void RefreshSlotRuntime(int slotIndex)
    {
        EnsureSkillPoolService();
        skillPoolService.RefreshSlotRuntime(skillPool, slotIndex);
    }

    public void ClearAllRuntimeData()
    {
        EnsureSkillPoolService();
        skillPoolService.ClearResolvedRuntimeData(skillPool);
    }

    private void EnsureSkillPoolService()
    {
        if (skillPoolService != null)
        {
            return;
        }

        skillPoolService = new SkillPoolService();
    }
}
