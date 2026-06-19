using UnityEngine;
using System.Collections.Generic;
using Skill;
[System.Serializable]
public class EquipmentSkillInstanceData
{
    [Header("Identity")]
    public string equipmentId;

    [Header("Progression")]
    public EquipmentGrade currentGrade = EquipmentGrade.Common;
    public int currentRuneSlotCount = 1;
    public int upgradeLevel = 0;

    [Header("Runes")]
    public List<RuneSO> equippedRunes = new();


    [Header("Optional Overrides")]
    public float projectileLifetimeOverride = -1f;
}

[System.Serializable]
public class EquipmentSkillRuntimeData
{
    [Header("Source")]
    public EquipmentSkillSO sourceEquipment;
    public EquipmentSkillInstanceData instanceData;

    [Header("Resolved Identity")]
    public bool skipAttackAnimation;
    public EquipmentGrade resolvedGrade;

    public int resolvedBurstCount = 1;
    public float resolvedBurstInterval;

    public int resolvedProjectileCount;
    public float resolvedProjectileSpreadAngle;
    public ProjectileArrangementType resolvedProjectileArrangement = ProjectileArrangementType.Single;
    public float resolvedProjectileArrangementValue;
    public float resolvedProjectileScale;

    [Header("Resolved Runtime Context")]
    public ResolvedVisualContextDto visualContext;
    public RuneRuntimeSetData runeRuntimeSet;
    public EffectRuntimeSetData effectRuntimeSet;
    public EquipmentUpgradeRuntimeData upgradeRuntimeData;
}