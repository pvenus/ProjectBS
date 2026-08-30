using System;
using System.Collections.Generic;
using System.Linq;
using Progression.RandomGrowth;

namespace Progression
{
    public sealed class SafeGrowthTransactionService
    {
        private readonly object sync = new();
        private readonly RunProgressionLedger progression;
        private readonly StageEventResultLedger results;
        private readonly ISafeGrowthInteractionGateway interaction;
        private readonly Dictionary<string, PreparedState> prepared = new(StringComparer.Ordinal);
        private readonly Dictionary<string, SafeGrowthTransactionReceipt> finalized = new(StringComparer.Ordinal);

        public SafeGrowthTransactionService(RunProgressionLedger progression,
            StageEventResultLedger results, ISafeGrowthInteractionGateway interaction)
        {
            this.progression = progression ?? throw new ArgumentNullException(nameof(progression));
            this.results = results ?? throw new ArgumentNullException(nameof(results));
            this.interaction = interaction ?? throw new ArgumentNullException(nameof(interaction));
        }

        public SafeGrowthTransactionReceipt Execute(SafeGrowthTransactionCommand command)
        {
            lock (sync)
            {
                SafeGrowthPrepareReceipt preparation = TryPrepareLocked(command);
                return preparation.Result switch
                {
                    SafeGrowthPrepareResult.Prepared => FinalizeLocked(preparation.Token),
                    SafeGrowthPrepareResult.AlreadyPrepared => Receipt(SafeGrowthTransactionResult.Busy),
                    SafeGrowthPrepareResult.AlreadyResolved => preparation.Completed
                        ?? Receipt(SafeGrowthTransactionResult.AlreadyResolved),
                    SafeGrowthPrepareResult.Busy => Receipt(SafeGrowthTransactionResult.Busy),
                    SafeGrowthPrepareResult.CandidateUnavailable => Receipt(SafeGrowthTransactionResult.CandidateUnavailable),
                    SafeGrowthPrepareResult.CapRejected => Receipt(SafeGrowthTransactionResult.CapRejected),
                    SafeGrowthPrepareResult.LedgerFaulted => Receipt(SafeGrowthTransactionResult.LedgerFaulted),
                    SafeGrowthPrepareResult.ResultFaulted => Receipt(SafeGrowthTransactionResult.ResultFaulted),
                    SafeGrowthPrepareResult.CompensationFaulted => Receipt(SafeGrowthTransactionResult.CompensationFaulted),
                    _ => Receipt(SafeGrowthTransactionResult.InvalidReservation)
                };
            }
        }

        public SafeGrowthPrepareReceipt TryPrepare(SafeGrowthTransactionCommand command)
        {
            lock (sync) return TryPrepareLocked(command);
        }

        public SafeGrowthTransactionReceipt Finalize(SafeGrowthPrepareToken token)
        {
            lock (sync) return FinalizeLocked(token);
        }

        public SafeGrowthTransactionResult Abort(SafeGrowthPrepareToken token)
        {
            lock (sync)
            {
                if (token == null || !prepared.TryGetValue(token.TokenId, out PreparedState state))
                    return finalized.ContainsKey(token?.TokenId ?? string.Empty)
                        ? SafeGrowthTransactionResult.AlreadyResolved
                        : SafeGrowthTransactionResult.InvalidReservation;
                bool clean = true;
                if (state.EarnPrepared)
                    clean &= progression.TryAbortPreparedEarn(token.TransactionId) == ProgressionEarnTransactionResult.Aborted;
                clean &= results.TryAbortOrRollback(token.ResultReservationId) == StageEventResultLedgerResult.RolledBack;
                clean &= RestoreInteraction(token) == SafeGrowthInteractionResult.Changed;
                if (clean) prepared.Remove(token.TokenId);
                return clean ? SafeGrowthTransactionResult.ResultFaulted : SafeGrowthTransactionResult.CompensationFaulted;
            }
        }

