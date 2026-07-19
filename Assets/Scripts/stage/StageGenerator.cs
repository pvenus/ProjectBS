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
        private readonly HashSet<string> generatedNodeIds = new(StringComparer.OrdinalIgnoreCase);

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
            generatedNodeIds.Clear();

            StageGraph graph = new StageGraph(definition.stageId, definition.stageName);

            List<RoundNode> fixedNodes = CreateFixedNodes();
            foreach (RoundNode node in fixedNodes)
            {
                graph.AddNode(node);
            }

            List<RoundNode> fixedNodesSnapshot = new List<RoundNode>(fixedNodes);

            Debug.Log($"[StageGenerator] Generation mode: {definition.generationMode}");

            switch (definition.generationMode)
            {
                case StageMapGenerationMode.RouteKeyRandomRules:
                    if (definition.randomRouteRules == null || definition.randomRouteRules.Count == 0)
                    {
                        Debug.LogWarning("[StageGenerator] RouteKeyRandomRules mode selected, but randomRouteRules is empty. Legacy segmentRules will not be used automatically.");
                    }
                    if (definition.segmentRules != null && definition.segmentRules.Count > 0)
                    {
                        Debug.Log("[StageGenerator] RouteKeyRandomRules mode selected. segmentRules are preserved as legacy data and ignored.");
                    }

                    GenerateRandomRouteNodes(definition, graph, fixedNodesSnapshot);
                    NormalizeIndexInDepth(graph);
                    ConnectAdjacentDepthNodes(graph);
                    break;

                case StageMapGenerationMode.LegacySegmentRules:
                    if (definition.randomRouteRules != null && definition.randomRouteRules.Count > 0)
                    {
                        Debug.Log("[StageGenerator] LegacySegmentRules mode selected. randomRouteRules are ignored.");
                    }

                    Debug.LogWarning("[StageGenerator] LegacySegmentRules mode selected, but legacy GenerateSegments is not active or not compatible with runtime nodeId. Generated fixed nodes only.");
                    NormalizeIndexInDepth(graph);
                    break;

                case StageMapGenerationMode.FixedOnly:
                    NormalizeIndexInDepth(graph);
                    ConnectAdjacentDepthNodes(graph);
                    break;
            }

            ValidateGeneratedGraph(graph);
            PrintGeneratedNodesSummary(graph);

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
            Dictionary<string, int> fixedOrdinalCounters = new(StringComparer.OrdinalIgnoreCase);

            foreach (StageRequiredNode source in sources)
            {
                int depth = Mathf.Max(0, source.depth);
                int column = Mathf.Max(0, source.column);

                // routeKey 결정 — ResolveRouteKey()로 fallback 처리
                string resolvedRouteKey = ResolveRouteKey(source);

                // GlobalHubNode는 routeKey empty 허용
                // RouteNode / RouteHubNode는 fallback 후에도 비어 있으면 경고
                if (source.kind != StageNodeKind.GlobalHubNode
                    && string.IsNullOrWhiteSpace(resolvedRouteKey))
                {
                    Debug.LogWarning(
                        $"[StageGenerator] RequiredNode has empty routeKey after fallback. " +
                        $"kind={source.kind}, depth={depth}, column={column}, node={source.node.name}");
                }

                // 결정론적 ordinal 계산 (depth_routeKey 별 독립 카운터)
                string counterKey = $"{depth}_{resolvedRouteKey}";
                if (!fixedOrdinalCounters.TryGetValue(counterKey, out int ordinal))
                {
                    ordinal = 0;
                }
                fixedOrdinalCounters[counterKey] = ordinal + 1;

                string templateId = source.node.nodeId;
                string runtimeNodeId = CreateFixedRuntimeNodeId(depth, resolvedRouteKey, ordinal, templateId);

                RoundNode node = CreateNodeFromSO(
                    source.node,
                    depth,
                    column,
                    runtimeNodeId,
                    resolvedRouteKey,
                    source.kind);

                node.hiddenByDefault = source.hiddenByDefault;

                if (node.hiddenByDefault)
                {
                    //node.Hide();
                }

                result.Add(node);
                fixedNodeByPosition[MakeFixedPositionKey(depth, column)] = node;

                Debug.Log(
                    $"[StageGenerator] Fixed node registered. " +
                    $"nodeId={node.nodeId}, template={source.node.name}, " +
                    $"kind={node.kind}, depth={node.depth}, " +
                    $"indexInDepth={node.indexInDepth}, routeKey={node.routeKey}");
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

        /// <summary>
        /// RoundNodeSO로부터 런타임 RoundNode를 생성한다.
        /// routeKey와 kind를 함께 지정한다.
        /// </summary>
        private RoundNode CreateNodeFromSO(
            RoundNodeSO source,
            int depth,
            int indexInDepth,
            string nodeId,
            string routeKey,
            StageNodeKind kind = StageNodeKind.RouteNode)
        {
            RoundNode node = new RoundNode(
                nodeId,
                source.nodeType,
                depth,
                indexInDepth,
                routeKey,
                kind)
            {
                templateNodeId = source.nodeId,
                roundNodeSO = source,
                popupEvent = source.popupEvent,
                isRequired = source.isRequired,
                icon = LibraryManager.Instance.GetNodeTypeIcon(source.nodeType)
            };

            return node;
        }

        /// <summary>
        /// 기존 랜덤 세그먼트 노드 생성 경로에서 사용하는 오버로드.
        /// routeKey는 indexInDepth.ToString()으로 임시 처리하고, kind는 RouteNode로 고정한다.
        /// (임시 호환 처리 — 랜덤 세그먼트 활성화 시 별도 routeKey 설계 필요)
        /// </summary>
        private RoundNode CreateNodeFromSO(RoundNodeSO source, int depth, int indexInDepth, string nodeId)
        {
            return CreateNodeFromSO(
                source,
                depth,
                indexInDepth,
                nodeId,
                routeKey: indexInDepth.ToString(),
                kind: StageNodeKind.RouteNode);
        }

        private static string GetSafeRouteKey(string routeKey)
        {
            if (string.IsNullOrEmpty(routeKey))
            {
                return "empty";
            }
            return routeKey.Replace('.', '_');
        }

        private string CreateFixedRuntimeNodeId(int depth, string routeKey, int ordinal, string templateId)
        {
            string safeRoute = GetSafeRouteKey(routeKey);
            string baseId = $"rt_fixed_d{depth}_k{safeRoute}_i{ordinal}_{templateId}";

            string finalId = baseId;
            int dupCounter = 1;
            while (generatedNodeIds.Contains(finalId))
            {
                finalId = $"{baseId}_dup{dupCounter}";
                dupCounter++;
            }

            generatedNodeIds.Add(finalId);
            return finalId;
        }

        private string CreateRandomRuntimeNodeId(int ruleIndex, int depth, string routeKey, int branchIndex, int ordinal, string templateId)
        {
            string safeRoute = GetSafeRouteKey(routeKey);
            string baseId = $"rt_random_g{ruleIndex}_d{depth}_k{safeRoute}_b{branchIndex}_n{ordinal}_{templateId}";

            string finalId = baseId;
            int dupCounter = 1;
            while (generatedNodeIds.Contains(finalId))
            {
                finalId = $"{baseId}_dup{dupCounter}";
                dupCounter++;
            }

            generatedNodeIds.Add(finalId);
            return finalId;
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

        private void GenerateRandomRouteNodes(StageDefinitionSO stageDefinition, StageGraph graph, List<RoundNode> fixedNodesSnapshot)
        {
            if (stageDefinition.randomRouteRules == null || stageDefinition.randomRouteRules.Count == 0)
            {
                return;
            }

            for (int ruleIdx = 0; ruleIdx < stageDefinition.randomRouteRules.Count; ruleIdx++)
            {
                StageRandomRouteRule rule = stageDefinition.randomRouteRules[ruleIdx];
                if (rule == null) continue;

                // 1. depth 검증
                if (rule.endDepth <= rule.startDepth + 1)
                {
                    Debug.LogWarning($"[StageGenerator] RandomRouteRule {ruleIdx} skipped: no generation depths. startDepth={rule.startDepth}, endDepth={rule.endDepth}");
                    continue;
                }

                // 2. pools 검증
                if (rule.pools == null || rule.pools.Count == 0)
                {
                    Debug.LogWarning($"[StageGenerator] RandomRouteRule {ruleIdx} skipped: pools is empty.");
                    continue;
                }

                // 3. base routeKey 수집
                List<string> baseRouteKeys = CollectBaseRouteKeys(rule, fixedNodesSnapshot);
                if (baseRouteKeys.Count == 0)
                {
                    Debug.LogWarning($"[StageGenerator] RandomRouteRule {ruleIdx} skipped: no base routeKeys found.");
                    continue;
                }

                int generatedNodeCount = 0;

                // 4. base routeKey 마다 분기 및 생성
                foreach (string baseRouteKey in baseRouteKeys)
                {
                    // branchCount 결정 (결정론적 난수)
                    int minB = Mathf.Max(1, rule.minBranchesPerRoute);
                    int maxB = Mathf.Max(minB, rule.maxBranchesPerRoute);
                    int branchCount = GetRandomRangeInclusive(minB, maxB);

                    for (int branchIdx = 0; branchIdx < branchCount; branchIdx++)
                    {
                        // branch routeKey 형식: {baseRouteKey}.g{ruleIndex}.b{branchIndex}
                        string branchRouteKey = $"{baseRouteKey}.g{ruleIdx}.b{branchIdx}";

                        // 5. startDepth + 1 ~ endDepth - 1 까지 모든 depth에 생성
                        Dictionary<EventPoolSO, int> poolUseCounts = new Dictionary<EventPoolSO, int>();
                        int ordinal = branchIdx; // ID 중복 방지 보조 카운터

                        for (int depth = rule.startDepth + 1; depth < rule.endDepth; depth++)
                        {
                            // pool에서 RoundNodeSO 추출
                            EventPoolSO selectedPool = PickRandomPoolForRule(rule, poolUseCounts);
                            if (selectedPool == null)
                            {
                                Debug.LogWarning($"[StageGenerator] RandomRouteRule {ruleIdx} depth {depth} skipped: no valid pool found.");
                                continue;
                            }

                            RegisterPoolUse(poolUseCounts, selectedPool);

                            EventPoolEntry selectedEntry = PickRandomEntry(selectedPool);
                            if (selectedEntry == null || selectedEntry.node == null)
                            {
                                Debug.LogWarning($"[StageGenerator] RandomRouteRule {ruleIdx} depth {depth} skipped: pool '{selectedPool.name}' has no satisfied entries.");
                                continue;
                            }

                            // runtime nodeId 생성
                            string templateId = selectedEntry.node.nodeId;
                            // ID 중복 방지 및 기획 정합성 준수를 위해 baseRouteKey 전달 (ID key 중복 해결)
                            string runtimeNodeId = CreateRandomRuntimeNodeId(ruleIdx, depth, baseRouteKey, branchIdx, ordinal, templateId);

                            // UI 겹침 방지 임시 indexInDepth 결정
                            int tempIndexInDepth = graph.GetNodesByDepth(depth).Count;

                            // 1차 구현에서는 무조건 RouteNode
                            RoundNode node = CreateNodeFromSO(
                                selectedEntry.node,
                                depth,
                                tempIndexInDepth,
                                runtimeNodeId,
                                branchRouteKey,
                                StageNodeKind.RouteNode);

                            node.useRandomEventPool = false;
                            node.randomPool = selectedPool;
                            node.resolved = true;

                            graph.AddNode(node);
                            generatedNodeCount++;
                        }
                    }
                }

                if (generatedNodeCount > 0)
                {
                    Debug.Log($"[StageGenerator] RandomRouteRule {ruleIdx} generated {generatedNodeCount} nodes from {baseRouteKeys.Count} base routes.");
                }
            }
        }

        private List<string> CollectBaseRouteKeys(StageRandomRouteRule rule, List<RoundNode> fixedNodes)
        {
            List<string> collected = new List<string>();

            // 1순위: startDepth
            var startNodes = fixedNodes.Where(x => x.depth == rule.startDepth).ToList();
            foreach (var node in startNodes)
            {
                if (node.kind == StageNodeKind.GlobalHubNode) continue;
                AddSeedKeysFromNode(node, rule, fixedNodes, collected);
            }

            if (collected.Count > 0)
            {
                return collected.Distinct().ToList();
            }

            // 2순위: endDepth
            var endNodes = fixedNodes.Where(x => x.depth == rule.endDepth).ToList();
            foreach (var node in endNodes)
            {
                if (node.kind == StageNodeKind.GlobalHubNode) continue;
                AddSeedKeysFromNode(node, rule, fixedNodes, collected);
            }

            if (collected.Count > 0)
            {
                return collected.Distinct().ToList();
            }

            // 3순위: 전체 fixed node
            foreach (var node in fixedNodes)
            {
                if (node.kind == StageNodeKind.GlobalHubNode) continue;
                AddSeedKeysFromNode(node, rule, fixedNodes, collected);
            }

            return collected.Distinct().ToList();
        }

        private void AddSeedKeysFromNode(RoundNode node, StageRandomRouteRule rule, List<RoundNode> fixedNodes, List<string> collected)
        {
            if (node == null || string.IsNullOrWhiteSpace(node.routeKey)) return;

            if (node.kind == StageNodeKind.RouteHubNode)
            {
                List<string> descendants = new List<string>();

                foreach (var other in fixedNodes)
                {
                    if (other.kind == StageNodeKind.GlobalHubNode) continue;
                    if (string.IsNullOrWhiteSpace(other.routeKey)) continue;

                    if (other.routeKey != node.routeKey && IsSameOrDescendantRouteKey(node.routeKey, other.routeKey))
                    {
                        descendants.Add(other.routeKey);
                    }
                }

                if (descendants.Count > 0)
                {
                    collected.AddRange(descendants);
                }
                else
                {
                    collected.Add(node.routeKey);
                }
            }
            else
            {
                collected.Add(node.routeKey);
            }
        }

        private bool IsSameOrDescendantRouteKey(string parentRouteKey, string childRouteKey)
        {
            if (string.IsNullOrEmpty(parentRouteKey) || string.IsNullOrEmpty(childRouteKey))
            {
                return false;
            }
            return childRouteKey == parentRouteKey || childRouteKey.StartsWith(parentRouteKey + ".");
        }

        private EventPoolSO PickRandomPoolForRule(StageRandomRouteRule rule, Dictionary<EventPoolSO, int> poolUseCounts)
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

        private void NormalizeIndexInDepth(StageGraph graph)
        {
            if (graph == null || graph.nodes == null) return;

            List<int> depths = graph.GetDepths();
            foreach (int depth in depths)
            {
                List<RoundNode> depthNodes = graph.nodes.Where(x => x.depth == depth).ToList();
                if (depthNodes.Count == 0) continue;

                depthNodes.Sort((a, b) =>
                {
                    // 1. routeKey 자연 정렬
                    int routeCompare = CompareRouteKey(a.routeKey, b.routeKey);
                    if (routeCompare != 0) return routeCompare;

                    // 2. fixed node 우선 배치
                    bool aFixed = a.nodeId.StartsWith("rt_fixed_");
                    bool bFixed = b.nodeId.StartsWith("rt_fixed_");
                    if (aFixed != bFixed)
                    {
                        return aFixed ? -1 : 1;
                    }

                    // 3. runtime nodeId 사전식 정렬 (최종 타이브레이커)
                    return string.Compare(a.nodeId, b.nodeId, StringComparison.OrdinalIgnoreCase);
                });

                for (int i = 0; i < depthNodes.Count; i++)
                {
                    depthNodes[i].indexInDepth = i;
                }
            }
        }

        private static List<int> ParseRouteKey(string routeKey)
        {
            List<int> segments = new List<int>();
            if (string.IsNullOrEmpty(routeKey))
            {
                return segments;
            }

            string[] tokens = routeKey.Split('.');
            foreach (string token in tokens)
            {
                if (int.TryParse(token, out int val))
                {
                    segments.Add(val);
                }
                else
                {
                    segments.Add(0);
                }
            }

            return segments;
        }

        private static int CompareRouteKey(string x, string y)
        {
            if (x == y) return 0;
            
            bool xEmpty = string.IsNullOrWhiteSpace(x);
            bool yEmpty = string.IsNullOrWhiteSpace(y);
            if (xEmpty && yEmpty) return 0;
            if (xEmpty) return 1;
            if (yEmpty) return -1;

            List<int> xSegments = ParseRouteKey(x);
            List<int> ySegments = ParseRouteKey(y);

            int minLength = Mathf.Min(xSegments.Count, ySegments.Count);
            for (int i = 0; i < minLength; i++)
            {
                if (xSegments[i] != ySegments[i])
                {
                    return xSegments[i].CompareTo(ySegments[i]);
                }
            }

            return xSegments.Count.CompareTo(ySegments.Count);
        }

        private void ConnectAdjacentDepthNodes(StageGraph graph)
        {
            if (graph == null)
            {
                Debug.LogWarning("[StageGenerator] ConnectAdjacentDepthNodes skipped: graph is null.");
                return;
            }

            if (graph.nodes == null || graph.nodes.Count == 0)
            {
                return;
            }

            // 1. depth별 그룹화
            Dictionary<int, List<RoundNode>> nodesByDepth = new Dictionary<int, List<RoundNode>>();
            foreach (RoundNode node in graph.nodes)
            {
                if (node == null) continue;
                if (!nodesByDepth.ContainsKey(node.depth))
                {
                    nodesByDepth[node.depth] = new List<RoundNode>();
                }
                nodesByDepth[node.depth].Add(node);
            }

            // 2. depth 오름차순 정렬
            List<int> sortedDepths = nodesByDepth.Keys.OrderBy(d => d).ToList();

            int edgeCount = 0;

            // 3. 인접 depth (d -> d + 1) 만 연결 검사
            for (int i = 0; i < sortedDepths.Count - 1; i++)
            {
                int currentDepth = sortedDepths[i];
                int nextDepth = sortedDepths[i + 1];

                // depth 차이가 정확히 1인 경우만 인접 연결 검사 (depth skip 방지)
                if (nextDepth != currentDepth + 1)
                {
                    continue;
                }

                List<RoundNode> currentNodes = nodesByDepth[currentDepth];
                List<RoundNode> nextNodes = nodesByDepth[nextDepth];

                foreach (RoundNode fromNode in currentNodes)
                {
                    if (string.IsNullOrWhiteSpace(fromNode.nodeId))
                    {
                        Debug.LogWarning($"[StageGenerator] ConnectAdjacentDepthNodes warning: node has empty runtime nodeId. depth={fromNode.depth}, routeKey={fromNode.routeKey}");
                        continue;
                    }

                    foreach (RoundNode toNode in nextNodes)
                    {
                        if (string.IsNullOrWhiteSpace(toNode.nodeId))
                        {
                            Debug.LogWarning($"[StageGenerator] ConnectAdjacentDepthNodes warning: node has empty runtime nodeId. depth={toNode.depth}, routeKey={toNode.routeKey}");
                            continue;
                        }

                        // CanConnectByRouteKey 조건 만족 시 연결
                        if (CanConnectByRouteKey(fromNode, toNode))
                        {
                            graph.ConnectNodes(fromNode.nodeId, toNode.nodeId);
                            edgeCount++;
                        }
                    }
                }
            }

            Debug.Log($"[StageGenerator] Connected adjacent depth nodes. edges={edgeCount}");
        }

        private bool CanConnectByRouteKey(RoundNode from, RoundNode to)
        {
            if (from == null || to == null) return false;

            // 1. GlobalHubNode 연결 규칙 (GlobalHub는 모든 노드와 연결 가능)
            if (from.kind == StageNodeKind.GlobalHubNode || to.kind == StageNodeKind.GlobalHubNode)
            {
                return true;
            }

            // 2. RouteHubNode 연결 규칙 (RouteHub는 자신의 routeKey 계열 하위 경로와 연결 가능)
            if (from.kind == StageNodeKind.RouteHubNode)
            {
                return IsSameOrDescendantRouteKey(from.routeKey, to.routeKey);
            }

            if (to.kind == StageNodeKind.RouteHubNode)
            {
                return IsSameOrDescendantRouteKey(to.routeKey, from.routeKey);
            }

            // 3. RouteNode 연결 규칙 (일치 또는 조상-자손 관계)
            if (from.kind == StageNodeKind.RouteNode && to.kind == StageNodeKind.RouteNode)
            {
                return IsSameOrDescendantRouteKey(from.routeKey, to.routeKey)
                    || IsSameOrDescendantRouteKey(to.routeKey, from.routeKey);
            }

            return false;
        }

        private void ValidateGeneratedGraph(StageGraph graph)
        {
            if (graph == null)
            {
                Debug.LogWarning("[StageGenerator] Graph validation skipped: graph is null.");
                return;
            }

            if (graph.nodes == null || graph.nodes.Count == 0)
            {
                Debug.LogWarning("[StageGenerator] Graph validation warning: graph has no nodes.");
                Debug.Log("[StageGenerator] Graph validation completed. nodes=0, edges=0, warnings=1");
                return;
            }

            int warningCount = 0;

            // 1. 노드 룩업 빌드
            Dictionary<string, RoundNode> lookup = new Dictionary<string, RoundNode>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> duplicateIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (RoundNode node in graph.nodes)
            {
                if (node == null) continue;

                // 2. runtime nodeId 누락 검증
                if (string.IsNullOrWhiteSpace(node.nodeId))
                {
                    Debug.LogWarning($"[StageGenerator] Graph validation warning: node has empty runtime nodeId. depth={node.depth}, routeKey={node.routeKey}, templateNodeId={node.templateNodeId}");
                    warningCount++;
                    continue;
                }

                // 3. 중복 nodeId 검증
                if (lookup.ContainsKey(node.nodeId))
                {
                    if (!duplicateIds.Contains(node.nodeId))
                    {
                        Debug.LogWarning($"[StageGenerator] Graph validation warning: duplicate nodeId found. nodeId={node.nodeId}");
                        warningCount++;
                        duplicateIds.Add(node.nodeId);
                    }
                }
                else
                {
                    lookup[node.nodeId] = node;
                }
            }

            // min/max depth 계산
            int minDepth = int.MaxValue;
            int maxDepth = int.MinValue;
            foreach (var node in lookup.Values)
            {
                if (node.depth < minDepth) minDepth = node.depth;
                if (node.depth > maxDepth) maxDepth = node.depth;
            }

            int minDepthCount = 0;
            int maxDepthCount = 0;

            // 엣지 수 카운팅 용도
            HashSet<string> countedEdges = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var node in lookup.Values)
            {
                if (node.depth == minDepth) minDepthCount++;
                if (node.depth == maxDepth) maxDepthCount++;

                // 4. 존재하지 않는 nodeId 참조 검증
                foreach (string nextId in node.nextNodeIds)
                {
                    if (string.IsNullOrWhiteSpace(nextId)) continue;

                    string edgeKey = $"{node.nodeId}->{nextId}";
                    countedEdges.Add(edgeKey);

                    if (!lookup.TryGetValue(nextId, out RoundNode nextNode))
                    {
                        Debug.LogWarning($"[StageGenerator] Graph validation warning: missing next node reference. from={node.nodeId}, missingNext={nextId}");
                        warningCount++;
                        continue;
                    }

                    // 5. edge 양방향 정합성 검증
                    if (!nextNode.prevNodeIds.Contains(node.nodeId))
                    {
                        Debug.LogWarning($"[StageGenerator] Graph validation warning: edge mismatch. from.next contains to, but to.prev does not contain from. from={node.nodeId}, to={nextId}");
                        warningCount++;
                    }

                    // 6. depth skip edge 검증
                    if (nextNode.depth != node.depth + 1)
                    {
                        Debug.LogWarning($"[StageGenerator] Graph validation warning: invalid depth edge. fromDepth={node.depth}, toDepth={nextNode.depth}, from={node.nodeId}, to={nextId}");
                        warningCount++;
                    }

                    // 7. routeKey 연결 규칙 검증
                    if (!CanConnectByRouteKey(node, nextNode))
                    {
                        Debug.LogWarning($"[StageGenerator] Graph validation warning: invalid route edge. fromRoute={node.routeKey}, toRoute={nextNode.routeKey}, fromKind={node.kind}, toKind={nextNode.kind}");
                        warningCount++;
                    }
                }

                foreach (string prevId in node.prevNodeIds)
                {
                    if (string.IsNullOrWhiteSpace(prevId)) continue;

                    if (!lookup.TryGetValue(prevId, out RoundNode prevNode))
                    {
                        Debug.LogWarning($"[StageGenerator] Graph validation warning: missing prev node reference. node={node.nodeId}, missingPrev={prevId}");
                        warningCount++;
                        continue;
                    }

                    // 5. edge 양방향 정합성 검증 (역방향)
                    if (!prevNode.nextNodeIds.Contains(node.nodeId))
                    {
                        Debug.LogWarning($"[StageGenerator] Graph validation warning: edge mismatch. node.prev contains prev, but prev.next does not contain node. node={node.nodeId}, prev={prevId}");
                        warningCount++;
                    }
                }

                // 8. 고립 노드 검증
                if (node.depth > minDepth)
                {
                    if (node.prevNodeIds.Count == 0)
                    {
                        Debug.LogWarning($"[StageGenerator] Graph validation warning: isolated incoming node. nodeId={node.nodeId}, depth={node.depth}, routeKey={node.routeKey}");
                        warningCount++;
                    }
                }

                if (node.depth < maxDepth)
                {
                    if (node.nextNodeIds.Count == 0)
                    {
                        Debug.LogWarning($"[StageGenerator] Graph validation warning: isolated outgoing node. nodeId={node.nodeId}, depth={node.depth}, routeKey={node.routeKey}");
                        warningCount++;
                    }
                }
            }

            // 9. 시작/종료 depth 검증
            if (minDepthCount == 0)
            {
                Debug.LogWarning($"[StageGenerator] Graph validation warning: no nodes found at minimum depth {minDepth}.");
                warningCount++;
            }
            if (maxDepthCount == 0)
            {
                Debug.LogWarning($"[StageGenerator] Graph validation warning: no nodes found at maximum depth {maxDepth}.");
                warningCount++;
            }

            int totalEdges = countedEdges.Count;
            Debug.Log($"[StageGenerator] Graph validation completed. nodes={lookup.Count}, edges={totalEdges}, warnings={warningCount}");
        }

        private void PrintGeneratedNodesSummary(StageGraph graph)
        {
            if (graph == null || graph.nodes == null) return;

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("\n[StageGenerator] ===== Generated Nodes Specification =====");
            
            List<int> depths = graph.GetDepths();
            foreach (int depth in depths)
            {
                var nodes = graph.nodes.Where(x => x.depth == depth).OrderBy(x => x.indexInDepth).ToList();
                sb.AppendLine($"Depth {depth} (Count: {nodes.Count})");
                foreach (var node in nodes)
                {
                    string isFixed = node.nodeId.StartsWith("rt_fixed_") ? "FIXED" : "RANDOM";
                    sb.AppendLine($"  - [{isFixed}] Id: {node.nodeId} | Key: {node.routeKey} | Kind: {node.kind} | IdxInDepth: {node.indexInDepth} | Template: {node.templateNodeId}");
                }
            }
            sb.AppendLine("=========================================================");
            Debug.Log(sb.ToString());
        }

        /// <summary>
        /// SO 필수 노드의 routeKey를 결정한다.
        /// routeKey가 명시되어 있으면 그대로 사용하고,
        /// 비어 있으면 column.ToString()으로 폴백한다. (기존 데이터 호환)
        /// </summary>
        private static string ResolveRouteKey(StageRequiredNode source)
        {
            if (source.kind == StageNodeKind.GlobalHubNode)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(source.routeKey))
            {
                return source.routeKey;
            }

            // routeKey 미설정: column 기반 1-based 폴백
            return (source.column + 1).ToString();
        }
    }
}