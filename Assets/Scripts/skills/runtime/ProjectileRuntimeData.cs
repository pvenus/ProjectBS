

using UnityEngine;
using Skills.Dto;

/// <summary>
/// 실제 투사체가 런타임에서 사용하는 최종 데이터 묶음.
/// 원형 SO, 룬, 업그레이드, Resolver 결과를 다 반영한 뒤
/// 투사체 Mono에 주입되는 순수 런타임 컨텍스트다.
/// </summary>
[System.Serializable]
public class ProjectileRuntimeData
{
    [Header("Ownership")]
    public GameObject owner;
    public GameObject target;

    [Header("Spawn")]
    public Vector2 spawnPosition;
    public Vector2 direction;

    [Header("Runtime Profiles")]
    public SkillProjectileMoveDto move;
    public SkillProjectileHitDto hit;
    public SkillDamageProfileDto damageProfile;

    [Header("Lifetime")]
    public float lifetime = 3f;

    [Header("Projectile Common")]
    public ProjectileEntity projectilePrefab;
    public int projectileCount = 1;
    public int spawnOrder = 0;
    public float projectileScale = 1f;

    [Header("Visual")]
    public ResolvedVisualContextDto visualContext;

    [Header("Effects")]
    public EffectRuntimeSetData effectRuntimeSet;

    // --- Resolved Visual Runtime (filled by resolver) ---
    [Header("Resolved Visual Runtime")]
    public AnimationClip spawnClip;
    public AnimationClip hitClip;
    public AnimationClip despawnClip;

    public Sprite sprite;
    public Material material;
    public Color color = Color.white;

    // Optional: if true, prefer Animator triggers over direct clips
    public bool useAnimatorTriggers;

    /// <summary>
    /// owner 기준 forward가 없는 2D 환경에서 direction이 비어 있으면 fallback 판단용.
    /// </summary>
    public bool HasDirection => direction.sqrMagnitude > 0.0001f;

    public Vector2 NormalizedDirection => HasDirection ? direction.normalized : Vector2.right;
}

/// <summary>
/// Resolver가 계산한 최종 비주얼 컨텍스트.
/// 실제 리소스 참조는 별도 VisualResolver/Assembler가 이 값을 보고 찾아 적용한다.
/// </summary>
[System.Serializable]
public class ResolvedVisualContextDto
{
    [Header("Base Context")]
    public AttackArchetype attackArchetype;
    public EquipmentGrade equipmentGrade;

    [Header("Element Context")]
    public ElementType mainElement = ElementType.None;
    public ElementType[] subElements;

    [Header("Optional Runtime Keys")]
    public string baseVisualId;
    public string mainVisualId;
}