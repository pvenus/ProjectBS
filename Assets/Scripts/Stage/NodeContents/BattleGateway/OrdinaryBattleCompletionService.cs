using System;
using System.Collections.Generic;
using System.Linq;
using Battle;
using Character;
using Session;

namespace Stage
{
    public enum OrdinaryBattleGoldClaimState
    {
        None = 0,
        Applied = 1,
        PendingRetry = 2
    }

    public sealed class OrdinaryBattleCompletionReceipt
    {
        internal OrdinaryBattleCompletionReceipt(
            OrdinaryBattleCompletionIdentity identity, int goldGranted)
        {
            Identity = identity;
            GoldGranted = goldGranted;
        }

        public OrdinaryBattleCompletionIdentity Identity { get; }
        public int GoldGranted { get; }
    }

    public sealed class OrdinaryBattleCompletionIdentity
    {
        internal OrdinaryBattleCompletionIdentity(
            string eventId, string nodeId, string sourcePopupId,
            string reservationId, string choiceId, string resultId,
            string battleId)
        {
            EventId = eventId;
            NodeId = nodeId;
            SourcePopupId = sourcePopupId;
            ReservationId = reservationId;
            ChoiceId = choiceId;
            ResultId = resultId;
            BattleId = battleId;
        }

        public string EventId { get; }
        public string NodeId { get; }
        public string SourcePopupId { get; }
        public string ReservationId { get; }
        public string ChoiceId { get; }
        public string ResultId { get; }
        public string BattleId { get; }
        public string Key => $"{EventId}|{NodeId}|{ReservationId}|{ChoiceId}|{ResultId}|{BattleId}";
    }

    [Serializable]
    public sealed class OrdinaryBattleCompletionOwnership
    {
        private readonly Dictionary<string, OrdinaryBattleCompletionReceipt> terminal =
            new(StringComparer.Ordinal);

        public OrdinaryBattleCompletionIdentity Pending { get; private set; }
        public OrdinaryBattleCompletionReceipt Finalized { get; private set; }
        public OrdinaryBattleCompletionReceipt Publication { get; private set; }
        public OrdinaryBattleGoldClaimState GoldClaimState { get; private set; }
        internal int GoldBeforeFinalize { get; private set; }
        internal int WalletRevisionBeforeFinalize { get; private set; }

        public void ResetForNewRun()
        {
            Pending = null;
            Finalized = null;
            Publication = null;
            GoldClaimState = OrdinaryBattleGoldClaimState.None;
            GoldBeforeFinalize = 0;
            WalletRevisionBeforeFinalize = 0;
            terminal.Clear();
        }

        internal bool TryPrepare(OrdinaryBattleCompletionIdentity identity)
        {
            if (identity == null) return false;
            if (terminal.ContainsKey(identity.Key)) return true;
            if (Pending == null) { Pending = identity; return true; }
            return string.Equals(Pending.Key, identity.Key, StringComparison.Ordinal);
        }

        internal bool Abort(OrdinaryBattleCompletionIdentity identity)
        {
            if (identity == null || Pending == null
                || !string.Equals(Pending.Key, identity.Key, StringComparison.Ordinal)) return false;
            Pending = null;
            return true;
        }

        internal bool TryFinalize(OrdinaryBattleCompletionReceipt receipt, int goldBefore,
            int walletRevisionBefore = 0)
        {
            if (receipt?.Identity == null || Pending == null
                || !string.Equals(Pending.Key, receipt.Identity.Key, StringComparison.Ordinal)) return false;
            Finalized = receipt;
            GoldBeforeFinalize = goldBefore;
            WalletRevisionBeforeFinalize = walletRevisionBefore;
            GoldClaimState = receipt.GoldGranted > 0
                ? OrdinaryBattleGoldClaimState.Applied
                : OrdinaryBattleGoldClaimState.None;
            return true;
        }

