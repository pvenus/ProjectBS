using System;
using System.Collections.Generic;
using System.Linq;

namespace Progression
{
    public sealed class RandomGrowthEventTransactionService
    {
        private readonly object sync = new();
        private readonly RunProgressionLedger progression;
        private readonly StageEventResultLedger results;
        private readonly IPartyVitalMutationGateway vitals;

        public RandomGrowthEventTransactionService(RunProgressionLedger progression,
            StageEventResultLedger results, IPartyVitalMutationGateway vitals)
        {
            this.progression = progression ?? throw new ArgumentNullException(nameof(progression));
            this.results = results ?? throw new ArgumentNullException(nameof(results));
            this.vitals = vitals ?? throw new ArgumentNullException(nameof(vitals));
        }

        public RandomGrowthEventTransactionReceipt Execute(RandomGrowthEventCommand command)
        {
            lock (sync)
            {
                if (command?.Cause == null || !command.Cause.IsValid)
                    return Receipt(RandomGrowthEventTransactionResult.InvalidRoster);
                RandomGrowthPayloadKind payloadKind = command.Choice == StageEventChoiceKind.Risk
                    ? RandomGrowthPayloadKind.Risk : RandomGrowthPayloadKind.Decline;
                bool knownIdentity = RandomGrowthEventIdentityCatalog.TryResolve(
                    command.Cause.EventId, command.Cause.ChoiceId, payloadKind,
                    out RandomGrowthEventIdentity identity);
                bool legacyRisk = string.Equals(command.Cause.EventId,
                    RandomGrowthEventIds.Event, StringComparison.Ordinal);
                if (!string.Equals(command.Cause.RunId, progression.RunId.Value, StringComparison.Ordinal)
                    || !knownIdentity
                    || (!legacyRisk && !string.Equals(command.Cause.ResultId,
                        identity.ResultId, StringComparison.Ordinal)))
                    return Receipt(RandomGrowthEventTransactionResult.InvalidRoster);

                string transactionId = "random-growth-" +
                    CanonicalOfferHash.ComputeHex(new[] { command.Cause.Key }).Substring(0, 32);
                StageEventResultLedgerResult reserve = results.TryReserve(command.Cause, command.Choice,
                    transactionId, out string resultReservation, out StageEventResultReceipt existing);
                if (reserve == StageEventResultLedgerResult.AlreadyResolved)
                    return new RandomGrowthEventTransactionReceipt(RandomGrowthEventTransactionResult.AlreadyResolved, existing, null);
                if (reserve == StageEventResultLedgerResult.RecoveryRequired)
                    return Receipt(RandomGrowthEventTransactionResult.RecoveryRequired);
                if (reserve != StageEventResultLedgerResult.Reserved)
                    return Receipt(RandomGrowthEventTransactionResult.ResultFaulted);

                if (command.Choice == StageEventChoiceKind.Decline)
                {
                    StageEventResultLedgerResult committed = results.TryCommit(resultReservation, string.Empty,
                        Array.Empty<PartyVitalMutation>(), out StageEventResultReceipt declineReceipt);
                    if (committed == StageEventResultLedgerResult.Committed)
                        return new RandomGrowthEventTransactionReceipt(RandomGrowthEventTransactionResult.Declined, declineReceipt, null);
                    results.TryAbortOrRollback(resultReservation);
                    return Receipt(RandomGrowthEventTransactionResult.ResultFaulted);
                }

                if (command.EarnRequest == null
                    || command.EarnRequest.SourceCategory != ProgressionSourceCategory.Random
                    || command.EarnRequest.SourceType != ProgressionSourceType.RandomEventRisk
                    || !string.Equals(command.EarnRequest.SegmentId, identity.SegmentId, StringComparison.Ordinal)
                    || !string.Equals(command.EarnRequest.SourceId, identity.SourceId, StringComparison.Ordinal)
                    || (!legacyRisk && !string.Equals(command.EarnRequest.ResultId,
                        identity.ResultId, StringComparison.Ordinal)))
                {
                    results.TryAbortOrRollback(resultReservation);
                    return Receipt(RandomGrowthEventTransactionResult.InvalidRoster);
                }

                PartyVitalCostPlan expectedPlan;
                try { expectedPlan = PartyVitalCostPolicy.Evaluate(command.ExpectedRoster); }
                catch { expectedPlan = null; }
                if (expectedPlan == null || string.Equals(expectedPlan.Reason, "InvalidRoster", StringComparison.Ordinal))
                {
                    results.TryAbortOrRollback(resultReservation);
                    return Receipt(RandomGrowthEventTransactionResult.InvalidRoster);
                }
                if (string.Equals(expectedPlan.Reason, "InvalidHpState", StringComparison.Ordinal))
                {
                    results.TryAbortOrRollback(resultReservation);
                    return Receipt(RandomGrowthEventTransactionResult.InvalidHpState);
                }
                if (!vitals.TryCapture(out IReadOnlyList<PartyVitalSnapshot> actual))
                {
                    results.TryAbortOrRollback(resultReservation);
                    return Receipt(RandomGrowthEventTransactionResult.InvalidRoster);
                }
                if (!RosterEquals(command.ExpectedRoster, actual))
                {
                    results.TryAbortOrRollback(resultReservation);
                    return Receipt(RandomGrowthEventTransactionResult.StaleRoster);
                }

                PartyVitalCostPlan plan;
                try { plan = PartyVitalCostPolicy.Evaluate(actual); }
                catch
                {
                    results.TryAbortOrRollback(resultReservation);
                    return Receipt(RandomGrowthEventTransactionResult.InvalidRoster);
                }
                if (!plan.IsEligible)
                {
                    results.TryAbortOrRollback(resultReservation);
                    return Receipt(plan.Reason == "InvalidRoster"
                        ? RandomGrowthEventTransactionResult.InvalidRoster
                        : plan.Reason == "InvalidHpState"
                            ? RandomGrowthEventTransactionResult.InvalidHpState
                            : RandomGrowthEventTransactionResult.Ineligible);
                }

                PartyVitalMutationReceipt vitalReceipt = null;
                ProgressionEarnPreparation preparation = null;
                ProgressionOpportunitySnapshot opportunity = null;
                bool vitalApplied = false, earnPrepared = false, earnCommitted = false;
                try
                {
                    if (!vitals.TryApply(transactionId, plan, out vitalReceipt))
                    {
                        if (vitalReceipt != null && vitalReceipt.Applied.Count > 0)
                        {
                            vitalApplied = true;
                            return FailWithCompensation(RandomGrowthEventTransactionResult.VitalMutationFailed,
                                resultReservation, transactionId, null, vitalReceipt, false, false);
                        }
                        results.TryAbortOrRollback(resultReservation);
                        return Receipt(RandomGrowthEventTransactionResult.VitalMutationFailed);
                    }
                    vitalApplied = true;

                    ProgressionEarnTransactionResult prepared = progression.TryPrepareEarn(
                        transactionId, command.EarnRequest, out preparation);
                    if (prepared != ProgressionEarnTransactionResult.Prepared)
                        return FailWithCompensation(prepared == ProgressionEarnTransactionResult.Rejected
                            ? RandomGrowthEventTransactionResult.CapRejected
                            : RandomGrowthEventTransactionResult.LedgerFaulted,
                            resultReservation, transactionId, null, vitalReceipt, false, false);
                    earnPrepared = true;

                    if (progression.TryCommitPreparedEarn(transactionId, out opportunity)
                        != ProgressionEarnTransactionResult.Committed)
                        return FailWithCompensation(RandomGrowthEventTransactionResult.LedgerFaulted,
                            resultReservation, transactionId, null, vitalReceipt, true, false);
                    earnPrepared = false; earnCommitted = true;

                    if (results.TryCommit(resultReservation, opportunity.OpportunityId, plan.Mutations,
                            out StageEventResultReceipt eventReceipt) != StageEventResultLedgerResult.Committed)
                        return FailWithCompensation(RandomGrowthEventTransactionResult.ResultFaulted,
                            resultReservation, transactionId, opportunity.OpportunityId, vitalReceipt, false, true);

                    return new RandomGrowthEventTransactionReceipt(RandomGrowthEventTransactionResult.Succeeded,
                        eventReceipt, opportunity);
                }
                catch
                {
                    return FailWithCompensation(RandomGrowthEventTransactionResult.LedgerFaulted,
                        resultReservation, transactionId, opportunity?.OpportunityId, vitalReceipt,
                        earnPrepared, earnCommitted || opportunity != null, vitalApplied);
                }
            }
        }

