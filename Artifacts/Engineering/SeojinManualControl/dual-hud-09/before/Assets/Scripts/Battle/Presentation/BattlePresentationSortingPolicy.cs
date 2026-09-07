namespace Battle
{
    /// <summary>
    /// Shared world-space presentation ordering for battle-floor renderers.
    /// Screen-space HUD canvases keep their own sorting policy.
    /// </summary>
    public static class BattlePresentationSortingPolicy
    {
        public const int Background = -1000;
        public const int GroundTelegraph = Background + 1;

        // Exact MORPG route only. Default layer: background < back props < Y-sorted bodies
        // < occluding props < telegraphs < existing world health HUD (200+) < overlay rewards.
        public const int MorpgSortingLayerId = 0;
        public const int MorpgBackProps = -900;
        public const int MorpgBodyCenter = -400;
        public const int MorpgForegroundProps = 50;
        public const int MorpgGroundTelegraph = 100;
        public static int MorpgBodyOrder(int legacyOrder) => MorpgBodyCenter +
            System.Math.Max(-400, System.Math.Min(400, legacyOrder / 2));
    }
}
