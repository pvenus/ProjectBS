#if UNITY_EDITOR
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using NUnit.Framework;
using ResourceTools.Stage;
using Stage;
using UnityEditor;
using UnityEngine;

public sealed class RandomGrowthContentContractTests
{
    private string EventJson => File.ReadAllText(RandomGrowthContentContractValidator.EventJsonPath);
    private string PoolJson => File.ReadAllText(RandomGrowthContentContractValidator.PoolJsonPath);

    [Test]
    public void CanonicalFilesValidateWithGoldenDigests()
    {
        var result = RandomGrowthContentContractValidator.ValidateFiles();
        Assert.That(result.Errors, Is.Empty);
        Assert.That(result.PresentationTextDigestKo, Is.EqualTo("13af056089beed858ed4856f604a45326875846df41332f8db64cfddbe59ea4d"));
        Assert.That(result.DefinitionFingerprint, Is.EqualTo("7f2d9d9f68e6294be22720f3277de595cfb22db6f4e8150890f7af5946ac2ce0"));
        Assert.That(result.BuildPlan.PresentationTextDigestKo, Is.EqualTo(result.PresentationTextDigestKo));
    }

    [Test]
    public void JsonRoundTripPreservesTypedContractAndFingerprint()
    {
        var first = RandomGrowthContentContractValidator.ValidateFiles();
        string eventRoundTrip = JsonUtility.ToJson(first.Content);
        string poolRoundTrip = JsonUtility.ToJson(first.Pool);
        var second = RandomGrowthContentContractValidator.ValidateJson(eventRoundTrip, poolRoundTrip);
        Assert.That(second.Errors, Is.Empty);
        Assert.That(second.DefinitionFingerprint, Is.EqualTo(first.DefinitionFingerprint));
        Assert.That(second.Content.nodes[0].choices.Select(x => x.execution.resultKind), Is.EqualTo(new[] { "RiskSelected", "Declined" }));
    }

    [Test]
    public void AliasOrChoiceOrderFailsClosed()
    {
        AssertInvalid(EventJson.Replace(RandomGrowthContentContractValidator.EventId, "event.alias", System.StringComparison.Ordinal), PoolJson, "EXACT_ID_ALLOW_LIST");
        string swapped = EventJson.Replace("\"RandomGrowthRisk\"", "\"TEMP\"")
            .Replace("\"RandomGrowthDecline\"", "\"RandomGrowthRisk\"").Replace("\"TEMP\"", "\"RandomGrowthDecline\"");
        Assert.That(RandomGrowthContentContractValidator.ValidateJson(swapped, PoolJson).IsValid, Is.False);
    }

    [Test]
    public void LegacyRewardOrTerminalRiskFailsClosed()
    {
        AssertInvalid(EventJson.Replace("\"rewards\": []", "\"rewards\": [\"legacy\"]", System.StringComparison.Ordinal), PoolJson, "RISK_REWARDS_EMPTY");
        AssertInvalid(EventJson.Replace("\"isTerminal\": false", "\"isTerminal\": true", System.StringComparison.Ordinal), PoolJson, "RISK_RESERVATION_STATE");
    }

    [Test]
    public void ReservationAuthorityCannotBecomeResultLedger()
    {
        AssertInvalid(EventJson.Replace("\"authority\": \"StageSession\"", "\"authority\": \"StageEventResultLedger\"", System.StringComparison.Ordinal), PoolJson, "INTERACTION_RESERVATION");
    }

    [Test]
    public void CopyOrPoolProbabilityMutationFailsClosed()
    {
        AssertInvalid(EventJson.Replace("우는 쇠종의 시련", "우는 쇠종의 시련 ", System.StringComparison.Ordinal), PoolJson, "CANONICAL_KO_COPY");
        AssertInvalid(EventJson, PoolJson.Replace("\"weight\": 1", "\"weight\": 40", System.StringComparison.Ordinal), "POOL_ENTRY");
    }

