#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using NUnit.Framework;
using Progression;
using Progression.Portfolio;
using Progression.RandomGrowth;
using ResourceTools.Stage;
using Session;
using Stage;
using UnityEditor;

namespace ProjectBS.EditorTests.Progression
{
    public sealed class Chapter1SafeGrowthPlacementTests
    {
        private const string DefinitionPath = "Assets/Contents/Stage/so/stage.chapter1.asset";
        private const string SafeNodePath =
            "Assets/Contents/Stage/so/stage.act1.random_growth.02.windworn_sword_marks.asset";
        private const string WeightedPath =
            "Assets/Contents/Stage/placement/rules/WeightedPoolPlacementRule.asset";
        private const string WeightedSha =
            "6215b492aa0ac29344e93e2d5676a30d51ffc2504078e0fc064dfa1b2239a60d";

        [Test]
        public void CanonicalEarlyPairedFirstSlotsAreRandomAndReachable()
        {
            StageDefinitionSO definition = Definition();
            AssertSection(definition, "sec_ep_2_to_ep_3_1", "slot_430_2085");
            AssertSection(definition, "sec_ep_2_to_ep_3_2", "slot_1370_2085");
        }

        [Test]
        public void SafeReservationProjectsMirrorsAndOverridesWeightedAssignment()
        {
            Fixture fixture = CreateFixture();
            StageSvgSlotMapGraphGenerator generator = Generate(fixture, Request(fixture));
            RandomGrowthReservationDescriptor[] safe = SafeReservations(generator).ToArray();

            Assert.That(safe, Has.Length.EqualTo(2));
            Assert.That(safe.Select(x => x.LogicalEncounterKey).Distinct().Count(), Is.EqualTo(1));
            Assert.That(safe.Select(x => x.DisplayedEventId).Distinct().Single(),
                Is.EqualTo(Chapter1PortfolioIds.SafeEvent));
            Assert.That(safe.All(x => !x.IsFallback && x.Node == fixture.SafeNode), Is.True);
            Assert.That(generator.LastAssignments["slot_430_2085"], Is.SameAs(fixture.SafeNode));
            Assert.That(generator.LastAssignments["slot_1370_2085"], Is.SameAs(fixture.SafeNode));
        }

        [TestCase(true, false)]
        [TestCase(false, true)]
        public void OptionalClaimBeforeDisclosureUsesStoredFallback(bool granted, bool applied)
        {
            Fixture fixture = CreateFixture();
            StageSvgSlotMapGraphGenerator first = Generate(fixture, Request(fixture));
            Assert.That(SafeReservations(first).All(x => !x.IsFallback), Is.True);

            StageSvgSlotMapGraphGenerator second = Generate(
                fixture, Request(fixture, granted, applied));
            RandomGrowthReservationDescriptor[] safe = SafeReservations(second).ToArray();
            Assert.That(safe.All(x => x.IsFallback && x.Node == fixture.FallbackNode), Is.True);
            Assert.That(safe.Select(x => x.DisplayedEventId).Distinct().Single(),
                Is.EqualTo(fixture.SafeCandidate.FallbackEventId));
        }

        [Test]
        public void DisclosedSafeAssignmentIsNeverReplacedAfterOptionalGrant()
        {
            Fixture fixture = CreateFixture();
            Generate(fixture, Request(fixture));
            Assert.That(fixture.Session.SafeGrowthPlacement.TryMarkDisclosed(), Is.True);

            StageSvgSlotMapGraphGenerator rebuilt = Generate(fixture, Request(fixture, true));
            Assert.That(SafeReservations(rebuilt).All(x => !x.IsFallback && x.Node == fixture.SafeNode),
                Is.True);
        }

        [Test]
        public void SelectedRouteEncountersOnceAndMirrorCannotDuplicateOrReplace()
        {
            Fixture fixture = CreateFixture();
            Generate(fixture, Request(fixture));
            Assert.That(fixture.Session.SafeGrowthPlacement.TryRecordEncounter(
                    fixture.SafeCandidate.LeftSectionId), Is.EqualTo(SafeGrowthEncounterResult.Encountered));
            Assert.That(fixture.Session.SafeGrowthPlacement.TryRecordEncounter(
                    fixture.SafeCandidate.RightSectionId), Is.EqualTo(SafeGrowthEncounterResult.AlreadyEncountered));

            StageSvgSlotMapGraphGenerator rebuilt = Generate(fixture, Request(fixture, true));
            Assert.That(SafeReservations(rebuilt).All(x => !x.IsFallback && x.Node == fixture.SafeNode),
                Is.True);
            Assert.That(fixture.Session.SafeGrowthPlacement.Assignment.EncounteredSectionId,
                Is.EqualTo(fixture.SafeCandidate.LeftSectionId));
        }

