using System;

namespace Progression.RandomGrowth
{
    public interface IRandomGrowthSessionIdentityFactory
    {
        ProgressionRunId CreateRunId();
        string CreateStageGenerationId(ProgressionRunId runId, string chapterId);
    }

    public sealed class GuidRandomGrowthSessionIdentityFactory : IRandomGrowthSessionIdentityFactory
    {
        public ProgressionRunId CreateRunId() => ProgressionRunId.NewId();

        public string CreateStageGenerationId(ProgressionRunId runId, string chapterId) =>
            Guid.NewGuid().ToString("N");
    }

    public enum RandomGrowthSessionCommitResult
    {
        Created = 0,
        Reused = 10,
        Suppressed = 20,
        IgnoredOtherChapter = 30
    }

    public sealed class RandomGrowthSessionOwnership
    {
        public const string Chapter1Id = "stage.chapter1";

        private bool stageGenerationCreationAttempted;

        public ProgressionRunId RunId { get; private set; }
        public string ChapterId { get; private set; } = string.Empty;
        public string StageGenerationId { get; private set; } = string.Empty;
        public RandomGrowthManifest StoredManifest { get; private set; }
        public RandomGrowthReservationState ReservationState { get; private set; }
        public RandomGrowthManifestStatus SuppressionStatus { get; private set; }
        public bool IsSuppressed => SuppressionStatus != RandomGrowthManifestStatus.Ready;

        public void ResetForNewRun(ProgressionRunId runId)
        {
            if (!runId.IsValid)
            {
                throw new ArgumentException("A valid run ID is required.", nameof(runId));
            }

            RunId = runId;
            ChapterId = string.Empty;
            StageGenerationId = string.Empty;
            StoredManifest = null;
            ReservationState = null;
            SuppressionStatus = RandomGrowthManifestStatus.Ready;
            stageGenerationCreationAttempted = false;
        }

        public void Clear()
        {
            RunId = default;
            ChapterId = string.Empty;
            StageGenerationId = string.Empty;
            StoredManifest = null;
            ReservationState = null;
            SuppressionStatus = RandomGrowthManifestStatus.Ready;
            stageGenerationCreationAttempted = false;
        }

        public RandomGrowthSessionCommitResult TryCommitChapter1Graph(
            ProgressionRunId runId,
            string chapterId,
            int leftSectionSlotCount,
            int rightSectionSlotCount,
            IRandomGrowthSessionIdentityFactory identityFactory,
            out RandomGrowthManifest manifest)
        {
            manifest = StoredManifest;
            if (!string.Equals(chapterId, Chapter1Id, StringComparison.Ordinal))
            {
                return RandomGrowthSessionCommitResult.IgnoredOtherChapter;
            }

            if (!RunId.IsValid || !RunId.Equals(runId))
            {
                SuppressionStatus = RandomGrowthManifestStatus.SuppressedInvalidIdentity;
                manifest = StoredManifest;
                return RandomGrowthSessionCommitResult.Suppressed;
            }

            if (identityFactory == null)
            {
                throw new ArgumentNullException(nameof(identityFactory));
            }

            RandomGrowthManifestBuilder builder = new();
            if (stageGenerationCreationAttempted)
            {
                RandomGrowthManifestRequest reuseRequest = Request(
                    runId,
                    StageGenerationId,
                    leftSectionSlotCount,
                    rightSectionSlotCount);
                StoredManifest = builder.UseStoredOrSuppress(StoredManifest, reuseRequest);
                SuppressionStatus = StoredManifest.Status;
                if (IsSuppressed)
                {
                    ReservationState = null;
                }
                manifest = StoredManifest;
                return IsSuppressed
                    ? RandomGrowthSessionCommitResult.Suppressed
                    : RandomGrowthSessionCommitResult.Reused;
            }

            stageGenerationCreationAttempted = true;
            ChapterId = chapterId;
            StageGenerationId = identityFactory.CreateStageGenerationId(runId, chapterId) ?? string.Empty;
            RandomGrowthManifestRequest request = Request(
                runId,
                StageGenerationId,
                leftSectionSlotCount,
                rightSectionSlotCount);
            StoredManifest = builder.Build(request);
            SuppressionStatus = StoredManifest.Status;
            ReservationState = IsSuppressed
                ? null
                : new RandomGrowthReservationState(StoredManifest);
            manifest = StoredManifest;
            return IsSuppressed
                ? RandomGrowthSessionCommitResult.Suppressed
                : RandomGrowthSessionCommitResult.Created;
        }

        public RandomGrowthEncounterResult TryRecordEncounter(string sectionId)
        {
            if (ReservationState == null)
            {
                return RandomGrowthEncounterResult.Suppressed;
            }

            RandomGrowthEncounterResult result = ReservationState.TryEncounter(sectionId, out var next);
            if (result == RandomGrowthEncounterResult.Encountered)
            {
                ReservationState = next;
            }

            return result;
        }

        internal void ReplaceStoredManifestForTests(RandomGrowthManifest manifest)
        {
            StoredManifest = manifest;
        }

        private static RandomGrowthManifestRequest Request(
            ProgressionRunId runId,
            string stageGenerationId,
            int leftSectionSlotCount,
            int rightSectionSlotCount) =>
            new(
                runId,
                stageGenerationId,
                RandomGrowthManifestConstants.GeneratorVersion,
                RandomGrowthManifestConstants.ReservationId,
                leftSectionSlotCount,
                rightSectionSlotCount);
    }
}
