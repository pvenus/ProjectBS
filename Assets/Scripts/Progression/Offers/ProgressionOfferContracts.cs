using System;
using System.Collections.Generic;
using System.Linq;

namespace Progression
{
    public static class ProgressionOfferConstants
    {
        public const string SchemaVersion = "progression.fixed_offer.v1";
        public const string NoValidCandidateReason = "NO_VALID_CANDIDATE";
    }

    public sealed class ProgressionSkillCandidateSnapshot
    {
        public ProgressionSkillCandidateSnapshot(
            string ownerCharacterId,
            string skillInstanceId,
            string canonicalSkillId,
            int currentLevel,
            int maxLevel,
            bool isActive = true,
            bool hasOwnerReference = true,
            bool hasSkillReference = true,
            bool canApplyNextLevel = true)
        {
            OwnerCharacterId = ownerCharacterId;
            SkillInstanceId = skillInstanceId;
            CanonicalSkillId = canonicalSkillId;
            CurrentLevel = currentLevel;
            MaxLevel = maxLevel;
            IsActive = isActive;
            HasOwnerReference = hasOwnerReference;
            HasSkillReference = hasSkillReference;
            CanApplyNextLevel = canApplyNextLevel;
        }

        public string OwnerCharacterId { get; }
        public string SkillInstanceId { get; }
        public string CanonicalSkillId { get; }
        public int CurrentLevel { get; }
        public int MaxLevel { get; }
        public bool IsActive { get; }
        public bool HasOwnerReference { get; }
        public bool HasSkillReference { get; }
        public bool CanApplyNextLevel { get; }

        public bool IsEligible =>
            !string.IsNullOrWhiteSpace(OwnerCharacterId)
            && !string.IsNullOrWhiteSpace(SkillInstanceId)
            && !string.IsNullOrWhiteSpace(CanonicalSkillId)
            && CurrentLevel > 0
            && CurrentLevel < MaxLevel
            && IsActive
            && HasOwnerReference
            && HasSkillReference
            && CanApplyNextLevel;

        internal string InstanceKey => OwnerCharacterId + "\u001f" + SkillInstanceId;
    }

    public sealed class ProgressionOfferSeedDescriptor
    {
        public ProgressionOfferSeedDescriptor(
            ProgressionRunId runId,
            string segmentId,
            string opportunityId,
            ProgressionPoolMode poolMode = ProgressionPoolMode.PartyWide,
            int targetCount = 3)
        {
            RunId = runId;
            SegmentId = segmentId;
            OpportunityId = opportunityId;
            PoolMode = poolMode;
            TargetCount = targetCount;
        }

        public ProgressionRunId RunId { get; }
        public string SegmentId { get; }
        public string OpportunityId { get; }
        public ProgressionPoolMode PoolMode { get; }
        public int TargetCount { get; }

        internal bool IsValid => RunId.IsValid
            && !string.IsNullOrWhiteSpace(SegmentId)
            && !string.IsNullOrWhiteSpace(OpportunityId)
            && PoolMode == ProgressionPoolMode.PartyWide
            && TargetCount >= 1
            && TargetCount <= 3;
    }

    public sealed class ProgressionOfferSnapshot
    {
        internal ProgressionOfferSnapshot(
            string opportunityId,
            IReadOnlyList<ProgressionSkillCandidateSnapshot> candidates,
            string fingerprint,
            bool duplicateSkillIdRelaxed,
            int targetCount = 3)
        {
            OpportunityId = opportunityId;
            Candidates = Array.AsReadOnly(candidates.ToArray());
            Fingerprint = fingerprint;
            DuplicateSkillIdRelaxed = duplicateSkillIdRelaxed;
            TargetCount = targetCount;
        }

        public string OpportunityId { get; }
        public IReadOnlyList<ProgressionSkillCandidateSnapshot> Candidates { get; }
        public string Fingerprint { get; }
        public bool DuplicateSkillIdRelaxed { get; }
        public int TargetCount { get; }
    }

    public sealed class ProgressionOfferCandidateAvailability
    {
        internal ProgressionOfferCandidateAvailability(
            ProgressionSkillCandidateSnapshot candidate,
            bool isSelectable)
        {
            Candidate = candidate;
            IsSelectable = isSelectable;
        }

        public ProgressionSkillCandidateSnapshot Candidate { get; }
        public bool IsSelectable { get; }
    }
}
