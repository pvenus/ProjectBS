using System;
using System.Collections.Generic;
using System.Linq;

namespace Stage
{
    public static class Chapter1WeightedEventProjection
    {
        public static IReadOnlyDictionary<string, RoundNodeSO> Project(
            StageDefinitionSO definition, Chapter1BattlePressureManifest manifest)
        {
            var result = new Dictionary<string, RoundNodeSO>(StringComparer.OrdinalIgnoreCase);
            if (definition?.svgMapSlots == null || manifest == null || !manifest.Success
                || manifest.Assignments.Count == 0) return result;
            List<StageMapSlot> slots = definition.svgMapSlots
                .Where(slot => slot != null && slot.role == StageMapSlotRole.Random)
                .OrderBy(slot => slot.depth).ThenBy(slot => slot.orderInDepth)
                .ThenBy(slot => slot.slotId, StringComparer.Ordinal).ToList();
            List<int> depths = slots.Select(slot => slot.depth).Distinct().OrderBy(value => value).ToList();
            List<Chapter1CompositionAssignment> assignments = manifest.Assignments.ToList();
            bool[] branchCapable = BranchCapableLogicalOrdinals(slots, depths, assignments.Count);
            for (int i = 0; i < assignments.Count; i++)
            {
                if (assignments[i].Kind != Chapter1EncounterKind.Event
                    || !RequiresBranchingSuccessors(assignments[i].Node) || branchCapable[i]) continue;
                int replacement = Enumerable.Range(0, assignments.Count).FirstOrDefault(candidate =>
                    branchCapable[candidate]
                    && assignments[candidate].Kind == Chapter1EncounterKind.Event
                    && !RequiresBranchingSuccessors(assignments[candidate].Node));
                if (!branchCapable[replacement]
                    || assignments[replacement].Kind != Chapter1EncounterKind.Event) return result;
                (assignments[i], assignments[replacement]) = (assignments[replacement], assignments[i]);
            }
            var logicalByDepth = depths.Select((depth, index) => new { depth, index })
                .ToDictionary(value => value.depth, value => value.index);
            foreach (StageMapSlot slot in slots)
            {
                int depthIndex = logicalByDepth[slot.depth];
                int logical = Math.Min((int)((long)depthIndex * assignments.Count
                    / Math.Max(1, depths.Count)), assignments.Count - 1);
                result[slot.slotId] = assignments[logical].Node;
            }
            return result;
        }

        public static IReadOnlyDictionary<string, RoundNodeSO> Project(
            StageDefinitionSO definition, Chapter1WeightedEventManifest manifest)
        {
            var result = new Dictionary<string, RoundNodeSO>(StringComparer.OrdinalIgnoreCase);
            if (definition?.svgRandomSections == null || manifest == null || !manifest.Success
                || manifest.Assignments.Count == 0) return result;
            List<StageMapSlot> slots = definition.svgMapSlots
                .Where(slot => slot != null && slot.role == StageMapSlotRole.Random)
                .OrderBy(slot => slot.depth).ThenBy(slot => slot.orderInDepth)
                .ThenBy(slot => slot.slotId, StringComparer.Ordinal).ToList();
            List<int> depths = slots.Select(slot => slot.depth).Distinct().OrderBy(value => value).ToList();
            List<Chapter1WeightedEventAssignment> assignments = manifest.Assignments.ToList();
            AlignRouteBoundEventsWithBranchingDepths(slots, depths, assignments);
            var logicalByDepth = depths.Select((depth, index) => new { depth, index })
                .ToDictionary(value => value.depth, value => value.index);
            foreach (StageMapSlot slot in slots)
            {
                // Parallel route/mirror slots at one depth share one immutable logical
                // assignment. Advancing to another depth must advance the logical event;
                // flattening all physical slots here repeats one event on consecutive nodes.
                int depthIndex = logicalByDepth[slot.depth];
                int logical = (int)((long)depthIndex * manifest.Assignments.Count
                    / Math.Max(1, depths.Count));
                logical = Math.Min(logical, manifest.Assignments.Count - 1);
                result[slot.slotId] = assignments[logical].Row.node;
            }
            return result;
        }

        private static void AlignRouteBoundEventsWithBranchingDepths(
            IReadOnlyList<StageMapSlot> slots,
            IReadOnlyList<int> depths,
            IList<Chapter1WeightedEventAssignment> assignments)
        {
            int logicalCount = assignments.Count;
            if (logicalCount == 0 || depths.Count == 0) return;

            bool[] branchCapable = BranchCapableLogicalOrdinals(slots, depths, logicalCount);

            for (int logical = 0; logical < logicalCount; logical++)
            {
                if (!RequiresBranchingSuccessors(assignments[logical]?.Row?.node)
                    || branchCapable[logical]) continue;
                int replacement = Enumerable.Range(0, logicalCount).FirstOrDefault(candidate =>
                    branchCapable[candidate]
                    && !RequiresBranchingSuccessors(assignments[candidate]?.Row?.node));
                if (!branchCapable[replacement]
                    || RequiresBranchingSuccessors(assignments[replacement]?.Row?.node)) continue;
                (assignments[logical], assignments[replacement]) =
                    (assignments[replacement], assignments[logical]);
            }
        }

        private static bool[] BranchCapableLogicalOrdinals(IReadOnlyList<StageMapSlot> slots,
            IReadOnlyList<int> depths, int logicalCount)
        {
            var result = new bool[logicalCount];
            for (int logical = 0; logical < logicalCount; logical++)
            {
                var projectedDepths = depths.Where((depth, depthIndex) =>
                    Math.Min((int)((long)depthIndex * logicalCount / depths.Count), logicalCount - 1)
                    == logical).ToHashSet();
                List<StageMapSlot> atDepth = slots.Where(slot => projectedDepths.Contains(slot.depth)).ToList();
                result[logical] = atDepth.Count > 0 && atDepth.All(slot =>
                    slot.connections != null && slot.connections.Select(item => item?.toSlotId)
                        .Where(id => !string.IsNullOrWhiteSpace(id))
                        .Distinct(StringComparer.OrdinalIgnoreCase).Count() >= 2);
            }
            return result;
        }

        private static bool RequiresBranchingSuccessors(RoundNodeSO node) =>
            node?.popupEvent?.choices?.Any(choice =>
                choice?.executionConfig?.data is PortfolioOutcomeExecutionData outcome
                && outcome.operations?.Any(operation => operation != null &&
                    (operation.kind == PortfolioOutcomeOperationKind.CommitImmediateSuccessorRoute
                     || operation.kind == PortfolioOutcomeOperationKind.RelicRouteTrade
                     || (operation.kind == PortfolioOutcomeOperationKind.RevealImmediateSuccessorPurpose
                         && operation.selectionMode != ImmediateSuccessorRouteSelectionMode.None))) == true) == true;
    }
}
