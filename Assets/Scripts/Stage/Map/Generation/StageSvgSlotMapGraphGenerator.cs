using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Session;

namespace Stage
{
    public class StageSvgSlotMapGraphGenerator
    {
        private readonly HashSet<string> generatedNodeIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, RoundNodeSO> lastAssignments =
            new Dictionary<string, RoundNodeSO>(StringComparer.OrdinalIgnoreCase);
        private readonly List<RandomGrowthReservationDescriptor> lastRandomGrowthReservations = new();
        private readonly Dictionary<string, string> lastRuntimeNodeIdBySlotId =
            new(StringComparer.Ordinal);
        private Chapter1WeightedEventManifest lastWeightedEventManifest;
        private Chapter1BattlePressureManifest lastBattlePressureManifest;

        public IReadOnlyDictionary<string, RoundNodeSO> LastAssignments => lastAssignments;
        public IReadOnlyList<RandomGrowthReservationDescriptor> LastRandomGrowthReservations =>
            lastRandomGrowthReservations.AsReadOnly();
        public IReadOnlyDictionary<string, string> LastRuntimeNodeIdBySlotId =>
            lastRuntimeNodeIdBySlotId;
        internal Chapter1WeightedEventManifest LastWeightedEventManifest => lastWeightedEventManifest;
        internal Chapter1BattlePressureManifest LastBattlePressureManifest => lastBattlePressureManifest;

