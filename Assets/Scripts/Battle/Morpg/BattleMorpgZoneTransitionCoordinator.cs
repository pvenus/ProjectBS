using System;

namespace Battle.Morpg
{
    internal enum ZoneTransitionPhase { Idle, CleanupSettling, Fading, Warped, Unlocked, NextWaveReady, Completed, Failed, Abandoned }

    // Sequencing only. A manager must supply the host and zone driver; no live caller exists in P1.
    internal sealed class BattleMorpgZoneTransitionCoordinator
    {
        internal const float FadeAt = .50f, WarpAt = .68f, UnlockAt = 1.05f, NextWaveAt = 1.25f;
        private readonly IMorpgZoneBattleHost host;
        private readonly IMorpgZoneWaveDriver waves;
        private readonly BattleAttemptKey attempt;
        private readonly string[] zones = new string[3], waveIds = new string[3], anchors = new string[3];
        private readonly int[] counts = new int[3];
        private MorpgZoneRuntimeHandle active;
        private MorpgZoneClearSnapshot clear;
        private TransitionRecord transition;
        private MorpgTransitionPlan plan;
        private MorpgTransitionScopeToken token;
        private bool paused, clearCommitted, victoryAccepted;
        private float elapsed;

        private BattleMorpgZoneTransitionCoordinator(BattleMorpgZoneRewardDefinition definition, BattleAttemptKey attempt,
            IMorpgZoneBattleHost host, IMorpgZoneWaveDriver waves)
        {
            this.attempt = attempt; this.host = host; this.waves = waves;
            for (int i = 0; i < 3; i++)
            { zones[i] = definition.zones[i].id; waveIds[i] = definition.zones[i].waveId;
              anchors[i] = definition.zones[i].entryWarpAnchorId; counts[i] = definition.zones[i].reservations.Length; }
        }
        internal int ActiveZoneIndex { get; private set; }
        internal MorpgZoneRuntimeHandle ActiveHandle => active;
        internal ZoneTransitionPhase Phase { get; private set; }
        internal float Elapsed => elapsed;
        internal string LastError { get; private set; }
        internal bool IsPaused => paused;
        internal TransitionRecordState? TransitionState => transition?.State;

        internal static bool TryCreate(bool enabled, BattleMorpgZoneRewardDefinition definition, string addendum,
            BattleAttemptKey attempt, IMorpgZoneBattleHost host, IMorpgZoneWaveDriver waves, bool producersReady,
            out BattleMorpgZoneTransitionCoordinator coordinator, out string error)
        {
            coordinator = null; error = null;
            if (!enabled) { error = "inactive: preactivation legacy v3/v2 fallback"; return false; }
            if (host == null || waves == null || !producersReady || attempt.BattleId != BattleMorpgDefinitionValidator.BattleId ||
                string.IsNullOrEmpty(attempt.RunId) || string.IsNullOrEmpty(attempt.EpisodeId) || string.IsNullOrEmpty(attempt.AttemptId))
            { error = "preactivation legacy fallback: host/producer/attempt incomplete"; return false; }
            if (!BattleMorpgDefinitionValidator.TryValidate(definition, out error) ||
                !BattleMorpgP0AddendumCodec.TryValidate(addendum, out _, out error)) return false;
            coordinator = new BattleMorpgZoneTransitionCoordinator(definition, attempt, host, waves); return true;
        }

