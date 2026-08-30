using System;
using System.Collections.Generic;
using System.Linq;

namespace Progression
{
    public sealed class FixedOfferService
    {
        private readonly PartyWideFixedOfferGenerator generator;

        public FixedOfferService()
            : this(new PartyWideFixedOfferGenerator())
        {
        }

        public FixedOfferService(PartyWideFixedOfferGenerator generator)
        {
            this.generator = generator ?? throw new ArgumentNullException(nameof(generator));
        }

        public ProgressionOfferAttachResult GetOrCreate(
            RunProgressionLedger ledger,
            string opportunityId,
            int expectedRevision,
            IEnumerable<ProgressionSkillCandidateSnapshot> catalog,
            out ProgressionOpportunitySnapshot opportunity)
            => GetOrCreate(ledger, opportunityId, expectedRevision, catalog, 3, out opportunity);

        public ProgressionOfferAttachResult GetOrCreate(
            RunProgressionLedger ledger,
            string opportunityId,
            int expectedRevision,
            IEnumerable<ProgressionSkillCandidateSnapshot> catalog,
            int targetCount,
            out ProgressionOpportunitySnapshot opportunity)
        {
            if (ledger == null)
            {
                throw new ArgumentNullException(nameof(ledger));
            }

            if (!ledger.TryGetOpportunity(opportunityId, out ProgressionOpportunitySnapshot current))
            {
                opportunity = null;
                return ProgressionOfferAttachResult.RejectedNotFound;
            }

            if (current.Offer != null)
            {
                opportunity = current;
                return ProgressionOfferAttachResult.AlreadyAttached;
            }

            ProgressionOfferSeedDescriptor descriptor = new(
                current.RunId,
                current.SegmentId,
                current.OpportunityId,
                current.PoolMode,
                targetCount);
            ProgressionOfferSnapshot offer = generator.Generate(descriptor, catalog);
            return ledger.TryAttachFixedOffer(
                opportunityId,
                expectedRevision,
                offer,
                out opportunity);
        }

        public IReadOnlyList<ProgressionOfferCandidateAvailability> EvaluateStale(
            ProgressionOfferSnapshot offer,
            IEnumerable<ProgressionSkillCandidateSnapshot> currentCatalog)
        {
            if (offer == null)
            {
                throw new ArgumentNullException(nameof(offer));
            }

            Dictionary<string, ProgressionSkillCandidateSnapshot> current =
                (currentCatalog ?? Enumerable.Empty<ProgressionSkillCandidateSnapshot>())
                .Where(candidate => candidate != null)
                .GroupBy(candidate => candidate.InstanceKey, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

            return offer.Candidates
                .Select(candidate => new ProgressionOfferCandidateAvailability(
                    candidate,
                    current.TryGetValue(candidate.InstanceKey, out ProgressionSkillCandidateSnapshot value)
                    && value.IsEligible
                    && value.CurrentLevel == candidate.CurrentLevel
                    && string.Equals(value.CanonicalSkillId, candidate.CanonicalSkillId, StringComparison.Ordinal)))
                .ToArray();
        }
    }
}
