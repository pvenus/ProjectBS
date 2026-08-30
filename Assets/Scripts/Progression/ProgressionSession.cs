using System;

namespace Progression
{
    public sealed class ProgressionSession
    {
        private readonly ProgressionCapPolicy capPolicy;
        private readonly ProgressionSourceRegistry sourceRegistry;

        public ProgressionSession()
            : this(
                ProgressionCapPolicy.Chapter1P0,
                new ProgressionSourceRegistry())
        {
        }

        public ProgressionSession(
            ProgressionCapPolicy capPolicy,
            ProgressionSourceRegistry sourceRegistry)
        {
            this.capPolicy = capPolicy ?? throw new ArgumentNullException(nameof(capPolicy));
            this.sourceRegistry = sourceRegistry ?? throw new ArgumentNullException(nameof(sourceRegistry));
        }

        public ProgressionRunId RunId { get; private set; }
        public RunProgressionLedger Ledger { get; private set; }
        public bool HasActiveRun => RunId.IsValid && Ledger != null;

        public void ResetForNewRun(ProgressionRunId runId)
        {
            if (!runId.IsValid)
            {
                throw new ArgumentException("A valid run ID is required.", nameof(runId));
            }

            RunId = runId;
            Ledger = new RunProgressionLedger(runId, capPolicy, sourceRegistry);
        }

        public void Clear()
        {
            RunId = default;
            Ledger = null;
        }
    }
}
