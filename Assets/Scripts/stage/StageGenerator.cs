using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Stage
{
    /// <summary>
    /// StageDefinitionSO를 기반으로 런타임 StageGraph를 생성한다.
    /// 1차 버전: Start → 중간 depth 랜덤 노드 → Boss 구조를 생성한다.
    /// </summary>
    public class StageGenerator
    {
        private StageDefinitionSO definition;
        private System.Random fixedRandom;

        public StageGraph Generate(StageDefinitionSO stageDefinition)
        {
            if (stageDefinition == null)
            {
                Debug.LogError("[StageGenerator] StageDefinitionSO is null.");
                return null;
            }

            definition = stageDefinition;
            fixedRandom = definition.useFixedSeed ? new System.Random(definition.seed) : null;

            StageGraph graph = new StageGraph(definition.stageId, definition.stageName);

            int totalDepth = Mathf.Max(2, definition.totalDepth);
            int bossDepth = totalDepth - 1;

            RoundNode startNode = CreateStartNode();
            graph.AddNode(startNode);

            Dictionary<int, List<RoundNode>> depthNodes = new();
            depthNodes[0] = new List<RoundNode> { startNode };

            for (int depth = 1; depth < bossDepth; depth++)
            {
                List<RoundNode> nodes = CreateMiddleDepthNodes(depth);
                depthNodes[depth] = nodes;

                foreach (RoundNode node in nodes)
                {
                    graph.AddNode(node);
                }
            }

            RoundNode bossNode = CreateBossNode(bossDepth);
            graph.AddNode(bossNode);
            depthNodes[bossDepth] = new List<RoundNode> { bossNode };

            ConnectDepths(graph, depthNodes, bossDepth);
            graph.StartStage();

            return graph;
        }

        private RoundNode CreateStartNode()
        {
            RoundNodeSO source = definition.startNode;

            if (source != null)
            {
                return CreateNodeFromSO(source, 0, 0, "start");
            }

            return new RoundNode(
                "start",
                "Start",
                RoundNodeType.Start,
                RoundExecuteMode.Immediate,
                0,
                0)
            {
                description = "스테이지 시작 지점입니다."
            };
        }

        private RoundNode CreateBossNode(int depth)
        {
            RoundNodeSO source = definition.bossNode;

            if (source != null)
            {
                return CreateNodeFromSO(source, depth, 0, $"boss_{depth}");
            }

            return new RoundNode(
                $"boss_{depth}",
                "Boss",
                RoundNodeType.Boss,
                RoundExecuteMode.Scene,
                depth,
                0)
            {
                description = "스테이지 보스 전투입니다."
            };
        }

        private List<RoundNode> CreateMiddleDepthNodes(int depth)
        {
            int minCount = Mathf.Max(1, definition.minNodesPerDepth);
            int maxCount = Mathf.Max(minCount, definition.maxNodesPerDepth);
            int nodeCount = GetRandomRangeInclusive(minCount, maxCount);

            List<RoundNode> result = new();
            List<RoundNodeSO> requiredNodes = GetRequiredSubEventsForDepth(depth);

            int index = 0;
            foreach (RoundNodeSO requiredNode in requiredNodes)
            {
                result.Add(CreateNodeFromSO(requiredNode, depth, index, $"required_{depth}_{index}"));
                index++;
            }

            while (result.Count < nodeCount)
            {
                RoundNode randomNode = CreateRandomPoolNode(depth, index);
                result.Add(randomNode);
                index++;
            }

            return result;
        }

        private List<RoundNodeSO> GetRequiredSubEventsForDepth(int depth)
        {
            if (definition.requiredSubEvents == null || definition.requiredSubEvents.Count == 0)
            {
                return new List<RoundNodeSO>();
            }
            return definition.requiredSubEvents
                .Where(x => x != null)
                .ToList();
        }

        private RoundNode CreateRandomPoolNode(
            int depth,
            int indexInDepth)
        {
            EventPoolSO selectedPool = PickRandomPool();

            if (selectedPool == null)
            {
                return CreateFallbackBattleNode(depth, indexInDepth);
            }

            return new RoundNode(
                $"pool_{depth}_{indexInDepth}",
                "?",
                RoundNodeType.Event,
                RoundExecuteMode.Popup,
                depth,
                indexInDepth)
            {
                description = "Unknown Event",
                useRandomEventPool = true,
                randomPool = selectedPool,
                resolved = false
            };
        }

        private EventPoolSO PickRandomPool()
        {
            if (definition.pools == null
                || definition.pools.Count == 0)
            {
                return null;
            }

            List<EventPoolSO> candidates =
                definition.pools
                    .Where(x => x != null)
                    .ToList();

            if (candidates.Count == 0)
            {
                return null;
            }

            int index =
                GetRandomRangeInclusive(0, candidates.Count - 1);

            return candidates[index];
        }

        private RoundNode CreateNodeFromSO(RoundNodeSO source, int depth, int indexInDepth, string fallbackId)
        {
            string nodeId = string.IsNullOrWhiteSpace(source.nodeId)
                ? fallbackId
                : $"{source.nodeId}_{depth}_{indexInDepth}";

            RoundNode node = new RoundNode(
                nodeId,
                string.IsNullOrWhiteSpace(source.title) ? source.name : source.title,
                source.nodeType,
                source.executeMode,
                depth,
                indexInDepth)
            {
                description = source.description,
                sceneName = source.sceneName,
                eventId = source.eventId,
                popupEvent = source.popupEvent,
                battleGroupId = source.battleGroupId,
                icon = source.icon,
                isRequired = source.isRequired
            };

            return node;
        }

        private RoundNode CreateFallbackBattleNode(int depth, int indexInDepth)
        {
            return new RoundNode(
                $"fallback_battle_{depth}_{indexInDepth}",
                "Battle",
                RoundNodeType.Battle,
                RoundExecuteMode.Scene,
                depth,
                indexInDepth)
            {
                description = "기본 전투 라운드입니다."
            };
        }

        private void ConnectDepths(StageGraph graph, Dictionary<int, List<RoundNode>> depthNodes, int bossDepth)
        {
            for (int depth = 0; depth < bossDepth; depth++)
            {
                if (!depthNodes.TryGetValue(depth, out List<RoundNode> currentDepthNodes))
                {
                    continue;
                }

                if (!depthNodes.TryGetValue(depth + 1, out List<RoundNode> nextDepthNodes))
                {
                    continue;
                }

                ConnectCurrentDepthToNextDepth(graph, currentDepthNodes, nextDepthNodes);
            }
        }

        private void ConnectCurrentDepthToNextDepth(StageGraph graph, List<RoundNode> currentDepthNodes, List<RoundNode> nextDepthNodes)
        {
            if (currentDepthNodes == null || currentDepthNodes.Count == 0)
            {
                return;
            }

            if (nextDepthNodes == null || nextDepthNodes.Count == 0)
            {
                return;
            }

            if (currentDepthNodes.Count == 1)
            {
                foreach (RoundNode nextNode in nextDepthNodes)
                {
                    graph.ConnectNodes(currentDepthNodes[0].nodeId, nextNode.nodeId);
                }

                return;
            }

            if (nextDepthNodes.Count == 1)
            {
                foreach (RoundNode currentNode in currentDepthNodes)
                {
                    graph.ConnectNodes(currentNode.nodeId, nextDepthNodes[0].nodeId);
                }

                return;
            }

            for (int i = 0; i < currentDepthNodes.Count; i++)
            {
                RoundNode currentNode = currentDepthNodes[i];
                RoundNode primaryNextNode = nextDepthNodes[Mathf.Clamp(i, 0, nextDepthNodes.Count - 1)];
                graph.ConnectNodes(currentNode.nodeId, primaryNextNode.nodeId);

                if (i + 1 < nextDepthNodes.Count)
                {
                    graph.ConnectNodes(currentNode.nodeId, nextDepthNodes[i + 1].nodeId);
                }
            }
        }

        private int GetRandomRangeInclusive(int minInclusive, int maxInclusive)
        {
            if (fixedRandom != null)
            {
                return fixedRandom.Next(minInclusive, maxInclusive + 1);
            }

            return Random.Range(minInclusive, maxInclusive + 1);
        }
    }
}