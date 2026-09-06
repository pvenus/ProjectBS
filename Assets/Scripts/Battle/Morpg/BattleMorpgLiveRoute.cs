using System;
using System.Collections.Generic;
using Character;
using Currency;
using Party;
using Session;
using Stat;
using UnityEngine;

namespace Battle.Morpg
{
    // SpawnManager owns this attempt; BattleManager alone performs the existing victory handoff.
    internal sealed class BattleMorpgLiveRoute : IMorpgZoneBattleHost, IMorpgZoneWaveDriver, IDisposable
    {
        private static readonly HashSet<CharacterManager> transitionPlayers = new();
        internal static bool IsTransitionLocked(CharacterManager character) => transitionPlayers.Contains(character);
        private const string Resource = "battle/act1/chapter01/battle.act1.chapter01.01.rescue_villagers.morpg-zone-reward";
        private readonly BattleSession session;
        private readonly BattleMorpgZoneRewardDefinition definition;
        private readonly BattleAttemptKey attempt;
        private readonly MorpgZoneWaveOwner waves;
        private readonly Dictionary<string, CharacterSO> units;
        private readonly Transform parent;
        private readonly CurrencyRutimeData currency;
        private readonly Dictionary<CharacterManager, string> roots = new();
        private readonly Dictionary<Behaviour, bool> locked = new();
        private BattleMorpgZoneTransitionCoordinator coordinator;
        private BattleRewardLedger ledger;
        private CharacterManager player;
        private GameObject staging;
        private MorpgTransitionScopeToken token;
        private TransitionRecord record;
        private MorpgTransitionPlan preparedPlan;
        private float elapsed;
        private int next, clearFrame = -1, creditedGold, creditedQuarters;
        private long deaths;
        private bool disposed, failed, activated;
        private string failure;
        internal bool VictoryReady => !failed && coordinator.Phase == ZoneTransitionPhase.Completed;

        private BattleMorpgLiveRoute(BattleSession session, BattleMorpgZoneRewardDefinition definition,
            BattleAttemptKey attempt, Dictionary<string, CharacterSO> units, Transform parent, CurrencyRutimeData currency)
        {
            this.session = session; this.definition = definition; this.attempt = attempt;
            this.units = units; this.parent = parent; this.currency = currency;
            waves = new MorpgZoneWaveOwner(attempt, definition);
        }

        internal static bool TryCreate(bool enabled, BattleSession session, ISpawnUnitResolver resolver,
            Transform parent, out BattleMorpgLiveRoute route, out string error)
        {
            route = null; error = "feature gate inactive";
            if (!enabled || session?.BattleSO?.BattleId != BattleMorpgDefinitionValidator.BattleId) return false;
            try
            {
                var text = Resources.Load<TextAsset>(Resource + ".v1");
                var addendum = Resources.Load<TextAsset>(Resource + ".p0-addendum.v1");
                var decoded = BattleMorpgDefinitionCodec.Decode(text != null ? text.text : null);
                if (decoded.IsLegacy || !decoded.Success || addendum == null)
                { error = decoded.Error ?? "definition/addendum missing"; return false; }
                var d = decoded.Definition;
                if (d.drops[0].unitKey != "black" || d.drops[0].gold != 1 || d.drops[0].xp != .25f ||
                    d.drops[1].unitKey != "chain" || d.drops[1].gold != 3 || d.drops[1].xp != .75f)
                { error = "unsupported reward table"; return false; }
                var units = new Dictionary<string, CharacterSO>();
                foreach (string key in new[] { "black", "chain" })
                {
                    var unit = resolver?.Resolve(new SpawnUnitRequest(key, SpawnUnitRole.Melee));
                    if (unit == null) { error = "unresolved producer: " + key; return false; }
                    units.Add(key, unit);
                }
                var currency = GameSession.Instance?.StageSession?.CurrencyRuntimeData;
                if (currency == null || currency.gold < 0 || currency.gold > int.MaxValue - 40 ||
                    currency.Revision > int.MaxValue - 28)
                { error = "reward account/headroom unavailable"; return false; }
                string run = GameSession.Instance?.ProgressionSession?.RunId.Value;
                // Direct battle test scenes have no durable progression run; identity stays attempt-local.
                string id = Guid.NewGuid().ToString("N");
                var attempt = new BattleAttemptKey(string.IsNullOrEmpty(run) ? "direct-" + id : run,
                    "act1.chapter01", d.battleId, id);
                var candidate = new BattleMorpgLiveRoute(session, d, attempt, units, parent, currency);
                if (!BattleMorpgZoneTransitionCoordinator.TryCreate(true, d, addendum.text, attempt,
                    candidate, candidate, true, out candidate.coordinator, out error)) return false;
                route = candidate; return true;
            }
            catch (Exception e) { error = "preflight: " + e.Message; return false; }
        }

