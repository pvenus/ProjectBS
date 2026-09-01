#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Progression.Portfolio;
using ResourceTools.Stage.Placement;
using Stage;
using UnityEditor;
using UnityEngine;

namespace ProjectBS.EditorTests.Stage
{
    public sealed class WeightedPlacementPhase1Tests
    {
        [Test]
        public void JsonRoundTrip_IsDeterministicallyOrdered()
        {
            StagePlacementRuleSO catalog = CreateRule(46);
            string first = WeightedPlacementJsonCodec.Serialize(catalog);
            string second = WeightedPlacementJsonCodec.Serialize(catalog);
            Assert.AreEqual(first, second);
            UnityEngine.Object.DestroyImmediate(catalog);
        }

        [Test]
        public void Validator_FailsDuplicateAndChildTopLevelRows()
        {
            WeightedPlacementJsonDocument document = WeightedPlacementJsonCodec.FromRule(CreateRule(46));
            document.rows[1].eventId = document.rows[0].eventId;
            document.rows.Add(new EventRowJson { eventId = "event.act1.random_event.34.half_vein_map.followup.unstable_vein" });
            IReadOnlyList<string> errors = WeightedPlacementJsonValidator.Validate(document);
            Assert.That(errors, Has.Some.EqualTo("EXACT46_REQUIRED"));
            Assert.That(errors, Has.Some.EqualTo("DUPLICATE_EVENT_ID"));
            Assert.That(errors, Has.Some.EqualTo("EVENT34_CHILD_TOP_LEVEL_INVALID"));
        }

        [Test]
        public void ManifestBuilder_IsSeedDeterministicAndMeetsBudget()
        {
            StagePlacementRuleSO catalog = CreateRule(46);
            var builder = new Chapter1WeightedEventManifestBuilder();
            Chapter1WeightedEventManifest a = builder.Build(catalog.weightedPool, 4127, Array.Empty<string>(), _ => true);
            Chapter1WeightedEventManifest b = builder.Build(catalog.weightedPool, 4127, Array.Empty<string>(), _ => true);
            Assert.IsTrue(a.Success, a.Error);
            Assert.AreEqual(12, a.Assignments.Count);
            CollectionAssert.AreEqual(a.Assignments.Select(x => x.Row.eventId), b.Assignments.Select(x => x.Row.eventId));
            UnityEngine.Object.DestroyImmediate(catalog);
        }

        [Test]
        public void LegacyFallback_DoesNotRepeatManifestAssignedOneShotNode()
        {
            var rule = ScriptableObject.CreateInstance<StagePlacementRuleSO>();
            var pool = ScriptableObject.CreateInstance<EventPoolSO>();
            var oneShot = ScriptableObject.CreateInstance<RoundNodeSO>();
            var repeatable = ScriptableObject.CreateInstance<RoundNodeSO>();
            try
            {
                rule.weightedPool.avoidDuplicateInSection = false;
                rule.weightedPool.rows.Add(new WeightedPlacementEventRow
                {
                    eventId = "event.act1.random_event.26.sleepless_waystation",
                    node = oneShot,
                    oneShot = true,
                    topLevelEligible = true,
                    rawWeight = 100
                });
                pool.entries.Add(new EventPoolEntry
                    { node = oneShot, entryId = "event26", weight = 100 });
                pool.entries.Add(new EventPoolEntry
                    { node = repeatable, entryId = "repeatable", weight = 1 });
                rule.weightedPool.pools.Add(new StagePlacementPoolEntry
                    { pool = pool, weight = 100 });

                var assignments = new Dictionary<string, RoundNodeSO>
                    { ["manifest-slot"] = oneShot };
                var section = new StageRandomSection { sectionId = "fallback" };
                var slots = new[]
                {
                    new StageMapSlot { slotId = "fallback-1", role = StageMapSlotRole.Random },
                    new StageMapSlot { slotId = "fallback-2", role = StageMapSlotRole.Random }
                };

                rule.Fill(section, slots, assignments, new System.Random(26));

                Assert.AreSame(oneShot, assignments["manifest-slot"]);
                Assert.AreSame(repeatable, assignments["fallback-1"]);
                Assert.AreSame(repeatable, assignments["fallback-2"],
                    "Non-oneShot legacy repetition must remain compatible when the section guard is disabled.");
                Assert.AreEqual(1, assignments.Values.Count(node => node == oneShot));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(oneShot);
                UnityEngine.Object.DestroyImmediate(repeatable);
                UnityEngine.Object.DestroyImmediate(pool);
                UnityEngine.Object.DestroyImmediate(rule);
            }
        }

