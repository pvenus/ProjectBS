#if UNITY_EDITOR
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using NUnit.Framework;
using ResourceTools.Stage;
using Stage;
using UnityEditor;
using UnityEngine;

public sealed class RandomGrowthGeneratedAssetBuilderTests
{
    [Test]
    public void EnumValuesPreserveLegacyAndAppendStableRandomGrowthValues()
    {
        Assert.That((int)ChoiceExecutionType.None, Is.EqualTo(0));
        Assert.That((int)ChoiceExecutionType.NextEvent, Is.EqualTo(50));
        Assert.That((int)ChoiceExecutionType.Battle, Is.EqualTo(100));
        Assert.That((int)ChoiceExecutionType.Shop, Is.EqualTo(200));
        Assert.That((int)ChoiceExecutionType.Shrine, Is.EqualTo(300));
        Assert.That((int)ChoiceExecutionType.CompleteEvent, Is.EqualTo(900));
        Assert.That((int)ChoiceExecutionType.RandomGrowthRisk, Is.EqualTo(1000));
        Assert.That((int)ChoiceExecutionType.RandomGrowthDecline, Is.EqualTo(1010));
        Assert.That((int)ChoiceExecutionType.RandomGrowthSafe, Is.EqualTo(1020));
    }

    [TestCase(ChoiceExecutionType.RandomGrowthRisk, typeof(RandomGrowthRiskExecutionData))]
    [TestCase(ChoiceExecutionType.RandomGrowthDecline, typeof(RandomGrowthDeclineExecutionData))]
    public void FactoryCreatesTypedSerializeReferenceData(ChoiceExecutionType type, System.Type expected)
    {
        ChoiceExecutionConfig config = ChoiceExecutionDataFactory.CreateConfig(type);
        Assert.That(config.data.GetType(), Is.EqualTo(expected));
        Assert.That(ChoiceExecutionConfigValidator.IsTypeMatch(type, config.data), Is.True);
    }

    [Test]
    public void BlueprintHasExactThreeOutputManifestAndOrderedChoices()
    {
        var source = RandomGrowthContentContractValidator.ValidateFiles();
        RandomGrowthProjectionBlueprint b = RandomGrowthGeneratedAssetBuilder.CreateBlueprint(source);
        Assert.That(b.Paths.RoundNodeAssetPath, Is.EqualTo("Assets/Contents/Stage/so/stage.act1.random_growth.01.crying_bell_smithy_trial.asset"));
        Assert.That(b.Paths.PopupEventAssetPath, Is.EqualTo("Assets/Contents/Stage/so/node.act1.random_growth.01.crying_bell_smithy_trial.intro.asset"));
        Assert.That(b.Paths.EventPoolAssetPath, Is.EqualTo("Assets/Contents/Stage/so/event_pool.act1.random_growth.cheongun_sangui.asset"));
        Assert.That(new[] { b.Paths.RoundNodeAssetPath, b.Paths.PopupEventAssetPath, b.Paths.EventPoolAssetPath }.Distinct().Count(), Is.EqualTo(3));
        Assert.That(b.Risk.choiceId, Is.EqualTo(RandomGrowthContentContractValidator.RiskChoiceId));
        Assert.That(b.Decline.choiceId, Is.EqualTo(RandomGrowthContentContractValidator.DeclineChoiceId));
        Assert.That(b.Risk.rewards, Is.Empty);
        Assert.That(b.Decline.rewards, Is.Empty);
    }