        private SafeGrowthPrepareReceipt TryPrepareLocked(SafeGrowthTransactionCommand command)
        {
            if (!Validate(command)) return Prepared(SafeGrowthPrepareResult.InvalidReservation);
            SafeGrowthTransactionResult? terminal = TryResolveTerminalDelivery(command);
            if (terminal == SafeGrowthTransactionResult.AlreadyResolved)
                return Prepared(SafeGrowthPrepareResult.AlreadyResolved);
            if (terminal.HasValue) return Prepared(SafeGrowthPrepareResult.InvalidReservation);
            if (prepared.Values.Any(x => string.Equals(x.Command.Token.TokenId,
                    command.Token.TokenId, StringComparison.Ordinal)))
                return Prepared(SafeGrowthPrepareResult.AlreadyPrepared,
                    prepared.Values.First(x => string.Equals(x.Command.Token.TokenId,
                        command.Token.TokenId, StringComparison.Ordinal)).Token);
            if (command.Choice == SafeGrowthTransactionChoice.Observe && command.CandidateCount <= 0)
                return Prepared(SafeGrowthPrepareResult.CandidateUnavailable);
            if (command.Choice == SafeGrowthTransactionChoice.Observe
                && progression.EvaluateEarn(command.EarnRequest) != ProgressionEarnResult.Earned)
                return Prepared(SafeGrowthPrepareResult.CapRejected);

            SafeGrowthInteractionState restoreState = interaction is SafeGrowthInteractionOwnership ownership
                ? ownership.State : SafeGrowthInteractionState.ObserveSelectedPendingRetry;
            SafeGrowthInteractionResult begin = interaction.TryBeginApply(command.Token);
            if (begin == SafeGrowthInteractionResult.Busy) return Prepared(SafeGrowthPrepareResult.Busy);
            if (begin != SafeGrowthInteractionResult.Changed) return Prepared(SafeGrowthPrepareResult.InvalidReservation);

            string transactionId = "safe-growth-" + command.Token.TokenId;
            StageEventResultLedgerResult reserved = results.TryReserve(Cause(command),
                command.Choice == SafeGrowthTransactionChoice.Observe ? StageEventChoiceKind.Risk : StageEventChoiceKind.Decline,
                transactionId, out string resultReservation, out StageEventResultReceipt existing);
            if (reserved == StageEventResultLedgerResult.AlreadyResolved)
                return Prepared(SafeGrowthPrepareResult.AlreadyResolved, completed:
                    new SafeGrowthTransactionReceipt(SafeGrowthTransactionResult.AlreadyResolved, existing, null));
            if (reserved != StageEventResultLedgerResult.Reserved)
                return PrepareFailure(command.Token, SafeGrowthPrepareResult.ResultFaulted, string.Empty, false, transactionId);

            ProgressionEarnPreparation earn = null;
            bool earnPrepared = false;
            if (command.Choice == SafeGrowthTransactionChoice.Observe)
            {
                ProgressionEarnTransactionResult result = progression.TryPrepareEarn(transactionId, command.EarnRequest, out earn);
                if (result != ProgressionEarnTransactionResult.Prepared)
                    return PrepareFailure(command.Token,
                        result == ProgressionEarnTransactionResult.Rejected || result == ProgressionEarnTransactionResult.AlreadyEarned
                            ? SafeGrowthPrepareResult.CapRejected : SafeGrowthPrepareResult.LedgerFaulted,
                        resultReservation, false, transactionId);
                earnPrepared = true;
            }

            string prepareId = "safe-prepare-" + command.Token.TokenId;
            SafeGrowthPrepareToken token = new(prepareId, transactionId, command.Token,
                command.Choice, resultReservation, earn?.OpportunityId, restoreState);
            prepared.Add(prepareId, new PreparedState(token, command, earnPrepared));
            return Prepared(SafeGrowthPrepareResult.Prepared, token);
        }

