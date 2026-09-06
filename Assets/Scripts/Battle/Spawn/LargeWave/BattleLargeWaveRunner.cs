using System.Collections.Generic;
using Character;
using UnityEngine;

namespace Battle
{
    public sealed class BattleLargeWaveRunner
    {
        private readonly BattleLargeWavePolicySO policy;
        private readonly ISpawnUnitResolver resolver;
        private IReadOnlyList<LargeWaveReservation> reservations;
        private readonly HashSet<int> emittedTokens = new();
        private readonly HashSet<int> livingInstanceIds = new();
        private float elapsed;
        private int nextReservation;
        private int terminalReadyFrame = -1;

        private BattleLargeWaveRunner(BattleLargeWavePolicySO policy, ISpawnUnitResolver resolver,
            IReadOnlyList<LargeWaveReservation> preflightReservations)
        {
            this.policy = policy;
            this.resolver = resolver;
            reservations = preflightReservations;
        }

        public static bool TryCreate(BattleLargeWavePolicySO policy, ISpawnUnitResolver resolver,
            out BattleLargeWaveRunner runner, out string error)
        {
            runner = null;
            if (policy == null || policy.PolicyId !=
                "seq.act1.chapter01.01.rescue_villagers.large_map.three_stage.v3")
            {
                error = "unsupported large-wave placement policy";
                return false;
            }
            if (!BattleLargeWavePlacementV3.TryLoadAndResolve(resolver, out var profile,
                    out IReadOnlyList<LargeWaveReservation> rows, out error)) return false;
            BattleMapBoundsContext.Activate(new Vector2(profile.map.x, profile.map.y),
                profile.boundary.movementInset);
            runner = new BattleLargeWaveRunner(policy, resolver, rows);
            return true;
        }

        public bool IsCommitted { get; private set; }
        public bool HasFailed { get; private set; }
        public int EmittedCount => emittedTokens.Count;
        public int PendingCount => IsCommitted ? reservations.Count - EmittedCount : 0;
        public int LivingCount => livingInstanceIds.Count;
        public bool IsTerminalReadyNextFrame => terminalReadyFrame >= 0 && Time.frameCount > terminalReadyFrame;

        public void Tick(float deltaTime)
        {
            if (HasFailed || IsTerminalReadyNextFrame) return;
            elapsed += Mathf.Max(0f, deltaTime);

            if (!IsCommitted && elapsed >= policy.ReservationCommitTime)
            {
                IsCommitted = true;
            }

            while (IsCommitted && nextReservation < reservations.Count && reservations[nextReservation].EmitTime <= elapsed)
            {
                Emit(reservations[nextReservation++]);
                if (HasFailed) return;
            }

            TryMarkTerminal();
        }

        public void NotifyEnemyDied(GameObject enemy)
        {
            if (enemy != null) livingInstanceIds.Remove(enemy.GetInstanceID());
            TryMarkTerminal();
        }

        private void Emit(LargeWaveReservation reservation)
        {
            if (livingInstanceIds.Count >= policy.HardLivingCap || !emittedTokens.Add(reservation.Token))
            {
                HasFailed = true;
                Debug.LogError("[BattleLargeWaveRunner] Exact1 living cap or duplicate reservation violation.");
                return;
            }

            CharacterSO character = resolver.Resolve(new SpawnUnitRequest(reservation.UnitKey, SpawnUnitRole.Melee));
            GameObject enemy = NpcSpawnService.Instance.SpawnNpc(character, reservation.Position, 0f, null);
            if (enemy == null)
            {
                HasFailed = true;
                return;
            }
            livingInstanceIds.Add(enemy.GetInstanceID());
        }

        private void TryMarkTerminal()
        {
            if (IsCommitted && EmittedCount == reservations.Count && PendingCount == 0 && LivingCount == 0 && terminalReadyFrame < 0)
            {
                terminalReadyFrame = Time.frameCount;
            }
        }
    }
}
