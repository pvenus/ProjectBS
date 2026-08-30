using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Progression.Portfolio;

namespace Progression.Tests
{
    public sealed class PortfolioB01Tests
    {
        [Test]
        public void SameInputIsDeterministicAcrossInsertionOrder()
        {
            List<PortfolioEventDescriptor> registry = Registry48();
            Chapter1PortfolioManifest first = Build("run.same", registry);
            registry.Reverse();
            Chapter1PortfolioManifest second = Build("run.same", registry);
            Assert.That(first.Status, Is.EqualTo(PortfolioManifestStatus.Ready));
            Assert.That(second.Fingerprint, Is.EqualTo(first.Fingerprint));
            Assert.That(second.Candidates.Select(x => x.FallbackEventId),
                Is.EqualTo(first.Candidates.Select(x => x.FallbackEventId)));
            List<PortfolioEventDescriptor> changedSelectorRegistry = Registry48();
            changedSelectorRegistry.Add(new PortfolioEventDescriptor(
                "event.fixture.extra", PortfolioPurpose.Relic, "extra"));
            Chapter1PortfolioManifest changedSelector = Build("run.same", changedSelectorRegistry);
            Assert.That(changedSelector.Candidates[2].AppearanceRoll,
                Is.EqualTo(first.Candidates[2].AppearanceRoll));
            Assert.That(Chapter1PortfolioManifestBuilder.LateAppearanceDomain,
                Is.EqualTo("chapter1.portfolio48.late-appearance.v1"));
        }

        [Test]
        public void CandidateContractIsTwoOrThreeAndLateIsFortyPercent()
        {
            Chapter1PortfolioManifest manifest = Build("run.contract", Registry48());
            Assert.That(manifest.Candidates.Count, Is.EqualTo(3));
            Assert.That(manifest.Candidates.Select(x => x.TargetCount), Is.EqualTo(new[] { 2, 3, 3 }));
            Assert.That(manifest.Candidates.Take(2).All(x => x.Appeared), Is.True);

            int exactRawAppearance = Enumerable.Range(0, 10000)
                .Count(Chapter1PortfolioManifestBuilder.IsLateAppearanceRoll);
            Assert.That(exactRawAppearance, Is.EqualTo(4000));

            int appeared = Enumerable.Range(0, 100000).Count(index =>
                Chapter1PortfolioManifestBuilder.IsLateAppearanceRoll(
                    Chapter1PortfolioManifestBuilder.ComputeLateAppearanceRoll(
                        "run.corpus." + index.ToString(CultureInfo.InvariantCulture), "stage.chapter1")));
            TestContext.Out.WriteLine("late100k=" + appeared);
            Assert.That(appeared, Is.InRange(39500, 40500));
        }

        [Test]
        public void GrantReplacesOnlyUnencounteredCandidatesWithFixedFallback()
        {
            Chapter1PortfolioManifest manifest = Build("run.grant", Registry48());
            PortfolioProjectionState initial = new PortfolioProjectionState(manifest);
            string safe = manifest.Candidates[0].ReservationId;
            PortfolioProjectionState encountered = initial.RecordEncounter(safe);
            PortfolioProjectionState resolved = encountered.ResolveAfterOptionalGrant();

            Assert.That(resolved.Candidates[0].Encountered, Is.True);
            Assert.That(resolved.Candidates[0].DisplayedEventId, Is.EqualTo(initial.Candidates[0].DisplayedEventId));
            Assert.That(resolved.Candidates.Skip(1).All(x => x.Binding == CandidateBindingKind.Fallback), Is.True);
            Assert.That(resolved.Candidates.Skip(1).Select(x => x.DisplayedEventId),
                Is.EqualTo(manifest.Candidates.Skip(1).Select(x => x.FallbackEventId)));
        }

        [Test]
        public void FallbackIsStableAcrossReentryAndUnique()
        {
            Chapter1PortfolioManifest manifest = Build("run.reentry", Registry48());
            PortfolioProjectionState first = new PortfolioProjectionState(manifest).ResolveAfterOptionalGrant();
            PortfolioProjectionState second = new PortfolioProjectionState(manifest).ResolveAfterOptionalGrant();
            Assert.That(first.Candidates.Select(x => x.DisplayedEventId),
                Is.EqualTo(second.Candidates.Select(x => x.DisplayedEventId)));
            Assert.That(first.Candidates.Select(x => x.DisplayedEventId).Distinct().Count(), Is.EqualTo(3));
        }

        [Test]
        public void MissingFallbackSuppressesAndMainRouteCanFailOpen()
        {
            List<PortfolioEventDescriptor> registry = Registry48()
                .Where(x => x.Purpose == PortfolioPurpose.Growth).ToList();
            Chapter1PortfolioManifest result = Build("run.no.fallback", registry);
            Assert.That(result.Status, Is.Not.EqualTo(PortfolioManifestStatus.Ready));
            Assert.That(result.Candidates, Is.Empty);
            Assert.That(result.Encounters, Is.Empty);
        }