        internal bool TryStartActiveWave()
        {
            if (paused || Phase != ZoneTransitionPhase.Idle || active != null) return false;
            if (!waves.TryStartMorpgZoneWave(attempt, ActiveZoneIndex, out var handle, out string error)) return Fail(error);
            if (!Matches(handle)) return Fail("driver returned foreign handle");
            active = handle; return true;
        }
        internal bool TryCompleteActiveWave(MorpgZoneClearSnapshot snapshot)
        {
            if (paused || Phase != ZoneTransitionPhase.Idle || clearCommitted || active == null || snapshot == null ||
                !ReferenceEquals(snapshot.Handle, active) || !snapshot.Eligible || snapshot.Declared != counts[ActiveZoneIndex]) return false;
            // A callback snapshot cannot clear on its own: re-read exact-handle source authority.
            if (!waves.TryGetMorpgZoneClearSnapshot(active, out var current, out string error)) return Fail(error);
            if (!ReferenceEquals(current.Handle, active) || !current.Eligible || current.Declared != counts[ActiveZoneIndex]) return false;
            var record = new ClearRecord();
            if (!record.TryDetect() || !record.TryCommit()) return false;
            clear = current; clearCommitted = true; return true;
        }
        internal bool TryBeginTransition()
        {
            if (paused || Phase != ZoneTransitionPhase.Idle || !clearCommitted || ActiveZoneIndex >= 2) return false;
            transition = new TransitionRecord(attempt, zones[ActiveZoneIndex], zones[ActiveZoneIndex + 1], ActiveZoneIndex + 1);
            plan = new MorpgTransitionPlan(active, zones[ActiveZoneIndex + 1], waveIds[ActiveZoneIndex + 1], anchors[ActiveZoneIndex + 1]);
            try
            {
                if (!host.TryAcquireTransitionScope(transition, out token, out string error)) return Fail(error);
                if (token == null) return Fail("host returned no token");
                if (!host.TryPrepareTransition(token, plan, clear, out error)) return Fail(error);
                // Host owns the transaction record; coordinator only observes it.
                if (transition.State != TransitionRecordState.Prepared) return Fail("host failed to record preparation");
                elapsed = 0f; Phase = ZoneTransitionPhase.CleanupSettling; return true;
            }
            catch (Exception e) { return Fail(e.GetType().Name); }
        }
        internal void Tick(float deltaTime)
        {
            if (paused || float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime < 0f ||
                Phase == ZoneTransitionPhase.Idle || Phase == ZoneTransitionPhase.NextWaveReady ||
                Phase == ZoneTransitionPhase.Completed || Phase == ZoneTransitionPhase.Failed || Phase == ZoneTransitionPhase.Abandoned) return;
            elapsed += deltaTime;
            try
            {
                if (elapsed >= FadeAt && Phase == ZoneTransitionPhase.CleanupSettling) Phase = ZoneTransitionPhase.Fading;
                if (elapsed >= WarpAt && (Phase == ZoneTransitionPhase.CleanupSettling || Phase == ZoneTransitionPhase.Fading))
                {
                    if (!host.TryCommitWarp(token, plan, active, out string error)) { Fail(error); return; }
                    if (transition.State != TransitionRecordState.Committed) { Fail("host failed to record commit"); return; }
                    ActiveZoneIndex++; Phase = ZoneTransitionPhase.Warped;
                }
                if (elapsed >= UnlockAt && Phase == ZoneTransitionPhase.Warped)
                {
                    if (!Release(MorpgTransitionReleaseReason.Success)) { Fail(LastError); return; }
                    Phase = ZoneTransitionPhase.Unlocked;
                }
                if (elapsed >= NextWaveAt && Phase == ZoneTransitionPhase.Unlocked) Phase = ZoneTransitionPhase.NextWaveReady;
            }
            catch (Exception e) { Fail(e.GetType().Name); }
        }
        internal bool TryAcknowledgeNextWave()
        {
            if (paused || Phase != ZoneTransitionPhase.NextWaveReady) return false;
            active = null; clear = null; clearCommitted = false; transition = null; plan = null;
            Phase = ZoneTransitionPhase.Idle; return TryStartActiveWave();
        }
        internal bool TryRequestVictory()
        {
            if (victoryAccepted) return true;
            if (paused || Phase != ZoneTransitionPhase.Idle || ActiveZoneIndex != 2 || !clearCommitted) return false;
            try
            {
                if (!host.TryCompleteFinalZone(attempt, active.ClearKey, out string error)) { LastError = error; return false; }
                victoryAccepted = true; Phase = ZoneTransitionPhase.Completed; return true;
            }
            catch (Exception e) { return Fail(e.GetType().Name); }
        }
        internal void SetPaused(bool value) { paused = value; }
        internal void DisposeMorpgRoute(MorpgRouteStopReason reason)
        {
            if (Phase == ZoneTransitionPhase.Completed || Phase == ZoneTransitionPhase.Abandoned) return;
            var release = reason == MorpgRouteStopReason.Failed ? MorpgTransitionReleaseReason.Failed :
                reason == MorpgRouteStopReason.Defeat ? MorpgTransitionReleaseReason.Defeat :
                reason == MorpgRouteStopReason.Teardown ? MorpgTransitionReleaseReason.Teardown : MorpgTransitionReleaseReason.Abort;
            bool released = Release(release);
            bool stopped = StopActive(reason == MorpgRouteStopReason.Failed ? MorpgZoneStopReason.Failed :
                reason == MorpgRouteStopReason.Defeat ? MorpgZoneStopReason.Defeat :
                reason == MorpgRouteStopReason.Teardown ? MorpgZoneStopReason.Teardown : MorpgZoneStopReason.Abort);
            Phase = released && stopped ? ZoneTransitionPhase.Abandoned : ZoneTransitionPhase.Failed;
        }
        private bool Matches(MorpgZoneRuntimeHandle handle) => handle != null && handle.Attempt.Equals(attempt) &&
            handle.ZoneId == zones[ActiveZoneIndex] && handle.WaveId == waveIds[ActiveZoneIndex] && handle.Generation > 0;
        private bool Release(MorpgTransitionReleaseReason reason)
        {
            if (token == null) return true;
            try
            {
                if (!host.TryReleaseTransitionScope(token, reason, out string error)) { LastError = error; return false; }
                token = null; return true;
            }
            catch (Exception e) { LastError = e.GetType().Name; return false; }
        }
        private bool StopActive(MorpgZoneStopReason reason)
        {
            if (active == null) return true;
            try
            {
                if (waves.TryStopMorpgZoneWave(active, reason, out string error)) return true;
                LastError = error; return false;
            }
            catch (Exception e) { LastError = e.GetType().Name; return false; }
        }
        private bool Fail(string error)
        {
            LastError = error;
            // The host distinguishes pre-mutation Restored from post-mutation Failed. Never infer restoration here.
            Release(MorpgTransitionReleaseReason.Failed);
            StopActive(MorpgZoneStopReason.Failed);
            Phase = ZoneTransitionPhase.Failed; return false;
        }
    }
}
