namespace Skill
{
    public enum SkillType
    {
        Active = 0,
        Passive = 1
    }

    public enum EffectType
    {
        Projectile = 0,
        Spawn = 1
    }

    public enum ProjectileMoveType
    {
        None = 0,
        Linear = 1,
        Warp = 2,
        Hover = 3,
        Orbit = 4,
        Homing = 5
    }
    public enum SkillStatModifierOperationType
    {
        Flat = 0,
        Percent = 1,
        Override = 2
    }

    /// <summary>
    /// 수정 대상 스탯 타입
    /// </summary>
    public enum SkillStatModifierType
    {
        BaseDamage = 0,
        AttackPercentDamage = 1,

        Cooldown = 2,
        Range = 3,
        SplitHitCount = 4,

        ProjectileCount = 5,
        ProjectileSpreadAngle = 6,
        ProjectileScale = 7,
        Lifetime = 8,

        ProjectileSpawnInterval = 9,
        ProjectileSpawnRadius = 10,
        ProjectileColliderRadius = 11
    }
}
