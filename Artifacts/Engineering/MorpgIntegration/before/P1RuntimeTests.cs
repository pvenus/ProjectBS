using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
#if NET10_0
using System.Text.Json;
#endif
using Battle.Morpg;

#if NET10_0
// Harness-only JSON bridge. The production source remains linked, not copied.
namespace UnityEngine
{
    public static class JsonUtility
    {
        public static T FromJson<T>(string json) => JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { IncludeFields = true });
    }
}

#endif

internal static class P1RuntimeTests
{
    private static void Require(bool yes, string why) { if (!yes) throw new InvalidOperationException(why); }
    internal static void Run(string root, Action<string, Action> test, List<string> trace)
    {
        string folder = Path.Combine(root, "Assets/Resources/battle/act1/chapter01/");
        string json = File.ReadAllText(folder + "battle.act1.chapter01.01.rescue_villagers.morpg-zone-reward.v1.json");
        string addendum = File.ReadAllText(folder + "battle.act1.chapter01.01.rescue_villagers.morpg-zone-reward.p0-addendum.v1.json");
        Func<Fixture> create = () => new Fixture(json, addendum);
        test("p1-internal-static-boundary", () =>
        {
            foreach (var type in new[] { typeof(IMorpgZoneBattleHost), typeof(MorpgTransitionScopeToken), typeof(MorpgZoneRuntimeHandle),
                typeof(MorpgZoneOwnedHandleRegistry), typeof(BattleMorpgZoneTransitionCoordinator), typeof(MorpgZoneWaveOwner) })
                Require(!type.IsPublic, "public P1 API: " + type.Name);
            Require(typeof(MorpgTransitionScopeToken).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Length == 0, "opaque token fields");
            foreach (string name in new[] { "MorpgZoneRuntimeSlice.cs", "BattleMorpgZoneTransitionCoordinator.cs" })
            {
                string source = File.ReadAllText(Path.Combine(root, "Assets/Scripts/Battle/Morpg/", name));
                foreach (string forbidden in new[] { "FindObjects", "FindGameObjectsWithTag", "EnemyRegistry", "StopSequence(", "SetTransitionImmunity", "TryEnterTransitionLock", "Camera.main", "GameObject", "StageSession", "SerializeField", "Currency", "CompleteBattle(" })
                    Require(!source.Contains(forbidden), name + " forbidden: " + forbidden);
            }
            foreach (string path in new[] { "Assets/Scripts/Battle/Core/BattleManager.cs", "Assets/Scripts/Battle/Spawn/Sequence/BattleSpawnManager.cs" })
                Require(!File.ReadAllText(Path.Combine(root, path)).Contains("Morpg"), "live wiring introduced");
            Require(!new MorpgP1SettlementNotReady().IsReady(default), "P2 default readiness opened");
        });
        test("p1-preactivation-fallback-producer-addendum-route", () =>
        {
            var f = create();
            Require(!BattleMorpgZoneTransitionCoordinator.TryCreate(false, f.Definition, addendum, f.Attempt, f.Host, f.Waves, true, out _, out _), "disabled accepted");
            Require(!BattleMorpgZoneTransitionCoordinator.TryCreate(true, f.Definition, addendum, f.Attempt, f.Host, f.Waves, false, out _, out _), "missing producer accepted");
            Require(!BattleMorpgZoneTransitionCoordinator.TryCreate(true, f.Definition, "{}", f.Attempt, f.Host, f.Waves, true, out _, out _), "bad unified preflight accepted");
            var foreign = new BattleAttemptKey("run", "episode", "other-battle", "attempt");
            Require(!BattleMorpgZoneTransitionCoordinator.TryCreate(true, f.Definition, addendum, foreign, f.Host, f.Waves, true, out _, out _), "foreign attempt accepted");
            Require(f.Waves.ActiveScope == null && f.Host.Log.Count == 0, "preactivation side effects");
        });
        test("p1-handle-generation-stale-snapshot-root-callbacks", () =>
        {
            var f = create(); Require(f.Coordinator.TryStartActiveWave(), "start"); var h = f.Coordinator.ActiveHandle;
            Require(!f.Coordinator.TryStartActiveWave(), "duplicate coordinator start");
            Require(f.Waves.TryStartMorpgZoneWave(f.Attempt, 0, out var same, out _) && ReferenceEquals(h, same), "duplicate start changed identity");
            Require(!f.Waves.TryStartMorpgZoneWave(f.Attempt, 1, out _, out _), "later reservation scope created early");
            var forged = new MorpgZoneRuntimeHandle(h.Attempt, h.ZoneId, h.WaveId, h.Generation);
            Require(!f.Waves.TryGetMorpgZoneClearSnapshot(forged, out _, out _) && !f.Waves.TryStopMorpgZoneWave(forged, MorpgZoneStopReason.Abort, out _), "forged handle accepted");
            Require(f.Waves.TryGetMorpgZoneClearSnapshot(h, out var before, out _) && before.Pending == 19 && !before.Eligible, "pending snapshot");
            Require(!f.Coordinator.TryCompleteActiveWave(new MorpgZoneClearSnapshot(h, 19, 19, 0, 0, false, false, true)), "forged clear bypassed source");
            f.Clear();
            Require(before.Pending == 19 && before.Emitted == 0 && !before.Eligible, "snapshot mutated");
            Require(f.Waves.TryGetMorpgZoneClearSnapshot(h, out var after, out _) && after.Eligible, "clear snapshot");
            Require(!f.Coordinator.TryCompleteActiveWave(after), "duplicate clear");
            Require(h.WaveStartKey == BattleMorpgCanonicalKeys.WaveStart(f.Attempt, f.Definition.zones[0].waveId), "canonical wave start");
            Require(h.ClearKey.Equals(new WaveClearKey(f.Attempt, f.Definition.zones[0].id, f.Definition.zones[0].waveId)), "canonical clear");
            var fresh = create(); fresh.Coordinator.TryStartActiveWave();
            Require(!fresh.Waves.TryGetMorpgZoneClearSnapshot(h, out _, out _), "stale identical attempt generation accepted");
        });
        test("p1-source-only-cleanup-kinds-identity-prepare", () =>
        {
            var f = create(); f.Coordinator.TryStartActiveWave(); var scope = f.Waves.ActiveScope; var h = scope.Handle;
            var other = create(); other.Coordinator.TryStartActiveWave(); var foreignRoot = new Owned(other.Coordinator.ActiveHandle, "same", MorpgOwnedKind.EnemyRoot);
            Require(other.Waves.ActiveScope.Registry.TryRegister(foreignRoot.Zone, foreignRoot, out _), "foreign fixture registration");
            var all = new List<Owned>();
            foreach (MorpgOwnedKind kind in Enum.GetValues(typeof(MorpgOwnedKind)))
            {
                var owned = new Owned(h, kind.ToString(), kind); all.Add(owned);
                Require(scope.Registry.TryRegister(h, owned, out _) && scope.Registry.TryRegister(h, owned, out _), "idempotent register");
                Require(!scope.Registry.TryRegister(h, new Owned(h, kind.ToString(), kind), out _), "ID collision accepted");
            }
            Require(!scope.Registry.TryRegister(h, foreignRoot, out _), "cross attempt root accepted");
            Require(scope.Registry.TryPrepareDisposeAll(h, out var plan, out _), "prepare cleanup");
            Require(all.All(x => x.Disposals == 0), "prepare destroyed handles");
            Require(!scope.Registry.TryRegister(h, new Owned(h, "late", MorpgOwnedKind.HostileEffect), out _), "late producer accepted");
            Require(!other.Waves.ActiveScope.Registry.TryDisposeAll(plan, out _), "foreign plan accepted");
            Require(scope.Registry.TryDisposeAll(plan, out _) && scope.Registry.TryDisposeAll(plan, out _), "dispose replay");
            Require(all.All(x => x.Disposals == 1) && foreignRoot.Disposals == 0, "cleanup ownership/dedup");
        });
        test("p1-registry-prepare-fault-and-owned-disposal-recovery", () =>
        {
            var f = create(); f.Coordinator.TryStartActiveWave(); var scope = f.Waves.ActiveScope;
            var a = new Owned(scope.Handle, "a", MorpgOwnedKind.HostileEffect) { RejectPrepare = true };
            var b = new Owned(scope.Handle, "b", MorpgOwnedKind.DelayedHit);
            Require(scope.Registry.TryRegister(scope.Handle, a, out _) && scope.Registry.TryRegister(scope.Handle, b, out _), "fault fixture");
            Require(!scope.Registry.TryPrepareDisposeAll(scope.Handle, out _, out _) && a.Disposals == 0 && b.Disposals == 0, "prepare fault mutated");
            a.RejectPrepare = false; a.ThrowDisposeOnce = true;
            Require(scope.Registry.TryPrepareDisposeAll(scope.Handle, out var plan, out _), "cleanup prepare recovery");
            Require(!scope.Registry.TryDisposeAll(plan, out _) && b.Disposals == 1 && !scope.Registry.IsDisposed, "partial failure did not finish other owners");
            Require(scope.Registry.TryDisposeAll(plan, out _) && a.Disposals == 1 && b.Disposals == 1, "owned retry not idempotent");
        });
        test("p1-zone19-5-4-pause-wave3-outcome-once-trace", () =>
        {
            var f = create(); Require(f.Coordinator.TryStartActiveWave(), "start");
            for (int zone = 0; zone < 3; zone++)
            {
                Require(!f.Coordinator.TryRequestVictory(), "early outcome");
                f.Clear();
                Require(f.Waves.TryGetMorpgZoneClearSnapshot(f.Coordinator.ActiveHandle, out var snap, out _) && snap.Declared == new[] {19, 5, 4}[zone], "zone count");
                trace.Add("zone=" + zone + ";start=" + snap.Handle.WaveStartKey + ";clear=" + BattleMorpgCanonicalKeys.Clear(f.Attempt, snap.Handle.ZoneId, snap.Handle.WaveId) + ";emitted=" + snap.Emitted);
                if (zone == 2) break;
                Require(f.Coordinator.TryBeginTransition() && !f.Coordinator.TryBeginTransition(), "transition CAS");
                f.Coordinator.Tick(.49f); float time = f.Coordinator.Elapsed;
                f.Coordinator.SetPaused(true); f.Coordinator.Tick(20f); Require(f.Coordinator.Elapsed == time && f.Host.Warps == zone, "paused clock");
                f.Coordinator.SetPaused(false); f.Coordinator.Tick(.20f); Require(f.Host.Warps == zone + 1, "commit");
                f.Coordinator.Tick(.57f); Require(f.Coordinator.TryAcknowledgeNextWave() && !f.Coordinator.TryAcknowledgeNextWave(), "next wave CAS");
            }
            f.Host.Ready = false; Require(!f.Coordinator.TryRequestVictory(), "P2 gate ignored");
            f.Host.Ready = true; Require(f.Coordinator.TryRequestVictory() && f.Coordinator.TryRequestVictory(), "victory replay result");
            Require(f.Host.Completions == 1 && f.Host.Outcome.Outcome == MorpgOutcome.Victory && f.Host.OwnedLocks == 0 && f.Host.ForeignLocks == 2, "host-only victory/owned release");
            Require(f.Host.Player == f.Host.Camera && f.Host.Camera == f.Host.Boundary && f.Host.Warps == 2, "mixed world");
            trace.AddRange(f.Host.Log);
        });
        test("p1-token-foreign-released-duplicate-mutation0", () =>
        {
            var f = create(); f.Coordinator.TryStartActiveWave(); f.Clear(); f.Coordinator.TryBeginTransition();
            var token = f.Host.LastToken; var record = f.Host.LastRecord;
            Require(f.Host.TryAcquireTransitionScope(record, out var duplicate, out _) && ReferenceEquals(token, duplicate) && f.Host.OwnedLocks == 1, "duplicate acquire");
            int log = f.Host.Log.Count;
            var other = create(); Require(!other.Host.TryReleaseTransitionScope(token, MorpgTransitionReleaseReason.Abort, out _), "foreign token release");
            Require(!f.Host.TryCommitWarp(new MorpgTransitionScopeToken(), f.Host.LastPlan, f.Coordinator.ActiveHandle, out _), "forged token");
            Require(f.Host.Log.Count == log, "stale token mutation");
            f.Coordinator.Tick(.70f);
            Require(f.Host.TryCommitWarp(token, f.Host.LastPlan, f.Host.LastPlan.Source, out _) && f.Host.Warps == 1, "duplicate committed warp");
            f.Coordinator.Tick(.55f);
            log = f.Host.Log.Count;
            Require(!f.Host.TryCommitWarp(token, f.Host.LastPlan, f.Host.LastPlan.Source, out _), "released token commit");
            Require(f.Host.TryReleaseTransitionScope(token, MorpgTransitionReleaseReason.Success, out _) && f.Host.Log.Count == log, "duplicate release mutation");
        });
        foreach (string point in new[] { "acquire", "prepare", "cleanup-prepare", "commit-before", "cleanup", "old-boundary", "player", "camera", "new-boundary", "cache", "release" })
        {
            string fault = point;
            test("p1-fault-" + fault, () =>
            {
                var f = create(); f.Coordinator.TryStartActiveWave(); f.Clear(); f.Host.Fault = fault;
                f.Coordinator.TryBeginTransition(); f.Coordinator.Tick(2f);
                Require(f.Coordinator.Phase == ZoneTransitionPhase.Failed && f.Host.Completions == 0, "fault not terminal: " + fault);
                bool irreversible = new[] { "cleanup", "old-boundary", "player", "camera", "new-boundary", "cache", "release" }.Contains(fault);
                if (irreversible) Require(f.Host.LastRecord.State == TransitionRecordState.Failed && f.Host.Outcome.Outcome == MorpgOutcome.Failed, "false restore: " + fault);
                else if (fault != "acquire") Require(f.Host.LastRecord.State == TransitionRecordState.Restored, "pre-mutation restoration");
                f.Coordinator.DisposeMorpgRoute(MorpgRouteStopReason.Teardown);
                Require(f.Host.OwnedLocks == 0 && f.Host.ForeignLocks == 2 && !f.Coordinator.TryAcknowledgeNextWave(), "fault leaked/restarted");
                Require(!f.Coordinator.TryRequestVictory(), "fault victory");
                trace.Add("fault=" + fault + ";transition=" + f.Host.LastRecord.State + ";outcome=" + f.Host.Outcome.Outcome);
            });
        }
        test("p1-abort-defeat-teardown-every-phase-new-attempt", () =>
        {
            foreach (float at in new[] { -2f, -1f, 0f, .51f, .70f, 1.06f, 1.26f })
            foreach (MorpgRouteStopReason reason in Enum.GetValues(typeof(MorpgRouteStopReason)))
            {
                var f = create(); f.Coordinator.TryStartActiveWave();
                if (at >= -1f) f.Clear();
                if (at >= 0f) { f.Coordinator.TryBeginTransition(); f.Coordinator.Tick(at); }
                f.Coordinator.SetPaused(true); f.Coordinator.DisposeMorpgRoute(reason); f.Coordinator.DisposeMorpgRoute(reason);
                Require(f.Host.OwnedLocks == 0 && f.Host.ForeignLocks == 2 && f.Coordinator.Phase == ZoneTransitionPhase.Abandoned, "interruption leak " + at);
                Require(!f.Coordinator.TryStartActiveWave() && !f.Coordinator.TryRequestVictory(), "abandoned resumed");
                var fresh = create(); Require(fresh.Coordinator.TryStartActiveWave(), "new attempt failed");
                Require(!fresh.Waves.TryStopMorpgZoneWave(f.Coordinator.ActiveHandle, MorpgZoneStopReason.Abort, out _), "new attempt touched by stale handle");
            }
        });
        test("p1-spawn-reward-failure-clear-ineligible", () =>
        {
            foreach (bool reward in new[] { false, true })
            {
                var f = create(); f.Coordinator.TryStartActiveWave();
                Require(f.Waves.ActiveScope.TryFail(f.Coordinator.ActiveHandle, reward), "failure report");
                Require(f.Waves.TryGetMorpgZoneClearSnapshot(f.Coordinator.ActiveHandle, out var snap, out _) && !snap.Eligible &&
                    !f.Coordinator.TryCompleteActiveWave(snap), "failure cleared");
            }
        });
    }

