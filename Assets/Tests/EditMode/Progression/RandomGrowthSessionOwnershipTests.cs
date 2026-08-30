using System;
using System.Linq;
using NUnit.Framework;
using Progression.RandomGrowth;

namespace Progression.Tests
{
    public sealed class RandomGrowthSessionOwnershipTests
    {
        [Test]
        public void FirstCommitCreatesStageIdentityOnceAndReentryReusesManifest()
        {
            CountingIdentityFactory factory = new();
            RandomGrowthSessionOwnership ownership = Ownership("run.session.one");

            RandomGrowthSessionCommitResult created = ownership.TryCommitChapter1Graph(
                ownership.RunId, RandomGrowthSessionOwnership.Chapter1Id, 5, 5, factory, out var first);
            RandomGrowthSessionCommitResult reused = ownership.TryCommitChapter1Graph(
                ownership.RunId, RandomGrowthSessionOwnership.Chapter1Id, 5, 5, factory, out var second);

            Assert.That(created, Is.EqualTo(RandomGrowthSessionCommitResult.Created));
            Assert.That(reused, Is.EqualTo(RandomGrowthSessionCommitResult.Reused));
            Assert.That(factory.StageIdCalls, Is.EqualTo(1));
            Assert.That(second, Is.SameAs(first));
            Assert.That(second.RawRoll, Is.EqualTo(first.RawRoll));
        }

        [Test]
        public void ExplicitNewRunClearsManifestAndCreatesOneNewStageIdentity()
        {
            CountingIdentityFactory factory = new();
            RandomGrowthSessionOwnership ownership = Ownership("run.session.old");
            ownership.TryCommitChapter1Graph(
                ownership.RunId, RandomGrowthSessionOwnership.Chapter1Id, 5, 5, factory, out var oldManifest);
            if (oldManifest.Appeared)
            {
                Assert.That(ownership.TryRecordEncounter(RandomGrowthManifestConstants.LeftSectionId),
                    Is.EqualTo(RandomGrowthEncounterResult.Encountered));
            }

            ProgressionRunId nextRun = new("run.session.new");
            ownership.ResetForNewRun(nextRun);

            Assert.That(ownership.StoredManifest, Is.Null);
            Assert.That(ownership.ReservationState, Is.Null);
            Assert.That(ownership.StageGenerationId, Is.Empty);
            ownership.TryCommitChapter1Graph(
                nextRun, RandomGrowthSessionOwnership.Chapter1Id, 5, 5, factory, out var newManifest);
            Assert.That(factory.StageIdCalls, Is.EqualTo(2));
            Assert.That(newManifest, Is.Not.SameAs(oldManifest));
            Assert.That(newManifest.RunId, Is.EqualTo(nextRun));
        }

        [Test]
        public void ReentryPreservesSharedReservationEncounterState()
        {
            CountingIdentityFactory factory = new();
            RandomGrowthSessionOwnership ownership = OwnershipWithAppearedManifest(
                "run.session.reservation",
                factory);
            int stageIdCallsBeforeReentry = factory.StageIdCalls;

            Assert.That(ownership.TryRecordEncounter(RandomGrowthManifestConstants.LeftSectionId),
                Is.EqualTo(RandomGrowthEncounterResult.Encountered));
            ownership.TryCommitChapter1Graph(
                ownership.RunId, RandomGrowthSessionOwnership.Chapter1Id, 5, 5, factory, out _);

            Assert.That(ownership.ReservationState.Encountered, Is.True);
            Assert.That(ownership.TryRecordEncounter(RandomGrowthManifestConstants.RightSectionId),
                Is.EqualTo(RandomGrowthEncounterResult.AlreadyEncountered));
            Assert.That(factory.StageIdCalls, Is.EqualTo(stageIdCallsBeforeReentry));
        }

        [Test]
        public void MissingStoredManifestAfterInitializationSuppressesWithoutNewIdOrRoll()
        {
            CountingIdentityFactory factory = new();
            RandomGrowthSessionOwnership ownership = Ownership("run.session.missing");
            ownership.TryCommitChapter1Graph(
                ownership.RunId, RandomGrowthSessionOwnership.Chapter1Id, 5, 5, factory, out _);
            ownership.ReplaceStoredManifestForTests(null);

            RandomGrowthSessionCommitResult result = ownership.TryCommitChapter1Graph(
                ownership.RunId, RandomGrowthSessionOwnership.Chapter1Id, 5, 5, factory, out var suppressed);

            Assert.That(result, Is.EqualTo(RandomGrowthSessionCommitResult.Suppressed));
            Assert.That(factory.StageIdCalls, Is.EqualTo(1));
            Assert.That(suppressed.Status, Is.EqualTo(RandomGrowthManifestStatus.SuppressedCorruptManifest));
            Assert.That(suppressed.RawRoll, Is.EqualTo(-1));
        }

