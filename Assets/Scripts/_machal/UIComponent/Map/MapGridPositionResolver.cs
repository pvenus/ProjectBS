using System.Collections.Generic;
using UnityEngine;
using Stage;

namespace UIFramework.Map
{
    public enum GridPlacementMode
    {
        Simple,
        Advanced
    }

    [System.Serializable]
    public struct MapGridSettings
    {
        public GridPlacementMode gridPlacementMode;
        public int gridColumnCount;
        public Vector2 gridCellSize;
        public Vector2 gridOrigin;
        public bool centerRoutesInGrid;
        public bool applyRandomOffsetInCell;
        public Vector2 randomOffsetRange;
        public bool growUpwards;
    }

    public static class MapGridPositionResolver
    {
        #region RouteKey Helper Functions
        /// <summary>
        /// routeKey를 "." 단위로 쪼개어 정수 리스트로 파싱합니다.
        /// </summary>
        public static List<int> ParseRouteKey(string routeKey)
        {
            List<int> segments = new List<int>();
            if (string.IsNullOrEmpty(routeKey))
            {
                return segments;
            }

            string[] tokens = routeKey.Split('.');
            foreach (string token in tokens)
            {
                if (int.TryParse(token, out int val))
                {
                    segments.Add(val);
                }
                else
                {
                    segments.Add(0);
                }
            }

            return segments;
        }

        /// <summary>
        /// 두 routeKey의 세그먼트별 숫자 크기를 비교합니다.
        /// </summary>
        public static int CompareRouteKey(string x, string y)
        {
            if (x == y) return 0;
            
            bool xEmpty = string.IsNullOrWhiteSpace(x);
            bool yEmpty = string.IsNullOrWhiteSpace(y);
            if (xEmpty && yEmpty) return 0;
            if (xEmpty) return 1; // 빈 값은 뒤로
            if (yEmpty) return -1;

            List<int> xSegments = ParseRouteKey(x);
            List<int> ySegments = ParseRouteKey(y);

            int minLength = Mathf.Min(xSegments.Count, ySegments.Count);
            for (int i = 0; i < minLength; i++)
            {
                if (xSegments[i] != ySegments[i])
                {
                    return xSegments[i].CompareTo(ySegments[i]);
                }
            }

            return xSegments.Count.CompareTo(ySegments.Count);
        }

        /// <summary>
        /// childRoute가 parentRoute의 하위 경로인지 확인합니다.
        /// </summary>
        public static bool IsRouteDescendant(string childRoute, string parentRoute)
        {
            if (string.IsNullOrEmpty(parentRoute))
            {
                return true;
            }

            if (string.IsNullOrEmpty(childRoute))
            {
                return false;
            }

            if (childRoute == parentRoute)
            {
                return true;
            }

            return childRoute.StartsWith(parentRoute + ".");
        }
        #endregion

        /// <summary>
        /// 주어진 StageGraph와 설정값을 바탕으로 노드들의 그리드 좌표를 계산하여 반환합니다.
        /// </summary>
        public static Dictionary<string, Vector2> CalculateGridNodePositions(StageGraph graph, MapGridSettings settings, System.Random rng)
        {
            Dictionary<string, Vector2> positions = new Dictionary<string, Vector2>();
            if (graph == null || graph.nodes == null)
            {
                return positions;
            }

            if (settings.gridPlacementMode == GridPlacementMode.Advanced)
            {
                CalculateGridNodePositions_Advanced(graph, settings, rng, positions);
            }
            else
            {
                CalculateGridNodePositions_Simple(graph, settings, rng, positions);
            }

            return positions;
        }

