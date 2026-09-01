using System;
using System.Collections.Generic;
using System.Linq;
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

        // SVG SlotMap 배치 결과 (StagePlacementRuleSO에 의해 배정된 slotId → RoundNodeSO)
        [Header("SVG Slot Placement Result")]
        [Tooltip("ApplyRandomSectionPlacements 실행 결과. StagePlacementRuleSO가 각 슬롯에 배정한 RoundNodeSO 목록.")]
        [SerializeField] private List<SvgPlacementResultEntry> _svgPlacementResult = new();

        /// <summary>인스펙터 및 외부에서 읽기 전용으로 접근하는 SVG 슬롯 배치 결과.</summary>
        public IReadOnlyList<SvgPlacementResultEntry> SvgPlacementResult => _svgPlacementResult;

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
        public event Action<OrdinaryBattleCompletionReceipt> OnOrdinaryBattleCompleted;

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
            if (runtimeData == null)
            {
                return;
            }

            // StageScene can be entered directly in the Editor. Weighted/random-growth
            // events require a run and stage-generation identity even on that path.
            // Returning from battle already owns an active run and must not reset it.
            GameSession gameSession = GameSession.Instance;
            if (gameSession?.ProgressionSession?.HasActiveRun != true)
            {
                gameSession?.BeginNewProgressionRun();
            }

            if (stageDefinition != null
                && (runtimeData.currentGraph == null
                    || runtimeData.currentGraph.nodes.Count == 0))
            {
                GenerateStage(stageDefinition);
            }

            ApplyPendingBattleNodeCompletion();
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

            // ── SVG SlotMap: Random Section 배치 실행 ───────────────────────────
            // svgRandomSections가 존재하는 경우에만 실행. 기존 routeKey 흐름과 무관.
            _svgPlacementResult.Clear();
            if (stageDefinition.svgRandomSections != null
                && stageDefinition.svgRandomSections.Count > 0)
            {
                // 그래프 생성에 실제로 사용된 동일한 배정 결과를 노출한다.
                // 비고정 시드에서도 StageManager가 랜덤 배정을 다시 실행하지 않는다.
                foreach (var section in stageDefinition.svgRandomSections)
                {
                    if (section == null || section.targetSlotIds == null) continue;
                    foreach (var slotId in section.targetSlotIds)
                    {
                        generator.LastAssignments.TryGetValue(slotId, out RoundNodeSO node);
                        _svgPlacementResult.Add(new SvgPlacementResultEntry
                        {
                            sectionId    = section.sectionId,
                            slotId       = slotId,
                            assignedNode = node
                        });
                    }
                }

                int assigned = _svgPlacementResult.Count(e => e.assignedNode != null);
                Debug.Log($"[StageManager] SVG Slot Placement: {assigned}/{_svgPlacementResult.Count} 슬롯 배정 완료.");
            }
            // ────────────────────────────────────────────────────────────────────

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
            TryCompleteCurrentNode(
                runtimeData?.currentGraph?.CurrentNode?.nodeId);
        }

        public bool TryCompleteCurrentNode(
            string expectedNodeId)
        {
            if (runtimeData.currentGraph == null)
            {
                Debug.LogWarning("[StageRuntime] CompleteCurrentNode failed. Current graph is null.");
                return false;
            }

            RoundNode completedNode = runtimeData.currentGraph.CurrentNode;
            if (completedNode == null)
            {
                Debug.LogWarning("[StageRuntime] CompleteCurrentNode failed. Current node is null.");
                return false;
            }

            bool wasCompleted = completedNode.IsCompleted;
            if (!runtimeData.currentGraph.TryCompleteCurrentNode(
                    expectedNodeId))
            {
                return false;
            }

            runtimeData.currentNode = runtimeData.currentGraph.CurrentNode;

            if (!wasCompleted)
            {
                OnNodeCompleted?.Invoke(completedNode);
                OnStageProgressChanged?.Invoke(
                    runtimeData.currentGraph.progressState);
            }

            return true;
        }

        public bool TryResolveSvgPlacement(RoundNode node, out string sectionId, out string slotId)
        {
            sectionId = string.Empty;
            slotId = string.Empty;
            if (node == null || node.roundNodeSO == null || string.IsNullOrWhiteSpace(node.nodeId))
                return false;
            foreach (SvgPlacementResultEntry entry in _svgPlacementResult)
            {
                if (entry == null || entry.assignedNode != node.roundNodeSO
                    || string.IsNullOrWhiteSpace(entry.slotId)) continue;
                string safeSlot = entry.slotId.Replace('.', '_').Replace('-', '_');
                if (!node.nodeId.Contains("_s" + safeSlot + "_", StringComparison.Ordinal)) continue;
                sectionId = entry.sectionId ?? string.Empty;
                slotId = entry.slotId;
                return !string.IsNullOrWhiteSpace(sectionId);
            }
            return false;
        }

        public void PublishAtomicCompletion(RoundNode completedNode, StageProgressState progress)
        {
            OnNodeCompleted?.Invoke(completedNode);
            OnStageProgressChanged?.Invoke(progress);
        }

        private void ApplyPendingBattleNodeCompletion()
        {
            GameSession gameSession = GameSession.Instance;
            BattleSession battleSession =
                gameSession?.BattleSession;

            if (battleSession == null
                || !battleSession.TryGetCompletedStageNodeId(
                    out string completedNodeId))
            {
                return;
            }

            if (!gameSession.StageSession.TryApplyCompletedBattleNode(
                    battleSession,
                    out RoundNode completedNode,
                    out bool newlyCompleted,
                    out string error))
            {
                Debug.LogError(
                    "[StageRuntime] Battle completion could not be applied. "
                    + $"expectedNodeId={completedNodeId}, "
                    + $"error={error}");
                return;
            }

            runtimeData = gameSession.StageSession.RuntimeData;

            if (newlyCompleted)
            {
                OnNodeCompleted?.Invoke(completedNode);
                OnStageProgressChanged?.Invoke(
                    runtimeData.currentGraph.progressState);
            }

            OrdinaryBattleCompletionReceipt ordinaryReceipt =
                gameSession.StageSession.OrdinaryBattles?.ConsumePublication();
            if (ordinaryReceipt != null)
            {
                OnOrdinaryBattleCompleted?.Invoke(ordinaryReceipt);
            }

            Debug.Log(
                "[StageRuntime] Battle completion applied. "
                + $"nodeId={completedNodeId}.");
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

        public void UnlockRoute(string routeId)
        {
            if (string.IsNullOrWhiteSpace(routeId))
            {
                Debug.LogWarning("[StageManager] UnlockRoute failed. routeId is empty.");
                return;
            }

            if (runtimeData == null)
            {
                Debug.LogWarning("[StageManager] UnlockRoute failed. RuntimeData is null.");
                return;
            }

            runtimeData.UnlockRoute(routeId);

            if (runtimeData.currentGraph == null || runtimeData.currentGraph.nodes == null)
            {
                Debug.LogWarning("[StageManager] UnlockRoute failed. Current graph is null.");
                return;
            }

            foreach (RoundNode node in runtimeData.currentGraph.nodes)
            {
                if (node == null)
                {
                    continue;
                }

                if (node.nodeId != routeId)
                {
                    continue;
                }

                node.Reveal();
                Debug.Log($"[StageManager] Route node revealed. routeId={routeId}");
            }
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
