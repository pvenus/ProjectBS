using System;
using System.Collections.Generic;
using System.Linq;

namespace Progression
{
    public sealed class RunProgressionLedger
    {
        private readonly ProgressionRunId runId;
        private readonly ProgressionCapPolicy capPolicy;
        private readonly ProgressionSourceRegistry sourceRegistry;
        private readonly Dictionary<string, ProgressionOpportunityRecord> recordsById =
            new(StringComparer.Ordinal);
        private readonly Dictionary<string, string> opportunityIdBySegment =
            new(StringComparer.Ordinal);
        private readonly Dictionary<string, string> opportunityIdByCause =
            new(StringComparer.Ordinal);
        private readonly Dictionary<string, ProgressionEarnPreparation> preparedEarns =
            new(StringComparer.Ordinal);
        private readonly Dictionary<string, string> earnTransactionByOpportunity =
            new(StringComparer.Ordinal);
        private readonly Action<ProgressionLedgerMutationPoint> mutationObserver;
        private int opportunitySequence;

        public RunProgressionLedger(
            ProgressionRunId runId,
            ProgressionCapPolicy capPolicy,
            ProgressionSourceRegistry sourceRegistry)
            : this(runId, capPolicy, sourceRegistry, null)
        {
        }

        internal RunProgressionLedger(
            ProgressionRunId runId,
            ProgressionCapPolicy capPolicy,
            ProgressionSourceRegistry sourceRegistry,
            Action<ProgressionLedgerMutationPoint> mutationObserver)
        {
            if (!runId.IsValid)
            {
                throw new ArgumentException("A valid run ID is required.", nameof(runId));
            }

            this.runId = runId;
            this.capPolicy = capPolicy ?? throw new ArgumentNullException(nameof(capPolicy));
            this.sourceRegistry = sourceRegistry ?? throw new ArgumentNullException(nameof(sourceRegistry));
            this.mutationObserver = mutationObserver;
        }

        public ProgressionRunId RunId => runId;
        public int Count => recordsById.Count;

        public ProgressionEarnResult TryEarn(
            ProgressionEarnRequest request,
            out ProgressionOpportunitySnapshot opportunity)
        {
            opportunity = null;

            if (request == null || !request.IsStructurallyValid)
            {
                return ProgressionEarnResult.RejectedInvalidRequest;
            }

            if (!sourceRegistry.IsAllowed(request))
            {
                return ProgressionEarnResult.RejectedSourceNotAllowed;
            }

            if (TryGetExisting(request, out ProgressionOpportunityRecord existing))
            {
                opportunity = new ProgressionOpportunitySnapshot(existing);
                return ProgressionEarnResult.AlreadyEarned;
            }

            ProgressionChapterSummary summary = GetChapterSummary();
            if (!capPolicy.AllowsEarn(request.SourceCategory, summary))
            {
                return ProgressionEarnResult.RejectedCap;
            }

            LedgerStateBackup backup = CaptureState();
            try
            {
                opportunitySequence++;
                string opportunityId =
                    $"progress.opportunity.{runId.Value}.{opportunitySequence:D3}";
                ProgressionOpportunityRecord record = new()
                {
                    OpportunityId = opportunityId,
                    RunId = runId,
                    SegmentId = request.SegmentId,
                    SourceCategory = request.SourceCategory,
                    SourceType = request.SourceType,
                    SourceId = request.SourceId,
                    ResultId = request.ResultId,
                    PoolMode = request.PoolMode,
                    State = ProgressionOpportunityState.Pending,
                    Revision = 1,
                    BlockedReason = string.Empty,
                    ConsumeReservationId = string.Empty,
                    Offer = null,
                    AppliedReceipt = null
                };

                recordsById.Add(opportunityId, record);
                opportunityIdBySegment.Add(request.SegmentId, opportunityId);
                opportunityIdByCause.Add(request.CauseKey, opportunityId);
                mutationObserver?.Invoke(ProgressionLedgerMutationPoint.EarnRecordAdded);

                opportunity = new ProgressionOpportunitySnapshot(record);
                return ProgressionEarnResult.Earned;
            }
            catch
            {
                RestoreState(backup);
                opportunity = null;
                return ProgressionEarnResult.Faulted;
            }
        }