        [Test]
        public void RebuildReusesStoredAssignmentWithoutFallbackReroll()
        {
            Fixture fixture = CreateFixture();
            StageSvgSlotMapGraphGenerator first = Generate(fixture, Request(fixture, capability: false));
            SafeGrowthStoredAssignment stored = fixture.Session.SafeGrowthPlacement.Assignment;
            StageSvgSlotMapGraphGenerator second = Generate(fixture, Request(fixture, capability: false));

            Assert.That(fixture.Session.SafeGrowthPlacement.Assignment, Is.SameAs(stored));
            Assert.That(SafeReservations(second).Select(x => x.DisplayedEventId),
                Is.EqualTo(SafeReservations(first).Select(x => x.DisplayedEventId)));
            Assert.That(SafeReservations(second).Select(x => x.SlotId),
                Is.EqualTo(SafeReservations(first).Select(x => x.SlotId)));
        }

        [Test]
        public void MissingContentSuppressesSafeOnlyAndMainGraphContinues()
        {
            Fixture fixture = CreateFixture();
            var invalid = new SafeGrowthPlacementRequest(fixture.Manifest, _ => null);
            StageSvgSlotMapGraphGenerator generator = Generate(fixture, invalid);

            Assert.That(SafeReservations(generator), Is.Empty);
            Assert.That(generator.LastAssignments, Has.Count.EqualTo(fixture.Definition.svgMapSlots.Count));
            Assert.That(fixture.Session.SafeGrowthPlacement.Assignment, Is.Null);
        }

        [Test]
        public void ProjectionLeavesOrdinaryWeightedRuleUnchangedAndUnlinked()
        {
            Fixture fixture = CreateFixture();
            string before = Sha256(WeightedPath);
            Generate(fixture, Request(fixture));
            OrdinaryPoolLinkSnapshot snapshot =
                RandomGrowthGeneratedAssetBuilder.CaptureOrdinaryPoolLinkSnapshot();

            Assert.That(before, Is.EqualTo(WeightedSha));
            Assert.That(Sha256(WeightedPath), Is.EqualTo(before));
            Assert.That(snapshot.PoolCardinality, Is.EqualTo(2));
            Assert.That(snapshot.ManifestOnlyLinkCardinality, Is.Zero);
        }

        private static Fixture CreateFixture()
        {
            StageDefinitionSO definition = Definition();
            Chapter1PortfolioManifest manifest = new Chapter1PortfolioManifestBuilder().Build(
                "run.b03a", "stage-generation.b03a", Registry());
            Assert.That(manifest.Status, Is.EqualTo(PortfolioManifestStatus.Ready));
            GrowthCandidateReservation safe = manifest.Candidates.Single(x => x.Kind == GrowthCandidateKind.Safe);
            RoundNodeSO safeNode = AssetDatabase.LoadAssetAtPath<RoundNodeSO>(SafeNodePath);
            RoundNodeSO fallback = definition.svgRandomSections
                .SelectMany(section => section.placementRule.weightedPool.pools)
                .SelectMany(entry => entry.pool.entries)
                .Select(entry => entry.node)
                .First(node => node != null && node != safeNode);
            Assert.That(safeNode, Is.Not.Null);
            Assert.That(fallback, Is.Not.Null);
            var session = new StageSession();
            session.ResetRandomGrowthForNewRun(new ProgressionRunId(manifest.RunId));
            return new Fixture(definition, manifest, safe, safeNode, fallback, session);
        }

        private static SafeGrowthPlacementRequest Request(
            Fixture fixture,
            bool granted = false,
            bool applied = false,
            bool capability = true) =>
            new(fixture.Manifest,
                eventId => string.Equals(eventId, Chapter1PortfolioIds.SafeEvent, StringComparison.Ordinal)
                    ? fixture.SafeNode
                    : fixture.FallbackNode,
                granted, applied, capability, true);

