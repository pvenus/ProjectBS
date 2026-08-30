using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using NUnit.Framework;
using Progression;
using Progression.RandomGrowth;
using Session;
using Stage;
using UnityEditor;
using UnityEngine;

namespace ProjectBS.EditorTests.Progression
{
    public sealed class Chapter1RandomGrowthGraphIntegrationTests
    {
        private const string DefinitionPath =
            "Assets/Contents/Stage/so/stage.chapter1.asset";
        private const string Event16Path =
            "Assets/Contents/Stage/json/event/act01/event.act1.16.crying_bell_smithy.json";
        private const string Event16Sha256 =
            "1e6ef22cc7bfd31c79b859f3ad1f07dbc26e3d4778f916e2c829c54af6a771fa";

        [Test]
        public void CanonicalChapterHasPairedFiveSlotReachableSections()
        {
            StageDefinitionSO definition = LoadDefinition();
            StageRandomSection left = Section(definition, RandomGrowthManifestConstants.LeftSectionId);
            StageRandomSection right = Section(definition, RandomGrowthManifestConstants.RightSectionId);

            Assert.That(left.targetSlotIds, Has.Count.EqualTo(5));
            Assert.That(right.targetSlotIds, Has.Count.EqualTo(5));
            Assert.That(left.targetSlotIds.Distinct().Count(), Is.EqualTo(5));
            Assert.That(right.targetSlotIds.Distinct().Count(), Is.EqualTo(5));

            RandomGrowthTestContext context = FindContext(definition, appeared: true);
            Assert.That(context.Result.Status, Is.EqualTo(RandomGrowthProjectionStatus.Projected));
            Assert.That(context.Result.Reservations, Has.Count.EqualTo(2));
        }

        [Test]
        public void AppearedManifestProjectsMirroredSameOrdinalAndLogicalKey()
        {
            StageDefinitionSO definition = LoadDefinition();
            RandomGrowthTestContext context = FindContext(definition, appeared: true);
            RandomGrowthReservationDescriptor left = context.Result.Reservations.Single(value =>
                value.SectionId == RandomGrowthManifestConstants.LeftSectionId);
            RandomGrowthReservationDescriptor right = context.Result.Reservations.Single(value =>
                value.SectionId == RandomGrowthManifestConstants.RightSectionId);

            Assert.That(left.Ordinal, Is.EqualTo(right.Ordinal));
            Assert.That(left.LogicalEncounterKey, Is.EqualTo(right.LogicalEncounterKey));
            Assert.That(left.ReservationId, Is.EqualTo(RandomGrowthManifestConstants.ReservationId));
            Assert.That(left.SlotId, Is.EqualTo(Section(definition, left.SectionId).targetSlotIds[left.Ordinal]));
            Assert.That(right.SlotId, Is.EqualTo(Section(definition, right.SectionId).targetSlotIds[right.Ordinal]));
        }

        [Test]
        public void GeneratorKeepsReservationsSeparateFromWeightedAssignmentsAndGraphReachable()
        {
            StageDefinitionSO definition = LoadDefinition();
            RandomGrowthTestContext context = FindContext(definition, appeared: true);
            var generator = new StageSvgSlotMapGraphGenerator();
            var graph = new StageGraph(definition.stageId, definition.stageName);

            bool generated = generator.Generate(definition, graph, context.GraphContext);

            Assert.That(generated, Is.True);
            Assert.That(generator.LastRandomGrowthReservations, Has.Count.EqualTo(2));
            Assert.That(generator.LastAssignments, Has.Count.EqualTo(definition.svgMapSlots.Count));
            foreach (RandomGrowthReservationDescriptor reservation in generator.LastRandomGrowthReservations)
            {
                Assert.That(generator.LastAssignments[reservation.SlotId], Is.Not.Null);
                string runtimeNodeId = generator.LastRuntimeNodeIdBySlotId[reservation.SlotId];
                Assert.That(graph.nodes.Any(node => node.nodeId == runtimeNodeId), Is.True);
            }
            foreach (StageStorySlotBinding binding in definition.svgStorySlotBindings)
            {
                Assert.That(generator.LastAssignments[binding.slotId], Is.SameAs(binding.node));
            }
            Assert.That(graph.nodes, Has.Count.EqualTo(definition.svgMapSlots.Count));
        }

