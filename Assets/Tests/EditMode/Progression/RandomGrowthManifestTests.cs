using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Progression.RandomGrowth;

namespace Progression.Tests
{
    public sealed class RandomGrowthManifestTests
    {
        [Test]
        public void SameIdentityProducesGoldenManifest()
        {
            RandomGrowthManifest first = Build("run.golden", "stage.golden", 5, 3);
            RandomGrowthManifest second = Build("run.golden", "stage.golden", 5, 3);

            Assert.That(second.RawRoll, Is.EqualTo(first.RawRoll));
            Assert.That(second.Ordinal, Is.EqualTo(first.Ordinal));
            Assert.That(second.Fingerprint, Is.EqualTo(first.Fingerprint));
            Assert.That(second.Projections.Select(Key), Is.EqualTo(first.Projections.Select(Key)));
            Assert.That(first.RawRoll, Is.EqualTo(3956));
            Assert.That(first.Ordinal, Is.EqualTo(1));
            Assert.That(first.Fingerprint,
                Is.EqualTo("f7114753820a1780c9d324e4bce88b6b7626cb9aab64e3b5b976913f4386cd48"));
        }

        [Test]
        public void RawRollDomainContainsExactlyFourThousandAppearances()
        {
            int appearances = Enumerable.Range(0, RandomGrowthManifestConstants.RollRange)
                .Count(RandomGrowthManifestBuilder.IsAppearanceRoll);

            Assert.That(appearances, Is.EqualTo(4000));
            Assert.That(RandomGrowthManifestBuilder.IsAppearanceRoll(3999), Is.True);
            Assert.That(RandomGrowthManifestBuilder.IsAppearanceRoll(4000), Is.False);
        }

        [Test]
        public void FixedTenAndHundredThousandIdentityCorporaAreWithinTolerance()
        {
            RandomGrowthManifestBuilder builder = new();
            int appeared = 0;
            int appearedFirstTenThousand = 0;
            for (int index = 0; index < 100000; index++)
            {
                RandomGrowthManifest manifest = builder.Build(Request(
                    "run.corpus." + index.ToString("D6"),
                    "stage.corpus." + index.ToString("D6"),
                    4,
                    4));
                appeared += manifest.Appeared ? 1 : 0;
                if (index < 10000)
                {
                    appearedFirstTenThousand += manifest.Appeared ? 1 : 0;
                }
            }

            Assert.That(appearedFirstTenThousand, Is.InRange(3950, 4050));
            Assert.That(appeared, Is.InRange(39500, 40500));
        }

        [Test]
        public void AppearanceAndManifestDoNotDependOnChosenRoute()
        {
            RandomGrowthManifest manifest = FindAppeared();

            Assert.That(manifest.TryGetProjection(RandomGrowthManifestConstants.LeftSectionId, out var left), Is.True);
            Assert.That(manifest.TryGetProjection(RandomGrowthManifestConstants.RightSectionId, out var right), Is.True);
            Assert.That(left.Ordinal, Is.EqualTo(right.Ordinal));
            Assert.That(left.LogicalEncounterKey, Is.EqualTo(right.LogicalEncounterKey));
            Assert.That(manifest.Appeared, Is.True);
        }

        [Test]
        public void RunAndStageGenerationIdentitiesChangeManifestIdentity()
        {
            RandomGrowthManifest baseline = Build("run.identity.a", "stage.identity.a", 4, 4);
            RandomGrowthManifest otherRun = Build("run.identity.b", "stage.identity.a", 4, 4);
            RandomGrowthManifest otherStage = Build("run.identity.a", "stage.identity.b", 4, 4);

            Assert.That(otherRun.Fingerprint, Is.Not.EqualTo(baseline.Fingerprint));
            Assert.That(otherStage.Fingerprint, Is.Not.EqualTo(baseline.Fingerprint));
            Assert.That(otherRun.LogicalEncounterKey, Is.Not.EqualTo(baseline.LogicalEncounterKey));
            Assert.That(otherStage.LogicalEncounterKey, Is.Not.EqualTo(baseline.LogicalEncounterKey));
        }

        [Test]
        public void AppearedManifestHasTwoValidMirroredProjections()
        {
            RandomGrowthManifest manifest = FindAppeared(leftSlots: 7, rightSlots: 3);

            Assert.That(manifest.Projections, Has.Count.EqualTo(2));
            Assert.That(manifest.Ordinal, Is.InRange(0, 2));
            Assert.That(manifest.Projections.Select(value => value.Ordinal), Is.All.EqualTo(manifest.Ordinal));
            Assert.That(manifest.Projections.Select(value => value.SectionId).Distinct().Count(), Is.EqualTo(2));
            Assert.That(manifest.Projections.Select(value => value.LogicalEncounterKey).Distinct().Count(), Is.EqualTo(1));
        }

        [Test]
        public void NonAppearanceCreatesNoReservationProjection()
        {
            RandomGrowthManifest manifest = Find(appeared: false);

            Assert.That(manifest.Status, Is.EqualTo(RandomGrowthManifestStatus.Ready));
            Assert.That(manifest.Projections, Is.Empty);
            Assert.That(manifest.Ordinal, Is.EqualTo(-1));
            Assert.That(new RandomGrowthReservationState(manifest)
                .TryEncounter(RandomGrowthManifestConstants.LeftSectionId, out _),
                Is.EqualTo(RandomGrowthEncounterResult.Suppressed));
        }

