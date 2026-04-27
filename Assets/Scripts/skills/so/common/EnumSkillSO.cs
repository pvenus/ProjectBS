/// <summary>
/// 스킬 시스템 전반에서 공통으로 사용하는 Enum 정의 모음
/// </summary>

public enum ElementType
{
    None = 0,

    // 기본 5속성
    Fire,
    Ice,
    Wind,
    Lightning,

    // 무속성 (단일 강력 타입)
    Neutral
}

public enum AttackArchetype
{
    None = 0,
    Melee,
    Ranged,
    Magic
}

public enum EquipmentGrade
{
    Common = 0,
    Rare,
    Epic,
    Legendary
}

/// <summary>
/// 데미지 타입 (추후 확장용)
/// </summary>
public enum DamageType
{
    Normal = 0,
    Explosion,
    Dot,
    True
}

/// <summary>
/// 히트 판정 타입
/// </summary>
public enum HitShapeType
{
    Point = 0,
    Circle,
    Cone,
    Box
}

/// <summary>
/// 투사체 이동 타입
/// </summary>
public enum MoveType
{
    Straight = 0,
    Homing,
    Arc,
    Instant
}

/// <summary>
/// 스킬 발동 방식
/// </summary>
public enum CastType
{
    Instant = 0,
    Channeling,
    Charge
}

/// <summary>
/// 타겟팅 방식
/// </summary>
public enum TargetingType
{
    None = 0,
    AutoTarget,
    Directional,
    Position
}
public enum ImpactType
{
    Hit,
    Explosion,
    Critical,
    Overheat,
    Special
}