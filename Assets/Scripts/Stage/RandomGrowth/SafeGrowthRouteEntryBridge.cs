using System;
using System.Collections.Generic;
using System.Linq;
using Progression;
using Progression.Portfolio;
using Progression.RandomGrowth;
using Session;
using Skill;
using Party;

namespace Stage
{
    public enum SafeGrowthRouteEntryStatus
    {
        Entered = 0, Existing = 10, Fallback = 20, DisabledRecheck = 30,
        Ignored = 40, Suppressed = 50, PresentationContentUnavailableAfterDisclosure = 60
    }

    public sealed class SafeGrowthRouteEncounterReceipt
    {
        public SafeGrowthRouteEncounterReceipt(string logicalKey, string sectionId,
            string slotId, string nodeInstanceId, string displayedEventId,
            string eligibilityFingerprint)
        {
            LogicalKey = logicalKey ?? string.Empty; SectionId = sectionId ?? string.Empty;
            SlotId = slotId ?? string.Empty; NodeInstanceId = nodeInstanceId ?? string.Empty;
            DisplayedEventId = displayedEventId ?? string.Empty;
            EligibilityFingerprint = eligibilityFingerprint ?? string.Empty;
        }
        public string LogicalKey { get; }
        public string SectionId { get; }
        public string SlotId { get; }
        public string NodeInstanceId { get; }
        public string DisplayedEventId { get; }
        public string EligibilityFingerprint { get; }
        public bool IsValid => !string.IsNullOrWhiteSpace(LogicalKey)
            && !string.IsNullOrWhiteSpace(SectionId) && !string.IsNullOrWhiteSpace(SlotId)
            && !string.IsNullOrWhiteSpace(NodeInstanceId) && !string.IsNullOrWhiteSpace(DisplayedEventId);
        public bool SameIdentity(SafeGrowthRouteEncounterReceipt other) => other != null
            && string.Equals(LogicalKey, other.LogicalKey, StringComparison.Ordinal)
            && string.Equals(SectionId, other.SectionId, StringComparison.Ordinal)
            && string.Equals(SlotId, other.SlotId, StringComparison.Ordinal)
            && string.Equals(NodeInstanceId, other.NodeInstanceId, StringComparison.Ordinal)
            && string.Equals(DisplayedEventId, other.DisplayedEventId, StringComparison.Ordinal);
    }

    public sealed class SafeGrowthRouteEntryResult
    {
        internal SafeGrowthRouteEntryResult(SafeGrowthRouteEntryStatus status,
            SafeGrowthRouteEncounterReceipt receipt, SafeGrowthEligibilitySnapshot eligibility)
        { Status = status; Receipt = receipt; Eligibility = eligibility; }
        public SafeGrowthRouteEntryStatus Status { get; }
        public SafeGrowthRouteEncounterReceipt Receipt { get; }
        public SafeGrowthEligibilitySnapshot Eligibility { get; }
    }

    public sealed class SafeGrowthRouteEntryBridge
    {
        private const string SafeEventId = "event.act1.random_growth.02.windworn_sword_marks";
        private readonly PartyWideSafeGrowthEligibilityQuery eligibility = new();

        public SafeGrowthRouteEntryStatus ResolveV2PresentationBeforePopup(
            StageSession session, RandomGrowthPresentationCopyAsset catalog)
        {
            SafeGrowthStoredAssignment assignment = session?.SafeGrowthPlacement?.Assignment;
            bool disclosed = assignment?.Disclosed == true || assignment?.Encountered == true;
            bool valid = TryResolveV2Catalog(catalog);
            if (valid) return SafeGrowthRouteEntryStatus.Entered;
            if (disclosed)
                return SafeGrowthRouteEntryStatus.PresentationContentUnavailableAfterDisclosure;
            if (assignment == null || assignment.IsFallback) return SafeGrowthRouteEntryStatus.Fallback;
            SafeGrowthPlacementRequest request = session.SafeGrowthPlacementRequest;
            GrowthCandidateReservation candidate = request?.Manifest?.Candidates == null ? null
                : System.Linq.Enumerable.SingleOrDefault(request.Manifest.Candidates,
                    x => x.Kind == GrowthCandidateKind.Safe);
            RoundNodeSO fallback = candidate == null ? null : request.ResolveNode(candidate.FallbackEventId);
            return candidate != null && fallback != null
                && session.SafeGrowthPlacement.TryReplaceWithFallback(candidate.FallbackEventId, fallback)
                ? SafeGrowthRouteEntryStatus.Fallback : SafeGrowthRouteEntryStatus.Suppressed;
        }

        private static bool TryResolveV2Catalog(RandomGrowthPresentationCopyAsset catalog)
        {
            if (catalog == null || catalog.Fields.Count != 31) return false;
            var expected = new RandomGrowthPresentationCopyExpectation(2,
                "chapter1-random-growth-safe-content.v2", "ko-KR",
                "presentation.catalog.act1.random_growth.02.windworn_sword_marks.ko-KR",
                "safe-growth-presentation-copy.v2", "chapter1.random-growth-safe.semantic-copy.v2",
                "chapter1.random-growth-safe.definition.v2", SafeEventId,
                "node.act1.random_growth.02.windworn_sword_marks.intro",
                SafeGrowthPresentationCopyResolver.V2SemanticDigest,
                SafeGrowthPresentationCopyResolver.V2DefinitionFingerprint,
                catalog.Fields.Select(x => x.Name));
            return SafeGrowthPresentationCopyResolver.TryResolveV2(catalog, expected,
                out _, out RandomGrowthPresentationCopyMismatch _);
        }