        [Test]
        public void SharedLogicalEncounterCanBeConsumedOnlyOnceAcrossRoutes()
        {
            RandomGrowthManifest manifest = FindAppeared();
            RandomGrowthReservationState initial = new(manifest);

            Assert.That(initial.TryEncounter(RandomGrowthManifestConstants.LeftSectionId, out var encountered),
                Is.EqualTo(RandomGrowthEncounterResult.Encountered));
            Assert.That(encountered.TryEncounter(RandomGrowthManifestConstants.RightSectionId, out var duplicate),
                Is.EqualTo(RandomGrowthEncounterResult.AlreadyEncountered));
            Assert.That(duplicate, Is.SameAs(encountered));
            Assert.That(encountered.EncounteredSectionId, Is.EqualTo(RandomGrowthManifestConstants.LeftSectionId));
        }

        [Test]
        public void StoredManifestReentryReusesSameInstanceWithoutReroll()
        {
            RandomGrowthManifestRequest request = Request("run.reentry", "stage.reentry", 4, 4);
            RandomGrowthManifest original = new RandomGrowthManifestBuilder().Build(request);

            RandomGrowthManifest reused = new RandomGrowthManifestBuilder().UseStoredOrSuppress(original, request);

            Assert.That(reused, Is.SameAs(original));
            Assert.That(reused.Fingerprint, Is.EqualTo(original.Fingerprint));
        }

        [Test]
        public void MissingOrCorruptStoredManifestSuppressesWithoutReroll()
        {
            RandomGrowthManifestRequest request = Request("run.corrupt", "stage.corrupt", 4, 4);
            RandomGrowthManifest original = new RandomGrowthManifestBuilder().Build(request);
            RandomGrowthManifest corrupt = new(
                original.Status,
                original.RunId,
                original.StageGenerationId,
                original.GeneratorVersion,
                original.ReservationId,
                original.RawRoll,
                original.Appeared,
                original.Ordinal,
                original.LogicalEncounterKey,
                original.Projections,
                "corrupt-fingerprint");

            RandomGrowthManifest missing = new RandomGrowthManifestBuilder().UseStoredOrSuppress(null, request);
            RandomGrowthManifest rejected = new RandomGrowthManifestBuilder().UseStoredOrSuppress(corrupt, request);

            Assert.That(missing.Status, Is.EqualTo(RandomGrowthManifestStatus.SuppressedCorruptManifest));
            Assert.That(rejected.Status, Is.EqualTo(RandomGrowthManifestStatus.SuppressedCorruptManifest));
            Assert.That(missing.RawRoll, Is.EqualTo(-1));
            Assert.That(rejected.RawRoll, Is.EqualTo(-1));
            Assert.That(missing.Projections, Is.Empty);
            Assert.That(rejected.Projections, Is.Empty);
        }

        [Test]
        public void OffPathInputDoesNotConsumeReservation()
        {
            RandomGrowthReservationState initial = new(FindAppeared());

            Assert.That(initial.TryEncounter("sec.off-path", out var unchanged),
                Is.EqualTo(RandomGrowthEncounterResult.OffRoute));
            Assert.That(unchanged, Is.SameAs(initial));
            Assert.That(initial.Encountered, Is.False);
        }

        [TestCase("version")]
        [TestCase("reservation")]
        [TestCase("stage")]
        [TestCase("cardinality")]
        public void InvalidIdentityVersionOrCardinalitySuppressesEventFailOpen(string mismatch)
        {
            RandomGrowthManifestRequest request = new(
                new ProgressionRunId("run.invalid"),
                mismatch == "stage" ? string.Empty : "stage.invalid",
                mismatch == "version" ? "chapter1.random_growth_manifest.v0" : RandomGrowthManifestConstants.GeneratorVersion,
                mismatch == "reservation" ? "reservation.invalid" : RandomGrowthManifestConstants.ReservationId,
                mismatch == "cardinality" ? 0 : 3,
                3);

            RandomGrowthManifest manifest = new RandomGrowthManifestBuilder().Build(request);

            Assert.That(manifest.Status, Is.Not.EqualTo(RandomGrowthManifestStatus.Ready));
            Assert.That(manifest.Appeared, Is.False);
            Assert.That(manifest.Projections, Is.Empty);
        }

        [Test]
        public void PublicManifestValuesExposeNoMutableSetters()
        {
            foreach (Type type in new[]
                     {
                         typeof(RandomGrowthManifestRequest),
                         typeof(RandomGrowthManifest),
                         typeof(RandomGrowthReservationProjection),
                         typeof(RandomGrowthReservationState)
                     })
            {
                Assert.That(type.GetProperties().All(property => !property.CanWrite), Is.True, type.Name);
            }
        }

        private static RandomGrowthManifest FindAppeared(int leftSlots = 4, int rightSlots = 4) =>
            Find(true, leftSlots, rightSlots);

        private static RandomGrowthManifest Find(
            bool appeared,
            int leftSlots = 4,
            int rightSlots = 4)
        {
            for (int index = 0; index < 1000; index++)
            {
                RandomGrowthManifest manifest = Build("run.find." + index, "stage.find", leftSlots, rightSlots);
                if (manifest.Appeared == appeared)
                {
                    return manifest;
                }
            }

            throw new AssertionException("No deterministic sample matched the requested appearance state.");
        }

        private static RandomGrowthManifest Build(
            string runId,
            string stageGenerationId,
            int leftSlots,
            int rightSlots) =>
            new RandomGrowthManifestBuilder().Build(Request(runId, stageGenerationId, leftSlots, rightSlots));

        private static RandomGrowthManifestRequest Request(
            string runId,
            string stageGenerationId,
            int leftSlots,
            int rightSlots) =>
            new(
                new ProgressionRunId(runId),
                stageGenerationId,
                RandomGrowthManifestConstants.GeneratorVersion,
                RandomGrowthManifestConstants.ReservationId,
                leftSlots,
                rightSlots);

        private static string Key(RandomGrowthReservationProjection projection) =>
            projection.SectionId + ":" + projection.Ordinal + ":" + projection.LogicalEncounterKey;
    }
}
