using System;
using System.Collections.Generic;

namespace Progression
{
    internal enum StageEventResultMutationPoint { Reserved, Committed, RolledBack }
    internal enum StageEventResultLedgerResult { Reserved, Committed, AlreadyResolved, Rejected, RolledBack, Faulted, RecoveryRequired }

    public sealed class StageEventResultLedger
    {
        private readonly Dictionary<string, Entry> entries = new(StringComparer.Ordinal);
        private readonly Action<StageEventResultMutationPoint> observer;
        public StageEventResultLedger() { }
        internal StageEventResultLedger(Action<StageEventResultMutationPoint> observer) => this.observer = observer;
        public int CommittedCount { get { int count = 0; foreach (Entry e in entries.Values) if (e.Receipt != null) count++; return count; } }

        internal StageEventResultLedgerResult TryReserve(StageEventCause cause, StageEventChoiceKind choice,
            string transactionId, out string reservationId, out StageEventResultReceipt existing)
        {
            reservationId = string.Empty; existing = null;
            if (cause == null || !cause.IsValid || string.IsNullOrWhiteSpace(transactionId)) return StageEventResultLedgerResult.Rejected;
            if (entries.TryGetValue(cause.EventKey, out Entry found))
            {
                existing = found.Receipt;
                return found.RecoveryRequired ? StageEventResultLedgerResult.RecoveryRequired :
                    found.Receipt != null ? StageEventResultLedgerResult.AlreadyResolved : StageEventResultLedgerResult.Rejected;
            }
            try
            {
                reservationId = "event-result-" + Guid.NewGuid().ToString("N");
                entries.Add(cause.EventKey, new Entry(cause, choice, transactionId, reservationId));
                observer?.Invoke(StageEventResultMutationPoint.Reserved);
                return StageEventResultLedgerResult.Reserved;
            }
            catch { entries.Remove(cause.EventKey); reservationId = string.Empty; return StageEventResultLedgerResult.Faulted; }
        }

        internal StageEventResultLedgerResult TryCommit(string reservationId, string opportunityId,
            IReadOnlyList<PartyVitalMutation> costs, out StageEventResultReceipt receipt)
        {
            receipt = null; Entry entry = Find(reservationId);
            if (entry == null || entry.Receipt != null) return StageEventResultLedgerResult.Rejected;
            try
            {
                receipt = new StageEventResultReceipt(entry.TransactionId, entry.Cause, entry.Choice, opportunityId, costs);
                entry.Receipt = receipt;
                observer?.Invoke(StageEventResultMutationPoint.Committed);
                return StageEventResultLedgerResult.Committed;
            }
            catch { entry.Receipt = null; receipt = null; return StageEventResultLedgerResult.Faulted; }
        }

        internal StageEventResultLedgerResult TryAbortOrRollback(string reservationId)
        {
            Entry entry = Find(reservationId); if (entry == null) return StageEventResultLedgerResult.Rejected;
            try { entries.Remove(entry.Cause.EventKey); observer?.Invoke(StageEventResultMutationPoint.RolledBack); return StageEventResultLedgerResult.RolledBack; }
            catch { entries[entry.Cause.EventKey] = entry; return StageEventResultLedgerResult.Faulted; }
        }

        internal void MarkRecoveryRequired(string reservationId) { Entry entry = Find(reservationId); if (entry != null) entry.RecoveryRequired = true; }
        private Entry Find(string id) { foreach (Entry e in entries.Values) if (string.Equals(e.ReservationId, id, StringComparison.Ordinal)) return e; return null; }
        private sealed class Entry
        {
            public Entry(StageEventCause cause, StageEventChoiceKind choice, string transactionId, string reservationId)
            { Cause = cause; Choice = choice; TransactionId = transactionId; ReservationId = reservationId; }
            public StageEventCause Cause; public StageEventChoiceKind Choice; public string TransactionId;
            public string ReservationId; public StageEventResultReceipt Receipt; public bool RecoveryRequired;
        }
    }
}