        internal ProgressionEarnResult EvaluateEarn(ProgressionEarnRequest request)
        {
            if (request == null || !request.IsStructurallyValid)
                return ProgressionEarnResult.RejectedInvalidRequest;
            if (!sourceRegistry.IsAllowed(request))
                return ProgressionEarnResult.RejectedSourceNotAllowed;
            if (TryGetExisting(request, out _))
                return ProgressionEarnResult.AlreadyEarned;
            return capPolicy.AllowsEarn(request.SourceCategory, GetChapterSummary())
                ? ProgressionEarnResult.Earned
                : ProgressionEarnResult.RejectedCap;
        }

        internal ProgressionEarnTransactionResult TryPrepareEarn(
            string transactionId,
            ProgressionEarnRequest request,
            out ProgressionEarnPreparation preparation)
        {
            preparation = null;
            if (string.IsNullOrWhiteSpace(transactionId)
                || request == null
                || !request.IsStructurallyValid
                || !sourceRegistry.IsAllowed(request))
            {
                return ProgressionEarnTransactionResult.Rejected;
            }

            if (TryGetExisting(request, out _)
                || preparedEarns.Values.Any(item =>
                    string.Equals(item.Request.SegmentId, request.SegmentId, StringComparison.Ordinal)
                    || string.Equals(item.Request.CauseKey, request.CauseKey, StringComparison.Ordinal)))
            {
                return ProgressionEarnTransactionResult.AlreadyEarned;
            }

            ProgressionChapterSummary summary = GetChapterSummary(includeConsuming: false, includePrepared: true);
            if (!capPolicy.AllowsEarn(request.SourceCategory, summary))
            {
                return ProgressionEarnTransactionResult.Rejected;
            }

            LedgerStateBackup backup = CaptureState();
            try
            {
                opportunitySequence++;
                preparation = new ProgressionEarnPreparation(
                    transactionId,
                    $"progress.opportunity.{runId.Value}.{opportunitySequence:D3}",
                    request);
                preparedEarns.Add(transactionId, preparation);
                mutationObserver?.Invoke(ProgressionLedgerMutationPoint.EarnPrepared);
                return ProgressionEarnTransactionResult.Prepared;
            }
            catch
            {
                RestoreState(backup);
                preparation = null;
                return ProgressionEarnTransactionResult.Faulted;
            }
        }

        internal ProgressionEarnTransactionResult TryCommitPreparedEarn(
            string transactionId,
            out ProgressionOpportunitySnapshot opportunity)
        {
            opportunity = null;
            if (string.IsNullOrWhiteSpace(transactionId)
                || !preparedEarns.TryGetValue(transactionId, out ProgressionEarnPreparation prepared))
            {
                return ProgressionEarnTransactionResult.Rejected;
            }

            LedgerStateBackup backup = CaptureState();
            try
            {
                ProgressionEarnRequest request = prepared.Request;
                ProgressionOpportunityRecord record = new()
                {
                    OpportunityId = prepared.OpportunityId,
                    RunId = runId,
                    SegmentId = request.SegmentId,
                    SourceCategory = request.SourceCategory,
                    SourceType = request.SourceType,
                    SourceId = request.SourceId,
                    ResultId = request.ResultId,
                    PoolMode = request.PoolMode,
                    State = ProgressionOpportunityState.Pending,
                    Revision = 1,
                    BlockedReason = string.Empty,
                    ConsumeReservationId = string.Empty
                };
                recordsById.Add(record.OpportunityId, record);
                opportunityIdBySegment.Add(request.SegmentId, record.OpportunityId);
                opportunityIdByCause.Add(request.CauseKey, record.OpportunityId);
                earnTransactionByOpportunity.Add(record.OpportunityId, transactionId);
                preparedEarns.Remove(transactionId);
                mutationObserver?.Invoke(ProgressionLedgerMutationPoint.EarnCommitted);
                opportunity = new ProgressionOpportunitySnapshot(record);
                return ProgressionEarnTransactionResult.Committed;
            }
            catch
            {
                RestoreState(backup);
                return ProgressionEarnTransactionResult.Faulted;
            }
        }

        internal ProgressionEarnTransactionResult TryAbortPreparedEarn(string transactionId)
        {
            if (!preparedEarns.ContainsKey(transactionId ?? string.Empty))
            {
                return ProgressionEarnTransactionResult.Rejected;
            }

            LedgerStateBackup backup = CaptureState();
            try
            {
                preparedEarns.Remove(transactionId);
                mutationObserver?.Invoke(ProgressionLedgerMutationPoint.EarnAborted);
                return ProgressionEarnTransactionResult.Aborted;
            }
            catch
            {
                RestoreState(backup);
                return ProgressionEarnTransactionResult.Faulted;
            }
        }

