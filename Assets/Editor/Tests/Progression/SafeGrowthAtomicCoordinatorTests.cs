#if UNITY_EDITOR
using System;
using NUnit.Framework;
using Progression;
using Progression.RandomGrowth;
using Session;
using Stage;
using UnityEngine;

namespace Progression.Tests
{
    public sealed class SafeGrowthAtomicCoordinatorTests
    {
        [Test]
        public void NodePrepareIsMutationFreeAndRejectsStaleRevision()
        {
            using Fixture f = Fixture.Create();
            string before = StageAtomicNodeCompletionService.ComputeRevision(f.Graph);
            Assert.That(f.Completion.TryPrepareCompletion(f.Session, f.RunId.Value, f.StageId,
                f.Current.nodeId, f.Content.nodeId, before, out AtomicNodeCompletionToken token),
                Is.EqualTo(AtomicNodeCompletionResult.Prepared));
            Assert.That(StageAtomicNodeCompletionService.ComputeRevision(f.Graph), Is.EqualTo(before));
            Assert.That(f.Completion.TryPrepareCompletion(f.Session, f.RunId.Value, f.StageId,
                f.Current.nodeId, f.Content.nodeId, before, out _), Is.EqualTo(AtomicNodeCompletionResult.Busy));
            Assert.That(f.Completion.TryAbort(token), Is.EqualTo(AtomicNodeCompletionResult.RolledBack));
            Assert.That(f.Completion.TryPrepareCompletion(f.Session, f.RunId.Value, f.StageId,
                f.Current.nodeId, f.Content.nodeId, "stale", out _), Is.EqualTo(AtomicNodeCompletionResult.StaleRevision));
        }

        [Test]
        public void NodeCommitAndRollbackRestoreExactGraphAndRuntimeState()
        {
            using Fixture f = Fixture.Create();
            string before = StageAtomicNodeCompletionService.ComputeRevision(f.Graph);
            f.Completion.TryPrepareCompletion(f.Session, f.RunId.Value, f.StageId,
                f.Current.nodeId, f.Content.nodeId, before, out AtomicNodeCompletionToken token);
            Assert.That(f.Completion.TryCommit(token), Is.EqualTo(AtomicNodeCompletionResult.Committed));
            Assert.That(f.Current.IsCompleted, Is.True);
            Assert.That(f.Next.IsAvailable, Is.True);
            Assert.That(f.Completion.TryRollback(token), Is.EqualTo(AtomicNodeCompletionResult.RolledBack));
            Assert.That(StageAtomicNodeCompletionService.ComputeRevision(f.Graph), Is.EqualTo(before));
            Assert.That(f.Session.RuntimeData.currentNode, Is.SameAs(f.Current));
        }

        [Test]
        public void GraphApplyFaultRestoresAllAuthoritativeState()
        {
            using Fixture f = Fixture.Create(point =>
            {
                if (point == AtomicNodeCompletionMutationPoint.GraphApplied) throw new InvalidOperationException();
            });
            string before = StageAtomicNodeCompletionService.ComputeRevision(f.Graph);
            f.Completion.TryPrepareCompletion(f.Session, f.RunId.Value, f.StageId,
                f.Current.nodeId, f.Content.nodeId, before, out AtomicNodeCompletionToken token);
            Assert.That(f.Completion.TryCommit(token), Is.EqualTo(AtomicNodeCompletionResult.ApplyFaulted));
            Assert.That(StageAtomicNodeCompletionService.ComputeRevision(f.Graph), Is.EqualTo(before));
            Assert.That(f.Current.IsCompleted, Is.False);
        }

        [Test]
        public void CoordinatorSuccessCommitsGraphPendingAndTerminalExactlyOnce()
        {
            using Fixture f = Fixture.Create();
            Assert.That(f.Execute(out SafeGrowthTransactionReceipt receipt), Is.EqualTo(SafeGrowthAtomicExecutionResult.Succeeded));
            Assert.That(receipt.Result, Is.EqualTo(SafeGrowthTransactionResult.Succeeded));
            Assert.That(f.Ledger.Count, Is.EqualTo(1));
            Assert.That(f.Results.CommittedCount, Is.EqualTo(1));
            Assert.That(f.Current.IsCompleted, Is.True);
            Assert.That(f.Interaction.State, Is.EqualTo(SafeGrowthInteractionState.SafeGrowthGranted));
            Assert.That(f.Execute(out _), Is.EqualTo(SafeGrowthAtomicExecutionResult.TransactionPrepareFailed));
            Assert.That(f.Ledger.Count, Is.EqualTo(1));
        }

