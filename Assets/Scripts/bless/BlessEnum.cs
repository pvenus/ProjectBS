

namespace Bless
{
    public enum BlessCategory
    {
        None = 0,

        Common = 100,

        Offense = 1000,
        Defense = 1100,
        Utility = 1200,
        Support = 1300,

        Economy = 2000,
        Survival = 2100,

        Special = 9000,
    }

    public enum BlessEffectType
    {
        None = 0,

        AttackPowerPercent = 1000,
        AttackSpeedPercent = 1010,
        CriticalChancePercent = 1020,
        BossDamagePercent = 1030,

        DamageReductionPercent = 2000,
        MaxHpPercent = 2010,
        StatusResistancePercent = 2020,
        LowHpDefensePercent = 2030,

        ExpGainPercent = 3000,
        GoldGainPercent = 3010,
        RelicDropPercent = 3020,
        ConsumableEffectPercent = 3030,

        AiReactionSpeedPercent = 4000,
        CooldownReductionPercent = 4010,
        StartBattleShield = 4020,

        Special = 9000,
    }
}