        internal ProgressionEarnTransactionResult TryRollbackCommittedEarn(
            string transactionId,
            string opportunityId)
        {
            if (!recordsById.TryGetValue(opportunityId ?? string.Empty, out ProgressionOpportunityRecord record)
                || !earnTransactionByOpportunity.TryGetValue(record.OpportunityId, out string ownerTransaction)
                || !string.Equals(ownerTransaction, transactionId, StringComparison.Ordinal)
                || record.State != ProgressionOpportunityState.Pending
                || record.Offer != null)
            {
                return ProgressionEarnTransactionResult.Rejected;
            }

            LedgerStateBackup backup = CaptureState();
            try
            {
                recordsById.Remove(record.OpportunityId);
                opportunityIdBySegment.Remove(record.SegmentId);
                opportunityIdByCause.Remove(new ProgressionEarnRequest(
                    record.SegmentId, record.SourceCategory, record.SourceType,
                    record.SourceId, record.ResultId, record.PoolMode).CauseKey);
                earnTransactionByOpportunity.Remove(record.OpportunityId);
                mutationObserver?.Invoke(ProgressionLedgerMutationPoint.EarnCommitRolledBack);
                return ProgressionEarnTransactionResult.RolledBack;
            }
            catch
            {
                RestoreState(backup);
                return ProgressionEarnTransactionResult.Faulted;
            }
        }

        public bool TryGetOpportunity(
            string opportunityId,
            out ProgressionOpportunitySnapshot opportunity)
        {
            opportunity = null;
            if (string.IsNullOrWhiteSpace(opportunityId)
                || !recordsById.TryGetValue(opportunityId, out ProgressionOpportunityRecord record))
            {
                return false;
            }

            opportunity = new ProgressionOpportunitySnapshot(record);
            return true;
        }

        public ProgressionOfferAttachResult TryAttachFixedOffer(
            string opportunityId,
            int expectedRevision,
            ProgressionOfferSnapshot offer,
            out ProgressionOpportunitySnapshot opportunity)
        {
            opportunity = null;
            if (!TryGetRecord(opportunityId, out ProgressionOpportunityRecord record))
            {
                return ProgressionOfferAttachResult.RejectedNotFound;
            }

            if (record.Offer != null)
            {
                opportunity = new ProgressionOpportunitySnapshot(record);
                return ProgressionOfferAttachResult.AlreadyAttached;
            }

            if (record.Revision != expectedRevision)
            {
                opportunity = new ProgressionOpportunitySnapshot(record);
                return ProgressionOfferAttachResult.RejectedRevision;
            }

            if (record.State != ProgressionOpportunityState.Pending
                || offer == null
                || !string.Equals(offer.OpportunityId, record.OpportunityId, StringComparison.Ordinal))
            {
                opportunity = new ProgressionOpportunitySnapshot(record);
                return offer == null
                    ? ProgressionOfferAttachResult.RejectedInvalidOffer
                    : ProgressionOfferAttachResult.RejectedState;
            }

            LedgerStateBackup backup = CaptureState();
            try
            {
                record.Offer = offer;
                if (offer.Candidates.Count == 0)
                {
                    record.State = ProgressionOpportunityState.PendingBlocked;
                    record.BlockedReason = ProgressionOfferConstants.NoValidCandidateReason;
                }

                record.Revision++;
                mutationObserver?.Invoke(ProgressionLedgerMutationPoint.OfferAttached);
                opportunity = new ProgressionOpportunitySnapshot(record);
                return ProgressionOfferAttachResult.Attached;
            }
            catch
            {
                RestoreState(backup);
                TryGetOpportunity(opportunityId, out opportunity);
                return ProgressionOfferAttachResult.Faulted;
            }
        }

        public IReadOnlyList<ProgressionOpportunitySnapshot> GetSnapshots()
        {
            return recordsById.Values
                .OrderBy(record => record.OpportunityId, StringComparer.Ordinal)
                .Select(record => new ProgressionOpportunitySnapshot(record))
                .ToArray();
        }

