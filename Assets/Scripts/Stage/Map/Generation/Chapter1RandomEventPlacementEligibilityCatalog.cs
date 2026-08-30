using System;
using System.Collections.Generic;
using System.Linq;
using Progression.Portfolio;

namespace Stage
{
    internal static class Chapter1RandomEventPlacementEligibilityCatalog
    {
        internal static bool IsEligible(WeightedPlacementEventRow row,
            IReadOnlyList<WeightedPlacementEventRow> selected,
            IReadOnlyCollection<string> roster,
            Func<string, bool> capabilityGate)
        {
            if (row == null || !row.topLevelEligible || row.rawWeight <= 0 || row.node == null)
                return false;
            if (!string.IsNullOrWhiteSpace(row.capabilityGate)
                && (capabilityGate == null || !capabilityGate(row.capabilityGate))) return false;
            if (!string.IsNullOrWhiteSpace(row.requiredCharacterId)
                && !(roster ?? Array.Empty<string>()).Any(value =>
                    string.Equals(value, row.requiredCharacterId, StringComparison.Ordinal)
                    || value.StartsWith(row.requiredCharacterId + ".", StringComparison.Ordinal))) return false;
            selected ??= Array.Empty<WeightedPlacementEventRow>();
            if (row.oneShot && selected.Any(item => string.Equals(item.eventId, row.eventId,
                    StringComparison.Ordinal))) return false;
            if (Chapter1BattleReuseSelectionContract.Conflicts(
                    selected.Select(ToDescriptor), ToDescriptor(row))) return false;
            if (row.exclusionGroupIds != null && row.exclusionGroupIds.Any(group =>
                    selected.Any(item => item.exclusionGroupIds != null
                        && item.exclusionGroupIds.Contains(group)))) return false;
            if (row.cooldown > 0 && row.motifTags != null && selected
                    .Skip(Math.Max(0, selected.Count - row.cooldown)).Any(item =>
                        item.motifTags != null && item.motifTags.Intersect(row.motifTags).Any())) return false;
            return selected.Count == 0 || selected[selected.Count - 1].primaryPurpose != row.primaryPurpose;
        }

        private static PortfolioEventDescriptor ToDescriptor(WeightedPlacementEventRow row) =>
            new(row?.eventId, row?.primaryPurpose ?? PortfolioPurpose.World,
                row?.motifTags?.FirstOrDefault() ?? string.Empty,
                row?.exclusionGroupIds?.FirstOrDefault() ?? string.Empty,
                requiredCharacterId: row?.requiredCharacterId ?? string.Empty);
    }
}
