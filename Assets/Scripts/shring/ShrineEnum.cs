namespace Shrine
{
    /// <summary>
    /// 신전에서 선택 가능한 신 종류.
    /// </summary>
    public enum ShrineGodType
    {
        None = 0,

        Life,
        War,
        Greed,
        Dark
    }

    /// <summary>
    /// 신전 메인 선택 타입.
    /// </summary>
    public enum ShrineActionType
    {
        None = 0,

        HealAndBless,
        Pray,
        Donate,
        Leave
    }

    /// <summary>
    /// 축복 카테고리.
    /// </summary>
    public enum ShrineBlessingCategory
    {
        None = 0,

        Attack,
        Defense,
        Growth,
        Strategy,
        Utility,
        Special
    }

    /// <summary>
    /// 축복 효과 타입.
    /// 실제 스탯 시스템 연결 전까지 공용 처리용.
    /// </summary>
    public enum ShrineBlessingEffectType
    {
        None = 0,

        AttackPowerPercent,
        AttackSpeedPercent,
        CriticalChancePercent,
        BossDamagePercent,

        DamageReductionPercent,
        MaxHpPercent,
        StatusResistancePercent,
        LowHpDefensePercent,

        ExpGainPercent,
        GoldGainPercent,
        RelicDropPercent,

        AiReactionSpeedPercent,
        CooldownReductionPercent,
        ConsumableEffectPercent,
        StartBattleShield,

        Special
    }

    /// <summary>
    /// 신앙 단계 상태.
    /// </summary>
    public enum FaithStageState
    {
        None = 0,

        Normal,
        Influenced,
        Locked,
        Devoted,
        Successor
    }

    /// <summary>
    /// 신앙 전용 미션 타입.
    /// </summary>
    public enum ShrineMissionType
    {
        None = 0,

        KillMonster,
        KillElite,
        KillBoss,

        KeepHighHp,
        KeepLowHp,

        SpendGold,
        HoldGold,

        CollectRelics,
        BuyItems,

        Reputation,
        PartyDeathVictory
    }

    /// <summary>
    /// 신전 메타 진행 키.
    /// </summary>
    public enum ShrineProgressKey
    {
        None = 0,

        ShopVisitCount = 100,
        BattleWinWithDeadMember = 200,
        ChaosEventClear = 300,
    }

    /// <summary>
    /// 신전 내부 진행 상태.
    /// </summary>
    public enum ShrineFlowState
    {
        None = 0,

        Enter,
        MainSelection,

        BlessingSelection,

        GodSelection,
        FaithActionSelection,

        Reward,
        Complete
    }
}
