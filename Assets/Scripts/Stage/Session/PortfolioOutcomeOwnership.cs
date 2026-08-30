using System;
using System.Collections.Generic;
using System.Linq;

namespace Stage
{
    public enum PortfolioSceneEntitlementState
    {
        None = 0,
        Pending = 10,
        GrantedToInventory = 20,
        ConsumedForRoute = 30
    }

    [Serializable]
    public sealed class PortfolioSceneEntitlementReceipt
    {
        public string eventId;
        public string reservationId;
        public string entitlementId;
        public string relicId;
        public PortfolioSceneEntitlementState state;

        public bool HasIdentity => !string.IsNullOrWhiteSpace(eventId)
            && !string.IsNullOrWhiteSpace(reservationId)
            && !string.IsNullOrWhiteSpace(entitlementId)
            && !string.IsNullOrWhiteSpace(relicId);
    }

    public enum PortfolioAtomicTransactionKind
    {
        None = 0,
        Immediate = 10,
        BattleRouteCommit = 20,
        BattleGoldGrant = 30,
        ParentChildContinuation = 40
    }

    [Serializable]
    public sealed class PortfolioAtomicTransactionReceipt
    {
        public PortfolioAtomicTransactionKind kind;
        public string transactionId;
        public string eventId;
        public string nodeId;
        public string choiceId;
        public string resultId;
        public string snapshotId;
        public int expectedRevision;
        public bool effectApplied;
        public bool terminalCommitted;

        public bool HasIdentity => kind != PortfolioAtomicTransactionKind.None
            && !string.IsNullOrWhiteSpace(transactionId)
            && !string.IsNullOrWhiteSpace(eventId)
            && !string.IsNullOrWhiteSpace(nodeId)
            && !string.IsNullOrWhiteSpace(choiceId)
            && !string.IsNullOrWhiteSpace(resultId);
    }

    [Serializable]
    public sealed class PortfolioOutcomePendingBattle
    {
        public string runId;
        public string stageGenerationId;
        public string nodeInstanceId;
        public PortfolioOutcomeExecutionData outcome;
        public string fixedInventoryId;
        public string eligibleFingerprint;
        public string rewardClaimCauseId;
        public Event25RelicClaimState rewardClaimState;
        public PortfolioOutcomeOperationKind continuationKind;
        public int goldBefore;
        public int walletRevisionBefore;
        public int routeRevisionBefore;
        public string routeSourceBefore;
        public string routeTargetBefore;
        public bool continuationApplied;
    }

    [Serializable]
    public sealed class PortfolioNextEventContinuationReceipt
    {
        public string parentEventId;
        public string parentNodeId;
        public string parentChoiceId;
        public string parentResultId;
        public string parentReservationId;
        public string childEventId;
        public string childNodeId;
        public string childReservationId;
        public bool childOpened;
        public bool childTerminal;
    }

    [Serializable]
    public sealed class PortfolioOutcomeOwnership
    {
        private string runId = string.Empty;
        private readonly HashSet<string> terminalKeys = new(StringComparer.Ordinal);
        private readonly HashSet<string> runFlags = new(StringComparer.Ordinal);
        private readonly Dictionary<string, string[]> disclosures = new(StringComparer.Ordinal);
        private readonly Dictionary<string, StageRouteCandidateSnapshot> routeSnapshots =
            new(StringComparer.Ordinal);
        [UnityEngine.SerializeField]
        private PortfolioSceneEntitlementReceipt sceneEntitlement;

        public PortfolioOutcomePendingBattle PendingBattle { get; private set; }
        public PortfolioAtomicTransactionReceipt PendingTransaction { get; private set; }
        public PortfolioNextEventContinuationReceipt PendingContinuation { get; private set; }
        public PortfolioSceneEntitlementReceipt SceneEntitlement => sceneEntitlement;
        public IReadOnlyCollection<string> RunFlags => runFlags;

        public void ResetForNewRun(string value)
        {
            runId = value ?? string.Empty;
            terminalKeys.Clear();
            runFlags.Clear();
            disclosures.Clear();
            routeSnapshots.Clear();
            PendingBattle = null;
            PendingTransaction = null;
            PendingContinuation = null;
            sceneEntitlement = null;
        }

        public bool IsResolved(string key) => !string.IsNullOrWhiteSpace(key)
            && terminalKeys.Contains(key);

        public bool TryCommitTerminal(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return false;
            return terminalKeys.Add(key);
        }

        public bool TryRollbackTerminal(string key) => terminalKeys.Remove(key);

        public bool TrySetRunFlag(string flagId)
        {
            if (string.IsNullOrWhiteSpace(runId) || string.IsNullOrWhiteSpace(flagId)) return false;
            return runFlags.Add(flagId);
        }

        public bool TryRollbackRunFlag(string flagId) => runFlags.Remove(flagId);

