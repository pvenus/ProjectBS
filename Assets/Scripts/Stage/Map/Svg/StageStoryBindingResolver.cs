using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stage
{
    public class StageStoryBindingResolver
    {
        public List<StageStorySlotBinding> ResolveBindings(
            List<StageMapSlot> slots, 
            string chapterKey, 
            IReadOnlyList<RoundNodeSO> allAvailableNodes, 
            StageMapImportReport report)
        {
            var log = new StringBuilder();
            log.AppendLine("─── [Step 7] RoundNodeSO Auto Matching (Resolved) ───");

            var nodeById = new Dictionary<string, RoundNodeSO>(StringComparer.OrdinalIgnoreCase);
            foreach (var node in allAvailableNodes)
            {
                if (node != null && !string.IsNullOrEmpty(node.nodeId))
                {
                    nodeById[node.nodeId] = node;
                }
            }
            log.AppendLine($"  RoundNodeSO 총 {nodeById.Count}개 로드됨.");

            var bindings = new List<StageStorySlotBinding>();
            int matchCount = 0;
            int missCount  = 0;

            var storySlots = slots
                .Where(s => s.role == StageMapSlotRole.Story)
                .OrderBy(s => s.depth)
                .ThenBy(s => s.orderInDepth)
                .ToList();

            foreach (var slot in storySlots)
            {
                string expectedId = BuildExpectedNodeId(slot.slotId, chapterKey);
                nodeById.TryGetValue(expectedId, out RoundNodeSO found);

                var binding = new StageStorySlotBinding
                {
                    slotId         = slot.slotId,
                    expectedNodeId = expectedId,
                    node           = found
                };
                bindings.Add(binding);

                if (found != null)
                {
                    log.AppendLine($"  [✓] {slot.slotId,-20} → {expectedId,-36} ({found.name})");
                    matchCount++;
                }
                else
                {
                    string warnMsg = $"Story slot '{slot.slotId}' 매칭 실패: '{expectedId}' 노드를 찾을 수 없습니다.";
                    log.AppendLine($"  [✗] {slot.slotId,-20} → {expectedId,-36} (NOT FOUND)");
                    report.warningMessages.Add(warnMsg);
                    missCount++;
                }
            }

            log.AppendLine($"  결과: {matchCount}개 매칭 / {missCount}개 누락");
            if (missCount > 0)
            {
                log.AppendLine("  [WARNING] 누락된 RoundNodeSO는 nodeId 또는 파일명을 확인하세요.");
            }

            report.rawImportLog += "\n" + log.ToString();
            report.storyBindingsCount = bindings.Count;
            report.matchedStoryBindingsCount = matchCount;
            report.missingStoryBindingsCount = missCount;

            return bindings;
        }

        private string BuildExpectedNodeId(string slotId, string chapterKey)
        {
            if (!slotId.StartsWith("ep_", StringComparison.OrdinalIgnoreCase))
                return $"stage.{chapterKey}.{slotId}";

            string rest = slotId.Substring(3);
            int underIdx = rest.IndexOf('_');
            string episodeKey = underIdx < 0
                ? $"episode{rest}"
                : $"episode{rest.Substring(0, underIdx)}_{rest.Substring(underIdx + 1)}";

            return $"stage.{chapterKey}.{episodeKey}";
        }
    }
}
