using System;
using Bless;
using Item;
using Currency;
using Stage;

namespace Session
{
    [Serializable]
    public class StageSession
    {
        public StageRuntimeData RuntimeData;

        public StrategicSkillItemRuntimeData StrategicSkillItemRuntimeData;

        public RelicRuntimeData RelicRuntimeData;

        public BlessRuntimeData BlessRuntimeData;

        public CurrencyRutimeData CurrencyRuntimeData;

        public bool isIntroCompleted;

        public void Initialize(
            StageRuntimeData runtimeData)
        {
            RuntimeData = runtimeData;
            
            StrategicSkillItemRuntimeData ??= new StrategicSkillItemRuntimeData();
            RelicRuntimeData ??= new RelicRuntimeData();
            BlessRuntimeData ??= new BlessRuntimeData();
            CurrencyRuntimeData ??= new CurrencyRutimeData();
        }

        public void ResetRuntime()
        {
            RuntimeData = new StageRuntimeData();

            StrategicSkillItemRuntimeData = new StrategicSkillItemRuntimeData();
            RelicRuntimeData = new RelicRuntimeData();
            BlessRuntimeData = new BlessRuntimeData();
            CurrencyRuntimeData = new CurrencyRutimeData();
            isIntroCompleted = false;
        }

        public void Clear()
        {
            RuntimeData = null;

            StrategicSkillItemRuntimeData = null;
            RelicRuntimeData = null;
            BlessRuntimeData = null;
            CurrencyRuntimeData = null;
            isIntroCompleted = false;
        }

        public bool TryApplyCompletedBattleNode(
            BattleSession battleSession,
            out RoundNode completedNode,
            out bool newlyCompleted,
            out string error)
        {
            completedNode = null;
            newlyCompleted = false;
            error = string.Empty;

            if (battleSession == null
                || !battleSession.TryGetCompletedStageNodeId(
                    out string completedNodeId))
            {
                error =
                    "BATTLE_COMPLETION_MISSING: "
                    + "No completed stage node is queued.";
                return false;
            }

            StageGraph graph = RuntimeData?.currentGraph;
            if (graph == null)
            {
                error =
                    "STAGE_GRAPH_MISSING: "
                    + "Stage graph is unavailable.";
                return false;
            }

            completedNode = graph.CurrentNode;
            if (completedNode == null)
            {
                error =
                    "CURRENT_NODE_MISSING: "
                    + "Stage graph current node is unavailable.";
                return false;
            }

            bool wasCompleted = completedNode.IsCompleted;
            if (!graph.TryCompleteCurrentNode(completedNodeId))
            {
                error =
                    "STAGE_NODE_MISMATCH: "
                    + $"expected={completedNodeId}, "
                    + $"current={completedNode.nodeId}.";
                return false;
            }

            RuntimeData.currentNode = graph.CurrentNode;
            newlyCompleted = !wasCompleted;

            if (!battleSession.ConsumeCompletedStageNodeId(
                    completedNodeId))
            {
                error =
                    "BATTLE_COMPLETION_CONSUME_FAILED: "
                    + $"nodeId={completedNodeId}.";
                return false;
            }

            return true;
        }
    }
}
