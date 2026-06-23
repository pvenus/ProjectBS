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
        HpRegenMaxHpPercent = 1120,

        BleedDamagePerSecond = 1140,

        Attack = 1200,
        AttackPercent = 1210,
        AttackSpeed = 1220,
        AttackSpeedPercent = 1230,

        MoveSpeed = 1235,
        MoveSpeedPercent = 1237,

        CritChance = 1240,
        CritDamage = 1250,

        LifeSteal = 1255,
        LifeStealPercent = 1257,

        BossDamagePercent = 1260,
        EliteDamagePercent = 1270,

        EliteApproachMoveSpeedPercent = 1275,

        LowHpAttackBonus = 1280,

        MissingHpAttackPercent = 1290,
        MissingHpFinalDamageAmplify = 1295,

        FinalDamageAmplify = 1297,

        Defense = 1300,

        LowHpDefenseBonus = 1370,

        SurroundedAttackPercent = 1380,
        SurroundedDamageReductionPercent = 1390,

        ReflectDamagePercent = 1325,

        StatusResistance = 1330,
        StatusResistancePercent = 1340,


        StunDuration = 1345,

        RootDuration = 1347,

        Shield = 1350,
        ShieldPercent = 1360,

        GoldGain = 1400,

        ExpGain = 1420,

        Level = 1430,
        Experience = 1431,

        DropGold = 1435,
        BonusGoldDropChance = 1436,
        BonusGoldDropPercent = 1438,
        DropExp = 1437,

        RelicDropRate = 1440,
        RelicDropRatePercent = 1450,


        EliteGoldBonus = 1460,
        BattleEndBonusGold = 1470,

        MaxOwnedGold = 1480,
        MaxOwnedGoldAttackPercent = 1490,

        GoldInterestPercent = 1495,

        BattleEndGoldInterestPercent = 1497,

        KillStack = 1590,
        KillStackAttackPercent = 1592,
        KillStackAttackPercentAmplify = 1595,

        KillCount = 1600,
        EliteKillCount = 1610,
        BossKillCount = 1620,

        AiReactionSpeed = 1500,
        AiReactionSpeedPercent = 1510,

        CooldownReduction = 1520,

        ConsumableEffectiveness = 1540,
        ConsumableEffectivenessPercent = 1550,

        StartBattleShield = 1560,

        ResurrectionToken = 1565,

        SkillRange = 1570,
        SkillRangePercent = 1580,


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