using System;
using System.Collections.Generic;

namespace Battle.Morpg
{
    // These types have no runtime entry point. Attachment belongs to the existing managers.
    internal enum MorpgTransitionReleaseReason { Success, Restored, Failed, Abort, Defeat, Teardown }
    internal enum MorpgZoneStopReason { Transition, Failed, Abort, Defeat, Teardown }
    internal enum MorpgRouteStopReason { Failed, Abort, Defeat, Teardown }
    internal enum MorpgOwnedKind { EnemyRoot, HostileProjectile, HostileEffect, DelayedHit, PathCache }

    internal sealed class MorpgZoneRuntimeHandle
    {
        internal MorpgZoneRuntimeHandle(BattleAttemptKey attempt, string zone, string wave, long generation)
        { Attempt = attempt; ZoneId = zone; WaveId = wave; Generation = generation; }
        internal BattleAttemptKey Attempt { get; }
        internal string ZoneId { get; }
        internal string WaveId { get; }
        internal long Generation { get; }
        internal WaveClearKey ClearKey => new WaveClearKey(Attempt, ZoneId, WaveId);
        internal string WaveStartKey => BattleMorpgCanonicalKeys.WaveStart(Attempt, WaveId);
    }

    // Possession alone does not authorize mutation: the issuing host keeps the identity table.
    internal sealed class MorpgTransitionScopeToken { internal MorpgTransitionScopeToken() { } }

    internal sealed class MorpgZoneClearSnapshot
    {
        internal MorpgZoneClearSnapshot(MorpgZoneRuntimeHandle handle, int declared, int emitted,
            int pending, int living, bool spawnFailed, bool rewardFailed, bool cleanupReady)
        { Handle = handle; Declared = declared; Emitted = emitted; Pending = pending; Living = living;
          SpawnFailed = spawnFailed; RewardTransactionFailed = rewardFailed; CleanupReady = cleanupReady; }
        internal MorpgZoneRuntimeHandle Handle { get; }
        internal int Declared { get; }
        internal int Emitted { get; }
        internal int Pending { get; }
        internal int Living { get; }
        internal bool SpawnFailed { get; }
        internal bool RewardTransactionFailed { get; }
        internal bool CleanupReady { get; }
        internal bool Eligible => Handle != null && Declared > 0 && Emitted == Declared && Pending == 0 &&
            Living == 0 && !SpawnFailed && !RewardTransactionFailed && CleanupReady;
    }

    internal sealed class MorpgTransitionPlan
    {
        internal MorpgTransitionPlan(MorpgZoneRuntimeHandle source, string destination, string wave, string anchor)
        { Source = source; DestinationZone = destination; DestinationWave = wave; Anchor = anchor; }
        internal MorpgZoneRuntimeHandle Source { get; }
        internal string DestinationZone { get; }
        internal string DestinationWave { get; }
        internal string Anchor { get; }
    }

    internal interface IMorpgZoneBattleHost
    {
        bool TryAcquireTransitionScope(TransitionRecord transition, out MorpgTransitionScopeToken token, out string error);
        bool TryPrepareTransition(MorpgTransitionScopeToken token, MorpgTransitionPlan plan, MorpgZoneClearSnapshot clear, out string error);
        bool TryCommitWarp(MorpgTransitionScopeToken token, MorpgTransitionPlan plan, MorpgZoneRuntimeHandle source, out string error);
        bool TryReleaseTransitionScope(MorpgTransitionScopeToken token, MorpgTransitionReleaseReason reason, out string error);
        bool TryCompleteFinalZone(BattleAttemptKey attempt, WaveClearKey clear, out string error);
    }

    internal interface IMorpgZoneOwnedHandle
    {
        MorpgZoneRuntimeHandle Zone { get; }
        string StableId { get; }
        MorpgOwnedKind Kind { get; }
        // Must perform all fallible validation here. Disposal must be idempotent and do no lookup.
        bool TryPrepareDispose(out string error);
        void DisposePrepared();
    }

    internal sealed class MorpgZoneCleanupPlan
    {
        internal MorpgZoneCleanupPlan(MorpgZoneOwnedHandleRegistry owner, MorpgZoneRuntimeHandle zone,
            IMorpgZoneOwnedHandle[] handles) { Owner = owner; Zone = zone; Handles = handles; }
        internal MorpgZoneOwnedHandleRegistry Owner { get; }
        internal MorpgZoneRuntimeHandle Zone { get; }
        internal IMorpgZoneOwnedHandle[] Handles { get; }
        internal bool Complete { get; set; }
    }