        internal bool RollbackFinalized()
        {
            if (Finalized == null) return false;
            Finalized = null;
            GoldClaimState = OrdinaryBattleGoldClaimState.PendingRetry;
            return true;
        }

        internal bool CommitFinalized()
        {
            if (Finalized?.Identity == null || Pending == null
                || !string.Equals(Pending.Key, Finalized.Identity.Key, StringComparison.Ordinal)) return false;
            if (!terminal.TryAdd(Finalized.Identity.Key, Finalized)) return false;
            Publication = Finalized;
            Pending = null;
            Finalized = null;
            return true;
        }

        public OrdinaryBattleCompletionReceipt ConsumePublication()
        {
            OrdinaryBattleCompletionReceipt result = Publication;
            Publication = null;
            return result;
        }
    }

    public sealed class OrdinaryBattleCompletionService
    {
        private sealed class Contract
        {
            public Contract(string eventId, string choiceId, string battleId,
                string resultId, int gold, string requiredCharacterId = "")
            {
                EventId = eventId; ChoiceId = choiceId; BattleId = battleId;
                ResultId = resultId; Gold = gold;
                RequiredCharacterId = requiredCharacterId;
            }
            public string EventId { get; }
            public string ChoiceId { get; }
            public string BattleId { get; }
            public string ResultId { get; }
            public int Gold { get; }
            public string RequiredCharacterId { get; }
        }

        private static readonly Contract[] contracts =
        {
            new("event.act1.random_event.27.paper_armor_bandits",
                "choice.act1.random_event.27.paper_armor_bandits.wait_for_paper_armor_to_soak",
                "battle.act1.event17.bandit_bowl_trap",
                "result.act1.random_event.27.paper_armor_bandits.soaked_bandits_driven_off", 0),
            new("event.act1.random_event.27.paper_armor_bandits",
                "choice.act1.random_event.27.paper_armor_bandits.attack_before_spoils_sink",
                "battle.act1.event02.expose_rain_peddler",
                "result.act1.random_event.27.paper_armor_bandits.bandits_defeated_spoils_secured", 50),
            new("event.act1.random_event.30.night_beacon_intruders",
                "choice.act1.random_event.30.night_beacon_intruders.extinguish_false_beacon",
                "battle.act1.event10.jangseung_bandit_ambush",
                "result.act1.random_event.30.night_beacon_intruders.false_beacon_extinguished", 0),
            new("event.act1.random_event.46.funeral_without_black_cloth",
                "choice.act1.random_event.46.funeral_without_black_cloth.inspect_empty_bier",
                "battle.act1.event13.rescue_child_bride",
                "result.act1.random_event.46.funeral_without_black_cloth.empty_bier_ambush_defeated", 0),
            new("event.act1.random_event.41.yujin_broken_arrow_fletching",
                "choice.act1.random_event.41.yujin_broken_arrow_fletching.follow_false_shot_trail",
                "battle.act1.event10.jangseung_bandit_ambush",
                "result.act1.random_event.41.yujin_broken_arrow_fletching.false_shot_ambush_defeated",
                0, "character.yujin")
        };

        public bool TryPrepare(BattleExecutionData data, StageSession session,
            out OrdinaryBattleCompletionIdentity identity, out string error)
        {
            identity = null;
            error = string.Empty;
            if (data == null || session?.OrdinaryBattles == null)
            {
                error = "ORDINARY_BATTLE_CONTEXT_INVALID";
                return false;
            }
            Contract contract = Find(data.eventId, data.choiceId);
            if (contract != null && !HasRequiredCharacter(contract.RequiredCharacterId))
            {
                error = "ORDINARY_BATTLE_REQUIRED_CHARACTER_MISSING";
                return false;
            }
            string battleId = data.battle?.BattleId;
            if (contract == null
                || !string.Equals(contract.BattleId, battleId, StringComparison.Ordinal)
                || !string.Equals(contract.ResultId, data.expectedVictoryResultId,
                    StringComparison.Ordinal))
            {
                error = "ORDINARY_BATTLE_CONTRACT_MISMATCH";
                return false;
            }
            identity = new OrdinaryBattleCompletionIdentity(data.eventId, data.nodeId,
                data.sourcePopupId, data.reservationId, data.choiceId,
                data.expectedVictoryResultId, battleId);
            if (!session.OrdinaryBattles.TryPrepare(identity))
            {
                error = "ORDINARY_BATTLE_PENDING_CONFLICT";
                identity = null;
                return false;
            }
            return true;
        }

