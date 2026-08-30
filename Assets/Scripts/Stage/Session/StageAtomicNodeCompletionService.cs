using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Progression;
using Session;

namespace Stage
{
    public enum AtomicNodeCompletionResult
    {
        Prepared = 0, Committed = 10, RolledBack = 20, AlreadyCommitted = 30,
        AlreadyPublished = 40, InvalidIdentity = 50, StaleRevision = 60,
        ApplyFaulted = 70, RollbackConflict = 80, Busy = 90
    }

    public enum AtomicNodeCompletionMutationPoint { GraphApplied = 0, DeferredPublish = 10 }

    public sealed class AtomicNodeCompletionToken
    {
        internal AtomicNodeCompletionToken(string tokenId, string runId, string stageGenerationId,
            string nodeInstanceId, string nodeId, string expectedRevision,
            StageGraph graph, IReadOnlyList<NodeBefore> nodes,
            StageProgressState progressState, string currentNodeId,
            StageRuntimeData sessionRuntime, RoundNode runtimeCurrentNode)
        {
            TokenId = tokenId; RunId = runId; StageGenerationId = stageGenerationId;
            NodeInstanceId = nodeInstanceId; NodeId = nodeId; ExpectedRevision = expectedRevision;
            Graph = graph; Nodes = nodes; ProgressState = progressState; CurrentNodeId = currentNodeId;
            SessionRuntime = sessionRuntime; RuntimeCurrentNode = runtimeCurrentNode;
        }
        public string TokenId { get; }
        public string RunId { get; }
        public string StageGenerationId { get; }
        public string NodeInstanceId { get; }
        public string NodeId { get; }
        public string ExpectedRevision { get; }
        internal StageGraph Graph { get; }
        internal IReadOnlyList<NodeBefore> Nodes { get; }
        internal StageProgressState ProgressState { get; }
        internal string CurrentNodeId { get; }
        internal StageRuntimeData SessionRuntime { get; }
        internal RoundNode RuntimeCurrentNode { get; }
        internal string AppliedRevision { get; set; }
        internal bool Committed { get; set; }
        internal bool Published { get; set; }
    }

    internal sealed class NodeBefore
    {
        public NodeBefore(RoundNode node)
        {
            Node = node; State = node.state; IsCleared = node.isCleared;
            IsSelected = node.isSelected; Resolved = node.resolved;
        }
        public RoundNode Node { get; }
        public RoundNodeState State { get; }
        public bool IsCleared { get; }
        public bool IsSelected { get; }
        public bool Resolved { get; }
    }

    public sealed class StageAtomicNodeCompletionService
    {
        private readonly Action<AtomicNodeCompletionMutationPoint> observer;
        private readonly Dictionary<StageGraph, AtomicNodeCompletionToken> active = new();
        public StageAtomicNodeCompletionService() { }
        public StageAtomicNodeCompletionService(Action<AtomicNodeCompletionMutationPoint> observer) =>
            this.observer = observer;