    [Test]
    public void PreflightAcceptsCanonicalOrdinaryPlacementWithoutWritingAssetOrMeta()
    {
        const string expectedSha = "6215b492aa0ac29344e93e2d5676a30d51ffc2504078e0fc064dfa1b2239a60d";
        byte[] assetBefore = File.ReadAllBytes(RandomGrowthGeneratedAssetBuilder.OrdinaryPlacementRulePath);
        byte[] metaBefore = File.ReadAllBytes(RandomGrowthGeneratedAssetBuilder.OrdinaryPlacementRulePath + ".meta");
        OrdinaryPoolLinkSnapshot before = RandomGrowthGeneratedAssetBuilder.CaptureOrdinaryPoolLinkSnapshot();
        RandomGrowthGeneratedAssetPreflight result = RandomGrowthGeneratedAssetBuilder.Preflight();
        OrdinaryPoolLinkSnapshot after = RandomGrowthGeneratedAssetBuilder.CaptureOrdinaryPoolLinkSnapshot();
        Assert.That(result.Disposition, Is.EqualTo(RandomGrowthGenerationDisposition.NoOp));
        Assert.That(result.Errors, Is.Empty);
        Assert.That(after, Is.EqualTo(before));
        Assert.That(before.Sha256, Is.EqualTo(expectedSha));
        Assert.That(before.PoolCardinality, Is.EqualTo(2));
        Assert.That(before.ManifestOnlyLinkCardinality, Is.Zero);
        CollectionAssert.AreEqual(assetBefore,
            File.ReadAllBytes(RandomGrowthGeneratedAssetBuilder.OrdinaryPlacementRulePath));
        CollectionAssert.AreEqual(metaBefore,
            File.ReadAllBytes(RandomGrowthGeneratedAssetBuilder.OrdinaryPlacementRulePath + ".meta"));
        Assert.That(File.Exists(result.Blueprint.Paths.RoundNodeAssetPath), Is.True);
        Assert.That(File.Exists(result.Blueprint.Paths.PopupEventAssetPath), Is.True);
        Assert.That(File.Exists(result.Blueprint.Paths.EventPoolAssetPath), Is.True);
    }

    [TestCase(false, false, false, false, RandomGrowthGenerationDisposition.ReadyToGenerate)]
    [TestCase(true, false, false, false, RandomGrowthGenerationDisposition.Blocked)]
    [TestCase(true, true, false, false, RandomGrowthGenerationDisposition.Blocked)]
    [TestCase(true, true, true, false, RandomGrowthGenerationDisposition.Blocked)]
    [TestCase(true, true, true, true, RandomGrowthGenerationDisposition.NoOp)]
    public void OutputSetEvaluationIsNewOnlyAndFailClosed(
        bool popup,
        bool node,
        bool pool,
        bool semanticMatch,
        RandomGrowthGenerationDisposition expected)
    {
        Assert.That(RandomGrowthGeneratedAssetBuilder.EvaluateOutputSet(
            popup, node, pool, semanticMatch), Is.EqualTo(expected));
    }

    [Test]
    public void SafeV2CatalogSyntheticPreflightIsAdditiveNoOpAndWriteFree()
    {
        SafeV2ProductionSnapshot before =
            RandomGrowthGeneratedAssetBuilder.CaptureSafeV2ProductionSnapshot();
        RandomGrowthPresentationCatalogPreflight add =
            RandomGrowthGeneratedAssetBuilder.PreflightSafeV2Catalog(
                true, true, true, true, false, false);
        RandomGrowthPresentationCatalogPreflight noOp =
            RandomGrowthGeneratedAssetBuilder.PreflightSafeV2Catalog(
                true, true, true, true, true, true);
        Assert.That(add.Disposition, Is.EqualTo(RandomGrowthGenerationDisposition.ReadyToGenerate));
        Assert.That(add.CanWrite, Is.True);
        Assert.That(noOp.Disposition, Is.EqualTo(RandomGrowthGenerationDisposition.NoOp));
        Assert.That(noOp.Errors, Is.Empty);
        AssertSafeV2ProductionSnapshotUnchanged(before,
            RandomGrowthGeneratedAssetBuilder.CaptureSafeV2ProductionSnapshot());
    }

    [Test]
    public void SafeV2CatalogPartialCollisionAndMismatchBlockWithoutWrites()
    {
        SafeV2ProductionSnapshot before =
            RandomGrowthGeneratedAssetBuilder.CaptureSafeV2ProductionSnapshot();
        RandomGrowthPresentationCatalogPreflight partial =
            RandomGrowthGeneratedAssetBuilder.PreflightSafeV2Catalog(
                true, false, false, false, false, false);
        RandomGrowthPresentationCatalogPreflight mismatch =
            RandomGrowthGeneratedAssetBuilder.PreflightSafeV2Catalog(
                true, true, true, true, true, false);
        RandomGrowthPresentationCatalogPreflight collision =
            RandomGrowthGeneratedAssetBuilder.PreflightSafeV2Catalog(
                true, true, true, true, false, false, new[] { "duplicate.catalog" });
        Assert.That(partial.Errors, Does.Contain("SAFE_CORE_PARTIAL_OUTPUT_SET"));
        Assert.That(mismatch.Errors, Does.Contain("PRESENTATION_CATALOG_SNAPSHOT_MISMATCH"));
        Assert.That(collision.Errors, Does.Contain("PRESENTATION_CATALOG_COLLISION:duplicate.catalog"));
        Assert.That(new[] { partial, mismatch, collision }.All(x => !x.CanWrite), Is.True);
        AssertSafeV2ProductionSnapshotUnchanged(before,
            RandomGrowthGeneratedAssetBuilder.CaptureSafeV2ProductionSnapshot());
    }

