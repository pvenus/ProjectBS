using Progression.RandomGrowth;

namespace Progression
{
    public static class SafeGrowthTransactionIds
    {
        public const string EventId = "event.act1.random_growth.02.windworn_sword_marks";
        public const string ReservationId = "reservation.act1.chapter01.random_growth.after_episode02";
        public const string ObserveChoiceId = "choice.act1.random_growth.02.windworn_sword_marks.observe_sword_path";
        public const string DeclineChoiceId = "choice.act1.random_growth.02.windworn_sword_marks.leave_training_ground";
        public const string GrantedResultId = "result.act1.random_growth.02.windworn_sword_marks.safe_growth_granted";
        public const string DeclinedResultId = "result.act1.random_growth.02.windworn_sword_marks.declined";
    }

    public enum SafeGrowthTransactionChoice { Observe = 0, Decline = 10 }
    public enum SafeGrowthTransactionResult
    {
        Succeeded = 0, Declined = 10, AlreadyResolved = 20, Busy = 30,
        InvalidReservation = 40, CandidateUnavailable = 50, CapRejected = 60,
        LedgerFaulted = 70, ResultFaulted = 80, CompensationFaulted = 90
    }

    public sealed class SafeGrowthTransactionCommand
    {
        public SafeGrowthTransactionCommand(SafeGrowthInteractionToken token,
            SafeGrowthTransactionChoice choice, ProgressionEarnRequest earnRequest,
            int candidateCount, RandomGrowthEventIdentity identity = null)
        { Token = token; Choice = choice; EarnRequest = earnRequest; CandidateCount = candidateCount; Identity = identity; }
        public SafeGrowthInteractionToken Token { get; }
        public SafeGrowthTransactionChoice Choice { get; }
        public ProgressionEarnRequest EarnRequest { get; }
        public int CandidateCount { get; }
        public RandomGrowthEventIdentity Identity { get; }
    }

    public sealed class SafeGrowthTransactionReceipt
    {
        internal SafeGrowthTransactionReceipt(SafeGrowthTransactionResult result,
            StageEventResultReceipt eventReceipt, ProgressionOpportunitySnapshot opportunity)
        { Result = result; EventReceipt = eventReceipt; Opportunity = opportunity; }
        public SafeGrowthTransactionResult Result { get; }
        public StageEventResultReceipt EventReceipt { get; }
        public ProgressionOpportunitySnapshot Opportunity { get; }
    }

    public enum SafeGrowthPrepareResult
    {
        Prepared = 0, AlreadyPrepared = 10, AlreadyResolved = 20, Busy = 30,
        InvalidReservation = 40, CandidateUnavailable = 50, CapRejected = 60,
        ResultFaulted = 70, LedgerFaulted = 80, CompensationFaulted = 90
    }

    public sealed class SafeGrowthPrepareToken
    {
        internal SafeGrowthPrepareToken(string tokenId, string transactionId,
            SafeGrowthInteractionToken interactionToken, SafeGrowthTransactionChoice choice,
            string resultReservationId, string preparedOpportunityId,
            SafeGrowthInteractionState restoreState)
        {
            TokenId = tokenId;
            TransactionId = transactionId;
            InteractionToken = interactionToken;
            Choice = choice;
            ResultReservationId = resultReservationId;
            PreparedOpportunityId = preparedOpportunityId ?? string.Empty;
            RestoreState = restoreState;
        }
        public string TokenId { get; }
        public string TransactionId { get; }
        public SafeGrowthInteractionToken InteractionToken { get; }
        public SafeGrowthTransactionChoice Choice { get; }
        public string ResultReservationId { get; }
        public string PreparedOpportunityId { get; }
        public SafeGrowthInteractionState RestoreState { get; }
    }

    public sealed class SafeGrowthPrepareReceipt
    {
        internal SafeGrowthPrepareReceipt(SafeGrowthPrepareResult result,
            SafeGrowthPrepareToken token, SafeGrowthTransactionReceipt completed)
        { Result = result; Token = token; Completed = completed; }
        public SafeGrowthPrepareResult Result { get; }
        public SafeGrowthPrepareToken Token { get; }
        public SafeGrowthTransactionReceipt Completed { get; }
    }
}
