using System;
using System.Collections.Generic;
using System.Globalization;

namespace Progression.RandomGrowth
{
    public sealed class RandomGrowthManifestBuilder
    {
        public RandomGrowthManifest Build(RandomGrowthManifestRequest request)
        {
            RandomGrowthManifestStatus status = Validate(request);
            if (status != RandomGrowthManifestStatus.Ready)
            {
                return Suppressed(request, status);
            }

            byte[] seed = CanonicalOfferHash.Compute(
                RandomGrowthManifestConstants.GeneratorVersion,
                request.RunId.Value,
                request.StageGenerationId,
                RandomGrowthManifestConstants.ReservationId,
                request.LeftSectionSlotCount.ToString(CultureInfo.InvariantCulture),
                request.RightSectionSlotCount.ToString(CultureInfo.InvariantCulture));
            FixedXoshiro256StarStar random = new(seed);
            int rawRoll = random.NextIndex(RandomGrowthManifestConstants.RollRange);
            bool appeared = IsAppearanceRoll(rawRoll);
            int ordinal = appeared
                ? random.NextIndex(Math.Min(request.LeftSectionSlotCount, request.RightSectionSlotCount))
                : -1;
            string encounterKey = RuntimeKey(
                request.RunId,
                request.StageGenerationId,
                request.ReservationId);
            List<RandomGrowthReservationProjection> projections = new();
            if (appeared)
            {
                projections.Add(new RandomGrowthReservationProjection(
                    RandomGrowthManifestConstants.LeftSectionId,
                    ordinal,
                    encounterKey));
                projections.Add(new RandomGrowthReservationProjection(
                    RandomGrowthManifestConstants.RightSectionId,
                    ordinal,
                    encounterKey));
            }

            string fingerprint = Fingerprint(
                request.RunId,
                request.StageGenerationId,
                request.ReservationId,
                rawRoll,
                appeared,
                ordinal,
                encounterKey);
            return new RandomGrowthManifest(
                RandomGrowthManifestStatus.Ready,
                request.RunId,
                request.StageGenerationId,
                request.GeneratorVersion,
                request.ReservationId,
                rawRoll,
                appeared,
                ordinal,
                encounterKey,
                projections,
                fingerprint);
        }

        public RandomGrowthManifest UseStoredOrSuppress(
            RandomGrowthManifest stored,
            RandomGrowthManifestRequest expected)
        {
            RandomGrowthManifestStatus requestStatus = Validate(expected);
            if (requestStatus != RandomGrowthManifestStatus.Ready)
            {
                return Suppressed(expected, requestStatus);
            }

            if (stored == null)
            {
                return Suppressed(expected, RandomGrowthManifestStatus.SuppressedCorruptManifest);
            }

            if (!string.Equals(stored.GeneratorVersion, expected.GeneratorVersion, StringComparison.Ordinal))
            {
                return Suppressed(expected, RandomGrowthManifestStatus.SuppressedIncompatibleVersion);
            }

            if (!stored.RunId.Equals(expected.RunId)
                || !string.Equals(stored.StageGenerationId, expected.StageGenerationId, StringComparison.Ordinal))
            {
                return Suppressed(expected, RandomGrowthManifestStatus.SuppressedInvalidIdentity);
            }

            if (!string.Equals(stored.ReservationId, expected.ReservationId, StringComparison.Ordinal))
            {
                return Suppressed(expected, RandomGrowthManifestStatus.SuppressedInvalidReservation);
            }

            string encounterKey = RuntimeKey(
                expected.RunId,
                expected.StageGenerationId,
                expected.ReservationId);
            bool validShape = stored.Status == RandomGrowthManifestStatus.Ready
                && stored.RawRoll >= 0
                && stored.RawRoll < RandomGrowthManifestConstants.RollRange
                && stored.Appeared == IsAppearanceRoll(stored.RawRoll)
                && string.Equals(stored.LogicalEncounterKey, encounterKey, StringComparison.Ordinal)
                && string.Equals(
                    stored.Fingerprint,
                    Fingerprint(
                        stored.RunId,
                        stored.StageGenerationId,
                        stored.ReservationId,
                        stored.RawRoll,
                        stored.Appeared,
                        stored.Ordinal,
                        stored.LogicalEncounterKey),
                    StringComparison.Ordinal);
            if (stored.Appeared)
            {
                int cardinality = Math.Min(
                    expected.LeftSectionSlotCount,
                    expected.RightSectionSlotCount);
                validShape = validShape
                    && stored.Ordinal >= 0
                    && stored.Ordinal < cardinality
                    && stored.Projections.Count == 2
                    && ProjectionValid(stored, RandomGrowthManifestConstants.LeftSectionId)
                    && ProjectionValid(stored, RandomGrowthManifestConstants.RightSectionId);
            }
            else
            {
                validShape = validShape
                    && stored.Ordinal == -1
                    && stored.Projections.Count == 0;
            }

            return validShape
                ? stored
                : Suppressed(expected, RandomGrowthManifestStatus.SuppressedCorruptManifest);
        }

