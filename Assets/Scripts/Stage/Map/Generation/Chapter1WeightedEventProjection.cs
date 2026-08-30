using System;
using System.Collections.Generic;
using System.Linq;

namespace Stage
{
    public static class Chapter1WeightedEventProjection
    {
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
            for (int i = 0; i < slots.Count; i++)
            {
                // Multiple physical route/mirror projections share one of the twelve
                // immutable logical assignments and therefore consume no extra budget.
                int logical = (int)((long)i * manifest.Assignments.Count / Math.Max(1, slots.Count));
                logical = Math.Min(logical, manifest.Assignments.Count - 1);
                result[slots[i].slotId] = manifest.Assignments[logical].Row.node;
            }
            return result;
        }
    }
}