        public AtomicNodeCompletionResult TryPrepareCompletion(StageSession session,
            string runId, string stageGenerationId, string nodeInstanceId, string nodeId,
            string expectedRouteRevision, out AtomicNodeCompletionToken token)
        {
            token = null;
            StageGraph graph = session?.RuntimeData?.currentGraph;
            RoundNode current = graph?.CurrentNode;
            if (graph == null || current == null || string.IsNullOrWhiteSpace(runId)
                || string.IsNullOrWhiteSpace(stageGenerationId)
                || string.IsNullOrWhiteSpace(nodeInstanceId) || string.IsNullOrWhiteSpace(nodeId)
                || session.RandomGrowthSession?.RunId == null
                || !string.Equals(session.RandomGrowthSession.RunId.Value, runId, StringComparison.Ordinal)
                || !string.Equals(session.RandomGrowthSession.StageGenerationId, stageGenerationId, StringComparison.Ordinal)
                || current.IsCompleted
                || !string.Equals(current.nodeId, nodeInstanceId, StringComparison.Ordinal)
                || !string.Equals(current.roundNodeSO?.nodeId ?? current.templateNodeId, nodeId, StringComparison.Ordinal))
                return AtomicNodeCompletionResult.InvalidIdentity;

            if (active.ContainsKey(graph)) return AtomicNodeCompletionResult.Busy;

            string revision = ComputeRevision(graph);
            if (!string.IsNullOrWhiteSpace(expectedRouteRevision)
                && !string.Equals(expectedRouteRevision, revision, StringComparison.Ordinal))
                return AtomicNodeCompletionResult.StaleRevision;

            NodeBefore[] nodes = graph.nodes.Select(x => new NodeBefore(x)).ToArray();
            string tokenId = "node-completion-" + HashFields(new[]
            { runId, stageGenerationId, nodeInstanceId, nodeId, revision }).Substring(0, 32);
            token = new AtomicNodeCompletionToken(tokenId, runId, stageGenerationId,
                nodeInstanceId, nodeId, revision, graph, Array.AsReadOnly(nodes),
                graph.progressState, graph.currentNodeId, session.RuntimeData, session.RuntimeData.currentNode);
            active.Add(graph, token);
            return AtomicNodeCompletionResult.Prepared;
        }

        public AtomicNodeCompletionResult TryCommit(AtomicNodeCompletionToken token)
        {
            if (token == null || token.Graph == null) return AtomicNodeCompletionResult.InvalidIdentity;
            if (token.Committed) return AtomicNodeCompletionResult.AlreadyCommitted;
            if (!string.Equals(ComputeRevision(token.Graph), token.ExpectedRevision, StringComparison.Ordinal))
            {
                active.Remove(token.Graph);
                return AtomicNodeCompletionResult.StaleRevision;
            }
            try
            {
                if (!token.Graph.TryCompleteCurrentNode(token.NodeInstanceId))
                {
                    active.Remove(token.Graph);
                    return AtomicNodeCompletionResult.ApplyFaulted;
                }
                token.SessionRuntime.currentNode = token.Graph.CurrentNode;
                observer?.Invoke(AtomicNodeCompletionMutationPoint.GraphApplied);
                token.AppliedRevision = ComputeRevision(token.Graph);
                token.Committed = true;
                return AtomicNodeCompletionResult.Committed;
            }
            catch
            {
                Restore(token);
                active.Remove(token.Graph);
                return AtomicNodeCompletionResult.ApplyFaulted;
            }
        }

        public AtomicNodeCompletionResult TryAbort(AtomicNodeCompletionToken token)
        {
            if (token == null || token.Graph == null || token.Committed)
                return AtomicNodeCompletionResult.InvalidIdentity;
            return active.Remove(token.Graph)
                ? AtomicNodeCompletionResult.RolledBack
                : AtomicNodeCompletionResult.InvalidIdentity;
        }

        public AtomicNodeCompletionResult TryRollback(AtomicNodeCompletionToken token)
        {
            if (token == null || !token.Committed) return AtomicNodeCompletionResult.InvalidIdentity;
            if (!string.Equals(ComputeRevision(token.Graph), token.AppliedRevision, StringComparison.Ordinal))
                return AtomicNodeCompletionResult.RollbackConflict;
            Restore(token);
            token.Committed = false;
            token.AppliedRevision = string.Empty;
            active.Remove(token.Graph);
            return AtomicNodeCompletionResult.RolledBack;
        }

        public AtomicNodeCompletionResult PublishDeferred(AtomicNodeCompletionToken token,
            Action<RoundNode> nodeCompleted = null, Action<StageProgressState> progressChanged = null)
        {
            if (token == null || !token.Committed) return AtomicNodeCompletionResult.InvalidIdentity;
            if (token.Published) return AtomicNodeCompletionResult.AlreadyPublished;
            token.Published = true;
            try
            {
                observer?.Invoke(AtomicNodeCompletionMutationPoint.DeferredPublish);
                nodeCompleted?.Invoke(token.Graph.GetNode(token.NodeInstanceId));
                progressChanged?.Invoke(token.Graph.progressState);
            }
            catch
            {
                // Deferred notifications never roll back authoritative state.
            }
            return AtomicNodeCompletionResult.Committed;
        }

