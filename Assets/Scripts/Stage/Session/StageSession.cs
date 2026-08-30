using System;
using Bless;
using Item;
using Currency;
using Stage;
using Progression;
using Progression.RandomGrowth;
using Party;

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

        public RandomGrowthSessionOwnership RandomGrowthSession { get; private set; } = new();
        public SafeGrowthPlacementOwnership SafeGrowthPlacement { get; private set; } = new();
        public SafeGrowthPlacementRequest SafeGrowthPlacementRequest { get; private set; }
        public SafeGrowthInteractionOwnership SafeGrowthInteraction { get; private set; } = new();
        public SafeGrowthRuntimeComposition SafeGrowthRuntime { get; private set; }
        public SafeGrowthRouteEncounterReceipt SafeGrowthRouteEncounter { get; private set; }
        public SafeGrowthPendingConfirmContext SafeGrowthPendingConfirm { get; private set; }
        public PortfolioOutcomeOwnership PortfolioOutcomes { get; private set; } = new();
        public OrdinaryBattleCompletionOwnership OrdinaryBattles { get; private set; } = new();
        public PortfolioRandomGrowthInteractionOwnership PortfolioRandomGrowth { get; private set; } = new();
        public PortfolioRandomGrowthRuntime PortfolioRandomGrowthRuntime { get; private set; }

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
            RandomGrowthSession?.Clear();
            SafeGrowthPlacement?.Clear();
            SafeGrowthInteraction?.Clear();
            SafeGrowthRuntime = null;
            SafeGrowthRouteEncounter = null;
            SafeGrowthPendingConfirm = null;
            SafeGrowthPlacementRequest = null;
            PortfolioOutcomes?.ResetForNewRun(string.Empty);
            OrdinaryBattles?.ResetForNewRun();
            PortfolioRandomGrowth?.ResetForNewRun(string.Empty);
            PortfolioRandomGrowthRuntime = null;
        }

        public void ResetRandomGrowthForNewRun(ProgressionRunId runId)
        {
            RandomGrowthSession ??= new RandomGrowthSessionOwnership();
            RandomGrowthSession.ResetForNewRun(runId);
            SafeGrowthPlacement ??= new SafeGrowthPlacementOwnership();
            SafeGrowthPlacement.Clear();
            SafeGrowthInteraction ??= new SafeGrowthInteractionOwnership();
            SafeGrowthInteraction.ResetForNewRun(runId);
            SafeGrowthRuntime = null;
            SafeGrowthRouteEncounter = null;
            SafeGrowthPendingConfirm = null;
            SafeGrowthPlacementRequest = null;
            PortfolioOutcomes ??= new PortfolioOutcomeOwnership();
            PortfolioOutcomes.ResetForNewRun(runId.Value);
            OrdinaryBattles ??= new OrdinaryBattleCompletionOwnership();
            OrdinaryBattles.ResetForNewRun();
            PortfolioRandomGrowth ??= new PortfolioRandomGrowthInteractionOwnership();
            PortfolioRandomGrowth.ResetForNewRun(runId.Value);
            PortfolioRandomGrowthRuntime = null;
        }

        public bool ConfigurePortfolioRandomGrowthRuntime(
            ProgressionSession progressionSession, PartyRuntimeData party)
        {
            if (progressionSession?.HasActiveRun != true || party == null) return false;
            PortfolioRandomGrowthRuntime ??= new PortfolioRandomGrowthRuntime(progressionSession, party);
            return PortfolioRandomGrowthRuntime.IsReady;
        }

        public bool ConfigureSafeGrowthRuntime(ProgressionSession progressionSession)
        {
            if (progressionSession?.HasActiveRun != true
                || !RandomGrowthSession.RunId.Equals(progressionSession.RunId))
            {
                SafeGrowthRuntime = null;
                return false;
            }

            if (SafeGrowthRuntime != null
                && SafeGrowthRuntime.Matches(progressionSession.RunId, SafeGrowthInteraction))
            {
                return true;
            }

            SafeGrowthRuntime = new SafeGrowthRuntimeComposition(
                progressionSession, SafeGrowthInteraction);
            return SafeGrowthRuntime.IsReady;
        }

        public bool TryStoreSafeGrowthRouteEncounter(SafeGrowthRouteEncounterReceipt receipt)
        {
            if (receipt == null || !receipt.IsValid) return false;
            if (SafeGrowthRouteEncounter == null)
            {
                SafeGrowthRouteEncounter = receipt;
                return true;
            }
            return SafeGrowthRouteEncounter.SameIdentity(receipt);
        }

        public bool TryStoreSafeGrowthPending(SafeGrowthPendingConfirmContext context)
        {
            if (context == null || !context.IsValid) return false;
            if (SafeGrowthPendingConfirm == null)
            {
                SafeGrowthPendingConfirm = context;
                return true;
            }
            if (!SafeGrowthPendingConfirm.SameIdentity(context)) return false;
            SafeGrowthPendingConfirm = context;
            return true;
        }

        public void ClearSafeGrowthPending() => SafeGrowthPendingConfirm = null;

        public void ConfigureSafeGrowthPlacement(SafeGrowthPlacementRequest request) =>
            SafeGrowthPlacementRequest = request;

        public RandomGrowthSessionCommitResult TryCommitChapter1RandomGrowthGraph(
            ProgressionRunId runId,
            string chapterId,
            int leftSectionSlotCount,
            int rightSectionSlotCount,
            IRandomGrowthSessionIdentityFactory identityFactory,
            out RandomGrowthManifest manifest)
        {
            RandomGrowthSession ??= new RandomGrowthSessionOwnership();
            return RandomGrowthSession.TryCommitChapter1Graph(
                runId,
                chapterId,
                leftSectionSlotCount,
                rightSectionSlotCount,
                identityFactory,
                out manifest);
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
            GameSession gameSession = GameSession.Instance;
            PortfolioOutcomeRuntimeService outcomeService = null;
            bool hasPortfolioBattle = PortfolioOutcomes?.PendingBattle != null;
            if (hasPortfolioBattle)
            {
                outcomeService = new PortfolioOutcomeRuntimeService();
                if (gameSession == null || !outcomeService.TryFinalizeCompletedBattle(
                        gameSession, completedNodeId, out error)) return false;
            }
            var ordinaryService = new OrdinaryBattleCompletionService();
            bool hasOrdinaryBattle = OrdinaryBattles?.Pending != null;
            if (hasOrdinaryBattle
                && !ordinaryService.TryFinalize(this, battleSession, completedNodeId, out error))
                return false;
            if (!graph.TryCompleteCurrentNode(completedNodeId))
            {
                if (hasPortfolioBattle
                    && !outcomeService.RollbackFinalizedBattle(gameSession, completedNodeId))
                {
                    error = "PORTFOLIO_OUTCOME_GRAPH_ROLLBACK_CONFLICT";
                    return false;
                }
                if (hasOrdinaryBattle && !ordinaryService.RollbackFinalized(this))
                {
                    error = "ORDINARY_BATTLE_GRAPH_ROLLBACK_CONFLICT";
                    return false;
                }
                error =
                    "STAGE_NODE_MISMATCH: "
                    + $"expected={completedNodeId}, "
                    + $"current={completedNode.nodeId}.";
                return false;
            }

            if (hasPortfolioBattle
                && !outcomeService.CommitFinalizedBattle(gameSession, completedNodeId))
            {
                error = "PORTFOLIO_OUTCOME_PENDING_CONSUME_FAILED";
                return false;
            }
            if (hasOrdinaryBattle && !ordinaryService.CommitFinalized(this))
            {
                error = "ORDINARY_BATTLE_PENDING_CONSUME_FAILED";
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
