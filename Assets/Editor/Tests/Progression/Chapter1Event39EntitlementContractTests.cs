using NUnit.Framework;
using Progression.Portfolio;
using Stage;

public sealed class Chapter1Event39EntitlementContractTests
{
    [Test]
    public void ReceiptMovesFromPendingToGrantedWithoutInventoryRemovalAuthority()
    {
        var ownership = new PortfolioOutcomeOwnership();
        ownership.ResetForNewRun("run.event39");
        var receipt = Receipt();

        Assert.That(ownership.TryReserveEntitlement(receipt), Is.True);
        Assert.That(ownership.TryCommitEntitlement(receipt,
            PortfolioSceneEntitlementState.GrantedToInventory), Is.True);
        Assert.That(ownership.TryGetEntitlementState(Chapter1Event39SelectionContract.Event39Id,
            "reservation.event39", Chapter1Event39SelectionContract.EntitlementId,
            out PortfolioSceneEntitlementState state), Is.True);
        Assert.That(state, Is.EqualTo(PortfolioSceneEntitlementState.GrantedToInventory));
    }

    [Test]
    public void NewRunClearsSceneEntitlementAndReplayCannotReserveTerminalReceipt()
    {
        var ownership = new PortfolioOutcomeOwnership();
        ownership.ResetForNewRun("run.event39.a");
        PortfolioSceneEntitlementReceipt receipt = Receipt();
        Assert.That(ownership.TryReserveEntitlement(receipt), Is.True);
        Assert.That(ownership.TryCommitEntitlement(receipt,
            PortfolioSceneEntitlementState.ConsumedForRoute), Is.True);
        Assert.That(ownership.TryReserveEntitlement(Receipt()), Is.False);

        ownership.ResetForNewRun("run.event39.b");
        Assert.That(ownership.TryReserveEntitlement(Receipt()), Is.True);
    }

    private static PortfolioSceneEntitlementReceipt Receipt() => new()
    {
        eventId = Chapter1Event39SelectionContract.Event39Id,
        reservationId = "reservation.event39",
        entitlementId = Chapter1Event39SelectionContract.EntitlementId,
        relicId = Chapter1Event39SelectionContract.RelicId,
        state = PortfolioSceneEntitlementState.Pending
    };
}