        internal void Activate()
        {
            activated = true;
            staging = new GameObject("MORPG inactive producer staging");
            staging.transform.SetParent(parent, false);
            staging.SetActive(false);
            CharacterManager.OnAnyCharacterDied += OnDied;
            BattleMapBoundsContext.Activate(new Vector2(32f, 18f), .75f);
        }

        internal void Tick(float delta)
        {
            if (disposed || failed || VictoryReady || !activated) return;
            if (Time.timeScale <= 0f || delta <= 0f) return;
            try
            {
                if (player == null)
                {
                    var members = PartyManager.Instance?.Members;
                    if (members == null || members.Count == 0) return;
                    if (members.Count != 1 || members[0]?.RuntimeData == null)
                    { Fail("exact route requires one initialized party member"); return; }
                    player = members[0];
                    float xp = player.GetStatValue(StatType.Experience);
                    if (float.IsNaN(xp) || float.IsInfinity(xp) || xp < 0 || xp > 1000000f || xp * 4f != (float)Math.Truncate(xp * 4f))
                    { Fail("raw XP headroom unavailable"); return; }
                    ledger = new BattleRewardLedger(new GoldAccount(currency), new XpAccount(player));
                    WarpTo(0);
                    if (!coordinator.TryStartActiveWave()) { Fail(coordinator.LastError); return; }
                }
                if (player.RuntimeData.isDead)
                { Fail("party defeated", MorpgRouteStopReason.Defeat); return; }
                coordinator.Tick(delta);
                if (coordinator.Phase == ZoneTransitionPhase.Failed) { Fail(coordinator.LastError); return; }
                if (coordinator.Phase == ZoneTransitionPhase.NextWaveReady)
                {
                    if (!coordinator.TryAcknowledgeNextWave()) Fail(coordinator.LastError);
                    return;
                }
                if (coordinator.Phase != ZoneTransitionPhase.Idle) return;
                elapsed += delta;
                var zone = definition.zones[coordinator.ActiveZoneIndex];
                while (next < zone.reservations.Length && zone.reservations[next].localDueTime <= elapsed)
                {
                    var reservation = zone.reservations[next++];
                    var scope = waves.ActiveScope;
                    var enemy = NpcSpawnService.Instance.SpawnNpc(units[reservation.unitKey],
                        new Vector3(reservation.position[0], reservation.position[1], 0f), 0f, null,
                        staging.transform, root => RegisterRoot(scope, reservation.reservationId, root));
                    if (enemy == null) { scope.TryFail(scope.Handle, false); Fail("enemy producer failed"); return; }
                }
                if (!TryGetMorpgZoneClearSnapshot(coordinator.ActiveHandle, out var clear, out string error))
                { Fail(error); return; }
                if (clear.SpawnFailed || clear.RewardTransactionFailed) { Fail("zone accounting failed"); return; }
                if (!clear.Eligible) { clearFrame = -1; return; }
                // Let the entire death callback/frame finish, including a simultaneous player death.
                if (clearFrame < 0) { clearFrame = Time.frameCount; return; }
                if (Time.frameCount <= clearFrame) return;
                if (!coordinator.TryCompleteActiveWave(clear)) return;
                if (coordinator.ActiveZoneIndex == 2)
                {
                    if (!coordinator.TryRequestVictory()) Fail(coordinator.LastError);
                }
                else if (!coordinator.TryBeginTransition()) Fail(coordinator.LastError);
            }
            catch (Exception e) { Fail("runtime: " + e.Message); }
        }