        private SafeGrowthTransactionReceipt FinalizeLocked(SafeGrowthPrepareToken token)
        {
            if (token == null) return Receipt(SafeGrowthTransactionResult.InvalidReservation);
            if (finalized.TryGetValue(token.TokenId, out SafeGrowthTransactionReceipt existing)) return existing;
            if (!prepared.TryGetValue(token.TokenId, out PreparedState state))
                return Receipt(SafeGrowthTransactionResult.InvalidReservation);

            ProgressionOpportunitySnapshot opportunity = null;
            bool earnCommitted = false, resultCommitted = false;
            try
            {
                if (state.EarnPrepared)
                {
                    if (progression.TryCommitPreparedEarn(token.TransactionId, out opportunity)
                        != ProgressionEarnTransactionResult.Committed)
                        return FinalizeFailure(state, opportunity, false, false, SafeGrowthTransactionResult.LedgerFaulted);
                    earnCommitted = true;
                }
                if (results.TryCommit(token.ResultReservationId, opportunity?.OpportunityId ?? string.Empty,
                        Array.Empty<PartyVitalMutation>(), out StageEventResultReceipt eventReceipt)
                    != StageEventResultLedgerResult.Committed)
                    return FinalizeFailure(state, opportunity, earnCommitted, false, SafeGrowthTransactionResult.ResultFaulted);
                resultCommitted = true;
                SafeGrowthInteractionState terminal = token.Choice == SafeGrowthTransactionChoice.Observe
                    ? SafeGrowthInteractionState.SafeGrowthGranted : SafeGrowthInteractionState.Declined;
                if (interaction.TryCommitTerminal(token.InteractionToken, terminal) != SafeGrowthInteractionResult.Changed)
                    return FinalizeFailure(state, opportunity, earnCommitted, resultCommitted, SafeGrowthTransactionResult.ResultFaulted);

                SafeGrowthTransactionReceipt receipt = new(
                    token.Choice == SafeGrowthTransactionChoice.Observe
                        ? SafeGrowthTransactionResult.Succeeded : SafeGrowthTransactionResult.Declined,
                    eventReceipt, opportunity);
                prepared.Remove(token.TokenId);
                finalized[token.TokenId] = receipt;
                return receipt;
            }
            catch
            {
                return FinalizeFailure(state, opportunity, earnCommitted, resultCommitted,
                    SafeGrowthTransactionResult.LedgerFaulted);
            }
        }

        private SafeGrowthPrepareReceipt PrepareFailure(SafeGrowthInteractionToken token,
            SafeGrowthPrepareResult failure, string resultReservation, bool earnPrepared, string transactionId)
        {
            bool clean = true;
            if (earnPrepared) clean &= progression.TryAbortPreparedEarn(transactionId) == ProgressionEarnTransactionResult.Aborted;
            if (!string.IsNullOrWhiteSpace(resultReservation))
                clean &= results.TryAbortOrRollback(resultReservation) == StageEventResultLedgerResult.RolledBack;
            clean &= interaction.TryMarkPendingRetry(token) == SafeGrowthInteractionResult.Changed;
            return Prepared(clean ? failure : SafeGrowthPrepareResult.CompensationFaulted);
        }

        private SafeGrowthTransactionReceipt FinalizeFailure(PreparedState state,
            ProgressionOpportunitySnapshot opportunity, bool earnCommitted, bool resultCommitted,
            SafeGrowthTransactionResult failure)
        {
            bool clean = true;
            if (resultCommitted)
                clean &= results.TryAbortOrRollback(state.Token.ResultReservationId) == StageEventResultLedgerResult.RolledBack;
            if (earnCommitted)
                clean &= progression.TryRollbackCommittedEarn(state.Token.TransactionId,
                    opportunity?.OpportunityId) == ProgressionEarnTransactionResult.RolledBack;
            else if (state.EarnPrepared)
                clean &= progression.TryAbortPreparedEarn(state.Token.TransactionId) == ProgressionEarnTransactionResult.Aborted;
            if (!resultCommitted)
                clean &= results.TryAbortOrRollback(state.Token.ResultReservationId) == StageEventResultLedgerResult.RolledBack;
            clean &= interaction.TryMarkPendingRetry(state.Token.InteractionToken) == SafeGrowthInteractionResult.Changed;
            if (clean) prepared.Remove(state.Token.TokenId);
            return Receipt(clean ? failure : SafeGrowthTransactionResult.CompensationFaulted);
        }

        private static SafeGrowthPrepareReceipt Prepared(SafeGrowthPrepareResult result,
            SafeGrowthPrepareToken token = null, SafeGrowthTransactionReceipt completed = null) =>
            new(result, token, completed);