        [Test]
        public void TransactionFinalizeFaultRollsGraphAndDomainBack()
        {
            bool fault = true;
            using Fixture f = Fixture.Create(resultObserver: point =>
            {
                if (fault && point == StageEventResultMutationPoint.Committed) throw new InvalidOperationException();
            });
            string before = StageAtomicNodeCompletionService.ComputeRevision(f.Graph);
            Assert.That(f.Execute(out _), Is.EqualTo(SafeGrowthAtomicExecutionResult.TransactionFinalizeFailed));
            Assert.That(StageAtomicNodeCompletionService.ComputeRevision(f.Graph), Is.EqualTo(before));
            Assert.That(f.Ledger.Count, Is.Zero);
            Assert.That(f.Results.CommittedCount, Is.Zero);
            Assert.That(f.Interaction.State, Is.EqualTo(SafeGrowthInteractionState.ObserveSelectedPendingRetry));
            fault = false;
            Assert.That(f.Execute(out _), Is.EqualTo(SafeGrowthAtomicExecutionResult.Succeeded));
        }

        [Test]
        public void DeferredPublishFaultKeepsCommittedDomainAndPublishesOnce()
        {
            int publish = 0;
            using Fixture f = Fixture.Create(point =>
            {
                if (point == AtomicNodeCompletionMutationPoint.DeferredPublish)
                { publish++; throw new InvalidOperationException(); }
            });
            Assert.That(f.Execute(out _), Is.EqualTo(SafeGrowthAtomicExecutionResult.Succeeded));
            Assert.That(publish, Is.EqualTo(1));
            Assert.That(f.Current.IsCompleted, Is.True);
            Assert.That(f.Ledger.Count, Is.EqualTo(1));
        }

        [Test]
        public void WrongRunStageOrNodeIdentityCannotPrepare()
        {
            using Fixture f = Fixture.Create();
            string revision = StageAtomicNodeCompletionService.ComputeRevision(f.Graph);
            Assert.That(f.Completion.TryPrepareCompletion(f.Session, "wrong", f.StageId,
                f.Current.nodeId, f.Content.nodeId, revision, out _), Is.EqualTo(AtomicNodeCompletionResult.InvalidIdentity));
            Assert.That(f.Completion.TryPrepareCompletion(f.Session, f.RunId.Value, f.StageId,
                "wrong", f.Content.nodeId, revision, out _), Is.EqualTo(AtomicNodeCompletionResult.InvalidIdentity));
        }

        [Test]
        public void NodeOnlyFailureCoordinatorCommitsNodeAndTechnicalTerminal()
        {
            using Fixture f=Fixture.Create();
            Assert.That(f.ExecuteNodeOnlyFailure(null,out var receipt),Is.EqualTo(SafeGrowthNodeOnlyFailureExecutionResult.Succeeded));
            Assert.That(receipt.Result,Is.EqualTo(SafeGrowthNodeOnlyFailureResult.Committed));
            Assert.That(f.Current.IsCompleted,Is.True);
            Assert.That(f.Interaction.State,Is.EqualTo(SafeGrowthInteractionState.ContentUnavailable));
            Assert.That(f.Ledger.Count,Is.Zero);
        }

        [Test]
        public void NodeOnlyFailureNodePrepareFailureAbortsDomain()
        {
            using Fixture f=Fixture.Create();
            var bad=new SafeGrowthNodeOnlyFailureCause(f.RunId.Value,f.StageId,SafeGrowthTransactionIds.EventId,
                "wrong-node-id",SafeGrowthTransactionIds.ReservationId,f.Current.nodeId,SafeGrowthNodeOnlyFailureIds.ReceiptId);
            var service=new SafeGrowthNodeOnlyFailureService(f.Results,f.Interaction);
            Assert.That(new SafeGrowthNodeOnlyFailureCoordinator().Execute(service,bad,f.Interaction.Token,f.Completion,f.Session,
                StageAtomicNodeCompletionService.ComputeRevision(f.Graph),null,out _),Is.EqualTo(SafeGrowthNodeOnlyFailureExecutionResult.NodePrepareFailed));
            Assert.That(f.Current.IsCompleted,Is.False);Assert.That(f.Results.CommittedCount,Is.Zero);
        }

        [Test]
        public void NodeOnlyFailurePublishFaultKeepsAuthoritativeNewState()
        {
            using Fixture f=Fixture.Create();int publish=0;
            Assert.That(f.ExecuteNodeOnlyFailure(()=>{publish++;throw new InvalidOperationException();},out _),Is.EqualTo(SafeGrowthNodeOnlyFailureExecutionResult.Succeeded));
            Assert.That(publish,Is.EqualTo(1));Assert.That(f.Current.IsCompleted,Is.True);
            Assert.That(f.Interaction.State,Is.EqualTo(SafeGrowthInteractionState.ContentUnavailable));
        }

        private sealed class FixedIdentityFactory : IRandomGrowthSessionIdentityFactory
        {
            private readonly string stageId;
            public FixedIdentityFactory(string stageId) => this.stageId = stageId;
            public ProgressionRunId CreateRunId() => new("unused");
            public string CreateStageGenerationId(ProgressionRunId runId, string chapterId) => stageId;
        }