        [Test]
        public void SelectorProducesUniqueTwelveWithoutAdjacentPurposeOrMotifCooldownViolation()
        {
            Chapter1PortfolioManifest manifest = Build("run.constraints", Registry48());
            Assert.That(manifest.Encounters.Count, Is.EqualTo(12));
            Assert.That(manifest.Encounters.Select(x => x.RootKey).Distinct().Count(), Is.EqualTo(12));
            Assert.That(manifest.Encounters.Count(x => x.Purpose == PortfolioPurpose.Growth), Is.GreaterThanOrEqualTo(2));
            Assert.That(manifest.Encounters.Count(x => x.Purpose == PortfolioPurpose.Battle), Is.GreaterThanOrEqualTo(2));
            foreach (PortfolioPurpose required in new[] { PortfolioPurpose.Recovery, PortfolioPurpose.Gold,
                         PortfolioPurpose.Route, PortfolioPurpose.World })
                Assert.That(manifest.Encounters.Any(x => x.Purpose == required), Is.True);
            for (int index = 0; index < manifest.Encounters.Count; index++)
            {
                if (index > 0) Assert.That(manifest.Encounters[index].Purpose,
                    Is.Not.EqualTo(manifest.Encounters[index - 1].Purpose));
                for (int prior = Math.Max(0, index - 3); prior < index; prior++)
                    Assert.That(manifest.Encounters[index].Motif,
                        Is.Not.EqualTo(manifest.Encounters[prior].Motif));
            }
        }

        [Test]
        public void ExclusionAndFollowupAreNotIndependentEncounters()
        {
            List<PortfolioEventDescriptor> registry = Registry48();
            registry.Add(new PortfolioEventDescriptor("event.act1.random_event.11.dry_waterwheel",
                PortfolioPurpose.World, "water", chainKey: "event.act1.random_event.18.water_scam", isFollowup: true));
            registry.Add(new PortfolioEventDescriptor(Chapter1PortfolioIds.OriginalSmithyEvent,
                PortfolioPurpose.Relic, "smithy", Chapter1PortfolioIds.SmithyExclusion));
            registry.Add(new PortfolioEventDescriptor(Chapter1PortfolioIds.SmithyEvent,
                PortfolioPurpose.Growth, "fire", Chapter1PortfolioIds.SmithyExclusion));
            Chapter1PortfolioManifest manifest = Build("run.chain", registry);
            Assert.That(manifest.Encounters.Any(x => x.IsFollowup), Is.False);
            Assert.That(manifest.Encounters.Count(x => x.ExclusionGroup == Chapter1PortfolioIds.SmithyExclusion),
                Is.LessThanOrEqualTo(1));
        }

        [Test]
        public void TargetCountTwoChangesFingerprintAndPreservesThreeCardDefault()
        {
            List<ProgressionSkillCandidateSnapshot> catalog = new()
            {
                Candidate("a", "s1"), Candidate("b", "s2"), Candidate("c", "s3")
            };
            ProgressionRunId run = new ProgressionRunId("run.offer");
            PartyWideFixedOfferGenerator generator = new();
            ProgressionOfferSnapshot two = generator.Generate(
                new ProgressionOfferSeedDescriptor(run, "segment", "opportunity", targetCount: 2), catalog);
            ProgressionOfferSnapshot three = generator.Generate(
                new ProgressionOfferSeedDescriptor(run, "segment", "opportunity"), catalog);
            Assert.That(two.TargetCount, Is.EqualTo(2));
            Assert.That(two.Candidates.Count, Is.EqualTo(2));
            Assert.That(three.TargetCount, Is.EqualTo(3));
            Assert.That(three.Candidates.Count, Is.EqualTo(3));
            Assert.That(two.Fingerprint, Is.Not.EqualTo(three.Fingerprint));
        }

        [Test]
        public void TwoCardPolicyReturnsTwoOneAndZeroWithoutRefill()
        {
            PartyWideFixedOfferGenerator generator = new();
            for (int count = 0; count <= 2; count++)
            {
                List<ProgressionSkillCandidateSnapshot> catalog = Enumerable.Range(0, count)
                    .Select(x => Candidate("owner" + x, "skill" + x)).ToList();
                ProgressionOfferSnapshot offer = generator.Generate(new ProgressionOfferSeedDescriptor(
                    new ProgressionRunId("run.count." + count), "segment", "opportunity", targetCount: 2), catalog);
                Assert.That(offer.Candidates.Count, Is.EqualTo(count));
                Assert.That(offer.TargetCount, Is.EqualTo(2));
            }
        }

        private static Chapter1PortfolioManifest Build(string runId, IEnumerable<PortfolioEventDescriptor> registry) =>
            new Chapter1PortfolioManifestBuilder().Build(runId, "stage.chapter1", registry);

        private static List<PortfolioEventDescriptor> Registry48()
        {
            List<PortfolioEventDescriptor> values = new();
            PortfolioPurpose[] purposes = (PortfolioPurpose[])Enum.GetValues(typeof(PortfolioPurpose));
            for (int index = 0; index < 48; index++)
                values.Add(new PortfolioEventDescriptor("event.fixture." + index.ToString("D2", CultureInfo.InvariantCulture),
                    purposes[index % purposes.Length], "motif." + index.ToString("D2", CultureInfo.InvariantCulture)));
            return values;
        }

        private static ProgressionSkillCandidateSnapshot Candidate(string owner, string skill) =>
            new(owner, skill, skill, 1, 5);
    }
}
