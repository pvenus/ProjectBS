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

    [Header("Runtime Resources")]
    public ProjectileEntity projectilePrefab;

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
    public SkillType skillType;
    public AttackArchetype attackArchetype;
    public bool skipAttackAnimation;
    public EquipmentGrade resolvedGrade;
    public int resolvedRuneSlotCount;
    public int resolvedProjectileCount;
    public float resolvedProjectileSpreadAngle;
    public float resolvedProjectileScale;
    [Header("Resolved SO References")]
    public SkillCastSO castSo;
    public SkillHitSO[] hitSos;
    public SkillMoveSO moveSo;
    public SpawnSkillSO spawnSkillSo;
    public SkillVisualSetSO visualSetSo;

    [Header("Resolved Runtime Context")]
    public ResolvedVisualContextDto visualContext;
    public RuneRuntimeSetData runeRuntimeSet;
    public EffectRuntimeSetData effectRuntimeSet;
    public EquipmentUpgradeRuntimeData upgradeRuntimeData;
    public ProjectileEntity projectilePrefab;
}