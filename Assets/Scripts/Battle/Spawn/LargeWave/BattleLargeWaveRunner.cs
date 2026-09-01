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

        public BattleLargeWaveRunner(BattleLargeWavePolicySO policy, ISpawnUnitResolver resolver)
        {
            this.policy = policy;
            this.resolver = resolver;
        }

        public bool IsCommitted { get; private set; }
        public bool HasFailed { get; private set; }
        public int EmittedCount => emittedTokens.Count;
        public int PendingCount => IsCommitted ? Episode1SoloLargeWaveManifest.TotalCount - EmittedCount : 0;
        public int LivingCount => livingInstanceIds.Count;
        public bool IsTerminalReadyNextFrame => terminalReadyFrame >= 0 && Time.frameCount > terminalReadyFrame;

        public void Tick(float deltaTime)
        {
            if (HasFailed || IsTerminalReadyNextFrame) return;
            elapsed += Mathf.Max(0f, deltaTime);

            if (!IsCommitted && elapsed >= policy.ReservationCommitTime)
            {
                if (!Episode1SoloLargeWaveManifest.TryCreate(policy, out reservations, out string error) || !ResolveAllUnits())
                {
                    HasFailed = true;
                    Debug.LogError($"[BattleLargeWaveRunner] Exact1 pilot disabled before commit: {error}");
                    return;
                }
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

        private bool ResolveAllUnits()
        {
            if (resolver == null) return false;
            for (int i = 0; i < reservations.Count; i++)
            {
                CharacterSO character = resolver.Resolve(new SpawnUnitRequest(reservations[i].UnitKey, SpawnUnitRole.Melee));
                if (character == null) return false;
            }
            return true;
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
            if (IsCommitted && EmittedCount == Episode1SoloLargeWaveManifest.TotalCount && PendingCount == 0 && LivingCount == 0 && terminalReadyFrame < 0)
            {
                terminalReadyFrame = Time.frameCount;
            }
        }
    }
}
