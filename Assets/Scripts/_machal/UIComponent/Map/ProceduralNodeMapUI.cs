using System.Collections.Generic;
using UnityEngine;
using Stage; // 실제 게임의 StageGraph, RoundNode 등을 사용
using Stage.UI; // RoundNodeButton 사용

namespace UIFramework.Map
{
    public class ProceduralNodeMapUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private StageManager stageManager;
        public RectTransform contentRoot;
        [Tooltip("비워두면 contentRoot를 사용합니다. 노드 뒤에 길을 그리기 위해 분리하는 것을 권장합니다.")]
        public RectTransform pathRoot;
        
        [Header("Prefabs")]
        [Tooltip("실제 게임의 RoundNodeButton 프리팹을 할당하세요.")]
        public RoundNodeButton nodeButtonPrefab;
        [Tooltip("길을 표시할 세그먼트 프리팹 (점선 느낌의 작은 이미지)")]
        public GameObject pathSegmentPrefab;
        
        [Header("UI Spacing & Offset")]
        public float horizontalSpacing = 150f;
        public float verticalSpacing = 180f;
        public float randomOffsetX = 25f;
        public float randomOffsetY = 25f;
        public Vector2 startPosition = Vector2.zero;
        [Tooltip("Slay the Spire처럼 위로 올라가는 형태라면 체크, 아래로 내려가면 해제")]
        public bool growUpwards = true;
        
        [Header("Path Visuals")]
        public float pathSegmentSpacing = 25f;
        public float pathSegmentRotationNoise = 15f;
        
        [Header("Randomization")]
        public bool useFixedSeed = false;
        public int randomSeed = 0;

        [Header("Options")]
        [SerializeField] private bool rebuildOnEnable = true;

        // Runtime Data
        private readonly List<RoundNodeButton> spawnedButtons = new();
        private readonly Dictionary<string, RoundNodeButton> buttonMap = new();
        private readonly List<GameObject> spawnedPathViews = new();
        private Dictionary<string, Vector2> nodeUIPositions = new();

        private void Awake()
        {
            if (stageManager == null) stageManager = StageManager.Instance;
            if (pathRoot == null) pathRoot = contentRoot;
        }

        private void OnEnable()
        {
            if (stageManager == null) stageManager = StageManager.Instance;
            Subscribe();
            
            if (rebuildOnEnable && stageManager != null && stageManager.HasGraph())
            {
                Build(stageManager.CurrentGraph);
            }
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        /// <summary>
        /// StageGraph 데이터를 받아 맵 UI를 생성합니다.
        /// </summary>
        public void Build(StageGraph graph)
        {
            Clear();

            if (graph == null) return;
            if (contentRoot == null || nodeButtonPrefab == null)
            {
                Debug.LogWarning("[ProceduralNodeMapUI] Build 실패: 필수 Reference가 누락되었습니다.");
                return;
            }

            if (pathRoot == null) pathRoot = contentRoot;

            System.Random rng = useFixedSeed ? new System.Random(randomSeed) : new System.Random();

            // 1. 노드들의 UI 좌표 계산 (가운데 정렬 + 랜덤 오프셋 적용)
            CalculateNodePositions(graph, rng);

            // 2. 길(Path Segments) 생성 (노드보다 먼저/뒤에 그려지도록)
            CreatePathViews(graph, rng);

            // 3. 실제 노드(버튼) 생성
            CreateNodeViews(graph);

            RefreshAll();
        }
        
        private void CalculateNodePositions(StageGraph graph, System.Random rng)
        {
            nodeUIPositions.Clear();
            List<int> depths = graph.GetDepths();
            
            foreach (int depth in depths)
            {
                List<RoundNode> nodes = graph.GetNodesByDepth(depth);
                if (nodes.Count == 0) continue;

                // 해당 깊이의 노드들을 가운데 정렬하기 위한 시작 x 좌표
                float totalWidth = (nodes.Count - 1) * horizontalSpacing;
                float startX = startPosition.x - totalWidth * 0.5f;
                
                // 방향에 따라 y 좌표 계산
                float ySign = growUpwards ? 1f : -1f;
                float y = startPosition.y + (depth * verticalSpacing * ySign);
                
                for (int i = 0; i < nodes.Count; i++)
                {
                    RoundNode node = nodes[i];
                    float x = startX + i * horizontalSpacing;
                    
                    // 자연스러운 배치를 위한 랜덤 오프셋 적용
                    float ox = (float)(rng.NextDouble() * 2 - 1) * randomOffsetX;
                    float oy = (float)(rng.NextDouble() * 2 - 1) * randomOffsetY;
                    
                    nodeUIPositions[node.nodeId] = new Vector2(x + ox, y + oy);
                }
            }
        }

        private void CreatePathViews(StageGraph graph, System.Random rng)
        {
            if (pathSegmentPrefab == null || pathRoot == null) return;

            foreach (var node in graph.nodes)
            {
                if (!nodeUIPositions.TryGetValue(node.nodeId, out Vector2 start)) continue;

                // 다음 노드들과의 연결선 그리기
                List<RoundNode> nextNodes = graph.GetNextNodes(node);
                foreach (var nextNode in nextNodes)
                {
                    if (!nodeUIPositions.TryGetValue(nextNode.nodeId, out Vector2 end)) continue;

                    float distance = Vector2.Distance(start, end);
                    int segmentCount = Mathf.FloorToInt(distance / pathSegmentSpacing);
                    
                    Vector2 dir = (end - start).normalized;
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

                    for (int i = 1; i < segmentCount; i++) // 시작점과 끝점 제외
                    {
                        float t = (float)i / segmentCount;
                        Vector2 pos = Vector2.Lerp(start, end, t);
                        
                        GameObject segment = Instantiate(pathSegmentPrefab, pathRoot);
                        RectTransform rect = segment.GetComponent<RectTransform>();
                        rect.localPosition = new Vector3(pos.x, pos.y, 0f);
                        
                        // 방향에 맞게 회전 + 무작위 오차
                        float noise = (float)(rng.NextDouble() * 2 - 1) * pathSegmentRotationNoise;
                        rect.localRotation = Quaternion.Euler(0, 0, angle + noise);
                        
                        spawnedPathViews.Add(segment);
                    }
                }
            }
        }

        private void CreateNodeViews(StageGraph graph)
        {
            foreach (var node in graph.nodes)
            {
                if (!nodeUIPositions.TryGetValue(node.nodeId, out Vector2 pos)) continue;

                RoundNodeButton button = Instantiate(nodeButtonPrefab, contentRoot);
                RectTransform rectTransform = button.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.localPosition = new Vector3(pos.x, pos.y, 0f);
                }

                button.Initialize(node);
                spawnedButtons.Add(button);
                buttonMap[node.nodeId] = button;
            }
        }