        public static string ComputeRevision(StageGraph graph)
        {
            if (graph == null) return string.Empty;
            List<string> fields = new() { graph.currentNodeId ?? string.Empty, ((int)graph.progressState).ToString() };
            foreach (RoundNode node in graph.nodes.OrderBy(x => x.nodeId, StringComparer.Ordinal))
            {
                fields.Add(node.nodeId ?? string.Empty); fields.Add(((int)node.state).ToString());
                fields.Add(node.isCleared ? "1" : "0"); fields.Add(node.isSelected ? "1" : "0");
                fields.Add(node.resolved ? "1" : "0");
            }
            return HashFields(fields);
        }

        private static string HashFields(IEnumerable<string> fields)
        {
            using MemoryStream stream = new();
            using (BinaryWriter writer = new(stream, new UTF8Encoding(false), true))
            {
                foreach (string field in fields)
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(field ?? string.Empty);
                    writer.Write(bytes.Length);
                    writer.Write(bytes);
                }
            }
            using SHA256 sha = SHA256.Create();
            return string.Concat(sha.ComputeHash(stream.ToArray()).Select(x => x.ToString("x2")));
        }

        private static void Restore(AtomicNodeCompletionToken token)
        {
            token.Graph.progressState = token.ProgressState;
            token.Graph.currentNodeId = token.CurrentNodeId;
            token.SessionRuntime.currentNode = token.RuntimeCurrentNode;
            foreach (NodeBefore before in token.Nodes)
            {
                before.Node.state = before.State; before.Node.isCleared = before.IsCleared;
                before.Node.isSelected = before.IsSelected; before.Node.resolved = before.Resolved;
            }
        }
    }

    public enum SafeGrowthAtomicExecutionResult
    {
        Succeeded = 0, TransactionPrepareFailed = 10, NodePrepareFailed = 20,
        NodeCommitFailed = 30, TransactionFinalizeFailed = 40, CompensationFaulted = 50
    }

    public sealed class SafeGrowthAtomicCoordinator
    {
        public SafeGrowthAtomicExecutionResult Execute(SafeGrowthTransactionService transaction,
            SafeGrowthTransactionCommand command, StageAtomicNodeCompletionService completion,
            StageSession session, string runId, string stageGenerationId,
            string nodeInstanceId, string nodeId, string expectedRouteRevision,
            out SafeGrowthTransactionReceipt receipt)
        {
            receipt = null;
            SafeGrowthPrepareReceipt prepared = transaction.TryPrepare(command);
            if (prepared.Result != SafeGrowthPrepareResult.Prepared)
                return SafeGrowthAtomicExecutionResult.TransactionPrepareFailed;
            if (completion.TryPrepareCompletion(session, runId, stageGenerationId,
                    nodeInstanceId, nodeId, expectedRouteRevision, out AtomicNodeCompletionToken nodeToken)
                != AtomicNodeCompletionResult.Prepared)
            {
                transaction.Abort(prepared.Token);
                return SafeGrowthAtomicExecutionResult.NodePrepareFailed;
            }
            if (completion.TryCommit(nodeToken) != AtomicNodeCompletionResult.Committed)
            {
                completion.TryAbort(nodeToken);
                transaction.Abort(prepared.Token);
                return SafeGrowthAtomicExecutionResult.NodeCommitFailed;
            }
            receipt = transaction.Finalize(prepared.Token);
            if (receipt.Result != SafeGrowthTransactionResult.Succeeded
                && receipt.Result != SafeGrowthTransactionResult.Declined)
            {
                bool graphClean = completion.TryRollback(nodeToken) == AtomicNodeCompletionResult.RolledBack;
                transaction.Abort(prepared.Token);
                return graphClean ? SafeGrowthAtomicExecutionResult.TransactionFinalizeFailed
                    : SafeGrowthAtomicExecutionResult.CompensationFaulted;
            }
            completion.PublishDeferred(nodeToken);
            return SafeGrowthAtomicExecutionResult.Succeeded;
        }
    }
}