        public static bool IsAppearanceRoll(int rawRoll)
        {
            if (rawRoll < 0 || rawRoll >= RandomGrowthManifestConstants.RollRange)
            {
                throw new ArgumentOutOfRangeException(nameof(rawRoll));
            }

            return rawRoll < RandomGrowthManifestConstants.AppearanceThreshold;
        }

        private static RandomGrowthManifestStatus Validate(RandomGrowthManifestRequest request)
        {
            if (request == null
                || !request.RunId.IsValid
                || string.IsNullOrWhiteSpace(request.StageGenerationId))
            {
                return RandomGrowthManifestStatus.SuppressedInvalidIdentity;
            }

            if (!string.Equals(
                    request.GeneratorVersion,
                    RandomGrowthManifestConstants.GeneratorVersion,
                    StringComparison.Ordinal))
            {
                return RandomGrowthManifestStatus.SuppressedIncompatibleVersion;
            }

            if (!string.Equals(
                    request.ReservationId,
                    RandomGrowthManifestConstants.ReservationId,
                    StringComparison.Ordinal))
            {
                return RandomGrowthManifestStatus.SuppressedInvalidReservation;
            }

            if (request.LeftSectionSlotCount <= 0 || request.RightSectionSlotCount <= 0)
            {
                return RandomGrowthManifestStatus.SuppressedInvalidSectionCardinality;
            }

            return RandomGrowthManifestStatus.Ready;
        }

        private static RandomGrowthManifest Suppressed(
            RandomGrowthManifestRequest request,
            RandomGrowthManifestStatus status) =>
            new(
                status,
                request?.RunId ?? default,
                request?.StageGenerationId,
                request?.GeneratorVersion,
                request?.ReservationId,
                -1,
                false,
                -1,
                string.Empty,
                Array.Empty<RandomGrowthReservationProjection>(),
                string.Empty);

        private static string RuntimeKey(
            ProgressionRunId runId,
            string stageGenerationId,
            string reservationId) =>
            CanonicalOfferHash.ComputeHex(new[]
            {
                "random-growth-reservation-key.v1",
                runId.Value,
                stageGenerationId,
                reservationId
            });

        private static bool ProjectionValid(RandomGrowthManifest manifest, string sectionId) =>
            manifest.TryGetProjection(sectionId, out RandomGrowthReservationProjection projection)
            && projection.Ordinal == manifest.Ordinal
            && string.Equals(
                projection.LogicalEncounterKey,
                manifest.LogicalEncounterKey,
                StringComparison.Ordinal);

        private static string Fingerprint(
            ProgressionRunId runId,
            string stageGenerationId,
            string reservationId,
            int rawRoll,
            bool appeared,
            int ordinal,
            string encounterKey) =>
            CanonicalOfferHash.ComputeHex(new[]
            {
                RandomGrowthManifestConstants.GeneratorVersion,
                runId.Value,
                stageGenerationId,
                reservationId,
                rawRoll.ToString(CultureInfo.InvariantCulture),
                appeared ? "1" : "0",
                ordinal.ToString(CultureInfo.InvariantCulture),
                encounterKey
            });
    }
}
