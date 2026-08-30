using System;
using System.Collections.Generic;

namespace Progression
{
    public enum ProgressionOpportunityState
    {
        Pending = 0,
        PendingBlocked = 10,
        Consuming = 20,
        Applied = 30
    }

    public enum ProgressionSourceCategory
    {
        Fixed = 0,
        Random = 10
    }

    public enum ProgressionPoolMode
    {
        PartyWide = 0
    }

    public enum ProgressionSourceType
    {
        BattleVictory = 0,
        MajorStoryResolution = 10,
        RandomEventRisk = 20,
        RandomEventSafe = 30
    }

    public enum ProgressionEarnResult
    {
        Earned = 0,
        AlreadyEarned = 10,
        RejectedInvalidRequest = 20,
        RejectedSourceNotAllowed = 30,
        RejectedCap = 40,
        Faulted = 50
    }

    internal enum ProgressionEarnTransactionResult
    {
        Prepared = 0,
        Committed = 10,
        Aborted = 20,
        RolledBack = 30,
        AlreadyEarned = 40,
        Rejected = 50,
        Faulted = 60
    }

    internal sealed class ProgressionEarnPreparation
    {
        public ProgressionEarnPreparation(string transactionId, string opportunityId, ProgressionEarnRequest request)
        {
            TransactionId = transactionId;
            OpportunityId = opportunityId;
            Request = request;
        }

        public string TransactionId { get; }
        public string OpportunityId { get; }
        public ProgressionEarnRequest Request { get; }
    }

    public enum ProgressionConsumeResult
    {
        Reserved = 0,
        Applied = 10,
        RolledBack = 20,
        RejectedNotFound = 30,
        RejectedState = 40,
        RejectedRevision = 50,
        RejectedReservation = 60,
        RejectedCap = 70,
        Faulted = 80
    }

    public enum ProgressionStateResult
    {
        Changed = 0,
        Unchanged = 10,
        RejectedNotFound = 20,
        RejectedState = 30,
        RejectedRevision = 40,
        Faulted = 50
    }

    public enum ProgressionOfferAttachResult
    {
        Attached = 0,
        AlreadyAttached = 10,
        RejectedNotFound = 20,
        RejectedState = 30,
        RejectedRevision = 40,
        RejectedInvalidOffer = 50,
        Faulted = 60
    }

    public readonly struct ProgressionRunId : IEquatable<ProgressionRunId>
    {
        public ProgressionRunId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Run ID is required.", nameof(value));
            }

