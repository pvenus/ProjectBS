using NUnit.Framework;
using Progression.RandomGrowth;

namespace Progression.Tests
{
    public sealed class SafeGrowthNodeOnlyFailureTests
    {
        [Test] public void PrepareIsMutationFreeAndAbortRestoresPendingRetry()
        { var f=Fixture.Create();var before=f.Interaction.State;var p=f.Service.Prepare(f.Cause,f.Token);Assert.That(p.Result,Is.EqualTo(SafeGrowthNodeOnlyFailureResult.Prepared));Assert.That(f.Results.CommittedCount,Is.Zero);Assert.That(f.Interaction.State,Is.EqualTo(before));Assert.That(f.Service.Abort(p.Token),Is.EqualTo(SafeGrowthNodeOnlyFailureResult.Aborted));Assert.That(f.Interaction.State,Is.EqualTo(before)); }
        [Test] public void FinalizeCreatesNodeOnlyTerminalReceipt()
        { var f=Fixture.Create();var p=f.Service.Prepare(f.Cause,f.Token);var r=f.Service.Finalize(p.Token);Assert.That(r.Result,Is.EqualTo(SafeGrowthNodeOnlyFailureResult.Committed));Assert.That(f.Results.CommittedCount,Is.EqualTo(1));Assert.That(f.Interaction.State,Is.EqualTo(SafeGrowthInteractionState.ContentUnavailable));Assert.That(r.EventReceipt.Cause.ResultId,Is.EqualTo(SafeGrowthNodeOnlyFailureIds.ReceiptId));Assert.That(r.EventReceipt.Costs,Is.Empty); }
        [Test] public void SameKeyReplayIsIdempotent()
        { var f=Fixture.Create();var p=f.Service.Prepare(f.Cause,f.Token);var a=f.Service.Finalize(p.Token);var b=f.Service.Prepare(f.Cause,f.Token);Assert.That(a.Result,Is.EqualTo(SafeGrowthNodeOnlyFailureResult.Committed));Assert.That(b.Result,Is.EqualTo(SafeGrowthNodeOnlyFailureResult.Committed));Assert.That(f.Results.CommittedCount,Is.EqualTo(1)); }
        [Test] public void IdentityMismatchMutatesNothing()
        { var f=Fixture.Create();var bad=new SafeGrowthNodeOnlyFailureCause(f.Cause.RunId,f.Cause.StageGenerationId,"wrong",f.Cause.NodeId,f.Cause.ReservationId,f.Cause.EncounteredNodeInstanceId,f.Cause.FailureReceiptId);Assert.That(f.Service.Prepare(bad,f.Token).Result,Is.EqualTo(SafeGrowthNodeOnlyFailureResult.InvalidIdentity));Assert.That(f.Results.CommittedCount,Is.Zero); }
        [Test] public void SuccessOrDeclineTerminalConflicts()
        { var f=Fixture.Create();f.Interaction.TryBeginApply(f.Token);f.Interaction.TryCommitTerminal(f.Token,SafeGrowthInteractionState.SafeGrowthGranted);Assert.That(f.Service.Prepare(f.Cause,f.Token).Result,Is.EqualTo(SafeGrowthNodeOnlyFailureResult.Conflict));Assert.That(f.Results.CommittedCount,Is.Zero); }
        [Test] public void PendingRetryRemainsNonterminalAndCanPrepare()
        { var f=Fixture.Create();f.Interaction.TryBeginApply(f.Token);f.Interaction.TryMarkPendingRetry(f.Token);Assert.That(f.Service.Prepare(f.Cause,f.Token).Result,Is.EqualTo(SafeGrowthNodeOnlyFailureResult.Prepared)); }
        [Test] public void StableTupleAndAuthorityConstantsAreExact()
        { var f=Fixture.Create();Assert.That(f.Cause.StableKey.Split('\n'),Has.Length.EqualTo(7));Assert.That(SafeGrowthNodeOnlyFailureIds.ResultKind,Is.EqualTo("SafePresentationContentUnavailableAfterDisclosure"));Assert.That(SafeGrowthNodeOnlyFailureIds.SemanticClass,Is.EqualTo("NodeOnlyTechnicalFailure"));Assert.That(SafeGrowthNodeOnlyFailureIds.GlobalCopyKey,Is.EqualTo("system.stage.content_unavailable.continue")); }

        private sealed class Fixture
        {
            public StageEventResultLedger Results;public SafeGrowthInteractionOwnership Interaction;public SafeGrowthInteractionToken Token;public SafeGrowthNodeOnlyFailureCause Cause;public SafeGrowthNodeOnlyFailureService Service;
            public static Fixture Create(){var f=new Fixture();var run=new ProgressionRunId("run.failure");f.Results=new StageEventResultLedger();f.Interaction=new SafeGrowthInteractionOwnership();f.Interaction.ResetForNewRun(run);var key=new SafeGrowthInteractionKey(run.Value,"stage.failure",SafeGrowthTransactionIds.ReservationId,"node.instance.failure");f.Interaction.TryEnterPreconfirm(key,SafeGrowthTransactionIds.ObserveChoiceId,"fingerprint",true,out f.Token);f.Cause=new SafeGrowthNodeOnlyFailureCause(run.Value,"stage.failure",SafeGrowthTransactionIds.EventId,"node.act1.random_growth.02.windworn_sword_marks.intro",SafeGrowthTransactionIds.ReservationId,"node.instance.failure",SafeGrowthNodeOnlyFailureIds.ReceiptId);f.Service=new SafeGrowthNodeOnlyFailureService(f.Results,f.Interaction);return f;}
        }
    }
}
