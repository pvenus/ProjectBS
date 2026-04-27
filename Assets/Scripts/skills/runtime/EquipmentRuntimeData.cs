using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EquipmentSkillInstanceData
{
    [Header("Identity")]
    public string equipmentId;

    [Header("Progression")]
    public EquipmentGrade currentGrade = EquipmentGrade.Common;
    public int currentRuneSlotCount = 1;
    public int upgradeLevel = 0;

    [Header("Elements")]
    public ElementType mainElement = ElementType.None;
    public List<ElementType> subElements = new();

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
    public AttackArchetype attackArchetype;
    public EquipmentGrade resolvedGrade;
    public int resolvedRuneSlotCount;
    public int resolvedProjectileCount;
    public float resolvedProjectileScale;

    [Header("Resolved Elements")]
    public ElementType mainElement = ElementType.None;
    public ElementType[] subElements;

    [Header("Resolved SO References")]
    public SkillCastSO castSo;
    public SkillDamageSO damageSo;
    public SkillHitSO hitSo;
    public SkillMoveSO moveSo;
    public SkillVisualSetSO visualSetSo;

    [Header("Resolved Runtime Context")]
    public ResolvedVisualContextDto visualContext;
    public RuneRuntimeSetData runeRuntimeSet;
    public EffectRuntimeSetData effectRuntimeSet;
    public EquipmentUpgradeRuntimeData upgradeRuntimeData;
    public ProjectileEntity projectilePrefab;
}