        [Test]
        public void CorruptStoredManifestSuppressesWithoutReroll()
        {
            CountingIdentityFactory factory = new();
            RandomGrowthSessionOwnership ownership = Ownership("run.session.corrupt");
            ownership.TryCommitChapter1Graph(
                ownership.RunId, RandomGrowthSessionOwnership.Chapter1Id, 5, 5, factory, out var original);
            ownership.ReplaceStoredManifestForTests(new RandomGrowthManifest(
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
                "corrupt"));

            ownership.TryCommitChapter1Graph(
                ownership.RunId, RandomGrowthSessionOwnership.Chapter1Id, 5, 5, factory, out var suppressed);

            Assert.That(factory.StageIdCalls, Is.EqualTo(1));
            Assert.That(suppressed.Status, Is.EqualTo(RandomGrowthManifestStatus.SuppressedCorruptManifest));
            Assert.That(suppressed.RawRoll, Is.EqualTo(-1));
        }

        [Test]
        public void MismatchedRunSuppressesAndDoesNotCreateStageIdentity()
        {
            CountingIdentityFactory factory = new();
            RandomGrowthSessionOwnership ownership = Ownership("run.session.expected");

            RandomGrowthSessionCommitResult result = ownership.TryCommitChapter1Graph(
                new ProgressionRunId("run.session.other"),
                RandomGrowthSessionOwnership.Chapter1Id,
                5,
                5,
                factory,
                out _);

            Assert.That(result, Is.EqualTo(RandomGrowthSessionCommitResult.Suppressed));
            Assert.That(ownership.SuppressionStatus,
                Is.EqualTo(RandomGrowthManifestStatus.SuppressedInvalidIdentity));
            Assert.That(factory.StageIdCalls, Is.Zero);
        }

        [Test]
        public void OtherChapterDoesNotContaminateChapterOneOwnership()
        {
            CountingIdentityFactory factory = new();
            RandomGrowthSessionOwnership ownership = Ownership("run.session.chapter");

            RandomGrowthSessionCommitResult ignored = ownership.TryCommitChapter1Graph(
                ownership.RunId, "stage.chapter2", 5, 5, factory, out _);

            Assert.That(ignored, Is.EqualTo(RandomGrowthSessionCommitResult.IgnoredOtherChapter));
            Assert.That(factory.StageIdCalls, Is.Zero);
            Assert.That(ownership.StageGenerationId, Is.Empty);
            Assert.That(ownership.StoredManifest, Is.Null);
        }

        [Test]
        public void PublicSessionOwnershipExposesNoSaveOrMutableSnapshotApi()
        {
            Assert.That(typeof(RandomGrowthSessionOwnership).GetMethods()
                .Any(method => method.Name.Contains("Save", StringComparison.OrdinalIgnoreCase)), Is.False);
            Assert.That(typeof(RandomGrowthSessionOwnership).GetProperties()
                .All(property => property.SetMethod == null || !property.SetMethod.IsPublic), Is.True);
        }

        private static RandomGrowthSessionOwnership Ownership(string runId)
        {
            RandomGrowthSessionOwnership ownership = new();
            ownership.ResetForNewRun(new ProgressionRunId(runId));
            return ownership;
        }

        private static RandomGrowthSessionOwnership OwnershipWithAppearedManifest(
            string runId,
            CountingIdentityFactory factory)
        {
            for (int index = 0; index < 10000; index++)
            {
                RandomGrowthSessionOwnership ownership = Ownership(runId + "." + index);
                ownership.TryCommitChapter1Graph(
                    ownership.RunId, RandomGrowthSessionOwnership.Chapter1Id, 5, 5, factory, out var manifest);
                if (manifest.Appeared)
                {
                    return ownership;
                }
            }

            Assert.Fail("Expected at least one appeared manifest in the canonical roll range.");
            return null;
        }

        private sealed class CountingIdentityFactory : IRandomGrowthSessionIdentityFactory
        {
            public int RunIdCalls { get; private set; }
            public int StageIdCalls { get; private set; }

            public ProgressionRunId CreateRunId()
            {
                RunIdCalls++;
                return new ProgressionRunId("run.factory." + RunIdCalls);
            }

            public string CreateStageGenerationId(ProgressionRunId runId, string chapterId)
            {
                StageIdCalls++;
                return "stage-generation." + StageIdCalls;
            }
        }
    }
}