        private RandomGrowthEventTransactionReceipt FailWithCompensation(
            RandomGrowthEventTransactionResult failure, string resultReservation, string transactionId,
            string opportunityId, PartyVitalMutationReceipt vitalReceipt, bool abortPrepared,
            bool rollbackCommitted, bool vitalWasApplied = true)
        {
            bool clean = true;
            if (rollbackCommitted)
                clean &= progression.TryRollbackCommittedEarn(transactionId, opportunityId) == ProgressionEarnTransactionResult.RolledBack;
            if (abortPrepared)
                clean &= progression.TryAbortPreparedEarn(transactionId) == ProgressionEarnTransactionResult.Aborted;
            if (vitalWasApplied && vitalReceipt != null)
                clean &= vitals.TryRestore(vitalReceipt);
            if (clean)
                clean = results.TryAbortOrRollback(resultReservation) == StageEventResultLedgerResult.RolledBack;
            if (!clean)
            {
                results.MarkRecoveryRequired(resultReservation);
                return Receipt(RandomGrowthEventTransactionResult.CompensationFaulted);
            }
            return Receipt(failure);
        }

        private static bool RosterEquals(IReadOnlyList<PartyVitalSnapshot> expected,
            IReadOnlyList<PartyVitalSnapshot> actual)
        {
            if (expected == null || actual == null || expected.Count != actual.Count) return false;
            PartyVitalSnapshot[] left = expected.OrderBy(x => x.MemberId, StringComparer.Ordinal).ToArray();
            PartyVitalSnapshot[] right = actual.OrderBy(x => x.MemberId, StringComparer.Ordinal).ToArray();
            for (int i = 0; i < left.Length; i++)
                if (left[i] == null || right[i] == null
                    || !string.Equals(left[i].MemberId, right[i].MemberId, StringComparison.Ordinal)
                    || !CanonicalFloatBits.AreEqual(left[i].CurrentHp, right[i].CurrentHp)
                    || !CanonicalFloatBits.AreEqual(left[i].MaxHp, right[i].MaxHp)) return false;
            return true;
        }

        private static RandomGrowthEventTransactionReceipt Receipt(RandomGrowthEventTransactionResult result) =>
            new(result, null, null);
    }
}
