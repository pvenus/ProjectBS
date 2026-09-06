using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Morpg
{
    public enum MorpgOutcome { None, Requested, Victory, Defeat, Failed }
    public enum RewardBundleState { Created, TransactionPending, Applied, Credited, Failed }
    public enum ClearRecordState { None, Detected, Committed, Failed }
    public enum WaveStartState { None, Started, Completed, Failed }

    [Serializable]
    public sealed class BattleMorpgZoneRewardDefinition
    {
        public const string SupportedSchema = "battle-morpg-zone-reward.v1";
        public string schemaVersion;
        public string flowId;
        public string dropProfileId;
        public string hudProfileId;
        public string battleId;
        public string fallbackProfileId;
        public Zone[] zones;
        public Drop[] drops;
        public Expected expected;
        public string[] suppressionIds;
        public bool backtracking;

        [Serializable] public sealed class Zone
        {
            public int index;
            public string id;
            public string waveId;
            public string entryWarpAnchorId;
            public float[] bounds;
            public float[] spawnBounds;
            public float[] entry;
            public Counts counts;
            public float[] radius;
            public Reservation[] reservations;
        }
        [Serializable] public sealed class Counts { public int black; public int chain; }
        [Serializable] public sealed class Reservation
        {
            public string reservationId;
            public string sourceZoneId;
            public string unitKey;
            public float localDueTime;
            public string positionCandidateKey;
            public float[] position;
        }
        [Serializable] public sealed class Drop { public string unitKey; public int gold; public float xp; }
        [Serializable] public sealed class Expected { public int deaths; public int gold; public float xp; }
    }

    public static class BattleMorpgDefinitionValidator
    {
        public const string BattleId = "battle.act1.chapter01.01.rescue_villagers";
        public static bool TryValidate(BattleMorpgZoneRewardDefinition d, out string error)
        {
            error = null;
            if (d == null || d.schemaVersion != BattleMorpgZoneRewardDefinition.SupportedSchema ||
                d.battleId != BattleId || d.zones == null || d.zones.Length != 3 || d.drops == null ||
                d.drops.Length != 2 || d.expected == null || d.expected.deaths != 28 ||
                d.expected.gold != 40 || !Approximately(d.expected.xp, 10f) || d.backtracking)
                return Fail("header/schema/expected mismatch", out error);

            int[] totals = { 19, 5, 4 };
            int black = 0, chain = 0, rows = 0;
            var ids = new HashSet<string>(StringComparer.Ordinal);
            for (int z = 0; z < d.zones.Length; z++)
            {
                var zone = d.zones[z];
                string side = z == 0 ? "left" : z == 1 ? "center" : "right";
                string expectedWave = $"wave.act1.chapter01.01.rescue_villagers.zone.{side}.0{z + 1}";
                string expectedAnchor = $"anchor.act1.chapter01.01.rescue_villagers.zone.{side}.entry_warp";
                if (zone == null || zone.index != z || string.IsNullOrEmpty(zone.id) ||
                    zone.waveId != expectedWave || zone.entryWarpAnchorId != expectedAnchor ||
                    zone.bounds?.Length != 4 || zone.spawnBounds?.Length != 4 || zone.entry?.Length != 2 ||
                    zone.radius?.Length != 2 || zone.counts == null || zone.reservations == null ||
                    zone.reservations.Length != totals[z] || zone.counts.black + zone.counts.chain != totals[z])
                    return Fail($"zone {z} shape/count mismatch", out error);
                black += zone.counts.black; chain += zone.counts.chain;
                int zoneBlack = 0, zoneChain = 0;
                float previousDue = -1f;
                foreach (var row in zone.reservations)
                {
                    if (row == null || !ids.Add(row.reservationId) || row.sourceZoneId != zone.id ||
                        row.position?.Length != 2 || row.localDueTime < previousDue ||
                        (row.unitKey != "black" && row.unitKey != "chain") ||
                        !Inside(row.position, zone.spawnBounds))
                        return Fail($"zone {z} reservation invalid", out error);
                    previousDue = row.localDueTime; rows++;
                    if (row.unitKey == "black") zoneBlack++; else zoneChain++;
                }
                if (zoneBlack != zone.counts.black || zoneChain != zone.counts.chain)
                    return Fail($"zone {z} role rows mismatch", out error);
            }
            if (rows != 28 || black != 22 || chain != 6) return Fail("role conservation mismatch", out error);
            if (d.suppressionIds == null || d.suppressionIds.Length != 2) return Fail("suppression IDs missing", out error);
            return true;
        }

        private static bool Inside(float[] p, float[] b) => p[0] >= b[0] && p[0] <= b[2] && p[1] >= b[1] && p[1] <= b[3];
        private static bool Approximately(float a, float b) => Math.Abs(a - b) <= .0001f;
        private static bool Fail(string value, out string error) { error = value; return false; }
    }

    public interface IRevisionedRewardAccount
    {
        decimal Value { get; }
        long Revision { get; }
        bool TryApply(decimal delta, long expectedRevision);
        bool TryRestore(decimal value, long expectedRevision);
    }

    public sealed class BattleRewardBundle
    {
        public string Id { get; internal set; }
        public long DeathSequence { get; internal set; }
        public string ReservationId { get; internal set; }
        public decimal Gold { get; internal set; }
        public decimal Xp { get; internal set; }
        public int XpQuarterUnits { get; internal set; }
        public RewardBundleState State { get; internal set; }
    }

    public sealed class BattleRewardLedger
    {
        private readonly IRevisionedRewardAccount gold;
        private readonly IRevisionedRewardAccount xp;
        private readonly Dictionary<string, BattleRewardBundle> bundles = new(StringComparer.Ordinal);
        public BattleRewardLedger(IRevisionedRewardAccount gold, IRevisionedRewardAccount xp)
        { this.gold = gold; this.xp = xp; }
        public IReadOnlyDictionary<string, BattleRewardBundle> Bundles => bundles;

        public bool TryCredit(string id, long deathSequence, string reservationId, decimal goldDelta, int xpQuarterUnits, out string error)
        {
            error = null;
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(reservationId)) { error = "missing bundle identity"; return false; }
            if (bundles.TryGetValue(id, out var prior)) return prior.State == RewardBundleState.Credited;
            if (goldDelta < 0m || xpQuarterUnits < 0) { error = "negative reward delta"; return false; }
            decimal xpDelta = xpQuarterUnits / 4m;
            var bundle = new BattleRewardBundle { Id=id, DeathSequence=deathSequence, ReservationId=reservationId,
                Gold=goldDelta, Xp=xpDelta, XpQuarterUnits=xpQuarterUnits, State=RewardBundleState.TransactionPending };
            bundles.Add(id, bundle);
            decimal oldGold=gold.Value, oldXp=xp.Value; long goldRev=gold.Revision, xpRev=xp.Revision;
            if (!gold.TryApply(goldDelta, goldRev)) return Fail(bundle, "gold apply failed", out error);
            if (!xp.TryApply(xpDelta, xpRev))
            {
                if (!gold.TryRestore(oldGold, goldRev + 1)) return Fail(bundle, "xp apply and gold rollback failed", out error);
                return Fail(bundle, "xp apply failed", out error);
            }
            if (gold.Value != oldGold + goldDelta || xp.Value != oldXp + xpDelta)
            {
                bool xr=xp.TryRestore(oldXp, xpRev + 1); bool gr=gold.TryRestore(oldGold, goldRev + 1);
                return Fail(bundle, xr && gr ? "readback mismatch" : "readback mismatch rollback failed", out error);
            }
            bundle.State=RewardBundleState.Applied;
            bundle.State=RewardBundleState.Credited;
            return true;
        }
        private static bool Fail(BattleRewardBundle b, string message, out string error)
        { b.State=RewardBundleState.Failed; error=message; return false; }
    }

    public sealed class BattleOutcomeArbiter
    {
        public BattleOutcomeArbiter(BattleAttemptKey attempt) { Attempt = attempt; }
        public BattleAttemptKey Attempt { get; }
        public string OutcomeKey => BattleMorpgCanonicalKeys.Attempt(Attempt);
        public MorpgOutcome Outcome { get; private set; }
        public bool TryRequest()
        {
            if (Outcome != MorpgOutcome.None) return false;
            Outcome = MorpgOutcome.Requested; return true;
        }
        public bool TryCommit(MorpgOutcome next)
        {
            if (Outcome != MorpgOutcome.Requested || next == MorpgOutcome.None || next == MorpgOutcome.Requested) return false;
            Outcome = next; return true;
        }
    }

    public sealed class ClearRecord
    {
        public ClearRecordState State { get; private set; }
        public bool TryDetect() { if (State != ClearRecordState.None) return false; State = ClearRecordState.Detected; return true; }
        public bool TryCommit() { if (State != ClearRecordState.Detected) return false; State = ClearRecordState.Committed; return true; }
        public bool TryFail() { if (State == ClearRecordState.Committed || State == ClearRecordState.Failed) return false; State = ClearRecordState.Failed; return true; }
    }

    public sealed class WaveStartRecord
    {
        public WaveStartState State { get; private set; }
        public bool TryStart() { if (State != WaveStartState.None) return false; State = WaveStartState.Started; return true; }
        public bool TryComplete() { if (State != WaveStartState.Started) return false; State = WaveStartState.Completed; return true; }
        public bool TryFail() { if (State == WaveStartState.Completed || State == WaveStartState.Failed) return false; State = WaveStartState.Failed; return true; }
    }

    public readonly struct BattleAttemptKey : IEquatable<BattleAttemptKey>
    {
        public BattleAttemptKey(string runId, string episodeId, string battleId, string attemptId)
        { RunId=runId; EpisodeId=episodeId; BattleId=battleId; AttemptId=attemptId; }
        public string RunId { get; } public string EpisodeId { get; }
        public string BattleId { get; } public string AttemptId { get; }
        public bool Equals(BattleAttemptKey other) => RunId==other.RunId && EpisodeId==other.EpisodeId && BattleId==other.BattleId && AttemptId==other.AttemptId;
    }

    public readonly struct WaveClearKey : IEquatable<WaveClearKey>
    {
        public WaveClearKey(BattleAttemptKey attempt, string zoneId, string waveId)
        { Attempt=attempt; ZoneId=zoneId; WaveId=waveId; }
        public BattleAttemptKey Attempt { get; } public string ZoneId { get; } public string WaveId { get; }
        public bool Equals(WaveClearKey other) => Attempt.Equals(other.Attempt) && ZoneId==other.ZoneId && WaveId==other.WaveId;
    }

    public enum TransitionRecordState { Requested, Prepared, Committed, Restored, Failed }
    public sealed class TransitionRecord
    {
        public TransitionRecord(BattleAttemptKey attempt, string fromZoneId, string toZoneId, int ordinal)
        { Attempt=attempt; FromZoneId=fromZoneId; ToZoneId=toZoneId; Ordinal=ordinal; State=TransitionRecordState.Requested; }
        public BattleAttemptKey Attempt { get; }
        public string FromZoneId { get; }
        public string ToZoneId { get; }
        public int Ordinal { get; }
        public TransitionRecordState State { get; private set; }
        public bool TryPrepare() { if (State != TransitionRecordState.Requested) return false; State=TransitionRecordState.Prepared; return true; }
        public bool TryCommit() { if (State != TransitionRecordState.Prepared) return false; State=TransitionRecordState.Committed; return true; }
        public bool TryRestore() { if (State != TransitionRecordState.Requested && State != TransitionRecordState.Prepared) return false; State=TransitionRecordState.Restored; return true; }
        public bool TryFail() { if (State == TransitionRecordState.Restored || State == TransitionRecordState.Failed) return false; State=TransitionRecordState.Failed; return true; }
    }

    public enum MorpgCheckpointState
    {
        WaveActive, ClearDetected, ClearCommitted, TransitionRequested, TransitionPrepared,
        TransitionCommitted, RewardTransactionPending, RewardApplied, RewardCredited, OutcomeRequested, OutcomeTerminal
    }

    public static class BattleMorpgCheckpointPolicy
    {
        public static bool MustQueueDurableSave(MorpgCheckpointState state) => state != MorpgCheckpointState.OutcomeTerminal;
        public static bool MustStartNewAttemptAfterReload(MorpgCheckpointState state) => state != MorpgCheckpointState.OutcomeTerminal;
        public static bool KeepsRunLocalPauseState(MorpgCheckpointState state) => state != MorpgCheckpointState.OutcomeTerminal;
    }

    public static class BattleMorpgCanonicalKeys
    {
        private static string Part(string value) { value ??= string.Empty; return value.Length + ":" + value; }
        public static string Attempt(BattleAttemptKey k) => $"attempt={Part(k.RunId)}/{Part(k.EpisodeId)}/{Part(k.BattleId)}/{Part(k.AttemptId)}";
        public static string Clear(BattleAttemptKey k, string zone, string wave) => $"clear={Attempt(k)}/zone={Part(zone)}/wave={Part(wave)}";
        public static string Transition(BattleAttemptKey k, int ordinal, string from, string to) => $"transition={Attempt(k)}/ordinal={ordinal}/from={Part(from)}/to={Part(to)}";
        public static string WaveStart(BattleAttemptKey k, string wave) => $"waveStart={Attempt(k)}/wave={Part(wave)}";
        public static string Death(BattleAttemptKey k, string reservation) => $"death={Attempt(k)}/reservation={Part(reservation)}";
        public static string Bundle(BattleAttemptKey k, string reservation) => $"bundle={Attempt(k)}/reservation={Part(reservation)}";
    }

    public readonly struct BattleMorpgDecodeResult
    {
        public BattleMorpgDecodeResult(bool legacy, BattleMorpgZoneRewardDefinition definition, string error)
        { IsLegacy=legacy; Definition=definition; Error=error; }
        public bool IsLegacy { get; } public BattleMorpgZoneRewardDefinition Definition { get; } public string Error { get; }
        public bool Success => IsLegacy || (Definition != null && Error == null);
    }

    public static class BattleMorpgDefinitionCodec
    {
        [Serializable] private sealed class Header { public string schemaVersion; }
        public static BattleMorpgDecodeResult Decode(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new BattleMorpgDecodeResult(true, null, null);
            Header header;
            try { header=JsonUtility.FromJson<Header>(json); }
            catch (Exception e) { return new BattleMorpgDecodeResult(false, null, e.Message); }
            if (header == null || string.IsNullOrEmpty(header.schemaVersion))
                return new BattleMorpgDecodeResult(true, null, null);
            if (header.schemaVersion != BattleMorpgZoneRewardDefinition.SupportedSchema)
                return new BattleMorpgDecodeResult(false, null, $"unsupported schemaVersion: {header.schemaVersion}");
            if (!BattleMorpgRootShape.TryValidate(json, out string shapeError))
                return new BattleMorpgDecodeResult(false, null, shapeError);
            try
            {
                var definition=JsonUtility.FromJson<BattleMorpgZoneRewardDefinition>(json);
                return BattleMorpgDefinitionValidator.TryValidate(definition, out string error)
                    ? new BattleMorpgDecodeResult(false, definition, null)
                    : new BattleMorpgDecodeResult(false, null, error);
            }
            catch (Exception e) { return new BattleMorpgDecodeResult(false, null, e.Message); }
        }
    }
}
