namespace Skill
{
    /// <summary>
    /// 스킬 시스템 전반에서 공통으로 사용하는 Enum 정의 모음
    /// </summary>

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
    /// 시전자 본체 이동 타입
    /// </summary>
    public enum CastMoveType
    {
        None = 0,
        DashForward,
        DashBackward,
        MoveToTarget,
        MoveAwayFromTarget
    }

    /// <summary>
    /// 자식 스킬 생성 시점
    /// </summary>
    public enum SpawnSkillTiming
    {
        None = 0,
        OnCast = 1,
        OnHit = 2,
        OnProjectileEnd = 3,
        OnInterval = 4
    }

    /// <summary>
    /// 자식 스킬 생성 위치
    /// </summary>
    public enum SpawnSkillPosition
    {
        Caster = 0,
        Target = 1,
        HitPoint = 2,
        ProjectilePosition = 3
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

        /// <summary>
        /// 자기 자신을 대상으로 한다.
        /// 타겟 탐색 없이 시전자 기준으로 즉시 발동.
        /// 예: 학익진, 자기 버프, 오라.
        /// </summary>
        Self = 1,

        /// <summary>
        /// 실제 타겟 오브젝트/좌표를 목표점으로 사용한다.
        /// 예: 단일 투사체 기본 공격, 타겟 위치에 직접 도달하는 스킬.
        /// </summary>
        AutoTarget = 2,

        /// <summary>
        /// 가까운 타겟은 방향 계산에만 사용하고,
        /// 실제 목표점은 시전자 위치 + 방향 * 사거리로 사용한다.
        /// 예: 화살파, 관통 직선기, 애쉬 W 계열.
        /// </summary>
        AutoTargetDirection = 3,

        /// <summary>
        /// 시전자 현재 방향 또는 입력 방향 기준으로 사거리 끝까지 발사한다.
        /// </summary>
        Directional = 4,

        /// <summary>
        /// 지정된 월드 좌표를 목표점으로 사용한다.
        /// 예: 장판, 워프형 위치 지정 스킬.
        /// </summary>
        Position = 5
    }
    public enum ImpactType
    {
        Hit,
        Explosion,
        Critical,
        Overheat,
        Special
    }
}