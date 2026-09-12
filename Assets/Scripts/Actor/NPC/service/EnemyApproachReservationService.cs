using System.Collections.Generic;
using UnityEngine;

namespace Npc.Service
{
    /// <summary>
    /// Enemy-only, target-root approach slots. It never moves actors; NpcPathing owns movement.
    /// Leases make pooled/disabled actors self-healing even if teardown is interrupted.
    /// </summary>
    public sealed class EnemyApproachReservationService
    {
        public const int SectorCount = 12;
        public const int MaxReservationsPerTarget = 24;
        public const float LeaseDuration = .60f;
        public const float StaleLeaseDuration = .80f;
        public const float HeartbeatInterval = .20f;
        public const float MeleeSecondRingOffset = .55f;

        public readonly struct Result
        {
            public Result(bool valid, Vector2 point, int sector, int ring, bool laneMode)
            { Valid = valid; Point = point; Sector = sector; Ring = ring; LaneMode = laneMode; }
            public bool Valid { get; }
            public Vector2 Point { get; }
            public int Sector { get; }
            public int Ring { get; }
            public bool LaneMode { get; }
        }

        private sealed class Lease
        {
            public int ownerId;
            public int sector;
            public int ring;
            public float expiresAt;
        }

        private static readonly Dictionary<int, List<Lease>> ByTarget = new();
        private static readonly Dictionary<long, float> QuarantinedUntil = new();
        private int _targetId;
        private int _ownerId;
        private int _sector = -1;
        private int _ring;
        private float _nextHeartbeat;
        private int _blockedAttempts;

        public Result Resolve(
            Transform targetRoot,
            Vector2 self,
            float radius,
            int stableOwnerId,
            float time)
        {
            if (targetRoot == null || radius <= 0f)
            {
                Release();
                return default;
            }

            int targetId = StableActorKey(targetRoot);
            if (_targetId != targetId || _ownerId != stableOwnerId)
            {
                Release();
                _targetId = targetId;
                _ownerId = stableOwnerId;
                Acquire(targetRoot.position, self, time);
            }
            else if (_sector < 0) Acquire(targetRoot.position, self, time);
            else if (time >= _nextHeartbeat)
            {
                Heartbeat(time);
            }

            if (_sector < 0) return default;
            if (_blockedAttempts >= 20)
            {
                Vector2 axis = ((Vector2)targetRoot.position - self).normalized;
                if (axis.sqrMagnitude < .001f) axis = Vector2.right;
                Vector2 lateral = new(-axis.y, axis.x);
                float side = (_ownerId & 1) == 0 ? 1f : -1f;
                int queue = Mathf.Abs(_ownerId % 4);
                Vector2 lanePoint = (Vector2)targetRoot.position - axis * (radius + queue * .70f) + lateral * (.28f * side);
                return new Result(true, lanePoint, _sector, _ring, true);
            }
            float angle = _sector * (360f / SectorCount) * Mathf.Deg2Rad;
            float shrink = Mathf.Min(3, _blockedAttempts / 5) * .20f;
            float resolvedRadius = Mathf.Max(.48f, radius - shrink) + (_ring > 0 ? MeleeSecondRingOffset : 0f);
            Vector2 direction = new(Mathf.Cos(angle), Mathf.Sin(angle));
            return new Result(true, (Vector2)targetRoot.position + direction * resolvedRadius, _sector, _ring, false);
        }

        public void ReportBlocked(float time)
        {
            if (_targetId == 0 || _sector < 0) return;
            QuarantinedUntil[SectorKey(_targetId, _sector)] = time + .50f;
            RemoveOwnLease();
            _sector = -1;
            _blockedAttempts = Mathf.Min(20, _blockedAttempts + 1);
        }

        public void ReportProgress() => _blockedAttempts = Mathf.Max(0, _blockedAttempts - 1);

        public static int GetTargetLoad(Transform targetRoot, float time)
        {
            int key = StableActorKey(targetRoot);
            if (!ByTarget.TryGetValue(key, out List<Lease> leases)) return 0;
            Prune(leases, time);
            return leases.Count;
        }