        [Test]
        public void Projection_MapsLogicalAssignmentsByDepth_NotFlattenedPhysicalSlotIndex()
        {
            var definition = ScriptableObject.CreateInstance<StageDefinitionSO>();
            var first = ScriptableObject.CreateInstance<RoundNodeSO>();
            var second = ScriptableObject.CreateInstance<RoundNodeSO>();
            try
            {
                definition.svgMapSlots.AddRange(new[]
                {
                    new StageMapSlot { slotId = "depth-0-a", depth = 0, orderInDepth = 0, role = StageMapSlotRole.Random },
                    new StageMapSlot { slotId = "depth-0-b", depth = 0, orderInDepth = 1, role = StageMapSlotRole.Random },
                    new StageMapSlot { slotId = "depth-1-a", depth = 1, orderInDepth = 0, role = StageMapSlotRole.Random },
                    new StageMapSlot { slotId = "depth-1-b", depth = 1, orderInDepth = 1, role = StageMapSlotRole.Random }
                });
                var manifest = new Chapter1WeightedEventManifest(true, string.Empty, new[]
                {
                    new Chapter1WeightedEventAssignment(0, WeightedPlacementBand.Early,
                        new WeightedPlacementEventRow { node = first }, false),
                    new Chapter1WeightedEventAssignment(1, WeightedPlacementBand.Mid,
                        new WeightedPlacementEventRow { node = second }, false)
                });

                IReadOnlyDictionary<string, RoundNodeSO> projected =
                    Chapter1WeightedEventProjection.Project(definition, manifest);

                Assert.AreSame(first, projected["depth-0-a"]);
                Assert.AreSame(first, projected["depth-0-b"]);
                Assert.AreSame(second, projected["depth-1-a"]);
                Assert.AreSame(second, projected["depth-1-b"]);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(first);
                UnityEngine.Object.DestroyImmediate(second);
                UnityEngine.Object.DestroyImmediate(definition);
            }
        }

        [Test]
        public void Projection_MovesRouteBoundEventToBranchCapableDepth()
        {
            var definition = ScriptableObject.CreateInstance<StageDefinitionSO>();
            var ordinary = ScriptableObject.CreateInstance<RoundNodeSO>();
            var route = ScriptableObject.CreateInstance<RoundNodeSO>();
            var popup = ScriptableObject.CreateInstance<PopupEventSO>();
            try
            {
                route.popupEvent = popup;
                popup.choices.Add(new PopupEventChoice
                {
                    choiceId = "route",
                    executionConfig = new ChoiceExecutionConfig
                    {
                        executionType = ChoiceExecutionType.PortfolioOutcome,
                        data = new PortfolioOutcomeExecutionData
                        {
                            operations = new List<PortfolioOutcomeOperationData>
                            {
                                new() { kind = PortfolioOutcomeOperationKind.CommitImmediateSuccessorRoute,
                                    selectionMode = ImmediateSuccessorRouteSelectionMode.ShortestRemainingToSectionExit }
                            }
                        }
                    }
                });
                definition.svgMapSlots.AddRange(new[]
                {
                    new StageMapSlot { slotId = "single", depth = 0, role = StageMapSlotRole.Random,
                        connections = new List<StageSlotConnection> { new() { toSlotId = "next" } } },
                    new StageMapSlot { slotId = "branch", depth = 1, role = StageMapSlotRole.Random,
                        connections = new List<StageSlotConnection> { new() { toSlotId = "left" }, new() { toSlotId = "right" } } }
                });
                var manifest = new Chapter1WeightedEventManifest(true, string.Empty, new[]
                {
                    new Chapter1WeightedEventAssignment(0, WeightedPlacementBand.Early,
                        new WeightedPlacementEventRow { node = route }, false),
                    new Chapter1WeightedEventAssignment(1, WeightedPlacementBand.Mid,
                        new WeightedPlacementEventRow { node = ordinary }, false)
                });

                IReadOnlyDictionary<string, RoundNodeSO> projected =
                    Chapter1WeightedEventProjection.Project(definition, manifest);

                Assert.AreSame(ordinary, projected["single"]);
                Assert.AreSame(route, projected["branch"]);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(popup);
                UnityEngine.Object.DestroyImmediate(route);
                UnityEngine.Object.DestroyImmediate(ordinary);
                UnityEngine.Object.DestroyImmediate(definition);
            }
        }

