namespace Effect
{
    public enum EffectSourceType
    {
        None = 0,

        Relic = 100,
        Bless = 200,
        Buff = 300,
        Skill = 400,
        Shrine = 500,
        Event = 600,
        Equipment = 700,
    }

    public enum HealTriggerTargetType
    {
        AnyAlly = 0,
        Self = 1,
        OtherAlly = 2,
        Party = 3
    }
    public enum StatModifierType
    {
        Flat = 0,
        Percent = 100,
        Multiply = 200,
    }

    public enum EffectLifetimeType
    {
        Instant,
        Manual,
        CombatOnly,
        Timed,
        CombatTimed,
        ConsumeOnBattleStart,
        ConsumeOnBattleEnd
    }

    public enum EffectCategoryType
    {
        Buff = 0,
        Debuff = 1
    }
    public enum EffectType
    {
        None = 0,

        StatModifier = 100,
        Heal = 200,
        Knockback = 300,
        CooldownReduce = 400,
        ChanceOnHitStatModifier = 500,
        ChanceOnHealStatModifier = 600,
        ChanceOnHealCooldownReduce = 700,
        AttackBleed = 800,
        ChanceOnHitSkill = 900,
        Taunt = 1000,
    }

    public enum CooldownReduceType
    {
        Percent = 0,
        FlatSeconds = 1,
        PercentAndFlat = 2
    }
    public enum KnockbackDirectionType
    {
        /// <summary>
        /// 중심에서 바깥 방향으로 밀어냄.
        /// </summary>
        PushAwayFromSource = 0,

        /// <summary>
        /// 중심점 방향으로 끌어당김.
        /// </summary>
        PullToSource = 1,

        ProjectileDirection = 2,
        CustomDirection = 3,
    }
    /// <summary>
    /// Effect 내부에서 수정할 필드
    /// </summary>
    public enum EffectModifierFieldType
    {
        Value = 0,
        Duration = 1,
        Chance = 2,
        Cooldown = 3,
        MaxApplyCount = 4,
        TickInterval = 5,
        Radius = 6
    }

}
