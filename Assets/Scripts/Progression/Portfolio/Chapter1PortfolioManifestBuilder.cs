using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Progression.Portfolio
{
    public static class Chapter1PortfolioIds
    {
        public const string SafeEvent = "event.act1.random_growth.02.windworn_sword_marks";
        public const string BattleEvent = "event.act1.random_growth.03.cut_signal_rope_ambush";
        public const string SmithyEvent = "event.act1.random_growth.01.crying_bell_smithy_trial";
        public const string OriginalSmithyEvent = "event.act1.random_event.16.crying_bell_smithy";
        public const string SmithyExclusion = "motif.exclusive.crying_bell_smithy";
    }

    public sealed class Chapter1PortfolioManifestBuilder
    {
        public const string GeneratorVersion = "chapter1.portfolio48.manifest.v1";
        public const string LateAppearanceDomain = "chapter1.portfolio48.late-appearance.v1";
        public const string FallbackDomain = "chapter1.portfolio48.fallback.v1";
        public const string LateReservationId = "reservation.act1.chapter01.random_growth.before_episode06";
        public static readonly IReadOnlyList<PortfolioEventDescriptor> PortfolioB1Registry =
            Array.AsReadOnly(new[]
            {
                new PortfolioEventDescriptor("event.act1.random_event.21.breath_between_water_drops",
                    PortfolioPurpose.Growth, "limestone-water-rhythm"),
                new PortfolioEventDescriptor("event.act1.random_event.22.sleeping_hawk_watch",
                    PortfolioPurpose.Growth, "sleeping-hawk-night-watch"),
                new PortfolioEventDescriptor("event.act1.random_event.23.temple_hundred_eight_steps",
                    PortfolioPurpose.Growth, "worn-temple-steps")
            });

        public Chapter1PortfolioManifest Build(string runId, string stageGenerationId,
            IEnumerable<PortfolioEventDescriptor> registry, int logicalEncounterCount = 12,
            IEnumerable<string> characterIds = null)
        {
            HashSet<string> roster = new((characterIds ?? Array.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value)), StringComparer.Ordinal);
            List<PortfolioEventDescriptor> catalog = (registry ?? Array.Empty<PortfolioEventDescriptor>())
                .Concat(PortfolioB1Registry)
                .Where(x => x != null && x.IsValid && x.CapabilityEnabled && !x.IsFollowup)
                .Select(ApplyCanonicalExclusions)
                .Where(x => IsCharacterEligible(x, roster))
                .GroupBy(x => x.EventId, StringComparer.Ordinal).Select(g => g.First())
                .OrderBy(x => x.EventId, StringComparer.Ordinal).ToList();
            if (string.IsNullOrWhiteSpace(runId) || string.IsNullOrWhiteSpace(stageGenerationId)
                || logicalEncounterCount != 12)
                return Suppressed(PortfolioManifestStatus.SuppressedInvalidInput, runId, stageGenerationId);

            byte[] seed = CanonicalOfferHash.Compute(GeneratorVersion, "selector", runId, stageGenerationId);
            FixedXoshiro256StarStar random = new(seed);
            List<PortfolioEventDescriptor> encounters = SelectEncounters(catalog, logicalEncounterCount, random);
            if (encounters.Count != logicalEncounterCount)
                return Suppressed(PortfolioManifestStatus.SuppressedInvalidInput, runId, stageGenerationId);

            HashSet<string> reserved = new(encounters.Select(x => x.RootKey), StringComparer.Ordinal);
            List<PortfolioEventDescriptor> fallbacks = catalog.Where(x =>
                    x.Purpose != PortfolioPurpose.Growth && !reserved.Contains(x.RootKey))
                .OrderBy(x => x.EventId, StringComparer.Ordinal).ToList();
            if (fallbacks.Count < 3)
                return Suppressed(PortfolioManifestStatus.SuppressedMissingFallback, runId, stageGenerationId);
            FixedXoshiro256StarStar fallbackRandom = new(CanonicalOfferHash.Compute(
                FallbackDomain, GeneratorVersion, runId, stageGenerationId));

            List<GrowthCandidateReservation> candidates = new();
            AddCandidate(candidates, GrowthCandidateKind.Safe, Chapter1PortfolioIds.SafeEvent,
                "reservation.act1.chapter01.random_growth.after_episode02", "sec_ep_2_to_ep_3_1", "sec_ep_2_to_ep_3_2",
                "slot_430_2085", "slot_1370_2085", 0, true, TakeFallback(fallbacks, fallbackRandom), 2);
            AddCandidate(candidates, GrowthCandidateKind.Battle, Chapter1PortfolioIds.BattleEvent,
                "reservation.act1.chapter01.random_growth.after_episode04", "sec_ep_4_1_to_ep_5_1", "sec_ep_4_1_to_ep_5_2",
                "slot_430_855", "slot_1370_855", 0, true, TakeFallback(fallbacks, fallbackRandom), 3);
            int lateRoll = ComputeLateAppearanceRoll(runId, stageGenerationId);
            AddCandidate(candidates, GrowthCandidateKind.Smithy, Chapter1PortfolioIds.SmithyEvent,
                LateReservationId, "sec_ep_5_1_to_ep_6", "sec_ep_5_2_to_ep_6",
                "slot_430_250", "slot_1370_250", lateRoll, IsLateAppearanceRoll(lateRoll),
                TakeFallback(fallbacks, fallbackRandom), 3);

            string fingerprint = CanonicalOfferHash.ComputeHex(new[] { "chapter1.portfolio48.manifest.v1", runId,
                stageGenerationId }.Concat(encounters.Select(x => x.EventId)).Concat(candidates.SelectMany(x => new[] {
                    x.EventId, x.ReservationId, x.FallbackEventId, x.AppearanceRoll.ToString(CultureInfo.InvariantCulture),
                    x.Appeared ? "1" : "0", x.TargetCount.ToString(CultureInfo.InvariantCulture) })));
            return new Chapter1PortfolioManifest(PortfolioManifestStatus.Ready, runId, stageGenerationId,
                candidates, encounters, fingerprint);
        }

        private static bool IsCharacterEligible(
            PortfolioEventDescriptor descriptor, IReadOnlyCollection<string> roster)
        {
            if (descriptor == null || string.IsNullOrWhiteSpace(descriptor.RequiredCharacterId))
                return true;
            return roster.Any(value => string.Equals(value, descriptor.RequiredCharacterId,
                    StringComparison.Ordinal)
                || value.StartsWith(descriptor.RequiredCharacterId + ".",
                    StringComparison.Ordinal));
        }

        public static int ComputeLateAppearanceRoll(string runId, string stageGenerationId)
        {
            if (string.IsNullOrWhiteSpace(runId) || string.IsNullOrWhiteSpace(stageGenerationId))
                throw new ArgumentException("Canonical run and stage identities are required.");
            byte[] seed = CanonicalOfferHash.Compute(
                LateAppearanceDomain,
                GeneratorVersion,
                runId,
                stageGenerationId,
                LateReservationId);
            return new FixedXoshiro256StarStar(seed).NextIndex(10000);
        }

        public static bool IsLateAppearanceRoll(int rawRoll)
        {
            if (rawRoll < 0 || rawRoll >= 10000) throw new ArgumentOutOfRangeException(nameof(rawRoll));
            return rawRoll < 4000;
        }

        private static List<PortfolioEventDescriptor> SelectEncounters(List<PortfolioEventDescriptor> catalog,
            int count, FixedXoshiro256StarStar random)
        {
            List<PortfolioEventDescriptor> result = new();
            List<PortfolioEventDescriptor> remaining = new(catalog);
            // Canonical Chapter 1 minima occupy eight of the twelve logical
            // encounters. The remaining four are selected from the full eligible
            // registry while preserving the same constraints.
            PortfolioPurpose[] required =
            {
                PortfolioPurpose.Growth,
                PortfolioPurpose.Recovery,
                PortfolioPurpose.Battle,
                PortfolioPurpose.Gold,
                PortfolioPurpose.Route,
                PortfolioPurpose.World,
                PortfolioPurpose.Growth,
                PortfolioPurpose.Battle
            };
            foreach (PortfolioPurpose purpose in required)
            {
                List<PortfolioEventDescriptor> feasible = remaining
                    .Where(x => x.Purpose == purpose && IsFeasible(result, x)).ToList();
                if (feasible.Count == 0) return new List<PortfolioEventDescriptor>();
                PortfolioEventDescriptor chosen = feasible[random.NextIndex(feasible.Count)];
                result.Add(chosen);
                remaining.Remove(chosen);
                remaining.RemoveAll(x => string.Equals(x.RootKey, chosen.RootKey, StringComparison.Ordinal));
            }
            while (result.Count < count)
            {
                List<PortfolioEventDescriptor> feasible = remaining.Where(x => IsFeasible(result, x)).ToList();
                if (feasible.Count == 0) return new List<PortfolioEventDescriptor>();
                PortfolioEventDescriptor chosen = feasible[random.NextIndex(feasible.Count)];
                result.Add(chosen);
                remaining.Remove(chosen);
                remaining.RemoveAll(x => string.Equals(x.RootKey, chosen.RootKey, StringComparison.Ordinal));
            }
            return result;
        }

        private static bool IsFeasible(IReadOnlyList<PortfolioEventDescriptor> selected, PortfolioEventDescriptor next)
        {
            if (Chapter1BattleReuseSelectionContract.Conflicts(selected, next)) return false;
            if (selected.Any(x => string.Equals(x.RootKey, next.RootKey, StringComparison.Ordinal))) return false;
            if (selected.Count > 0 && selected[selected.Count - 1].Purpose == next.Purpose) return false;
            if (!string.IsNullOrWhiteSpace(next.Motif)
                && selected.Skip(Math.Max(0, selected.Count - 3)).Any(x => string.Equals(x.Motif, next.Motif, StringComparison.Ordinal))) return false;
            if (!string.IsNullOrWhiteSpace(next.ExclusionGroup)
                && selected.Any(x => string.Equals(x.ExclusionGroup, next.ExclusionGroup, StringComparison.Ordinal))) return false;
            return true;
        }

        private static PortfolioEventDescriptor ApplyCanonicalExclusions(
            PortfolioEventDescriptor descriptor)
        {
            if (descriptor.EventId == Chapter1Event25SelectionContract.OriginalEvent18Id)
                return Chapter1Event25SelectionContract.CreateOriginalEvent18();
            if (descriptor.EventId == Chapter1Event25SelectionContract.Event25Id)
                return Chapter1Event25SelectionContract.CreateEvent25();
            if (descriptor.EventId == Chapter1Event37SelectionContract.OriginalEvent01Id)
                return Chapter1Event37SelectionContract.CreateOriginalEvent01();
            if (descriptor.EventId == Chapter1Event37SelectionContract.Event37Id)
                return Chapter1Event37SelectionContract.CreateEvent37();
            return descriptor;
        }

        private static string TakeFallback(List<PortfolioEventDescriptor> values, FixedXoshiro256StarStar random)
        { int index = random.NextIndex(values.Count); string id = values[index].EventId; values.RemoveAt(index); return id; }
        private static void AddCandidate(List<GrowthCandidateReservation> values, GrowthCandidateKind kind,
            string eventId, string reservationId, string leftSection, string rightSection, string leftSlot,
            string rightSlot, int roll, bool appeared, string fallback, int targetCount) => values.Add(
                new GrowthCandidateReservation(kind, eventId, reservationId, leftSection, rightSection,
                    leftSlot, rightSlot, roll, appeared, fallback, targetCount));
        private static Chapter1PortfolioManifest Suppressed(PortfolioManifestStatus status, string runId, string stageId) =>
            new(status, runId, stageId, Array.Empty<GrowthCandidateReservation>(),
                Array.Empty<PortfolioEventDescriptor>(), string.Empty);
    }
}
