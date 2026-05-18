

using System.Text;
using UnityEngine;

namespace Stage
{
    /// <summary>
    /// StageManager 이벤트를 구독하여 플레이어가 선택/완료한 스테이지 진행 경로를 기록한다.
    /// 이후 경로 UI, 저장/복구, 디버그 분석에 사용할 수 있다.
    /// </summary>
    public class StageRouteManager : MonoBehaviour
    {
        public static StageRouteManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private StageManager stageManager;

        [Header("Runtime")]
        [SerializeField] private StageRouteRecord currentRouteRecord;

        [Header("Debug")]
        [SerializeField] private bool logOnRouteChanged = true;

        public StageRouteRecord CurrentRouteRecord => currentRouteRecord;
        public int RouteCount => currentRouteRecord?.Count ?? 0;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

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
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void Initialize(StageGraph graph)
        {
            if (graph == null)
            {
                Debug.LogWarning("[StageRouteManager] Initialize failed. StageGraph is null.");
                currentRouteRecord = null;
                return;
            }

            currentRouteRecord = new StageRouteRecord(graph.stageId, graph.stageName);

            if (graph.StartNode != null)
            {
                currentRouteRecord.AddCompletedNode(graph.StartNode);
            }

            if (logOnRouteChanged)
            {
                Debug.Log($"[StageRouteManager] Route initialized. stage={graph.stageId}, start={graph.startNodeId}");
            }
        }

        public void Clear()
        {
            if (currentRouteRecord != null)
            {
                currentRouteRecord.Clear();
            }
        }

        public void ResetRecord()
        {
            currentRouteRecord = null;
        }

        public void AddSelectedNode(RoundNode node)
        {
            if (node == null)
            {
                return;
            }

            EnsureRecordExists();
            currentRouteRecord?.AddSelectedNode(node);

            if (logOnRouteChanged)
            {
                Debug.Log($"[StageRouteManager] Selected: {node.nodeId} / {node.title}");
            }
        }

        public void AddCompletedNode(RoundNode node)
        {
            if (node == null)
            {
                return;
            }

            EnsureRecordExists();
            currentRouteRecord?.AddCompletedNode(node);

            if (logOnRouteChanged)
            {
                Debug.Log($"[StageRouteManager] Completed: {node.nodeId} / {node.title}");
            }
        }

        public void AddFailedNode(RoundNode node)
        {
            if (node == null)
            {
                return;
            }

            EnsureRecordExists();
            currentRouteRecord?.AddFailedNode(node);

            if (logOnRouteChanged)
            {
                Debug.Log($"[StageRouteManager] Failed: {node.nodeId} / {node.title}");
            }
        }

        public string GetRouteSummaryText()
        {
            if (currentRouteRecord == null || currentRouteRecord.records == null || currentRouteRecord.records.Count == 0)
            {
                return "Route is empty.";
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"Stage Route: {currentRouteRecord.stageId} / {currentRouteRecord.stageName}");

            for (int i = 0; i < currentRouteRecord.records.Count; i++)
            {
                StageRouteNodeRecord record = currentRouteRecord.records[i];
                builder.AppendLine($"{i + 1}. [{record.actionType}] {record.nodeId} / {record.title} / depth={record.depth} / type={record.nodeType}");
            }

            return builder.ToString();
        }

        public void LogRouteSummary()
        {
            Debug.Log($"[StageRouteManager]\n{GetRouteSummaryText()}");
        }

        private void EnsureRecordExists()
        {
            if (currentRouteRecord != null)
            {
                return;
            }

            StageGraph graph = stageManager != null ? stageManager.CurrentGraph : null;
            if (graph == null)
            {
                currentRouteRecord = new StageRouteRecord();
                return;
            }

            currentRouteRecord = new StageRouteRecord(graph.stageId, graph.stageName);
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
            Initialize(graph);
        }

        private void HandleNodeSelected(RoundNode node)
        {
            AddSelectedNode(node);
        }

        private void HandleNodeCompleted(RoundNode node)
        {
            AddCompletedNode(node);
        }

        private void HandleStageProgressChanged(StageProgressState state)
        {
            if (state == StageProgressState.Failed && stageManager != null)
            {
                AddFailedNode(stageManager.CurrentNode);
            }
        }
    }
}