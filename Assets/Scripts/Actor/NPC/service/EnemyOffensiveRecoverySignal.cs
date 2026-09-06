namespace Npc.Service
{
    public enum EnemyOffensiveRecoveryState
    {
        Ready,
        Windup,
        Recovery,
        Cooldown,
        Disabled
    }

    public readonly struct EnemyOffensiveRecoverySignal
    {
        public EnemyOffensiveRecoverySignal(
            EnemyOffensiveRecoveryState state,
            bool inEffectiveRange,
            float remainingCooldown)
        {
            State = state;
            InEffectiveRange = inEffectiveRange;
            RemainingCooldown = remainingCooldown;
        }

        public EnemyOffensiveRecoveryState State { get; }
        public bool InEffectiveRange { get; }
        public float RemainingCooldown { get; }
        public bool AllowsReposition =>
            State == EnemyOffensiveRecoveryState.Recovery ||
            State == EnemyOffensiveRecoveryState.Cooldown;
    }
}
