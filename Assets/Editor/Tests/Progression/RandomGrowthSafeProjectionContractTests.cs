#if UNITY_EDITOR
using System.Globalization;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ResourceTools.Stage;
using Stage;
using UnityEngine;

public sealed class RandomGrowthSafeProjectionContractTests
{
    private static string CopyDigest => RandomGrowthSafeProjectionContract.SemanticCopyDigest;

    [Test]
    public void StableEnumAndFactoryMapSafeWithoutChangingRiskOrDecline()
    {
        Assert.That((int)ChoiceExecutionType.RandomGrowthRisk, Is.EqualTo(1000));
        Assert.That((int)ChoiceExecutionType.RandomGrowthDecline, Is.EqualTo(1010));
        Assert.That((int)ChoiceExecutionType.RandomGrowthSafe, Is.EqualTo(1020));
        Assert.That(ChoiceExecutionDataFactory.Create(ChoiceExecutionType.RandomGrowthSafe),
            Is.TypeOf<RandomGrowthSafeExecutionData>());
        Assert.That(ChoiceExecutionDataFactory.Create(ChoiceExecutionType.RandomGrowthRisk),
            Is.TypeOf<RandomGrowthRiskExecutionData>());
        Assert.That(ChoiceExecutionDataFactory.Create(ChoiceExecutionType.Battle),
            Is.Not.TypeOf<RandomGrowthSafeExecutionData>());
    }

    [Test]
    public void SnapshotUsesCanonicalSafeIdentityAndZeroCostTargetTwo()
    {
        RandomGrowthSafeProjectionSnapshot x = RandomGrowthSafeProjectionContract.CreateSnapshot(CopyDigest);
        var safe = (RandomGrowthSafeExecutionData)x.Safe.data;
        var decline = (RandomGrowthDeclineExecutionData)x.Decline.data;
        Assert.That(safe.eventId, Is.EqualTo("event.act1.random_growth.02.windworn_sword_marks"));
        Assert.That(safe.sourcePopupId, Is.EqualTo("node.act1.random_growth.02.windworn_sword_marks.intro"));
        Assert.That(safe.reservationId, Is.EqualTo("reservation.act1.chapter01.random_growth.after_episode02"));
        Assert.That(RandomGrowthSafeProjectionContract.LeftSectionId, Is.EqualTo("sec_ep_2_to_ep_3_1"));
        Assert.That(RandomGrowthSafeProjectionContract.RightSectionId, Is.EqualTo("sec_ep_2_to_ep_3_2"));
        Assert.That(RandomGrowthSafeProjectionContract.LeftSlotId, Is.EqualTo("slot_430_2085"));
        Assert.That(RandomGrowthSafeProjectionContract.RightSlotId, Is.EqualTo("slot_1370_2085"));
        Assert.That(safe.poolMode, Is.EqualTo("PartyWide"));
        Assert.That(safe.targetCount, Is.EqualTo(2));
        Assert.That(safe.cost, Is.Zero);
        Assert.That(safe.growthGrant, Is.EqualTo(1));
        Assert.That(safe.resultKind, Is.EqualTo("ObserveSelected"));
        Assert.That(safe.successResultKind, Is.EqualTo("SafeGrowthGranted"));
        Assert.That(safe.failureState, Is.EqualTo("ObserveSelectedPendingRetry"));
        Assert.That(safe.capPolicy.fixedApplied, Is.EqualTo(2));
        Assert.That(safe.capPolicy.optionalGranted, Is.EqualTo(1));
        Assert.That(safe.capPolicy.optionalApplied, Is.EqualTo(1));
        Assert.That(safe.capPolicy.totalApplied, Is.EqualTo(3));
        Assert.That(x.SafeRewards, Is.Empty);
        Assert.That(x.DeclineRewards, Is.Empty);
        Assert.That(decline.cost, Is.Zero);
        Assert.That(decline.growthGrant, Is.Zero);
        Assert.That(decline.resultKind, Is.EqualTo("Declined"));
        Assert.That(ChoiceExecutionConfigValidator.Validate(x.Safe), Is.Empty);
        Assert.That(ChoiceExecutionConfigValidator.Validate(x.Decline), Is.Empty);
    }

    [Test]
    public void CandidateZeroBlocksExecutorAndAllowsManifestFallback()
    {
        var safe = (RandomGrowthSafeExecutionData)RandomGrowthSafeProjectionContract.CreateSnapshot(CopyDigest).Safe.data;
        Assert.That(safe.candidateUnavailableState, Is.EqualTo("CandidateUnavailable"));
        Assert.That(safe.candidateZeroBlocksExecutor, Is.True);
        Assert.That(safe.candidateZeroAllowsFallback, Is.True);
        Assert.That(safe.mutationCountBeforeConfirm, Is.Zero);
    }

