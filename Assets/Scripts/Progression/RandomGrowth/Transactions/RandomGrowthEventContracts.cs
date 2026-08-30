using System;
using System.Collections.Generic;
using System.Linq;

namespace Progression
{
    public static class CanonicalFloatBits
    {
        public static int GetBits(float value) => BitConverter.SingleToInt32Bits(value);
        public static bool AreEqual(float left, float right) => GetBits(left) == GetBits(right);
        public static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        public static bool IsNegativeZero(float value) => GetBits(value) == unchecked((int)0x80000000);
        public static bool IsPositiveSubnormal(float value)
        {
            int bits = GetBits(value);
            return bits > 0 && (bits & 0x7f800000) == 0;
        }
    }

    public static class RandomGrowthEventIds
    {
        public const string Event = "event.act1.random_growth.01.crying_bell_smithy_trial";
        public const string RiskChoice = "choice.act1.random_growth.01.crying_bell_smithy_trial.take_heated_talisman";
        public const string DeclineChoice = "choice.act1.random_growth.01.crying_bell_smithy_trial.leave_forge";
    }

    public enum RandomGrowthEventTransactionResult
    {
        Succeeded, Declined, AlreadyResolved, Ineligible, InvalidRoster, InvalidHpState,
        StaleRoster, CapRejected, VitalMutationFailed, LedgerFaulted,
        ResultFaulted, CompensationFaulted, RecoveryRequired
    }

    public enum StageEventChoiceKind { Risk, Decline, TechnicalFailure }

    public static class SafeGrowthNodeOnlyFailureIds
    {
        public const string ResultKind = "SafePresentationContentUnavailableAfterDisclosure";
        public const string ReceiptId = "failure.act1.random_growth.02.windworn_sword_marks.presentation_content_unavailable_after_disclosure";
        public const string SemanticClass = "NodeOnlyTechnicalFailure";
        public const string CauseDomain = "chapter1.safe-growth.node-only-failure-receipt.v1";
        public const string GlobalCopyKey = "system.stage.content_unavailable.continue";
    }

    public sealed class SafeGrowthNodeOnlyFailureCause
    {
        public SafeGrowthNodeOnlyFailureCause(string runId, string stageGenerationId,
            string eventId, string nodeId, string reservationId,
            string encounteredNodeInstanceId, string failureReceiptId)
        { RunId=runId; StageGenerationId=stageGenerationId; EventId=eventId; NodeId=nodeId;
          ReservationId=reservationId; EncounteredNodeInstanceId=encounteredNodeInstanceId;
          FailureReceiptId=failureReceiptId; }
        public string RunId { get; } public string StageGenerationId { get; }
        public string EventId { get; } public string NodeId { get; }
        public string ReservationId { get; } public string EncounteredNodeInstanceId { get; }
        public string FailureReceiptId { get; }
        public bool IsValid => new[]{RunId,StageGenerationId,EventId,NodeId,ReservationId,
            EncounteredNodeInstanceId,FailureReceiptId}.All(x=>!string.IsNullOrWhiteSpace(x));
        public string StableKey => string.Join("\n", RunId,StageGenerationId,EventId,NodeId,
            ReservationId,EncounteredNodeInstanceId,FailureReceiptId);
    }

    public sealed class StageEventCause
    {
        public StageEventCause(string runId, string stageGenerationId, string slotId,
            string eventId, string choiceId, string resultId)
        {
            RunId = runId; StageGenerationId = stageGenerationId; SlotId = slotId;
            EventId = eventId; ChoiceId = choiceId; ResultId = resultId;
        }

        public string RunId { get; }
        public string StageGenerationId { get; }
        public string SlotId { get; }
        public string EventId { get; }
        public string ChoiceId { get; }
        public string ResultId { get; }
        public bool IsValid => new[] { RunId, StageGenerationId, SlotId, EventId, ChoiceId, ResultId }
            .All(value => !string.IsNullOrWhiteSpace(value));
        public string Key => string.Join("\n", RunId, StageGenerationId, SlotId, EventId, ChoiceId, ResultId);
        internal string EventKey => string.Join("\n", RunId, StageGenerationId, SlotId, EventId);
    }

