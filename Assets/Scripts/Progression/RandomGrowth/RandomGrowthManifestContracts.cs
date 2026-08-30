using System;
using System.Collections.Generic;
using System.Linq;

namespace Progression.RandomGrowth
{
    public static class RandomGrowthManifestConstants
    {
        public const string GeneratorVersion = "chapter1.random_growth_manifest.v1";
        public const string ReservationId = "reservation.act1.chapter01.random_growth.before_episode06";
        public const string LeftSectionId = "sec_ep_5_1_to_ep_6";
        public const string RightSectionId = "sec_ep_5_2_to_ep_6";
        public const int RollRange = 10000;
        public const int AppearanceThreshold = 4000;
    }

    public enum RandomGrowthManifestStatus
    {
        Ready = 0,
        SuppressedInvalidIdentity = 10,
        SuppressedIncompatibleVersion = 20,
        SuppressedInvalidReservation = 30,
        SuppressedInvalidSectionCardinality = 40,
        SuppressedCorruptManifest = 50
    }

    public sealed class RandomGrowthManifestRequest
    {
        public RandomGrowthManifestRequest(
            ProgressionRunId runId,
            string stageGenerationId,
            string generatorVersion,
            string reservationId,
            int leftSectionSlotCount,
            int rightSectionSlotCount)
        {
            RunId = runId;
            StageGenerationId = stageGenerationId;
            GeneratorVersion = generatorVersion;
            ReservationId = reservationId;
            LeftSectionSlotCount = leftSectionSlotCount;
            RightSectionSlotCount = rightSectionSlotCount;
        }

        public ProgressionRunId RunId { get; }
        public string StageGenerationId { get; }
        public string GeneratorVersion { get; }
        public string ReservationId { get; }
        public int LeftSectionSlotCount { get; }
        public int RightSectionSlotCount { get; }
    }

    public sealed class RandomGrowthReservationProjection
    {
        internal RandomGrowthReservationProjection(
            string sectionId,
            int ordinal,
            string logicalEncounterKey)
        {
            SectionId = sectionId;
            Ordinal = ordinal;
            LogicalEncounterKey = logicalEncounterKey;
        }

        public string SectionId { get; }
        public int Ordinal { get; }
        public string LogicalEncounterKey { get; }
    }

    public sealed class RandomGrowthManifest
    {
        internal RandomGrowthManifest(
            RandomGrowthManifestStatus status,
            ProgressionRunId runId,
            string stageGenerationId,
            string generatorVersion,
            string reservationId,
            int rawRoll,
            bool appeared,
            int ordinal,
            string logicalEncounterKey,
            IReadOnlyList<RandomGrowthReservationProjection> projections,
            string fingerprint)
        {
            Status = status;
            RunId = runId;
            StageGenerationId = stageGenerationId ?? string.Empty;
            GeneratorVersion = generatorVersion ?? string.Empty;
            ReservationId = reservationId ?? string.Empty;
            RawRoll = rawRoll;
            Appeared = appeared;
            Ordinal = ordinal;
            LogicalEncounterKey = logicalEncounterKey ?? string.Empty;
            Projections = Array.AsReadOnly((projections ?? Array.Empty<RandomGrowthReservationProjection>()).ToArray());
            Fingerprint = fingerprint ?? string.Empty;
        }

        public RandomGrowthManifestStatus Status { get; }
        public ProgressionRunId RunId { get; }
        public string StageGenerationId { get; }
        public string GeneratorVersion { get; }
        public string ReservationId { get; }
        public int RawRoll { get; }
        public bool Appeared { get; }
        public int Ordinal { get; }
        public string LogicalEncounterKey { get; }
        public IReadOnlyList<RandomGrowthReservationProjection> Projections { get; }
        public string Fingerprint { get; }

        public bool TryGetProjection(
            string sectionId,
            out RandomGrowthReservationProjection projection)
        {
            projection = Projections.SingleOrDefault(value =>
                string.Equals(value.SectionId, sectionId, StringComparison.Ordinal));
            return projection != null;
        }
    }

    public enum RandomGrowthEncounterResult
    {
        Encountered = 0,
        AlreadyEncountered = 10,
        Suppressed = 20,
        OffRoute = 30
    }

    public sealed class RandomGrowthReservationState
    {
        public RandomGrowthReservationState(RandomGrowthManifest manifest)
            : this(manifest, false, string.Empty)
        {
        }

        private RandomGrowthReservationState(
            RandomGrowthManifest manifest,
            bool encountered,
            string encounteredSectionId)
        {
            Manifest = manifest ?? throw new ArgumentNullException(nameof(manifest));
            Encountered = encountered;
            EncounteredSectionId = encounteredSectionId ?? string.Empty;
        }

        public RandomGrowthManifest Manifest { get; }
        public bool Encountered { get; }
        public string EncounteredSectionId { get; }

        public RandomGrowthEncounterResult TryEncounter(
            string sectionId,
            out RandomGrowthReservationState next)
        {
            next = this;
            if (Manifest.Status != RandomGrowthManifestStatus.Ready || !Manifest.Appeared)
            {
                return RandomGrowthEncounterResult.Suppressed;
            }

            if (!Manifest.TryGetProjection(sectionId, out _))
            {
                return RandomGrowthEncounterResult.OffRoute;
            }

            if (Encountered)
            {
                return RandomGrowthEncounterResult.AlreadyEncountered;
            }

            next = new RandomGrowthReservationState(Manifest, true, sectionId);
            return RandomGrowthEncounterResult.Encountered;
        }
    }
}
