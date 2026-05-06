

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

            return node.nextNodeIds
                .Select(GetNode)
                .Where(x => x != null)
                .OrderBy(x => x.depth)
                .ThenBy(x => x.indexInDepth)
                .ToList();
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

            RoundNode startNode = StartNode;
            if (startNode == null)
            {
                Debug.LogWarning("[StageGraph] StartStage failed. Start node is missing.");
                return;
            }

            startNode.SetCleared();
            currentNodeId = startNode.nodeId;
            UnlockNextNodes(startNode);
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
            RoundNode currentNode = CurrentNode;
            if (currentNode == null)
            {
                Debug.LogWarning("[StageGraph] CompleteCurrentNode failed. Current node is missing.");
                return;
            }

            currentNode.SetCleared();

            if (currentNode.IsBossNode)
            {
                progressState = StageProgressState.Completed;
                return;
            }

            LockAllAvailableNodes();
            UnlockNextNodes(currentNode);
        }

        public void FailStage()
        {
            progressState = StageProgressState.Failed;
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