        public SafeGrowthRouteEntryResult TryEnter(StageSession session, string sectionId,
            string slotId, string nodeInstanceId, string nodeId, string displayedEventId,
            PartyRuntimeData party, IEnumerable<EquipmentSkillSO> catalog)
        {
            SafeGrowthStoredAssignment assignment = session?.SafeGrowthPlacement?.Assignment;
            if (assignment == null || session.SafeGrowthRuntime?.IsReady != true)
                return Result(SafeGrowthRouteEntryStatus.Suppressed);
            if (!string.Equals(assignment.RunId, session.RandomGrowthSession?.RunId.Value, StringComparison.Ordinal)
                || !string.Equals(assignment.StageGenerationId,
                    session.RandomGrowthSession?.StageGenerationId, StringComparison.Ordinal))
                return Result(SafeGrowthRouteEntryStatus.Suppressed);
            bool left = string.Equals(sectionId, assignment.LeftSectionId, StringComparison.Ordinal)
                && string.Equals(slotId, assignment.LeftSlotId, StringComparison.Ordinal);
            bool right = string.Equals(sectionId, assignment.RightSectionId, StringComparison.Ordinal)
                && string.Equals(slotId, assignment.RightSlotId, StringComparison.Ordinal);
            if ((!left && !right) || string.IsNullOrWhiteSpace(nodeInstanceId)
                || !string.Equals(nodeId, assignment.Node?.nodeId, StringComparison.Ordinal)
                || !string.Equals(displayedEventId, assignment.DisplayedEventId, StringComparison.Ordinal))
                return Result(SafeGrowthRouteEntryStatus.Ignored);

            if (session.SafeGrowthRouteEncounter != null)
                return session.SafeGrowthRouteEncounter.SameIdentity(new SafeGrowthRouteEncounterReceipt(
                        assignment.LogicalEncounterKey, sectionId, slotId, nodeInstanceId,
                        displayedEventId, session.SafeGrowthRouteEncounter.EligibilityFingerprint))
                    ? new SafeGrowthRouteEntryResult(SafeGrowthRouteEntryStatus.Existing,
                        session.SafeGrowthRouteEncounter, null)
                    : Result(SafeGrowthRouteEntryStatus.Ignored);

            if (assignment.IsFallback || !string.Equals(displayedEventId, SafeEventId, StringComparison.Ordinal))
                return Result(SafeGrowthRouteEntryStatus.Fallback);

            SafeGrowthEligibilitySnapshot snapshot = eligibility.Query(party, catalog);
            ProgressionChapterSummary summary = session.SafeGrowthRuntime.ProgressionLedger.GetChapterSummary();
            bool optionalAlreadyGranted = summary.RandomEarned >= 1 || summary.TotalApplied >= 3;
            bool disabled = optionalAlreadyGranted || snapshot.Status != SafeGrowthEligibilityStatus.Eligible;
            if (disabled && !assignment.Disclosed && !assignment.Encountered)
            {
                SafeGrowthPlacementRequest request = session.SafeGrowthPlacementRequest;
                GrowthCandidateReservation candidate = request?.Manifest?.Candidates == null ? null
                    : System.Linq.Enumerable.SingleOrDefault(request.Manifest.Candidates,
                        x => x.Kind == GrowthCandidateKind.Safe);
                RoundNodeSO fallback = candidate == null ? null : request.ResolveNode(candidate.FallbackEventId);
                if (candidate != null && fallback != null
                    && session.SafeGrowthPlacement.TryReplaceWithFallback(candidate.FallbackEventId, fallback))
                    return new SafeGrowthRouteEntryResult(SafeGrowthRouteEntryStatus.Fallback, null, snapshot);
                return Result(SafeGrowthRouteEntryStatus.Suppressed, snapshot);
            }
            if (disabled) return Result(SafeGrowthRouteEntryStatus.DisabledRecheck, snapshot);

            session.SafeGrowthPlacement.TryMarkDisclosed();
            SafeGrowthEncounterResult encounter = session.SafeGrowthPlacement.TryRecordEncounter(sectionId);
            if (encounter != SafeGrowthEncounterResult.Encountered
                && encounter != SafeGrowthEncounterResult.AlreadyEncountered)
                return Result(SafeGrowthRouteEntryStatus.Ignored, snapshot);
            SafeGrowthRouteEncounterReceipt receipt = new(assignment.LogicalEncounterKey,
                sectionId, slotId, nodeInstanceId, displayedEventId, snapshot.Fingerprint);
            if (!session.TryStoreSafeGrowthRouteEncounter(receipt))
                return Result(SafeGrowthRouteEntryStatus.Ignored, snapshot);
            return new SafeGrowthRouteEntryResult(SafeGrowthRouteEntryStatus.Entered, receipt, snapshot);
        }

        private static SafeGrowthRouteEntryResult Result(SafeGrowthRouteEntryStatus status,
            SafeGrowthEligibilitySnapshot eligibility = null) => new(status, null, eligibility);
    }
}