        public bool Generate(
            StageDefinitionSO definition,
            StageGraph graph,
            RandomGrowthGraphContext randomGrowthContext = null)
        {
            if (definition == null)
            {
                Debug.LogError("[SvgSlotMapGraphGenerator] Generate failed: definition is null.");
                return false;
            }

            if (graph == null)
            {
                Debug.LogError("[SvgSlotMapGraphGenerator] Generate failed: graph is null.");
                return false;
            }

            generatedNodeIds.Clear();
            lastAssignments.Clear();
            lastRandomGrowthReservations.Clear();
            lastRuntimeNodeIdBySlotId.Clear();
            lastWeightedEventManifest = null;
            lastBattlePressureManifest = null;
            var slots = definition.svgMapSlots;

            if (slots == null || slots.Count == 0)
            {
                Debug.LogError("[SvgSlotMapGraphGenerator] Generate failed: svgMapSlots is null or empty.");
                return false;
            }

            var slotById = new Dictionary<string, StageMapSlot>(StringComparer.OrdinalIgnoreCase);
            foreach (var slot in slots)
            {
                if (slot == null)
                {
                    Debug.LogError("[SvgSlotMapGraphGenerator] Generate failed: null slot element in svgMapSlots.");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(slot.slotId))
                {
                    Debug.LogError("[SvgSlotMapGraphGenerator] Generate failed: slot has empty slotId.");
                    return false;
                }

                if (slotById.ContainsKey(slot.slotId))
                {
                    Debug.LogError($"[SvgSlotMapGraphGenerator] Generate failed: duplicate slotId '{slot.slotId}' found in svgMapSlots.");
                    return false;
                }

                slotById[slot.slotId] = slot;
            }

            foreach (var slot in slots)
            {
                if (slot.connections == null) continue;

                var seenToSlotIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var conn in slot.connections)
                {
                    if (conn == null || string.IsNullOrWhiteSpace(conn.toSlotId)) continue;

                    if (string.Equals(conn.toSlotId, slot.slotId, StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.LogError($"[SvgSlotMapGraphGenerator] Generate failed: Self-connecting slotId '{slot.slotId}' is not allowed.");
                        return false;
                    }

                    if (!seenToSlotIds.Add(conn.toSlotId))
                    {
                        Debug.LogWarning($"[SvgSlotMapGraphGenerator] Slot '{slot.slotId}' has duplicate connection to '{conn.toSlotId}'.");
                    }

                    if (!slotById.TryGetValue(conn.toSlotId, out var toSlot))
                    {
                        Debug.LogError($"[SvgSlotMapGraphGenerator] Generate failed: Slot '{slot.slotId}' connects to non-existent toSlotId '{conn.toSlotId}'.");
                        return false;
                    }

                    if (toSlot.depth <= slot.depth)
                    {
                        Debug.LogWarning($"[SvgSlotMapGraphGenerator] Slot '{slot.slotId}' (depth:{slot.depth}) connects to '{toSlot.slotId}' with backward/same depth ({toSlot.depth}).");
                    }
                }
            }

            RandomGrowthGraphContext context = randomGrowthContext
                ?? RandomGrowthGraphContext.TryCreateCurrent();
            if (context != null)
            {
                RandomGrowthProjectionResult projection =
                    new Chapter1RandomGrowthReservationProjector().Project(
                        definition,
                        context.StageSession,
                        context.RunId,
                        context.IdentityFactory);
                lastRandomGrowthReservations.AddRange(projection.Reservations);
                if (context.SafePlacement != null)
                {
                    SafeGrowthProjectionResult safeProjection =
                        new Chapter1RandomGrowthReservationProjector().ProjectSafe(
                            definition,
                            context.StageSession,
                            context.SafePlacement);
                    lastRandomGrowthReservations.AddRange(safeProjection.Reservations);
                }
            }

            var assignmentResolver = new StageSlotAssignmentResolver();
            // A non-fixed stage must vary between runs, but remain reproducible while
            // rebuilding the same run. Previously the weighted manifest always used
            // StableSeed(stageId), so every new game received the exact same events.
            int runSeed = StableSeed($"{definition.stageId}|{context?.RunId.Value ?? Guid.NewGuid().ToString("N")}");
            int? seed = definition.useFixedSeed ? definition.seed : runSeed;
            WeightedPoolPlacementConfig weightedConfig = definition.svgRandomSections
                .Select(section => section?.placementRule)
                .Where(rule => rule != null && rule.mode == StagePlacementRuleMode.WeightedPool)
                .Select(rule => rule.weightedPool)
                .FirstOrDefault(config => config != null && config.HasCompiledPlacement);
            if (weightedConfig != null)
            {
                int manifestSeed = seed.Value;
                if (weightedConfig.composition?.enabled == true)
                {
                    lastBattlePressureManifest = new Chapter1BattlePressureManifestBuilder().Build(
                        weightedConfig, manifestSeed, CurrentRoster(), CapabilityEnabled);
                    if (!lastBattlePressureManifest.Success)
                    {
                        Debug.LogError($"[SvgSlotMapGraphGenerator] Composition manifest failed: "
                            + lastBattlePressureManifest.Error);
                        return false;
                    }
                }
                else
                    lastWeightedEventManifest = new Chapter1WeightedEventManifestBuilder().Build(
                        weightedConfig, manifestSeed, CurrentRoster(), CapabilityEnabled);
            }
            var assignedNodeBySlotId = lastBattlePressureManifest?.Success == true
                ? assignmentResolver.ResolveAssignments(definition, lastBattlePressureManifest,
                    seed, lastRandomGrowthReservations)
                : lastWeightedEventManifest?.Success == true
                ? assignmentResolver.ResolveAssignments(definition, lastWeightedEventManifest,
                    seed, lastRandomGrowthReservations)
                : assignmentResolver.ResolveAssignments(definition, seed, lastRandomGrowthReservations);
            foreach (var assignment in assignedNodeBySlotId)
            {
                lastAssignments[assignment.Key] = assignment.Value;
            }

            foreach (var slot in slots)
            {
                if (!assignedNodeBySlotId.TryGetValue(slot.slotId, out var nodeSO) || nodeSO == null)
                {
                    Debug.LogError($"[SvgSlotMapGraphGenerator] Generate failed: Slot '{slot.slotId}' (role:{slot.role}) was NOT assigned a valid RoundNodeSO.");
                    return false;
                }
            }

            var runtimeNodeIdBySlotId = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var sortedSlots = slots
                .OrderBy(s => s.depth)
                .ThenBy(s => s.orderInDepth)
                .ToList();

            foreach (var slot in sortedSlots)
            {
                RoundNodeSO nodeSO = assignedNodeBySlotId[slot.slotId];
                string runtimeNodeId = CreateRuntimeNodeId(slot.role, slot.depth, slot.slotId, nodeSO.nodeId);

                RoundNode roundNode = CreateNodeFromSO(
                    nodeSO,
                    slot.depth,
                    slot.orderInDepth,
                    runtimeNodeId,
                    routeKey: string.Empty,
                    kind: StageNodeKind.RouteNode);

                roundNode.isRequired = (slot.role == StageMapSlotRole.Story);
                roundNode.hiddenByDefault = false;

                graph.AddNode(roundNode);
                runtimeNodeIdBySlotId[slot.slotId] = runtimeNodeId;
                lastRuntimeNodeIdBySlotId[slot.slotId] = runtimeNodeId;
            }

            foreach (var slot in sortedSlots)
            {
                if (slot.connections == null) continue;

                string fromRuntimeId = runtimeNodeIdBySlotId[slot.slotId];
                foreach (var conn in slot.connections)
                {
                    if (conn == null || string.IsNullOrWhiteSpace(conn.toSlotId)) continue;

                    if (runtimeNodeIdBySlotId.TryGetValue(conn.toSlotId, out string toRuntimeId))
                    {
                        graph.ConnectNodes(fromRuntimeId, toRuntimeId);
                    }
                    else
                    {
                        Debug.LogError($"[SvgSlotMapGraphGenerator] Cannot connect edge: toSlotId '{conn.toSlotId}' not found in runtime node map.");
                    }
                }
            }

            RoundNode startNode = graph.nodes.FirstOrDefault(n => n.IsStartNode)
                                  ?? graph.nodes.OrderBy(n => n.depth).ThenBy(n => n.indexInDepth).FirstOrDefault();
            if (startNode != null)
            {
                graph.startNodeId = startNode.nodeId;
                startNode.SetAvailable();
            }

            RoundNode bossNode = graph.nodes.FirstOrDefault(n => n.IsBossNode)
                                 ?? graph.nodes.OrderByDescending(n => n.depth).ThenBy(n => n.indexInDepth).FirstOrDefault();
            if (bossNode != null)
            {
                graph.bossNodeId = bossNode.nodeId;
            }

            ValidateGraph(definition, graph);
            Debug.Log($"[SvgSlotMapGraphGenerator] Generate completed successfully with {graph.nodes.Count} nodes.");
            return true;
        }