    internal sealed class MorpgZoneOwnedHandleRegistry
    {
        private readonly MorpgZoneRuntimeHandle zone;
        private readonly Dictionary<string, IMorpgZoneOwnedHandle> owned = new Dictionary<string, IMorpgZoneOwnedHandle>(StringComparer.Ordinal);
        private readonly HashSet<string> disposed = new HashSet<string>(StringComparer.Ordinal);
        private MorpgZoneCleanupPlan prepared;
        internal MorpgZoneOwnedHandleRegistry(MorpgZoneRuntimeHandle zone) { this.zone = zone ?? throw new ArgumentNullException(nameof(zone)); }
        internal bool IsDisposed => prepared != null && prepared.Complete;
        internal bool TryRegister(MorpgZoneRuntimeHandle scope, IMorpgZoneOwnedHandle handle, out string error)
        {
            error = null;
            if (!ReferenceEquals(scope, zone) || handle == null || !ReferenceEquals(handle.Zone, zone) || string.IsNullOrEmpty(handle.StableId))
                return Fail("foreign handle", out error);
            if (owned.TryGetValue(handle.StableId, out var prior))
                return ReferenceEquals(prior, handle) || Fail("duplicate identity with different object", out error);
            if (prepared != null) return Fail("source sealed for cleanup", out error);
            owned.Add(handle.StableId, handle); return true;
        }
        internal bool TryPrepareDisposeAll(MorpgZoneRuntimeHandle scope, out MorpgZoneCleanupPlan plan, out string error)
        {
            plan = null; error = null;
            if (!ReferenceEquals(scope, zone)) return Fail("stale/foreign cleanup scope", out error);
            if (prepared != null) { plan = prepared; return true; }
            var ids = new List<string>(owned.Keys); ids.Sort(StringComparer.Ordinal);
            var handles = new IMorpgZoneOwnedHandle[ids.Count];
            for (int i = 0; i < ids.Count; i++)
            {
                handles[i] = owned[ids[i]];
                try { if (!handles[i].TryPrepareDispose(out error)) return false; }
                catch (Exception e) { return Fail("cleanup prepare: " + e.GetType().Name, out error); }
            }
            prepared = plan = new MorpgZoneCleanupPlan(this, zone, handles); return true;
        }
        internal bool TryDisposeAll(MorpgZoneCleanupPlan plan, out string error)
        {
            error = null;
            if (plan == null || !ReferenceEquals(plan, prepared) || !ReferenceEquals(plan.Owner, this) || !ReferenceEquals(plan.Zone, zone))
                return Fail("foreign cleanup plan", out error);
            if (plan.Complete) return true;
            // Continue exact-owned cleanup after one producer throws; never touch another registry.
            foreach (var handle in plan.Handles)
            {
                if (disposed.Contains(handle.StableId)) continue;
                try { handle.DisposePrepared(); disposed.Add(handle.StableId); }
                catch (Exception e) { error = "owned disposal: " + e.GetType().Name; }
            }
            plan.Complete = disposed.Count == plan.Handles.Length;
            return plan.Complete;
        }
        private static bool Fail(string message, out string error) { error = message; return false; }
    }

    // Active-zone-only accounting component, intended for BattleSpawnManager ownership at integration.
    // The producer must register the root BEFORE activation; descendants use this same exact registry.
    internal sealed class MorpgZoneWaveScope
    {
        private readonly HashSet<string> reservations;
        private readonly HashSet<string> emitted = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> dead = new HashSet<string>(StringComparer.Ordinal);
        private bool stopped, spawnFailed, rewardFailed;
        internal MorpgZoneWaveScope(MorpgZoneRuntimeHandle handle, IEnumerable<string> reservationIds)
        { Handle = handle; Registry = new MorpgZoneOwnedHandleRegistry(handle); reservations = new HashSet<string>(reservationIds, StringComparer.Ordinal); }
        internal MorpgZoneRuntimeHandle Handle { get; }
        internal MorpgZoneOwnedHandleRegistry Registry { get; }
        internal bool TryEmitted(MorpgZoneRuntimeHandle handle, string reservation, IMorpgZoneOwnedHandle root, out string error)
        {
            error = null;
            if (stopped || !ReferenceEquals(handle, Handle) || !reservations.Contains(reservation)) { error = "foreign/stopped emission"; return false; }
            if (emitted.Contains(reservation)) return false;
            if (root == null || root.Kind != MorpgOwnedKind.EnemyRoot || !Registry.TryRegister(handle, root, out error))
            { spawnFailed = true; error = error ?? "root producer missing"; return false; }
            emitted.Add(reservation); return true;
        }
        internal bool TryDied(MorpgZoneRuntimeHandle handle, string reservation)
        { return !stopped && ReferenceEquals(handle, Handle) && emitted.Contains(reservation) && dead.Add(reservation); }
        internal bool TryFail(MorpgZoneRuntimeHandle handle, bool rewardTransaction)
        { if (stopped || !ReferenceEquals(handle, Handle)) return false; if (rewardTransaction) rewardFailed = true; else spawnFailed = true; return true; }
        internal bool TryGetMorpgZoneClearSnapshot(MorpgZoneRuntimeHandle handle, out MorpgZoneClearSnapshot snapshot, out string error)
        {
            snapshot = null; error = null;
            if (!ReferenceEquals(handle, Handle) || stopped) { error = "foreign/stopped snapshot"; return false; }
            bool ready = emitted.Count == reservations.Count && dead.Count == emitted.Count && !spawnFailed && !rewardFailed;
            if (ready) ready = Registry.TryPrepareDisposeAll(handle, out _, out error);
            snapshot = new MorpgZoneClearSnapshot(handle, reservations.Count, emitted.Count, reservations.Count - emitted.Count,
                emitted.Count - dead.Count, spawnFailed, rewardFailed, ready); return true;
        }
        internal bool TryStopMorpgZoneWave(MorpgZoneRuntimeHandle handle, MorpgZoneStopReason reason, out string error)
        {
            error = null;
            if (!ReferenceEquals(handle, Handle)) { error = "foreign stop"; return false; }
            stopped = true;
            return Registry.TryPrepareDisposeAll(handle, out var plan, out error) && Registry.TryDisposeAll(plan, out error);
        }
    }

