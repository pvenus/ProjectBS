using System;
using System.Collections.Generic;
using UnityEngine;
using Session;

namespace Stage
{
    /// <summary>
    /// 현재 진행 중인 스테이지 그래프를 보관하고 진행 상태를 관리하는 런타임 허브.
    /// UI, Executor, Scene 전환 시스템은 이 클래스를 통해 현재 스테이지 상태에 접근한다.
    /// </summary>
    public class StageManager : MonoBehaviour
    {
        public static StageManager Instance { get; private set; }

        [Header("Definition")]
        [SerializeField] private StageDefinitionSO stageDefinition;

        [Header("Runtime")]
        [SerializeField] private StageRuntimeData runtimeData;

        public StageDefinitionSO StageDefinition => stageDefinition;
        public StageRuntimeData RuntimeData => runtimeData;
        public StageGraph CurrentGraph => runtimeData?.currentGraph;
        public RoundNode CurrentNode => runtimeData?.currentNode;
        public StageProgressState ProgressState =>
            runtimeData?.currentGraph?.progressState
            ?? StageProgressState.NotStarted;

        public event Action<StageGraph> OnStageGenerated;
        public event Action<RoundNode> OnNodeSelected;
        public event Action<RoundNode> OnNodeCompleted;
        public event Action<StageProgressState> OnStageProgressChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            // InitializeRuntime(); // Moved to Start()
        }

        private void InitializeRuntime()
        {
            GameSession gameSession =
                GameSession.Instance;

            if (gameSession == null)
            {
                Debug.LogError(
                    "[StageManager] GameSession not found.");
                return;
            }

            runtimeData =
                gameSession.StageSession.RuntimeData;

            if (runtimeData == null)
            {
                Debug.LogError(
                    "[StageManager] StageSession RuntimeData is null.");
            }
        }

        private void Start()
        {
            InitializeRuntime();
            if (stageDefinition != null && runtimeData.currentGraph == null)
            {
                GenerateStage(stageDefinition);
            }
        }

        public void SetStageDefinition(StageDefinitionSO definition)
        {
            stageDefinition = definition;
        }

        public StageGraph GenerateStage(StageDefinitionSO definition = null)
        {
            if (definition != null)
            {
                stageDefinition = definition;
            }

            if (stageDefinition == null)
            {
                Debug.LogError("[StageRuntime] Cannot generate stage. StageDefinitionSO is null.");
                return null;
            }

            StageGenerator generator = new StageGenerator();

            if (runtimeData == null)
            {
                Debug.LogError(
                    "[StageManager] RuntimeData is null.");
                return null;
            }

            runtimeData.currentGraph = generator.Generate(stageDefinition);

            runtimeData.stageId = stageDefinition.name;
            runtimeData.currentNode = runtimeData.currentGraph?.CurrentNode;

            if (runtimeData.currentGraph == null)
            {
                Debug.LogError("[StageRuntime] Stage generation failed.");
                return null;
            }

            OnStageGenerated?.Invoke(runtimeData.currentGraph);
            OnStageProgressChanged?.Invoke(runtimeData.currentGraph.progressState);

            return runtimeData.currentGraph;
        }

        public bool SelectNode(string nodeId)
        {
            if (runtimeData.currentGraph == null)
            {
                Debug.LogWarning("[StageRuntime] SelectNode failed. Current graph is null.");
                return false;
            }

            bool selected = runtimeData.currentGraph.SelectNode(nodeId);
            if (!selected)
            {
                return false;
            }

            runtimeData.currentNode = runtimeData.currentGraph.CurrentNode;
            OnNodeSelected?.Invoke(runtimeData.currentGraph.CurrentNode);
            return true;
        }

        public void CompleteCurrentNode()
        {
            if (runtimeData.currentGraph == null)
            {
                Debug.LogWarning("[StageRuntime] CompleteCurrentNode failed. Current graph is null.");
                return;
            }

            RoundNode completedNode = runtimeData.currentGraph.CurrentNode;
            if (completedNode == null)
            {
                Debug.LogWarning("[StageRuntime] CompleteCurrentNode failed. Current node is null.");
                return;
            }

            runtimeData.currentGraph.CompleteCurrentNode();

            runtimeData.currentNode = runtimeData.currentGraph.CurrentNode;

            OnNodeCompleted?.Invoke(completedNode);
            OnStageProgressChanged?.Invoke(runtimeData.currentGraph.progressState);
        }

        public void FailStage()
        {
            if (runtimeData.currentGraph == null)
            {
                Debug.LogWarning("[StageRuntime] FailStage failed. Current graph is null.");
                return;
            }

            runtimeData.currentGraph.FailStage();
            OnStageProgressChanged?.Invoke(runtimeData.currentGraph.progressState);
        }

        public List<RoundNode> GetAvailableNodes()
        {
            if (runtimeData.currentGraph == null)
            {
                return new List<RoundNode>();
            }

            return runtimeData.currentGraph.GetAvailableNodes();
        }

        public List<RoundNode> GetNodesByDepth(int depth)
        {
            if (runtimeData.currentGraph == null)
            {
                return new List<RoundNode>();
            }

            return runtimeData.currentGraph.GetNodesByDepth(depth);
        }

        public List<int> GetDepths()
        {
            if (runtimeData.currentGraph == null)
            {
                return new List<int>();
            }

            return runtimeData.currentGraph.GetDepths();
        }

        public RoundNode GetNode(string nodeId)
        {
            if (runtimeData.currentGraph == null)
            {
                return null;
            }

            return runtimeData.currentGraph.GetNode(nodeId);
        }


        public bool HasGraph()
        {
            return runtimeData.currentGraph != null;
        }

        public void ClearRuntime()
        {
            GameSession.Instance
                .StageSession
                .ResetRuntime();

            runtimeData =
                GameSession.Instance
                    .StageSession
                    .RuntimeData;

            OnStageProgressChanged?.Invoke(
                StageProgressState.NotStarted);
        }
    }
}