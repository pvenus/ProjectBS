


using UnityEngine;

/// <summary>
/// 장비 = 스킬의 기본 정체성 데이터를 정의하는 Profile SO.
/// 장비의 등급, 룬 슬롯, 공격 원형, 투사체 생성 리소스처럼
/// 장비 자체에 귀속되는 기본값을 한곳에 모아 관리한다.
/// </summary>
[CreateAssetMenu(fileName = "EquipmentBaseProfileSO", menuName = "BS/Skills/Equipment/EquipmentBaseProfileSO")]
public class EquipmentBaseProfileSO : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private EquipmentGrade baseGrade = EquipmentGrade.Common;
    [SerializeField, Min(1)] private int baseRuneSlotCount = 1;
    [SerializeField] private AttackArchetype attackArchetype = AttackArchetype.None;

    [Header("Projectile Resource")]
    [SerializeField] private ProjectileEntity projectilePrefab;
    [SerializeField] private float projectileSpawnOffset = 0f;

    [Header("Projectile Base Stats")]
    [SerializeField, Min(1)] private int projectileCount = 1;
    [SerializeField] private float projectileScale = 1f;
    [SerializeField] private float projectileLifetime = 3f;

    [Header("Base Damage")]
    [SerializeField] private DamageType damageType = DamageType.Normal;
    [SerializeField] private float baseDamage = 10f;
    [SerializeField] private float flatBonusDamage = 0f;
    [SerializeField] private bool canCritical = true;
    [SerializeField] private float criticalMultiplier = 1.5f;
    [SerializeField] private bool ignoreDefense = false;

    public EquipmentGrade BaseGrade => baseGrade;
    public int BaseRuneSlotCount => baseRuneSlotCount;
    public AttackArchetype AttackArchetype => attackArchetype;
    public ProjectileEntity ProjectilePrefab => projectilePrefab;
    public float ProjectileSpawnOffset => projectileSpawnOffset;
    public int ProjectileCount => Mathf.Max(1, projectileCount);
    public float ProjectileScale => Mathf.Max(0.01f, projectileScale);
    public float ProjectileLifetime => Mathf.Max(0.01f, projectileLifetime);

    public DamageType DamageType => damageType;
    public float BaseDamage => baseDamage;
    public float FlatBonusDamage => flatBonusDamage;
    public bool CanCritical => canCritical;
    public float CriticalMultiplier => criticalMultiplier;
    public bool IgnoreDefense => ignoreDefense;
}