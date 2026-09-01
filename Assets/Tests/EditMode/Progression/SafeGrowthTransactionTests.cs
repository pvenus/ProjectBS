using System;
using NUnit.Framework;
using Progression.RandomGrowth;

namespace Progression.Tests
{
    public sealed class SafeGrowthTransactionTests
    {
        [Test]
        public void PreconfirmAndCancelMutateNoLedger()
        {
            Context c = Context.Create();
            Assert.That(c.Interaction.State, Is.EqualTo(SafeGrowthInteractionState.Preconfirm));
            Assert.That(c.Interaction.TryCancelPreconfirm(c.Token), Is.EqualTo(SafeGrowthInteractionResult.Changed));
            Assert.That(c.Interaction.State, Is.EqualTo(SafeGrowthInteractionState.Offerable));
            Assert.That(c.Ledger.Count, Is.Zero);
            Assert.That(c.Results.CommittedCount, Is.Zero);
        }

        [Test]
        public void ObserveSuccessCommitsCostFreePendingAndTerminal()
        {
            Context c = Context.Create();
            SafeGrowthTransactionReceipt receipt = c.Service.Execute(c.Observe());
            Assert.That(receipt.Result, Is.EqualTo(SafeGrowthTransactionResult.Succeeded));
            Assert.That(receipt.Opportunity.State, Is.EqualTo(ProgressionOpportunityState.Pending));
            Assert.That(receipt.EventReceipt.Costs, Is.Empty);
            Assert.That(c.Ledger.Count, Is.EqualTo(1));
            Assert.That(c.Results.CommittedCount, Is.EqualTo(1));
            Assert.That(c.Interaction.State, Is.EqualTo(SafeGrowthInteractionState.SafeGrowthGranted));
        }

        [Test]
        public void Event21TypedIdentityUsesItsOwnReservationAndResultContract()
        {
            Context c = Context.Create();
            Assert.That(RandomGrowthEventIdentityCatalog.TryResolve(
                "event.act1.random_event.21.breath_between_water_drops",
                "choice.act1.random_event.21.breath_between_water_drops.follow_silent_rhythm",
                RandomGrowthPayloadKind.Safe,out var identity),Is.True);
            var interaction = new SafeGrowthInteractionOwnership();
            interaction.ResetForNewRun(c.RunId);
            var key = new SafeGrowthInteractionKey(c.RunId.Value,
                "stage-generation.event21",identity.ReservationId,"node-instance.event21");
            Assert.That(interaction.TryEnterPreconfirm(key,identity.ChoiceId,
                "definition.event21",true,out var token),Is.EqualTo(SafeGrowthInteractionResult.Changed));
            var service = new SafeGrowthTransactionService(c.Ledger,c.Results,interaction);
            var earn = new ProgressionEarnRequest(identity.SegmentId,
                ProgressionSourceCategory.Random,ProgressionSourceType.RandomEventSafe,
                identity.SourceId,identity.ResultId);

            SafeGrowthTransactionReceipt receipt = service.Execute(
                new SafeGrowthTransactionCommand(token,SafeGrowthTransactionChoice.Observe,
                    earn,2,identity));

            Assert.That(receipt.Result,Is.EqualTo(SafeGrowthTransactionResult.Succeeded));
            Assert.That(receipt.EventReceipt.Cause.EventId,Is.EqualTo(identity.EventId));
            Assert.That(receipt.EventReceipt.Cause.ResultId,Is.EqualTo(identity.ResultId));
        }

        [Test]
        public void PrepareReservesExclusivelyWithoutVisibleLedgerMutation()
        {
            Context c = Context.Create();
            SafeGrowthPrepareReceipt prepared = c.Service.TryPrepare(c.Observe());
            Assert.That(prepared.Result, Is.EqualTo(SafeGrowthPrepareResult.Prepared));
            Assert.That(prepared.Token, Is.Not.Null);
            Assert.That(c.Ledger.Count, Is.Zero);
            Assert.That(c.Results.CommittedCount, Is.Zero);
            Assert.That(c.Interaction.State, Is.EqualTo(SafeGrowthInteractionState.Preconfirm));
            Assert.That(c.Interaction.IsApplying, Is.True);
            Assert.That(c.Service.TryPrepare(c.Observe()).Result,
                Is.EqualTo(SafeGrowthPrepareResult.AlreadyPrepared));
        }

