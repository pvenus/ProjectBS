using System.Collections.Generic;
using UnityEngine;
using Stage;

namespace Stage.UI
{
    public class StageNodeMapUIGenerator
    {
        private readonly Transform contentRoot;
        private readonly Transform pathRoot;
        private readonly RoundNodeButton nodeButtonPrefab;
        private readonly GameObject pathSegmentPrefab;
        private readonly float pathSegmentSpacing;
        private readonly float pathSegmentPositionNoiseX;
        private readonly float nodeExclusionRadius;

        public StageNodeMapUIGenerator(
            Transform contentRoot,
            Transform pathRoot,
            RoundNodeButton nodeButtonPrefab,
            GameObject pathSegmentPrefab,
            float pathSegmentSpacing,
            float pathSegmentPositionNoiseX,
            float nodeExclusionRadius = 0f)
        {
            this.contentRoot = contentRoot;
            this.pathRoot = pathRoot;
            this.nodeButtonPrefab = nodeButtonPrefab;
            this.pathSegmentPrefab = pathSegmentPrefab;
            this.pathSegmentSpacing = pathSegmentSpacing;
            this.pathSegmentPositionNoiseX = pathSegmentPositionNoiseX;
            this.nodeExclusionRadius = nodeExclusionRadius;
        }

        public void BuildFromStageGraph(
            StageGraph graph,
            Dictionary<string, Vector2> nodeUIPositions,
            System.Random rng,
            List<RoundNodeButton> spawnedButtons,
            Dictionary<string, RoundNodeButton> buttonMap,
            List<GameObject> spawnedPathViews)
        {
            if (graph == null) return;

            CreatePathViews(graph, nodeUIPositions, rng, spawnedPathViews);
            CreateNodeViews(graph, nodeUIPositions, spawnedButtons, buttonMap);
        }

        public void BuildSlotPreview(
            List<StageMapSlot> slots,
            Dictionary<string, Vector2> nodeUIPositions,
            System.Random rng,
            List<RoundNodeButton> spawnedButtons,
            Dictionary<string, RoundNodeButton> buttonMap,
            List<GameObject> spawnedPathViews)
        {
            if (slots == null) return;

            CreateSlotPathViews(slots, nodeUIPositions, rng, spawnedPathViews);
            CreateSlotNodeViews(slots, nodeUIPositions, spawnedButtons, buttonMap);
        }

        private void CreatePathViews(
            StageGraph graph,
            Dictionary<string, Vector2> nodePositions,
            System.Random rng,
            List<GameObject> spawnedPathViews)
        {
            if (pathSegmentPrefab == null || pathRoot == null) return;

            var drawnConnections = new HashSet<string>();

            foreach (var node in graph.nodes)
            {
                if (node == null || !nodePositions.TryGetValue(node.nodeId, out Vector2 startPos)) continue;
                if (node.nextNodeIds == null) continue;

                foreach (var childId in node.nextNodeIds)
                {
                    if (string.IsNullOrEmpty(childId) || !nodePositions.TryGetValue(childId, out Vector2 endPos)) continue;

                    string key = string.CompareOrdinal(node.nodeId, childId) < 0
                        ? $"{node.nodeId}->{childId}"
                        : $"{childId}->{node.nodeId}";

                    if (!drawnConnections.Add(key)) continue;

                    CreateConnectionView(startPos, endPos, rng, spawnedPathViews);
                }
            }
        }

        private void CreateSlotPathViews(
            List<StageMapSlot> slots,
            Dictionary<string, Vector2> nodePositions,
            System.Random rng,
            List<GameObject> spawnedPathViews)
        {
            if (pathSegmentPrefab == null || pathRoot == null) return;

            var drawnConnections = new HashSet<string>();

            foreach (var slot in slots)
            {
                if (slot == null || !nodePositions.TryGetValue(slot.slotId, out Vector2 startPos)) continue;
                if (slot.connections == null) continue;

                foreach (var conn in slot.connections)
                {
                    string targetId = conn?.toSlotId;
                    if (string.IsNullOrEmpty(targetId) || !nodePositions.TryGetValue(targetId, out Vector2 endPos)) continue;

                    string key = string.CompareOrdinal(slot.slotId, targetId) < 0
                        ? $"{slot.slotId}->{targetId}"
                        : $"{targetId}->{slot.slotId}";

                    if (!drawnConnections.Add(key)) continue;

                    CreateConnectionView(startPos, endPos, rng, spawnedPathViews);
                }
            }
        }

