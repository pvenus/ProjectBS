using System;
using System.Collections.Generic;
using Battle;
using Item;

namespace Stage
{
    public enum PortfolioOutcomeOperationKind
    {
        VitalDelta = 10,
        InventoryGrant = 20,
        SetRunFlagTrue = 30,
        RevealImmediateSuccessorPurpose = 40,
        BeginBattle = 50,
        CommitImmediateSuccessorRoute = 60,
        GoldSpend = 70,
        GoldGrant = 80,
        RelicRouteTrade = 90
    }

    public enum ImmediateSuccessorRouteSelectionMode
    {
        None = 0,
        ShortestRemainingToSectionExit = 10,
        LongestRemainingToSectionExit = 20,
        BattlePurposeThenShortestRemainingToSectionExit = 30
    }

    [Serializable]
    public sealed class PortfolioOutcomeOperationData
    {
        public PortfolioOutcomeOperationKind kind;
        public int maxHpPercent;
        public bool nonlethal;
        public string targetId;
        public string snapshotId;
        public int count;
        public int amount;
        public bool unique;
        public bool allowEffectiveZero;
        public ImmediateSuccessorRouteSelectionMode selectionMode;
        public string sourceEntitlementId;
        public RelicSO relic;
        public RelicPoolSO relicPool;
        public BattleSO battle;
    }

    [Serializable]
    public sealed class PortfolioOutcomeExecutionData : ChoiceExecutionData
    {
        public int schemaVersion = 1;
        public string eventId;
        public string nodeId;
        public string sourcePopupId;
        public string choiceId;
        public string resultId;
        public string reservationId;
        public List<PortfolioOutcomeOperationData> operations = new();
    }

    public sealed class PortfolioOutcomeChoiceExecutionExecutor : IChoiceExecutionExecutor
    {
        public ChoiceExecutionType ExecutionType => ChoiceExecutionType.PortfolioOutcome;

        public bool TryExecute(ChoiceExecutionData data, ChoiceExecutionContext context,
            out string error)
        {
            error = string.Empty;
            if (data is not PortfolioOutcomeExecutionData outcome)
            {
                error = "PORTFOLIO_OUTCOME_DATA_INVALID";
                return false;
            }
            if (context?.ApplyPortfolioOutcome == null)
            {
                error = "PORTFOLIO_OUTCOME_CONTEXT_MISSING";
                return false;
            }
            return context.ApplyPortfolioOutcome(outcome, out error);
        }
    }
}
