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
        Neutral = 0,
        Buff = 1,
        Debuff = 2
    }
}