        private bool RegisterRoot(MorpgZoneWaveScope scope, string reservation, GameObject root)
        {
            var character = root.GetComponent<CharacterManager>();
            if (character == null) return false;
            var marker = root.AddComponent<MorpgOwnedObject>();
            marker.Bind(scope, reservation, MorpgOwnedKind.EnemyRoot, staging.transform);
            if (!scope.TryEmitted(scope.Handle, reservation, marker, out _)) return false;
            roots.Add(character, reservation);
            return true;
        }

        private void OnDied(CharacterManager character)
        {
            if (disposed || failed || character == null || !roots.TryGetValue(character, out string reservation)) return;
            var scope = waves.ActiveScope;
            var marker = character.GetComponent<MorpgOwnedObject>();
            if (marker == null || !ReferenceEquals(marker.Zone, scope.Handle) ||
                !scope.TryDied(scope.Handle, reservation)) return;
            marker.MarkDied();
            bool chain = units["chain"] == character.RuntimeData.characterSO;
            int gold = chain ? 3 : 1, quarters = chain ? 3 : 1;
            try
            {
                if (!ledger.TryCredit(BattleMorpgCanonicalKeys.Bundle(attempt, reservation), ++deaths,
                    reservation, gold, quarters, out string error))
                { scope.TryFail(scope.Handle, true); Fail(error); return; }
                creditedGold += gold; creditedQuarters += quarters;
            }
            catch (Exception e)
            { scope.TryFail(scope.Handle, true); Fail("reward callback: " + e.Message); }
        }

        public bool TryStartMorpgZoneWave(BattleAttemptKey key, int ordinal, out MorpgZoneRuntimeHandle handle, out string error)
        {
            var prior = waves.ActiveScope?.Handle;
            if (!waves.TryStartMorpgZoneWave(key, ordinal, out handle, out error)) return false;
            if (!ReferenceEquals(prior, handle)) { elapsed = 0f; next = 0; clearFrame = -1; roots.Clear(); }
            return true;
        }
        public bool TryGetMorpgZoneClearSnapshot(MorpgZoneRuntimeHandle handle, out MorpgZoneClearSnapshot snapshot, out string error) =>
            waves.TryGetMorpgZoneClearSnapshot(handle, out snapshot, out error);
        public bool TryStopMorpgZoneWave(MorpgZoneRuntimeHandle handle, MorpgZoneStopReason reason, out string error) =>
            waves.TryStopMorpgZoneWave(handle, reason, out error);