    [Test]
    public void SafeV2ProductionJson31AndCatalogPreflightIsNoOpAndReadOnly()
    {
        RandomGrowthSafeContentValidation source =
            RandomGrowthSafeProjectionContract.ValidateProductionV2Files();
        Assert.That(source.IsValid, Is.True, string.Join("\n", source.Errors));
        Assert.That(source.Content.schemaVersion, Is.EqualTo(2));
        Assert.That(source.Content.semanticCopyKo, Has.Count.EqualTo(31));
        Assert.That(source.Content.presentationCatalogId,
            Is.EqualTo(RandomGrowthSafeProjectionContract.V2CatalogId));
        Assert.That(source.Content.semanticCopyDigest,
            Is.EqualTo(RandomGrowthSafeProjectionContract.V2SemanticCopyDigest));
        Assert.That(source.Content.definitionFingerprint,
            Is.EqualTo(RandomGrowthSafeProjectionContract.V2DefinitionFingerprint));

        SafeV2ProductionSnapshot before =
            RandomGrowthGeneratedAssetBuilder.CaptureSafeV2ProductionSnapshot();
        RandomGrowthPresentationCatalogPreflight result =
            RandomGrowthGeneratedAssetBuilder.PreflightSafeV2Production();
        SafeV2ProductionSnapshot after =
            RandomGrowthGeneratedAssetBuilder.CaptureSafeV2ProductionSnapshot();
        Assert.That(result.Disposition, Is.EqualTo(RandomGrowthGenerationDisposition.NoOp));
        Assert.That(result.Errors, Is.Empty);
        Assert.That(File.Exists(result.Blueprint.AssetPath), Is.True);
        AssertSafeV2ProductionSnapshotUnchanged(before, after);
        Assert.That(before.PopupSha, Is.EqualTo(
            "1a600c2c3eadf50f659bf2c54c348c0badbd7bb883dee08bba12f11e855a51ee"));
        Assert.That(before.CatalogSha, Is.EqualTo(
            "25cc9beb8522b16148d10197b592bc177a785d082130ad1b5cc1f21920391ef7"));
        Assert.That(before.CatalogGuid, Is.EqualTo("7f41149e2c81047798fd8a3b97dc762d"));
        RandomGrowthPresentationCopyAsset catalog =
            AssetDatabase.LoadAssetAtPath<RandomGrowthPresentationCopyAsset>(result.Blueprint.AssetPath);
        Assert.That(catalog, Is.Not.Null);
        Assert.That(catalog.SchemaVersion, Is.EqualTo(2));
        Assert.That(catalog.ContentContractVersion,
            Is.EqualTo(RandomGrowthSafeProjectionContract.V2ContentContractVersion));
        Assert.That(catalog.CatalogId, Is.EqualTo(RandomGrowthSafeProjectionContract.V2CatalogId));
        Assert.That(catalog.ProjectionKind,
            Is.EqualTo(RandomGrowthSafeProjectionContract.V2ProjectionKind));
        Assert.That(catalog.SemanticCopyDigest,
            Is.EqualTo(RandomGrowthSafeProjectionContract.V2SemanticCopyDigest));
        Assert.That(catalog.DefinitionFingerprint,
            Is.EqualTo(RandomGrowthSafeProjectionContract.V2DefinitionFingerprint));
        Assert.That(catalog.Fields, Has.Count.EqualTo(31));
    }