        [Test]
        public void AbortPreparedRemovesReservationsAndRestoresOriginalState()
        {
            Context c = Context.Create();
            SafeGrowthPrepareReceipt prepared = c.Service.TryPrepare(c.Observe());
            Assert.That(c.Service.Abort(prepared.Token), Is.EqualTo(SafeGrowthTransactionResult.ResultFaulted));
            Assert.That(c.Ledger.Count, Is.Zero);
            Assert.That(c.Results.CommittedCount, Is.Zero);
            Assert.That(c.Interaction.State, Is.EqualTo(SafeGrowthInteractionState.Preconfirm));
            Assert.That(c.Interaction.IsApplying, Is.False);
            Assert.That(c.Service.TryPrepare(c.Observe()).Result, Is.EqualTo(SafeGrowthPrepareResult.Prepared));
        }

        [Test]
        public void FinalizePreparedIsIdempotent()
        {
            Context c = Context.Create();
            SafeGrowthPrepareToken token = c.Service.TryPrepare(c.Observe()).Token;
            SafeGrowthTransactionReceipt first = c.Service.Finalize(token);
            SafeGrowthTransactionReceipt second = c.Service.Finalize(token);
            Assert.That(first.Result, Is.EqualTo(SafeGrowthTransactionResult.Succeeded));
            Assert.That(second, Is.SameAs(first));
            Assert.That(c.Ledger.Count, Is.EqualTo(1));
            Assert.That(c.Results.CommittedCount, Is.EqualTo(1));
        }

        [Test]
        public void DeclineCommitsTerminalWithoutClaimingOptionalGrowth()
        {
            Context c = Context.Create(SafeGrowthTransactionIds.DeclineChoiceId);
            SafeGrowthTransactionReceipt receipt = c.Service.Execute(c.Decline());
            Assert.That(receipt.Result, Is.EqualTo(SafeGrowthTransactionResult.Declined));
            Assert.That(receipt.Opportunity, Is.Null);
            Assert.That(receipt.EventReceipt.Costs, Is.Empty);
            Assert.That(c.Ledger.Count, Is.Zero);
            Assert.That(c.Interaction.State, Is.EqualTo(SafeGrowthInteractionState.Declined));
        }

        [Test]
        public void SameKeyDeliveredOneHundredTimesHasOneEffect()
        {
            Context c = Context.Create();
            Assert.That(c.Service.Execute(c.Observe()).Result, Is.EqualTo(SafeGrowthTransactionResult.Succeeded));
            for (int i = 0; i < 99; i++)
                Assert.That(c.Service.Execute(c.Observe()).Result, Is.EqualTo(SafeGrowthTransactionResult.AlreadyResolved));
            Assert.That(c.Ledger.Count, Is.EqualTo(1));
            Assert.That(c.Results.CommittedCount, Is.EqualTo(1));
        }

        [Test]
        public void ReentrantBeginIsBusyAndDoesNotMutate()
        {
            Context c = Context.Create();
            Assert.That(c.Interaction.TryBeginApply(c.Token), Is.EqualTo(SafeGrowthInteractionResult.Changed));
            Assert.That(c.Service.Execute(c.Observe()).Result, Is.EqualTo(SafeGrowthTransactionResult.Busy));
            Assert.That(c.Ledger.Count, Is.Zero);
            Assert.That(c.Results.CommittedCount, Is.Zero);
        }

        [Test]
        public void CandidateZeroDoesNotEnterExecutorOrMutate()
        {
            Context c = Context.Create();
            Assert.That(c.Service.Execute(c.Observe(candidateCount: 0)).Result,
                Is.EqualTo(SafeGrowthTransactionResult.CandidateUnavailable));
            Assert.That(c.Interaction.IsApplying, Is.False);
            Assert.That(c.Interaction.State, Is.EqualTo(SafeGrowthInteractionState.Preconfirm));
            Assert.That(c.Ledger.Count, Is.Zero);
        }