        public ProgressionConsumeResult TryReserveConsume(
            string opportunityId,
            int expectedRevision,
            out ProgressionOpportunitySnapshot opportunity)
        {
            opportunity = null;
            if (!recordsById.TryGetValue(
                    opportunityId ?? string.Empty,
                    out ProgressionOpportunityRecord record))
            {
                return ProgressionConsumeResult.RejectedNotFound;
            }

            if (record.Revision != expectedRevision)
            {
                opportunity = new ProgressionOpportunitySnapshot(record);
                return ProgressionConsumeResult.RejectedRevision;
            }

            if (record.State != ProgressionOpportunityState.Pending)
            {
                opportunity = new ProgressionOpportunitySnapshot(record);
                return ProgressionConsumeResult.RejectedState;
            }

            ProgressionChapterSummary summary = GetChapterSummary(includeConsuming: true);
            if (!capPolicy.AllowsConsume(record.SourceCategory, summary))
            {
                opportunity = new ProgressionOpportunitySnapshot(record);
                return ProgressionConsumeResult.RejectedCap;
            }

            return MutateRecordAtomically(
                record,
                ProgressionLedgerMutationPoint.ConsumeReserved,
                () =>
                {
                    record.State = ProgressionOpportunityState.Consuming;
                    record.ConsumeReservationId = Guid.NewGuid().ToString("N");
                    record.Revision++;
                },
                ProgressionConsumeResult.Reserved,
                out opportunity);
        }

        public ProgressionStateResult TryMarkPendingBlocked(
            string opportunityId,
            int expectedRevision,
            string blockedReason,
            out ProgressionOpportunitySnapshot opportunity)
        {
            opportunity = null;
            if (!TryGetRecord(opportunityId, out ProgressionOpportunityRecord record))
            {
                return ProgressionStateResult.RejectedNotFound;
            }

            if (record.Revision != expectedRevision)
            {
                opportunity = new ProgressionOpportunitySnapshot(record);
                return ProgressionStateResult.RejectedRevision;
            }

            if (record.State == ProgressionOpportunityState.PendingBlocked
                && string.Equals(record.BlockedReason, blockedReason, StringComparison.Ordinal))
            {
                opportunity = new ProgressionOpportunitySnapshot(record);
                return ProgressionStateResult.Unchanged;
            }

            if (record.State != ProgressionOpportunityState.Pending
                || string.IsNullOrWhiteSpace(blockedReason))
            {
                opportunity = new ProgressionOpportunitySnapshot(record);
                return ProgressionStateResult.RejectedState;
            }

            return MutatePendingStateAtomically(
                record,
                ProgressionLedgerMutationPoint.Blocked,
                () =>
                {
                    record.State = ProgressionOpportunityState.PendingBlocked;
                    record.BlockedReason = blockedReason;
                    record.Revision++;
                },
                out opportunity);
        }

        public ProgressionStateResult TryRestorePending(
            string opportunityId,
            int expectedRevision,
            out ProgressionOpportunitySnapshot opportunity)
        {
            opportunity = null;
            if (!TryGetRecord(opportunityId, out ProgressionOpportunityRecord record))
            {
                return ProgressionStateResult.RejectedNotFound;
            }

            if (record.Revision != expectedRevision)
            {
                opportunity = new ProgressionOpportunitySnapshot(record);
                return ProgressionStateResult.RejectedRevision;
            }

            if (record.State == ProgressionOpportunityState.Pending)
            {
                opportunity = new ProgressionOpportunitySnapshot(record);
                return ProgressionStateResult.Unchanged;
            }

            if (record.State != ProgressionOpportunityState.PendingBlocked)
            {
                opportunity = new ProgressionOpportunitySnapshot(record);
                return ProgressionStateResult.RejectedState;
            }

            return MutatePendingStateAtomically(
                record,
                ProgressionLedgerMutationPoint.Unblocked,
                () =>
                {
                    record.State = ProgressionOpportunityState.Pending;
                    record.BlockedReason = string.Empty;
                    record.Revision++;
                },
                out opportunity);
        }

        public ProgressionConsumeResult TryCommitConsume(
            string opportunityId,
            string reservationId,
            out ProgressionOpportunitySnapshot opportunity)
        {
            return TryCommitConsume(
                opportunityId,
                reservationId,
                null,
                out opportunity);
        }