        [Test]
        public void BattlePressureBuilder_ProducesLockedExactCompositionAndPhaseCounts()
        {
            StagePlacementRuleSO rule = CreateRule(46);
            var battlePool = ScriptableObject.CreateInstance<EventPoolSO>();
            var shopPool = ScriptableObject.CreateInstance<EventPoolSO>();
            var restPool = ScriptableObject.CreateInstance<EventPoolSO>();
            var created = new List<RoundNodeSO>();
            try
            {
                for (int i = 0; i < 5; i++)
                {
                    var node = ScriptableObject.CreateInstance<RoundNodeSO>();
                    node.nodeType = RoundNodeType.Battle; created.Add(node);
                    battlePool.entries.Add(new EventPoolEntry { node = node, weight = 100 });
                }
                var shop = ScriptableObject.CreateInstance<RoundNodeSO>();
                shop.nodeType = RoundNodeType.Shop; created.Add(shop);
                shopPool.entries.Add(new EventPoolEntry { node = shop, weight = 100 });
                var rest = ScriptableObject.CreateInstance<RoundNodeSO>();
                rest.nodeType = RoundNodeType.Rest; created.Add(rest);
                restPool.entries.Add(new EventPoolEntry { node = rest, weight = 100 });
                rule.weightedPool.composition = new BattlePressureCompositionConfig
                {
                    enabled = true, directBattlePool = battlePool,
                    shopPool = shopPool, restPool = restPool
                };

                Chapter1BattlePressureManifest manifest =
                    new Chapter1BattlePressureManifestBuilder().Build(rule.weightedPool, 17);

                Assert.IsTrue(manifest.Success, manifest.Error);
                Assert.AreEqual(12, manifest.Assignments.Count);
                Assert.AreEqual(4, manifest.Assignments.Count(x => x.Kind == Chapter1EncounterKind.DirectBattle));
                Assert.AreEqual(2, manifest.Assignments.Count(x => x.Kind == Chapter1EncounterKind.Shop));
                Assert.AreEqual(2, manifest.Assignments.Count(x => x.Kind == Chapter1EncounterKind.Rest));
                Assert.AreEqual(4, manifest.Assignments.Count(x => x.Kind == Chapter1EncounterKind.Event));
                Assert.AreEqual(1, manifest.Assignments.Count(x => x.Phase == WeightedPlacementBand.Early
                    && x.Kind == Chapter1EncounterKind.DirectBattle));
                Assert.AreEqual(2, manifest.Assignments.Count(x => x.Phase == WeightedPlacementBand.Mid
                    && x.Kind == Chapter1EncounterKind.DirectBattle));
                Assert.AreEqual(1, manifest.Assignments.Count(x => x.Phase == WeightedPlacementBand.Late
                    && x.Kind == Chapter1EncounterKind.DirectBattle));
                Assert.IsFalse(manifest.Assignments.Zip(manifest.Assignments.Skip(1),
                    (left, right) => left.Kind == Chapter1EncounterKind.DirectBattle
                        && right.Kind == Chapter1EncounterKind.DirectBattle).Any(value => value));
                int[] directOrdinals = manifest.Assignments
                    .Where(x => x.Kind == Chapter1EncounterKind.DirectBattle)
                    .Select(x => x.Ordinal).ToArray();
                Assert.LessOrEqual(directOrdinals[0] - 1, 3);
                Assert.LessOrEqual(12 - directOrdinals[^1], 3);
                Assert.IsTrue(directOrdinals.Zip(directOrdinals.Skip(1),
                    (left, right) => right - left - 1 <= 3).All(value => value));
                Assert.AreEqual(4, manifest.Assignments
                    .Where(x => x.Kind == Chapter1EncounterKind.DirectBattle)
                    .Select(x => x.Node).Distinct().Count());
            }
            finally
            {
                foreach (RoundNodeSO node in created) UnityEngine.Object.DestroyImmediate(node);
                UnityEngine.Object.DestroyImmediate(battlePool);
                UnityEngine.Object.DestroyImmediate(shopPool);
                UnityEngine.Object.DestroyImmediate(restPool);
                UnityEngine.Object.DestroyImmediate(rule);
            }
        }