        private static int StableSeed(string value)
        {
            unchecked
            {
                int hash = 17;
                foreach (char c in value ?? string.Empty) hash = hash * 31 + c;
                return hash;
            }
        }

        private static IReadOnlyCollection<string> CurrentRoster()
        {
            return GameSession.Instance?.BattleSession?.PartyRuntimeData?.Members?
                .Where(member => member != null && member.characterSO != null)
                .Select(member => member.characterSO.CharacterId).ToArray()
                ?? Array.Empty<string>();
        }

        private static bool CapabilityEnabled(string gate) => string.IsNullOrWhiteSpace(gate);

        private static RoundNode CreateNodeFromSO(
            RoundNodeSO source,
            int depth,
            int indexInDepth,
            string nodeId,
            string routeKey,
            StageNodeKind kind = StageNodeKind.RouteNode)
        {
            return new RoundNode(
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
                resolvedIconType = source.GetResolvedIconType()
            };
        }

        private string CreateRuntimeNodeId(StageMapSlotRole role, int depth, string slotId, string templateId)
        {
            string safeSlot = slotId.Replace('.', '_').Replace('-', '_');
            string prefix = (role == StageMapSlotRole.Story) ? "rt_svg_story" : "rt_svg_random";
            string baseId = $"{prefix}_d{depth}_s{safeSlot}_{templateId}";

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

        public void ValidateGraph(StageDefinitionSO definition, StageGraph graph)
        {
            if (graph == null || graph.nodes == null || graph.nodes.Count == 0)
            {
                Debug.LogWarning("[SvgSlotMapGraphGenerator] ValidateGraph skipped: graph or nodes is empty.");
                return;
            }

            int warningCount = 0;
            int errorCount = 0;

            var lookup = new Dictionary<string, RoundNode>(StringComparer.OrdinalIgnoreCase);
            foreach (var node in graph.nodes)
            {
                if (node == null) continue;

                if (string.IsNullOrWhiteSpace(node.nodeId))
                {
                    Debug.LogError("[SvgSlotMapGraphGenerator] SvgSlotMap validation error: Node has empty nodeId.");
                    errorCount++;
                    continue;
                }

                if (lookup.ContainsKey(node.nodeId))
                {
                    Debug.LogError($"[SvgSlotMapGraphGenerator] SvgSlotMap validation error: Duplicate nodeId '{node.nodeId}'.");
                    errorCount++;
                }
                else
                {
                    lookup[node.nodeId] = node;
                }
            }

            int totalEdges = 0;
            foreach (var node in lookup.Values)
            {
                foreach (var prevId in node.prevNodeIds)
                {
                    if (!lookup.TryGetValue(prevId, out var prevNode))
                    {
                        Debug.LogError($"[SvgSlotMapGraphGenerator] SvgSlotMap validation error: Node '{node.nodeId}' has missing prevNodeId '{prevId}'.");
                        errorCount++;
                    }
                    else if (!prevNode.nextNodeIds.Contains(node.nodeId))
                    {
                        Debug.LogError($"[SvgSlotMapGraphGenerator] SvgSlotMap validation error: Node '{node.nodeId}' lists '{prevId}' in prevNodeIds, but '{prevId}' does not list '{node.nodeId}' in nextNodeIds.");
                        errorCount++;
                    }
                }

                foreach (var nextId in node.nextNodeIds)
                {
                    totalEdges++;
                    if (!lookup.TryGetValue(nextId, out var nextNode))
                    {
                        Debug.LogError($"[SvgSlotMapGraphGenerator] SvgSlotMap validation error: Node '{node.nodeId}' has missing nextNodeId '{nextId}'.");
                        errorCount++;
                    }
                    else
                    {
                        if (!nextNode.prevNodeIds.Contains(node.nodeId))
                        {
                            Debug.LogError($"[SvgSlotMapGraphGenerator] SvgSlotMap validation error: Node '{node.nodeId}' lists '{nextId}' in nextNodeIds, but '{nextId}' does not list '{node.nodeId}' in prevNodeIds.");
                            errorCount++;
                        }

                        if (nextNode.depth <= node.depth)
                        {
                            Debug.LogWarning($"[SvgSlotMapGraphGenerator] SvgSlotMap validation warning: Backward/same-depth edge from '{node.nodeId}' (d:{node.depth}) to '{nextId}' (d:{nextNode.depth}).");
                            warningCount++;
                        }
                    }
                }

                bool isStart = (node.nodeId == graph.startNodeId);
                bool isBoss = (node.nodeId == graph.bossNodeId);

                if (!isStart && node.prevNodeIds.Count == 0)
                {
                    Debug.LogWarning($"[SvgSlotMapGraphGenerator] SvgSlotMap validation warning: Isolated node '{node.nodeId}' (depth:{node.depth}) has no incoming edges.");
                    warningCount++;
                }

                if (!isBoss && node.nextNodeIds.Count == 0)
                {
                    Debug.LogWarning($"[SvgSlotMapGraphGenerator] SvgSlotMap validation warning: Non-boss node '{node.nodeId}' (depth:{node.depth}) has no outgoing edges.");
                    warningCount++;
                }
            }

            if (definition.svgRandomSections != null)
            {
                var sectionSlots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var sec in definition.svgRandomSections)
                {
                    if (sec == null) continue;
                    if (sec.placementRule == null)
                    {
                        Debug.LogWarning($"[SvgSlotMapGraphGenerator] SvgSlotMap validation warning: Section '{sec.sectionId}' has null placementRule.");
                        warningCount++;
                    }

                    if (sec.targetSlotIds != null)
                    {
                        foreach (var tid in sec.targetSlotIds)
                        {
                            sectionSlots.Add(tid);
                        }
                    }
                }

                if (definition.svgMapSlots != null)
                {
                    foreach (var slot in definition.svgMapSlots)
                    {
                        if (slot != null && slot.role == StageMapSlotRole.Random)
                        {
                            if (!sectionSlots.Contains(slot.slotId))
                            {
                                Debug.LogWarning($"[SvgSlotMapGraphGenerator] SvgSlotMap validation warning: Random slot '{slot.slotId}' is not included in any StageRandomSection.");
                                warningCount++;
                            }
                        }
                    }
                }
            }

            Debug.Log($"[SvgSlotMapGraphGenerator] SvgSlotMap Validation completed. nodes={lookup.Count}, edges={totalEdges}, errors={errorCount}, warnings={warningCount}");
        }
    }
}
