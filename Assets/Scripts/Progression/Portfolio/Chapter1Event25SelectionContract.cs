namespace Progression.Portfolio
{
    /// <summary>
    /// Canonical selector identities shared by the original Event18 encounter and Event25.
    /// Both descriptors must use <see cref="BattleReuseExclusionGroup"/> so a single run
    /// cannot select both encounters backed by the same battle.
    /// </summary>
    public static class Chapter1Event25SelectionContract
    {
        public const string OriginalEvent18Id =
            "event.act1.random_event.18.reverse_flowing_stream";
        public const string Event25Id =
            "event.act1.random_event.25.hot_spring_beneath_ice";
        public const string BattleId = "battle.act1.event18.water_theft_guard";
        public const string BattleReuseExclusionGroup =
            "battle.exclusive.act1.event18.water_theft_guard";
        public const string RelicPoolId =
            "relic_pool.act1.chapter01.random_event.standard.v1";

        public static PortfolioEventDescriptor CreateOriginalEvent18() => new(
            OriginalEvent18Id,
            PortfolioPurpose.World,
            "water",
            BattleReuseExclusionGroup);

        public static PortfolioEventDescriptor CreateEvent25() => new(
            Event25Id,
            PortfolioPurpose.Relic,
            "ice_hot_spring",
            BattleReuseExclusionGroup);
    }
}
