using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Stage
{
    public class StageSlotAssignmentResolver
    {
        public Dictionary<string, RoundNodeSO> ResolveAssignments(
            StageDefinitionSO stageDefinition,
            int? seed = null)
        {
            var result = new Dictionary<string, RoundNodeSO>(StringComparer.OrdinalIgnoreCase);

            if (stageDefinition == null)
            {
                Debug.LogError("[StageSlotAssignmentResolver] stageDefinition is null.");
                return result;
            }

            ResolveStoryAssignments(stageDefinition, result);
            ResolveRandomAssignments(stageDefinition, result, seed);
            ValidateAssignedSlots(stageDefinition, result);

            return result;
        }

        private void ResolveStoryAssignments(
            StageDefinitionSO stageDefinition,
            Dictionary<string, RoundNodeSO> resultBySlotId)
        {
            if (stageDefinition.svgStorySlotBindings == null) return;

            foreach (var binding in stageDefinition.svgStorySlotBindings)
            {
                if (binding == null || string.IsNullOrEmpty(binding.slotId)) continue;

                if (binding.node != null)
                {
                    resultBySlotId[binding.slotId] = binding.node;
                }
                else
                {
                    Debug.LogWarning($"[StageSlotAssignmentResolver] Story slot binding for '{binding.slotId}' has null RoundNodeSO.");
                }
            }
        }

        private void ResolveRandomAssignments(
            StageDefinitionSO stageDefinition,
            Dictionary<string, RoundNodeSO> resultBySlotId,
            int? seed)
        {
            if (stageDefinition.svgRandomSections == null || stageDefinition.svgRandomSections.Count == 0)
                return;

            if (stageDefinition.svgMapSlots == null || stageDefinition.svgMapSlots.Count == 0)
                return;

            var slotById = stageDefinition.svgMapSlots
                .Where(s => s != null && !string.IsNullOrEmpty(s.slotId))
                .ToDictionary(s => s.slotId, s => s, StringComparer.OrdinalIgnoreCase);

            System.Random random = seed.HasValue ? new System.Random(seed.Value) : new System.Random();
            var processedSlots = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var section in stageDefinition.svgRandomSections)
            {
                if (section == null) continue;

                if (section.placementRule == null)
                {
                    Debug.LogWarning($"[StageSlotAssignmentResolver] Section '{section.sectionId}' has no placementRule. Skipping.");
                    continue;
                }

                if (section.targetSlotIds == null || section.targetSlotIds.Count == 0)
                    continue;

                var targetSlots = new List<StageMapSlot>(section.targetSlotIds.Count);
                foreach (var slotId in section.targetSlotIds)
                {
                    if (string.IsNullOrEmpty(slotId)) continue;

                    if (slotById.TryGetValue(slotId, out var slot))
                    {
                        if (slot.role != StageMapSlotRole.Random)
                        {
                            Debug.LogWarning($"[StageSlotAssignmentResolver] Slot '{slotId}' in Section '{section.sectionId}' is not a Random role. Skipping.");
                            continue;
                        }
                        targetSlots.Add(slot);
                    }
                    else
                    {
                        Debug.LogWarning($"[StageSlotAssignmentResolver] Section '{section.sectionId}' references non-existent slotId '{slotId}'. Skipping.");
                    }
                }

                if (targetSlots.Count == 0) continue;

                foreach (var slot in targetSlots)
                {
                    if (processedSlots.TryGetValue(slot.slotId, out string prevSec))
                    {
                        Debug.LogError($"[StageSlotAssignmentResolver] 중복 배정 감지! 슬롯 '{slot.slotId}'이(가) 이미 섹션 '{prevSec}'에 의해 처리되었으나 섹션 '{section.sectionId}'에도 포함되어 있습니다.");
                    }
                    else
                    {
                        processedSlots[slot.slotId] = section.sectionId;
                    }
                }

                section.placementRule.Fill(section, targetSlots, resultBySlotId, random);
            }
        }

        private void ValidateAssignedSlots(
            StageDefinitionSO stageDefinition,
            Dictionary<string, RoundNodeSO> resultBySlotId)
        {
            if (stageDefinition.svgMapSlots == null) return;

            foreach (var slot in stageDefinition.svgMapSlots)
            {
                if (slot == null || string.IsNullOrEmpty(slot.slotId)) continue;

                if (!resultBySlotId.ContainsKey(slot.slotId))
                {
                    Debug.LogWarning($"[StageSlotAssignmentResolver] Slot '{slot.slotId}' (role: {slot.role}) 에 배치된 유효한 RoundNodeSO가 없습니다.");
                }
            }
        }
    }
}
