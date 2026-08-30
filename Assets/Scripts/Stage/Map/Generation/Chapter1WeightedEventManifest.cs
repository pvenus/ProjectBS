using System;
using System.Collections.Generic;

namespace Stage
{
    public sealed class Chapter1WeightedEventAssignment
    {
        public Chapter1WeightedEventAssignment(int ordinal, WeightedPlacementBand band,
            WeightedPlacementEventRow row, bool generationCellFallback)
        {
            Ordinal = ordinal; Band = band; Row = row;
            GenerationCellFallback = generationCellFallback;
        }
        public int Ordinal { get; }
        public WeightedPlacementBand Band { get; }
        public WeightedPlacementEventRow Row { get; }
        public bool GenerationCellFallback { get; }
    }

    public sealed class Chapter1WeightedEventManifest
    {
        public Chapter1WeightedEventManifest(bool success, string error,
            IReadOnlyList<Chapter1WeightedEventAssignment> assignments)
        {
            Success = success; Error = error ?? string.Empty;
            Assignments = assignments ?? Array.Empty<Chapter1WeightedEventAssignment>();
        }
        public bool Success { get; }
        public string Error { get; }
        public IReadOnlyList<Chapter1WeightedEventAssignment> Assignments { get; }
    }
}
