using System;
using System.Collections.Generic;
using System.Linq;
using Progression.Portfolio;

namespace Stage
{
    public sealed class Chapter1WeightedEventManifestBuilder
    {
        public Chapter1WeightedEventManifest Build(WeightedPoolPlacementConfig catalog, int seed,
            IReadOnlyCollection<string> roster = null, Func<string, bool> capabilityGate = null)
        {
            string error = ValidateCatalog(catalog);
            if (!string.IsNullOrEmpty(error))
                return new Chapter1WeightedEventManifest(false, error, null);

            List<WeightedPlacementBand> slots = BuildBands(catalog);
            ShufflePhase(slots, 0, catalog.earlyBudget, new Random(seed));
            ShufflePhase(slots, catalog.earlyBudget, catalog.midBudget, new Random(seed ^ 0x1717));
            ShufflePhase(slots, catalog.earlyBudget + catalog.midBudget,
                catalog.lateBudget, new Random(seed ^ 0x3535));
            if (!InitialBandsHaveBreadth(slots, catalog, roster, capabilityGate))
                return new Chapter1WeightedEventManifest(false,
                    "WEIGHTED_PLACEMENT_INITIAL_BREADTH_INSUFFICIENT", null);
            var selected = new List<WeightedPlacementEventRow>(slots.Count);
            var assignments = new List<Chapter1WeightedEventAssignment>(slots.Count);
            bool success = TryAssign(0, slots, catalog, roster, capabilityGate,
                new Random(seed ^ 0x51f15e), selected, assignments);
            return success
                ? new Chapter1WeightedEventManifest(true, string.Empty, assignments.ToArray())
                : new Chapter1WeightedEventManifest(false,
                    "WEIGHTED_PLACEMENT_COMPLETE_MANIFEST_INFEASIBLE", null);
        }

        private static bool TryAssign(int index, IReadOnlyList<WeightedPlacementBand> slots,
            WeightedPoolPlacementConfig catalog, IReadOnlyCollection<string> roster,
            Func<string, bool> capabilityGate, Random random,
            List<WeightedPlacementEventRow> selected,
            List<Chapter1WeightedEventAssignment> assignments)
        {
            if (index == slots.Count) return true;
            WeightedPlacementBand band = slots[index];
            List<WeightedPlacementEventRow> eligible = catalog.rows.Where(row =>
                    row != null && row.primaryBand == band
                    && Chapter1RandomEventPlacementEligibilityCatalog.IsEligible(
                        row, selected, roster, capabilityGate))
                .OrderBy(row => row.order).ThenBy(row => row.eventId, StringComparer.Ordinal).ToList();
            foreach ((WeightedPlacementEventRow row, bool fallback) in WeightedOrder(
                         eligible, catalog.legacyMass, catalog.newMass, random))
            {
                selected.Add(row);
                assignments.Add(new Chapter1WeightedEventAssignment(index + 1, band, row, fallback));
                if (TryAssign(index + 1, slots, catalog, roster,
                        capabilityGate, random, selected, assignments)) return true;
                assignments.RemoveAt(assignments.Count - 1);
                selected.RemoveAt(selected.Count - 1);
            }
            return false;
        }

        private static bool InitialBandsHaveBreadth(
            IReadOnlyList<WeightedPlacementBand> slots, WeightedPoolPlacementConfig catalog,
            IReadOnlyCollection<string> roster, Func<string, bool> capabilityGate)
        {
            foreach (WeightedPlacementBand band in slots.Distinct())
            {
                List<WeightedPlacementEventRow> candidates = catalog.rows.Where(row =>
                    row != null && row.primaryBand == band
                    && Chapter1RandomEventPlacementEligibilityCatalog.IsEligible(
                        row, Array.Empty<WeightedPlacementEventRow>(), roster, capabilityGate)).ToList();
                if (candidates.Count < catalog.minEligibleCandidates
                    || candidates.Select(row => row.primaryPurpose).Distinct().Count()
                        < catalog.minEligiblePurposes) return false;
            }
            return true;
        }

        private static IEnumerable<(WeightedPlacementEventRow Row, bool Fallback)> WeightedOrder(
            List<WeightedPlacementEventRow> candidates, int legacyMass, int newMass, Random random)
        {
            var remaining = new List<WeightedPlacementEventRow>(candidates);
            while (remaining.Count > 0)
            {
                bool hasLegacy = remaining.Any(row => row.generation == WeightedPlacementGeneration.Legacy);
                bool hasNew = remaining.Any(row => row.generation == WeightedPlacementGeneration.New);
                bool fallback = hasLegacy != hasNew;
                WeightedPlacementGeneration generation;
                if (!hasLegacy) generation = WeightedPlacementGeneration.New;
                else if (!hasNew) generation = WeightedPlacementGeneration.Legacy;
                else generation = random.Next(legacyMass + newMass) < legacyMass
                    ? WeightedPlacementGeneration.Legacy : WeightedPlacementGeneration.New;
                List<WeightedPlacementEventRow> cell = remaining.Where(row =>
                    row.generation == generation).ToList();
                int total = cell.Sum(row => row.rawWeight);
                int roll = random.Next(total);
                WeightedPlacementEventRow chosen = cell[0];
                foreach (WeightedPlacementEventRow row in cell)
                {
                    if (roll < row.rawWeight) { chosen = row; break; }
                    roll -= row.rawWeight;
                }
                remaining.Remove(chosen);
                yield return (chosen, fallback);
            }
        }

        private static List<WeightedPlacementBand> BuildBands(WeightedPoolPlacementConfig catalog)
        {
            var result = new List<WeightedPlacementBand>(catalog.runEncounterBudget);
            AddPhase(result, WeightedPlacementBand.Early, catalog.earlyBudget);
            AddPhase(result, WeightedPlacementBand.Mid, catalog.midBudget);
            AddPhase(result, WeightedPlacementBand.Late, catalog.lateBudget);
            return result;
        }

        private static void AddPhase(List<WeightedPlacementBand> result,
            WeightedPlacementBand phase, int budget)
        {
            for (int i = 0; i < Math.Max(0, budget - 1); i++) result.Add(phase);
            if (budget > 0) result.Add(WeightedPlacementBand.All);
        }

        private static void Shuffle<T>(IList<T> values, Random random)
        {
            for (int i = values.Count - 1; i > 0; i--)
            { int j = random.Next(i + 1); (values[i], values[j]) = (values[j], values[i]); }
        }

        private static void ShufflePhase<T>(IList<T> values, int start, int count, Random random)
        {
            for (int i = start + count - 1; i > start; i--)
            {
                int j = random.Next(start, i + 1);
                (values[i], values[j]) = (values[j], values[i]);
            }
        }

        private static string ValidateCatalog(WeightedPoolPlacementConfig value)
        {
            if (value == null) return "WEIGHTED_PLACEMENT_CATALOG_MISSING";
            if (!value.HasCompiledPlacement)
                return "WEIGHTED_PLACEMENT_SCHEMA_INVALID";
            if (value.runEncounterBudget != 12 || value.earlyBudget != 4
                || value.midBudget != 4 || value.lateBudget != 4)
                return "WEIGHTED_PLACEMENT_BUDGET_INVALID";
            if (value.legacyMass != 45 || value.newMass != 55)
                return "WEIGHTED_PLACEMENT_GENERATION_MASS_INVALID";
            return value.rows == null ? "WEIGHTED_PLACEMENT_ROWS_MISSING" : string.Empty;
        }
    }
}
