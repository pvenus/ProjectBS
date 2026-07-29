using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stage
{
    public class StageRandomSectionResolver
    {
        public List<StageRandomSection> ResolveSections(
            List<StageMapSlot> slots,
            List<StageRandomSection> existingSections,
            StageMapImportReport report)
        {
            var log = new StringBuilder();
            log.AppendLine("─── [Step 8] Random Sections Inference (Resolved) ───");

            if (slots == null || slots.Count == 0)
            {
                log.AppendLine("  [Warning] 슬롯 데이터가 없어 추론을 건너뜁니다.");
                report.rawImportLog += "\n" + log.ToString();
                return new List<StageRandomSection>();
            }

            var slotMap = slots.ToDictionary(s => s.slotId, s => s, StringComparer.OrdinalIgnoreCase);
            var storySlots = slots.Where(s => s.role == StageMapSlotRole.Story).ToList();

            var sections = new List<StageRandomSection>();
            var randomToSections = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            foreach (var fromSlot in storySlots)
            {
                if (fromSlot.connections == null || fromSlot.connections.Count == 0)
                    continue;

                foreach (var conn in fromSlot.connections)
                {
                    var startNodeId = conn.toSlotId;
                    if (!slotMap.TryGetValue(startNodeId, out var startNode))
                        continue;

                    var sectionRandoms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    var reachedStories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    var queue = new Queue<string>();
                    var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    queue.Enqueue(startNodeId);
                    visited.Add(startNodeId);

                    while (queue.Count > 0)
                    {
                        var currId = queue.Dequeue();
                        if (!slotMap.TryGetValue(currId, out var currNode))
                            continue;

                        if (currNode.role == StageMapSlotRole.Story)
                        {
                            reachedStories.Add(currId);
                            continue;
                        }

                        sectionRandoms.Add(currId);

                        if (currNode.connections != null)
                        {
                            foreach (var nextConn in currNode.connections)
                            {
                                if (visited.Add(nextConn.toSlotId))
                                {
                                    queue.Enqueue(nextConn.toSlotId);
                                }
                            }
                        }
                    }

                    if (sectionRandoms.Count > 0)
                    {
                        foreach (var toStoryId in reachedStories)
                        {
                            string secId = CreateSectionId(fromSlot.slotId, toStoryId);

                            var existingSec = sections.FirstOrDefault(s => s.sectionId == secId);
                            if (existingSec != null)
                            {
                                foreach (var rId in sectionRandoms)
                                {
                                    if (!existingSec.targetSlotIds.Contains(rId))
                                    {
                                        existingSec.targetSlotIds.Add(rId);
                                    }
                                }
                                existingSec.targetSlotIds = existingSec.targetSlotIds
                                    .OrderBy(r => slotMap[r].depth)
                                    .ThenBy(r => slotMap[r].orderInDepth)
                                    .ToList();
                            }
                            else
                            {
                                StagePlacementRuleSO preservedRule = null;
                                if (existingSections != null)
                                {
                                    var match = existingSections.FirstOrDefault(s => s.sectionId == secId);
                                    if (match != null)
                                    {
                                        preservedRule = match.placementRule;
                                    }
                                }

                                var newSec = new StageRandomSection
                                {
                                    sectionId = secId,
                                    fromStorySlotId = fromSlot.slotId,
                                    toStorySlotId = toStoryId,
                                    targetSlotIds = sectionRandoms
                                        .OrderBy(r => slotMap[r].depth)
                                        .ThenBy(r => slotMap[r].orderInDepth)
                                        .ToList(),
                                    placementRule = preservedRule
                                };
                                sections.Add(newSec);
                            }
                        }
                    }
                }
            }

            foreach (var sec in sections)
            {
                foreach (var rId in sec.targetSlotIds)
                {
                    if (!randomToSections.TryGetValue(rId, out var secList))
                    {
                        secList = new List<string>();
                        randomToSections[rId] = secList;
                    }
                    secList.Add(sec.sectionId);
                }
            }

            int overlapCount = 0;
            foreach (var kvp in randomToSections)
            {
                if (kvp.Value.Count > 1)
                {
                    overlapCount++;
                    string warnMsg = $"Random Slot '{kvp.Key}' 이/가 여러 섹션({string.Join(", ", kvp.Value)})에 중복 소속되어 있습니다.";
                    log.AppendLine($"  [Overlap Warning] {warnMsg}");
                    report.warningMessages.Add(warnMsg);
                }
            }

            log.AppendLine($"  총 {sections.Count}개 Random Section 추론 완료. (중복 슬롯: {overlapCount}개)");
            foreach (var sec in sections)
            {
                string ruleName = sec.placementRule != null ? sec.placementRule.name : "null";
                log.AppendLine($"    - {sec.sectionId} ({sec.targetSlotIds.Count} slots, rule={ruleName})");
            }

            report.rawImportLog += "\n" + log.ToString();
            report.randomSectionsCount = sections.Count;
            report.totalRandomSlotsInSection = randomToSections.Count;

            return sections;
        }

        private string CreateSectionId(string fromStoryId, string toStoryId)
        {
            return $"sec_{fromStoryId}_to_{toStoryId}";
        }
    }
}