        public bool TryReserveEntitlement(PortfolioSceneEntitlementReceipt receipt)
        {
            if (receipt?.HasIdentity != true
                || receipt.state != PortfolioSceneEntitlementState.Pending) return false;
            if (sceneEntitlement == null)
            {
                sceneEntitlement = receipt;
                return true;
            }
            return SameEntitlement(sceneEntitlement, receipt)
                && sceneEntitlement.state == PortfolioSceneEntitlementState.Pending;
        }

        public bool TryCommitEntitlement(PortfolioSceneEntitlementReceipt receipt,
            PortfolioSceneEntitlementState terminalState)
        {
            if (receipt == null || !ReferenceEquals(sceneEntitlement, receipt)
                || receipt.state != PortfolioSceneEntitlementState.Pending
                || terminalState is not (PortfolioSceneEntitlementState.GrantedToInventory
                    or PortfolioSceneEntitlementState.ConsumedForRoute)) return false;
            receipt.state = terminalState;
            return true;
        }

        public bool TryReleaseEntitlement(PortfolioSceneEntitlementReceipt receipt)
        {
            if (receipt == null || !ReferenceEquals(sceneEntitlement, receipt)
                || receipt.state != PortfolioSceneEntitlementState.Pending) return false;
            sceneEntitlement = null;
            return true;
        }

        public bool TryRollbackEntitlement(PortfolioSceneEntitlementReceipt receipt)
        {
            if (receipt == null || !ReferenceEquals(sceneEntitlement, receipt)) return false;
            sceneEntitlement = null;
            return true;
        }

        public bool TryGetEntitlementState(string eventId, string reservationId,
            string entitlementId, out PortfolioSceneEntitlementState state)
        {
            state = PortfolioSceneEntitlementState.None;
            if (sceneEntitlement == null
                || !string.Equals(sceneEntitlement.eventId, eventId, StringComparison.Ordinal)
                || !string.Equals(sceneEntitlement.reservationId, reservationId, StringComparison.Ordinal)
                || !string.Equals(sceneEntitlement.entitlementId, entitlementId,
                    StringComparison.Ordinal)) return false;
            state = sceneEntitlement.state;
            return true;
        }

        private static bool SameEntitlement(PortfolioSceneEntitlementReceipt left,
            PortfolioSceneEntitlementReceipt right) =>
            string.Equals(left.eventId, right.eventId, StringComparison.Ordinal)
            && string.Equals(left.reservationId, right.reservationId, StringComparison.Ordinal)
            && string.Equals(left.entitlementId, right.entitlementId, StringComparison.Ordinal)
            && string.Equals(left.relicId, right.relicId, StringComparison.Ordinal);

        public bool TryStoreDisclosure(string key, IEnumerable<string> successorPurposeIds)
        {
            if (string.IsNullOrWhiteSpace(key) || successorPurposeIds == null) return false;
            string[] values = successorPurposeIds.Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
            if (values.Length == 0) return false;
            if (disclosures.TryGetValue(key, out string[] existing))
                return existing.SequenceEqual(values, StringComparer.Ordinal);
            disclosures[key] = values;
            return true;
        }

        public bool TryRollbackDisclosure(string key) => disclosures.Remove(key);

        public bool TryStoreRouteSnapshot(StageRouteCandidateSnapshot snapshot)
        {
            if (snapshot == null || string.IsNullOrWhiteSpace(snapshot.snapshotId)
                || string.IsNullOrWhiteSpace(snapshot.sourceNodeId)
                || snapshot.candidates == null || snapshot.candidates.Count < 2) return false;
            if (routeSnapshots.TryGetValue(snapshot.snapshotId, out StageRouteCandidateSnapshot existing))
                return SameRouteSnapshot(existing, snapshot);
            routeSnapshots[snapshot.snapshotId] = snapshot;
            return true;
        }

        public bool TryGetRouteSnapshot(string snapshotId, out StageRouteCandidateSnapshot snapshot) =>
            routeSnapshots.TryGetValue(snapshotId ?? string.Empty, out snapshot);

        private static bool SameRouteSnapshot(
            StageRouteCandidateSnapshot left, StageRouteCandidateSnapshot right) =>
            left.graphRevision == right.graphRevision
            && string.Equals(left.sourceNodeId, right.sourceNodeId, StringComparison.Ordinal)
            && left.candidates.Select(item => $"{item.nodeId}|{item.purposeId}|{item.remainingNodeCount}")
                .SequenceEqual(right.candidates.Select(item =>
                    $"{item.nodeId}|{item.purposeId}|{item.remainingNodeCount}"),
                    StringComparer.Ordinal);

        public bool TryPrepareBattle(PortfolioOutcomePendingBattle pending)
        {
            if (pending?.outcome == null || string.IsNullOrWhiteSpace(pending.nodeInstanceId)) return false;
            if (PendingBattle == null) { PendingBattle = pending; return true; }
            return ReferenceEquals(PendingBattle.outcome, pending.outcome)
                && string.Equals(PendingBattle.nodeInstanceId, pending.nodeInstanceId, StringComparison.Ordinal);
        }

