using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;

namespace Progression.Tests
{
    public sealed class PartyWideFixedOfferTests
    {
        [Test]
        public void CanonicalHashHasStableGoldenVector()
        {
            Assert.That(
                CanonicalOfferHash.ComputeHex(new[] { "abc", "한글", string.Empty }),
                Is.EqualTo("10fc3a4c38de25afa5e6b8588ca1d9e3864196c2b6e5e3195b5b8ca2b0ad14bb"));
        }

        [Test]
        public void SameInputIgnoresInsertionOrderAndCurrentCulture()
        {
            List<ProgressionSkillCandidateSnapshot> catalog = Catalog(
                ("owner.가", "instance.3", "skill.셋"),
                ("owner.a", "instance.1", "skill.one"),
                ("owner.a", "instance.2", "skill.two"),
                ("owner.b", "instance.4", "skill.four"));
            CultureInfo previousCulture = CultureInfo.CurrentCulture;
            CultureInfo previousUiCulture = CultureInfo.CurrentUICulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
                CultureInfo.CurrentUICulture = new CultureInfo("tr-TR");
                ProgressionOfferSnapshot first = Generate("opportunity.fixed", catalog);

                CultureInfo.CurrentCulture = new CultureInfo("ko-KR");
                CultureInfo.CurrentUICulture = new CultureInfo("ko-KR");
                catalog.Reverse();
                ProgressionOfferSnapshot second = Generate("opportunity.fixed", catalog);

                Assert.That(Keys(second), Is.EqualTo(Keys(first)));
                Assert.That(second.Fingerprint, Is.EqualTo(first.Fingerprint));
            }
            finally
            {
                CultureInfo.CurrentCulture = previousCulture;
                CultureInfo.CurrentUICulture = previousUiCulture;
            }
        }

        [Test]
        public void OwnerNormalizedFirstDrawIsNotDominatedBySkillCount()
        {
            List<ProgressionSkillCandidateSnapshot> catalog = Catalog(
                ("owner.a", "a1", "skill.a1"),
                ("owner.a", "a2", "skill.a2"),
                ("owner.a", "a3", "skill.a3"),
                ("owner.a", "a4", "skill.a4"),
                ("owner.a", "a5", "skill.a5"),
                ("owner.a", "a6", "skill.a6"),
                ("owner.b", "b1", "skill.b1"));
            int ownerA = 0;
            const int samples = 1000;

            for (int index = 0; index < samples; index++)
            {
                ProgressionOfferSnapshot offer = Generate("opportunity." + index, catalog);
                ownerA += offer.Candidates[0].OwnerCharacterId == "owner.a" ? 1 : 0;
            }

            Assert.That(ownerA, Is.InRange(430, 570));
        }

        [Test]
        public void ThreeCardsNeverUseOneOwnerWhenTwoOwnersAreEligible()
        {
            List<ProgressionSkillCandidateSnapshot> catalog = Catalog(
                ("owner.a", "a1", "skill.1"),
                ("owner.a", "a2", "skill.2"),
                ("owner.a", "a3", "skill.3"),
                ("owner.b", "b1", "skill.4"));

            for (int index = 0; index < 200; index++)
            {
                ProgressionOfferSnapshot offer = Generate("opportunity." + index, catalog);
                Assert.That(offer.Candidates.Select(value => value.OwnerCharacterId).Distinct().Count(),
                    Is.GreaterThanOrEqualTo(2));
            }
        }

        [Test]
        public void DistinctSkillIdsAreRequiredWhenEnoughExist()
        {
            ProgressionOfferSnapshot offer = Generate("opportunity.distinct", Catalog(
                ("owner.a", "a1", "skill.shared"),
                ("owner.b", "b1", "skill.shared"),
                ("owner.a", "a2", "skill.two"),
                ("owner.b", "b2", "skill.three")));

            Assert.That(offer.Candidates.Select(value => value.CanonicalSkillId).Distinct().Count(), Is.EqualTo(3));
            Assert.That(offer.DuplicateSkillIdRelaxed, Is.False);
        }

