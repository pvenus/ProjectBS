using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Stage
{
    public class StageSvgSlotMapBuilder
    {
        private const float DepthGroupTolerance = 5f;

        private static readonly HashSet<string> StoryClasses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "main", "jihan", "yujin", "final"
        };

        public class BuildResult
        {
            public List<StageMapSlot> slots;
            public StageMapImportReport report;
        }

        private class SvgCircleData
        {
            public float cx, cy;
            public float svgCyOriginal;
            public float r = 34f;
            public string cssClass = "";
            public bool isLegend;

            public int depth;
            public int orderInDepth;
            public string slotId = "";
            public string label = "";
            public string subLabel = "";

            public List<string> connectionTargets = new List<string>();
        }

        private class SvgTextData
        {
            public float x, y;
            public string cssClass = "";
            public string content = "";
        }

        private class SvgPathEndpoints
        {
            public Vector2 start;
            public Vector2 end;
        }

        public BuildResult BuildFromSvg(string svgText)
        {
            var report = new StageMapImportReport { isSuccess = true };
            var log = new StringBuilder();
            log.AppendLine("=== SVG SlotMap Build Start ===");

            try
            {
                float groupOffsetY = ExtractGroupTranslateY(svgText);
                log.AppendLine($"  GroupOffsetY: {groupOffsetY}");

                var circles = ParseCircles(svgText, groupOffsetY);
                var nodeCircles = circles.Where(c => !c.isLegend).ToList();
                log.AppendLine($"  Parsed total circles: {circles.Count}, node circles: {nodeCircles.Count}");

                AssignDepthAndOrder(nodeCircles, log);

                var allTexts = ParseTexts(svgText, groupOffsetY);
                MatchLabels(nodeCircles, allTexts, log);

                var paths = ParsePathEndpoints(svgText, groupOffsetY);
                log.AppendLine($"  Parsed paths: {paths.Count}");

                BuildConnections(nodeCircles, paths, log);

                var slots = BuildSlots(nodeCircles, log);

                report.totalSlotsParsed = slots.Count;
                report.storySlotsParsed = slots.Count(s => s.role == StageMapSlotRole.Story);
                report.randomSlotsParsed = slots.Count(s => s.role == StageMapSlotRole.Random);
                report.totalConnectionsParsed = slots.Sum(s => s.connections?.Count ?? 0);
                report.rawImportLog = log.ToString();

                return new BuildResult { slots = slots, report = report };
            }
            catch (Exception ex)
            {
                report.isSuccess = false;
                report.errorMessages.Add(ex.Message);
                log.AppendLine($"[ERROR] Build failed: {ex}");
                report.rawImportLog = log.ToString();
                return new BuildResult { slots = new List<StageMapSlot>(), report = report };
            }
        }

        private static float ExtractGroupTranslateY(string svgText)
        {
            Match m = Regex.Match(svgText,
                @"<g\s[^>]*transform\s*=\s*""translate\(0,\s*([\d.-]+)\)""", RegexOptions.IgnoreCase);
            if (m.Success && float.TryParse(m.Groups[1].Value, out float val))
            {
                return val;
            }
            return 0f;
        }

        private static List<SvgCircleData> ParseCircles(string svgText, float groupOffsetY)
        {
            var result = new List<SvgCircleData>();

            Match groupMatch = Regex.Match(svgText,
                @"<g\s[^>]*transform\s*=\s*""translate\(", RegexOptions.IgnoreCase);
            int groupStartIndex = groupMatch.Success ? groupMatch.Index : svgText.Length;

            var circleTagRx = new Regex(
                @"<circle\b([^/]*)(?:/>|>)",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            foreach (Match m in circleTagRx.Matches(svgText))
            {
                string attrs = m.Groups[1].Value;
                string cssClass = ExtractAttr(attrs, "class");
                if (string.IsNullOrEmpty(cssClass)) continue;
                string cxStr = ExtractAttr(attrs, "cx");
                string cyStr = ExtractAttr(attrs, "cy");
                if (!float.TryParse(cxStr, System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out float cx)) continue;
                if (!float.TryParse(cyStr, System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out float cyOriginal)) continue;

                bool insideGroup = m.Index > groupStartIndex;
                float cyGlobal = insideGroup ? cyOriginal + groupOffsetY : cyOriginal;

                bool isLegend = !insideGroup;

                result.Add(new SvgCircleData
                {
                    cx = cx,
                    cy = cyGlobal,
                    svgCyOriginal = cyOriginal,
                    cssClass = cssClass,
                    isLegend = isLegend
                });
            }

            return result;
        }

        private static void AssignDepthAndOrder(List<SvgCircleData> circles, StringBuilder log)
        {
            var sorted = circles.OrderByDescending(c => c.cy).ToList();

            var depthGroups = new List<List<SvgCircleData>>();
            foreach (var circle in sorted)
            {
                bool grouped = false;
                foreach (var group in depthGroups)
                {
                    if (Math.Abs(group[0].cy - circle.cy) <= DepthGroupTolerance)
                    {
                        group.Add(circle);
                        grouped = true;
                        break;
                    }
                }
                if (!grouped)
                {
                    depthGroups.Add(new List<SvgCircleData> { circle });
                }
            }

            log.AppendLine($"[Step 3] Depth groups: {depthGroups.Count}");

            for (int d = 0; d < depthGroups.Count; d++)
            {
                var group = depthGroups[d].OrderBy(c => c.cx).ToList();
                float groupY = group[0].cy;

                for (int o = 0; o < group.Count; o++)
                {
                    var circle = group[o];
                    circle.depth = d;
                    circle.orderInDepth = o;
                    circle.slotId = GenerateSlotId(circle, d);
                }

                log.AppendLine(
                    $"  depth {d:D2} (y≈{groupY:F0}): " +
                    string.Join(", ", group.Select(c => $"{c.slotId}({c.cx:F0})")));
            }
        }

        private static string GenerateSlotId(SvgCircleData c, int depth)
        {
            if (StoryClasses.Contains(c.cssClass))
            {
                return $"ep_tmp_{depth}";
            }
            else
            {
                int icx = Mathf.RoundToInt(c.cx);
                int icy = Mathf.RoundToInt(c.svgCyOriginal);
                string yPart = icy < 0 ? $"neg{Math.Abs(icy)}" : $"{icy}";
                return $"slot_{icx}_{yPart}";
            }
        }

        private static List<SvgTextData> ParseTexts(string svgText, float groupOffsetY)
        {
            var result = new List<SvgTextData>();

            Match groupMatch = Regex.Match(svgText,
                @"<g\s[^>]*transform\s*=\s*""translate\(", RegexOptions.IgnoreCase);
            int groupStartIndex = groupMatch.Success ? groupMatch.Index : svgText.Length;

            var textRx = new Regex(
                @"<text\s+(?:[^>]*?\s+)?class=""([^""]+)""(?:[^>]*?\s+)?x=""([^""]+)""(?:[^>]*?\s+)?y=""([^""]+)""[^>]*>([^<]*)</text>",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            foreach (Match m in textRx.Matches(svgText))
            {
                string cssClass = m.Groups[1].Value.Trim();
                if (!float.TryParse(m.Groups[2].Value, out float x)) continue;
                if (!float.TryParse(m.Groups[3].Value, out float y)) continue;
                string content = m.Groups[4].Value.Trim();
                if (string.IsNullOrEmpty(content)) continue;

                bool insideGroup = m.Index > groupStartIndex;
                float yGlobal = insideGroup ? y + groupOffsetY : y;

                result.Add(new SvgTextData
                {
                    x = x,
                    y = yGlobal,
                    cssClass = cssClass,
                    content = content
                });
            }

            return result;
        }

        private static void MatchLabels(
            List<SvgCircleData> nodeCircles,
            List<SvgTextData> allTexts,
            StringBuilder log)
        {
            log.AppendLine("[Step 4] Label matching:");

            foreach (var circle in nodeCircles)
            {
                if (!StoryClasses.Contains(circle.cssClass))
                {
                    circle.label = "?";
                    circle.subLabel = "";
                    continue;
                }

                const float labelYRange = 80f;
                const float xRange = 120f;

                SvgTextData episodeText = allTexts
                    .Where(t => t.cssClass == "label"
                             && Math.Abs(t.x - circle.cx) <= xRange
                             && Math.Abs(t.y - circle.cy) <= labelYRange)
                    .OrderBy(t => Math.Abs(t.y - circle.cy))
                    .FirstOrDefault(t => t.content.StartsWith("Episode", StringComparison.OrdinalIgnoreCase));

                SvgTextData subText = allTexts
                    .Where(t => t.cssClass == "small"
                             && Math.Abs(t.x - circle.cx) <= xRange
                             && Math.Abs(t.y - circle.cy) <= labelYRange)
                    .OrderBy(t => Math.Abs(t.y - circle.cy))
                    .FirstOrDefault();

                circle.label = episodeText?.content ?? $"ep_tmp_d{circle.depth}";
                circle.subLabel = subText?.content ?? "";
                circle.slotId = EpisodeLabelToSlotId(circle.label);

                log.AppendLine($"  {circle.slotId,-22} label=\"{circle.label}\" subLabel=\"{circle.subLabel}\"");
            }
        }

        private static string EpisodeLabelToSlotId(string label)
        {
            Match m = Regex.Match(label,
                @"Episode\s+(\d+)(?:[–\-](\d+))?", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return "ep_unknown";
            }

            string ep = m.Groups[1].Value;
            string sub = m.Groups[2].Value;
            return string.IsNullOrEmpty(sub) ? $"ep_{ep}" : $"ep_{ep}_{sub}";
        }

        private static List<SvgPathEndpoints> ParsePathEndpoints(string svgText, float groupOffsetY)
        {
            var result = new List<SvgPathEndpoints>();

            Match groupMatch = Regex.Match(svgText,
                @"<g\s[^>]*transform\s*=\s*""translate\(", RegexOptions.IgnoreCase);
            int groupStartIndex = groupMatch.Success ? groupMatch.Index : svgText.Length;

            var pathRx = new Regex(
                @"<path\s+(?:[^>]*?\s+)?class=""([^""]+)""(?:[^>]*?\s+)?d=""([^""]+)""",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            foreach (Match m in pathRx.Matches(svgText))
            {
                string cssClass = m.Groups[1].Value.Trim();

                if (!cssClass.Contains("Line") && cssClass != "mergeLine")
                {
                    continue;
                }

                string d = m.Groups[2].Value;
                bool insideGroup = m.Index > groupStartIndex;
                float yOffset = insideGroup ? groupOffsetY : 0f;

                Vector2? startPt = ExtractPathStart(d, yOffset);
                Vector2? endPt = ExtractPathEnd(d, yOffset);

                if (startPt.HasValue && endPt.HasValue)
                {
                    result.Add(new SvgPathEndpoints
                    {
                        start = startPt.Value,
                        end = endPt.Value
                    });
                }
            }

            return result;
        }

        private static Vector2? ExtractPathStart(string d, float yOffset)
        {
            Match m = Regex.Match(d,
                @"M\s*(-?[\d.]+)[,\s]+(-?[\d.]+)", RegexOptions.IgnoreCase);
            if (!m.Success) return null;
            if (!float.TryParse(m.Groups[1].Value,
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out float x)) return null;
            if (!float.TryParse(m.Groups[2].Value,
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out float y)) return null;
            return new Vector2(x, y + yOffset);
        }

        private static Vector2? ExtractPathEnd(string d, float yOffset)
        {
            var coords = Regex.Matches(d, @"(-?[\d.]+)[,\s]+(-?[\d.]+)");
            if (coords.Count < 2) return null;

            var last = coords[coords.Count - 1];
            if (!float.TryParse(last.Groups[1].Value,
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out float x)) return null;
            if (!float.TryParse(last.Groups[2].Value,
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out float y)) return null;

            return new Vector2(x, y + yOffset);
        }

        private static void BuildConnections(
            List<SvgCircleData> circles,
            List<SvgPathEndpoints> paths,
            StringBuilder log)
        {
            log.AppendLine();
            log.AppendLine("─── [Step 6] Build Connections ───");

            var depthYs = circles
                .Select(c => c.cy)
                .Distinct()
                .OrderByDescending(y => y)
                .ToList();

            int matchedPaths = 0;
            int failedPaths  = 0;

            foreach (var path in paths)
            {
                int fromDepth = FindDepthForPoint(path.start.y, depthYs, isStart: true);
                int toDepth   = FindDepthForPoint(path.end.y, depthYs, isStart: false);

                var fromCircle = FindNearestCircleByDepth(path.start.x, fromDepth, circles);
                var toCircle   = FindNearestCircleByDepth(path.end.x, toDepth, circles);

                if (fromCircle != null && toCircle != null)
                {
                    if (fromCircle.cy < toCircle.cy)
                    {
                        var temp = fromCircle;
                        fromCircle = toCircle;
                        toCircle = temp;
                    }

                    if (!fromCircle.connectionTargets.Contains(toCircle.slotId))
                    {
                        fromCircle.connectionTargets.Add(toCircle.slotId);
                    }
                    matchedPaths++;
                }
                else
                {
                    failedPaths++;
                }
            }

            log.AppendLine($"  Connections 빌드 결과: 성공={matchedPaths}, 실패={failedPaths}");
        }

        private static int FindDepthForPoint(float y, List<float> depthYs, bool isStart)
        {
            for (int d = 0; d < depthYs.Count; d++)
            {
                if (Math.Abs(depthYs[d] - y) <= 15f)
                {
                    return d;
                }
            }

            for (int d = 0; d < depthYs.Count - 1; d++)
            {
                float lowerY = depthYs[d];
                float upperY = depthYs[d + 1];

                if (lowerY > y && y > upperY)
                {
                    return isStart ? d : (d + 1);
                }
            }

            if (y > depthYs[0]) return 0;
            return depthYs.Count - 1;
        }

        private static SvgCircleData FindNearestCircleByDepth(
            float x, int depth, List<SvgCircleData> circles)
        {
            var candidates = circles.Where(c => c.depth == depth).ToList();
            if (candidates.Count == 0) return null;

            SvgCircleData best = null;
            float minDist = float.MaxValue;

            foreach (var c in candidates)
            {
                float dist = Math.Abs(c.cx - x);
                if (dist < minDist)
                {
                    minDist = dist;
                    best = c;
                }
            }

            if (minDist > 250f)
            {
                return null;
            }

            return best;
        }

        private static List<StageMapSlot> BuildSlots(List<SvgCircleData> circles, StringBuilder log)
        {
            log.AppendLine();
            log.AppendLine("─── [Step 6-2] Build Slots ───");

            var slots = new List<StageMapSlot>();
            foreach (var c in circles)
            {
                var role = StoryClasses.Contains(c.cssClass) ? StageMapSlotRole.Story : StageMapSlotRole.Random;
                var slot = new StageMapSlot
                {
                    slotId = c.slotId,
                    role = role,
                    depth = c.depth,
                    orderInDepth = c.orderInDepth,
                    label = c.label,
                    subLabel = c.subLabel,
                    connections = c.connectionTargets
                        .Select(t => new StageSlotConnection { toSlotId = t })
                        .ToList()
                };
                slots.Add(slot);
            }

            return slots;
        }

        private static string ExtractAttr(string attrs, string attrName)
        {
            Match m = Regex.Match(attrs,
                attrName + @"\s*=\s*""([^""]+)""", RegexOptions.IgnoreCase);
            return m.Success ? m.Groups[1].Value.Trim() : "";
        }
    }
}
