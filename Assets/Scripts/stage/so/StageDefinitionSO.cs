using System.Collections.Generic;
using UnityEngine;

namespace Stage
{
    [System.Serializable]
    public class StageRequiredNode
    {
        public RoundNodeSO node;
        public int depth;
        public int column;
        public bool hiddenByDefault;
        public string routeKey;
        public StageNodeKind kind = StageNodeKind.RouteNode;
    }

    /// <summary>
    /// 스테이지 생성에 필요한 정의 데이터 (디자인 타임)
    /// </summary>
    [CreateAssetMenu(menuName = "Stage/Stage Definition")]
    public class StageDefinitionSO : ScriptableObject
    {
        [Header("Identity")]
        public string stageId;
        public string stageName;

        [Header("Stage Map Generation")]
        [Tooltip("스테이지 맵 생성 방식입니다. 기본값은 신규 routeKey 기반 randomRouteRules 방식입니다.")]
        public StageMapGenerationMode generationMode = StageMapGenerationMode.RouteKeyRandomRules;

        [Header("Segments")]
        [Tooltip("핵심 노드 사이에 생성할 랜덤 구간 규칙")]
        public List<StageSegmentRule> segmentRules = new();

        [Header("Random Route Rules")]
        [Tooltip("routeKey 기반 랜덤 branch 노드 생성 규칙. 기존 segmentRules를 대체하는 신규 생성 규칙입니다.")]
        public List<StageRandomRouteRule> randomRouteRules = new();

        [Header("Required / Main Story")]
        [Tooltip("무조건 지나가야 하는 메인 스토리 노드. StageDefinitionSO에서 depth/column으로 배치한다.")]
        public List<StageRequiredNode> requiredSubEvents = new();


        [Header("Debug")]
        public bool useFixedSeed = false;
        public int seed = 0;

#if UNITY_EDITOR
        /// <summary>
        /// 에디터에서 SO 값이 변경될 때 빈 routeKey를 자동으로 채운다.
        /// 이미 routeKey가 있는 노드는 덮어쓰지 않는다.
        /// 전체 재생성이 필요하면 우클릭 → "Auto Fill Route Keys (Force Reset All)" 사용.
        /// </summary>
        private void OnValidate()
        {
            FillEmptyRouteKeys();

            if (randomRouteRules != null)
            {
                foreach (var rule in randomRouteRules)
                {
                    if (rule == null) continue;
                    if (rule.minBranchesPerRoute < 1)
                    {
                        rule.minBranchesPerRoute = 1;
                    }
                    if (rule.maxBranchesPerRoute < rule.minBranchesPerRoute)
                    {
                        rule.maxBranchesPerRoute = rule.minBranchesPerRoute;
                    }
                }
            }
        }

        /// <summary>
        /// depth + column 계층 구조를 분석하여 모든 routeKey를 강제로 재생성한다.
        /// 기존 routeKey를 모두 초기화하고 계층형 키로 덮어쓴다.
        /// </summary>
        [ContextMenu("Auto Fill Route Keys (Force Reset All)")]
        private void AutoFillAllRouteKeys()
        {
            if (requiredSubEvents == null)
            {
                return;
            }

            // 기존 routeKey 전체 초기화
            foreach (StageRequiredNode node in requiredSubEvents)
            {
                if (node != null)
                {
                    node.routeKey = string.Empty;
                }
            }

            FillEmptyRouteKeys();

            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log("[StageDefinitionSO] Route keys fully regenerated from depth + column.");
        }

        /// <summary>
        /// routeKey가 비어 있는 노드만 계층형 키로 채운다.
        /// 이미 값이 있는 노드는 건드리지 않는다.
        /// </summary>
        private void FillEmptyRouteKeys()
        {
            if (requiredSubEvents == null)
            {
                return;
            }

            // depth별로 그룹화
            Dictionary<int, List<StageRequiredNode>> nodesByDepth = new Dictionary<int, List<StageRequiredNode>>();

            foreach (StageRequiredNode node in requiredSubEvents)
            {
                if (node == null)
                {
                    continue;
                }

                if (!nodesByDepth.ContainsKey(node.depth))
                {
                    nodesByDepth[node.depth] = new List<StageRequiredNode>();
                }

                nodesByDepth[node.depth].Add(node);
            }

            // 각 depth를 column 오름차순 정렬
            foreach (List<StageRequiredNode> group in nodesByDepth.Values)
            {
                group.Sort((a, b) => a.column.CompareTo(b.column));
            }

            // depth를 오름차순으로 정렬
            List<int> sortedDepths = new List<int>(nodesByDepth.Keys);
            sortedDepths.Sort();

            // 이전 depth의 effective routeKey 목록 (GlobalHubNode는 "" 로 포함)
            List<string> prevEffectiveKeys = new List<string>();

            foreach (int depth in sortedDepths)
            {
                List<StageRequiredNode> nodes = nodesByDepth[depth];
                List<string> currentEffectiveKeys = new List<string>();

                for (int i = 0; i < nodes.Count; i++)
                {
                    StageRequiredNode node = nodes[i];

                    // GlobalHubNode: routeKey 비어 있음 허용, tracking은 "" 로
                    if (node.kind == StageNodeKind.GlobalHubNode)
                    {
                        currentEffectiveKeys.Add(string.Empty);
                        continue;
                    }

                    // 이미 routeKey 있음: 덮어쓰지 않고 그대로 tracking
                    if (!string.IsNullOrWhiteSpace(node.routeKey))
                    {
                        currentEffectiveKeys.Add(node.routeKey);
                        continue;
                    }

                    // routeKey 비어 있음: 계층 알고리즘으로 계산
                    string computed = ComputeHierarchicalRouteKey(i, nodes.Count, prevEffectiveKeys);
                    node.routeKey = computed;
                    currentEffectiveKeys.Add(computed);
                }

                prevEffectiveKeys = currentEffectiveKeys;
            }
        }

        /// <summary>
        /// 현재 depth에서 rank 위치의 노드 routeKey를 계층 구조로 계산한다.
        /// </summary>
        private static string ComputeHierarchicalRouteKey(
            int rank,
            int countInDepth,
            List<string> prevKeys)
        {
            List<string> effectiveParentKeys = new List<string>();
            foreach (string k in prevKeys)
            {
                if (!string.IsNullOrEmpty(k))
                {
                    effectiveParentKeys.Add(k);
                }
            }

            if (effectiveParentKeys.Count == 0)
            {
                return (rank + 1).ToString();
            }

            if (countInDepth <= prevKeys.Count)
            {
                int parentIdx = Mathf.Min(rank, effectiveParentKeys.Count - 1);
                return effectiveParentKeys[parentIdx];
            }
            else
            {
                int nodesPerParent = Mathf.CeilToInt((float)countInDepth / effectiveParentKeys.Count);
                int parentIdx = Mathf.Min(rank / nodesPerParent, effectiveParentKeys.Count - 1);
                int localSubIdx = rank % nodesPerParent + 1;
                return effectiveParentKeys[parentIdx] + "." + localSubIdx;
            }
        }
#endif
    }
}