        public void RefreshAll()
        {
            foreach (RoundNodeButton button in spawnedButtons)
            {
                if (button != null) button.Refresh();
            }
        }

        public void Clear()
        {
            foreach (RoundNodeButton button in spawnedButtons)
            {
                if (button != null) Destroy(button.gameObject);
            }
            foreach (GameObject path in spawnedPathViews)
            {
                if (path != null) Destroy(path);
            }

            spawnedButtons.Clear();
            spawnedPathViews.Clear();
            buttonMap.Clear();
            nodeUIPositions.Clear();
        }

        // --- Event Subscriptions (StageManager) ---
        private void Subscribe()
        {
            if (stageManager == null) return;
            stageManager.OnStageGenerated -= HandleStageGenerated;
            stageManager.OnNodeSelected -= HandleNodeSelected;
            stageManager.OnNodeCompleted -= HandleNodeCompleted;
            stageManager.OnStageProgressChanged -= HandleStageProgressChanged;

            stageManager.OnStageGenerated += HandleStageGenerated;
            stageManager.OnNodeSelected += HandleNodeSelected;
            stageManager.OnNodeCompleted += HandleNodeCompleted;
            stageManager.OnStageProgressChanged += HandleStageProgressChanged;
        }

        private void Unsubscribe()
        {
            if (stageManager == null) return;
            stageManager.OnStageGenerated -= HandleStageGenerated;
            stageManager.OnNodeSelected -= HandleNodeSelected;
            stageManager.OnNodeCompleted -= HandleNodeCompleted;
            stageManager.OnStageProgressChanged -= HandleStageProgressChanged;
        }

        private void HandleStageGenerated(StageGraph graph)
        {
            Build(graph);
        }

        private void HandleNodeSelected(RoundNode node)
        {
            RefreshAll();
        }

        private void HandleNodeCompleted(RoundNode node)
        {
            RefreshAll();
        }

        private void HandleStageProgressChanged(StageProgressState state)
        {
            RefreshAll();
        }
        
        [ContextMenu("Test Build (If Graph Exists)")]
        private void TestBuild()
        {
            if (stageManager != null && stageManager.HasGraph())
            {
                Build(stageManager.CurrentGraph);
            }
            else
            {
                Debug.LogWarning("StageManager에 현재 생성된 그래프가 없습니다.");
            }
        }
    }
}
