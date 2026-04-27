using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 장비 = 스킬의 원형 데이터를 정의하는 최상위 허브 SO.
/// 이 객체는 절대 변하지 않는 설계도 역할만 담당하며,
/// 기본 Effect / 룬 / 업그레이드 / 최종 비주얼 / 최종 스탯은 별도 Resolver에서 해석한다.
/// </summary>
[CreateAssetMenu(fileName = "EquipmentSkillSO", menuName = "BS/Skills/Equipment/EquipmentSkillSO")]
public class EquipmentSkillSO : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string equipmentId;
    [SerializeField] private string displayName;
    [SerializeField, TextArea] private string description;
    [SerializeField] private Sprite icon;

    [Header("Base Profile")]
    [SerializeField] private EquipmentBaseProfileSO baseProfileSo;

    [Header("Core Profiles")]
    [SerializeField] private SkillCastSO castSo;
    [SerializeField] private SkillDamageSO damageSo;
    [SerializeField] private SkillHitSO hitSo;
    [SerializeField] private SkillMoveSO moveSo;

    [Header("Visual")]
    [SerializeField] private SkillVisualSetSO visualSetSo;

    [Header("Upgrade")]
    [SerializeField] private EquipmentUpgradeTableSO upgradeTableSo;

    [Header("Effects")]
    [SerializeField] private List<SkillEffectSO> effects = new();

    public string EquipmentId => equipmentId;
    public string DisplayName => displayName;
    public string Description => description;
    public Sprite Icon => icon;

    public EquipmentBaseProfileSO BaseProfileSo => baseProfileSo;
    public AttackArchetype AttackArchetype => baseProfileSo != null ? baseProfileSo.AttackArchetype : AttackArchetype.Melee;
    public EquipmentGrade BaseGrade => baseProfileSo != null ? baseProfileSo.BaseGrade : EquipmentGrade.Common;
    public int BaseRuneSlotCount => baseProfileSo != null ? baseProfileSo.BaseRuneSlotCount : 1;

    public ProjectileEntity ProjectilePrefab => baseProfileSo != null ? baseProfileSo.ProjectilePrefab : null;
    public float ProjectileSpawnOffset => baseProfileSo != null ? baseProfileSo.ProjectileSpawnOffset : 0f;

    public SkillCastSO CastSo => castSo;
    public SkillDamageSO DamageSo => damageSo;
    public SkillHitSO HitSo => hitSo;
    public SkillMoveSO MoveSo => moveSo;
    public SkillVisualSetSO VisualSetSo => visualSetSo;
    public EquipmentUpgradeTableSO UpgradeTableSo => upgradeTableSo;
    public IReadOnlyList<SkillEffectSO> Effects => effects;
    public bool HasAnyEffect => effects != null && effects.Count > 0;

}