namespace Stat
{
    public enum StatType
    {
        None = 0,

        Reputation = 100,
        Fame = 200,
        Notoriety = 300,

        MaxHp = 1000,
        MaxHpPercent = 1010,

        Hp = 1100,

        HpRegen = 1110,
        HpRegenPercent = 1120,
        MaxHpRegenPercentPerSecond = 1130,

        Attack = 1200,
        AttackPercent = 1210,
        AttackSpeed = 1220,
        AttackSpeedPercent = 1230,

        MoveSpeed = 1235,
        MoveSpeedPercent = 1237,

        CritChance = 1240,
        CritDamage = 1250,

        BossDamagePercent = 1260,
        EliteDamagePercent = 1270,

        LowHpAttackBonus = 1280,

        MissingHpAttackPercent = 1290,
        MissingHpFinalDamageAmplify = 1295,

        Defense = 1300,
        DefensePercent = 1310,

        DamageReductionPercent = 1320,

        StatusResistance = 1330,
        StatusResistancePercent = 1340,

        StunDuration = 1345,

        Shield = 1350,
        ShieldPercent = 1360,

        LowHpDefenseBonus = 1370,

        GoldGain = 1400,
        GoldGainPercent = 1410,

        ExpGain = 1420,
        ExpGainPercent = 1430,

        DropGold = 1435,
        DropExp = 1437,

        RelicDropRate = 1440,
        RelicDropRatePercent = 1450,


        EliteGoldBonus = 1460,
        BattleEndBonusGold = 1470,

        MaxOwnedGold = 1480,
        MaxOwnedGoldAttackPercent = 1490,

        GoldInterestPercent = 1495,

        AiReactionSpeed = 1500,
        AiReactionSpeedPercent = 1510,

        CooldownReduction = 1520,
        CooldownReductionPercent = 1530,

        ConsumableEffectiveness = 1540,
        ConsumableEffectivenessPercent = 1550,

        StartBattleShield = 1560,

        SkillRange = 1570,
        SkillRangePercent = 1580,

        KillStack = 1590,
        KillStackAttackPercent = 1592,
        KillStackAttackPercentAmplify = 1595,

        KillCount = 1600,
        EliteKillCount = 1610,
        BossKillCount = 1620,

        LifeFaithLevel = 2000,
        WarFaithLevel = 2100,
        GreedFaithLevel = 2200,
        DarkFaithLevel = 2300,

        LifeAffinity = 3000,
        WarAffinity = 3100,
        GreedAffinity = 3200,
        DarkAffinity = 3300,
    }
}