    [Test]
    public void FingerprintIsCultureStableAndSeparatedFromSmithy()
    {
        CultureInfo before = CultureInfo.CurrentCulture;
        string a = RandomGrowthSafeProjectionContract.ComputeFingerprint(CopyDigest);
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
            Assert.That(RandomGrowthSafeProjectionContract.ComputeFingerprint(CopyDigest), Is.EqualTo(a));
        }
        finally { CultureInfo.CurrentCulture = before; }
        Assert.That(a, Has.Length.EqualTo(64));
        Assert.That(CopyDigest, Is.EqualTo("a5d02f07c900e11c29887811197dd8183c269162abaec5219a0551bdec19ac35"));
        Assert.That(a, Is.EqualTo("0de9e9ac1418ccdce75d0fc2826c919d26790eebbc5b841d69bc2e35814252bb"));
        Assert.That(a, Is.Not.EqualTo(RandomGrowthContentContractValidator.ValidateFiles().DefinitionFingerprint));
    }

    [Test]
    public void TypedSafeDataRoundTripsWithoutFingerprintOrTargetLoss()
    {
        var source = (RandomGrowthSafeExecutionData)RandomGrowthSafeProjectionContract.CreateSnapshot(CopyDigest).Safe.data;
        string json = JsonUtility.ToJson(source);
        RandomGrowthSafeExecutionData restored = JsonUtility.FromJson<RandomGrowthSafeExecutionData>(json);
        Assert.That(restored.definitionFingerprint, Is.EqualTo(source.definitionFingerprint));
        Assert.That(restored.choiceId, Is.EqualTo(source.choiceId));
        Assert.That(restored.targetCount, Is.EqualTo(2));
        Assert.That(restored.capPolicy.optionalGranted, Is.EqualTo(1));
        Assert.That(restored.capPolicy.optionalApplied, Is.EqualTo(1));
        Assert.That(restored.successResultKind, Is.EqualTo("SafeGrowthGranted"));
    }

    [Test]
    public void BuildPlanHasExactThreeDistinctSafeOutputs()
    {
        RandomGrowthSafeBuildPlan x = RandomGrowthSafeProjectionContract.CreateSnapshot(CopyDigest).Paths;
        Assert.That(x.RoundNodeAssetPath, Is.EqualTo("Assets/Contents/Stage/so/stage.act1.random_growth.02.windworn_sword_marks.asset"));
        Assert.That(x.PopupEventAssetPath, Is.EqualTo("Assets/Contents/Stage/so/node.act1.random_growth.02.windworn_sword_marks.intro.asset"));
        Assert.That(x.EventPoolAssetPath, Is.EqualTo("Assets/Contents/Stage/so/event_pool.act1.random_growth.safe.asset"));
        Assert.That(x.RoundNodeAssetPath, Is.Not.EqualTo(x.PopupEventAssetPath));
        Assert.That(x.PopupEventAssetPath, Is.Not.EqualTo(x.EventPoolAssetPath));
    }

    [TestCase(false, false, false, false, RandomGrowthSafePreflightDisposition.ReadyToGenerate)]
    [TestCase(true, false, false, false, RandomGrowthSafePreflightDisposition.Blocked)]
    [TestCase(true, true, false, false, RandomGrowthSafePreflightDisposition.Blocked)]
    [TestCase(true, true, true, false, RandomGrowthSafePreflightDisposition.Blocked)]
    [TestCase(true, true, true, true, RandomGrowthSafePreflightDisposition.NoOp)]
    public void PreflightIsNewOnlyPartialAndMismatchFailClosed(bool popup, bool node, bool pool,
        bool semanticMatch, RandomGrowthSafePreflightDisposition expected)
    {
        Assert.That(RandomGrowthSafeProjectionContract.Preflight(CopyDigest,
            popup, node, pool, semanticMatch).Disposition,
            Is.EqualTo(expected));
    }

    [Test]
    public void StableIdCollisionBlocksBeforeAnyWriteAuthority()
    {
        RandomGrowthSafePreflight x = RandomGrowthSafeProjectionContract.Preflight(CopyDigest,
            false, false, false, false,
            new[] { RandomGrowthSafeProjectionContract.EventId });
        Assert.That(x.Disposition, Is.EqualTo(RandomGrowthSafePreflightDisposition.Blocked));
        Assert.That(x.Errors, Does.Contain("SAFE_STABLE_ID_COLLISION:" + RandomGrowthSafeProjectionContract.EventId));
        Assert.That(x.CanWrite, Is.False);
    }

    [Test]
    public void ContractContainsNoAssetDatabaseWriteOrLegacyBuilderReference()
    {
        string source = File.ReadAllText("Assets/Editor/tools/stage/RandomGrowthSafeProjectionContract.cs");
        Assert.That(source, Does.Not.Contain("CreateAsset("));
        Assert.That(source, Does.Not.Contain("SaveAssets("));
        Assert.That(source, Does.Not.Contain("StageStringBuilder"));
        Assert.That(source, Does.Not.Contain("stage_string.csv"));
    }

    [Test]
    public void PreflightDoesNotMutateSharedCsvWeightedRuleOrSmithyAssets()
    {
        string[] paths =
        {
            "Assets/Resources/string/stage_string.csv",
            "Assets/Contents/Stage/placement/rules/WeightedPoolPlacementRule.asset",
            "Assets/Contents/Stage/so/stage.act1.random_growth.01.crying_bell_smithy_trial.asset",
            "Assets/Contents/Stage/so/node.act1.random_growth.01.crying_bell_smithy_trial.intro.asset",
            "Assets/Contents/Stage/so/event_pool.act1.random_growth.cheongun_sangui.asset"
        };
        byte[][] before = paths.Select(File.ReadAllBytes).ToArray();
        RandomGrowthSafePreflight result = RandomGrowthSafeProjectionContract.Preflight(
            CopyDigest, false, false, false, false);
        Assert.That(result.Disposition, Is.EqualTo(RandomGrowthSafePreflightDisposition.ReadyToGenerate));
        for (int i = 0; i < paths.Length; i++)
            CollectionAssert.AreEqual(before[i], File.ReadAllBytes(paths[i]), paths[i]);
    }


    [Test]
    public void MissingOrNonCanonicalCopyDigestBlocksBeforeSnapshotOrWrite()
    {
        RandomGrowthSafePreflight missing = RandomGrowthSafeProjectionContract.Preflight(
            null, false, false, false, false);
        RandomGrowthSafePreflight uppercase = RandomGrowthSafeProjectionContract.Preflight(
            new string('A', 64), false, false, false, false);
        RandomGrowthSafePreflight obsolete = RandomGrowthSafeProjectionContract.Preflight(
            new string('a', 64), false, false, false, false);
        Assert.That(missing.Disposition, Is.EqualTo(RandomGrowthSafePreflightDisposition.Blocked));
        Assert.That(missing.Snapshot, Is.Null);
        Assert.That(missing.Errors, Does.Contain("SAFE_SEMANTIC_COPY_DIGEST_INVALID"));
        Assert.That(uppercase.Disposition, Is.EqualTo(RandomGrowthSafePreflightDisposition.Blocked));
        Assert.That(obsolete.Disposition, Is.EqualTo(RandomGrowthSafePreflightDisposition.Blocked));
    }

    [Test]
    public void CanonicalSemanticCopyHasExactOrdered28FieldsAndFinalCorrections()
    {
        var fields = RandomGrowthSafeProjectionContract.GetSemanticCopyFields();
        Assert.That(fields, Has.Count.EqualTo(28));
        Assert.That(fields[0].Key, Is.EqualTo("discoveryTitle"));
        Assert.That(fields[1].Value, Does.Contain("한 걸음 물러서자"));
        Assert.That(fields[4].Value, Is.EqualTo("전투와 비용 없이 흔적을 관찰합니다."));
        Assert.That(fields[5].Value, Is.EqualTo("성장 정비 · 무작위 후보 최대 2개"));
        Assert.That(fields[7].Value, Is.EqualTo("비용 없이 관찰을 마치면 성장 정비가 열립니다."));
        Assert.That(fields[9].Value, Is.EqualTo("성장 없음 · 이번 장의 추가 성장은 아직 가능합니다."));
        Assert.That(fields[11].Value, Does.Contain("무작위 후보는 최대 2개이며"));
        Assert.That(fields[15].Value, Is.EqualTo("강화 가능한 스킬이 생긴 뒤 다시 확인할 수 있습니다."));
        Assert.That(fields[22].Value, Is.EqualTo("성장 없음 · 이번 장의 추가 성장은 아직 가능합니다."));
        Assert.That(fields[24].Value, Does.Contain("추가 성장도 소비되지 않았습니다."));
        Assert.That(fields[27].Value, Does.Contain("{ownerName}"));
        Assert.That(fields[27].Value, Does.Contain("{levelAfter}"));
    }

    [Test]
    public void V2CatalogBlueprintAppendsExactThreeFieldsAndGoldenTuple()
    {
        RandomGrowthSafePresentationCatalogBlueprint b =
            RandomGrowthSafeProjectionContract.CreateV2CatalogBlueprint();
        Assert.That(b.Fields, Has.Count.EqualTo(31));
        Assert.That(b.Fields.Take(28), Is.EqualTo(
            RandomGrowthSafeProjectionContract.GetSemanticCopyFields()));
        Assert.That(b.Fields[28].Key, Is.EqualTo("candidateTwoStatus"));
        Assert.That(b.Fields[29].Key, Is.EqualTo("candidateOneStatus"));
        Assert.That(b.Fields[30].Key, Is.EqualTo("busyStatus"));
        Assert.That(b.Expectation.SemanticCopyDigest,
            Is.EqualTo(RandomGrowthSafeProjectionContract.V2SemanticCopyDigest));
        Assert.That(b.Expectation.DefinitionFingerprint,
            Is.EqualTo(RandomGrowthSafeProjectionContract.V2DefinitionFingerprint));
        Assert.That(b.AssetPath, Does.StartWith("Assets/Resources/Stage/RandomGrowth/Presentation/"));
    }

    [Test]
    public void V2ExecutionProjectionCarriesExactCatalogIdentity()
    {
        RandomGrowthChoiceExecutionData data = (RandomGrowthChoiceExecutionData)
            RandomGrowthSafeProjectionContract.CreateV2ExecutionConfig(false).data;
        Assert.That(data.schemaVersion, Is.EqualTo(2));
        Assert.That(data.contentContractVersion,
            Is.EqualTo(RandomGrowthSafeProjectionContract.V2ContentContractVersion));
        Assert.That(data.presentationCatalogId,
            Is.EqualTo(RandomGrowthSafeProjectionContract.V2CatalogId));
        Assert.That(data.presentationProjectionKind,
            Is.EqualTo(RandomGrowthSafeProjectionContract.V2ProjectionKind));
        Assert.That(data.presentationLocale, Is.EqualTo("ko-KR"));
        Assert.That(ChoiceExecutionConfigValidator.Validate(
            RandomGrowthSafeProjectionContract.CreateV2ExecutionConfig(false)), Is.Empty);
    }

    [Test]
    public void V2DigestIsCultureAndOrderDeterministicAndV1Distinct()
    {
        string before = RandomGrowthSafeProjectionContract.ComputeV2SemanticCopyDigest();
        CultureInfo old = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
            Assert.That(RandomGrowthSafeProjectionContract.ComputeV2SemanticCopyDigest(),
                Is.EqualTo(before));
        }
        finally { CultureInfo.CurrentCulture = old; }
        Assert.That(before, Is.EqualTo(RandomGrowthSafeProjectionContract.V2SemanticCopyDigest));
        Assert.That(before, Is.Not.EqualTo(CopyDigest));
    }

    [Test]
    public void ProductionJsonRoundTripsCanonicalSafeContract()
    {
        RandomGrowthSafeContentValidation result = RandomGrowthSafeProjectionContract.ValidateProductionV2Files();
        Assert.That(result.Errors, Is.Empty);
        Assert.That(result.IsValid, Is.True);
        Assert.That(result.Content.schemaVersion, Is.EqualTo(2));
        Assert.That(result.Content.contentContractVersion,
            Is.EqualTo(RandomGrowthSafeProjectionContract.V2ContentContractVersion));
        Assert.That(result.Content.presentationCatalogId,
            Is.EqualTo(RandomGrowthSafeProjectionContract.V2CatalogId));
        Assert.That(result.Content.presentationProjectionKind,
            Is.EqualTo(RandomGrowthSafeProjectionContract.V2ProjectionKind));
        Assert.That(result.Content.semanticCopyDigest,
            Is.EqualTo(RandomGrowthSafeProjectionContract.V2SemanticCopyDigest));
        Assert.That(result.Content.definitionFingerprint,
            Is.EqualTo(RandomGrowthSafeProjectionContract.V2DefinitionFingerprint));
        Assert.That(result.Content.semanticCopyKo, Has.Count.EqualTo(31));

        string eventRoundTrip = JsonUtility.ToJson(result.Content);
        string poolRoundTrip = JsonUtility.ToJson(result.Pool);
        Assert.That(RandomGrowthSafeProjectionContract.ValidateProductionV2Json(
            eventRoundTrip, poolRoundTrip).Errors, Is.Empty);
    }

    [Test]
    public void ProductionJsonHasObserveThenDeclineAndNonNullEmptyRewards()
    {
        RandomGrowthSafeContentDocument content =
            RandomGrowthSafeProjectionContract.ValidateProductionV2Files().Content;
        Assert.That(content.choices.Select(x => x.choiceId), Is.EqualTo(new[]
        {
            RandomGrowthSafeProjectionContract.SafeChoiceId,
            RandomGrowthSafeProjectionContract.DeclineChoiceId
        }));
        Assert.That(content.choices[0].sourcePopupId, Is.EqualTo(content.sourcePopupId));
        Assert.That(content.choices[1].sourcePopupId, Is.EqualTo(content.sourcePopupId));
        Assert.That(content.choices[0].rewards, Is.Not.Null.And.Empty);
        Assert.That(content.choices[1].rewards, Is.Not.Null.And.Empty);
        Assert.That(content.choices[0].executionTypeValue, Is.EqualTo(1020));
        Assert.That(content.choices[0].cost, Is.Zero);
        Assert.That(content.choices[0].battleRequired, Is.False);
        Assert.That(content.choices[0].targetCount, Is.EqualTo(2));
        Assert.That(content.choices[1].isTerminal, Is.True);
        Assert.That(content.choices[1].cost, Is.Zero);
        Assert.That(content.choices[1].growthGrant, Is.Zero);
    }

    [Test]
    public void ProductionJsonRejectsObsoleteGoldenAndLegacyReward()
    {
        string eventJson = File.ReadAllText(RandomGrowthSafeProjectionContract.EventJsonPath);
        string poolJson = File.ReadAllText(RandomGrowthSafeProjectionContract.PoolJsonPath);
        string obsolete = eventJson.Replace(RandomGrowthSafeProjectionContract.V2SemanticCopyDigest,
            new string('a', 64));
        Assert.That(RandomGrowthSafeProjectionContract.ValidateProductionV2Json(obsolete, poolJson).Errors,
            Does.Contain("SAFE_V2_GOLDEN"));

        string v1 = eventJson.Replace("\"schemaVersion\": 2", "\"schemaVersion\": 1")
            .Replace(RandomGrowthSafeProjectionContract.V2ContentContractVersion,
                RandomGrowthSafeProjectionContract.ContentContractVersion);
        Assert.That(RandomGrowthSafeProjectionContract.ValidateProductionV2Json(v1, poolJson).IsValid,
            Is.False);
        string subset = eventJson.Replace(
            "\"name\": \"busyStatus\"", "\"name\": \"busyStatusMissing\"");
        Assert.That(RandomGrowthSafeProjectionContract.ValidateProductionV2Json(subset, poolJson).Errors,
            Does.Contain("SAFE_V2_CANONICAL_COPY31"));

        string reward = eventJson.Replace("\"rewards\": []", "\"rewards\": [\"legacy.immediate\"]");
        RandomGrowthSafeContentValidation invalid =
            RandomGrowthSafeProjectionContract.ValidateProductionV2Json(reward, poolJson);
        Assert.That(invalid.Errors.Any(x => x == "SAFE_OBSERVE_REWARDS"
            || x == "SAFE_DECLINE_REWARDS"), Is.True);
    }

    [Test]
    public void ProductionValidationDoesNotMutateExistingContentOrBuildOutputs()
    {
        string[] paths =
        {
            "Assets/Contents/Stage/json/event/act01/event.act1.06.shrine_eaves_empty_perch.json",
            RandomGrowthContentContractValidator.EventJsonPath,
            "Assets/Contents/Stage/json/event/act01/event.act1.16.crying_bell_smithy.json",
            "Assets/Contents/Stage/placement/rules/WeightedPoolPlacementRule.asset",
            "Assets/Contents/Stage/so/stage.act1.random_growth.01.crying_bell_smithy_trial.asset",
            "Assets/Contents/Stage/so/node.act1.random_growth.01.crying_bell_smithy_trial.intro.asset",
            "Assets/Contents/Stage/so/event_pool.act1.random_growth.cheongun_sangui.asset",
            "Assets/Resources/string/stage_string.csv"
        };
        byte[][] before = paths.Select(File.ReadAllBytes).ToArray();
        RandomGrowthSafeContentValidation result = RandomGrowthSafeProjectionContract.ValidateProductionV2Files();
        Assert.That(result.IsValid, Is.True);
        Assert.That(RandomGrowthSafeProjectionContract.CreateSnapshot(CopyDigest).Paths.RoundNodeAssetPath,
            Is.EqualTo("Assets/Contents/Stage/so/stage.act1.random_growth.02.windworn_sword_marks.asset"));
        for (int i = 0; i < paths.Length; i++)
            CollectionAssert.AreEqual(before[i], File.ReadAllBytes(paths[i]), paths[i]);
    }
}
#endif
