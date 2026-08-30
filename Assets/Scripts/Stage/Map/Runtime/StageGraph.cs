using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Stage
{
    /// <summary>
    /// 런타임에서 생성된 하나의 스테이지 맵 그래프 데이터.
    /// 노드 목록, 현재 노드, depth 기반 조회, 연결 정보를 관리한다.
    /// </summary>
    [Serializable]
    public class StageGraph
    {
        [Header("Identity")]
        public string stageId;
        public string stageName;

        [Header("Progress")]
        public StageProgressState progressState = StageProgressState.NotStarted;
        public string startNodeId;
        public string currentNodeId;
        public string bossNodeId;
        public int routeRevision;
        public string committedRouteSourceNodeId;
        public string committedRouteTargetNodeId;

        [Header("Nodes")]
        public List<RoundNode> nodes = new();

        public StageGraph()
        {
        }

        public StageGraph(string stageId, string stageName)
        {
            this.stageId = stageId;
            this.stageName = stageName;
        }

        public RoundNode CurrentNode => GetNode(currentNodeId);
        public RoundNode StartNode => GetNode(startNodeId);
        public RoundNode BossNode => GetNode(bossNodeId);

        public bool HasCurrentNode => !string.IsNullOrWhiteSpace(currentNodeId) && CurrentNode != null;
        public bool IsCompleted => progressState == StageProgressState.Completed;
        public bool IsFailed => progressState == StageProgressState.Failed;

        public void AddNode(RoundNode node)
        {
            if (node == null)
            {
                Debug.LogWarning("[StageGraph] Cannot add null node.");
                return;
            }

            if (string.IsNullOrWhiteSpace(node.nodeId))
            {
                Debug.LogWarning("[StageGraph] Cannot add node with empty nodeId.");
                return;
            }

            if (nodes.Any(x => x.nodeId == node.nodeId))
            {
                Debug.LogWarning($"[StageGraph] Duplicate nodeId ignored: {node.nodeId}");
                return;
            }

            nodes.Add(node);

            if (node.nodeType == RoundNodeType.Start)
            {
                startNodeId = node.nodeId;
            }
            else if (string.IsNullOrWhiteSpace(startNodeId)
                     || node.depth < StartNode.depth)
            {
                startNodeId = node.nodeId;
            }

            if (node.nodeType == RoundNodeType.Boss)
            {
                bossNodeId = node.nodeId;
            }
        }

        public RoundNode GetNode(string nodeId)
        {
            if (string.IsNullOrWhiteSpace(nodeId))
            {
                return null;
            }

            return nodes.FirstOrDefault(x => x.nodeId == nodeId);
        }

        public List<RoundNode> GetNodesByDepth(int depth)
        {
            return nodes
                .Where(x => x.depth == depth)
                .OrderBy(x => x.indexInDepth)
                .ToList();
        }

        public List<int> GetDepths()
        {
            return nodes
                .Select(x => x.depth)
                .Distinct()
                .OrderBy(x => x)
                .ToList();
        }

        public int GetMaxDepth()
        {
            if (nodes.Count == 0)
            {
                return 0;
            }

            return nodes.Max(x => x.depth);
        }

        public List<RoundNode> GetAvailableNodes()
        {
            return nodes
                .Where(x => x.IsAvailable)
                .OrderBy(x => x.depth)
                .ThenBy(x => x.indexInDepth)
                .ToList();
        }

        public List<RoundNode> GetNextNodes(RoundNode node)
        {
            if (node == null)
            {
                return new List<RoundNode>();
            }

            IEnumerable<string> nextIds = node.nextNodeIds;
            if (string.Equals(node.nodeId, committedRouteSourceNodeId, StringComparison.Ordinal)
                && !string.IsNullOrWhiteSpace(committedRouteTargetNodeId))
                nextIds = nextIds.Where(id => string.Equals(id,
                    committedRouteTargetNodeId, StringComparison.Ordinal));
            return nextIds
                .Select(GetNode)
                .Where(x => x != null)
                .OrderBy(x => x.depth)
                .ThenBy(x => x.indexInDepth)
                .ToList();
        }

        public bool TryCommitImmediateSuccessorRoute(
            RoundNode source,
            ImmediateSuccessorRouteSelectionMode mode,
            out StageRouteCommitSnapshot snapshot,
            out string error)
        {
            snapshot = new StageRouteCommitSnapshot(routeRevision,
                committedRouteSourceNodeId, committedRouteTargetNodeId);
            error = string.Empty;
            if (source == null || mode == ImmediateSuccessorRouteSelectionMode.None
                || !string.IsNullOrWhiteSpace(committedRouteTargetNodeId)
                || routeRevision == int.MaxValue)
            {
                error = "STAGE_ROUTE_COMMIT_STATE_INVALID";
                return false;
            }
            List<RoundNode> candidates = source.nextNodeIds.Select(GetNode)
                .Where(node => node != null).Distinct().ToList();
            if (candidates.Count < 2)
            {
                error = "STAGE_ROUTE_COMMIT_CANDIDATES_INSUFFICIENT";
                return false;
            }
            var ranked = candidates.Select(node => new
                {
                    Node = node,
                    Remaining = CountShortestRemainingNodes(node)
                })
                .Where(item => item.Remaining >= 0)
                .ToList();
            if (ranked.Count != candidates.Count)
            {
                error = "STAGE_ROUTE_COMMIT_EXIT_UNREACHABLE";
                return false;
            }
            if (mode == ImmediateSuccessorRouteSelectionMode.BattlePurposeThenShortestRemainingToSectionExit)
            {
                int battleCount = ranked.Count(item => string.Equals(
                    item.Node.LocalizationMainKey, "Battle", StringComparison.Ordinal));
                if (battleCount == 0 || battleCount == ranked.Count)
                {
                    error = "STAGE_ROUTE_BATTLE_PURPOSE_CANDIDATES_INVALID";
                    return false;
                }
                ranked = ranked.Where(item => string.Equals(item.Node.LocalizationMainKey,
                    "Battle", StringComparison.Ordinal)).ToList();
            }
            int selectedDistance = mode == ImmediateSuccessorRouteSelectionMode.LongestRemainingToSectionExit
                ? ranked.Max(item => item.Remaining)
                : ranked.Min(item => item.Remaining);
            RoundNode selected = ranked.Where(item => item.Remaining == selectedDistance)
                .Select(item => item.Node)
                .OrderBy(node => node.nodeId, StringComparer.Ordinal)
                .First();
            committedRouteSourceNodeId = source.nodeId;
            committedRouteTargetNodeId = selected.nodeId;
            routeRevision++;
            return true;
        }

        public bool TryCreateImmediateSuccessorRouteSnapshot(
            RoundNode source,
            string snapshotId,
            out StageRouteCandidateSnapshot snapshot,
            out string error)
        {
            snapshot = null;
            error = string.Empty;
            if (source == null || string.IsNullOrWhiteSpace(snapshotId))
            {
                error = "STAGE_ROUTE_SNAPSHOT_IDENTITY_INVALID";
                return false;
            }
            List<StageRouteCandidate> candidates = source.nextNodeIds.Select(GetNode)
                .Where(node => node != null)
                .Distinct()
                .Select(node => new StageRouteCandidate
                {
                    nodeId = node.nodeId,
                    purposeId = node.LocalizationMainKey,
                    remainingNodeCount = CountShortestRemainingNodes(node)
                })
                .OrderBy(item => item.nodeId, StringComparer.Ordinal)
                .ToList();
            if (candidates.Count < 2 || candidates.Any(item => item.remainingNodeCount < 0))
            {
                error = "STAGE_ROUTE_SNAPSHOT_CANDIDATES_INVALID";
                return false;
            }
            snapshot = new StageRouteCandidateSnapshot
            {
                snapshotId = snapshotId,
                sourceNodeId = source.nodeId,
                graphRevision = routeRevision,
                candidates = candidates
            };
            return true;
        }

        public bool TryCommitImmediateSuccessorRoute(
            StageRouteCandidateSnapshot candidateSnapshot,
            ImmediateSuccessorRouteSelectionMode mode,
            out StageRouteCommitSnapshot rollbackSnapshot,
            out string error)
        {
            rollbackSnapshot = new StageRouteCommitSnapshot(routeRevision,
                committedRouteSourceNodeId, committedRouteTargetNodeId);
            error = string.Empty;
            if (candidateSnapshot == null || candidateSnapshot.graphRevision != routeRevision
                || candidateSnapshot.candidates == null || candidateSnapshot.candidates.Count < 2
                || mode == ImmediateSuccessorRouteSelectionMode.None
                || !string.IsNullOrWhiteSpace(committedRouteTargetNodeId))
            {
                error = "STAGE_ROUTE_SNAPSHOT_STALE";
                return false;
            }
            RoundNode source = GetNode(candidateSnapshot.sourceNodeId);
            if (source == null || candidateSnapshot.candidates.Any(candidate =>
                    !source.nextNodeIds.Contains(candidate.nodeId)
                    || GetNode(candidate.nodeId) == null))
            {
                error = "STAGE_ROUTE_SNAPSHOT_GRAPH_MISMATCH";
                return false;
            }
            IReadOnlyList<StageRouteCandidate> selectable = candidateSnapshot.candidates;
            if (mode == ImmediateSuccessorRouteSelectionMode.BattlePurposeThenShortestRemainingToSectionExit)
            {
                int battleCount = candidateSnapshot.candidates.Count(item =>
                    string.Equals(item.purposeId, "Battle", StringComparison.Ordinal));
                if (battleCount == 0 || battleCount == candidateSnapshot.candidates.Count)
                {
                    error = "STAGE_ROUTE_BATTLE_PURPOSE_CANDIDATES_INVALID";
                    return false;
                }
                selectable = candidateSnapshot.candidates.Where(item =>
                    string.Equals(item.purposeId, "Battle", StringComparison.Ordinal)).ToArray();
            }
            int selectedDistance = mode == ImmediateSuccessorRouteSelectionMode.LongestRemainingToSectionExit
                ? selectable.Max(item => item.remainingNodeCount)
                : selectable.Min(item => item.remainingNodeCount);
            StageRouteCandidate selected = selectable
                .Where(item => item.remainingNodeCount == selectedDistance)
                .OrderBy(item => item.nodeId, StringComparer.Ordinal).First();
            committedRouteSourceNodeId = source.nodeId;
            committedRouteTargetNodeId = selected.nodeId;
            routeRevision++;
            return true;
        }

        public bool TryRollbackImmediateSuccessorRoute(StageRouteCommitSnapshot snapshot)
        {
            if (routeRevision != snapshot.Revision + 1) return false;
            routeRevision = snapshot.Revision;
            committedRouteSourceNodeId = snapshot.SourceNodeId;
            committedRouteTargetNodeId = snapshot.TargetNodeId;
            return true;
        }

        private int CountShortestRemainingNodes(RoundNode start)
        {
            var queue = new Queue<(RoundNode node, int count)>();
            var visited = new HashSet<string>(StringComparer.Ordinal);
            queue.Enqueue((start, 0));
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (!visited.Add(current.node.nodeId)) continue;
                List<RoundNode> next = current.node.nextNodeIds.Select(GetNode)
                    .Where(node => node != null).ToList();
                if (next.Count == 0 || current.node.IsBossNode) return current.count;
                foreach (RoundNode node in next.OrderBy(node => node.nodeId, StringComparer.Ordinal))
                    queue.Enqueue((node, current.count + 1));
            }
            return -1;
        }

        public List<RoundNode> GetPrevNodes(RoundNode node)
        {
            if (node == null)
            {
                return new List<RoundNode>();
            }

            return node.prevNodeIds
                .Select(GetNode)
                .Where(x => x != null)
                .OrderBy(x => x.depth)
                .ThenBy(x => x.indexInDepth)
                .ToList();
        }

        public void ConnectNodes(string fromNodeId, string toNodeId)
        {
            RoundNode fromNode = GetNode(fromNodeId);
            RoundNode toNode = GetNode(toNodeId);

            if (fromNode == null || toNode == null)
            {
                Debug.LogWarning($"[StageGraph] Failed to connect nodes. from={fromNodeId}, to={toNodeId}");
                return;
            }

            fromNode.AddNextNode(toNodeId);
            toNode.AddPrevNode(fromNodeId);
        }

        public void StartStage()
        {
            progressState = StageProgressState.InProgress;

            foreach (RoundNode node in nodes)
            {
                node.SetLocked();
                node.SetSelected(false);
            }

            List<RoundNode> firstDepthNodes = GetNodesByDepth(0);
            if (firstDepthNodes.Count == 0)
            {
                Debug.LogWarning("[StageGraph] StartStage failed. First depth node is missing.");
                return;
            }

            foreach (RoundNode node in firstDepthNodes)
            {
                node.SetAvailable();
            }

            RoundNode firstNode = firstDepthNodes[0];
            startNodeId = firstNode.nodeId;
            currentNodeId = firstNode.nodeId;
        }

        public bool SelectNode(string nodeId)
        {
            RoundNode node = GetNode(nodeId);
            if (node == null)
            {
                Debug.LogWarning($"[StageGraph] SelectNode failed. nodeId={nodeId}");
                return false;
            }

            if (!node.CanExecute())
            {
                Debug.LogWarning($"[StageGraph] Node cannot execute. nodeId={nodeId}, state={node.state}");
                return false;
            }

            foreach (RoundNode graphNode in nodes)
            {
                graphNode.SetSelected(false);
            }

            node.SetSelected(true);
            currentNodeId = node.nodeId;
            return true;
        }

        public void CompleteCurrentNode()
        {
            TryCompleteCurrentNode(currentNodeId);
        }

        public bool TryCompleteCurrentNode(
            string expectedNodeId)
        {
            RoundNode currentNode = CurrentNode;
            if (currentNode == null)
            {
                Debug.LogWarning("[StageGraph] CompleteCurrentNode failed. Current node is missing.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(expectedNodeId)
                || !string.Equals(
                    currentNode.nodeId,
                    expectedNodeId,
                    StringComparison.Ordinal))
            {
                Debug.LogWarning(
                    "[StageGraph] CompleteCurrentNode rejected. "
                    + $"expected={expectedNodeId}, "
                    + $"current={currentNode.nodeId}.");
                return false;
            }

            if (currentNode.IsCompleted)
            {
                return true;
            }

            currentNode.SetCleared();
            Debug.Log(
                $"[StageGraph] Complete node={currentNode.nodeId}, nextCount={currentNode.nextNodeIds.Count}, next={string.Join(",", currentNode.nextNodeIds)}");

            if (currentNode.IsBossNode)
            {
                progressState = StageProgressState.Completed;
                return true;
            }

            LockAvailableNodesAtDepth(currentNode.depth);
            UnlockNextNodes(currentNode);
            return true;
        }

        public void FailStage()
        {
            progressState = StageProgressState.Failed;
        }

        private void LockAvailableNodesAtDepth(int depth)
        {
            foreach (RoundNode node in nodes)
            {
                if (node.depth != depth)
                {
                    continue;
                }

                if (node.IsAvailable)
                {
                    node.SetLocked();
                }
            }
        }

        private void UnlockNextNodes(RoundNode fromNode)
        {
            List<RoundNode> nextNodes = GetNextNodes(fromNode);
            foreach (RoundNode nextNode in nextNodes)
            {
                nextNode.SetAvailable();
            }
        }

        private void LockAllAvailableNodes()
        {
            foreach (RoundNode node in nodes)
            {
                if (node.IsAvailable)
                {
                    node.SetLocked();
                }
            }
        }
    }
}