        public bool TryAcquireTransitionScope(TransitionRecord transition, out MorpgTransitionScopeToken acquired, out string error)
        {
            acquired = null; error = "foreign/duplicate transition";
            if (token != null || transition == null || !transition.Attempt.Equals(attempt) ||
                transition.FromZoneId != coordinator.ActiveHandle.ZoneId || transition.Ordinal != coordinator.ActiveZoneIndex + 1)
                return false;
            record = transition; acquired = token = new MorpgTransitionScopeToken(); error = null; return true;
        }
        public bool TryPrepareTransition(MorpgTransitionScopeToken scope, MorpgTransitionPlan plan,
            MorpgZoneClearSnapshot clear, out string error)
        {
            error = "invalid transition preparation";
            if (!ReferenceEquals(scope, token) || token == null || plan == null ||
                !ReferenceEquals(plan.Source, coordinator.ActiveHandle) || !clear.Eligible ||
                !ReferenceEquals(clear.Handle, plan.Source) || player == null || player.RuntimeData.isDead) return false;
            var destination = definition.zones[record.Ordinal];
            if (plan.DestinationZone != destination.id || plan.DestinationWave != destination.waveId ||
                plan.Anchor != destination.entryWarpAnchorId) return false;
            preparedPlan = plan;
            LockPlayer();
            // Every owned producer has already prepared disposal through the clear snapshot.
            if (!TryStopMorpgZoneWave(plan.Source, MorpgZoneStopReason.Transition, out error)) return false;
            return record.TryPrepare();
        }
        public bool TryCommitWarp(MorpgTransitionScopeToken scope, MorpgTransitionPlan plan,
            MorpgZoneRuntimeHandle source, out string error)
        {
            error = "invalid warp authority";
            if (token == null || !ReferenceEquals(scope, token) || !ReferenceEquals(plan, preparedPlan) ||
                !ReferenceEquals(source, plan.Source) || !waves.ActiveScope.Registry.IsDisposed ||
                record.State != TransitionRecordState.Prepared || player == null || player.RuntimeData.isDead) return false;
            WarpTo(record.Ordinal);
            error = null; return record.TryCommit();
        }
        public bool TryReleaseTransitionScope(MorpgTransitionScopeToken scope, MorpgTransitionReleaseReason reason, out string error)
        {
            error = "foreign release";
            if (token == null || !ReferenceEquals(scope, token)) return false;
            if (reason != MorpgTransitionReleaseReason.Success) record.TryFail();
            UnlockPlayer(); token = null; preparedPlan = null; error = null; return true;
        }
        public bool TryCompleteFinalZone(BattleAttemptKey key, WaveClearKey clear, out string error)
        {
            error = "final settlement not ready";
            if (!key.Equals(attempt) || coordinator.ActiveZoneIndex != 2 ||
                !clear.Equals(coordinator.ActiveHandle.ClearKey) || player == null || player.RuntimeData.isDead ||
                deaths != 28 || creditedGold != 40 || creditedQuarters != 40 || ledger.Bundles.Count != 28) return false;
            if (!TryStopMorpgZoneWave(coordinator.ActiveHandle, MorpgZoneStopReason.Transition, out error)) return false;
            // Per-death raw XP has settled. Do not offer the same final XP payload again.
            session.BattleRuntime.rewardExperience = 0f;
            error = null; return true;
        }

