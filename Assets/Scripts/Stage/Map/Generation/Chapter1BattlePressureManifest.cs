using System;
using System.Collections.Generic;

namespace Stage
{
    public enum Chapter1EncounterKind { Event = 0, DirectBattle = 10, Shop = 20, Rest = 30 }

    public sealed class Chapter1CompositionAssignment
    {
        public Chapter1CompositionAssignment(int ordinal, WeightedPlacementBand phase,
            Chapter1EncounterKind kind, RoundNodeSO node, WeightedPlacementEventRow eventRow = null)
        {
            Ordinal = ordinal; Phase = phase; Kind = kind; Node = node; EventRow = eventRow;
        }
        public int Ordinal { get; }
        public WeightedPlacementBand Phase { get; }
        public Chapter1EncounterKind Kind { get; }
        public RoundNodeSO Node { get; }
        public WeightedPlacementEventRow EventRow { get; }
    }

    public sealed class Chapter1BattlePressureManifest
    {
        public Chapter1BattlePressureManifest(bool success, string error,
            IReadOnlyList<Chapter1CompositionAssignment> assignments, bool degraded = false)
        {
            Success = success; Error = error ?? string.Empty; Degraded = degraded;
            Assignments = assignments ?? Array.Empty<Chapter1CompositionAssignment>();
        }
        public bool Success { get; }
        public string Error { get; }
        public bool Degraded { get; }
        public IReadOnlyList<Chapter1CompositionAssignment> Assignments { get; }
    }
}
