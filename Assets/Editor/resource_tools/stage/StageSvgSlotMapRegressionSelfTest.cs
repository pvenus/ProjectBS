using System;
using System.Collections.Generic;
using System.Linq;
using Stage;
using UnityEditor;
using UnityEngine;

namespace StageEditor
{
    public static class StageSvgSlotMapRegressionSelfTest
    {
        private const string DefinitionPath =
            "Assets/Resources/stage_new/definitions/stage.chapter1.asset";

        [MenuItem(
            "Tools/Stage/Choice Execution Tests/Run SVG Map Regression")]
        public static void RunFromMenu()
        {
            RunAll();
        }

        public static void RunAll()
        {
            StageDefinitionSO definition =
                AssetDatabase.LoadAssetAtPath<StageDefinitionSO>(
                    DefinitionPath);
            Ensure(definition != null, "Stage definition was not found.");

            List<StageMapSlot> slots =
                definition.svgMapSlots ?? new List<StageMapSlot>();
            Ensure(slots.Count > 0, "SVG slot list is empty.");

            Dictionary<string, StageMapSlot> slotById =
                slots.ToDictionary(
                    slot => slot.slotId,
                    StringComparer.OrdinalIgnoreCase);
            int storySlotCount =
                slots.Count(slot => slot.role == StageMapSlotRole.Story);
            int randomSlotCount =
                slots.Count(slot => slot.role == StageMapSlotRole.Random);
            int sourceEdgeCount =
                slots.Sum(slot => slot.connections?.Count ?? 0);

            ValidateStoryBindings(
                definition,
                slotById,
                storySlotCount);
            ValidateRandomSections(
                definition,
                slotById,
                randomSlotCount);

            StageGraph graph =
                new(definition.stageId, definition.stageName);
            StageSvgSlotMapGraphGenerator generator = new();
            Ensure(
                generator.Generate(definition, graph),
                "SVG graph generation failed.");
            Ensure(
                graph.nodes.Count == slots.Count,
                $"Node count mismatch. slots={slots.Count}, "
                + $"nodes={graph.nodes.Count}");
            Ensure(
                generator.LastAssignments.Count == slots.Count,
                "Not every SVG slot received a RoundNodeSO.");

            int runtimeEdgeCount =
                graph.nodes.Sum(node => node.nextNodeIds.Count);
            Ensure(
                runtimeEdgeCount == sourceEdgeCount,
                $"Edge count mismatch. source={sourceEdgeCount}, "
                + $"runtime={runtimeEdgeCount}");
            Ensure(
                graph.nodes.All(
                    node => node == graph.StartNode
                            || node.prevNodeIds.Count > 0),
                "A non-start node has no incoming edge.");
            Ensure(
                graph.nodes.All(
                    node => node == graph.BossNode
                            || node.nextNodeIds.Count > 0),
                "A non-boss node has no outgoing edge.");

            Debug.Log(
                "SVG map regression tests passed.\n"
                + $"Slots: {slots.Count} "
                + $"(Story={storySlotCount}, Random={randomSlotCount})\n"
                + $"Sections: {definition.svgRandomSections.Count}\n"
                + $"Nodes: {graph.nodes.Count}\n"
                + $"Edges: {runtimeEdgeCount}");
        }

        private static void ValidateStoryBindings(
            StageDefinitionSO definition,
            IReadOnlyDictionary<string, StageMapSlot> slotById,
            int storySlotCount)
        {
            List<StageStorySlotBinding> bindings =
                definition.svgStorySlotBindings
                ?? new List<StageStorySlotBinding>();
            Ensure(
                bindings.Count == storySlotCount,
                $"Story binding count mismatch. slots={storySlotCount}, "
                + $"bindings={bindings.Count}");

            HashSet<string> boundSlots =
                new(StringComparer.OrdinalIgnoreCase);
            foreach (StageStorySlotBinding binding in bindings)
            {
                Ensure(
                    binding != null
                    && !string.IsNullOrWhiteSpace(binding.slotId),
                    "Story binding has an empty slotId.");
                Ensure(
                    boundSlots.Add(binding.slotId),
                    $"Duplicate Story binding: {binding.slotId}");
                Ensure(
                    slotById.TryGetValue(
                        binding.slotId,
                        out StageMapSlot slot)
                    && slot.role == StageMapSlotRole.Story,
                    $"Story binding targets an invalid slot: "
                    + binding.slotId);
                Ensure(
                    binding.node != null
                    && string.Equals(
                        binding.node.nodeId,
                        binding.expectedNodeId,
                        StringComparison.Ordinal),
                    $"Story binding node mismatch: {binding.slotId}");
            }
        }

        private static void ValidateRandomSections(
            StageDefinitionSO definition,
            IReadOnlyDictionary<string, StageMapSlot> slotById,
            int randomSlotCount)
        {
            List<StageRandomSection> sections =
                definition.svgRandomSections
                ?? new List<StageRandomSection>();
            HashSet<string> covered =
                new(StringComparer.OrdinalIgnoreCase);

            foreach (StageRandomSection section in sections)
            {
                Ensure(
                    section != null
                    && !string.IsNullOrWhiteSpace(section.sectionId),
                    "Random section has an empty sectionId.");
                Ensure(
                    section.placementRule != null,
                    $"Random section has no placement rule: "
                    + section.sectionId);

                foreach (string slotId in section.targetSlotIds)
                {
                    Ensure(
                        covered.Add(slotId),
                        $"Random slot belongs to multiple sections: "
                        + slotId);
                    Ensure(
                        slotById.TryGetValue(
                            slotId,
                            out StageMapSlot slot)
                        && slot.role == StageMapSlotRole.Random,
                        $"Random section targets an invalid slot: "
                        + slotId);
                }
            }

            Ensure(
                covered.Count == randomSlotCount,
                $"Random section coverage mismatch. slots={randomSlotCount}, "
                + $"covered={covered.Count}");
        }

        private static void Ensure(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
