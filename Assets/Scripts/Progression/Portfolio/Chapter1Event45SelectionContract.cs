namespace Progression.Portfolio
{
    public static class Chapter1Event45SelectionContract
    {
        public const string Event45Id =
            "event.act1.random_event.45.false_wildfire_boundary_stones";
        public const string NodeId =
            "node.act1.random_event.45.false_wildfire_boundary_stones.intro";
        public const string PositiveFlagId =
            "runflag.act1.chapter01.event45.seized_land_boundary_restored";
        // Keep the cross-assembly contract as its serialized numeric value. The
        // Progression assembly intentionally does not reference the Stage assembly.
        public const int SelectionModeValue = 30;
    }
}