        [Test]
        public void DuplicateSkillIdRelaxesOnlyWhenNeededAndNeverDuplicatesInstance()
        {
            ProgressionOfferSnapshot offer = Generate("opportunity.relaxed", Catalog(
                ("owner.a", "a1", "skill.shared"),
                ("owner.b", "b1", "skill.shared"),
                ("owner.a", "a2", "skill.other")));

            Assert.That(offer.Candidates, Has.Count.EqualTo(3));
            Assert.That(offer.DuplicateSkillIdRelaxed, Is.True);
            Assert.That(offer.Candidates.Select(value => value.InstanceKey).Distinct().Count(), Is.EqualTo(3));
        }

        [TestCase(3)]
        [TestCase(2)]
        [TestCase(1)]
        [TestCase(0)]
        public void CandidateCountIsThreeTwoOneOrZeroWithoutPlaceholders(int count)
        {
            List<ProgressionSkillCandidateSnapshot> catalog = Enumerable.Range(0, count)
                .Select(index => Candidate("owner." + index, "instance." + index, "skill." + index))
                .ToList();

            Assert.That(Generate("opportunity.count." + count, catalog).Candidates.Count, Is.EqualTo(count));
        }

        [Test]
        public void IneligibleAndDuplicateInstancesAreFiltered()
        {
            List<ProgressionSkillCandidateSnapshot> catalog = new()
            {
                Candidate("owner.a", "instance.1", "skill.1"),
                Candidate("owner.a", "instance.1", "skill.1"),
                new ProgressionSkillCandidateSnapshot("owner.a", "instance.2", "skill.2", 10, 10),
                new ProgressionSkillCandidateSnapshot("owner.b", "instance.3", "skill.3", 1, 10, isActive: false)
            };

            ProgressionOfferSnapshot offer = Generate("opportunity.filtered", catalog);
            Assert.That(offer.Candidates, Has.Count.EqualTo(1));
            Assert.That(offer.Candidates[0].SkillInstanceId, Is.EqualTo("instance.1"));
        }

        [Test]
        public void LedgerFixesFirstOfferAndZeroCandidatesBecomePendingBlocked()
        {
            RunProgressionLedger ledger = CreateLedger(out ProgressionOpportunitySnapshot earned);
            FixedOfferService service = new();
            List<ProgressionSkillCandidateSnapshot> firstCatalog = Catalog(
                ("owner.a", "a1", "skill.1"),
                ("owner.b", "b1", "skill.2"),
                ("owner.a", "a2", "skill.3"));

            Assert.That(service.GetOrCreate(
                    ledger, earned.OpportunityId, earned.Revision, firstCatalog,
                    out ProgressionOpportunitySnapshot attached),
                Is.EqualTo(ProgressionOfferAttachResult.Attached));
            Assert.That(service.GetOrCreate(
                    ledger, earned.OpportunityId, attached.Revision,
                    Catalog(("owner.x", "x1", "skill.x")),
                    out ProgressionOpportunitySnapshot reentered),
                Is.EqualTo(ProgressionOfferAttachResult.AlreadyAttached));
            Assert.That(reentered.Offer.Fingerprint, Is.EqualTo(attached.Offer.Fingerprint));

            RunProgressionLedger emptyLedger = CreateLedger(out ProgressionOpportunitySnapshot emptyEarned);
            Assert.That(service.GetOrCreate(
                    emptyLedger, emptyEarned.OpportunityId, emptyEarned.Revision,
                    Array.Empty<ProgressionSkillCandidateSnapshot>(),
                    out ProgressionOpportunitySnapshot blocked),
                Is.EqualTo(ProgressionOfferAttachResult.Attached));
            Assert.That(blocked.State, Is.EqualTo(ProgressionOpportunityState.PendingBlocked));
            Assert.That(blocked.Offer.Candidates, Is.Empty);
        }

        [Test]
        public void OfferAttachmentFaultRestoresOldPendingRecord()
        {
            ProgressionRunId runId = new("run.offer.fault");
            RunProgressionLedger ledger = new(
                runId,
                ProgressionCapPolicy.Chapter1P0,
                new ProgressionSourceRegistry(),
                point =>
                {
                    if (point == ProgressionLedgerMutationPoint.OfferAttached)
                    {
                        throw new InvalidOperationException("Injected offer fault.");
                    }
                });
            ledger.TryEarn(new ProgressionEarnRequest(
                    ProgressionSourceRegistry.FixedRescueSegment,
                    ProgressionSourceCategory.Fixed,
                    ProgressionSourceType.BattleVictory,
                    ProgressionSourceRegistry.RescueBattleSource,
                    "result.offer.fault"),
                out ProgressionOpportunitySnapshot earned);
            ProgressionOfferSnapshot offer = new PartyWideFixedOfferGenerator().Generate(
                new ProgressionOfferSeedDescriptor(
                    runId,
                    earned.SegmentId,
                    earned.OpportunityId),
                Catalog(("owner.a", "a1", "skill.1")));

            Assert.That(ledger.TryAttachFixedOffer(
                    earned.OpportunityId,
                    earned.Revision,
                    offer,
                    out ProgressionOpportunitySnapshot restored),
                Is.EqualTo(ProgressionOfferAttachResult.Faulted));
            Assert.That(restored.State, Is.EqualTo(ProgressionOpportunityState.Pending));
            Assert.That(restored.Offer, Is.Null);
            Assert.That(restored.Revision, Is.EqualTo(earned.Revision));
        }

