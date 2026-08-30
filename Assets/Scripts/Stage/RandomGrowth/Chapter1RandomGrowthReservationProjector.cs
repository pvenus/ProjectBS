using System;
using System.Collections.Generic;
using System.Linq;
using Progression;
using Progression.Portfolio;
using Progression.RandomGrowth;
using Session;

namespace Stage
{
    public enum RandomGrowthProjectionStatus
    {
        Projected = 0,
        NotAppeared = 10,
        Suppressed = 20,
        IgnoredOtherChapter = 30
    }

    public sealed class RandomGrowthReservationDescriptor
    {
        public RandomGrowthReservationDescriptor(
            string reservationId,
            string logicalEncounterKey,
            string sectionId,
            string slotId,
            int ordinal,
            RoundNodeSO node = null,
            string displayedEventId = "",
            bool isFallback = false)
        {
            ReservationId = reservationId ?? string.Empty;
            LogicalEncounterKey = logicalEncounterKey ?? string.Empty;
            SectionId = sectionId ?? string.Empty;
            SlotId = slotId ?? string.Empty;
            Ordinal = ordinal;
            Node = node;
            DisplayedEventId = displayedEventId ?? string.Empty;
            IsFallback = isFallback;
        }

        public string ReservationId { get; }
        public string LogicalEncounterKey { get; }
        public string SectionId { get; }
        public string SlotId { get; }
        public int Ordinal { get; }
        public RoundNodeSO Node { get; }
        public string DisplayedEventId { get; }
        public bool IsFallback { get; }
    }

    public sealed class RandomGrowthProjectionResult
    {
        public RandomGrowthProjectionResult(
            RandomGrowthProjectionStatus status,
            IReadOnlyList<RandomGrowthReservationDescriptor> reservations)
        {
            Status = status;
            Reservations = Array.AsReadOnly(
                (reservations ?? Array.Empty<RandomGrowthReservationDescriptor>()).ToArray());
        }

        public RandomGrowthProjectionStatus Status { get; }
        public IReadOnlyList<RandomGrowthReservationDescriptor> Reservations { get; }
    }

    public sealed class RandomGrowthGraphContext
    {
        public RandomGrowthGraphContext(
            StageSession stageSession,
            ProgressionRunId runId,
            IRandomGrowthSessionIdentityFactory identityFactory,
            SafeGrowthPlacementRequest safePlacement = null)
        {
            StageSession = stageSession ?? throw new ArgumentNullException(nameof(stageSession));
            RunId = runId;
            IdentityFactory = identityFactory
                ?? throw new ArgumentNullException(nameof(identityFactory));
            SafePlacement = safePlacement;
        }

        public StageSession StageSession { get; }
        public ProgressionRunId RunId { get; }
        public IRandomGrowthSessionIdentityFactory IdentityFactory { get; }
        public SafeGrowthPlacementRequest SafePlacement { get; }

        public static RandomGrowthGraphContext CreateDefault(
            StageSession stageSession,
            ProgressionRunId runId) =>
            new(stageSession, runId, new GuidRandomGrowthSessionIdentityFactory(),
                stageSession?.SafeGrowthPlacementRequest);

        public RandomGrowthProjectionResult Project(StageDefinitionSO definition) =>
            new Chapter1RandomGrowthReservationProjector().Project(
                definition,
                StageSession,
                RunId,
                IdentityFactory);

        public static RandomGrowthGraphContext TryCreateCurrent()
        {
            GameSession gameSession = GameSession.Instance;
            if (gameSession?.StageSession == null
                || gameSession.ProgressionSession == null
                || !gameSession.ProgressionSession.HasActiveRun)
            {
                return null;
            }

            return CreateDefault(
                gameSession.StageSession,
                gameSession.ProgressionSession.RunId);
        }
    }

    public enum SafeGrowthProjectionStatus
    {
        Projected = 0,
        FallbackProjected = 10,
        Suppressed = 20
    }

    public enum SafeGrowthEncounterResult
    {
        Encountered = 0,
        AlreadyEncountered = 10,
        InvalidSection = 20,
        MissingAssignment = 30
    }