        private static void CalculateGridNodePositions_Simple(
            StageGraph graph, 
            MapGridSettings settings, 
            System.Random rng, 
            Dictionary<string, Vector2> positions)
        {
            float ySign = settings.growUpwards ? 1f : -1f;
            List<int> depths = graph.GetDepths();
            int M = settings.gridColumnCount;

            foreach (int depth in depths)
            {
                List<RoundNode> nodes = graph.GetNodesByDepth(depth);
                if (nodes == null || nodes.Count == 0)
                    continue;

                int K = nodes.Count;
                if (M > 0 && K > M)
                {
                    Debug.LogWarning(
                        $"[MapGridPositionResolver] Depth {depth} has {K} nodes, " +
                        $"which exceeds gridColumnCount {M}.");
                }

                // CenterRoutesInGrid에 따른 시작 열 인덱스 계산
                float startCol = 0f;
                if (settings.centerRoutesInGrid)
                {
                    startCol = (M - K) * 0.5f;
                }

                for (int i = 0; i < K; i++)
                {
                    RoundNode node = nodes[i];
                    float col = startCol + i;

                    float x = settings.gridOrigin.x + col * settings.gridCellSize.x;
                    float y = settings.gridOrigin.y + depth * settings.gridCellSize.y * ySign;

                    if (settings.applyRandomOffsetInCell)
                    {
                        float ox = (float)(rng.NextDouble() * 2.0 - 1.0) * settings.randomOffsetRange.x;
                        float oy = (float)(rng.NextDouble() * 2.0 - 1.0) * settings.randomOffsetRange.y;
                        x += ox;
                        y += oy;
                    }

                    positions[node.nodeId] = new Vector2(x, y);
                    Debug.Log($"[CalculateGridNodePositions_Simple] Node: {node.nodeId}, Depth: {depth}, Col: {col} -> Pos: ({x}, {y})");
                }
            }
        }