        public ProgressionConsumeResult TryCommitConsume(
            string opportunityId,
            string reservationId,
            ProgressionApplyReceipt receipt,
            out ProgressionOpportunitySnapshot opportunity)
        {
            opportunity = null;
            if (!TryGetConsumingRecord(
                    opportunityId,
                    reservationId,
                    out ProgressionOpportunityRecord record,
                    out ProgressionConsumeResult rejection))
            {
                TryGetOpportunity(opportunityId, out opportunity);
                return rejection;
            }

            return MutateRecordAtomically(
                record,
                ProgressionLedgerMutationPoint.ConsumeApplied,
                () =>
                {
                    record.State = ProgressionOpportunityState.Applied;
                    record.ConsumeReservationId = string.Empty;
                    record.AppliedReceipt = receipt;
                    record.Revision++;
                },
                ProgressionConsumeResult.Applied,
                out opportunity);
        }

        public ProgressionConsumeResult TryRollbackConsume(
            string opportunityId,
            string reservationId,
            out ProgressionOpportunitySnapshot opportunity)
        {
            opportunity = null;
            if (!TryGetConsumingRecord(
                    opportunityId,
                    reservationId,
                    out ProgressionOpportunityRecord record,
                    out ProgressionConsumeResult rejection))
            {
                TryGetOpportunity(opportunityId, out opportunity);
                return rejection;
            }

            return MutateRecordAtomically(
                record,
                ProgressionLedgerMutationPoint.ConsumeRolledBack,
                () =>
                {
                    record.State = ProgressionOpportunityState.Pending;
                    record.ConsumeReservationId = string.Empty;
                    record.Revision++;
                },
                ProgressionConsumeResult.RolledBack,
                out opportunity);
        }

        public ProgressionChapterSummary GetChapterSummary()
        {
            return GetChapterSummary(includeConsuming: false);
        }

        private ProgressionChapterSummary GetChapterSummary(bool includeConsuming)
        {
            return GetChapterSummary(includeConsuming, includePrepared: false);
        }

        private ProgressionChapterSummary GetChapterSummary(bool includeConsuming, bool includePrepared)
        {
            int fixedEarned = 0;
            int randomEarned = 0;
            int fixedApplied = 0;
            int randomApplied = 0;

            foreach (ProgressionOpportunityRecord record in recordsById.Values)
            {
                if (record.SourceCategory == ProgressionSourceCategory.Fixed)
                {
                    fixedEarned++;
                    if (record.State == ProgressionOpportunityState.Applied
                        || (includeConsuming
                            && record.State == ProgressionOpportunityState.Consuming))
                    {
                        fixedApplied++;
                    }
                }
                else
                {
                    randomEarned++;
                    if (record.State == ProgressionOpportunityState.Applied
                        || (includeConsuming
                            && record.State == ProgressionOpportunityState.Consuming))
                    {
                        randomApplied++;
                    }
                }
            }

            if (includePrepared)
            {
                foreach (ProgressionEarnPreparation prepared in preparedEarns.Values)
                {
                    if (prepared.Request.SourceCategory == ProgressionSourceCategory.Fixed) fixedEarned++;
                    else randomEarned++;
                }
            }

            return new ProgressionChapterSummary(
                fixedEarned,
                randomEarned,
                fixedApplied,
                randomApplied);
        }

        private bool TryGetExisting(
            ProgressionEarnRequest request,
            out ProgressionOpportunityRecord record)
        {
            record = null;
            if (opportunityIdBySegment.TryGetValue(
                    request.SegmentId,
                    out string segmentOpportunityId)
                && recordsById.TryGetValue(segmentOpportunityId, out record))
            {
                return true;
            }

            return opportunityIdByCause.TryGetValue(
                       request.CauseKey,
                       out string causeOpportunityId)
                   && recordsById.TryGetValue(causeOpportunityId, out record);
        }

        private bool TryGetConsumingRecord(
            string opportunityId,
            string reservationId,
            out ProgressionOpportunityRecord record,
            out ProgressionConsumeResult rejection)
        {
            record = null;
            rejection = ProgressionConsumeResult.RejectedNotFound;
            if (string.IsNullOrWhiteSpace(opportunityId)
                || !recordsById.TryGetValue(opportunityId, out record))
            {
                return false;
            }

            if (record.State != ProgressionOpportunityState.Consuming)
            {
                rejection = ProgressionConsumeResult.RejectedState;
                return false;
            }

            if (string.IsNullOrWhiteSpace(reservationId)
                || !string.Equals(
                    record.ConsumeReservationId,
                    reservationId,
                    StringComparison.Ordinal))
            {
                rejection = ProgressionConsumeResult.RejectedReservation;
                return false;
            }

            return true;
        }

