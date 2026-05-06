
using System.Text;
using UnityEngine;

namespace Stage
{
    /// <summary>
    /// 스테이지 생성/선택/완료 흐름을 전투 없이 검증하기 위한 디버그 컨트롤러.
    /// UI Button OnClick에 public 메서드를 연결해서 사용한다.
    /// </summary>
    public class StageDebugController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private StageRuntime stageRuntime;
        [SerializeField] private StageDefinitionSO stageDefinition;

        [Header("Options")]
        [SerializeField] private bool generateOnStart = false;
        [SerializeField] private bool logGraphOnGenerate = true;
        [SerializeField] private bool logOnNodeSelected = true;
        [SerializeField] private bool logOnNodeCompleted = true;

        private void Awake()
        {
            if (stageRuntime == null)
            {
                stageRuntime = StageRuntime.Instance;
            }
        }

        private void OnEnable()
        {
            if (stageRuntime == null)
            {
                stageRuntime = StageRuntime.Instance;
            }

            Subscribe();
        }

        private void Start()
        {
            if (generateOnStart)
            {
                GenerateStage();
            }
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void GenerateStage()
        {
            if (stageRuntime == null)
            {
                Debug.LogWarning("[StageDebugController] GenerateStage failed. StageRuntime is null.");
                return;
            }

            StageDefinitionSO targetDefinition = stageDefinition != null
                ? stageDefinition
                : stageRuntime.StageDefinition;

            if (targetDefinition == null)
            {
                Debug.LogWarning("[StageDebugController] GenerateStage failed. StageDefinitionSO is null.");
                return;
            }

            StageGraph graph = stageRuntime.GenerateStage(targetDefinition);
            if (graph == null)
            {
                return;
            }

            if (logGraphOnGenerate)
            {
                LogGraph();
            }
        }

        public void RegenerateStage()
        {
            if (stageRuntime == null)
            {
                Debug.LogWarning("[StageDebugController] RegenerateStage failed. StageRuntime is null.");
                return;
            }

            stageRuntime.ClearRuntime();
            GenerateStage();
        }

        public void CompleteCurrentNode()
        {
            if (stageRuntime == null)
            {
                Debug.LogWarning("[StageDebugController] CompleteCurrentNode failed. StageRuntime is null.");
                return;
            }

            RoundNode currentNode = stageRuntime.CurrentNode;
            if (currentNode == null)
            {
                Debug.LogWarning("[StageDebugController] CompleteCurrentNode failed. Current node is null.");
                return;
            }

            if (!currentNode.isSelected && !currentNode.IsStartNode)
            {
                Debug.LogWarning($"[StageDebugController] Current node is not selected. nodeId={currentNode.nodeId}");
            }

            stageRuntime.CompleteCurrentNode();
        }

        public void LogGraph()
        {
            if (stageRuntime == null || stageRuntime.CurrentGraph == null)
            {
                Debug.LogWarning("[StageDebugController] LogGraph failed. Current graph is null.");
                return;
            }

            StageGraph graph = stageRuntime.CurrentGraph;
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"[StageDebugController] Stage Graph: {graph.stageId} / {graph.stageName}");
            builder.AppendLine($"Progress: {graph.progressState}");
            builder.AppendLine($"Start: {graph.startNodeId}, Current: {graph.currentNodeId}, Boss: {graph.bossNodeId}");

            foreach (int depth in graph.GetDepths())
            {
                builder.AppendLine($"Depth {depth}");

                foreach (RoundNode node in graph.GetNodesByDepth(depth))
                {
                    string next = node.nextNodeIds == null || node.nextNodeIds.Count == 0
                        ? "-"
                        : string.Join(", ", node.nextNodeIds);

                    builder.AppendLine($"  - {node.nodeId} | {node.title} | {node.nodeType} | {node.state} | next: {next}");
                }
            }

            Debug.Log(builder.ToString());
        }

        public void LogCurrentNode()
        {
            if (stageRuntime == null || stageRuntime.CurrentNode == null)
            {
                Debug.Log("[StageDebugController] Current node is null.");
                return;
            }

            RoundNode node = stageRuntime.CurrentNode;
            Debug.Log($"[StageDebugController] Current Node: {node.nodeId} / {node.title} / {node.nodeType} / {node.state}");
        }

        public void LogAvailableNodes()
        {
            if (stageRuntime == null || stageRuntime.CurrentGraph == null)
            {
                Debug.LogWarning("[StageDebugController] LogAvailableNodes failed. Current graph is null.");
                return;
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("[StageDebugController] Available Nodes");

            foreach (RoundNode node in stageRuntime.GetAvailableNodes())
            {
                builder.AppendLine($"  - {node.nodeId} | {node.title} | {node.nodeType}");
            }

            Debug.Log(builder.ToString());
        }

        private void Subscribe()
        {
            if (stageRuntime == null)
            {
                return;
            }

            stageRuntime.OnNodeSelected -= HandleNodeSelected;
            stageRuntime.OnNodeCompleted -= HandleNodeCompleted;
            stageRuntime.OnStageProgressChanged -= HandleStageProgressChanged;

            stageRuntime.OnNodeSelected += HandleNodeSelected;
            stageRuntime.OnNodeCompleted += HandleNodeCompleted;
            stageRuntime.OnStageProgressChanged += HandleStageProgressChanged;
        }

        private void Unsubscribe()
        {
            if (stageRuntime == null)
            {
                return;
            }

            stageRuntime.OnNodeSelected -= HandleNodeSelected;
            stageRuntime.OnNodeCompleted -= HandleNodeCompleted;
            stageRuntime.OnStageProgressChanged -= HandleStageProgressChanged;
        }

        private void HandleNodeSelected(RoundNode node)
        {
            if (!logOnNodeSelected || node == null)
            {
                return;
            }

            Debug.Log($"[StageDebugController] Node Selected: {node.nodeId} / {node.title} / {node.nodeType}");
        }

        private void HandleNodeCompleted(RoundNode node)
        {
            if (!logOnNodeCompleted || node == null)
            {
                return;
            }

            Debug.Log($"[StageDebugController] Node Completed: {node.nodeId} / {node.title} / {node.nodeType}");
            LogAvailableNodes();
        }

        private void HandleStageProgressChanged(StageProgressState state)
        {
            Debug.Log($"[StageDebugController] Stage Progress Changed: {state}");
        }
    }
}