        public static int StableActorKey(Transform actor)
        {
            if (actor == null) return 0;
            unchecked
            {
                uint hash = 2166136261u;
                string value = actor.name ?? string.Empty;
                for (int i = 0; i < value.Length; i++)
                {
                    hash ^= value[i];
                    hash *= 16777619u;
                }
                Transform cursor = actor;
                while (cursor != null)
                {
                    hash ^= (uint)(cursor.GetSiblingIndex() + 1);
                    hash *= 16777619u;
                    cursor = cursor.parent;
                }
                Vector3 position = actor.position;
                hash ^= (uint)Mathf.RoundToInt(position.x * 100f);
                hash *= 16777619u;
                hash ^= (uint)Mathf.RoundToInt(position.y * 100f);
                hash *= 16777619u;
                return hash == 0u ? 1 : (int)hash;
            }
        }

        public void Release()
        {
            if (_targetId != 0 && ByTarget.TryGetValue(_targetId, out List<Lease> leases))
            {
                for (int i = leases.Count - 1; i >= 0; i--)
                    if (leases[i].ownerId == _ownerId) leases.RemoveAt(i);
                if (leases.Count == 0) ByTarget.Remove(_targetId);
            }
            _targetId = 0;
            _ownerId = 0;
            _sector = -1;
            _ring = 0;
            _nextHeartbeat = 0f;
            _blockedAttempts = 0;
        }

        private void Acquire(Vector2 target, Vector2 self, float time)
        {
            if (!ByTarget.TryGetValue(_targetId, out List<Lease> leases))
            {
                leases = new List<Lease>(MaxReservationsPerTarget);
                ByTarget.Add(_targetId, leases);
            }
            Prune(leases, time);
            if (leases.Count >= MaxReservationsPerTarget) return;

            Vector2 fromTarget = self - target;
            float currentAngle = Mathf.Atan2(fromTarget.y, fromTarget.x) * Mathf.Rad2Deg;
            if (currentAngle < 0f) currentAngle += 360f;
            int preferred = Mathf.RoundToInt(currentAngle / (360f / SectorCount)) % SectorCount;
            float bestCost = float.MaxValue;
            int bestSector = -1;
            int bestRing = 0;
            uint tieSeed = unchecked((uint)(_ownerId * 397) ^ (uint)_targetId);

            for (int ring = 0; ring < 2; ring++)
            for (int sector = 0; sector < SectorCount; sector++)
            {
                bool occupied = false;
                if (QuarantinedUntil.TryGetValue(SectorKey(_targetId, sector), out float until) && until > time)
                    continue;
                for (int i = 0; i < leases.Count; i++)
                    if (leases[i].sector == sector && leases[i].ring == ring) { occupied = true; break; }
                if (occupied) continue;
                int delta = Mathf.Abs(sector - preferred);
                delta = Mathf.Min(delta, SectorCount - delta);
                float tie = ((tieSeed + (uint)(sector * 31 + ring * 131)) & 1023u) * .000001f;
                float cost = delta * .35f + ring + tie;
                if (cost < bestCost) { bestCost = cost; bestSector = sector; bestRing = ring; }
            }

            if (bestSector < 0) return;
            _sector = bestSector;
            _ring = bestRing;
            leases.Add(new Lease { ownerId = _ownerId, sector = _sector, ring = _ring, expiresAt = time + StaleLeaseDuration });
            _nextHeartbeat = time + HeartbeatInterval;
        }

        private void RemoveOwnLease()
        {
            if (!ByTarget.TryGetValue(_targetId, out List<Lease> leases)) return;
            for (int i = leases.Count - 1; i >= 0; i--)
                if (leases[i].ownerId == _ownerId) leases.RemoveAt(i);
            if (leases.Count == 0) ByTarget.Remove(_targetId);
        }

        private static long SectorKey(int target, int sector) => ((long)target << 8) ^ (uint)sector;

        private void Heartbeat(float time)
        {
            if (!ByTarget.TryGetValue(_targetId, out List<Lease> leases)) { _sector = -1; return; }
            Prune(leases, time);
            for (int i = 0; i < leases.Count; i++)
                if (leases[i].ownerId == _ownerId)
                {
                    leases[i].expiresAt = time + StaleLeaseDuration;
                    _nextHeartbeat = time + HeartbeatInterval;
                    return;
                }
            _sector = -1;
        }

        private static void Prune(List<Lease> leases, float time)
        {
            for (int i = leases.Count - 1; i >= 0; i--)
                if (leases[i].expiresAt <= time) leases.RemoveAt(i);
        }
    }
}