    [Test]
    public void ApprovedImageAndEvent16BaselinesAreImmutable()
    {
        Assert.That(Sha(RandomGrowthContentContractValidator.ImagePath), Is.EqualTo(RandomGrowthContentContractValidator.ImageSha256));
        Assert.That(Sha("Assets/Contents/Stage/json/event/act01/event.act1.16.crying_bell_smithy.json"), Is.EqualTo("1e6ef22cc7bfd31c79b859f3ad1f07dbc26e3d4778f916e2c829c54af6a771fa"));
        Assert.That(Guid("Assets/Contents/Stage/json/event/act01/event.act1.16.crying_bell_smithy.json.meta"), Is.EqualTo("38f3fd89dce734f76a2ed3fb9d137511"));
        Assert.That(Sha("Assets/Contents/Stage/so/stage.act1.random_event.16.crying_bell_smithy.asset"), Is.EqualTo("599bb288dd4290506b9d66ab160ebbae04a682e71bb359907610ad2a955fb59b"));
        Assert.That(Guid("Assets/Contents/Stage/so/stage.act1.random_event.16.crying_bell_smithy.asset.meta"), Is.EqualTo("99cfc79c663d64b0a8992944f0f75405"));
        Assert.That(Sha("Assets/Contents/Stage/so/node.act1.random_event.16.crying_bell_smithy.intro.asset"), Is.EqualTo("b72bfc5252d3a6ae1fd4015c0120bdbfdd6ea54cd9a262eb40d71acf5aced9ee"));
        Assert.That(Guid("Assets/Contents/Stage/so/node.act1.random_event.16.crying_bell_smithy.intro.asset.meta"), Is.EqualTo("2ba37c39d0b5c40fdb983bacbab87b50"));
    }

    [Test]
    public void BuildPlanValidationDoesNotMutateGeneratedAssets()
    {
        string roundPath = $"Assets/Contents/Stage/so/{RandomGrowthContentContractValidator.StageId}.asset";
        string popupPath = $"Assets/Contents/Stage/so/{RandomGrowthContentContractValidator.NodeId}.asset";
        string poolPath = $"Assets/Contents/Stage/so/{RandomGrowthContentContractValidator.PoolId}.asset";
        string[] paths = { roundPath, popupPath, poolPath };
        string[] shaBefore = paths.Select(Sha).ToArray();
        string[] guidBefore = paths.Select(x => AssetDatabase.AssetPathToGUID(x)).ToArray();
        string semanticBefore = GeneratedSemanticSnapshot(roundPath, popupPath, poolPath);

        var result = RandomGrowthContentContractValidator.ValidateFiles();
        Assert.That(result.BuildPlan.RoundNodeAssetPath, Is.EqualTo(roundPath));
        Assert.That(result.BuildPlan.PopupEventAssetPath, Is.EqualTo(popupPath));
        Assert.That(result.BuildPlan.EventPoolAssetPath, Is.EqualTo(poolPath));
        Assert.That(paths.Select(Sha), Is.EqualTo(shaBefore));
        Assert.That(paths.Select(x => AssetDatabase.AssetPathToGUID(x)), Is.EqualTo(guidBefore));
        Assert.That(GeneratedSemanticSnapshot(roundPath, popupPath, poolPath), Is.EqualTo(semanticBefore));
        Assert.That(result.Content.nodes[0].choices.All(x =>
            x.rewards != null && x.rewards.Count == 0
            && x.execution.rewards != null && x.execution.rewards.Count == 0), Is.True);
    }

    private static string GeneratedSemanticSnapshot(
        string roundPath,
        string popupPath,
        string poolPath)
    {
        RoundNodeSO round = AssetDatabase.LoadAssetAtPath<RoundNodeSO>(roundPath);
        PopupEventSO popup = AssetDatabase.LoadAssetAtPath<PopupEventSO>(popupPath);
        EventPoolSO pool = AssetDatabase.LoadAssetAtPath<EventPoolSO>(poolPath);
        var risk = popup.choices[0].executionConfig.data as RandomGrowthRiskExecutionData;
        var decline = popup.choices[1].executionConfig.data as RandomGrowthDeclineExecutionData;
        return string.Join("|", new[]
        {
            round.nodeId,
            round.nodeType.ToString(),
            AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(round.popupEvent)),
            popup.eventId,
            popup.choices.Count.ToString(),
            popup.choices[0].choiceId,
            popup.choices[0].rewards.Count.ToString(),
            risk?.definitionFingerprint,
            risk?.resultKind,
            risk?.costPolicy?.rateBasisPoints.ToString(),
            risk?.capPolicy?.fixedApplied.ToString(),
            risk?.capPolicy?.randomApplied.ToString(),
            risk?.capPolicy?.totalApplied.ToString(),
            popup.choices[1].choiceId,
            popup.choices[1].rewards.Count.ToString(),
            decline?.resultKind,
            pool.poolId,
            pool.entries.Count.ToString(),
            pool.entries[0].entryId,
            pool.entries[0].weight.ToString(),
            AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(pool.entries[0].node))
        });
    }

    private static void AssertInvalid(string eventJson, string poolJson, string error)
    { Assert.That(RandomGrowthContentContractValidator.ValidateJson(eventJson, poolJson).Errors, Does.Contain(error)); }

    private static string Sha(string path)
    {
        using var sha = SHA256.Create(); using var stream = File.OpenRead(path);
        return string.Concat(sha.ComputeHash(stream).Select(x => x.ToString("x2")));
    }

    private static string Guid(string path) => File.ReadLines(path).First(x => x.StartsWith("guid: ")).Substring(6);
}
#endif
