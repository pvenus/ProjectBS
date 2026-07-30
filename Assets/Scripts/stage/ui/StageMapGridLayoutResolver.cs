using System.Collections.Generic;
using UnityEngine;

namespace Stage.UI
{
    public class StageMapGridLayoutResolver
    {
        public Dictionary<string, Vector2> ResolveAllPositions(
            Stage.StageGraph graph,
            bool useGridLayout,
            MapGridSettings gridSettings,
            float horizontalSpacing,
            float verticalSpacing,
            Vector2 startPosition,
            bool growUpwards,
            float randomOffsetX,
            float randomOffsetY,
            bool useFixedSeed,
            int randomSeed)
        {
            Dictionary<string, Vector2> positions = new Dictionary<string, Vector2>(System.StringComparer.OrdinalIgnoreCase);
            if (graph == null || graph.nodes == null) return positions;

            System.Random rng = useFixedSeed ? new System.Random(randomSeed) : new System.Random();

            if (useGridLayout)
            {
                // 기존 MapGridPositionResolver의 연산을 그대로 수행
                return MapGridPositionResolver.CalculateGridNodePositions(graph, gridSettings, rng);
            }

            // Legacy 배치 방식 연산
            List<int> depths = graph.GetDepths();
            foreach (int depth in depths)
            {
                var nodes = graph.GetNodesByDepth(depth);
                if (nodes == null || nodes.Count == 0) continue;

                float totalWidth = (nodes.Count - 1) * horizontalSpacing;
                float startX = startPosition.x - totalWidth * 0.5f;

                float ySign = growUpwards ? 1f : -1f;
                float y = startPosition.y + (depth * verticalSpacing * ySign);

                for (int i = 0; i < nodes.Count; i++)
                {
                    var node = nodes[i];
                    float x = startX + i * horizontalSpacing;

                    float ox = (float)(rng.NextDouble() * 2 - 1) * randomOffsetX;
                    float oy = (float)(rng.NextDouble() * 2 - 1) * randomOffsetY;

                    positions[node.nodeId] = new Vector2(x + ox, y + oy);
                }
            }

            return positions;
        }

        // 특정 단일 노드 배치 계산 (개별 확인용)
        public Vector2 ResolvePosition(
            int depth,
            int orderInDepth,
            int totalDepthNodeCount,
            float horizontalSpacing,
            float verticalSpacing,
            Vector2 startPosition,
            bool growUpwards,
            float randomOffsetX,
            float randomOffsetY,
            System.Random rng)
        {
            float totalWidth = (totalDepthNodeCount - 1) * horizontalSpacing;
            float startX = startPosition.x - totalWidth * 0.5f;

            float ySign = growUpwards ? 1f : -1f;
            float y = startPosition.y + (depth * verticalSpacing * ySign);

            float x = startX + orderInDepth * horizontalSpacing;

            float ox = rng != null ? (float)(rng.NextDouble() * 2 - 1) * randomOffsetX : 0f;
            float oy = rng != null ? (float)(rng.NextDouble() * 2 - 1) * randomOffsetY : 0f;

            return new Vector2(x + ox, y + oy);
        }
    }
}
