using UnityEngine;
using Skill;
[System.Serializable]
public class EquipmentSkillInstanceData
{
    [Header("Identity")]
    public string equipmentId;

    [Header("Progression")]
    public int currentLevel = 1;
    public int upgradeLevel = 0;
}

[System.Serializable]
public class EquipmentSkillRuntimeData
{
    [Header("Source")]
    public EquipmentSkillSO sourceEquipment;
    public EquipmentSkillInstanceData instanceData;

    [Header("Resolved Identity")]
    public bool skipAttackAnimation;
    public int resolvedLevel;
    public float resolvedRange;

    public int resolvedBurstCount = 1;
    public float resolvedBurstInterval;

    public int resolvedProjectileCount;
    public float resolvedProjectileSpreadAngle;
    public ProjectileArrangementType resolvedProjectileArrangement = ProjectileArrangementType.Single;
    public float resolvedProjectileArrangementValue;
    public float resolvedProjectileScale;

    [Header("Resolved Runtime Context")]
    public ResolvedVisualContextDto visualContext;
    public EquipmentUpgradeRuntimeData upgradeRuntimeData;
}