    public sealed class PartyVitalSnapshot
    {
        public PartyVitalSnapshot(string memberId, float currentHp, float maxHp)
        { MemberId = memberId; CurrentHp = currentHp; MaxHp = maxHp; }
        public string MemberId { get; }
        public float CurrentHp { get; }
        public float MaxHp { get; }
    }

    public sealed class PartyVitalMutation
    {
        public PartyVitalMutation(string memberId, float before, float after, int cost, float expectedMaxHp)
        { MemberId = memberId; Before = before; After = after; Cost = cost; ExpectedMaxHp = expectedMaxHp; }
        public string MemberId { get; }
        public float Before { get; }
        public float After { get; }
        public int Cost { get; }
        public float ExpectedMaxHp { get; }
    }

    public sealed class PartyVitalCostPlan
    {
        internal PartyVitalCostPlan(bool isEligible, string reason, IEnumerable<PartyVitalMutation> mutations)
        { IsEligible = isEligible; Reason = reason; Mutations = Array.AsReadOnly(mutations.ToArray()); }
        public bool IsEligible { get; }
        public string Reason { get; }
        public IReadOnlyList<PartyVitalMutation> Mutations { get; }
    }

    public sealed class PartyVitalMutationReceipt
    {
        public PartyVitalMutationReceipt(string transactionId, IEnumerable<PartyVitalMutation> applied)
        { TransactionId = transactionId; Applied = Array.AsReadOnly(applied.ToArray()); }
        public string TransactionId { get; }
        public IReadOnlyList<PartyVitalMutation> Applied { get; }
    }

    public interface IPartyVitalMutationGateway
    {
        bool TryCapture(out IReadOnlyList<PartyVitalSnapshot> roster);
        bool TryApply(string transactionId, PartyVitalCostPlan plan, out PartyVitalMutationReceipt receipt);
        bool TryRestore(PartyVitalMutationReceipt receipt);
    }

    public sealed class RandomGrowthEventCommand
    {
        public RandomGrowthEventCommand(StageEventCause cause, StageEventChoiceKind choice,
            IEnumerable<PartyVitalSnapshot> expectedRoster, ProgressionEarnRequest earnRequest = null)
        {
            Cause = cause; Choice = choice;
            ExpectedRoster = Array.AsReadOnly((expectedRoster ?? Array.Empty<PartyVitalSnapshot>()).ToArray());
            EarnRequest = earnRequest;
        }
        public StageEventCause Cause { get; }
        public StageEventChoiceKind Choice { get; }
        public IReadOnlyList<PartyVitalSnapshot> ExpectedRoster { get; }
        public ProgressionEarnRequest EarnRequest { get; }
    }

    public sealed class StageEventResultReceipt
    {
        internal StageEventResultReceipt(string transactionId, StageEventCause cause,
            StageEventChoiceKind choice, string opportunityId, IEnumerable<PartyVitalMutation> costs)
        {
            TransactionId = transactionId; Cause = cause; Choice = choice;
            OpportunityId = opportunityId ?? string.Empty; Costs = Array.AsReadOnly(costs.ToArray());
        }
        public string TransactionId { get; }
        public StageEventCause Cause { get; }
        public StageEventChoiceKind Choice { get; }
        public string OpportunityId { get; }
        public IReadOnlyList<PartyVitalMutation> Costs { get; }
    }

    public sealed class RandomGrowthEventTransactionReceipt
    {
        internal RandomGrowthEventTransactionReceipt(RandomGrowthEventTransactionResult result,
            StageEventResultReceipt eventReceipt, ProgressionOpportunitySnapshot opportunity)
        { Result = result; EventReceipt = eventReceipt; Opportunity = opportunity; }
        public RandomGrowthEventTransactionResult Result { get; }
        public StageEventResultReceipt EventReceipt { get; }
        public ProgressionOpportunitySnapshot Opportunity { get; }
    }
}