        [Test]
        public void AuthoringMetadata_RoundTripsLosslesslyWithDeterministicDigest()
        {
            StagePlacementRuleSO catalog = CreateRule(46);
            catalog.weightedPool.overrides.Add(new WeightedPlacementOverride { overrideId="override.1",rowId=catalog.weightedPool.rows[0].eventId,
                field="rawWeight",baseContractVersion=catalog.weightedPool.contractVersion,oldValue="10",newValue="9",
                rationale="evidence-backed",evidence="receipt",affectedDistributionCells=new List<string>{"early.new"} });
            string json = WeightedPlacementJsonCodec.Serialize(catalog);
            WeightedPlacementJsonDocument document = WeightedPlacementJsonCodec.Parse(json);
            Assert.AreEqual("authority", document.rows[0].sourceAuthority);
            Assert.AreEqual("override.1", document.overrides.Single().overrideId);
            Assert.AreEqual(document.canonicalContentSha256, WeightedPlacementJsonCodec.ComputeCanonicalDigest(document));
        }

        [Test]
        public void Validator_FailsDigestTamperAndStaleDependency()
        {
            WeightedPlacementJsonDocument document = WeightedPlacementJsonCodec.Parse(
                WeightedPlacementJsonCodec.Serialize(CreateRule(46)));
            document.rows[0].rawWeight++;
            document.staleRecalcState = WeightedPlacementStaleState.StaleRewardRiskDependency.ToString();
            IReadOnlyList<string> errors = WeightedPlacementJsonValidator.Validate(document);
            Assert.That(errors, Has.Some.EqualTo("DIGEST_MISMATCH"));
            Assert.That(errors, Has.Some.EqualTo("STALE_RECALC_REQUIRED"));
        }

