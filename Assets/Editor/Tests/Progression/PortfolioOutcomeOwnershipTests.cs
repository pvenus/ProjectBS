using NUnit.Framework;
using Stage;

public sealed class PortfolioOutcomeOwnershipTests
{
    [Test]
    public void NewRunClearsTerminalFlagDisclosureAndPending()
    {
        PortfolioOutcomeOwnership value = Ready();
        value.TryCommitTerminal("terminal");
        value.TrySetRunFlag("flag");
        value.TryStoreDisclosure("node", new[] { "purpose" });
        value.TryPrepareBattle(Pending("node"));
        value.ResetForNewRun("run.2");
        Assert.That(value.IsResolved("terminal"), Is.False);
        Assert.That(value.RunFlags, Is.Empty);
        Assert.That(value.PendingBattle, Is.Null);
    }

    [Test] public void TerminalIsExactlyOnce() { var v=Ready(); Assert.That(v.TryCommitTerminal("t"),Is.True); Assert.That(v.TryCommitTerminal("t"),Is.False); }
    [Test] public void TerminalRollbackAllowsRetry() { var v=Ready(); v.TryCommitTerminal("t"); Assert.That(v.TryRollbackTerminal("t"),Is.True); Assert.That(v.TryCommitTerminal("t"),Is.True); }
    [Test] public void FlagIsExactlyOnce() { var v=Ready(); Assert.That(v.TrySetRunFlag("f"),Is.True); Assert.That(v.TrySetRunFlag("f"),Is.False); }
    [Test] public void FlagRollbackAllowsRetry() { var v=Ready(); v.TrySetRunFlag("f"); Assert.That(v.TryRollbackRunFlag("f"),Is.True); }
    [Test] public void DisclosureReplayIsIdempotent() { var v=Ready(); Assert.That(v.TryStoreDisclosure("n",new[]{"b","a"}),Is.True); Assert.That(v.TryStoreDisclosure("n",new[]{"a","b"}),Is.True); }
    [Test] public void DisclosureMismatchFailsClosed() { var v=Ready(); v.TryStoreDisclosure("n",new[]{"a"}); Assert.That(v.TryStoreDisclosure("n",new[]{"b"}),Is.False); }
    [Test] public void DisclosureRollbackAllowsReplacement() { var v=Ready(); v.TryStoreDisclosure("n",new[]{"a"}); v.TryRollbackDisclosure("n"); Assert.That(v.TryStoreDisclosure("n",new[]{"b"}),Is.True); }
    [Test] public void BattlePrepareIsExclusive() { var v=Ready(); Assert.That(v.TryPrepareBattle(Pending("a")),Is.True); Assert.That(v.TryPrepareBattle(Pending("b")),Is.False); }
    [Test] public void BattleAbortRequiresOwnedToken() { var v=Ready(); var p=Pending("a"); v.TryPrepareBattle(p); Assert.That(v.AbortBattle(Pending("a")),Is.False); Assert.That(v.AbortBattle(p),Is.True); }

    private static PortfolioOutcomeOwnership Ready() { var v=new PortfolioOutcomeOwnership(); v.ResetForNewRun("run.1"); return v; }
    private static PortfolioOutcomePendingBattle Pending(string node) => new()
        { nodeInstanceId=node, outcome=PortfolioOutcomeContractTests.ValidData() };
}
