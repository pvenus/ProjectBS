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