        [Test]
        public void AuthoritativeExact46_CompilesWithStableReferencesAndProjection()
        {
            const string path = "Assets/Contents/Stage/placement/json/chapter1-events01-46.weighted-placement.json";
            string json = File.ReadAllText(path);
            WeightedPlacementJsonDocument document = WeightedPlacementJsonCodec.Parse(json);
            CollectionAssert.IsEmpty(WeightedPlacementJsonValidator.Validate(document));
            Assert.AreEqual("6e58a3620aadfe1d87cb9058e29e4fcbb9c36c6a3d11dd6a9bc57fe814d43661",
                WeightedPlacementJsonCodec.ComputeCanonicalDigest(document));

            StagePlacementRuleSO catalog = WeightedPlacementApplyService.CompileTransient(json);
            try
            {
                WeightedPoolPlacementConfig config = catalog.weightedPool;
                Assert.AreEqual(46, config.rows.Count);
                Assert.AreEqual(46, config.rows.Count(row => row.node != null));
                Assert.AreEqual(46, config.rows.Select(row => row.node).Distinct().Count());
                Assert.AreEqual(20, config.rows.Count(row => row.generation == WeightedPlacementGeneration.Legacy));
                Assert.AreEqual(26, config.rows.Count(row => row.generation == WeightedPlacementGeneration.New));
                Assert.AreEqual(10, config.rows.Count(row => row.primaryBand == WeightedPlacementBand.All));
                Assert.AreEqual(8, config.rows.Count(row => row.primaryBand == WeightedPlacementBand.Early));
                Assert.AreEqual(16, config.rows.Count(row => row.primaryBand == WeightedPlacementBand.Mid));
                Assert.AreEqual(12, config.rows.Count(row => row.primaryBand == WeightedPlacementBand.Late));

                string exported = WeightedPlacementJsonCodec.Serialize(catalog);
                WeightedPlacementJsonDocument roundTrip = WeightedPlacementJsonCodec.Parse(exported);
                CollectionAssert.IsEmpty(WeightedPlacementJsonValidator.Validate(roundTrip));
                Assert.AreEqual(document.canonicalContentSha256, roundTrip.canonicalContentSha256);
                CollectionAssert.AreEqual(document.rows.Select(row => row.eventId), roundTrip.rows.Select(row => row.eventId));
                CollectionAssert.AreEqual(document.rows.Select(row => row.nodeGuid), roundTrip.rows.Select(row => row.nodeGuid));

                string[] roster = config.rows.Select(row => row.requiredCharacterId)
                    .Where(value => !string.IsNullOrWhiteSpace(value)).Distinct().ToArray();
                var builder = new Chapter1WeightedEventManifestBuilder();
                var reachable = new HashSet<string>();
                int legacy = 0, newer = 0;
                for (int seed = 0; seed < 512; seed++)
                {
                    Chapter1WeightedEventManifest first = builder.Build(config, seed, roster, _ => true);
                    Chapter1WeightedEventManifest second = builder.Build(config, seed, roster, _ => true);
                    Assert.IsTrue(first.Success, $"seed={seed}:{first.Error}");
                    Assert.AreEqual(12, first.Assignments.Count);
                    CollectionAssert.AreEqual(first.Assignments.Select(item => item.Row.eventId),
                        second.Assignments.Select(item => item.Row.eventId));
                    foreach (Chapter1WeightedEventAssignment item in first.Assignments)
                    {
                        reachable.Add(item.Row.eventId);
                        if (item.Row.generation == WeightedPlacementGeneration.Legacy) legacy++;
                        else newer++;
                    }
                }
                CollectionAssert.IsSubsetOf(config.rows.Where(row => row.topLevelEligible)
                    .Select(row => row.eventId), reachable);
                Assert.IsFalse(reachable.Contains("event.act1.random_event.11.dry_waterwheel"));
                double newShare = newer / (double)(legacy + newer);
                Assert.That(newShare, Is.InRange(0.50, 0.60), $"newShare={newShare}");

                foreach (WeightedPlacementEventRow row in config.rows) row.topLevelEligible = false;
                Chapter1WeightedEventManifest empty = builder.Build(config, 1, roster, _ => true);
                Assert.IsFalse(empty.Success);
                Assert.AreEqual("WEIGHTED_PLACEMENT_INITIAL_BREADTH_INSUFFICIENT", empty.Error);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(catalog);
            }
        }

