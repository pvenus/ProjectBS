using System;

namespace Progression
{
    public enum ProgressionApplyResultCode
    {
        Applied = 0,
        Busy = 10,
        AlreadyApplied = 20,
        RejectedInvalidCommand = 30,
        RejectedNotFound = 40,
        RejectedState = 50,
        RejectedRevision = 60,
        RejectedFingerprint = 70,
        RejectedCandidate = 80,
        RejectedExpectedLevel = 90,
        RejectedCap = 100,
        GatewayRejected = 110,
        GatewayFaulted = 120,
        LedgerCommitFailedRestored = 130,
        CompensationFaulted = 140
    }

    public enum SkillLevelMutationResult
    {
        Applied = 0,
        RejectedNotFound = 10,
        RejectedIdentity = 20,
        RejectedExpectedLevel = 30,
        RejectedMaxLevel = 40,
        Faulted = 50
    }

    public sealed class ProgressionApplyCandidateCommand
    {
        public ProgressionApplyCandidateCommand(
            string opportunityId,
            string fingerprint,
            string ownerCharacterId,
            string skillInstanceId,
            string canonicalSkillId,
            int expectedLevel,
            int expectedLedgerRevision)
        {
            OpportunityId = opportunityId;
            Fingerprint = fingerprint;
            OwnerCharacterId = ownerCharacterId;
            SkillInstanceId = skillInstanceId;
            CanonicalSkillId = canonicalSkillId;
            ExpectedLevel = expectedLevel;
            ExpectedLedgerRevision = expectedLedgerRevision;
        }

        public string OpportunityId { get; }
        public string Fingerprint { get; }
        public string OwnerCharacterId { get; }
        public string SkillInstanceId { get; }
        public string CanonicalSkillId { get; }
        public int ExpectedLevel { get; }
        public int ExpectedLedgerRevision { get; }

        internal bool IsValid =>
            !string.IsNullOrWhiteSpace(OpportunityId)
            && !string.IsNullOrWhiteSpace(Fingerprint)
            && !string.IsNullOrWhiteSpace(OwnerCharacterId)
            && !string.IsNullOrWhiteSpace(SkillInstanceId)
            && !string.IsNullOrWhiteSpace(CanonicalSkillId)
            && ExpectedLevel > 0
            && ExpectedLedgerRevision > 0;
    }

    public readonly struct ProgressionSkillMutationKey : IEquatable<ProgressionSkillMutationKey>
    {
        public ProgressionSkillMutationKey(
            string ownerCharacterId,
            string skillInstanceId,
            string canonicalSkillId)
        {
            OwnerCharacterId = ownerCharacterId;
            SkillInstanceId = skillInstanceId;
            CanonicalSkillId = canonicalSkillId;
        }

        public string OwnerCharacterId { get; }
        public string SkillInstanceId { get; }
        public string CanonicalSkillId { get; }

        public bool Equals(ProgressionSkillMutationKey other) =>
            string.Equals(OwnerCharacterId, other.OwnerCharacterId, StringComparison.Ordinal)
            && string.Equals(SkillInstanceId, other.SkillInstanceId, StringComparison.Ordinal)
            && string.Equals(CanonicalSkillId, other.CanonicalSkillId, StringComparison.Ordinal);

        public override bool Equals(object obj) =>
            obj is ProgressionSkillMutationKey other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = StringComparer.Ordinal.GetHashCode(OwnerCharacterId ?? string.Empty);
                hash = (hash * 397) ^ StringComparer.Ordinal.GetHashCode(SkillInstanceId ?? string.Empty);
                hash = (hash * 397) ^ StringComparer.Ordinal.GetHashCode(CanonicalSkillId ?? string.Empty);
                return hash;
            }
        }
    }

    public sealed class ProgressionSkillLevelMutation
    {
        public ProgressionSkillLevelMutation(
            ProgressionSkillMutationKey key,
            int previousLevel,
            int appliedLevel,
            string mutationId)
        {
            Key = key;
            PreviousLevel = previousLevel;
            AppliedLevel = appliedLevel;
            MutationId = mutationId;
        }

        public ProgressionSkillMutationKey Key { get; }
        public int PreviousLevel { get; }
        public int AppliedLevel { get; }
        public string MutationId { get; }
    }

    public interface IProgressionSkillLevelGateway
    {
        bool TryGetCurrentLevel(ProgressionSkillMutationKey key, out int currentLevel);

        SkillLevelMutationResult TryApplyExactOne(
            ProgressionSkillMutationKey key,
            int expectedLevel,
            out ProgressionSkillLevelMutation mutation);

        bool TryRollback(ProgressionSkillLevelMutation mutation);

        bool TryRestoreExactLevel(
            ProgressionSkillMutationKey key,
            int expectedAppliedLevel,
            int restoreLevel);
    }

    public sealed class ProgressionApplyReceipt
    {
        public ProgressionApplyReceipt(
            string opportunityId,
            string fingerprint,
            string ownerCharacterId,
            string skillInstanceId,
            string canonicalSkillId,
            int previousLevel,
            int appliedLevel,
            string mutationId)
        {
            OpportunityId = opportunityId;
            Fingerprint = fingerprint;
            OwnerCharacterId = ownerCharacterId;
            SkillInstanceId = skillInstanceId;
            CanonicalSkillId = canonicalSkillId;
            PreviousLevel = previousLevel;
            AppliedLevel = appliedLevel;
            MutationId = mutationId;
        }

        public string OpportunityId { get; }
        public string Fingerprint { get; }
        public string OwnerCharacterId { get; }
        public string SkillInstanceId { get; }
        public string CanonicalSkillId { get; }
        public int PreviousLevel { get; }
        public int AppliedLevel { get; }
        public string MutationId { get; }
    }

    public sealed class ProgressionApplyResult
    {
        internal ProgressionApplyResult(
            ProgressionApplyResultCode code,
            ProgressionOpportunitySnapshot opportunity,
            ProgressionApplyReceipt receipt)
        {
            Code = code;
            Opportunity = opportunity;
            Receipt = receipt;
        }

        public ProgressionApplyResultCode Code { get; }
        public ProgressionOpportunitySnapshot Opportunity { get; }
        public ProgressionApplyReceipt Receipt { get; }
    }
}