        private static StageSvgSlotMapGraphGenerator Generate(
            Fixture fixture,
            SafeGrowthPlacementRequest request)
        {
            fixture.Session.ConfigureSafeGrowthPlacement(request);
            var context = new RandomGrowthGraphContext(
                fixture.Session,
                new ProgressionRunId(fixture.Manifest.RunId),
                new FixedIdentityFactory(fixture.Manifest.StageGenerationId),
                request);
            var generator = new StageSvgSlotMapGraphGenerator();
            Assert.That(generator.Generate(fixture.Definition,
                new StageGraph(fixture.Definition.stageId, fixture.Definition.stageName), context), Is.True);
            return generator;
        }

        private static IEnumerable<RandomGrowthReservationDescriptor> SafeReservations(
            StageSvgSlotMapGraphGenerator generator) =>
            generator.LastRandomGrowthReservations.Where(x =>
                string.Equals(x.ReservationId,
                    "reservation.act1.chapter01.random_growth.after_episode02",
                    StringComparison.Ordinal));

        private static void AssertSection(StageDefinitionSO definition, string sectionId, string slotId)
        {
            StageRandomSection section = definition.svgRandomSections.Single(x => x.sectionId == sectionId);
            Assert.That(section.targetSlotIds[0], Is.EqualTo(slotId));
            StageMapSlot slot = definition.svgMapSlots.Single(x => x.slotId == slotId);
            Assert.That(slot.role, Is.EqualTo(StageMapSlotRole.Random));
            Assert.That(definition.svgMapSlots.Single(x => x.slotId == section.fromStorySlotId)
                .connections.Any(x => x.toSlotId == slotId), Is.True);
        }

        private static List<PortfolioEventDescriptor> Registry()
        {
            var values = new List<PortfolioEventDescriptor>
            {
                new(Chapter1PortfolioIds.SafeEvent, PortfolioPurpose.Growth, "safe"),
                new(Chapter1PortfolioIds.BattleEvent, PortfolioPurpose.Growth, "battle-growth"),
                new(Chapter1PortfolioIds.SmithyEvent, PortfolioPurpose.Growth, "smithy",
                    Chapter1PortfolioIds.SmithyExclusion)
            };
            foreach (PortfolioPurpose purpose in Enum.GetValues(typeof(PortfolioPurpose)))
                for (int i = 0; i < 7; i++)
                    values.Add(new PortfolioEventDescriptor(
                        $"event.fixture.{purpose.ToString().ToLowerInvariant()}.{i}",
                        purpose, $"motif.{purpose}.{i}"));
            return values;
        }

        private static StageDefinitionSO Definition()
        {
            StageDefinitionSO value = AssetDatabase.LoadAssetAtPath<StageDefinitionSO>(DefinitionPath);
            Assert.That(value, Is.Not.Null);
            return value;
        }

        private static string Sha256(string path)
        {
            using SHA256 hash = SHA256.Create();
            return string.Concat(hash.ComputeHash(File.ReadAllBytes(path))
                .Select(value => value.ToString("x2")));
        }

        private sealed class FixedIdentityFactory : IRandomGrowthSessionIdentityFactory
        {
            private readonly string stageGenerationId;
            public FixedIdentityFactory(string stageGenerationId) =>
                this.stageGenerationId = stageGenerationId;
            public ProgressionRunId CreateRunId() => new("unused.run");
            public string CreateStageGenerationId(ProgressionRunId runId, string chapterId) =>
                stageGenerationId;
        }

        private sealed class Fixture
        {
            public Fixture(StageDefinitionSO definition, Chapter1PortfolioManifest manifest,
                GrowthCandidateReservation safeCandidate, RoundNodeSO safeNode,
                RoundNodeSO fallbackNode, StageSession session)
            {
                Definition = definition; Manifest = manifest; SafeCandidate = safeCandidate;
                SafeNode = safeNode; FallbackNode = fallbackNode; Session = session;
            }
            public StageDefinitionSO Definition { get; }
            public Chapter1PortfolioManifest Manifest { get; }
            public GrowthCandidateReservation SafeCandidate { get; }
            public RoundNodeSO SafeNode { get; }
            public RoundNodeSO FallbackNode { get; }
            public StageSession Session { get; }
        }
    }
}
#endif