    public sealed class SafeGrowthStoredAssignment
    {
        public SafeGrowthStoredAssignment(
            string runId,
            string stageGenerationId,
            string manifestFingerprint,
            string reservationId,
            string leftSectionId,
            string rightSectionId,
            string leftSlotId,
            string rightSlotId,
            string displayedEventId,
            RoundNodeSO node,
            bool isFallback,
            bool disclosed = false,
            bool encountered = false,
            string encounteredSectionId = "")
        {
            RunId = runId ?? string.Empty;
            StageGenerationId = stageGenerationId ?? string.Empty;
            ManifestFingerprint = manifestFingerprint ?? string.Empty;
            ReservationId = reservationId ?? string.Empty;
            LeftSectionId = leftSectionId ?? string.Empty;
            RightSectionId = rightSectionId ?? string.Empty;
            LeftSlotId = leftSlotId ?? string.Empty;
            RightSlotId = rightSlotId ?? string.Empty;
            DisplayedEventId = displayedEventId ?? string.Empty;
            Node = node;
            IsFallback = isFallback;
            Disclosed = disclosed;
            Encountered = encountered;
            EncounteredSectionId = encounteredSectionId ?? string.Empty;
        }

        public string RunId { get; }
        public string StageGenerationId { get; }
        public string ManifestFingerprint { get; }
        public string ReservationId { get; }
        public string LogicalEncounterKey => RunId + "/" + StageGenerationId + "/" + ReservationId;
        public string LeftSectionId { get; }
        public string RightSectionId { get; }
        public string LeftSlotId { get; }
        public string RightSlotId { get; }
        public string DisplayedEventId { get; }
        public RoundNodeSO Node { get; }
        public bool IsFallback { get; }
        public bool Disclosed { get; }
        public bool Encountered { get; }
        public string EncounteredSectionId { get; }

        public bool Matches(Chapter1PortfolioManifest manifest, GrowthCandidateReservation candidate) =>
            manifest != null && candidate != null
            && string.Equals(RunId, manifest.RunId, StringComparison.Ordinal)
            && string.Equals(StageGenerationId, manifest.StageGenerationId, StringComparison.Ordinal)
            && string.Equals(ManifestFingerprint, manifest.Fingerprint, StringComparison.Ordinal)
            && string.Equals(ReservationId, candidate.ReservationId, StringComparison.Ordinal)
            && string.Equals(LeftSlotId, candidate.LeftSlotId, StringComparison.Ordinal)
            && string.Equals(RightSlotId, candidate.RightSlotId, StringComparison.Ordinal);

        internal SafeGrowthStoredAssignment WithPresentation(
            string displayedEventId, RoundNodeSO node, bool isFallback) =>
            new(RunId, StageGenerationId, ManifestFingerprint, ReservationId,
                LeftSectionId, RightSectionId, LeftSlotId, RightSlotId,
                displayedEventId, node, isFallback, Disclosed, Encountered, EncounteredSectionId);

        internal SafeGrowthStoredAssignment WithDisclosure() =>
            new(RunId, StageGenerationId, ManifestFingerprint, ReservationId,
                LeftSectionId, RightSectionId, LeftSlotId, RightSlotId,
                DisplayedEventId, Node, IsFallback, true, Encountered, EncounteredSectionId);

        internal SafeGrowthStoredAssignment WithEncounter(string sectionId) =>
            new(RunId, StageGenerationId, ManifestFingerprint, ReservationId,
                LeftSectionId, RightSectionId, LeftSlotId, RightSlotId,
                DisplayedEventId, Node, IsFallback, true, true, sectionId);
    }

    public sealed class SafeGrowthPlacementOwnership
    {
        public SafeGrowthStoredAssignment Assignment { get; private set; }

        public void Clear() => Assignment = null;

        public bool TryStore(SafeGrowthStoredAssignment value)
        {
            if (value == null || value.Node == null) return false;
            if (Assignment == null) Assignment = value;
            return ReferenceEquals(Assignment, value)
                || (string.Equals(Assignment.LogicalEncounterKey, value.LogicalEncounterKey,
                        StringComparison.Ordinal)
                    && string.Equals(Assignment.ManifestFingerprint, value.ManifestFingerprint,
                        StringComparison.Ordinal));
        }

        public bool TryReplaceWithFallback(string eventId, RoundNodeSO node)
        {
            if (Assignment == null || node == null || Assignment.Disclosed || Assignment.Encountered)
                return false;
            Assignment = Assignment.WithPresentation(eventId, node, true);
            return true;
        }

        public bool TryMarkDisclosed()
        {
            if (Assignment == null) return false;
            Assignment = Assignment.WithDisclosure();
            return true;
        }