    [Test]
    public void SafeV2ProductionValidatorRejectsV1MixedSubsetAndWrongIdentity()
    {
        string eventJson = File.ReadAllText(RandomGrowthSafeProjectionContract.EventJsonPath);
        string poolJson = File.ReadAllText(RandomGrowthSafeProjectionContract.PoolJsonPath);
        Assert.That(RandomGrowthSafeProjectionContract.ValidateProductionV2Json(
            eventJson.Replace("\"schemaVersion\": 2", "\"schemaVersion\": 1"), poolJson).IsValid, Is.False);
        Assert.That(RandomGrowthSafeProjectionContract.ValidateProductionV2Json(
            eventJson.Replace("chapter1-random-growth-safe-content.v2",
                "chapter1-random-growth-safe-content.v1"), poolJson).IsValid, Is.False);
        Assert.That(RandomGrowthSafeProjectionContract.ValidateProductionV2Json(
            eventJson.Replace("presentation.catalog.act1.random_growth.02.windworn_sword_marks.ko-KR",
                "presentation.catalog.wrong"), poolJson).IsValid, Is.False);
        Assert.That(RandomGrowthSafeProjectionContract.ValidateProductionV2Json(
            eventJson.Replace("\"name\": \"busyStatus\"", "\"name\": \"busyStatusMissing\""),
            poolJson).IsValid, Is.False);
    }

    [Test]
    public void PartialOutputGuardRetainsConcreteErrorCodeForOneOrTwoAssets()
    {
        string source = File.ReadAllText(
            "Assets/Editor/tools/stage/RandomGrowthGeneratedAssetBuilder.cs");
        Assert.That(source, Does.Contain("if (!exists.All(x => x))"));
        Assert.That(source, Does.Contain("PARTIAL_OUTPUT_SET"));
    }

    [Test]
    public void CleanAndUnrelatedOrdinaryPoolFixturesPassWithoutFalsePositive()
    {
        Assert.That(RandomGrowthGeneratedAssetBuilder.EvaluateOrdinaryPoolLinks(
            System.Array.Empty<string>(), System.Array.Empty<string>()), Is.Empty);
        Assert.That(RandomGrowthGeneratedAssetBuilder.EvaluateOrdinaryPoolLinks(
            new[] { "guid.ordinary.a", "guid.ordinary.b" },
            new[] { "pool.ordinary.a", "pool.ordinary.b" }), Is.Empty);
        Assert.That(RandomGrowthGeneratedAssetBuilder.EvaluateOrdinaryPoolLinks(
            new[] { RandomGrowthGeneratedAssetBuilder.ManifestOnlyPoolGuid },
            new[] { RandomGrowthGeneratedAssetBuilder.ManifestOnlyPoolId }),
            Does.Contain(RandomGrowthGeneratedAssetBuilder.ManifestOnlyOrdinaryLinkError + ":cardinality=1"));
    }

