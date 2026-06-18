using System.Collections;
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

            if (rebuildOnEnable)
            {
                StartCoroutine(RebuildAfterStageManagerStart());
            }
        }

        private IEnumerator RebuildAfterStageManagerStart()
        {
            yield return null;

            if (!isActiveAndEnabled)
            {
                yield break;
            }

            if (stageManager == null)
            {
                stageManager = StageManager.Instance;
            }

            if (stageManager != null && stageManager.HasGraph())
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

            // 4. 전부 배치한 다음 위젯의 사이즈와 정렬 갱신
            UpdateLayoutAndContentSize();

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

        private void UpdateLayoutAndContentSize()
        {
            if (spawnedButtons.Count == 0) return;

            // 1. 실제로 생성된 노드들의 로컬 anchoredPosition 기준 바운딩 박스 계산
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            foreach (var button in spawnedButtons)
            {
                if (button == null) continue;
                RectTransform rect = button.GetComponent<RectTransform>();
                if (rect == null) continue;

                Vector2 pos = rect.anchoredPosition;
                if (pos.x < minX) minX = pos.x;
                if (pos.x > maxX) maxX = pos.x;
                if (pos.y < minY) minY = pos.y;
                if (pos.y > maxY) maxY = pos.y;
            }

            // 여백(Padding) 설정
            float paddingX = horizontalSpacing * 0.8f;
            float paddingY = verticalSpacing * 0.8f;

            float mapWidth = (maxX - minX) + paddingX * 2f;
            float mapHeight = (maxY - minY) + paddingY * 2f;

            RectTransform scrollContent = contentRoot.parent as RectTransform;
            if (scrollContent != null)
            {
                // ContentSizeFitter 충돌 방지 비활성화
                var fitter = scrollContent.GetComponent<UnityEngine.UI.ContentSizeFitter>();
                if (fitter != null) fitter.enabled = false;

                var contentFitter = contentRoot.GetComponent<UnityEngine.UI.ContentSizeFitter>();
                if (contentFitter != null) contentFitter.enabled = false;
                if (pathRoot != null && pathRoot != contentRoot)
                {
                    var pathFitter = pathRoot.GetComponent<UnityEngine.UI.ContentSizeFitter>();
                    if (pathFitter != null) pathFitter.enabled = false;
                }

                // NodeRoot와 PathRoot를 Stretch 구조로 밀착
                ResetRootRectTransform(contentRoot);
                if (pathRoot != null && pathRoot != contentRoot)
                {
                    ResetRootRectTransform(pathRoot);
                }

                // 최하단 기준 배치 및 위쪽 확장 방식 (Pivot을 하단 중앙 0.5, 0 으로 설정)
                scrollContent.anchorMin = new Vector2(0.5f, 0f);
                scrollContent.anchorMax = new Vector2(0.5f, 0f);
                scrollContent.pivot = new Vector2(0.5f, 0f);

                scrollContent.sizeDelta = new Vector2(mapWidth, mapHeight);
                // anchoredPosition.y = 0 이면 하단(시작 지점)이 화면에 보임
                scrollContent.anchoredPosition = Vector2.zero;
            }

            // 2. 모든 자식 오브젝트(노드 버튼 및 패스 세그먼트)의 좌표를 보정된 크기에 맞춰 일괄 시프트
            // 가로는 중앙 정렬, 세로는 최하단(y=0) 기준 위쪽 방향 정렬
            float offsetX = -(minX + maxX) * 0.5f;
            float offsetY = -minY + paddingY;

            // 노드 버튼 시프트
            foreach (var button in spawnedButtons)
            {
                if (button == null) continue;
                RectTransform rect = button.GetComponent<RectTransform>();
                if (rect != null)
                {
                    Vector2 pos = rect.anchoredPosition;
                    rect.anchoredPosition = new Vector2(pos.x + offsetX, pos.y + offsetY);
                }
            }

            // 패스 세그먼트 시프트 (길)
            foreach (var path in spawnedPathViews)
            {
                if (path == null) continue;
                RectTransform rect = path.GetComponent<RectTransform>();
                if (rect != null)
                {
                    Vector2 pos = rect.anchoredPosition;
                    rect.anchoredPosition = new Vector2(pos.x + offsetX, pos.y + offsetY);
                }
            }

            // 3. UI 강제 즉시 갱신 (스크롤 범위 즉각 반영 보장)
            if (scrollContent != null)
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(scrollContent);
            }
        }

        private void ResetRootRectTransform(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
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
                        rect.anchorMin = new Vector2(0.5f, 0f);
                        rect.anchorMax = new Vector2(0.5f, 0f);
                        rect.pivot = new Vector2(0.5f, 0.5f);
                        rect.anchoredPosition = pos;
                        
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
                    rectTransform.anchorMin = new Vector2(0.5f, 0f);
                    rectTransform.anchorMax = new Vector2(0.5f, 0f);
                    rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    rectTransform.anchoredPosition = pos;
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