        private void LockPlayer()
        {
            transitionPlayers.Add(player);
            player.GetComponent<MovementMono>()?.StopAllMotion();
            player.GetComponent<CharacterSkillManager>()?.CancelCasting();
            var movement = player.GetComponent<PartyMovementMono>();
            if (movement != null && !movement.TryAcquireExternalMovement(token, true))
                throw new InvalidOperationException("player movement scope already owned");
            foreach (var behaviour in player.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (!(behaviour is CharacterManager || behaviour is CharacterSkillManager ||
                    behaviour is MovementMono || behaviour is MovementController || behaviour is KnockbackController ||
                    behaviour is PartyMovementMono || behaviour is SkillBrainMono || behaviour is SkillExecutorMono)) continue;
                locked[behaviour] = behaviour.enabled;
                if (behaviour.enabled) behaviour.enabled = false;
            }
        }
        private void UnlockPlayer()
        {
            transitionPlayers.Remove(player);
            if (player != null) player.GetComponent<PartyMovementMono>()?.ReleaseExternalMovement(token);
            foreach (var pair in locked) if (pair.Key != null && pair.Value) pair.Key.enabled = true;
            locked.Clear();
        }
        private void WarpTo(int ordinal)
        {
            var zone = definition.zones[ordinal];
            BattleMapBoundsContext.SetActorZone(Rect.MinMaxRect(zone.bounds[0], zone.bounds[1], zone.bounds[2], zone.bounds[3]));
            player.GetComponent<MovementMono>()?.StopAllMotion();
            Vector3 position = new Vector3(zone.entry[0], zone.entry[1], player.transform.position.z);
            player.transform.position = position;
            var body = player.GetComponent<Rigidbody2D>();
            if (body != null) { body.position = position; body.linearVelocity = Vector2.zero; body.angularVelocity = 0f; }
            var camera = Camera.main;
            if (camera != null)
            {
                Vector2 center = BattleMapBoundsContext.ClampCameraCenter(position, camera);
                camera.transform.position = new Vector3(center.x, center.y, camera.transform.position.z);
                camera.GetComponent<BattlePlayerCameraFollowMono>()?.ResetSmoothing();
            }
        }
        private void Fail(string error, MorpgRouteStopReason reason = MorpgRouteStopReason.Failed)
        {
            if (failed) return;
            failed = true; failure = error;
            coordinator.DisposeMorpgRoute(reason);
            UnlockPlayer();
            Debug.LogError("[MORPG] Attempt stopped (no legacy replay): " + error);
        }
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            CharacterManager.OnAnyCharacterDied -= OnDied;
            coordinator.DisposeMorpgRoute(MorpgRouteStopReason.Teardown);
            UnlockPlayer();
            if (staging != null) UnityEngine.Object.Destroy(staging);
        }
        internal void DrawHud()
        {
            if (!activated || disposed) return;
            if (token != null)
            {
                float t = coordinator.Elapsed;
                float alpha = t < .68f ? Mathf.Clamp01((t - .50f) / .18f) : Mathf.Clamp01((1.05f - t) / .37f);
                Color previous = GUI.color;
                GUI.color = new Color(0f, 0f, 0f, alpha);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
                GUI.color = previous;
            }
            GUI.Box(new Rect(16, 16, 340, 76), "");
            GUI.Label(new Rect(28, 24, 320, 26), $"Zone {coordinator.ActiveZoneIndex + 1}/3   Gold +{creditedGold}   XP +{creditedQuarters / 4m:0.##}");
            string status = failed ? "Battle stopped. Restart required." : VictoryReady ? "Victory" :
                token != null ? "Moving to the next zone..." : $"Defeated {deaths}/28";
            GUI.Label(new Rect(28, 50, 320, 26), status);
        }

        private sealed class GoldAccount : IRevisionedRewardAccount
        {
            private readonly CurrencyRutimeData data;
            internal GoldAccount(CurrencyRutimeData data) { this.data = data; }
            public decimal Value => data.gold;
            public long Revision => data.Revision;
            public bool TryApply(decimal delta, long revision) => revision == Revision && delta <= int.MaxValue &&
                decimal.Truncate(delta) == delta && data.TryAddGoldExact((int)delta);
            public bool TryRestore(decimal value, long revision) => revision == Revision &&
                data.TryRestoreSnapshot(new CurrencyRuntimeSnapshot((int)value, (int)revision - 1));
        }
        private sealed class XpAccount : IRevisionedRewardAccount
        {
            private readonly CharacterManager character;
            private long revision;
            private decimal observed;
            internal XpAccount(CharacterManager character) { this.character = character; observed = Value; }
            public decimal Value => (decimal)character.GetStatValue(StatType.Experience);
            public long Revision { get { if (Value != observed) { observed = Value; revision++; } return revision; } }
            public bool TryApply(decimal delta, long expected)
            {
                if (expected != Revision || character.RuntimeData.isDead) return false;
                decimal before = Value, next = before + delta;
                try { character.SetStat(StatType.Experience, (float)next); }
                catch
                {
                    // A stat notification can throw after writing. Restore only our exact write.
                    if (Value == next)
                        try { character.SetStat(StatType.Experience, (float)before); } catch { }
                    observed = Value; revision++;
                    return false;
                }
                observed = Value; revision++;
                return observed == next;
            }
            public bool TryRestore(decimal value, long expected)
            {
                if (expected != Revision) return false;
                character.SetStat(StatType.Experience, (float)value);
                observed = Value; revision++;
                return observed == value;
            }
        }
    }
}