        private bool TryGetRecord(
            string opportunityId,
            out ProgressionOpportunityRecord record)
        {
            record = null;
            return !string.IsNullOrWhiteSpace(opportunityId)
                   && recordsById.TryGetValue(opportunityId, out record);
        }

        private ProgressionConsumeResult MutateRecordAtomically(
            ProgressionOpportunityRecord record,
            ProgressionLedgerMutationPoint mutationPoint,
            Action mutation,
            ProgressionConsumeResult success,
            out ProgressionOpportunitySnapshot opportunity)
        {
            LedgerStateBackup backup = CaptureState();
            try
            {
                mutation();
                mutationObserver?.Invoke(mutationPoint);
                opportunity = new ProgressionOpportunitySnapshot(record);
                return success;
            }
            catch
            {
                RestoreState(backup);
                TryGetOpportunity(record.OpportunityId, out opportunity);
                return ProgressionConsumeResult.Faulted;
            }
        }

        private ProgressionStateResult MutatePendingStateAtomically(
            ProgressionOpportunityRecord record,
            ProgressionLedgerMutationPoint mutationPoint,
            Action mutation,
            out ProgressionOpportunitySnapshot opportunity)
        {
            LedgerStateBackup backup = CaptureState();
            try
            {
                mutation();
                mutationObserver?.Invoke(mutationPoint);
                opportunity = new ProgressionOpportunitySnapshot(record);
                return ProgressionStateResult.Changed;
            }
            catch
            {
                RestoreState(backup);
                TryGetOpportunity(record.OpportunityId, out opportunity);
                return ProgressionStateResult.Faulted;
            }
        }

        private LedgerStateBackup CaptureState()
        {
            return new LedgerStateBackup(
                recordsById.ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value.Clone(),
                    StringComparer.Ordinal),
                new Dictionary<string, string>(opportunityIdBySegment, StringComparer.Ordinal),
                new Dictionary<string, string>(opportunityIdByCause, StringComparer.Ordinal),
                new Dictionary<string, ProgressionEarnPreparation>(preparedEarns, StringComparer.Ordinal),
                new Dictionary<string, string>(earnTransactionByOpportunity, StringComparer.Ordinal),
                opportunitySequence);
        }

        private void RestoreState(LedgerStateBackup backup)
        {
            recordsById.Clear();
            foreach (KeyValuePair<string, ProgressionOpportunityRecord> pair in backup.Records)
            {
                recordsById.Add(pair.Key, pair.Value);
            }

            opportunityIdBySegment.Clear();
            foreach (KeyValuePair<string, string> pair in backup.Segments)
            {
                opportunityIdBySegment.Add(pair.Key, pair.Value);
            }

            opportunityIdByCause.Clear();
            foreach (KeyValuePair<string, string> pair in backup.Causes)
            {
                opportunityIdByCause.Add(pair.Key, pair.Value);
            }


            preparedEarns.Clear();
            foreach (KeyValuePair<string, ProgressionEarnPreparation> pair in backup.PreparedEarns)
            {
                preparedEarns.Add(pair.Key, pair.Value);
            }

            earnTransactionByOpportunity.Clear();
            foreach (KeyValuePair<string, string> pair in backup.EarnTransactions)
            {
                earnTransactionByOpportunity.Add(pair.Key, pair.Value);
            }

            opportunitySequence = backup.Sequence;
        }

        private sealed class LedgerStateBackup
        {
            public LedgerStateBackup(
                Dictionary<string, ProgressionOpportunityRecord> records,
                Dictionary<string, string> segments,
                Dictionary<string, string> causes,
                Dictionary<string, ProgressionEarnPreparation> preparedEarns,
                Dictionary<string, string> earnTransactions,
                int sequence)
            {
                Records = records;
                Segments = segments;
                Causes = causes;
                PreparedEarns = preparedEarns;
                EarnTransactions = earnTransactions;
                Sequence = sequence;
            }

            public Dictionary<string, ProgressionOpportunityRecord> Records { get; }
            public Dictionary<string, string> Segments { get; }
            public Dictionary<string, string> Causes { get; }
            public Dictionary<string, ProgressionEarnPreparation> PreparedEarns { get; }
            public Dictionary<string, string> EarnTransactions { get; }
            public int Sequence { get; }
        }
    }
}