        [Test]
        public void StaleCardsAreDisabledWithoutReplacement()
        {
            ProgressionOfferSnapshot offer = Generate("opportunity.stale", Catalog(
                ("owner.a", "a1", "skill.1"),
                ("owner.b", "b1", "skill.2"),
                ("owner.c", "c1", "skill.3")));
            FixedOfferService service = new();
            List<ProgressionSkillCandidateSnapshot> current = offer.Candidates
                .Select((candidate, index) => new ProgressionSkillCandidateSnapshot(
                    candidate.OwnerCharacterId,
                    candidate.SkillInstanceId,
                    candidate.CanonicalSkillId,
                    index == 0 ? candidate.CurrentLevel + 1 : candidate.CurrentLevel,
                    candidate.MaxLevel))
                .Concat(new[] { Candidate("owner.new", "new", "skill.new") })
                .ToList();

            IReadOnlyList<ProgressionOfferCandidateAvailability> availability =
                service.EvaluateStale(offer, current);

            Assert.That(availability.Count, Is.EqualTo(3));
            Assert.That(availability.Count(value => value.IsSelectable), Is.EqualTo(2));
            Assert.That(availability.Any(value => value.Candidate.SkillInstanceId == "new"), Is.False);
        }

        [Test]
        public void PublicSnapshotsExposeNoMutableSetters()
        {
            AssertNoWritableProperties(typeof(ProgressionOfferSnapshot));
            AssertNoWritableProperties(typeof(ProgressionSkillCandidateSnapshot));
            AssertNoWritableProperties(typeof(ProgressionOfferCandidateAvailability));
        }

        private static void AssertNoWritableProperties(Type type)
        {
            foreach (System.Reflection.PropertyInfo property in type.GetProperties())
            {
                Assert.That(property.CanWrite, Is.False, type.Name + "." + property.Name);
            }
        }

        private static RunProgressionLedger CreateLedger(out ProgressionOpportunitySnapshot earned)
        {
            RunProgressionLedger ledger = new(
                new ProgressionRunId("run.offer.test"),
                ProgressionCapPolicy.Chapter1P0,
                new ProgressionSourceRegistry());
            Assert.That(ledger.TryEarn(new ProgressionEarnRequest(
                    ProgressionSourceRegistry.FixedRescueSegment,
                    ProgressionSourceCategory.Fixed,
                    ProgressionSourceType.BattleVictory,
                    ProgressionSourceRegistry.RescueBattleSource,
                    "result.offer.test"), out earned),
                Is.EqualTo(ProgressionEarnResult.Earned));
            return ledger;
        }

        private static ProgressionOfferSnapshot Generate(
            string opportunityId,
            IEnumerable<ProgressionSkillCandidateSnapshot> catalog)
        {
            return new PartyWideFixedOfferGenerator().Generate(
                new ProgressionOfferSeedDescriptor(
                    new ProgressionRunId("run.offer.test"),
                    ProgressionSourceRegistry.FixedRescueSegment,
                    opportunityId),
                catalog);
        }

        private static List<ProgressionSkillCandidateSnapshot> Catalog(
            params (string owner, string instance, string skill)[] entries) =>
            entries.Select(value => Candidate(value.owner, value.instance, value.skill)).ToList();

        private static ProgressionSkillCandidateSnapshot Candidate(
            string owner,
            string instance,
            string skill) =>
            new(owner, instance, skill, 1, 10);

        private static string[] Keys(ProgressionOfferSnapshot offer) =>
            offer.Candidates.Select(value => value.InstanceKey).ToArray();
    }
}
