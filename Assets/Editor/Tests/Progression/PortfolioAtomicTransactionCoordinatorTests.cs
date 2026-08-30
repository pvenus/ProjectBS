using NUnit.Framework;
using Stage;

public sealed class PortfolioAtomicTransactionCoordinatorTests
{
    [Test]
    public void ApplyAndCommit_RecordsTerminalExactlyOnce()
    {
        PortfolioOutcomeOwnership ownership = NewOwnership();
        PortfolioAtomicTransactionReceipt receipt = NewReceipt();
        var coordinator = new PortfolioAtomicTransactionCoordinator();
        int value = 0;

        Assert.That(coordinator.TryApply(ownership, receipt, () => { value++; return true; },
            () => value--, out _, out string error), Is.True, error);
        Assert.That(coordinator.TryCommit(ownership, receipt, out error), Is.True, error);
        Assert.That(value, Is.EqualTo(1));
        Assert.That(ownership.IsResolved(receipt.transactionId), Is.True);
    }

    [Test]
    public void ApplyFailure_ReleasesReservation()
    {
        PortfolioOutcomeOwnership ownership = NewOwnership();
        PortfolioAtomicTransactionReceipt receipt = NewReceipt();
        var coordinator = new PortfolioAtomicTransactionCoordinator();

        Assert.That(coordinator.TryApply(ownership, receipt, () => false, () => { },
            out _, out string error), Is.False);
        Assert.That(error, Is.EqualTo("PORTFOLIO_ATOMIC_EFFECT_FAILED"));
        Assert.That(ownership.PendingTransaction, Is.Null);
    }

    [Test]
    public void Rollback_ReversesEffectAndReleasesReservation()
    {
        PortfolioOutcomeOwnership ownership = NewOwnership();
        PortfolioAtomicTransactionReceipt receipt = NewReceipt();
        var coordinator = new PortfolioAtomicTransactionCoordinator();
        int value = 0;

        Assert.That(coordinator.TryApply(ownership, receipt, () => { value = 7; return true; },
            () => value = 0, out System.Action rollback, out string error), Is.True, error);
        rollback();
        Assert.That(value, Is.Zero);
        Assert.That(ownership.PendingTransaction, Is.Null);
    }

    [Test]
    public void DifferentPendingIdentity_IsRejected()
    {
        PortfolioOutcomeOwnership ownership = NewOwnership();
        Assert.That(ownership.TryReserveTransaction(NewReceipt()), Is.True);
        PortfolioAtomicTransactionReceipt other = NewReceipt();
        other.choiceId = "choice.other";
        Assert.That(ownership.TryReserveTransaction(other), Is.False);
    }

    [Test]
    public void ResetForNewRun_ClearsPendingTransactionAndContinuation()
    {
        PortfolioOutcomeOwnership ownership = NewOwnership();
        Assert.That(ownership.TryReserveTransaction(NewReceipt()), Is.True);
        Assert.That(ownership.TryReserveContinuation(NewContinuation()), Is.True);
        ownership.ResetForNewRun("run.two");
        Assert.That(ownership.PendingTransaction, Is.Null);
        Assert.That(ownership.PendingContinuation, Is.Null);
    }

    [Test]
    public void Continuation_DuplicateIdentityIsIdempotent()
    {
        PortfolioOutcomeOwnership ownership = NewOwnership();
        Assert.That(ownership.TryReserveContinuation(NewContinuation()), Is.True);
        Assert.That(ownership.TryReserveContinuation(NewContinuation()), Is.True);
    }

    [Test]
    public void Continuation_MismatchIsRejected()
    {
        PortfolioOutcomeOwnership ownership = NewOwnership();
        Assert.That(ownership.TryReserveContinuation(NewContinuation()), Is.True);
        PortfolioNextEventContinuationReceipt mismatch = NewContinuation();
        mismatch.childEventId = "event.other";
        Assert.That(ownership.TryReserveContinuation(mismatch), Is.False);
    }

    [Test]
    public void Continuation_ChildTerminalCommitsParentResult()
    {
        PortfolioOutcomeOwnership ownership = NewOwnership();
        PortfolioNextEventContinuationReceipt receipt = NewContinuation();
        Assert.That(ownership.TryReserveContinuation(receipt), Is.True);
        Assert.That(ownership.TryCommitContinuation(receipt.childEventId, receipt.childNodeId), Is.True);
        Assert.That(ownership.IsResolved(receipt.parentResultId), Is.True);
    }

    private static PortfolioOutcomeOwnership NewOwnership()
    {
        var ownership = new PortfolioOutcomeOwnership();
        ownership.ResetForNewRun("run.one");
        return ownership;
    }

    private static PortfolioAtomicTransactionReceipt NewReceipt() => new()
    {
        kind = PortfolioAtomicTransactionKind.Immediate,
        transactionId = "transaction.one",
        eventId = "event.one",
        nodeId = "node.one",
        choiceId = "choice.one",
        resultId = "result.one",
        snapshotId = "snapshot.one",
        expectedRevision = 0
    };

    private static PortfolioNextEventContinuationReceipt NewContinuation() => new()
    {
        parentEventId = "event.parent",
        parentNodeId = "node.parent",
        parentChoiceId = "choice.parent",
        parentResultId = "result.parent",
        parentReservationId = "reservation.parent",
        childEventId = "event.child",
        childNodeId = "node.child",
        childReservationId = "reservation.child"
    };
}