        public SafeGrowthEncounterResult TryRecordEncounter(string sectionId)
        {
            if (Assignment == null) return SafeGrowthEncounterResult.MissingAssignment;
            if (Assignment.Encountered) return SafeGrowthEncounterResult.AlreadyEncountered;
            if (!string.Equals(sectionId, Assignment.LeftSectionId, StringComparison.Ordinal)
                && !string.Equals(sectionId, Assignment.RightSectionId, StringComparison.Ordinal))
                return SafeGrowthEncounterResult.InvalidSection;
            Assignment = Assignment.WithEncounter(sectionId);
            return SafeGrowthEncounterResult.Encountered;
        }
    }

    public sealed class SafeGrowthPlacementRequest
    {
        public SafeGrowthPlacementRequest(
            Chapter1PortfolioManifest manifest,
            Func<string, RoundNodeSO> resolveNode,
            bool optionalGranted = false,
            bool optionalApplied = false,
            bool capabilityEnabled = true,
            bool eligibleAtFirstReveal = true)
        {
            Manifest = manifest;
            ResolveNode = resolveNode ?? throw new ArgumentNullException(nameof(resolveNode));
            OptionalGranted = optionalGranted;
            OptionalApplied = optionalApplied;
            CapabilityEnabled = capabilityEnabled;
            EligibleAtFirstReveal = eligibleAtFirstReveal;
        }

        public Chapter1PortfolioManifest Manifest { get; }
        public Func<string, RoundNodeSO> ResolveNode { get; }
        public bool OptionalGranted { get; }
        public bool OptionalApplied { get; }
        public bool CapabilityEnabled { get; }
        public bool EligibleAtFirstReveal { get; }
    }

    public sealed class SafeGrowthProjectionResult
    {
        public SafeGrowthProjectionResult(
            SafeGrowthProjectionStatus status,
            IReadOnlyList<RandomGrowthReservationDescriptor> reservations)
        {
            Status = status;
            Reservations = Array.AsReadOnly(
                (reservations ?? Array.Empty<RandomGrowthReservationDescriptor>()).ToArray());
        }

        public SafeGrowthProjectionStatus Status { get; }
        public IReadOnlyList<RandomGrowthReservationDescriptor> Reservations { get; }
    }

    public sealed class Chapter1RandomGrowthReservationProjector
    {
        public const int RequiredSectionCardinality = 5;

        public RandomGrowthProjectionResult Project(
            StageDefinitionSO definition,
            StageSession stageSession,
            ProgressionRunId runId,
            IRandomGrowthSessionIdentityFactory identityFactory)
        {
            if (definition == null
                || stageSession == null
                || !string.Equals(
                    definition.stageId,
                    RandomGrowthSessionOwnership.Chapter1Id,
                    StringComparison.Ordinal))
            {
                return Result(RandomGrowthProjectionStatus.IgnoredOtherChapter);
            }

            if (!TryGetCanonicalSections(definition, out var left, out var right)
                || !SectionsAreValid(definition, left, right))
            {
                stageSession.TryCommitChapter1RandomGrowthGraph(
                    runId,
                    definition.stageId,
                    0,
                    0,
                    identityFactory,
                    out _);
                return Result(RandomGrowthProjectionStatus.Suppressed);
            }

            RandomGrowthSessionCommitResult commit =
                stageSession.TryCommitChapter1RandomGrowthGraph(
                    runId,
                    definition.stageId,
                    left.targetSlotIds.Count,
                    right.targetSlotIds.Count,
                    identityFactory,
                    out RandomGrowthManifest manifest);
            if (commit == RandomGrowthSessionCommitResult.IgnoredOtherChapter)
            {
                return Result(RandomGrowthProjectionStatus.IgnoredOtherChapter);
            }

            if (commit == RandomGrowthSessionCommitResult.Suppressed
                || manifest == null
                || manifest.Status != RandomGrowthManifestStatus.Ready)
            {
                return Result(RandomGrowthProjectionStatus.Suppressed);
            }

            if (!manifest.Appeared)
            {
                return Result(RandomGrowthProjectionStatus.NotAppeared);
            }

            var reservations = new List<RandomGrowthReservationDescriptor>(2);
            foreach (RandomGrowthReservationProjection projection in manifest.Projections)
            {
                StageRandomSection section = string.Equals(
                    projection.SectionId,
                    RandomGrowthManifestConstants.LeftSectionId,
                    StringComparison.Ordinal)
                    ? left
                    : right;
                string slotId = section.targetSlotIds[projection.Ordinal];
                reservations.Add(new RandomGrowthReservationDescriptor(
                    manifest.ReservationId,
                    manifest.LogicalEncounterKey,
                    projection.SectionId,
                    slotId,
                    projection.Ordinal));
            }

            return new RandomGrowthProjectionResult(
                RandomGrowthProjectionStatus.Projected,
                reservations);
        }