    [Test]
    public void ImageReferenceUsesApprovedGuidAndSpriteFileId()
    {
        var result = RandomGrowthGeneratedAssetBuilder.Preflight();
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(result.Blueprint.ImagePath);
        Assert.That(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sprite, out string guid, out long localId), Is.True);
        Assert.That(guid, Is.EqualTo(RandomGrowthGeneratedAssetBuilder.ExpectedImageGuid));
        Assert.That(localId, Is.EqualTo(RandomGrowthGeneratedAssetBuilder.ExpectedSpriteLocalId));
    }

    [Test]
    public void PreflightGenerateAndNoOpPathContainPlacementSnapshotGuard()
    {
        string source = File.ReadAllText(
            "Assets/Editor/tools/stage/RandomGrowthGeneratedAssetBuilder.cs");
        Assert.That(source, Does.Contain("CaptureOrdinaryPoolLinkSnapshot"));
        Assert.That(source, Does.Contain("ORDINARY_PLACEMENT_RULE_MUTATED"));
        Assert.That(source, Does.Contain("ValidateOrdinaryPoolLinks"));
    }

    [Test]
    public void RiskValidatorRejectsReservationMutationBeforeAtomicTransaction()
    {
        var risk = ValidRisk();
        risk.interactionReservation.mutationCountBeforeAtomicTransaction = 1;
        ChoiceExecutionConfig config = new()
        {
            executionType = ChoiceExecutionType.RandomGrowthRisk,
            data = risk
        };
        Assert.That(string.Join("\n", ChoiceExecutionConfigValidator.Validate(config)),
            Does.Contain("RESERVATION_CONTRACT"));
    }

    [Test]
    public void SerializeReferenceRoundTripPreservesTypedRiskPayload()
    {
        var holder = ScriptableObject.CreateInstance<RandomGrowthExecutionHolder>();
        holder.config = new ChoiceExecutionConfig
        {
            executionType = ChoiceExecutionType.RandomGrowthRisk,
            data = ValidRisk()
        };
        string json = EditorJsonUtility.ToJson(holder);
        var clone = ScriptableObject.CreateInstance<RandomGrowthExecutionHolder>();
        EditorJsonUtility.FromJsonOverwrite(json, clone);
        Assert.That(clone.config.data, Is.TypeOf<RandomGrowthRiskExecutionData>());
        Assert.That(((RandomGrowthRiskExecutionData)clone.config.data).definitionFingerprint,
            Is.EqualTo(RandomGrowthContentContractValidator.ValidateFiles().DefinitionFingerprint));
        Object.DestroyImmediate(holder);
        Object.DestroyImmediate(clone);
    }

    [Test]
    public void SafeBlueprintOwnsExactTypedProjectionWithoutSmithyHardcoding()
    {
        RandomGrowthSafeContentValidation source =
            RandomGrowthSafeProjectionContract.ValidateProductionV2Files();
        RandomGrowthProjectionBlueprint b =
            RandomGrowthGeneratedAssetBuilder.CreateSafeBlueprint(source);
        Assert.That(b.Kind, Is.EqualTo(RandomGrowthGeneratedContentKind.SafeObservation));
        Assert.That(b.Paths.RoundNodeAssetPath, Is.EqualTo(
            "Assets/Contents/Stage/so/stage.act1.random_growth.02.windworn_sword_marks.asset"));
        Assert.That(b.Paths.PopupEventAssetPath, Is.EqualTo(
            "Assets/Contents/Stage/so/node.act1.random_growth.02.windworn_sword_marks.intro.asset"));
        Assert.That(b.Paths.EventPoolAssetPath, Is.EqualTo(
            "Assets/Contents/Stage/so/event_pool.act1.random_growth.safe.asset"));
        Assert.That(b.ImageGuid, Is.EqualTo(RandomGrowthGeneratedAssetBuilder.SafeExpectedImageGuid));
        Assert.That(b.ProjectedChoices.Count, Is.EqualTo(2));
        Assert.That(b.ProjectedChoices[0].executionConfig.executionType,
            Is.EqualTo(ChoiceExecutionType.RandomGrowthSafe));
        Assert.That(b.ProjectedChoices[0].executionConfig.data,
            Is.TypeOf<RandomGrowthSafeExecutionData>());
        Assert.That(b.ProjectedChoices[1].executionConfig.data,
            Is.TypeOf<RandomGrowthDeclineExecutionData>());
        var safe = (RandomGrowthSafeExecutionData)b.ProjectedChoices[0].executionConfig.data;
        Assert.That(safe.definitionFingerprint,
            Is.EqualTo("0de9e9ac1418ccdce75d0fc2826c919d26790eebbc5b841d69bc2e35814252bb"));
        Assert.That(safe.targetCount, Is.EqualTo(2));
        Assert.That(safe.capPolicy.optionalGranted, Is.EqualTo(1));
        Assert.That(safe.capPolicy.optionalApplied, Is.EqualTo(1));
        Assert.That(b.ProjectedChoices.All(x => x.rewards != null && x.rewards.Count == 0), Is.True);
        Assert.That(b.EntryId, Is.EqualTo(RandomGrowthSafeProjectionContract.StageId));
        Assert.That(b.EntryWeight, Is.EqualTo(1));
        Assert.That(b.EntryOneShot, Is.True);
    }

    [Test]
    public void SafeCanonicalGeneratedAssetsPreflightIsNoOpAndWritesNothing()
    {
        const string roundNodeGuid = "0957a0dd60b77418e95024c5e1b1bd99";
        const string popupGuid = "8c9c53be506cb43ea9ce072d0bb3c0d0";
        const string poolGuid = "494e48d357bd949e5adb3cf03070b9f7";
        string[] assetPaths =
        {
            "Assets/Contents/Stage/so/stage.act1.random_growth.02.windworn_sword_marks.asset",
            "Assets/Contents/Stage/so/node.act1.random_growth.02.windworn_sword_marks.intro.asset",
            "Assets/Contents/Stage/so/event_pool.act1.random_growth.safe.asset"
        };
        string[] paths = assetPaths.Concat(assetPaths.Select(x => x + ".meta")).ToArray();
        string[] expectedSha =
        {
            "07d09c2e52447a002c4a2f11cd372b14a6d4073f135ff15259c8a83cca1cc6dc",
            "1a600c2c3eadf50f659bf2c54c348c0badbd7bb883dee08bba12f11e855a51ee",
            "c9d33a167a38c517fdb5c9ea242dcab067d4cb31215b46e7acbf48488fcc1c70",
            "5a057dcc2c317f568f7777832294a2b868d9f7a78b993b0271d81e367b009fd2",
            "49989d3c68ff6599d9449963209dc4005651a97d0726ff9ffe5e2f7fbc46150c",
            "913ae2e46aa04f8635ee3bde4070eebaa67897a3acb77e3bda2951f6dbbfc611"
        };
        Assert.That(paths.All(File.Exists), Is.True);
        for (int i = 0; i < paths.Length; i++)
            Assert.That(FileSha256(paths[i]), Is.EqualTo(expectedSha[i]), paths[i]);

        RoundNodeSO roundNode = AssetDatabase.LoadAssetAtPath<RoundNodeSO>(assetPaths[0]);
        PopupEventSO popup = AssetDatabase.LoadAssetAtPath<PopupEventSO>(assetPaths[1]);
        EventPoolSO pool = AssetDatabase.LoadAssetAtPath<EventPoolSO>(assetPaths[2]);
        Assert.That(roundNode, Is.Not.Null);
        Assert.That(popup, Is.Not.Null);
        Assert.That(pool, Is.Not.Null);
        Assert.That(AssetDatabase.AssetPathToGUID(assetPaths[0]), Is.EqualTo(roundNodeGuid));
        Assert.That(AssetDatabase.AssetPathToGUID(assetPaths[1]), Is.EqualTo(popupGuid));
        Assert.That(AssetDatabase.AssetPathToGUID(assetPaths[2]), Is.EqualTo(poolGuid));
        Assert.That(roundNode.nodeId, Is.EqualTo(RandomGrowthSafeProjectionContract.StageId));
        Assert.That(roundNode.popupEvent, Is.SameAs(popup));
        Assert.That(popup.eventId, Is.EqualTo(RandomGrowthSafeProjectionContract.NodeId));
        Assert.That(popup.choices.Count, Is.EqualTo(2));
        Assert.That(popup.choices[0].executionConfig.executionType,
            Is.EqualTo(ChoiceExecutionType.RandomGrowthSafe));
        Assert.That(popup.choices[1].executionConfig.executionType,
            Is.EqualTo(ChoiceExecutionType.RandomGrowthDecline));
        Assert.That(popup.choices.All(x => x.rewards != null && x.rewards.Count == 0), Is.True);
        var safe = popup.choices[0].executionConfig.data as RandomGrowthSafeExecutionData;
        Assert.That(safe, Is.Not.Null);
        Assert.That(safe.definitionFingerprint,
            Is.EqualTo(RandomGrowthSafeProjectionContract.V2DefinitionFingerprint));
        Assert.That(safe.contentContractVersion,
            Is.EqualTo(RandomGrowthSafeProjectionContract.V2ContentContractVersion));
        Assert.That(safe.presentationCatalogId,
            Is.EqualTo(RandomGrowthSafeProjectionContract.V2CatalogId));
        Assert.That(safe.presentationProjectionKind,
            Is.EqualTo(RandomGrowthSafeProjectionContract.V2ProjectionKind));
        Assert.That(safe.targetCount, Is.EqualTo(2));
        Assert.That(safe.capPolicy.fixedApplied, Is.EqualTo(2));
        Assert.That(safe.capPolicy.optionalGranted, Is.EqualTo(1));
        Assert.That(safe.capPolicy.optionalApplied, Is.EqualTo(1));
        Assert.That(safe.capPolicy.totalApplied, Is.EqualTo(3));
        Assert.That(pool.poolId, Is.EqualTo(RandomGrowthSafeProjectionContract.PoolId));
        Assert.That(pool.entries.Count, Is.EqualTo(1));
        Assert.That(pool.entries[0].node, Is.SameAs(roundNode));
        Assert.That(pool.entries[0].weight, Is.EqualTo(1));
        Assert.That(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(
            popup.mainImage, out string imageGuid, out long imageLocalId), Is.True);
        Assert.That(imageGuid, Is.EqualTo(RandomGrowthGeneratedAssetBuilder.SafeExpectedImageGuid));
        Assert.That(imageLocalId, Is.EqualTo(RandomGrowthGeneratedAssetBuilder.ExpectedSpriteLocalId));

        byte[][] before = paths.Select(File.ReadAllBytes).ToArray();
        bool[] dirtyBefore = { EditorUtility.IsDirty(roundNode), EditorUtility.IsDirty(popup), EditorUtility.IsDirty(pool) };
        RandomGrowthPresentationCatalogPreflight result =
            RandomGrowthGeneratedAssetBuilder.PreflightSafeV2Production();
        Assert.That(result.Disposition, Is.EqualTo(RandomGrowthGenerationDisposition.NoOp));
        Assert.That(result.Errors, Is.Empty);
        Assert.That(result.CanWrite, Is.False);
        for (int i = 0; i < paths.Length; i++)
            CollectionAssert.AreEqual(before[i], File.ReadAllBytes(paths[i]), paths[i]);
        Assert.That(AssetDatabase.AssetPathToGUID(assetPaths[0]), Is.EqualTo(roundNodeGuid));
        Assert.That(AssetDatabase.AssetPathToGUID(assetPaths[1]), Is.EqualTo(popupGuid));
        Assert.That(AssetDatabase.AssetPathToGUID(assetPaths[2]), Is.EqualTo(poolGuid));
        CollectionAssert.AreEqual(dirtyBefore,
            new[] { EditorUtility.IsDirty(roundNode), EditorUtility.IsDirty(popup), EditorUtility.IsDirty(pool) });
    }

    [Test]
    public void SafeOutputSetSyntheticStatesRemainNewOnlyAndFailClosed()
    {
        Assert.That(RandomGrowthGeneratedAssetBuilder.EvaluateOutputSet(false, false, false, false),
            Is.EqualTo(RandomGrowthGenerationDisposition.ReadyToGenerate));
        Assert.That(RandomGrowthGeneratedAssetBuilder.EvaluateOutputSet(true, false, false, false),
            Is.EqualTo(RandomGrowthGenerationDisposition.Blocked));
        Assert.That(RandomGrowthGeneratedAssetBuilder.EvaluateOutputSet(true, true, false, false),
            Is.EqualTo(RandomGrowthGenerationDisposition.Blocked));
        Assert.That(RandomGrowthGeneratedAssetBuilder.EvaluateOutputSet(true, true, true, false),
            Is.EqualTo(RandomGrowthGenerationDisposition.Blocked));
        Assert.That(RandomGrowthGeneratedAssetBuilder.EvaluateOutputSet(true, true, true, true),
            Is.EqualTo(RandomGrowthGenerationDisposition.NoOp));
    }

    [Test]
    public void SerializeReferenceRoundTripPreservesTypedSafePayload()
    {
        RandomGrowthProjectionBlueprint b = RandomGrowthGeneratedAssetBuilder.CreateSafeBlueprint(
            RandomGrowthSafeProjectionContract.ValidateProductionV2Files());
        var holder = ScriptableObject.CreateInstance<RandomGrowthExecutionHolder>();
        holder.config = b.ProjectedChoices[0].executionConfig;
        string json = EditorJsonUtility.ToJson(holder);
        var clone = ScriptableObject.CreateInstance<RandomGrowthExecutionHolder>();
        EditorJsonUtility.FromJsonOverwrite(json, clone);
        Assert.That(clone.config.data, Is.TypeOf<RandomGrowthSafeExecutionData>());
        var safe = (RandomGrowthSafeExecutionData)clone.config.data;
        Assert.That(safe.definitionFingerprint, Is.EqualTo(b.DefinitionFingerprint));
        Assert.That(safe.targetCount, Is.EqualTo(2));
        Assert.That(safe.capPolicy.optionalGranted, Is.EqualTo(1));
        Assert.That(safe.capPolicy.optionalApplied, Is.EqualTo(1));
        Object.DestroyImmediate(holder);
        Object.DestroyImmediate(clone);
    }

    [Test]
    public void SafeManifestPoolIsAlsoRejectedFromOrdinaryWeightedPlacement()
    {
        Assert.That(RandomGrowthGeneratedAssetBuilder.EvaluateOrdinaryPoolLinks(
            System.Array.Empty<string>(),
            new[] { RandomGrowthGeneratedAssetBuilder.SafeManifestOnlyPoolId }),
            Does.Contain(RandomGrowthGeneratedAssetBuilder.ManifestOnlyOrdinaryLinkError
                + ":cardinality=1"));
    }

    [Test]
    public void SafeGenerationApiIsExplicitAndLegacyBuildersRemainUnreferenced()
    {
        string source = File.ReadAllText(
            "Assets/Editor/tools/stage/RandomGrowthGeneratedAssetBuilder.cs");
        Assert.That(source, Does.Contain("GenerateApprovedSafeAssets"));
        Assert.That(source, Does.Contain("PreflightSafe"));
        Assert.That(source, Does.Not.Contain("StageStringBuilder"));
        Assert.That(source, Does.Not.Contain("StagePoolGennerator"));
        Assert.That(source, Does.Not.Contain("stage_string.csv"));
        Assert.That(source, Does.Not.Contain("DeleteAsset("));
    }

    private static RandomGrowthRiskExecutionData ValidRisk()
    {
        var source = RandomGrowthContentContractValidator.ValidateFiles();
        RandomGrowthTypedExecution x = source.Content.nodes[0].choices[0].execution;
        return new RandomGrowthRiskExecutionData
        {
            schemaVersion = 1,
            contentContractVersion = x.contentContractVersion,
            definitionFingerprint = source.DefinitionFingerprint,
            presentationTextDigestKo = source.PresentationTextDigestKo,
            eventId = x.eventId,
            stageNodeId = x.stageNodeId,
            sourcePopupId = source.Content.nodeId,
            choiceId = x.choiceId,
            segmentId = x.segmentId,
            reservationId = x.reservationId,
            poolMode = x.poolMode,
            resultKind = x.resultKind,
            successResultKind = x.successResultKind,
            failureState = x.failureState,
            interactionReservation = new RandomGrowthInteractionReservationData
            {
                authority = x.interactionReservation.authority,
                lifetime = x.interactionReservation.lifetime,
                stableKeyFields = x.interactionReservation.stableKeyFields.ToList(),
                orderedStates = x.interactionReservation.orderedStates.ToList(),
                locksDecline = true,
                blocksDuplicateConfirm = true
            },
            costPolicy = new RandomGrowthCostProjectionData
            {
                type = x.costPolicy.type,
                rateBasisPoints = 1000,
                rounding = "Ceil",
                minimumRemainingHp = 1f
            },
            capPolicy = new RandomGrowthCapProjectionData
            {
                fixedApplied = 2,
                randomApplied = 1,
                totalApplied = 3
            },
            growthGrant = 1
        };
    }

    private static string FileSha256(string path)
    {
        using SHA256 sha = SHA256.Create();
        return string.Concat(sha.ComputeHash(File.ReadAllBytes(path))
            .Select(value => value.ToString("x2")));
    }

    private static void AssertSafeV2ProductionSnapshotUnchanged(
        SafeV2ProductionSnapshot before, SafeV2ProductionSnapshot after)
    {
        Assert.That(after.PopupSha, Is.EqualTo(before.PopupSha));
        Assert.That(after.PopupGuid, Is.EqualTo(before.PopupGuid));
        Assert.That(after.CatalogSha, Is.EqualTo(before.CatalogSha));
        Assert.That(after.CatalogGuid, Is.EqualTo(before.CatalogGuid));
        Assert.That(after.RoundNodeSha, Is.EqualTo(before.RoundNodeSha));
        Assert.That(after.EventPoolSha, Is.EqualTo(before.EventPoolSha));
        Assert.That(after.Placement, Is.EqualTo(before.Placement));
    }

}

public sealed class RandomGrowthExecutionHolder : ScriptableObject
{
    [SerializeReference] public ChoiceExecutionConfig config;
}
#endif
