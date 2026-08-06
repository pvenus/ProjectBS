

using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace Stage.UI
{
    /// <summary>
    /// 생성된 StageGraph를 UI 버튼 맵으로 표시한다.
    /// 1차 버전: depth 기준으로 노드를 배치하고, 노드 상태 변경 시 버튼을 갱신한다.
    /// </summary>
    public class StageMapUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private StageManager stageManager;
        [SerializeField] private RectTransform nodeRoot;
        [SerializeField] private RoundNodeButton nodeButtonPrefab;

        [Header("Layout")]
        [SerializeField] private float horizontalSpacing = 180f;
        [SerializeField] private float verticalSpacing = 160f;
        [SerializeField] private Vector2 startPosition = Vector2.zero;

        [Header("Options")]
        [SerializeField] private bool rebuildOnEnable = true;

        private readonly List<RoundNodeButton> spawnedButtons = new();
        private readonly Dictionary<string, RoundNodeButton> buttonMap = new();

        private void Awake()
        {
            if (stageManager == null)
            {
                stageManager = StageManager.Instance;
            }
        }

        private void OnEnable()
        {
            if (stageManager == null)
            {
                stageManager = StageManager.Instance;
            }

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

        public void Build(StageGraph graph)
        {
            Clear();

            if (graph == null)
            {
                Debug.LogWarning("[StageMapUI] Build failed. StageGraph is null.");
                return;
            }

            if (nodeRoot == null)
            {
                Debug.LogWarning("[StageMapUI] Build failed. Node root is null.");
                return;
            }

            if (nodeButtonPrefab == null)
            {
                Debug.LogWarning("[StageMapUI] Build failed. Node button prefab is null.");
                return;
            }

            List<int> depths = graph.GetDepths();
            foreach (int depth in depths)
            {
                List<RoundNode> nodes = graph.GetNodesByDepth(depth);
                SpawnDepthNodes(depth, nodes);
            }

            RefreshAll();
        }

        public void RefreshAll()
        {
            foreach (RoundNodeButton button in spawnedButtons)
            {
                if (button != null)
                {
                    button.Refresh();
                }
            }
        }

        public void Clear()
        {
            foreach (RoundNodeButton button in spawnedButtons)
            {
                if (button != null)
                {
                    Destroy(button.gameObject);
                }
            }

            spawnedButtons.Clear();
            buttonMap.Clear();
        }

        private void SpawnDepthNodes(int depth, List<RoundNode> nodes)
        {
            if (nodes == null || nodes.Count == 0)
            {
                return;
            }

            float totalWidth = (nodes.Count - 1) * horizontalSpacing;
            float startX = startPosition.x - totalWidth * 0.5f;
            float y = startPosition.y - depth * verticalSpacing;

            for (int i = 0; i < nodes.Count; i++)
            {
                RoundNode node = nodes[i];
                RoundNodeButton button = Instantiate(nodeButtonPrefab, nodeRoot);

                RectTransform rectTransform = button.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = new Vector2(startX + i * horizontalSpacing, y);
                }

                button.Initialize(node);
                spawnedButtons.Add(button);
                buttonMap[node.nodeId] = button;
            }
        }

        private void Subscribe()
        {
            if (stageManager == null)
            {
                return;
            }

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
            if (stageManager == null)
            {
                return;
            }

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
    }
}