        [Test]
        public void ChosenRouteEncountersOnceAndMirrorCallbackCannotDuplicate()
        {
            StageDefinitionSO definition = LoadDefinition();
            RandomGrowthTestContext context = FindContext(definition, appeared: true);

            Assert.That(context.StageSession.RandomGrowthSession.ReservationState.Encountered, Is.False);
            Assert.That(context.StageSession.RandomGrowthSession.TryRecordEncounter(
                    RandomGrowthManifestConstants.LeftSectionId),
                Is.EqualTo(RandomGrowthEncounterResult.Encountered));
            Assert.That(context.StageSession.RandomGrowthSession.TryRecordEncounter(
                    RandomGrowthManifestConstants.RightSectionId),
                Is.EqualTo(RandomGrowthEncounterResult.AlreadyEncountered));
            Assert.That(context.StageSession.RandomGrowthSession.ReservationState.EncounteredSectionId,
                Is.EqualTo(RandomGrowthManifestConstants.LeftSectionId));
        }

        [Test]
        public void NotAppearedManifestAddsNoReservationAndKeepsExistingGraph()
        {
            StageDefinitionSO definition = LoadDefinition();
            RandomGrowthTestContext context = FindContext(definition, appeared: false);
            var generator = new StageSvgSlotMapGraphGenerator();
            var graph = new StageGraph(definition.stageId, definition.stageName);

            Assert.That(generator.Generate(definition, graph, context.GraphContext), Is.True);
            Assert.That(generator.LastRandomGrowthReservations, Is.Empty);
            Assert.That(graph.nodes, Has.Count.EqualTo(definition.svgMapSlots.Count));
        }

        [Test]
        public void RebuildUsesStoredManifestWithoutNewStageIdentityOrReroll()
        {
            StageDefinitionSO definition = LoadDefinition();
            RandomGrowthTestContext context = FindContext(definition, appeared: true);
            string fingerprint = context.StageSession.RandomGrowthSession.StoredManifest.Fingerprint;
            int rawRoll = context.StageSession.RandomGrowthSession.StoredManifest.RawRoll;
            string stageGenerationId = context.StageSession.RandomGrowthSession.StageGenerationId;
            var first = new StageSvgSlotMapGraphGenerator();
            var second = new StageSvgSlotMapGraphGenerator();

            Assert.That(first.Generate(
                definition,
                new StageGraph(definition.stageId, definition.stageName),
                context.GraphContext), Is.True);
            Assert.That(second.Generate(
                definition,
                new StageGraph(definition.stageId, definition.stageName),
                context.GraphContext), Is.True);

            Assert.That(context.StageSession.RandomGrowthSession.StageGenerationId,
                Is.EqualTo(stageGenerationId));
            Assert.That(context.StageSession.RandomGrowthSession.StoredManifest.Fingerprint,
                Is.EqualTo(fingerprint));
            Assert.That(context.StageSession.RandomGrowthSession.StoredManifest.RawRoll,
                Is.EqualTo(rawRoll));
            Assert.That(second.LastRandomGrowthReservations.Select(value => value.SlotId),
                Is.EqualTo(first.LastRandomGrowthReservations.Select(value => value.SlotId)));
        }

        [Test]
        public void InvalidCardinalitySuppressesReservationButMainGraphFailsOpen()
        {
            StageDefinitionSO source = LoadDefinition();
            StageDefinitionSO invalid = ScriptableObject.CreateInstance<StageDefinitionSO>();
            try
            {
                invalid.stageId = source.stageId;
                invalid.stageName = source.stageName;
                invalid.svgMapSlots = source.svgMapSlots.Select(CloneSlot).ToList();
                invalid.svgStorySlotBindings = source.svgStorySlotBindings;
                invalid.svgRandomSections = source.svgRandomSections.Select(CloneSection).ToList();
                const string removedSlotId = "slot_440_neg15";
                Section(invalid, RandomGrowthManifestConstants.LeftSectionId)
                    .targetSlotIds.Remove(removedSlotId);
                invalid.svgMapSlots.RemoveAll(slot => slot.slotId == removedSlotId);
                StageMapSlot upstream = invalid.svgMapSlots.Single(slot => slot.slotId == "slot_545_115");
                upstream.connections.RemoveAll(connection => connection.toSlotId == removedSlotId);
                upstream.connections.Add(new StageSlotConnection { toSlotId = "ep_6" });

                StageSession session = NewSession("run.e2.invalid");
                var context = RandomGrowthGraphContext.CreateDefault(
                    session,
                    session.RandomGrowthSession.RunId);
                var generator = new StageSvgSlotMapGraphGenerator();
                var graph = new StageGraph(invalid.stageId, invalid.stageName);

                Assert.That(generator.Generate(invalid, graph, context), Is.True);
                Assert.That(generator.LastRandomGrowthReservations, Is.Empty);
                Assert.That(session.RandomGrowthSession.IsSuppressed, Is.True);
                Assert.That(session.RandomGrowthSession.StoredManifest.RawRoll, Is.EqualTo(-1));
                Assert.That(graph.nodes, Has.Count.EqualTo(invalid.svgMapSlots.Count));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(invalid);
            }
        }

