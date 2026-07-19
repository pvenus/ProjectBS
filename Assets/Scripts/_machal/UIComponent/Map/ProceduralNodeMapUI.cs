using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Stage; // 실제 게임의 StageGraph, RoundNode 등을 사용
using Stage.UI; // RoundNodeButton 사용

namespace UIFramework.Map
{
    public enum MapScrollDirection
    {
        Auto,
        Vertical,
        Horizontal,
        Both
    }

    public class ProceduralNodeMapUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private StageManager stageManager;
        public RectTransform contentRoot;
        [Tooltip("비워두면 contentRoot를 사용합니다. 노드 뒤에 길을 그리기 위해 분리하는 것을 권장합니다.")]
        public RectTransform pathRoot;
        [Tooltip("Content를 소유하는 ScrollRect 컴포넌트입니다. 지정하지 않을 시 contentRoot의 상위에서 자동으로 탐색합니다.")]
        [SerializeField] private UnityEngine.UI.ScrollRect targetScrollRect;

        [Header("Scroll Settings")]
        [Tooltip("스크롤 크기를 제어할 방향을 선택합니다. Auto의 경우 ScrollRect의 설정을 자동으로 분석합니다.")]
        public MapScrollDirection scrollDirectionOption = MapScrollDirection.Auto;
        
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

        [Header("Grid Layout Settings")]
        public bool useGridLayout = true;
        public MapGridSettings gridSettings = new MapGridSettings
        {
            gridPlacementMode = GridPlacementMode.Advanced,
            gridColumnCount = 5,
            gridCellSize = new Vector2(150, 180),
            gridOrigin = Vector2.zero,
            centerRoutesInGrid = true,
            applyRandomOffsetInCell = true,
            randomOffsetRange = new Vector2(15, 15),
            growUpwards = true
        };

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
            if (targetScrollRect == null && contentRoot != null)
            {
                targetScrollRect = contentRoot.GetComponentInParent<UnityEngine.UI.ScrollRect>();
            }
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

            // 런타임 방어 및 디버그용 검증 로그 추가
            gridSettings.gridColumnCount = Mathf.Max(1, gridSettings.gridColumnCount);
            gridSettings.gridCellSize.x = Mathf.Max(10f, gridSettings.gridCellSize.x);
            gridSettings.gridCellSize.y = Mathf.Max(10f, gridSettings.gridCellSize.y);
            Debug.Log($"[ProceduralNodeMapUI] Build 시작 - useGridLayout: {useGridLayout}, Mode: {gridSettings.gridPlacementMode}, gridColumnCount: {gridSettings.gridColumnCount}, gridCellSize: {gridSettings.gridCellSize}, CenterRoutes: {gridSettings.centerRoutesInGrid}, RandomOffset: {gridSettings.applyRandomOffsetInCell}");

            if (pathRoot == null) pathRoot = contentRoot;

            System.Random rng = useFixedSeed ? new System.Random(randomSeed) : new System.Random();

            // 1. 노드들의 UI 좌표 계산 (그리드 또는 레거시)
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

            if (useGridLayout)
            {
                nodeUIPositions = MapGridPositionResolver.CalculateGridNodePositions(graph, gridSettings, rng);
                return;
            }