        public void CreateConnectionView(Vector2 start, Vector2 end, System.Random rng, List<GameObject> spawnedPathViews)
        {
            float distance = Vector2.Distance(start, end);
            int segmentCount = Mathf.FloorToInt(distance / pathSegmentSpacing);

            Vector2 dir = (end - start).normalized;
            Vector2 perpDir = new Vector2(-dir.y, dir.x); // 경로에 직교하는 횡방향 벡터

            for (int i = 1; i < segmentCount; i++)
            {
                float t = (float)i / segmentCount;
                Vector2 basePos = Vector2.Lerp(start, end, t);

                // 노드 경계 범위 내 세그먼트 스킵
                if (nodeExclusionRadius > 0f)
                {
                    float distFromStart = Vector2.Distance(basePos, start);
                    float distFromEnd   = Vector2.Distance(basePos, end);
                    if (distFromStart < nodeExclusionRadius || distFromEnd < nodeExclusionRadius) continue;
                }

                // 횡방향(perpendicular) X 오프셋 노이즈
                float noiseX = (rng != null) ? (float)(rng.NextDouble() * 2 - 1) * pathSegmentPositionNoiseX : 0f;
                Vector2 pos = basePos + perpDir * noiseX;

                GameObject segment = Object.Instantiate(pathSegmentPrefab, pathRoot);

                // 나중에 생성된 path가 먼저 생성된 것 뒤(아래)에 렌더링되도록 첫 번째 자식으로 배치
                segment.transform.SetAsFirstSibling();

                RectTransform rect = segment.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(0.5f, 0f);
                    rect.anchorMax = new Vector2(0.5f, 0f);
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.anchoredPosition = pos;
                }

                // 비주얼 하위 프리팹 무작위 생성
                UIPathSegment pathSeg = segment.GetComponent<UIPathSegment>()
                    ?? segment.GetComponentInChildren<UIPathSegment>()
                    ?? segment.AddComponent<UIPathSegment>();

                pathSeg.ApplyRandomVisual(null, rng);

                spawnedPathViews.Add(segment);
            }
        }

        private void CreateNodeViews(
            StageGraph graph,
            Dictionary<string, Vector2> nodePositions,
            List<RoundNodeButton> spawnedButtons,
            Dictionary<string, RoundNodeButton> buttonMap)
        {
            if (nodeButtonPrefab == null || contentRoot == null) return;

            foreach (var node in graph.nodes)
            {
                if (node == null || !nodePositions.TryGetValue(node.nodeId, out Vector2 pos)) continue;

                RoundNodeButton button = CreateNodeView(node, pos);
                if (button != null)
                {
                    spawnedButtons.Add(button);
                    buttonMap[node.nodeId] = button;
                }
            }
        }

        private void CreateSlotNodeViews(
            List<StageMapSlot> slots,
            Dictionary<string, Vector2> nodePositions,
            List<RoundNodeButton> spawnedButtons,
            Dictionary<string, RoundNodeButton> buttonMap)
        {
            if (nodeButtonPrefab == null || contentRoot == null) return;

            foreach (var slot in slots)
            {
                if (slot == null || !nodePositions.TryGetValue(slot.slotId, out Vector2 pos)) continue;

                RoundNodeButton button = CreateNodeView(null, pos);
                if (button != null)
                {
                    spawnedButtons.Add(button);
                    buttonMap[slot.slotId] = button;
                }
            }
        }

        public RoundNodeButton CreateNodeView(RoundNode node, Vector2 pos)
        {
            RoundNodeButton button = Object.Instantiate(nodeButtonPrefab, contentRoot);
            RectTransform rect = button.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.5f, 0f);
                rect.anchorMax = new Vector2(0.5f, 0f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = pos;
            }

            if (node != null)
            {
                button.Initialize(node);
            }

            return button;
        }

        public void Clear(
            List<RoundNodeButton> spawnedButtons,
            List<GameObject> spawnedPathViews,
            Dictionary<string, RoundNodeButton> buttonMap)
        {
            foreach (var btn in spawnedButtons)
            {
                if (btn != null) Object.Destroy(btn.gameObject);
            }
            spawnedButtons.Clear();
            buttonMap.Clear();

            foreach (var path in spawnedPathViews)
            {
                if (path != null) Object.Destroy(path);
            }
            spawnedPathViews.Clear();
        }
    }
}
