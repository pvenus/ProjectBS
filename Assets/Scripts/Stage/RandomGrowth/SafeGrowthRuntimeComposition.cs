using Progression;
using Progression.RandomGrowth;

namespace Stage
{
    public sealed class SafeGrowthRuntimeComposition
    {
        public const string ContractVersion = "chapter1.safe-growth.runtime-composition.v1";

        public SafeGrowthRuntimeComposition(
            ProgressionSession progressionSession,
            SafeGrowthInteractionOwnership interaction,
            string contractVersion = ContractVersion)
        {
            Version = contractVersion ?? string.Empty;
            RunId = progressionSession?.RunId ?? default;
            Interaction = interaction;
            if (progressionSession?.HasActiveRun != true || interaction == null
                || Version != ContractVersion)
            {
                return;
            }

            ResultLedger = new StageEventResultLedger();
            ProgressionLedger = progressionSession.Ledger;
            Transaction = new SafeGrowthTransactionService(
                progressionSession.Ledger, ResultLedger, interaction);
            NodeCompletion = new StageAtomicNodeCompletionService();
            Coordinator = new SafeGrowthAtomicCoordinator();
            NodeOnlyFailure = new SafeGrowthNodeOnlyFailureService(ResultLedger, interaction);
            NodeOnlyFailureCoordinator = new SafeGrowthNodeOnlyFailureCoordinator();
        }

        public string Version { get; }
        public ProgressionRunId RunId { get; }
        public SafeGrowthInteractionOwnership Interaction { get; }
        public StageEventResultLedger ResultLedger { get; }
        public RunProgressionLedger ProgressionLedger { get; }
        public SafeGrowthTransactionService Transaction { get; }
        public StageAtomicNodeCompletionService NodeCompletion { get; }
        public SafeGrowthAtomicCoordinator Coordinator { get; }
        public SafeGrowthNodeOnlyFailureService NodeOnlyFailure { get; }
        public SafeGrowthNodeOnlyFailureCoordinator NodeOnlyFailureCoordinator { get; }
        public bool IsReady => Version == ContractVersion && RunId.IsValid
            && Interaction != null && ResultLedger != null && Transaction != null
            && ProgressionLedger != null && NodeCompletion != null && Coordinator != null
            && NodeOnlyFailure != null && NodeOnlyFailureCoordinator != null;

        public bool Matches(ProgressionRunId runId, SafeGrowthInteractionOwnership interaction) =>
            IsReady && RunId.Equals(runId) && ReferenceEquals(Interaction, interaction);
    }
}