        private static bool HasRequiredCharacter(string requiredId)
        {
            if (string.IsNullOrWhiteSpace(requiredId)) return true;
            IReadOnlyList<CharacterRuntimeData> members =
                GameSession.Instance?.BattleSession?.PartyRuntimeData?.Members;
            return members != null && members.Any(member =>
            {
                string id = member?.characterSO?.CharacterId;
                return string.Equals(id, requiredId, StringComparison.Ordinal)
                    || id?.StartsWith(requiredId + ".", StringComparison.Ordinal) == true;
            });
        }

        public bool TryFinalize(StageSession session, BattleSession battleSession,
            string nodeId, out string error)
        {
            error = string.Empty;
            OrdinaryBattleCompletionIdentity pending = session?.OrdinaryBattles?.Pending;
            if (pending == null) return true;
            if (!string.Equals(pending.NodeId, nodeId, StringComparison.Ordinal)
                || !string.Equals(pending.BattleId, battleSession?.BattleId,
                    StringComparison.Ordinal))
            {
                error = "ORDINARY_BATTLE_COMPLETION_IDENTITY_MISMATCH";
                return false;
            }
            Contract contract = Find(pending.EventId, pending.ChoiceId);
            if (contract == null) { error = "ORDINARY_BATTLE_CONTRACT_MISSING"; return false; }
            Currency.CurrencyRuntimeSnapshot walletSnapshot =
                session.CurrencyRuntimeData?.CaptureSnapshot() ?? default;
            if (contract.Gold > 0
                && (session.CurrencyRuntimeData == null
                    || !session.CurrencyRuntimeData.TryAddGoldExact(contract.Gold)))
            {
                error = "ORDINARY_BATTLE_GOLD_PENDING_RETRY";
                return false;
            }
            var receipt = new OrdinaryBattleCompletionReceipt(pending, contract.Gold);
            if (!session.OrdinaryBattles.TryFinalize(receipt, walletSnapshot.Gold,
                    walletSnapshot.Revision))
            {
                if (contract.Gold > 0) session.CurrencyRuntimeData?.TryRestoreSnapshot(walletSnapshot);
                error = "ORDINARY_BATTLE_RECEIPT_PREPARE_FAILED";
                return false;
            }
            return true;
        }

        public bool RollbackFinalized(StageSession session)
        {
            if (session?.OrdinaryBattles?.Finalized == null) return false;
            if (session.OrdinaryBattles.Finalized.GoldGranted > 0
                && session.CurrencyRuntimeData?.TryRestoreSnapshot(
                    new Currency.CurrencyRuntimeSnapshot(
                        session.OrdinaryBattles.GoldBeforeFinalize,
                        session.OrdinaryBattles.WalletRevisionBeforeFinalize)) != true)
                return false;
            return session.OrdinaryBattles.RollbackFinalized();
        }

        public bool CommitFinalized(StageSession session) =>
            session?.OrdinaryBattles?.CommitFinalized() == true;

        public bool Abort(StageSession session, OrdinaryBattleCompletionIdentity identity) =>
            session?.OrdinaryBattles?.Abort(identity) == true;

        private static Contract Find(string eventId, string choiceId) =>
            Array.Find(contracts, item =>
                string.Equals(item.EventId, eventId, StringComparison.Ordinal)
                && string.Equals(item.ChoiceId, choiceId, StringComparison.Ordinal));
    }
}
