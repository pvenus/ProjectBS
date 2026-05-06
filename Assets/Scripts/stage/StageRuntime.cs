

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Stage
{
    /// <summary>
    /// 현재 진행 중인 스테이지 그래프를 보관하고 진행 상태를 관리하는 런타임 허브.
    /// UI, Executor, Scene 전환 시스템은 이 클래스를 통해 현재 스테이지 상태에 접근한다.
    /// </summary>
    public class StageRuntime : MonoBehaviour
    {
        public static StageRuntime Instance { get; private set; }

        [Header("Definition")]
        [SerializeField] private StageDefinitionSO stageDefinition;

        [Header("Runtime")]
        [SerializeField] private StageGraph currentGraph;

        public StageDefinitionSO StageDefinition => stageDefinition;
        public StageGraph CurrentGraph => currentGraph;
        public RoundNode CurrentNode => currentGraph?.CurrentNode;
        public StageProgressState ProgressState => currentGraph?.progressState ?? StageProgressState.NotStarted;

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
        }

        private void Start()
        {
            if (stageDefinition != null && currentGraph == null)
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
            currentGraph = generator.Generate(stageDefinition);

            if (currentGraph == null)
            {
                Debug.LogError("[StageRuntime] Stage generation failed.");
                return null;
            }

            OnStageGenerated?.Invoke(currentGraph);
            OnStageProgressChanged?.Invoke(currentGraph.progressState);

            return currentGraph;
        }

        public bool SelectNode(string nodeId)
        {
            if (currentGraph == null)
            {
                Debug.LogWarning("[StageRuntime] SelectNode failed. Current graph is null.");
                return false;
            }

            bool selected = currentGraph.SelectNode(nodeId);
            if (!selected)
            {
                return false;
            }

            OnNodeSelected?.Invoke(currentGraph.CurrentNode);
            return true;
        }

        public void CompleteCurrentNode()
        {
            if (currentGraph == null)
            {
                Debug.LogWarning("[StageRuntime] CompleteCurrentNode failed. Current graph is null.");
                return;
            }

            RoundNode completedNode = currentGraph.CurrentNode;
            if (completedNode == null)
            {
                Debug.LogWarning("[StageRuntime] CompleteCurrentNode failed. Current node is null.");
                return;
            }

            currentGraph.CompleteCurrentNode();

            OnNodeCompleted?.Invoke(completedNode);
            OnStageProgressChanged?.Invoke(currentGraph.progressState);
        }

        public void FailStage()
        {
            if (currentGraph == null)
            {
                Debug.LogWarning("[StageRuntime] FailStage failed. Current graph is null.");
                return;
            }

            currentGraph.FailStage();
            OnStageProgressChanged?.Invoke(currentGraph.progressState);
        }

        public List<RoundNode> GetAvailableNodes()
        {
            if (currentGraph == null)
            {
                return new List<RoundNode>();
            }

            return currentGraph.GetAvailableNodes();
        }

        public List<RoundNode> GetNodesByDepth(int depth)
        {
            if (currentGraph == null)
            {
                return new List<RoundNode>();
            }

            return currentGraph.GetNodesByDepth(depth);
        }

        public List<int> GetDepths()
        {
            if (currentGraph == null)
            {
                return new List<int>();
            }

            return currentGraph.GetDepths();
        }

        public RoundNode GetNode(string nodeId)
        {
            if (currentGraph == null)
            {
                return null;
            }

            return currentGraph.GetNode(nodeId);
        }

        public bool HasGraph()
        {
            return currentGraph != null;
        }

        public void ClearRuntime()
        {
            currentGraph = null;
            OnStageProgressChanged?.Invoke(StageProgressState.NotStarted);
        }
    }
}