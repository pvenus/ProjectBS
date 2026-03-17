using System.Collections.Generic;
using UnityEngine;

public class SkillLoadoutMono : MonoBehaviour
{
    [Header("Loadout")]
    [SerializeField] private ScriptableObject basicAttackSkill;
    [SerializeField] private ScriptableObject skill1;
    [SerializeField] private ScriptableObject skill2;
    [SerializeField] private ScriptableObject skill3;

    public ScriptableObject BasicAttackSkill => basicAttackSkill;
    public ScriptableObject Skill1 => skill1;
    public ScriptableObject Skill2 => skill2;
    public ScriptableObject Skill3 => skill3;

    public int ActiveSkillCount
    {
        get
        {
            int count = 0;
            if (skill1 != null) count++;
            if (skill2 != null) count++;
            if (skill3 != null) count++;
            return count;
        }
    }

    public ScriptableObject GetSkillBySlot(int slotIndex)
    {
        switch (slotIndex)
        {
            case 0: return basicAttackSkill;
            case 1: return skill1;
            case 2: return skill2;
            case 3: return skill3;
            default: return null;
        }
    }

    public ScriptableObject GetBasicAttack()
    {
        return basicAttackSkill;
    }

    public ScriptableObject[] GetActiveSkills()
    {
        List<ScriptableObject> skills = new List<ScriptableObject>(3);

        if (skill1 != null) skills.Add(skill1);
        if (skill2 != null) skills.Add(skill2);
        if (skill3 != null) skills.Add(skill3);

        return skills.ToArray();
    }

    public ScriptableObject[] GetAllSkills()
    {
        List<ScriptableObject> skills = new List<ScriptableObject>(4);

        if (basicAttackSkill != null) skills.Add(basicAttackSkill);
        if (skill1 != null) skills.Add(skill1);
        if (skill2 != null) skills.Add(skill2);
        if (skill3 != null) skills.Add(skill3);

        return skills.ToArray();
    }

    public bool HasAnyActiveSkill()
    {
        return skill1 != null || skill2 != null || skill3 != null;
    }

    public bool HasBasicAttack()
    {
        return basicAttackSkill != null;
    }

    public void SetBasicAttack(ScriptableObject skill)
    {
        basicAttackSkill = skill;
    }

    public void SetActiveSkill(int slotIndex, ScriptableObject skill)
    {
        switch (slotIndex)
        {
            case 1:
                skill1 = skill;
                break;
            case 2:
                skill2 = skill;
                break;
            case 3:
                skill3 = skill;
                break;
            default:
                Debug.LogWarning($"[SkillLoadout] Invalid active skill slot index={slotIndex} on {name}");
                break;
        }
    }

    public void ClearAllSkills()
    {
        basicAttackSkill = null;
        skill1 = null;
        skill2 = null;
        skill3 = null;
    }
}
