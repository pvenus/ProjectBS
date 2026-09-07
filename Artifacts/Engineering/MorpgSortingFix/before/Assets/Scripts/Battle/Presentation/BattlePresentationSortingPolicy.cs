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
    }
}