        private SafeGrowthInteractionResult RestoreInteraction(SafeGrowthPrepareToken token) =>
            interaction is SafeGrowthInteractionOwnership ownership
                ? ownership.TryAbortApply(token.InteractionToken, token.RestoreState)
                : interaction.TryMarkPendingRetry(token.InteractionToken);

        private bool Validate(SafeGrowthTransactionCommand command)
        {
            if (command?.Token?.Key == null || !command.Token.Key.IsValid
                || !string.Equals(command.Token.Key.RunId, progression.RunId.Value, StringComparison.Ordinal)
                || !string.Equals(command.Token.Key.ReservationId, SafeGrowthTransactionIds.ReservationId, StringComparison.Ordinal))
                return false;
            string choice = command.Choice == SafeGrowthTransactionChoice.Observe
                ? SafeGrowthTransactionIds.ObserveChoiceId : SafeGrowthTransactionIds.DeclineChoiceId;
            if (!string.Equals(command.Token.ChoiceId, choice, StringComparison.Ordinal)) return false;
            if (command.Choice == SafeGrowthTransactionChoice.Decline) return command.EarnRequest == null;
            ProgressionEarnRequest earn = command.EarnRequest;
            return earn != null
                && earn.SourceCategory == ProgressionSourceCategory.Random
                && earn.SourceType == ProgressionSourceType.RandomEventSafe
                && string.Equals(earn.SegmentId, ProgressionSourceRegistry.OptionalRandomGrowthSegment, StringComparison.Ordinal)
                && string.Equals(earn.SourceId, ProgressionSourceRegistry.RandomGrowthSafeSource, StringComparison.Ordinal)
                && string.Equals(earn.ResultId, SafeGrowthTransactionIds.GrantedResultId, StringComparison.Ordinal);
        }

        private SafeGrowthTransactionResult? TryResolveTerminalDelivery(
            SafeGrowthTransactionCommand command)
        {
            SafeGrowthInteractionState expected = command.Choice == SafeGrowthTransactionChoice.Observe
                ? SafeGrowthInteractionState.SafeGrowthGranted
                : SafeGrowthInteractionState.Declined;

            if (interaction is SafeGrowthInteractionOwnership ownership)
            {
                if (ownership.Token == null
                    || !string.Equals(ownership.Token.TokenId, command.Token.TokenId, StringComparison.Ordinal))
                    return SafeGrowthTransactionResult.InvalidReservation;
                if (ownership.State == expected) return SafeGrowthTransactionResult.AlreadyResolved;
                if (ownership.State == SafeGrowthInteractionState.SafeGrowthGranted
                    || ownership.State == SafeGrowthInteractionState.Declined)
                    return SafeGrowthTransactionResult.InvalidReservation;
                return null;
            }

            SafeGrowthInteractionResult probe = interaction.TryCommitTerminal(command.Token, expected);
            if (probe == SafeGrowthInteractionResult.Existing)
                return SafeGrowthTransactionResult.AlreadyResolved;
            if (probe == SafeGrowthInteractionResult.AlreadyTerminal)
                return SafeGrowthTransactionResult.InvalidReservation;
            return null;
        }

        private static StageEventCause Cause(SafeGrowthTransactionCommand command) => new(
            command.Token.Key.RunId, command.Token.Key.StageGenerationId,
            command.Token.Key.EncounteredNodeInstanceId, SafeGrowthTransactionIds.EventId,
            command.Token.ChoiceId, command.Choice == SafeGrowthTransactionChoice.Observe
                ? SafeGrowthTransactionIds.GrantedResultId : SafeGrowthTransactionIds.DeclinedResultId);
        private static SafeGrowthTransactionReceipt Receipt(SafeGrowthTransactionResult result) => new(result, null, null);

        private sealed class PreparedState
        {
            public PreparedState(SafeGrowthPrepareToken token,
                SafeGrowthTransactionCommand command, bool earnPrepared)
            { Token = token; Command = command; EarnPrepared = earnPrepared; }
            public SafeGrowthPrepareToken Token { get; }
            public SafeGrowthTransactionCommand Command { get; }
            public bool EarnPrepared { get; }
        }
    }
}