        [Test]
        public void ExistingOptionalClaimRejectsBeforeExecutorMutation()
        {
            Context c = Context.Create();
            Assert.That(c.Ledger.TryEarn(c.Earn("existing"), out _), Is.EqualTo(ProgressionEarnResult.Earned));
            Assert.That(c.Service.Execute(c.Observe()).Result, Is.EqualTo(SafeGrowthTransactionResult.CapRejected));
            Assert.That(c.Ledger.Count, Is.EqualTo(1));
            Assert.That(c.Results.CommittedCount, Is.Zero);
            Assert.That(c.Interaction.State, Is.EqualTo(SafeGrowthInteractionState.Preconfirm));
            Assert.That(c.Interaction.IsApplying, Is.False);
        }

        [Test]
        public void EarnFaultRollsBackResultAndCanRetrySameReservation()
        {
            bool fault = true;
            Context c = Context.Create(ledgerObserver: point =>
            {
                if (fault && point == ProgressionLedgerMutationPoint.EarnCommitted) throw new InvalidOperationException();
            });
            Assert.That(c.Service.Execute(c.Observe()).Result, Is.EqualTo(SafeGrowthTransactionResult.LedgerFaulted));
            AssertOld(c);
            fault = false;
            Assert.That(c.Service.Execute(c.Observe()).Result, Is.EqualTo(SafeGrowthTransactionResult.Succeeded));
        }

        [Test]
        public void ResultCommitFaultRestoresEarnAndCanRetry()
        {
            bool fault = true;
            Context c = Context.Create(resultObserver: point =>
            {
                if (fault && point == StageEventResultMutationPoint.Committed) throw new InvalidOperationException();
            });
            Assert.That(c.Service.Execute(c.Observe()).Result, Is.EqualTo(SafeGrowthTransactionResult.ResultFaulted));
            AssertOld(c);
            fault = false;
            Assert.That(c.Service.Execute(c.Observe()).Result, Is.EqualTo(SafeGrowthTransactionResult.Succeeded));
        }

        [Test]
        public void ResultReservationFaultLeavesNoTerminalOrEntitlement()
        {
            Context c = Context.Create(resultObserver: point =>
            {
                if (point == StageEventResultMutationPoint.Reserved) throw new InvalidOperationException();
            });
            Assert.That(c.Service.Execute(c.Observe()).Result, Is.EqualTo(SafeGrowthTransactionResult.ResultFaulted));
            AssertOld(c);
        }

        [Test]
        public void RollbackFaultIsExplicitCompensationFault()
        {
            Context c = Context.Create(resultObserver: point =>
            {
                if (point == StageEventResultMutationPoint.Committed
                    || point == StageEventResultMutationPoint.RolledBack) throw new InvalidOperationException();
            });
            Assert.That(c.Service.Execute(c.Observe()).Result, Is.EqualTo(SafeGrowthTransactionResult.CompensationFaulted));
            Assert.That(c.Ledger.Count, Is.Zero);
            Assert.That(c.Interaction.State, Is.EqualTo(SafeGrowthInteractionState.ObserveSelectedPendingRetry));
        }

        [Test]
        public void TerminalFaultRestoresResultAndEarn()
        {
            Context c = Context.Create(interactionFault: true);
            Assert.That(c.Service.Execute(c.Observe()).Result, Is.EqualTo(SafeGrowthTransactionResult.ResultFaulted));
            AssertOld(c);
        }

        [Test]
        public void InvalidReservationAndWrongChoiceMutateNothing()
        {
            Context c = Context.Create();
            ProgressionRunId wrongRun = new("run.wrong");
            SafeGrowthInteractionOwnership wrong = new(); wrong.ResetForNewRun(wrongRun);
            SafeGrowthInteractionKey key = new(wrongRun.Value, "stage.1", SafeGrowthTransactionIds.ReservationId, "node.1");
            wrong.TryEnterPreconfirm(key, SafeGrowthTransactionIds.ObserveChoiceId, "fp", true, out var token);
            SafeGrowthTransactionCommand command = new(token, SafeGrowthTransactionChoice.Observe, c.Earn(), 2);
            Assert.That(c.Service.Execute(command).Result, Is.EqualTo(SafeGrowthTransactionResult.InvalidReservation));
            Assert.That(c.Ledger.Count, Is.Zero);
        }

