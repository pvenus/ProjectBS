using System;
using System.Collections.Generic;
using System.Linq;

namespace Progression.Portfolio
{
    public enum PortfolioPurpose
    {
        Growth = 0,
        Recovery = 10,
        Battle = 20,
        Gold = 30,
        Relic = 40,
        Character = 50,
        Route = 60,
        World = 70
    }

    public enum GrowthCandidateKind { Safe = 0, Battle = 10, Smithy = 20 }
    public enum CandidateBindingKind { Growth = 0, Fallback = 10, Suppressed = 20 }
    public enum PortfolioManifestStatus { Ready = 0, SuppressedInvalidInput = 10, SuppressedMissingFallback = 20 }

    public sealed class PortfolioEventDescriptor
    {
        public PortfolioEventDescriptor(string eventId, PortfolioPurpose purpose, string motif,
            string exclusionGroup = "", string chainKey = "", bool isFollowup = false,
            bool capabilityEnabled = true, string requiredCharacterId = "")
        {
            EventId = eventId ?? string.Empty;
            Purpose = purpose;
            Motif = motif ?? string.Empty;
            ExclusionGroup = exclusionGroup ?? string.Empty;
            ChainKey = chainKey ?? string.Empty;
            IsFollowup = isFollowup;
            CapabilityEnabled = capabilityEnabled;
            RequiredCharacterId = requiredCharacterId ?? string.Empty;
        }

        public string EventId { get; }
        public PortfolioPurpose Purpose { get; }
        public string Motif { get; }
        public string ExclusionGroup { get; }
        public string ChainKey { get; }
        public bool IsFollowup { get; }
        public bool CapabilityEnabled { get; }
        public string RequiredCharacterId { get; }
        public string RootKey => IsFollowup && !string.IsNullOrWhiteSpace(ChainKey) ? ChainKey : EventId;
        public bool IsValid => !string.IsNullOrWhiteSpace(EventId) && (!IsFollowup || !string.IsNullOrWhiteSpace(ChainKey));
    }

    public sealed class GrowthCandidateReservation
    {
        internal GrowthCandidateReservation(GrowthCandidateKind kind, string eventId, string reservationId,
            string leftSectionId, string rightSectionId, string leftSlotId, string rightSlotId,
            int appearanceRoll, bool appeared, string fallbackEventId, int targetCount)
        {
            Kind = kind; EventId = eventId; ReservationId = reservationId;
            LeftSectionId = leftSectionId; RightSectionId = rightSectionId;
            LeftSlotId = leftSlotId; RightSlotId = rightSlotId;
            AppearanceRoll = appearanceRoll; Appeared = appeared;
            FallbackEventId = fallbackEventId; TargetCount = targetCount;
        }
        public GrowthCandidateKind Kind { get; }
        public string EventId { get; }
        public string ReservationId { get; }
        public string LeftSectionId { get; }
        public string RightSectionId { get; }
        public string LeftSlotId { get; }
        public string RightSlotId { get; }
        public int AppearanceRoll { get; }
        public bool Appeared { get; }
        public string FallbackEventId { get; }
        public int TargetCount { get; }
    }

    public sealed class CandidateProjectionState
    {
        internal CandidateProjectionState(GrowthCandidateReservation reservation, CandidateBindingKind binding,
            string displayedEventId, bool encountered)
        { Reservation = reservation; Binding = binding; DisplayedEventId = displayedEventId; Encountered = encountered; }
        public GrowthCandidateReservation Reservation { get; }
        public CandidateBindingKind Binding { get; }
        public string DisplayedEventId { get; }
        public bool Encountered { get; }
    }

    public sealed class Chapter1PortfolioManifest
    {
        internal Chapter1PortfolioManifest(PortfolioManifestStatus status, string runId, string stageGenerationId,
            IReadOnlyList<GrowthCandidateReservation> candidates, IReadOnlyList<PortfolioEventDescriptor> encounters,
            string fingerprint)
        {
            Status = status; RunId = runId ?? string.Empty; StageGenerationId = stageGenerationId ?? string.Empty;
            Candidates = Array.AsReadOnly((candidates ?? Array.Empty<GrowthCandidateReservation>()).ToArray());
            Encounters = Array.AsReadOnly((encounters ?? Array.Empty<PortfolioEventDescriptor>()).ToArray());
            Fingerprint = fingerprint ?? string.Empty;
        }
        public PortfolioManifestStatus Status { get; }
        public string RunId { get; }
        public string StageGenerationId { get; }
        public IReadOnlyList<GrowthCandidateReservation> Candidates { get; }
        public IReadOnlyList<PortfolioEventDescriptor> Encounters { get; }
        public string Fingerprint { get; }
    }

    public sealed class PortfolioProjectionState
    {
        private readonly IReadOnlyList<CandidateProjectionState> states;
        public PortfolioProjectionState(Chapter1PortfolioManifest manifest)
            : this(manifest, manifest?.Candidates.Select(c => new CandidateProjectionState(c,
                c.Appeared ? CandidateBindingKind.Growth : CandidateBindingKind.Fallback,
                c.Appeared ? c.EventId : c.FallbackEventId, false)).ToArray()) { }
        private PortfolioProjectionState(Chapter1PortfolioManifest manifest, IReadOnlyList<CandidateProjectionState> states)
        { Manifest = manifest ?? throw new ArgumentNullException(nameof(manifest)); this.states = Array.AsReadOnly(states.ToArray()); }
        public Chapter1PortfolioManifest Manifest { get; }
        public IReadOnlyList<CandidateProjectionState> Candidates => states;

        public PortfolioProjectionState RecordEncounter(string reservationId)
        {
            return new PortfolioProjectionState(Manifest, states.Select(s =>
                string.Equals(s.Reservation.ReservationId, reservationId, StringComparison.Ordinal)
                    ? new CandidateProjectionState(s.Reservation, s.Binding, s.DisplayedEventId, true) : s).ToArray());
        }

        public PortfolioProjectionState ResolveAfterOptionalGrant()
        {
            return new PortfolioProjectionState(Manifest, states.Select(s => s.Encountered
                ? s
                : new CandidateProjectionState(s.Reservation, CandidateBindingKind.Fallback,
                    s.Reservation.FallbackEventId, false)).ToArray());
        }
    }
}