            CalculateLegacyNodePositions(graph, rng);
        }

        private void CalculateLegacyNodePositions(StageGraph graph, System.Random rng)
        {
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

            // 스크롤 방향 감지 및 이에 따른 피벗과 정렬 적용
            bool isHorizontalScroll = false;
            bool isVerticalScroll = true; // 디폴트는 세로

            if (scrollDirectionOption == MapScrollDirection.Auto)
            {
                if (targetScrollRect != null)
                {
                    if (targetScrollRect.horizontal && !targetScrollRect.vertical)
                    {
                        isHorizontalScroll = true;
                        isVerticalScroll = false;
                    }
                    else if (!targetScrollRect.horizontal && targetScrollRect.vertical)
                    {
                        isHorizontalScroll = false;
                        isVerticalScroll = true;
                    }
                    else
                    {
                        isHorizontalScroll = true;
                        isVerticalScroll = true;
                    }
                }
            }
            else if (scrollDirectionOption == MapScrollDirection.Vertical)
            {
                isHorizontalScroll = false;
                isVerticalScroll = true;
            }
            else if (scrollDirectionOption == MapScrollDirection.Horizontal)
            {
                isHorizontalScroll = true;
                isVerticalScroll = false;
            }
            else if (scrollDirectionOption == MapScrollDirection.Both)
            {
                isHorizontalScroll = true;
                isVerticalScroll = true;
            }

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

                // Viewport 크기 쿼리
                RectTransform viewport = null;
                if (targetScrollRect != null)
                {
                    viewport = targetScrollRect.viewport;
                }
                if (viewport == null)
                {
                    viewport = scrollContent.parent as RectTransform;
                }

                float viewportWidth = 0f;
                float viewportHeight = 0f;
                if (viewport != null)
                {
                    viewportWidth = viewport.rect.width;
                    viewportHeight = viewport.rect.height;
                }
                else
                {
                    viewportWidth = scrollContent.sizeDelta.x;
                    viewportHeight = scrollContent.sizeDelta.y;
                }

                Vector2 rootPivot;
                Vector2 targetSize = scrollContent.sizeDelta;

                if (isHorizontalScroll && !isVerticalScroll)
                {
                    rootPivot = new Vector2(0f, 0.5f);
                    targetSize.x = mapWidth;
                    targetSize.y = viewportHeight; // 제어되지 않는 축(세로)은 Viewport 높이에 맞춤
                }
                else if (isVerticalScroll && !isHorizontalScroll)
                {
                    rootPivot = new Vector2(0.5f, 0f);
                    targetSize.y = mapHeight;
                    targetSize.x = viewportWidth; // 제어되지 않는 축(가로)은 Viewport 너비에 맞춤
                }
                else
                {
                    rootPivot = new Vector2(0.5f, 0f);
                    targetSize = new Vector2(mapWidth, mapHeight);
                }

                // NodeRoot와 PathRoot를 Stretch 구조로 밀착
                ResetRootRectTransform(contentRoot, rootPivot);
                if (pathRoot != null && pathRoot != contentRoot)
                {
                    ResetRootRectTransform(pathRoot, rootPivot);
                }

                // 스크롤 컨텐트 설정 적용
                if (isHorizontalScroll && !isVerticalScroll)
                {
                    // 좌측 중앙 정렬 및 가로 확장
                    scrollContent.anchorMin = new Vector2(0f, 0.5f);
                    scrollContent.anchorMax = new Vector2(0f, 0.5f);
                    scrollContent.pivot = new Vector2(0f, 0.5f);
                    scrollContent.sizeDelta = targetSize;
                    scrollContent.anchoredPosition = new Vector2(0f, scrollContent.anchoredPosition.y);
                }
                else if (isVerticalScroll && !isHorizontalScroll)
                {
                    // 하단 중앙 정렬 및 세로 확장
                    scrollContent.anchorMin = new Vector2(0.5f, 0f);
                    scrollContent.anchorMax = new Vector2(0.5f, 0f);
                    scrollContent.pivot = new Vector2(0.5f, 0f);
                    scrollContent.sizeDelta = targetSize;
                    scrollContent.anchoredPosition = new Vector2(scrollContent.anchoredPosition.x, 0f);
                }
                else
                {
                    // 기본 하단 중앙 정렬 및 양방향 확장
                    scrollContent.anchorMin = new Vector2(0.5f, 0f);
                    scrollContent.anchorMax = new Vector2(0.5f, 0f);
                    scrollContent.pivot = new Vector2(0.5f, 0f);
                    scrollContent.sizeDelta = targetSize;
                    scrollContent.anchoredPosition = Vector2.zero;
                }
            }

            // 2. 모든 자식 오브젝트(노드 버튼 및 패스 세그먼트)의 좌표를 보정된 크기에 맞춰 일괄 시프트
            float offsetX = 0f;
            float offsetY = 0f;

            bool isHorizontalOnly = isHorizontalScroll && !isVerticalScroll;
            bool isVerticalOnly = isVerticalScroll && !isHorizontalScroll;

            if (isHorizontalOnly)
            {
                // 가로는 좌측 기준(paddingX만큼 오프셋), 세로는 중앙 정렬
                offsetX = -minX + paddingX;
                offsetY = -(minY + maxY) * 0.5f;
            }
            else if (isVerticalOnly)
            {
                // 가로는 중앙 정렬, 세로는 하단 기준(paddingY만큼 오프셋)
                offsetX = -(minX + maxX) * 0.5f;
                offsetY = -minY + paddingY;
            }
            else
            {
                // 기존 기본값
                offsetX = -(minX + maxX) * 0.5f;
                offsetY = -minY + paddingY;
            }

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

        private void ResetRootRectTransform(RectTransform rect, Vector2 pivot)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = pivot;
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