        public PortfolioOutcomePendingBattle ConsumeBattle(string nodeInstanceId)
        {
            if (PendingBattle == null || !string.Equals(PendingBattle.nodeInstanceId,
                    nodeInstanceId, StringComparison.Ordinal)) return null;
            PortfolioOutcomePendingBattle result = PendingBattle;
            PendingBattle = null;
            return result;
        }

        public bool AbortBattle(PortfolioOutcomePendingBattle pending)
        {
            if (pending == null || !ReferenceEquals(PendingBattle, pending)) return false;
            PendingBattle = null;
            return true;
        }

        public bool TryReserveTransaction(PortfolioAtomicTransactionReceipt receipt)
        {
            if (receipt?.HasIdentity != true || IsResolved(receipt.transactionId)) return false;
            if (PendingTransaction == null)
            {
                PendingTransaction = receipt;
                return true;
            }
            return SameTransaction(PendingTransaction, receipt);
        }

        public bool TryCommitTransaction(PortfolioAtomicTransactionReceipt receipt)
        {
            if (receipt == null || !ReferenceEquals(PendingTransaction, receipt)
                || !receipt.effectApplied || !receipt.terminalCommitted
                || !TryCommitTerminal(receipt.transactionId)) return false;
            PendingTransaction = null;
            return true;
        }

        public bool TryReleaseTransaction(PortfolioAtomicTransactionReceipt receipt)
        {
            if (receipt == null || !ReferenceEquals(PendingTransaction, receipt)) return false;
            PendingTransaction = null;
            return true;
        }

        private static bool SameTransaction(
            PortfolioAtomicTransactionReceipt left,
            PortfolioAtomicTransactionReceipt right) =>
            left.kind == right.kind
            && string.Equals(left.transactionId, right.transactionId, StringComparison.Ordinal)
            && string.Equals(left.eventId, right.eventId, StringComparison.Ordinal)
            && string.Equals(left.nodeId, right.nodeId, StringComparison.Ordinal)
            && string.Equals(left.choiceId, right.choiceId, StringComparison.Ordinal)
            && string.Equals(left.resultId, right.resultId, StringComparison.Ordinal)
            && string.Equals(left.snapshotId, right.snapshotId, StringComparison.Ordinal)
            && left.expectedRevision == right.expectedRevision;

        public bool TryReserveContinuation(PortfolioNextEventContinuationReceipt receipt)
        {
            if (!IsContinuationValid(receipt)) return false;
            if (PendingContinuation == null)
            {
                PendingContinuation = receipt;
                return true;
            }
            return SameContinuation(PendingContinuation, receipt);
        }

        public bool TryCommitContinuation(string childEventId, string childNodeId)
        {
            PortfolioNextEventContinuationReceipt receipt = PendingContinuation;
            if (!IsContinuationValid(receipt)
                || !string.Equals(receipt.childEventId, childEventId, StringComparison.Ordinal)
                || !string.Equals(receipt.childNodeId, childNodeId, StringComparison.Ordinal))
                return false;
            receipt.childTerminal = true;
            if (!TryCommitTerminal(receipt.parentResultId)) return false;
            PendingContinuation = null;
            return true;
        }

        public bool TryReleaseContinuation(PortfolioNextEventContinuationReceipt receipt)
        {
            if (receipt == null || !ReferenceEquals(PendingContinuation, receipt)) return false;
            PendingContinuation = null;
            return true;
        }

        private static bool IsContinuationValid(PortfolioNextEventContinuationReceipt receipt) =>
            receipt != null
            && !string.IsNullOrWhiteSpace(receipt.parentEventId)
            && !string.IsNullOrWhiteSpace(receipt.parentNodeId)
            && !string.IsNullOrWhiteSpace(receipt.parentChoiceId)
            && !string.IsNullOrWhiteSpace(receipt.parentResultId)
            && !string.IsNullOrWhiteSpace(receipt.parentReservationId)
            && !string.IsNullOrWhiteSpace(receipt.childEventId)
            && !string.IsNullOrWhiteSpace(receipt.childNodeId)
            && !string.IsNullOrWhiteSpace(receipt.childReservationId);

        private static bool SameContinuation(
            PortfolioNextEventContinuationReceipt left,
            PortfolioNextEventContinuationReceipt right) =>
            string.Equals(left.parentEventId, right.parentEventId, StringComparison.Ordinal)
            && string.Equals(left.parentNodeId, right.parentNodeId, StringComparison.Ordinal)
            && string.Equals(left.parentChoiceId, right.parentChoiceId, StringComparison.Ordinal)
            && string.Equals(left.parentResultId, right.parentResultId, StringComparison.Ordinal)
            && string.Equals(left.parentReservationId, right.parentReservationId, StringComparison.Ordinal)
            && string.Equals(left.childEventId, right.childEventId, StringComparison.Ordinal)
            && string.Equals(left.childNodeId, right.childNodeId, StringComparison.Ordinal)
            && string.Equals(left.childReservationId, right.childReservationId, StringComparison.Ordinal);
    }
}