        private sealed class Fixture : IDisposable
        {
            public ProgressionRunId RunId;
            public string StageId;
            public StageSession Session;
            public StageGraph Graph;
            public RoundNode Current;
            public RoundNode Next;
            public RoundNodeSO Content;
            public RunProgressionLedger Ledger;
            public StageEventResultLedger Results;
            public SafeGrowthInteractionOwnership Interaction;
            public SafeGrowthTransactionService Transaction;
            public SafeGrowthTransactionCommand Command;
            public StageAtomicNodeCompletionService Completion;

            public static Fixture Create(Action<AtomicNodeCompletionMutationPoint> graphObserver = null,
                Action<StageEventResultMutationPoint> resultObserver = null)
            {
                Fixture f = new();
                f.RunId = new ProgressionRunId("run.atomic.safe");
                f.StageId = "stage-generation.atomic.safe";
                f.Content = ScriptableObject.CreateInstance<RoundNodeSO>();
                f.Content.nodeId = "node.act1.random_growth.02.windworn_sword_marks.intro";
                f.Current = new RoundNode("node-instance.safe", RoundNodeType.Event, 0, 0)
                { roundNodeSO = f.Content, state = RoundNodeState.Available, isSelected = true };
                f.Next = new RoundNode("node-instance.next", RoundNodeType.Event, 1, 0);
                f.Graph = new StageGraph("stage.chapter1", "Chapter1")
                { progressState = StageProgressState.InProgress, currentNodeId = f.Current.nodeId };
                f.Graph.AddNode(f.Current); f.Graph.AddNode(f.Next);
                f.Graph.ConnectNodes(f.Current.nodeId, f.Next.nodeId);
                f.Session = new StageSession();
                f.Session.Initialize(new StageRuntimeData { currentGraph = f.Graph, currentNode = f.Current });
                f.Session.ResetRandomGrowthForNewRun(f.RunId);
                f.Session.TryCommitChapter1RandomGrowthGraph(f.RunId, RandomGrowthSessionOwnership.Chapter1Id,
                    5, 5, new FixedIdentityFactory(f.StageId), out _);
                f.Ledger = new RunProgressionLedger(f.RunId, ProgressionCapPolicy.Chapter1P0, new ProgressionSourceRegistry());
                f.Results = resultObserver == null ? new StageEventResultLedger() : new StageEventResultLedger(resultObserver);
                f.Interaction = f.Session.SafeGrowthInteraction;
                SafeGrowthInteractionKey key = new(f.RunId.Value, f.StageId,
                    SafeGrowthTransactionIds.ReservationId, f.Current.nodeId);
                f.Interaction.TryEnterPreconfirm(key, SafeGrowthTransactionIds.ObserveChoiceId,
                    "definition.safe", true, out SafeGrowthInteractionToken token);
                f.Transaction = new SafeGrowthTransactionService(f.Ledger, f.Results, f.Interaction);
                ProgressionEarnRequest earn = new(ProgressionSourceRegistry.OptionalRandomGrowthSegment,
                    ProgressionSourceCategory.Random, ProgressionSourceType.RandomEventSafe,
                    ProgressionSourceRegistry.RandomGrowthSafeSource, SafeGrowthTransactionIds.GrantedResultId);
                f.Command = new SafeGrowthTransactionCommand(token, SafeGrowthTransactionChoice.Observe, earn, 2);
                f.Completion = new StageAtomicNodeCompletionService(graphObserver);
                return f;
            }

            public SafeGrowthAtomicExecutionResult Execute(out SafeGrowthTransactionReceipt receipt) =>
                new SafeGrowthAtomicCoordinator().Execute(Transaction, Command, Completion, Session,
                    RunId.Value, StageId, Current.nodeId, Content.nodeId,
                    StageAtomicNodeCompletionService.ComputeRevision(Graph), out receipt);

            public SafeGrowthNodeOnlyFailureExecutionResult ExecuteNodeOnlyFailure(Action publish,
                out SafeGrowthNodeOnlyFailureReceipt receipt)
            {
                var cause=new SafeGrowthNodeOnlyFailureCause(RunId.Value,StageId,SafeGrowthTransactionIds.EventId,
                    Content.nodeId,SafeGrowthTransactionIds.ReservationId,Current.nodeId,SafeGrowthNodeOnlyFailureIds.ReceiptId);
                return new SafeGrowthNodeOnlyFailureCoordinator().Execute(
                    new SafeGrowthNodeOnlyFailureService(Results,Interaction),cause,Interaction.Token,
                    Completion,Session,StageAtomicNodeCompletionService.ComputeRevision(Graph),publish,out receipt);
            }

            public void Dispose() => UnityEngine.Object.DestroyImmediate(Content);
        }
    }
}
#endif
