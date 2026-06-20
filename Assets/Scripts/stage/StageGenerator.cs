using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Stage
{
    /// <summary>
    /// StageDefinitionSO를 기반으로 런타임 StageGraph를 생성한다.
    /// 핵심 노드는 StageDefinitionSO.requiredSubEvents의 순서/배치 정보로 배치하고,
    /// StageSegmentRule은 핵심 노드 사이에 weight 기반 랜덤 미니 그래프를 생성한다.
    /// </summary>
    public class StageGenerator
    {
        private StageDefinitionSO definition;
        private System.Random fixedRandom;

        private readonly Dictionary<string, RoundNode> fixedNodeByPosition = new();

        public StageGraph Generate(StageDefinitionSO stageDefinition)
        {
            if (stageDefinition == null)
            {
                Debug.LogError("[StageGenerator] StageDefinitionSO is null.");
                return null;
            }

            definition = stageDefinition;
            fixedRandom = definition.useFixedSeed ? new System.Random(definition.seed) : null;
            fixedNodeByPosition.Clear();

            StageGraph graph = new StageGraph(definition.stageId, definition.stageName);

            List<RoundNode> fixedNodes = CreateFixedNodes();
            foreach (RoundNode node in fixedNodes)
            {
                graph.AddNode(node);
            }

            HashSet<string> segmentedDepthConnections = GenerateSegments(graph);
            ConnectFixedNodes(graph, fixedNodes, segmentedDepthConnections);

            graph.StartStage();
            return graph;
        }

        private List<RoundNode> CreateFixedNodes()
        {
            List<StageRequiredNode> sources = definition.requiredSubEvents == null
                ? new List<StageRequiredNode>()
                : definition.requiredSubEvents
                    .Where(x => x != null && x.node != null)
                    .OrderBy(x => x.depth)
                    .ThenBy(x => x.column)
                    .ToList();

            List<RoundNode> result = new();

            foreach (StageRequiredNode source in sources)
            {
                int depth = Mathf.Max(0, source.depth);
                int column = Mathf.Max(0, source.column);
                RoundNode node = CreateNodeFromSO(source.node, depth, column, $"fixed_{depth}_{column}");

                node.hiddenByDefault = source.hiddenByDefault;

                if (node.hiddenByDefault)
                {
                    //node.Hide();
                }

                result.Add(node);
                fixedNodeByPosition[MakeFixedPositionKey(depth, column)] = node;
                Debug.Log($"[StageGenerator] Fixed node registered. node={node.nodeId}, depth={depth}, column={column}, key={MakeFixedPositionKey(depth, column)}");
            }

            return result;
        }

        private HashSet<string> GenerateSegments(StageGraph graph)
        {
            HashSet<string> segmentedConnections = new(StringComparer.OrdinalIgnoreCase);

            if (definition.segmentRules == null || definition.segmentRules.Count == 0)
            {
                return segmentedConnections;
            }

            for (int i = 0; i < definition.segmentRules.Count; i++)
            {
                StageSegmentRule rule = definition.segmentRules[i];
                if (rule == null)
                {
                    continue;
                }

                string fromKey = MakeFixedPositionKey(rule.fromDepth, rule.fromColumn);

                if (!fixedNodeByPosition.TryGetValue(fromKey, out RoundNode fromNode))
                {
                    Debug.LogWarning($"[StageGenerator] Segment skipped. Missing from fixed node. index={i}, from={fromKey}");
                    continue;
                }

                List<RoundNode> toNodes = ResolveSegmentTargetNodes(rule);
                if (toNodes.Count == 0)
                {
                    Debug.LogWarning($"[StageGenerator] Segment skipped. Missing to fixed nodes. index={i}, toDepth={rule.toDepth}, toColumn={rule.toColumn}");
                    continue;
                }

                segmentedConnections.Add(MakeDepthConnectionKey(rule.fromDepth, rule.toDepth));
                GenerateSegmentGraph(graph, rule, i, fromNode, toNodes);
            }

            return segmentedConnections;
        }

        private List<RoundNode> ResolveSegmentTargetNodes(StageSegmentRule rule)
        {
            if (rule == null)
            {
                return new List<RoundNode>();
            }

            int targetDepth = Mathf.Max(0, rule.toDepth);
            List<RoundNode> depthTargets = fixedNodeByPosition
                .Values
                .Where(x => x != null && x.depth == targetDepth)
                .OrderBy(x => x.indexInDepth)
                .ToList();

            if (depthTargets.Count > 0)
            {
                Debug.Log(
                    $"[StageGenerator] Segment targets resolved by depth. " +
                    $"toDepth={targetDepth}, toColumn={rule.toColumn}, " +
                    $"count={depthTargets.Count}, " +
                    $"targets={string.Join(",", depthTargets.Select(x => x.nodeId))}");

                return depthTargets;
            }

            if (rule.toColumn >= 0)
            {
                string toKey = MakeFixedPositionKey(rule.toDepth, rule.toColumn);
                if (fixedNodeByPosition.TryGetValue(toKey, out RoundNode toNode))
                {
                    Debug.Log(
                        $"[StageGenerator] Segment target resolved by exact key. " +
                        $"key={toKey}, target={toNode.nodeId}");

                    return new List<RoundNode> { toNode };
                }
            }

            Debug.LogWarning(
                $"[StageGenerator] Segment target resolve failed. " +
                $"toDepth={rule.toDepth}, toColumn={rule.toColumn}, " +
                $"fixedNodes={string.Join(",", fixedNodeByPosition.Values.Select(x => x.nodeId + "@" + x.depth + ":" + x.indexInDepth))}");

            return new List<RoundNode>();
        }

        private void GenerateSegmentGraph(
            StageGraph graph,
            StageSegmentRule rule,
            int segmentIndex,
            RoundNode fromNode,
            List<RoundNode> toNodes)
        {
            RoundNode primaryToNode = toNodes != null && toNodes.Count > 0
                ? toNodes[0]
                : null;

            int targetDepth = primaryToNode != null
                ? primaryToNode.depth
                : Mathf.Max(0, rule.toDepth);

            int totalWeight = GetRandomRangeInclusive(
                Mathf.Min(rule.minTotalWeight, rule.maxTotalWeight),
                Mathf.Max(rule.minTotalWeight, rule.maxTotalWeight));

            int layerCount = GetRandomRangeInclusive(
                Mathf.Max(1, Mathf.Min(rule.minLayerCount, rule.maxLayerCount)),
                Mathf.Max(1, Mathf.Max(rule.minLayerCount, rule.maxLayerCount)));

            List<int> layerWeights = SplitWeightIntoLayers(
                totalWeight,
                layerCount,
                Mathf.Max(1, rule.minLayerWeight),
                Mathf.Max(1, rule.maxLayerWeight));

            List<List<RoundNode>> layers = new();
            Dictionary<EventPoolSO, int> poolUseCounts = new();

            for (int layerIndex = 0; layerIndex < layerWeights.Count; layerIndex++)
            {
                int layerWeight = layerWeights[layerIndex];
                int nodeCount = GetRandomRangeInclusive(
                    Mathf.Max(1, Mathf.Min(rule.minNodesPerLayer, rule.maxNodesPerLayer)),
                    Mathf.Max(1, Mathf.Max(rule.minNodesPerLayer, rule.maxNodesPerLayer)));

                List<int> nodeWeights = SplitWeightIntoNodes(layerWeight, nodeCount);
                List<RoundNode> layerNodes = new();

                for (int nodeIndex = 0; nodeIndex < nodeWeights.Count; nodeIndex++)
                {
                    int generatedDepth = fromNode.depth + layerIndex + 1;
                    int generatedColumn = CalculateSegmentColumn(
                        fromNode,
                        primaryToNode,
                        segmentIndex,
                        nodeIndex);

                    RoundNode node = CreateSegmentPoolNode(
                        rule,
                        poolUseCounts,
                        segmentIndex,
                        layerIndex,
                        nodeIndex,
                        generatedDepth,
                        generatedColumn,
                        nodeWeights[nodeIndex]);

                    graph.AddNode(node);
                    layerNodes.Add(node);
                }

                layers.Add(layerNodes);
            }

            if (layers.Count == 0)
            {
                List<RoundNode> directToNodes = ResolveSegmentTailTargetNodes(targetDepth, toNodes, 1);
                ConnectLayerNodes(graph, new List<RoundNode> { fromNode }, directToNodes);
                return;
            }

            ConnectLayerNodes(graph, new List<RoundNode> { fromNode }, layers[0]);

            for (int layerIndex = 0; layerIndex < layers.Count - 1; layerIndex++)
            {
                ConnectLayerNodes(graph, layers[layerIndex], layers[layerIndex + 1]);
            }

            List<RoundNode> tailToNodes = ResolveSegmentTailTargetNodes(targetDepth, toNodes, layers[^1].Count);
            Debug.Log(
                $"[StageGenerator] Connect segment tail. " +
                $"segment={segmentIndex}, tailCount={layers[^1].Count}, " +
                $"toCount={tailToNodes.Count}, to={string.Join(",", tailToNodes.Select(x => x.nodeId))}");
            ConnectLayerNodes(graph, layers[^1], tailToNodes);
        }

        private List<RoundNode> ResolveSegmentTailTargetNodes(
            int targetDepth,
            List<RoundNode> defaultTargets,
            int tailNodeCount)
        {
            if (tailNodeCount == 1)
            {
                List<RoundNode> depthTargets = GetFixedNodesByDepth(targetDepth);
                if (depthTargets.Count > 0)
                {
                    Debug.Log(
                        $"[StageGenerator] Single tail node fan-out. " +
                        $"targetDepth={targetDepth}, count={depthTargets.Count}, " +
                        $"targets={string.Join(",", depthTargets.Select(x => x.nodeId))}");

                    return depthTargets;
                }
            }

            return defaultTargets ?? new List<RoundNode>();
        }

        private List<RoundNode> GetFixedNodesByDepth(int depth)
        {
            int targetDepth = Mathf.Max(0, depth);
            return fixedNodeByPosition
                .Values
                .Where(x => x != null && x.depth == targetDepth)
                .OrderBy(x => x.indexInDepth)
                .ToList();
        }

        private List<int> SplitWeightIntoLayers(
            int totalWeight,
            int layerCount,
            int minLayerWeight,
            int maxLayerWeight)
        {
            List<int> result = new();
            int remaining = Mathf.Max(1, totalWeight);

            for (int i = 0; i < layerCount; i++)
            {
                int remainingLayers = layerCount - i;
                if (remainingLayers <= 1)
                {
                    result.Add(Mathf.Max(1, remaining));
                    break;
                }

                int minForRest = remainingLayers - 1;
                int maxAllowed = Mathf.Min(maxLayerWeight, remaining - minForRest);
                int minAllowed = Mathf.Min(Mathf.Max(1, minLayerWeight), maxAllowed);

                int weight = GetRandomRangeInclusive(minAllowed, maxAllowed);
                result.Add(weight);
                remaining -= weight;
            }

            return result;
        }

        private List<int> SplitWeightIntoNodes(int layerWeight, int nodeCount)
        {
            List<int> result = new();
            int remaining = Mathf.Max(1, layerWeight);
            int count = Mathf.Max(1, nodeCount);

            for (int i = 0; i < count; i++)
            {
                int remainingNodes = count - i;
                if (remainingNodes <= 1)
                {
                    result.Add(Mathf.Max(1, remaining));
                    break;
                }

                int maxAllowed = Mathf.Max(1, remaining - (remainingNodes - 1));
                int weight = GetRandomRangeInclusive(1, maxAllowed);
                result.Add(weight);
                remaining -= weight;
            }

            return result;
        }

        private void ConnectFixedNodes(
            StageGraph graph,
            List<RoundNode> fixedNodes,
            HashSet<string> segmentedDepthConnections)
        {
            Dictionary<int, List<RoundNode>> nodesByDepth = fixedNodes
                .GroupBy(x => x.depth)
                .ToDictionary(
                    x => x.Key,
                    x => x.OrderBy(n => n.indexInDepth).ToList());

            List<int> depths = nodesByDepth.Keys
                .OrderBy(x => x)
                .ToList();

            for (int i = 0; i < depths.Count - 1; i++)
            {
                if (segmentedDepthConnections.Contains(MakeDepthConnectionKey(depths[i], depths[i + 1])))
                {
                    continue;
                }

                List<RoundNode> currentDepthNodes = nodesByDepth[depths[i]];
                List<RoundNode> nextDepthNodes = nodesByDepth[depths[i + 1]];

                foreach (RoundNode currentNode in currentDepthNodes)
                {
                    foreach (RoundNode nextNode in nextDepthNodes)
                    {
                        graph.ConnectNodes(currentNode.nodeId, nextNode.nodeId);
                    }
                }
            }
        }

        private void ConnectLayerNodes(
            StageGraph graph,
            List<RoundNode> currentNodes,
            List<RoundNode> nextNodes)
        {
            if (currentNodes == null || currentNodes.Count == 0)
            {
                return;
            }

            if (nextNodes == null || nextNodes.Count == 0)
            {
                return;
            }

            if (currentNodes.Count == 1)
            {
                foreach (RoundNode nextNode in nextNodes)
                {
                    graph.ConnectNodes(currentNodes[0].nodeId, nextNode.nodeId);
                }

                return;
            }

            if (nextNodes.Count == 1)
            {
                foreach (RoundNode currentNode in currentNodes)
                {
                    graph.ConnectNodes(currentNode.nodeId, nextNodes[0].nodeId);
                }

                return;
            }

            for (int i = 0; i < currentNodes.Count; i++)
            {
                RoundNode currentNode = currentNodes[i];
                RoundNode primaryNextNode = nextNodes[Mathf.Clamp(i, 0, nextNodes.Count - 1)];
                graph.ConnectNodes(currentNode.nodeId, primaryNextNode.nodeId);

                if (i + 1 < nextNodes.Count)
                {
                    graph.ConnectNodes(currentNode.nodeId, nextNodes[i + 1].nodeId);
                }
            }
        }

        private int CalculateSegmentColumn(
            RoundNode fromNode,
            RoundNode toNode,
            int segmentIndex,
            int nodeIndex)
        {
            int branchColumn;

            if (toNode != null)
            {
                branchColumn = Mathf.Max(0, toNode.indexInDepth);
            }
            else if (fromNode != null)
            {
                branchColumn = Mathf.Max(0, fromNode.indexInDepth);
            }
            else
            {
                branchColumn = Mathf.Max(0, segmentIndex);
            }

            return branchColumn * 1000 + nodeIndex;
        }

        private RoundNode CreateNodeFromSO(RoundNodeSO source, int depth, int indexInDepth, string fallbackId)
        {
            string nodeId = string.IsNullOrWhiteSpace(source.nodeId)
                ? fallbackId
                : source.nodeId;

            RoundNode node = new RoundNode(
                nodeId,
                source.nodeType,
                depth,
                indexInDepth)
            {
                roundNodeSO = source,
                popupEvent = source.popupEvent,
                isRequired = source.isRequired,
                icon = LibraryManager.Instance.GetNodeTypeIcon(source.nodeType)
            };

            return node;
        }

        private RoundNode CreateSegmentPoolNode(
            StageSegmentRule rule,
            Dictionary<EventPoolSO, int> poolUseCounts,
            int segmentIndex,
            int layerIndex,
            int nodeIndex,
            int depth,
            int indexInDepth,
            int weight)
        {
            EventPoolSO selectedPool = PickRandomPool(rule, poolUseCounts);
            string nodeId = $"segment_{segmentIndex}_layer_{layerIndex}_node_{nodeIndex}_w{weight}";

            if (selectedPool == null)
            {
                Debug.LogWarning($"[StageGenerator] Segment pool is missing. Fallback battle node created. nodeId={nodeId}");
                return new RoundNode(
                    nodeId,
                    RoundNodeType.Battle,
                    depth,
                    indexInDepth);
            }

            RegisterPoolUse(poolUseCounts, selectedPool);

            EventPoolEntry selectedEntry = PickRandomEntry(selectedPool);
            if (selectedEntry == null || selectedEntry.node == null)
            {
                Debug.LogWarning($"[StageGenerator] Segment pool has no valid entry. Fallback event node created. pool={selectedPool.name}, nodeId={nodeId}");
                return new RoundNode(
                    nodeId,
                    RoundNodeType.Event,
                    depth,
                    indexInDepth);
            }

            RoundNode node = CreateNodeFromSO(
                selectedEntry.node,
                depth,
                indexInDepth,
                nodeId);

            node.useRandomEventPool = false;
            node.randomPool = selectedPool;
            node.resolved = true;

            return node;
        }

        private EventPoolEntry PickRandomEntry(EventPoolSO pool)
        {
            if (pool == null || pool.entries == null || pool.entries.Count == 0)
            {
                return null;
            }

            List<EventPoolEntry> validEntries = pool.entries
                .Where(x => x != null && x.node != null)
                .Where(x => IsRoundNodeConditionSatisfied(x.node))
                .ToList();

            if (validEntries.Count == 0)
            {
                return null;
            }

            int totalWeight = validEntries.Sum(x => Mathf.Max(1, x.weight));
            int roll = GetRandomRangeInclusive(1, totalWeight);
            int current = 0;

            foreach (EventPoolEntry entry in validEntries)
            {
                current += Mathf.Max(1, entry.weight);
                if (roll <= current)
                {
                    return entry;
                }
            }

            return validEntries[0];
        }

        private EventPoolSO PickRandomPool(
            StageSegmentRule rule,
            Dictionary<EventPoolSO, int> poolUseCounts)
        {
            if (rule == null || rule.pools == null || rule.pools.Count == 0)
            {
                return null;
            }

            List<StageSegmentPoolRule> validRules = rule.pools
                .Where(x => x != null && x.pool != null)
                .Where(x => CanUsePool(x, poolUseCounts))
                .Where(x => HasSatisfiedCandidate(x.pool))
                .ToList();

            if (validRules.Count == 0)
            {
                return null;
            }

            List<StageSegmentPoolRule> requiredRules = validRules
                .Where(x => x.required && GetPoolUseCount(poolUseCounts, x.pool) == 0)
                .OrderBy(x => x.priority)
                .ToList();

            if (requiredRules.Count > 0)
            {
                int samePriorityCount = requiredRules.Count(x => x.priority == requiredRules[0].priority);
                int index = GetRandomRangeInclusive(0, samePriorityCount - 1);
                return requiredRules[index].pool;
            }

            int randomIndex = GetRandomRangeInclusive(0, validRules.Count - 1);
            return validRules[randomIndex].pool;
        }

        private bool HasSatisfiedCandidate(EventPoolSO pool)
        {
            if (pool == null || pool.entries == null || pool.entries.Count == 0)
            {
                return false;
            }

            return pool.entries
                .Where(x => x != null && x.node != null)
                .Any(x => IsRoundNodeConditionSatisfied(x.node));
        }

        private bool IsRoundNodeConditionSatisfied(RoundNodeSO node)
        {
            if (node == null)
            {
                return false;
            }

            if (node.appearanceConditions == null || node.appearanceConditions.Count == 0)
            {
                return true;
            }

            foreach (RoundNodeCondition condition in node.appearanceConditions)
            {
                if (!IsConditionSatisfied(condition))
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsConditionSatisfied(RoundNodeCondition condition)
        {
            if (condition == null || condition.conditionType == RoundNodeConditionType.None)
            {
                return true;
            }

            bool satisfied = condition.conditionType switch
            {
                RoundNodeConditionType.HasCharacter => HasCharacter(condition.targetId),
                RoundNodeConditionType.HasEquipment => HasEquipment(condition.targetId),
                RoundNodeConditionType.HasRelic => HasRelic(condition.targetId),
                RoundNodeConditionType.HasItem => HasItem(condition.targetId),
                RoundNodeConditionType.HasFaith => HasFaith(condition.targetId),
                RoundNodeConditionType.HasBless => HasBless(condition.targetId),
                _ => true
            };

            return condition.invert ? !satisfied : satisfied;
        }

        private bool HasCharacter(string targetId)
        {
            if (string.IsNullOrWhiteSpace(targetId))
            {
                return false;
            }

            // TODO: Party/Player 데이터 구조 확정 후 실제 보유 캐릭터 체크로 연결.
            return false;
        }

        private bool HasEquipment(string targetId)
        {
            if (string.IsNullOrWhiteSpace(targetId))
            {
                return false;
            }

            // TODO: EquipmentManager/Inventory 데이터 구조 확정 후 실제 장비 보유 체크로 연결.
            return false;
        }

        private bool HasRelic(string targetId)
        {
            if (string.IsNullOrWhiteSpace(targetId))
            {
                return false;
            }

            // TODO: RelicManager 데이터 구조 확정 후 실제 유물 보유 체크로 연결.
            return false;
        }

        private bool HasItem(string targetId)
        {
            if (string.IsNullOrWhiteSpace(targetId))
            {
                return false;
            }

            // TODO: ItemManager/Inventory 데이터 구조 확정 후 실제 아이템 보유 체크로 연결.
            return false;
        }

        private bool HasFaith(string targetId)
        {
            if (string.IsNullOrWhiteSpace(targetId))
            {
                return false;
            }

            // TODO: Faith/Bless 데이터 구조 확정 후 실제 신앙 보유 체크로 연결.
            return false;
        }

        private bool HasBless(string targetId)
        {
            if (string.IsNullOrWhiteSpace(targetId))
            {
                return false;
            }

            // TODO: BlessManager 데이터 구조 확정 후 실제 축복 보유 체크로 연결.
            return false;
        }

        private bool CanUsePool(
            StageSegmentPoolRule poolRule,
            Dictionary<EventPoolSO, int> poolUseCounts)
        {
            if (poolRule == null || poolRule.pool == null)
            {
                return false;
            }

            if (poolRule.maxAppearCount <= 0)
            {
                return true;
            }

            return GetPoolUseCount(poolUseCounts, poolRule.pool) < poolRule.maxAppearCount;
        }

        private int GetPoolUseCount(
            Dictionary<EventPoolSO, int> poolUseCounts,
            EventPoolSO pool)
        {
            if (poolUseCounts == null || pool == null)
            {
                return 0;
            }

            return poolUseCounts.TryGetValue(pool, out int count)
                ? count
                : 0;
        }

        private void RegisterPoolUse(
            Dictionary<EventPoolSO, int> poolUseCounts,
            EventPoolSO pool)
        {
            if (poolUseCounts == null || pool == null)
            {
                return;
            }

            poolUseCounts[pool] = GetPoolUseCount(poolUseCounts, pool) + 1;
        }

        private string MakeFixedPositionKey(int depth, int column)
        {
            return $"{Mathf.Max(0, depth)}:{Mathf.Max(0, column)}";
        }

        private string MakeDepthConnectionKey(int fromDepth, int toDepth)
        {
            return $"{Mathf.Max(0, fromDepth)}->{Mathf.Max(0, toDepth)}";
        }

        private int GetRandomRangeInclusive(int minInclusive, int maxInclusive)
        {
            if (maxInclusive < minInclusive)
            {
                return minInclusive;
            }

            if (fixedRandom != null)
            {
                return fixedRandom.Next(minInclusive, maxInclusive + 1);
            }

            return Random.Range(minInclusive, maxInclusive + 1);
        }
    }
}