        [Test]
        public void InteractionReentryRestoresTokenAndLocksDifferentChoice()
        {
            Context c = Context.Create();
            Assert.That(c.Interaction.TryEnterPreconfirm(c.Key, SafeGrowthTransactionIds.ObserveChoiceId,
                "definition.fp", true, out var restored), Is.EqualTo(SafeGrowthInteractionResult.Existing));
            Assert.That(restored.TokenId, Is.EqualTo(c.Token.TokenId));
            Assert.That(c.Interaction.TryEnterPreconfirm(c.Key, SafeGrowthTransactionIds.DeclineChoiceId,
                "definition.fp", true, out _), Is.EqualTo(SafeGrowthInteractionResult.Rejected));
        }

        private static void AssertOld(Context c)
        {
            Assert.That(c.Ledger.Count, Is.Zero);
            Assert.That(c.Results.CommittedCount, Is.Zero);
            Assert.That(c.Interaction.State, Is.EqualTo(SafeGrowthInteractionState.ObserveSelectedPendingRetry));
            Assert.That(c.Interaction.IsApplying, Is.False);
        }

        private sealed class Context
        {
            public ProgressionRunId RunId;
            public RunProgressionLedger Ledger;
            public StageEventResultLedger Results;
            public SafeGrowthInteractionOwnership Interaction;
            public SafeGrowthInteractionKey Key;
            public SafeGrowthInteractionToken Token;
            public SafeGrowthTransactionService Service;

            public static Context Create(string choice = null,
                Action<ProgressionLedgerMutationPoint> ledgerObserver = null,
                Action<StageEventResultMutationPoint> resultObserver = null,
                bool interactionFault = false)
            {
                Context c = new();
                c.RunId = new ProgressionRunId("run.safe.transaction");
                c.Ledger = new RunProgressionLedger(c.RunId, ProgressionCapPolicy.Chapter1P0,
                    new ProgressionSourceRegistry(), ledgerObserver);
                c.Results = resultObserver == null ? new StageEventResultLedger() : new StageEventResultLedger(resultObserver);
                c.Interaction = new SafeGrowthInteractionOwnership();
                c.Interaction.ResetForNewRun(c.RunId);
                c.Key = new SafeGrowthInteractionKey(c.RunId.Value, "stage-generation.safe.001",
                    SafeGrowthTransactionIds.ReservationId, "node-instance.safe.001");
                c.Interaction.TryEnterPreconfirm(c.Key, choice ?? SafeGrowthTransactionIds.ObserveChoiceId,
                    "definition.fp", true, out c.Token);
                ISafeGrowthInteractionGateway gateway = interactionFault
                    ? new TerminalFaultGateway(c.Interaction) : c.Interaction;
                c.Service = new SafeGrowthTransactionService(c.Ledger, c.Results, gateway);
                return c;
            }

            public ProgressionEarnRequest Earn(string result = null) => new(
                ProgressionSourceRegistry.OptionalRandomGrowthSegment,
                ProgressionSourceCategory.Random,
                ProgressionSourceType.RandomEventSafe,
                ProgressionSourceRegistry.RandomGrowthSafeSource,
                result ?? SafeGrowthTransactionIds.GrantedResultId);
            public SafeGrowthTransactionCommand Observe(int candidateCount = 2) =>
                new(Token, SafeGrowthTransactionChoice.Observe, Earn(), candidateCount);
            public SafeGrowthTransactionCommand Decline() =>
                new(Token, SafeGrowthTransactionChoice.Decline, null, 0);
        }

        private sealed class TerminalFaultGateway : ISafeGrowthInteractionGateway
        {
            private readonly SafeGrowthInteractionOwnership inner;
            public TerminalFaultGateway(SafeGrowthInteractionOwnership inner) => this.inner = inner;
            public SafeGrowthInteractionResult TryBeginApply(SafeGrowthInteractionToken token) => inner.TryBeginApply(token);
            public SafeGrowthInteractionResult TryMarkPendingRetry(SafeGrowthInteractionToken token) => inner.TryMarkPendingRetry(token);
            public SafeGrowthInteractionResult TryCommitTerminal(SafeGrowthInteractionToken token, SafeGrowthInteractionState state) =>
                SafeGrowthInteractionResult.Faulted;
        }
    }
}