        public SafeGrowthProjectionResult ProjectSafe(
            StageDefinitionSO definition,
            StageSession stageSession,
            SafeGrowthPlacementRequest request)
        {
            if (definition == null || stageSession == null || request?.Manifest == null
                || request.Manifest.Status != PortfolioManifestStatus.Ready)
            {
                return SafeResult(SafeGrowthProjectionStatus.Suppressed);
            }

            GrowthCandidateReservation[] matches = request.Manifest.Candidates
                .Where(value => value.Kind == GrowthCandidateKind.Safe).ToArray();
            if (matches.Length != 1 || !TryValidateSafeCandidate(definition, matches[0]))
                return SafeResult(SafeGrowthProjectionStatus.Suppressed);

            GrowthCandidateReservation candidate = matches[0];
            bool requiresFallback = request.OptionalGranted || request.OptionalApplied
                || !request.CapabilityEnabled || !request.EligibleAtFirstReveal || !candidate.Appeared;
            SafeGrowthStoredAssignment stored = stageSession.SafeGrowthPlacement?.Assignment;
            if (stored == null)
            {
                string displayedEventId = requiresFallback
                    ? candidate.FallbackEventId
                    : candidate.EventId;
                RoundNodeSO node = request.ResolveNode(displayedEventId);
                if (node == null || string.IsNullOrWhiteSpace(displayedEventId))
                    return SafeResult(SafeGrowthProjectionStatus.Suppressed);
                stored = new SafeGrowthStoredAssignment(
                    request.Manifest.RunId,
                    request.Manifest.StageGenerationId,
                    request.Manifest.Fingerprint,
                    candidate.ReservationId,
                    candidate.LeftSectionId,
                    candidate.RightSectionId,
                    candidate.LeftSlotId,
                    candidate.RightSlotId,
                    displayedEventId,
                    node,
                    requiresFallback);
                if (!stageSession.SafeGrowthPlacement.TryStore(stored))
                    return SafeResult(SafeGrowthProjectionStatus.Suppressed);
            }
            else if (!stored.Matches(request.Manifest, candidate))
            {
                return SafeResult(SafeGrowthProjectionStatus.Suppressed);
            }
            else if (requiresFallback && !stored.IsFallback)
            {
                RoundNodeSO fallback = request.ResolveNode(candidate.FallbackEventId);
                if (fallback == null
                    || !stageSession.SafeGrowthPlacement.TryReplaceWithFallback(
                        candidate.FallbackEventId, fallback))
                {
                    if (!stored.Disclosed && !stored.Encountered)
                        return SafeResult(SafeGrowthProjectionStatus.Suppressed);
                }
                stored = stageSession.SafeGrowthPlacement.Assignment;
            }

            var reservations = new[]
            {
                new RandomGrowthReservationDescriptor(stored.ReservationId,
                    stored.LogicalEncounterKey, stored.LeftSectionId, stored.LeftSlotId, 0,
                    stored.Node, stored.DisplayedEventId, stored.IsFallback),
                new RandomGrowthReservationDescriptor(stored.ReservationId,
                    stored.LogicalEncounterKey, stored.RightSectionId, stored.RightSlotId, 0,
                    stored.Node, stored.DisplayedEventId, stored.IsFallback)
            };
            return new SafeGrowthProjectionResult(
                stored.IsFallback
                    ? SafeGrowthProjectionStatus.FallbackProjected
                    : SafeGrowthProjectionStatus.Projected,
                reservations);
        }