        [Test]
        public void Event16AndPlacementRuleAssetsRemainUnchangedDuringProjection()
        {
            StageDefinitionSO definition = LoadDefinition();
            string eventHashBefore = Sha256(Event16Path);
            string placementPath = AssetDatabase.GUIDToAssetPath(
                AssetDatabase.AssetPathToGUID(
                    AssetDatabase.GetAssetPath(
                        Section(definition, RandomGrowthManifestConstants.LeftSectionId).placementRule)));
            string placementHashBefore = Sha256(placementPath);
            RandomGrowthTestContext context = FindContext(definition, appeared: true);

            var generator = new StageSvgSlotMapGraphGenerator();
            Assert.That(generator.Generate(
                definition,
                new StageGraph(definition.stageId, definition.stageName),
                context.GraphContext), Is.True);

            Assert.That(eventHashBefore, Is.EqualTo(Event16Sha256));
            Assert.That(Sha256(Event16Path), Is.EqualTo(eventHashBefore));
            Assert.That(Sha256(placementPath), Is.EqualTo(placementHashBefore));
        }

        private static StageDefinitionSO LoadDefinition()
        {
            StageDefinitionSO definition = AssetDatabase.LoadAssetAtPath<StageDefinitionSO>(DefinitionPath);
            Assert.That(definition, Is.Not.Null);
            return definition;
        }

        private static StageRandomSection Section(StageDefinitionSO definition, string sectionId) =>
            definition.svgRandomSections.Single(value => value.sectionId == sectionId);

        private static StageRandomSection CloneSection(StageRandomSection source) => new()
        {
            sectionId = source.sectionId,
            fromStorySlotId = source.fromStorySlotId,
            toStorySlotId = source.toStorySlotId,
            targetSlotIds = new List<string>(source.targetSlotIds),
            placementRule = source.placementRule
        };

        private static StageMapSlot CloneSlot(StageMapSlot source) => new()
        {
            slotId = source.slotId,
            role = source.role,
            depth = source.depth,
            orderInDepth = source.orderInDepth,
            label = source.label,
            subLabel = source.subLabel,
            connections = (source.connections ?? new List<StageSlotConnection>())
                .Select(connection => new StageSlotConnection { toSlotId = connection.toSlotId })
                .ToList()
        };

        private static RandomGrowthTestContext FindContext(
            StageDefinitionSO definition,
            bool appeared)
        {
            for (int index = 0; index < RandomGrowthManifestConstants.RollRange; index++)
            {
                string identity = (appeared ? "appeared." : "absent.") + index;
                StageSession session = NewSession("run.e2." + identity);
                ProgressionRunId runId = session.RandomGrowthSession.RunId;
                var graphContext = RandomGrowthGraphContext.CreateDefault(session, runId);
                RandomGrowthProjectionResult result = graphContext.Project(definition);
                if (session.RandomGrowthSession.StoredManifest.Appeared == appeared)
                {
                    return new RandomGrowthTestContext(session, graphContext, result);
                }
            }

            Assert.Fail("Unable to locate a canonical manifest fixture with the requested appearance state.");
            return null;
        }

        private static StageSession NewSession(string runId)
        {
            var session = new StageSession();
            session.ResetRandomGrowthForNewRun(new ProgressionRunId(runId));
            return session;
        }

        private static string Sha256(string assetPath)
        {
            using SHA256 hash = SHA256.Create();
            byte[] bytes = File.ReadAllBytes(Path.GetFullPath(assetPath));
            return string.Concat(hash.ComputeHash(bytes).Select(value => value.ToString("x2")));
        }

        private sealed class RandomGrowthTestContext
        {
            public RandomGrowthTestContext(
                StageSession stageSession,
                RandomGrowthGraphContext graphContext,
                RandomGrowthProjectionResult result)
            {
                StageSession = stageSession;
                GraphContext = graphContext;
                Result = result;
            }

            public StageSession StageSession { get; }
            public RandomGrowthGraphContext GraphContext { get; }
            public RandomGrowthProjectionResult Result { get; }
        }

    }
}
