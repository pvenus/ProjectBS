using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Progression
{
    public sealed class PartyWideFixedOfferGenerator
    {
        public ProgressionOfferSnapshot Generate(
            ProgressionOfferSeedDescriptor descriptor,
            IEnumerable<ProgressionSkillCandidateSnapshot> catalog)
        {
            if (descriptor == null || !descriptor.IsValid)
            {
                throw new ArgumentException("A valid offer descriptor is required.", nameof(descriptor));
            }

            List<ProgressionSkillCandidateSnapshot> eligible = (catalog
                    ?? Enumerable.Empty<ProgressionSkillCandidateSnapshot>())
                .Where(candidate => candidate != null && candidate.IsEligible)
                .GroupBy(candidate => candidate.InstanceKey, StringComparer.Ordinal)
                .Select(group => group
                    .OrderBy(CanonicalCandidateKey, StringComparer.Ordinal)
                    .First())
                .OrderBy(CanonicalCandidateKey, StringComparer.Ordinal)
                .ToList();

            int targetCount = Math.Min(descriptor.TargetCount, eligible.Count);
            bool relaxed = eligible
                .Select(candidate => candidate.CanonicalSkillId)
                .Distinct(StringComparer.Ordinal)
                .Count() < targetCount;
            bool requireOwnerDiversity = targetCount == 3
                && eligible.Select(candidate => candidate.OwnerCharacterId)
                    .Distinct(StringComparer.Ordinal).Count() >= 2;

            byte[] seed = CanonicalOfferHash.Compute(
                ProgressionOfferConstants.SchemaVersion,
                descriptor.RunId.Value,
                descriptor.SegmentId,
                descriptor.OpportunityId,
                ((int)descriptor.PoolMode).ToString(CultureInfo.InvariantCulture));
            FixedXoshiro256StarStar random = new(seed);
            List<ProgressionSkillCandidateSnapshot> selected = new(targetCount);

            while (selected.Count < targetCount)
            {
                List<ProgressionSkillCandidateSnapshot> feasible = eligible
                    .Where(candidate => !selected.Any(chosen =>
                        string.Equals(chosen.InstanceKey, candidate.InstanceKey, StringComparison.Ordinal)))
                    .Where(candidate => relaxed || !selected.Any(chosen =>
                        string.Equals(chosen.CanonicalSkillId, candidate.CanonicalSkillId, StringComparison.Ordinal)))
                    .Where(candidate => CanComplete(
                        selected,
                        candidate,
                        eligible,
                        targetCount,
                        relaxed,
                        requireOwnerDiversity))
                    .ToList();

                List<string> owners = feasible
                    .Select(candidate => candidate.OwnerCharacterId)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToList();
                string owner = owners[random.NextIndex(owners.Count)];
                List<ProgressionSkillCandidateSnapshot> ownerCandidates = feasible
                    .Where(candidate => string.Equals(
                        candidate.OwnerCharacterId, owner, StringComparison.Ordinal))
                    .OrderBy(CanonicalCandidateKey, StringComparer.Ordinal)
                    .ToList();
                selected.Add(ownerCandidates[random.NextIndex(ownerCandidates.Count)]);
            }

            string fingerprint = BuildFingerprint(descriptor, selected, relaxed);
            return new ProgressionOfferSnapshot(
                descriptor.OpportunityId,
                selected,
                fingerprint,
                relaxed,
                descriptor.TargetCount);
        }

        private static bool CanComplete(
            IReadOnlyList<ProgressionSkillCandidateSnapshot> selected,
            ProgressionSkillCandidateSnapshot next,
            IReadOnlyList<ProgressionSkillCandidateSnapshot> eligible,
            int targetCount,
            bool relaxed,
            bool requireOwnerDiversity)
        {
            List<ProgressionSkillCandidateSnapshot> prefix = selected.Concat(new[] { next }).ToList();
            return HasCompletion(prefix, eligible, targetCount, relaxed, requireOwnerDiversity);
        }

        private static bool HasCompletion(
            List<ProgressionSkillCandidateSnapshot> selected,
            IReadOnlyList<ProgressionSkillCandidateSnapshot> eligible,
            int targetCount,
            bool relaxed,
            bool requireOwnerDiversity)
        {
            if (selected.Count == targetCount)
            {
                return !requireOwnerDiversity
                    || selected.Select(candidate => candidate.OwnerCharacterId)
                        .Distinct(StringComparer.Ordinal).Count() >= 2;
            }

            foreach (ProgressionSkillCandidateSnapshot candidate in eligible)
            {
                if (selected.Any(chosen => string.Equals(
                        chosen.InstanceKey, candidate.InstanceKey, StringComparison.Ordinal))
                    || (!relaxed && selected.Any(chosen => string.Equals(
                        chosen.CanonicalSkillId, candidate.CanonicalSkillId, StringComparison.Ordinal))))
                {
                    continue;
                }

                selected.Add(candidate);
                bool complete = HasCompletion(selected, eligible, targetCount, relaxed, requireOwnerDiversity);
                selected.RemoveAt(selected.Count - 1);
                if (complete)
                {
                    return true;
                }
            }

            return false;
        }

        private static string BuildFingerprint(
            ProgressionOfferSeedDescriptor descriptor,
            IEnumerable<ProgressionSkillCandidateSnapshot> selected,
            bool relaxed)
        {
            List<string> fields = new()
            {
                ProgressionOfferConstants.SchemaVersion,
                descriptor.RunId.Value,
                descriptor.SegmentId,
                descriptor.OpportunityId,
                ((int)descriptor.PoolMode).ToString(CultureInfo.InvariantCulture),
                relaxed ? "duplicate_skill_id_relaxed=true" : "duplicate_skill_id_relaxed=false"
            };
            // Preserve the accepted v1 three-card golden vectors. A non-default
            // policy is part of the offer identity and cannot alias that contract.
            if (descriptor.TargetCount != 3)
            {
                fields.Add("target_count=" + descriptor.TargetCount.ToString(CultureInfo.InvariantCulture));
            }
            foreach (ProgressionSkillCandidateSnapshot candidate in selected)
            {
                fields.Add(candidate.OwnerCharacterId);
                fields.Add(candidate.SkillInstanceId);
                fields.Add(candidate.CanonicalSkillId);
                fields.Add(candidate.CurrentLevel.ToString(CultureInfo.InvariantCulture));
                fields.Add(candidate.MaxLevel.ToString(CultureInfo.InvariantCulture));
            }

            return CanonicalOfferHash.ComputeHex(fields);
        }

        private static string CanonicalCandidateKey(ProgressionSkillCandidateSnapshot candidate) =>
            candidate.OwnerCharacterId + "\u001f"
            + candidate.SkillInstanceId + "\u001f"
            + candidate.CanonicalSkillId + "\u001f"
            + candidate.CurrentLevel.ToString("D10", CultureInfo.InvariantCulture) + "\u001f"
            + candidate.MaxLevel.ToString("D10", CultureInfo.InvariantCulture);
    }
}