        private static void CalculateGridNodePositions_Advanced(
            StageGraph graph, 
            MapGridSettings settings, 
            System.Random rng, 
            Dictionary<string, Vector2> positions)
        {
            // 1. active leaf route 수집
            HashSet<string> allRouteKeys = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            foreach (RoundNode node in graph.nodes)
            {
                if (node == null) continue;
                if (!string.IsNullOrWhiteSpace(node.routeKey) &&
                    (node.kind == StageNodeKind.RouteNode || node.kind == StageNodeKind.RouteHubNode))
                {
                    allRouteKeys.Add(node.routeKey);
                }
            }

            List<string> leafRoutes = new List<string>();
            foreach (string route in allRouteKeys)
            {
                bool isParent = false;
                foreach (string other in allRouteKeys)
                {
                    if (other != route && IsRouteDescendant(other, route))
                    {
                        isParent = true;
                        break;
                    }
                }
                if (!isParent)
                {
                    leafRoutes.Add(route);
                }
            }

            // routeKey 숫자 세그먼트 기준으로 정렬
            leafRoutes.Sort(CompareRouteKey);
            Debug.Log($"[CalculateGridNodePositions_Advanced] leafRoutes (Count={leafRoutes.Count}): {string.Join(", ", leafRoutes)}");

            // 2. leaf route를 grid column에 매핑
            int N = leafRoutes.Count;
            int M = settings.gridColumnCount;
            Dictionary<string, float> leafRouteToColumn = new Dictionary<string, float>();

            if (N > 0)
            {
                float startCol = 0f;
                float colStep = 1f;

                if (M >= N)
                {
                    if (settings.centerRoutesInGrid)
                    {
                        startCol = (M - N) * 0.5f;
                    }
                    else
                    {
                        startCol = 0f;
                    }
                    colStep = 1f;
                }
                else
                {
                    Debug.LogWarning($"[MapGridPositionResolver] Leaf route count ({N}) exceeds gridColumnCount ({M}). Compressing lanes.");
                    startCol = 0f;
                    colStep = (N > 1) ? (float)(M - 1) / (N - 1) : 1f;
                }

                for (int i = 0; i < N; i++)
                {
                    leafRouteToColumn[leafRoutes[i]] = startCol + i * colStep;
                    Debug.Log($"[CalculateGridNodePositions_Advanced] Mapping: '{leafRoutes[i]}' -> column={leafRouteToColumn[leafRoutes[i]]}");
                }
            }

            // 3. 각 노드의 row 및 column 결정
            float ySign = settings.growUpwards ? 1f : -1f;

            foreach (RoundNode node in graph.nodes)
            {
                if (node == null) continue;

                int row = node.depth;
                float column = 0f;

                if (node.kind == StageNodeKind.GlobalHubNode)
                {
                    // GlobalHubNode: 모든 leaf route column들의 평균
                    if (N > 0)
                    {
                        float sum = 0f;
                        foreach (string leaf in leafRoutes)
                        {
                            sum += leafRouteToColumn[leaf];
                        }
                        column = sum / N;
                    }
                    else
                    {
                        column = (M - 1) * 0.5f;
                    }
                }
                else if (node.kind == StageNodeKind.RouteHubNode)
                {
                    // RouteHubNode: 해당 routeKey 하위 leaf route들의 column 평균
                    List<string> descendants = new List<string>();
                    if (!string.IsNullOrWhiteSpace(node.routeKey))
                    {
                        foreach (string leaf in leafRoutes)
                        {
                            if (IsRouteDescendant(leaf, node.routeKey))
                            {
                                descendants.Add(leaf);
                            }
                        }
                    }

                    if (descendants.Count > 0)
                    {
                        float sum = 0f;
                        foreach (string desc in descendants)
                        {
                            sum += leafRouteToColumn[desc];
                        }
                        column = sum / descendants.Count;
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(node.routeKey) && leafRouteToColumn.TryGetValue(node.routeKey, out float col))
                        {
                            column = col;
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(node.routeKey))
                            {
                                Debug.LogWarning($"[MapGridPositionResolver] RouteHubNode '{node.nodeId}' has empty routeKey. Fallback to center column.");
                            }
                            else
                            {
                                Debug.LogWarning($"[MapGridPositionResolver] RouteHubNode '{node.nodeId}' has routeKey '{node.routeKey}' which has no mapping. Fallback to center column.");
                            }
                            column = (M - 1) * 0.5f;
                        }
                    }
                }
                else // RouteNode
                {
                    if (string.IsNullOrWhiteSpace(node.routeKey))
                    {
                        Debug.LogWarning($"[MapGridPositionResolver] RouteNode '{node.nodeId}' has empty routeKey. Fallback to column 0.");
                        column = 0f;
                    }
                    else if (leafRouteToColumn.TryGetValue(node.routeKey, out float col))
                    {
                        column = col;
                    }
                    else
                    {
                        // leaf는 아니지만 descendants 검색 시도
                        List<string> descendants = new List<string>();
                        foreach (string leaf in leafRoutes)
                        {
                            if (IsRouteDescendant(leaf, node.routeKey))
                            {
                                descendants.Add(leaf);
                            }
                        }

                        if (descendants.Count > 0)
                        {
                            float sum = 0f;
                            foreach (string desc in descendants)
                            {
                                sum += leafRouteToColumn[desc];
                            }
                            column = sum / descendants.Count;
                        }
                        else
                        {
                            Debug.LogWarning($"[MapGridPositionResolver] RouteNode '{node.nodeId}' (routeKey: '{node.routeKey}') could not be mapped to any leaf route. Fallback to column 0.");
                            column = 0f;
                        }
                    }
                }

                // 4. anchoredPosition 계산
                float x = settings.gridOrigin.x + column * settings.gridCellSize.x;
                float y = settings.gridOrigin.y + row * settings.gridCellSize.y * ySign;

                if (settings.applyRandomOffsetInCell)
                {
                    float ox = (float)(rng.NextDouble() * 2 - 1) * settings.randomOffsetRange.x;
                    float oy = (float)(rng.NextDouble() * 2 - 1) * settings.randomOffsetRange.y;
                    x += ox;
                    y += oy;
                }

                Debug.Log($"[CalculateGridNodePositions_Advanced] Node: {node.nodeId}, Kind: {node.kind}, RouteKey: '{node.routeKey}', Depth: {node.depth} -> Column: {column} -> Pos: ({x}, {y})");
                positions[node.nodeId] = new Vector2(x, y);
            }
        }
    }
}
