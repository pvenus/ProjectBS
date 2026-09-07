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
    internal enum MorpgFinalSettlementState { NotStarted, FinalSettlementPending, Completed, Failed, Defeated, Abandoned }

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
        private readonly Dictionary<string, int> rewardByReservation = new(StringComparer.Ordinal);
        private readonly Dictionary<Behaviour, bool> locked = new();
        private BattleMorpgZoneTransitionCoordinator coordinator;
        private BattleRewardLedger ledger;
        private MorpgRewardDeliveryQueue delivery;
        private MorpgRewardHudMono rewardHud;
        private MorpgEnvironmentRuntime environment;
        private CharacterManager player;
        private GameObject staging;
        private MorpgTransitionScopeToken token;
        private TransitionRecord record;
        private MorpgTransitionPlan preparedPlan;
        private float elapsed;
        private int next, clearFrame = -1, creditedGold, creditedQuarters;
        private long deaths;
        private bool disposed, failed, activated;
        internal const float FinalSettlementTimeoutSeconds = 1.25f;
        private MorpgTransitionScopeToken finalSettlementToken;
        private float finalSettlementElapsed, initialXp;
        private int initialGold;
        internal MorpgFinalSettlementState SettlementState { get; private set; }
        internal float FinalSettlementElapsed => finalSettlementElapsed;
        internal bool FinalSettlementTimedOut { get; private set; }
        private string failure;
        internal bool VictoryReady => !failed && coordinator.Phase == ZoneTransitionPhase.Completed;

        private BattleMorpgLiveRoute(BattleSession session, BattleMorpgZoneRewardDefinition definition,
            BattleAttemptKey attempt, Dictionary<string, CharacterSO> units, Transform parent, CurrencyRutimeData currency)
        {
            this.session = session; this.definition = definition; this.attempt = attempt;
            this.units = units; this.parent = parent; this.currency = currency;
            waves = new MorpgZoneWaveOwner(attempt, definition);
            foreach (var zone in definition.zones)
                foreach (var row in zone.reservations)
                    rewardByReservation.Add(row.reservationId, row.unitKey == "chain" ? 3 : 1);
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
                if (session.BattleRuntime?.backgroundSprite == null)
                { error = "environment background binding missing"; return false; }
                if (!MorpgEnvironmentRuntime.TryLoad(d, out candidate.environment, out error)) return false;
                route = candidate; return true;
            }
            catch (Exception e) { error = "preflight: " + e.Message; return false; }
        }

        internal bool Activate()
        {
            try
            {
                // Build the complete profile before control; partial creation is disposed before fallback.
                environment.Activate(parent, session.BattleRuntime.backgroundSprite);
                staging = new GameObject("MORPG inactive producer staging");
                staging.transform.SetParent(parent, false);
                staging.SetActive(false);
                CharacterManager.OnAnyCharacterDied += OnDied;
                BattleMapBoundsContext.Activate(new Vector2(32f, 18f), .75f);
                activated = true;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[MORPG] Environment activation rejected before control: " + e.Message);
                Dispose();
                BattleMapBoundsContext.Clear();
                return false;
            }
        }

        internal void Tick(float delta)
        {
            if (disposed || failed || VictoryReady || !activated) return;
            if (Time.timeScale <= 0f || delta <= 0f || float.IsNaN(delta) || float.IsInfinity(delta)) return;
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
                    initialGold = currency.gold; initialXp = xp;
                    ledger = new BattleRewardLedger(new GoldAccount(currency), new XpAccount(player));
                    delivery = new MorpgRewardDeliveryQueue(TryCommitArrivedReward);
                    rewardHud = MorpgRewardHudMono.TryCreate(parent, FlushPendingRewards, currency.gold, xp);
                    delivery.Presentation = rewardHud;
                    environment.RegisterActor(player);
                    WarpTo(0);
                    if (!coordinator.TryStartActiveWave()) { Fail(coordinator.LastError); return; }
                }
                if (!delivery.Tick(delta, false)) { Fail(delivery.Error); return; }
                try { rewardHud?.TickHud(delta, currency.gold, player.GetStatValue(StatType.Experience)); }
                catch (Exception e)
                {
                    Debug.LogWarning("[MORPG] Optional HUD disabled: " + e.Message);
                    FlushPendingRewards();
                    rewardHud?.DisposeView();
                    rewardHud = null;
                    delivery.Presentation = null;
                }
                if (failed) return;
                environment.Tick(delta);
                if (player.RuntimeData.isDead)
                { Fail("party defeated", MorpgRouteStopReason.Defeat); return; }
                if (SettlementState == MorpgFinalSettlementState.FinalSettlementPending)
                {
                    TickFinalSettlement(delta);
                    return;
                }
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
                // Arrival-commit override: normal clear waits for all confirmed-death receipts.
                // This also ensures that no new zone or final handoff can outrun a pending reward.
                if (!clear.Eligible || (coordinator.ActiveZoneIndex != 2 && delivery.PendingCount != 0))
                { clearFrame = -1; return; }
                // Let the entire death callback/frame finish, including a simultaneous player death.
                if (clearFrame < 0) { clearFrame = Time.frameCount; return; }
                if (Time.frameCount <= clearFrame) return;
                if (!coordinator.TryCompleteActiveWave(clear)) return;
                if (coordinator.ActiveZoneIndex == 2)
                {
                    BeginFinalSettlement();
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
            environment.RegisterActor(character);
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
            // The reservation is the reward authority, even if presentation/pooling swaps a CharacterSO.
            int gold = rewardByReservation[reservation], quarters = gold;
            if (!delivery.TryReserve(BattleMorpgCanonicalKeys.Bundle(attempt, reservation), ++deaths,
                reservation, gold, quarters, character.transform.position.x, character.transform.position.y))
            { scope.TryFail(scope.Handle, true); Fail(delivery.Error); }
        }

        private bool TryCommitArrivedReward(MorpgPendingReward reward, out string error)
        {
            error = null;
            try
            {
                if (!ledger.TryCredit(reward.Id, reward.DeathSequence, reward.ReservationId,
                    reward.Gold, reward.XpQuarterUnits, out error))
                {
                    waves.ActiveScope?.TryFail(waves.ActiveScope.Handle, true);
                    return false;
                }
                creditedGold += reward.Gold; creditedQuarters += reward.XpQuarterUnits;
            }
            catch (Exception e)
            {
                error = "reward callback: " + e.Message;
                waves.ActiveScope?.TryFail(waves.ActiveScope.Handle, true);
                return false;
            }
            // A receipt is already committed: optional UI exceptions cannot turn it into a retry.
            try { rewardHud?.NotifyCredited(reward.Gold, reward.XpQuarterUnits, currency.gold,
                player.GetStatValue(StatType.Experience)); }
            catch (Exception e) { Debug.LogWarning("[MORPG] Optional HUD receipt: " + e.Message); }
            return true;
        }

        internal void FlushPendingRewards()
        {
            if (delivery != null && !delivery.Flush()) Fail(delivery.Error);
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
        private void BeginFinalSettlement()
        {
            if (SettlementState != MorpgFinalSettlementState.NotStarted) return;
            SettlementState = MorpgFinalSettlementState.FinalSettlementPending;
            finalSettlementElapsed = 0f;
            finalSettlementToken = new MorpgTransitionScopeToken();
            LockPlayer();
            // Clear was source-authorized and committed after the final-frame defeat barrier.
            // No spawn/hostile work may run while optional rewards finish their flight.
            if (!TryStopMorpgZoneWave(coordinator.ActiveHandle, MorpgZoneStopReason.Transition, out string error))
            { Fail(error); return; }
            TickFinalSettlement(0f);
        }

        private void TickFinalSettlement(float delta)
        {
            if (SettlementState != MorpgFinalSettlementState.FinalSettlementPending || failed) return;
            if (player == null || player.RuntimeData.isDead)
            { Fail("party defeated", MorpgRouteStopReason.Defeat); return; }
            if (delivery.Failed) { Fail(delivery.Error); return; }
            finalSettlementElapsed += delta;
            if (delivery.PendingCount > 0 && finalSettlementElapsed >= FinalSettlementTimeoutSeconds)
            {
                FinalSettlementTimedOut = true;
                if (!delivery.Flush()) { Fail(delivery.Error); return; }
            }
            // Normal waiting is not a rejected final outcome or accounting error.
            if (delivery.PendingCount > 0) return;
            if (!TryReconcileFinalRewards(out string error)) { Fail(error); return; }
            if (!coordinator.TryRequestVictory())
            { Fail(coordinator.LastError ?? "final victory authority rejected"); return; }
            SettlementState = MorpgFinalSettlementState.Completed;
            UnlockPlayer();
        }

        private bool TryReconcileFinalRewards(out string error)
        {
            error = null;
            if (delivery.PendingCount > 0) return false;
            int gold = 0, quarters = 0;
            if (deaths != rewardByReservation.Count || ledger.Bundles.Count != rewardByReservation.Count)
            { error = "final accounting mismatch: death/ledger reservation count"; return false; }
            foreach (var expected in rewardByReservation)
            {
                string id = BattleMorpgCanonicalKeys.Bundle(attempt, expected.Key);
                if (!ledger.Bundles.TryGetValue(id, out var bundle) || bundle.State != RewardBundleState.Credited ||
                    bundle.ReservationId != expected.Key || bundle.Gold != expected.Value || bundle.XpQuarterUnits != expected.Value)
                { error = "final accounting mismatch: " + expected.Key; return false; }
                gold += expected.Value; quarters += expected.Value;
            }
            if (gold != definition.expected.gold || quarters / 4m != (decimal)definition.expected.xp ||
                creditedGold != gold || creditedQuarters != quarters ||
                currency.gold != (long)initialGold + gold || player.GetStatValue(StatType.Experience) != initialXp + quarters / 4f)
            { error = "final accounting mismatch: ledger/account totals"; return false; }
            return true;
        }

        public bool TryCompleteFinalZone(BattleAttemptKey key, WaveClearKey clear, out string error)
        {
            error = "final authority mismatch";
            if (!key.Equals(attempt) || coordinator.ActiveZoneIndex != 2 ||
                !clear.Equals(coordinator.ActiveHandle.ClearKey) || player == null || player.RuntimeData.isDead) return false;
            if (SettlementState != MorpgFinalSettlementState.FinalSettlementPending || delivery.PendingCount > 0)
            { error = "final settlement pending"; return false; }
            if (!TryReconcileFinalRewards(out error)) return false;
            if (!TryStopMorpgZoneWave(coordinator.ActiveHandle, MorpgZoneStopReason.Transition, out error)) return false;
            session.BattleRuntime.rewardExperience = 0f;
            try { rewardHud?.SnapToAuthoritative(currency.gold, player.GetStatValue(StatType.Experience)); }
            catch (Exception e) { Debug.LogWarning("[MORPG] Optional final HUD: " + e.Message); }
            error = null; return true;
        }

        private void LockPlayer()
        {
            transitionPlayers.Add(player);
            player.GetComponent<MovementMono>()?.StopAllMotion();
            player.GetComponent<CharacterSkillManager>()?.CancelCasting();
            var movement = player.GetComponent<PartyMovementMono>();
            if (movement != null && !movement.TryAcquireExternalMovement(token ?? finalSettlementToken, true))
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
            if (player != null) player.GetComponent<PartyMovementMono>()?.ReleaseExternalMovement(token ?? finalSettlementToken);
            finalSettlementToken = null;
            foreach (var pair in locked) if (pair.Key != null && pair.Value) pair.Key.enabled = true;
            locked.Clear();
        }
        private void WarpTo(int ordinal)
        {
            var zone = definition.zones[ordinal];
            if (!environment.TryBeginWarp(ordinal, out string environmentError))
                throw new InvalidOperationException(environmentError);
            BattleMapBoundsContext.SetActorZone(Rect.MinMaxRect(zone.bounds[0], zone.bounds[1], zone.bounds[2], zone.bounds[3]));
            player.GetComponent<MovementMono>()?.StopAllMotion();
            Vector3 position = new Vector3(zone.entry[0], zone.entry[1], player.transform.position.z);
            player.transform.position = position;
            var body = player.GetComponent<Rigidbody2D>();
            if (body != null) { body.position = position; body.linearVelocity = Vector2.zero; body.angularVelocity = 0f; }
            environment.CompleteWarp(ordinal);
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
            SettlementState = reason == MorpgRouteStopReason.Defeat ? MorpgFinalSettlementState.Defeated : MorpgFinalSettlementState.Failed;
            // Pay confirmed kills on defeat/abort; no living or un-emitted reservation is fabricated.
            if (delivery != null && !delivery.Close()) failure = delivery.Error;
            rewardHud?.DisposeView();
            rewardHud = null;
            if (delivery != null) delivery.Presentation = null;
            coordinator.DisposeMorpgRoute(reason);
            UnlockPlayer();
            environment?.Dispose();
            Debug.LogError("[MORPG] Attempt stopped (no legacy replay): " + error);
        }
        public void Dispose()
        {
            if (disposed) return;
            // Settle before destroying either the view or actor/account owners.
            if (delivery != null && !delivery.Close()) Fail(delivery.Error);
            // Teardown settles confirmed receipts but abort wins over an uncommitted victory.
            if (SettlementState == MorpgFinalSettlementState.NotStarted ||
                SettlementState == MorpgFinalSettlementState.FinalSettlementPending)
                SettlementState = player != null && player.RuntimeData.isDead
                    ? MorpgFinalSettlementState.Defeated : MorpgFinalSettlementState.Abandoned;
            disposed = true;
            rewardHud?.DisposeView();
            rewardHud = null;
            CharacterManager.OnAnyCharacterDied -= OnDied;
            coordinator.DisposeMorpgRoute(MorpgRouteStopReason.Teardown);
            UnlockPlayer();
            environment?.Dispose();
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
            if (rewardHud != null && rewardHud.isActiveAndEnabled) return;
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
                // Confirmed kills still settle during defeat/scene teardown.
                if (expected != Revision) return false;
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