    internal interface IMorpgZoneWaveDriver
    {
        bool TryStartMorpgZoneWave(BattleAttemptKey attempt, int ordinal, out MorpgZoneRuntimeHandle handle, out string error);
        bool TryGetMorpgZoneClearSnapshot(MorpgZoneRuntimeHandle handle, out MorpgZoneClearSnapshot snapshot, out string error);
        bool TryStopMorpgZoneWave(MorpgZoneRuntimeHandle handle, MorpgZoneStopReason reason, out string error);
    }

    // A single attempt's accounting owner. No later-zone scope exists until the prior scope is disposed.
    internal sealed class MorpgZoneWaveOwner : IMorpgZoneWaveDriver
    {
        private static long nextGeneration;
        private readonly BattleAttemptKey attempt;
        private readonly string[] zones = new string[3], waves = new string[3];
        private readonly string[][] reservations = new string[3][];
        private readonly MorpgZoneWaveScope[] scopes = new MorpgZoneWaveScope[3];
        private int active = -1;
        private bool terminated;
        internal MorpgZoneWaveOwner(BattleAttemptKey attempt, BattleMorpgZoneRewardDefinition definition)
        {
            if (attempt.BattleId != BattleMorpgDefinitionValidator.BattleId || !BattleMorpgDefinitionValidator.TryValidate(definition, out _))
                throw new ArgumentException("invalid exact route");
            this.attempt = attempt;
            for (int z = 0; z < 3; z++)
            {
                zones[z] = definition.zones[z].id; waves[z] = definition.zones[z].waveId;
                reservations[z] = Array.ConvertAll(definition.zones[z].reservations, r => r.reservationId);
            }
        }
        internal MorpgZoneWaveScope ActiveScope => active < 0 ? null : scopes[active];
        public bool TryStartMorpgZoneWave(BattleAttemptKey key, int ordinal, out MorpgZoneRuntimeHandle handle, out string error)
        {
            handle = null; error = null;
            if (terminated || !key.Equals(attempt) || ordinal < 0 || ordinal > 2) { error = "foreign/stopped start"; return false; }
            if (ordinal == active) { handle = scopes[ordinal].Handle; return true; }
            if (ordinal != active + 1 || (active >= 0 && !scopes[active].Registry.IsDisposed)) { error = "out-of-order start"; return false; }
            handle = new MorpgZoneRuntimeHandle(attempt, zones[ordinal], waves[ordinal], System.Threading.Interlocked.Increment(ref nextGeneration));
            scopes[ordinal] = new MorpgZoneWaveScope(handle, reservations[ordinal]); active = ordinal; return true;
        }
        public bool TryGetMorpgZoneClearSnapshot(MorpgZoneRuntimeHandle handle, out MorpgZoneClearSnapshot snapshot, out string error)
        {
            snapshot = null; error = "no active scope";
            return !terminated && ActiveScope != null && ActiveScope.TryGetMorpgZoneClearSnapshot(handle, out snapshot, out error);
        }
        public bool TryStopMorpgZoneWave(MorpgZoneRuntimeHandle handle, MorpgZoneStopReason reason, out string error)
        {
            error = "foreign stop";
            if (ActiveScope == null || !ReferenceEquals(ActiveScope.Handle, handle)) return false;
            if (reason != MorpgZoneStopReason.Transition) terminated = true;
            return ActiveScope.TryStopMorpgZoneWave(handle, reason, out error);
        }
    }

    internal interface IMorpgSettlementReadiness { bool IsReady(WaveClearKey clear); }
    internal sealed class MorpgP1SettlementNotReady : IMorpgSettlementReadiness
    { public bool IsReady(WaveClearKey clear) => false; }
}