    private sealed class Fixture
    {
        internal readonly BattleMorpgZoneRewardDefinition Definition;
        internal readonly BattleAttemptKey Attempt;
        internal readonly MorpgZoneWaveOwner Waves;
        internal readonly FakeHost Host;
        internal readonly BattleMorpgZoneTransitionCoordinator Coordinator;
        internal Fixture(string json, string addendum)
        {
            Definition = BattleMorpgDefinitionCodec.Decode(json).Definition;
            Attempt = new BattleAttemptKey("run", "episode", Definition.battleId, "P1-attempt");
            Waves = new MorpgZoneWaveOwner(Attempt, Definition); Host = new FakeHost(Attempt, Waves, Definition);
            Require(BattleMorpgZoneTransitionCoordinator.TryCreate(true, Definition, addendum, Attempt, Host, Waves, true, out Coordinator, out string error), error);
        }
        internal void Clear()
        {
            var scope = Waves.ActiveScope;
            foreach (var row in Definition.zones[Coordinator.ActiveZoneIndex].reservations.Reverse())
            {
                var root = new Owned(scope.Handle, row.reservationId, MorpgOwnedKind.EnemyRoot);
                Require(scope.TryEmitted(scope.Handle, row.reservationId, root, out _) && !scope.TryEmitted(scope.Handle, row.reservationId, root, out _), "emission once");
                foreach (var kind in new[] { MorpgOwnedKind.HostileProjectile, MorpgOwnedKind.HostileEffect, MorpgOwnedKind.DelayedHit, MorpgOwnedKind.PathCache })
                    Require(scope.Registry.TryRegister(scope.Handle, new Owned(scope.Handle, row.reservationId + ":" + kind, kind), out _), "producer registration");
                Require(scope.TryDied(scope.Handle, row.reservationId) && !scope.TryDied(scope.Handle, row.reservationId), "death callback once");
            }
            Require(Waves.TryGetMorpgZoneClearSnapshot(scope.Handle, out var snap, out _) && Coordinator.TryCompleteActiveWave(snap), "clear commit");
        }
    }