            Value = value;
        }

        public string Value { get; }
        public bool IsValid => !string.IsNullOrWhiteSpace(Value);

        public static ProgressionRunId NewId()
        {
            return new ProgressionRunId(Guid.NewGuid().ToString("N"));
        }

        public bool Equals(ProgressionRunId other) =>
            string.Equals(Value, other.Value, StringComparison.Ordinal);

        public override bool Equals(object obj) =>
            obj is ProgressionRunId other && Equals(other);

        public override int GetHashCode() =>
            Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);

        public override string ToString() => Value ?? string.Empty;
    }

    public sealed class ProgressionEarnRequest
    {
        public ProgressionEarnRequest(
            string segmentId,
            ProgressionSourceCategory sourceCategory,
            ProgressionSourceType sourceType,
            string sourceId,
            string resultId,
            ProgressionPoolMode poolMode = ProgressionPoolMode.PartyWide)
        {
            SegmentId = segmentId;
            SourceCategory = sourceCategory;
            SourceType = sourceType;
            SourceId = sourceId;
            ResultId = resultId;
            PoolMode = poolMode;
        }

        public string SegmentId { get; }
        public ProgressionSourceCategory SourceCategory { get; }
        public ProgressionSourceType SourceType { get; }
        public string SourceId { get; }
        public string ResultId { get; }
        public ProgressionPoolMode PoolMode { get; }

        internal bool IsStructurallyValid =>
            !string.IsNullOrWhiteSpace(SegmentId)
            && !string.IsNullOrWhiteSpace(SourceId)
            && !string.IsNullOrWhiteSpace(ResultId)
            && PoolMode == ProgressionPoolMode.PartyWide;

        internal string CauseKey =>
            $"{(int)SourceType}:{SourceId}:{ResultId}";
    }

    public sealed class ProgressionOpportunitySnapshot
    {
        internal ProgressionOpportunitySnapshot(ProgressionOpportunityRecord record)
        {
            OpportunityId = record.OpportunityId;
            RunId = record.RunId;
            SegmentId = record.SegmentId;
            SourceCategory = record.SourceCategory;
            SourceType = record.SourceType;
            SourceId = record.SourceId;
            ResultId = record.ResultId;
            PoolMode = record.PoolMode;
            State = record.State;
            Revision = record.Revision;
            BlockedReason = record.BlockedReason;
            ConsumeReservationId = record.ConsumeReservationId;
            Offer = record.Offer;
            AppliedReceipt = record.AppliedReceipt;
        }

        public string OpportunityId { get; }
        public ProgressionRunId RunId { get; }
        public string SegmentId { get; }
        public ProgressionSourceCategory SourceCategory { get; }
        public ProgressionSourceType SourceType { get; }
        public string SourceId { get; }
        public string ResultId { get; }
        public ProgressionPoolMode PoolMode { get; }
        public ProgressionOpportunityState State { get; }
        public int Revision { get; }
        public string BlockedReason { get; }
        public string ConsumeReservationId { get; }
        public ProgressionOfferSnapshot Offer { get; }
        public ProgressionApplyReceipt AppliedReceipt { get; }
    }

    public sealed class ProgressionChapterSummary
    {
        internal ProgressionChapterSummary(
            int fixedEarned,
            int randomEarned,
            int fixedApplied,
            int randomApplied)
        {
            FixedEarned = fixedEarned;
            RandomEarned = randomEarned;
            FixedApplied = fixedApplied;
            RandomApplied = randomApplied;
        }

        public int FixedEarned { get; }
        public int RandomEarned { get; }
        public int TotalEarned => FixedEarned + RandomEarned;
        public int FixedApplied { get; }
        public int RandomApplied { get; }
        public int TotalApplied => FixedApplied + RandomApplied;
    }

    public sealed class ProgressionCapPolicy
    {
        public ProgressionCapPolicy(
            int fixedCap,
            int randomCap,
            int totalCap)
        {
            if (fixedCap < 0 || randomCap < 0 || totalCap < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(totalCap));
            }

            FixedCap = fixedCap;
            RandomCap = randomCap;
            TotalCap = totalCap;
        }

        public int FixedCap { get; }
        public int RandomCap { get; }
        public int TotalCap { get; }

        public static ProgressionCapPolicy Chapter1P0 { get; } =
            new ProgressionCapPolicy(2, 1, 3);

        internal bool AllowsEarn(
            ProgressionSourceCategory category,
            ProgressionChapterSummary summary)
        {
            if (summary.TotalEarned >= TotalCap)
            {
                return false;
            }

            return category == ProgressionSourceCategory.Fixed
                ? summary.FixedEarned < FixedCap
                : summary.RandomEarned < RandomCap;
        }

        internal bool AllowsConsume(
            ProgressionSourceCategory category,
            ProgressionChapterSummary summary)
        {
            if (summary.TotalApplied >= TotalCap)
            {
                return false;
            }

            return category == ProgressionSourceCategory.Fixed
                ? summary.FixedApplied < FixedCap
                : summary.RandomApplied < RandomCap;
        }
    }

    internal sealed class ProgressionOpportunityRecord
    {
        public string OpportunityId;
        public ProgressionRunId RunId;
        public string SegmentId;
        public ProgressionSourceCategory SourceCategory;
        public ProgressionSourceType SourceType;
        public string SourceId;
        public string ResultId;
        public ProgressionPoolMode PoolMode;
        public ProgressionOpportunityState State;
        public int Revision;
        public string BlockedReason;
        public string ConsumeReservationId;
        public ProgressionOfferSnapshot Offer;
        public ProgressionApplyReceipt AppliedReceipt;

        public ProgressionOpportunityRecord Clone()
        {
            return (ProgressionOpportunityRecord)MemberwiseClone();
        }
    }

    internal enum ProgressionLedgerMutationPoint
    {
        EarnRecordAdded,
        EarnPrepared,
        EarnCommitted,
        EarnAborted,
        EarnCommitRolledBack,
        ConsumeReserved,
        ConsumeApplied,
        ConsumeRolledBack,
        Blocked,
        Unblocked,
        OfferAttached
    }
}
