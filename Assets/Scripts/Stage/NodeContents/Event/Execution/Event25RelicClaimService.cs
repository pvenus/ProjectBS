using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Stage
{
    public enum Event25RelicClaimState
    {
        None = 0,
        BattleVictoryCommitted = 10,
        RelicClaimPending = 20,
        Granting = 30,
        RelicClaimPendingRetry = 40,
        GrantedTerminal = 50
    }

    [Serializable]
    public sealed class Event25RelicClaim
    {
        public string causeId;
        public string selectedRelicId;
        public string eligibleFingerprint;
        public Event25RelicClaimState state;
        public bool addRelicCommitted;
        public bool terminalCommitted;
    }

    public sealed class Event25RelicClaimService
    {
        public const string PoolId = "relic_pool.act1.chapter01.random_event.standard.v1";
        public const string PoolVersion = "1";
        public const string EligibleZeroCopy =
            "획득 가능한 새 유물이 없어 전투 보상을 받을 수 없습니다.";

        public static string[] EligibleRelicIds(
            IEnumerable<string> poolRelicIds,
            IEnumerable<string> acquiredRelicIds)
        {
            HashSet<string> acquired = new(acquiredRelicIds ?? Array.Empty<string>(),
                StringComparer.Ordinal);
            return (poolRelicIds ?? Array.Empty<string>())
                .Where(id => !string.IsNullOrWhiteSpace(id) && !acquired.Contains(id))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToArray();
        }

        public static bool CanBeginBattle(
            IEnumerable<string> poolRelicIds,
            IEnumerable<string> acquiredRelicIds) =>
            EligibleRelicIds(poolRelicIds, acquiredRelicIds).Length > 0;

        public Event25RelicClaim CreateVictoryClaim(
            string runId,
            string stageGenerationId,
            string event25ReservationId,
            string battleVictoryReceiptId,
            IEnumerable<string> poolRelicIds,
            IEnumerable<string> acquiredRelicIds)
        {
            string[] eligible = EligibleRelicIds(poolRelicIds, acquiredRelicIds);
            if (eligible.Length == 0) return null;
            string causeId = CanonicalHash(runId, stageGenerationId, event25ReservationId,
                battleVictoryReceiptId, PoolId, PoolVersion, "0");
            return new Event25RelicClaim
            {
                causeId = causeId,
                selectedRelicId = eligible[SelectUniformIndex(causeId, eligible.Length)],
                eligibleFingerprint = CanonicalHash(eligible),
                state = Event25RelicClaimState.RelicClaimPending
            };
        }

        public bool TryGrant(
            Event25RelicClaim claim,
            Func<string, bool> addRelic,
            Func<string, bool> commitTerminal)
        {
            if (claim == null || string.IsNullOrWhiteSpace(claim.selectedRelicId)) return false;
            if (claim.state == Event25RelicClaimState.GrantedTerminal)
                return claim.addRelicCommitted && claim.terminalCommitted;
            if (claim.state != Event25RelicClaimState.RelicClaimPending
                && claim.state != Event25RelicClaimState.RelicClaimPendingRetry) return false;

            claim.state = Event25RelicClaimState.Granting;
            if (!claim.addRelicCommitted)
            {
                if (addRelic == null || !addRelic(claim.selectedRelicId))
                {
                    claim.state = Event25RelicClaimState.RelicClaimPendingRetry;
                    return false;
                }
                claim.addRelicCommitted = true;
            }
            if (commitTerminal == null || !commitTerminal(claim.causeId))
            {
                claim.state = Event25RelicClaimState.RelicClaimPendingRetry;
                return false;
            }
            claim.terminalCommitted = true;
            claim.state = Event25RelicClaimState.GrantedTerminal;
            return true;
        }

        private static string CanonicalHash(params string[] values)
        {
            using SHA256 sha = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(string.Join("\n",
                values.Select(value => value ?? string.Empty)));
            return string.Concat(sha.ComputeHash(bytes).Select(value => value.ToString("x2")));
        }

        private static ulong ReadUInt64BigEndian(byte[] value) =>
            ((ulong)value[0] << 56) | ((ulong)value[1] << 48)
            | ((ulong)value[2] << 40) | ((ulong)value[3] << 32)
            | ((ulong)value[4] << 24) | ((ulong)value[5] << 16)
            | ((ulong)value[6] << 8) | value[7];

        private static int SelectUniformIndex(string causeId, int count)
        {
            ulong range = ulong.MaxValue - (ulong.MaxValue % (ulong)count);
            for (int counter = 0; ; counter++)
            {
                byte[] digest;
                using (SHA256 sha = SHA256.Create())
                    digest = sha.ComputeHash(Encoding.UTF8.GetBytes(
                        causeId + "\n" + counter));
                ulong value = ReadUInt64BigEndian(digest);
                if (value < range) return (int)(value % (ulong)count);
            }
        }
    }
}