        private static bool TryValidateSafeCandidate(
            StageDefinitionSO definition,
            GrowthCandidateReservation candidate)
        {
            if (!string.Equals(candidate.EventId, Chapter1PortfolioIds.SafeEvent, StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(candidate.FallbackEventId))
                return false;
            StageRandomSection left = SingleSection(definition, candidate.LeftSectionId);
            StageRandomSection right = SingleSection(definition, candidate.RightSectionId);
            if (left == null || right == null
                || !left.targetSlotIds.Contains(candidate.LeftSlotId)
                || !right.targetSlotIds.Contains(candidate.RightSlotId))
                return false;
            var slotById = (definition.svgMapSlots ?? new List<StageMapSlot>())
                .Where(slot => slot != null && !string.IsNullOrWhiteSpace(slot.slotId))
                .GroupBy(slot => slot.slotId, StringComparer.Ordinal)
                .Where(group => group.Count() == 1)
                .ToDictionary(group => group.Key, group => group.Single(), StringComparer.Ordinal);
            return SafeSlotIsValid(left, candidate.LeftSlotId, slotById)
                && SafeSlotIsValid(right, candidate.RightSlotId, slotById);
        }

        private static bool SafeSlotIsValid(
            StageRandomSection section,
            string slotId,
            IReadOnlyDictionary<string, StageMapSlot> slotById) =>
            slotById.TryGetValue(slotId, out StageMapSlot slot)
            && slot.role == StageMapSlotRole.Random
            && IsReachable(section.fromStorySlotId, slotId, slotById)
            && IsReachable(slotId, section.toStorySlotId, slotById);

        private static SafeGrowthProjectionResult SafeResult(SafeGrowthProjectionStatus status) =>
            new(status, Array.Empty<RandomGrowthReservationDescriptor>());

        private static bool TryGetCanonicalSections(
            StageDefinitionSO definition,
            out StageRandomSection left,
            out StageRandomSection right)
        {
            left = SingleSection(definition, RandomGrowthManifestConstants.LeftSectionId);
            right = SingleSection(definition, RandomGrowthManifestConstants.RightSectionId);
            return left != null && right != null;
        }

        private static StageRandomSection SingleSection(
            StageDefinitionSO definition,
            string sectionId)
        {
            StageRandomSection[] matches = (definition.svgRandomSections
                    ?? new List<StageRandomSection>())
                .Where(section => section != null
                    && string.Equals(section.sectionId, sectionId, StringComparison.Ordinal))
                .ToArray();
            return matches.Length == 1 ? matches[0] : null;
        }

        private static bool SectionsAreValid(
            StageDefinitionSO definition,
            StageRandomSection left,
            StageRandomSection right)
        {
            if (left.targetSlotIds == null
                || right.targetSlotIds == null
                || left.targetSlotIds.Count != RequiredSectionCardinality
                || right.targetSlotIds.Count != RequiredSectionCardinality)
            {
                return false;
            }

            var slotById = (definition.svgMapSlots ?? new List<StageMapSlot>())
                .Where(slot => slot != null && !string.IsNullOrWhiteSpace(slot.slotId))
                .GroupBy(slot => slot.slotId, StringComparer.Ordinal)
                .Where(group => group.Count() == 1)
                .ToDictionary(group => group.Key, group => group.Single(), StringComparer.Ordinal);
            return SectionIsValid(left, slotById)
                && SectionIsValid(right, slotById);
        }

        private static bool SectionIsValid(
            StageRandomSection section,
            IReadOnlyDictionary<string, StageMapSlot> slotById)
        {
            if (!slotById.ContainsKey(section.fromStorySlotId)
                || !slotById.ContainsKey(section.toStorySlotId)
                || section.targetSlotIds.Distinct(StringComparer.Ordinal).Count()
                    != RequiredSectionCardinality)
            {
                return false;
            }

            foreach (string targetSlotId in section.targetSlotIds)
            {
                if (!slotById.TryGetValue(targetSlotId, out StageMapSlot target)
                    || target.role != StageMapSlotRole.Random
                    || !IsReachable(section.fromStorySlotId, targetSlotId, slotById)
                    || !IsReachable(targetSlotId, section.toStorySlotId, slotById))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsReachable(
            string fromSlotId,
            string toSlotId,
            IReadOnlyDictionary<string, StageMapSlot> slotById)
        {
            var pending = new Stack<string>();
            var visited = new HashSet<string>(StringComparer.Ordinal);
            pending.Push(fromSlotId);
            while (pending.Count > 0)
            {
                string current = pending.Pop();
                if (!visited.Add(current))
                {
                    continue;
                }

                if (string.Equals(current, toSlotId, StringComparison.Ordinal))
                {
                    return true;
                }

                if (!slotById.TryGetValue(current, out StageMapSlot slot)
                    || slot.connections == null)
                {
                    continue;
                }

                foreach (StageSlotConnection connection in slot.connections)
                {
                    if (connection != null && !string.IsNullOrWhiteSpace(connection.toSlotId))
                    {
                        pending.Push(connection.toSlotId);
                    }
                }
            }

            return false;
        }

        private static RandomGrowthProjectionResult Result(RandomGrowthProjectionStatus status) =>
            new(status, Array.Empty<RandomGrowthReservationDescriptor>());
    }
}
