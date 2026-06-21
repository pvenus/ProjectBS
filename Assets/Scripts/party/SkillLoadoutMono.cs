using UnityEngine;
using Skill;
using Character;

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


    public SkillPoolSlotData[] GetActiveEntries()
    {
        EnsureSkillPoolService();
        return skillPoolService.GetActiveEntries(skillPool).ToArray();
    }


    public SkillPoolSlotData[] GetAllEntries()
    {
        EnsureSkillPoolService();
        return skillPoolService.GetAllEntries(skillPool).ToArray();
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

    public void ApplyOverride(SkillPoolOverrideSO overrideSo)
    {
        if (overrideSo == null || skillPool == null)
        {
            return;
        }

        foreach (SkillPoolOverrideEntry entry in overrideSo.overrides)
        {
            if (entry == null || string.IsNullOrEmpty(entry.slotKey))
            {
                continue;
            }

            SkillPoolSlotData slot = skillPool.GetSlotByKey(entry.slotKey);

            if (slot == null)
            {
                SkillPoolSlotData newSlot = new SkillPoolSlotData();
                newSlot.Configure(
                    entry.slotKey,
                    entry.skillSo);

                skillPool.AddSlot(newSlot);

                continue;
            }

            slot.SetSkill(entry.skillSo);
        }
    }

    public void ApplyOverrideFromCharacter(CharacterSO characterSo)
    {
        if (characterSo == null)
        {
            return;
        }

        ApplyOverride(characterSo.SkillOverrideSet);
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