        [Test]
        public void ImmediateGenerator_OnlyAcceptsCanonicalDocumentIdentity()
        {
            TextAsset canonical = AssetDatabase.LoadAssetAtPath<TextAsset>(
                "Assets/Contents/Stage/placement/json/chapter1-events01-46.weighted-placement.json");
            TextAsset schema = AssetDatabase.LoadAssetAtPath<TextAsset>(
                "Assets/Contents/Stage/placement/schema/WeightedPoolPlacementRule.schema.json");
            StagePlacementRuleSO catalog = AssetDatabase.LoadAssetAtPath<StagePlacementRuleSO>(
                WeightedPlacementApplyService.RulePath);
            Assert.NotNull(canonical);
            Assert.NotNull(schema);
            Assert.NotNull(catalog);
            Assert.IsNull(AssetDatabase.LoadAssetAtPath<StagePlacementRuleSO>(
                "Assets/Resources/definitions/WeightedPoolPlacementRule.asset"));
            Assert.AreEqual("90ef4f6016c18d342a0f6aaf00e394bd",
                AssetDatabase.AssetPathToGUID(WeightedPlacementApplyService.RulePath));
            string before = File.ReadAllText(WeightedPlacementApplyService.RulePath);
            UnityEngine.Object[] previous = Selection.objects;
            try
            {
                Selection.objects = Array.Empty<UnityEngine.Object>();
                Assert.IsFalse(WeightedPoolPlacementRuleGenerator.TryGetSelectedDocument(out _, out _));
                Selection.objects = new UnityEngine.Object[] { canonical, schema };
                Assert.IsFalse(WeightedPoolPlacementRuleGenerator.TryGetSelectedDocument(out _, out _));
                Selection.objects = new UnityEngine.Object[] { schema };
                Assert.IsFalse(WeightedPoolPlacementRuleGenerator.TryGetSelectedDocument(out _, out _));
                Assert.IsFalse(WeightedPoolPlacementRuleGenerator.TryGenerateSelected(false));
                Assert.AreEqual(before, File.ReadAllText(WeightedPlacementApplyService.RulePath));
                Selection.objects = new UnityEngine.Object[] { new TextAsset("{not-json") };
                Assert.IsFalse(WeightedPoolPlacementRuleGenerator.TryGetSelectedDocument(out _, out _));
                Selection.objects = new UnityEngine.Object[] { canonical };
                Assert.IsTrue(WeightedPoolPlacementRuleGenerator.TryGetSelectedDocument(out _, out _));
            }
            finally
            {
                Selection.objects = previous;
            }
        }

        private static StagePlacementRuleSO CreateRule(int count)
        {
            var rule = ScriptableObject.CreateInstance<StagePlacementRuleSO>();
            WeightedPoolPlacementConfig catalog = rule.weightedPool;
            catalog.schemaVersion = WeightedPoolPlacementConfig.CurrentSchemaVersion;
            catalog.documentType = WeightedPoolPlacementConfig.DocumentType;
            catalog.catalogId = "catalog.test"; catalog.chapterId = "chapter01";
            catalog.sourceRewardRiskDefinition = "AgentDocs/reward-risk.json";
            catalog.rewardRiskDefinitionSha256 = new string('a', 64);
            for (int i = 0; i < count; i++)
            {
                int number = i + 1;
                catalog.rows.Add(new WeightedPlacementEventRow {
                    eventId = $"event.act1.random_event.{number:00}.test", topLevelNodeId = $"node.{number:00}",
                    generation = i % 2 == 0 ? WeightedPlacementGeneration.Legacy : WeightedPlacementGeneration.New,
                    primaryBand = (WeightedPlacementBand)new[] { 10, 20, 30, 0 }[i % 4], rawWeight = 10,
                    node = ScriptableObject.CreateInstance<RoundNodeSO>(),
                    topLevelEligible = true, primaryPurpose = (PortfolioPurpose)((i % 5) * 10), secondaryPurpose = PortfolioPurpose.World,
                    oneShot = true, order = i, rationale = "locked rationale",
                    sourceAuthority = "authority", staleState = WeightedPlacementStaleState.Current
                });
            }
            catalog.rows[10].eventId = "event.act1.random_event.11.child_only"; catalog.rows[10].topLevelEligible = false; catalog.rows[10].rawWeight = 0;
            catalog.rows[15].eventId = "event.act1.random_event.16.gated"; catalog.rows[15].capabilityGate = "relic_p1";
            return rule;
        }
    }
}
#endif
