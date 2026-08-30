namespace Progression.Portfolio
{
    /// <summary>
    /// Canonical selector identities for the original Event01 encounter and Event37.
    /// The shared exclusion group prevents the same battle from appearing twice in one run.
    /// </summary>
    public static class Chapter1Event37SelectionContract
    {
        public const string OriginalEvent01Id =
            "event.act1.random_event.01.name_swallowing_well";
        public const string Event37Id =
            "event.act1.random_event.37.nameless_long_sword_in_rain";
        public const string BattleId = "battle.act1.event01.named_well_wraith";
        public const string RelicId = "item.relic.ember_necklace";
        public const string BattleReuseExclusionGroup =
            "battle_reuse.exclusive.event01_event37";

        public static PortfolioEventDescriptor CreateOriginalEvent01() => new(
            OriginalEvent01Id,
            PortfolioPurpose.World,
            "named_well",
            BattleReuseExclusionGroup);

        public static PortfolioEventDescriptor CreateEvent37() => new(
            Event37Id,
            PortfolioPurpose.Relic,
            "nameless_sword_rain",
            BattleReuseExclusionGroup);
    }
}
