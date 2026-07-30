using System;
using System.Collections.Generic;
using System.Linq;

namespace Stage
{
    public class StageSlotMapDefinitionApplier
    {
        public void ApplyToDefinition(
            StageDefinitionSO targetSO,
            List<StageMapSlot> slots,
            List<StageStorySlotBinding> bindings,
            List<StageRandomSection> sections)
        {
            if (targetSO == null) return;

            targetSO.svgMapSlots = slots ?? new List<StageMapSlot>();
            targetSO.svgStorySlotBindings = bindings ?? new List<StageStorySlotBinding>();

            var ruleMap = new Dictionary<string, StagePlacementRuleSO>(StringComparer.OrdinalIgnoreCase);
            if (targetSO.svgRandomSections != null)
            {
                foreach (var oldSec in targetSO.svgRandomSections)
                {
                    if (oldSec != null && !string.IsNullOrEmpty(oldSec.sectionId) && oldSec.placementRule != null)
                    {
                        ruleMap[oldSec.sectionId] = oldSec.placementRule;
                    }
                }
            }

            targetSO.svgRandomSections = sections ?? new List<StageRandomSection>();
            foreach (var newSec in targetSO.svgRandomSections)
            {
                if (newSec != null && !string.IsNullOrEmpty(newSec.sectionId))
                {
                    if (newSec.placementRule == null && ruleMap.TryGetValue(newSec.sectionId, out var oldRule))
                    {
                        newSec.placementRule = oldRule;
                    }
                }
            }
        }
    }
}