    private sealed class Owned : IMorpgZoneOwnedHandle
    {
        internal int Disposals;
        internal bool RejectPrepare, ThrowDisposeOnce;
        internal Owned(MorpgZoneRuntimeHandle zone, string id, MorpgOwnedKind kind) { Zone = zone; StableId = id; Kind = kind; }
        public MorpgZoneRuntimeHandle Zone { get; }
        public string StableId { get; }
        public MorpgOwnedKind Kind { get; }
        public bool TryPrepareDispose(out string error) { error = RejectPrepare ? "prepare injected" : null; return !RejectPrepare; }
        public void DisposePrepared() { if (ThrowDisposeOnce) { ThrowDisposeOnce = false; throw new InvalidOperationException("dispose injected"); } if (Disposals == 0) Disposals++; }
    }

    // Model of the authorized host transaction; never instantiated by production.
    private sealed class FakeHost : IMorpgZoneBattleHost
    {
        private sealed class Scope
        {
            internal TransitionRecord Record; internal MorpgTransitionPlan Plan;
            internal MorpgZoneOwnedHandleRegistry Registry; internal MorpgZoneCleanupPlan Cleanup;
            internal bool Released, Irreversible; internal Action[] Operations;
        }
        private readonly BattleAttemptKey attempt;
        private readonly MorpgZoneWaveOwner waves;
        private readonly BattleMorpgZoneRewardDefinition definition;
        private readonly Dictionary<MorpgTransitionScopeToken, Scope> scopes = new Dictionary<MorpgTransitionScopeToken, Scope>();
        private readonly Dictionary<string, MorpgTransitionScopeToken> keys = new Dictionary<string, MorpgTransitionScopeToken>(StringComparer.Ordinal);
        internal readonly List<string> Log = new List<string>();
        internal readonly BattleOutcomeArbiter Outcome;
        internal string Fault;
        internal bool Ready = true;
        internal int OwnedLocks, ForeignLocks = 2, Warps, Completions;
        internal string Player = "left", Camera = "left", Boundary = "left";
        internal MorpgTransitionScopeToken LastToken;
        internal TransitionRecord LastRecord;
        internal MorpgTransitionPlan LastPlan;
        internal FakeHost(BattleAttemptKey attempt, MorpgZoneWaveOwner waves, BattleMorpgZoneRewardDefinition definition)
        { this.attempt = attempt; this.waves = waves; this.definition = definition; Outcome = new BattleOutcomeArbiter(attempt); }
        public bool TryAcquireTransitionScope(TransitionRecord record, out MorpgTransitionScopeToken token, out string error)
        {
            token = null; error = null;
            if (record == null || !record.Attempt.Equals(attempt)) return Reject("foreign transition", out error);
            string key = BattleMorpgCanonicalKeys.Transition(record.Attempt, record.Ordinal, record.FromZoneId, record.ToZoneId);
            if (keys.TryGetValue(key, out token)) return !scopes[token].Released && ReferenceEquals(scopes[token].Record, record);
            LastRecord = record;
            if (Fault == "acquire") return Reject("acquire", out error);
            if (record.State != TransitionRecordState.Requested || OwnedLocks != 0) return Reject("active transition", out error);
            token = LastToken = new MorpgTransitionScopeToken(); scopes.Add(token, new Scope { Record = record }); keys.Add(key, token);
            OwnedLocks++; Log.Add("acquire:" + key); return true;
        }
        public bool TryPrepareTransition(MorpgTransitionScopeToken token, MorpgTransitionPlan plan, MorpgZoneClearSnapshot clear, out string error)
        {
            error = null;
            if (!Live(token, out var state)) return Reject("stale prepare", out error);
            if (state.Plan != null) return ReferenceEquals(state.Plan, plan);
            if (Fault == "prepare" || Fault == "cleanup-prepare") return Reject(Fault, out error);
            int destination = state.Record.Ordinal;
            if (plan == null || clear == null || !clear.Eligible || !ReferenceEquals(clear.Handle, plan.Source) ||
                !ReferenceEquals(waves.ActiveScope.Handle, plan.Source) || !Ready || destination < 1 || destination > 2 ||
                plan.DestinationZone != definition.zones[destination].id || plan.DestinationWave != definition.zones[destination].waveId ||
                plan.Anchor != definition.zones[destination].entryWarpAnchorId) return Reject("invalid prepared plan", out error);
            state.Registry = waves.ActiveScope.Registry;
            if (!state.Registry.TryPrepareDisposeAll(plan.Source, out state.Cleanup, out error)) return false;
            state.Plan = LastPlan = plan;
            // Capture every destination operation before commit. No runtime lookup in this transaction.
            string target = plan.DestinationZone;
            state.Operations = new Action[] { () => Boundary = "covered", () => Player = target, () => Camera = target, () => Boundary = target, () => { } };
            Log.Add("prepare:" + target); return state.Record.TryPrepare();
        }
        public bool TryCommitWarp(MorpgTransitionScopeToken token, MorpgTransitionPlan plan, MorpgZoneRuntimeHandle source, out string error)
        {
            error = null;
            if (!Live(token, out var state) || !ReferenceEquals(state.Plan, plan) || plan == null || !ReferenceEquals(plan.Source, source))
                return Reject("stale commit", out error);
            if (state.Record.State == TransitionRecordState.Committed) return true;
            if (state.Record.State != TransitionRecordState.Prepared || Fault == "commit-before") return Reject("commit-before", out error);
            state.Irreversible = true;
            try
            {
                if (!state.Registry.TryDisposeAll(state.Cleanup, out error)) throw new InvalidOperationException(error);
                Inject("cleanup");
                string[] names = { "old-boundary", "player", "camera", "new-boundary", "cache" };
                for (int i = 0; i < state.Operations.Length; i++) { state.Operations[i](); Log.Add("commit:" + names[i]); Inject(names[i]); }
                Require(state.Record.TryCommit(), "host record commit"); Warps++; return true;
            }
            catch (Exception e) { TerminalFailure(state); return Reject(e.Message, out error); }
        }
        public bool TryReleaseTransitionScope(MorpgTransitionScopeToken token, MorpgTransitionReleaseReason reason, out string error)
        {
            error = null;
            if (token == null || !scopes.TryGetValue(token, out var state)) return Reject("foreign release", out error);
            if (state.Released) return true;
            if (reason != MorpgTransitionReleaseReason.Success)
            {
                if (state.Irreversible) TerminalFailure(state); else state.Record.TryRestore();
            }
            state.Released = true; OwnedLocks--; Log.Add("release:" + reason);
            if (Fault == "release") { TerminalFailure(state); return Reject("release", out error); }
            return true;
        }
        public bool TryCompleteFinalZone(BattleAttemptKey key, WaveClearKey clear, out string error)
        {
            error = null;
            if (!key.Equals(attempt) || !clear.Attempt.Equals(attempt) || clear.ZoneId != definition.zones[2].id || clear.WaveId != definition.zones[2].waveId)
                return Reject("foreign final clear", out error);
            if (Outcome.Outcome == MorpgOutcome.Victory) return true;
            if (!Ready || OwnedLocks != 0 || !waves.TryGetMorpgZoneClearSnapshot(waves.ActiveScope.Handle, out var snapshot, out error) ||
                !snapshot.Eligible || !snapshot.Handle.ClearKey.Equals(clear)) return Reject("final gate closed", out error);
            var registry = waves.ActiveScope.Registry;
            if (!registry.TryPrepareDisposeAll(snapshot.Handle, out var cleanup, out error) || !registry.TryDisposeAll(cleanup, out error)) return false;
            if (!Outcome.TryRequest() || !Outcome.TryCommit(MorpgOutcome.Victory)) return Reject("outcome CAS", out error);
            Completions++; Log.Add("victory:" + BattleMorpgCanonicalKeys.Clear(key, clear.ZoneId, clear.WaveId)); return true;
        }
        private bool Live(MorpgTransitionScopeToken token, out Scope scope)
        { scope = null; return token != null && scopes.TryGetValue(token, out scope) && !scope.Released; }
        private static bool Reject(string message, out string error) { error = message; return false; }
        private void Inject(string point) { if (Fault == point) throw new InvalidOperationException(point); }
        private void TerminalFailure(Scope state)
        {
            state.Record.TryFail(); if (Outcome.Outcome == MorpgOutcome.None) Outcome.TryRequest(); Outcome.TryCommit(MorpgOutcome.Failed);
            if (state.Cleanup != null) state.Registry.TryDisposeAll(state.Cleanup, out _);
            Player = Camera = Boundary = "failed";
        }
    }
}
