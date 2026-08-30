using System;
using NUnit.Framework;
using ResourceTools.Stage;
using Stage;
using UnityEditor;

public sealed class ConfirmableChoiceRouterTests
{
    private const string PopupPath =
        "Assets/Contents/Stage/so/node.act1.random_growth.02.windworn_sword_marks.intro.asset";

    [Test]
    public void ActualSafeObserveAndDeclineRequireConfirmation()
    {
        PopupEventSO popup = AssetDatabase.LoadAssetAtPath<PopupEventSO>(PopupPath);
        Assert.That(popup, Is.Not.Null);
        ChoiceExecutionRouter router = ChoiceExecutionRouter.CreateDefault();

        Assert.That(router.QueryConfirmable(popup.choices[0].executionConfig).Kind,
            Is.EqualTo(ConfirmableChoiceDispatchKind.RequiresConfirmation));
        Assert.That(router.QueryConfirmable(popup.choices[1].executionConfig).Kind,
            Is.EqualTo(ConfirmableChoiceDispatchKind.RequiresConfirmation));
    }

    [TestCase("sourcePopupId")]
    [TestCase("eventId")]
    [TestCase("choiceId")]
    [TestCase("definitionFingerprint")]
    [TestCase("contentContractVersion")]
    public void SafeIdentityMismatchIsDisabledWithoutMutation(string field)
    {
        ChoiceExecutionConfig config = CloneConfig(0);
        RandomGrowthChoiceExecutionData data = (RandomGrowthChoiceExecutionData)config.data;
        switch (field)
        {
            case "sourcePopupId": data.sourcePopupId += ".mismatch"; break;
            case "eventId": data.eventId += ".mismatch"; break;
            case "choiceId": data.choiceId += ".mismatch"; break;
            case "definitionFingerprint": data.definitionFingerprint = new string('0', 64); break;
            case "contentContractVersion": data.contentContractVersion += ".mismatch"; break;
        }

        ChoiceExecutionRouter router = ChoiceExecutionRouter.CreateDefault();
        ConfirmableChoiceDispatchResult result = router.QueryConfirmable(config);
        Assert.That(result.Kind, Is.EqualTo(ConfirmableChoiceDispatchKind.Disabled));
        Assert.That(result.DisabledReason, Is.EqualTo(ConfirmableChoiceDisabledReason.IdentityMismatch));
        Assert.That(router.TryExecute("safe", config, default, out _),
            Is.EqualTo(ChoiceExecutionResult.UnsupportedType));
    }

    [Test]
    public void SmithyAndUnmatchedDeclineRemainUnsupported()
    {
        ChoiceExecutionConfig smithyDecline = new()
        {
            executionType = ChoiceExecutionType.RandomGrowthDecline,
            data = new RandomGrowthDeclineExecutionData
            {
                schemaVersion = 1,
                contentContractVersion = "crying_bell_smithy_trial.v1",
                definitionFingerprint = new string('a', 64),
                presentationTextDigestKo = new string('b', 64),
                eventId = "event.act1.random_growth.01.crying_bell_smithy_trial",
                stageNodeId = "stage.act1.random_growth.01.crying_bell_smithy_trial",
                sourcePopupId = "node.act1.random_growth.01.crying_bell_smithy_trial.intro",
                choiceId = "choice.act1.random_growth.01.crying_bell_smithy_trial.leave_forge",
                segmentId = "progress.segment.act1.chapter01.random_before_episode06",
                reservationId = "reservation.act1.chapter01.random_growth.before_episode06",
                poolMode = "PartyWide", targetCount = 3,
                resultKind = "Declined", cost = 0, growthGrant = 0
            }
        };
        ChoiceExecutionRouter router = ChoiceExecutionRouter.CreateDefault();
        Assert.That(router.QueryConfirmable(smithyDecline).Kind,
            Is.EqualTo(ConfirmableChoiceDispatchKind.Unsupported));
        Assert.That(router.TryExecute("smithy", smithyDecline, default, out _),
            Is.EqualTo(ChoiceExecutionResult.UnsupportedType));
    }

    [Test]
    public void QueryStateIsImmutableAndDoesNotAffectLegacyHistory()
    {
        ChoiceExecutionConfig config = CloneConfig(0);
        ChoiceExecutionRouter router = ChoiceExecutionRouter.CreateDefault();
        ConfirmableChoiceDispatchResult pending = router.QueryConfirmable(
            config, ConfirmableChoiceRuntimeState.PendingRetry);
        ConfirmableChoiceDispatchResult terminal = router.QueryConfirmable(
            config, ConfirmableChoiceRuntimeState.Terminal);

        Assert.That(pending.Kind, Is.EqualTo(ConfirmableChoiceDispatchKind.PendingRetry));
        Assert.That(terminal.Kind, Is.EqualTo(ConfirmableChoiceDispatchKind.TerminalReplay));
        Assert.That(pending.ChoiceId, Is.EqualTo(ConfirmableChoiceContract.ObserveChoiceId));
        Assert.That(router.TryExecute("same", config, default, out _),
            Is.EqualTo(ChoiceExecutionResult.UnsupportedType));
        Assert.That(router.TryExecute("same", config, default, out _),
            Is.EqualTo(ChoiceExecutionResult.UnsupportedType));
    }

    [Test]
    public void QueryIsCultureIndependent()
    {
        ChoiceExecutionConfig config = CloneConfig(0);
        ChoiceExecutionRouter router = ChoiceExecutionRouter.CreateDefault();
        var before = System.Globalization.CultureInfo.CurrentCulture;
        try
        {
            foreach (string culture in new[] { "ko-KR", "tr-TR", "en-US" })
            {
                System.Globalization.CultureInfo.CurrentCulture =
                    System.Globalization.CultureInfo.GetCultureInfo(culture);
                Assert.That(router.QueryConfirmable(config).Kind,
                    Is.EqualTo(ConfirmableChoiceDispatchKind.RequiresConfirmation));
            }
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = before;
        }
    }

    [Test]
    public void SyntheticV2ObserveAndDeclineAreConfirmable()
    {
        ChoiceExecutionRouter router = ChoiceExecutionRouter.CreateDefault();
        Assert.That(router.QueryConfirmable(
            RandomGrowthSafeProjectionContract.CreateV2ExecutionConfig(false)).Kind,
            Is.EqualTo(ConfirmableChoiceDispatchKind.RequiresConfirmation));
        Assert.That(router.QueryConfirmable(
            RandomGrowthSafeProjectionContract.CreateV2ExecutionConfig(true)).Kind,
            Is.EqualTo(ConfirmableChoiceDispatchKind.RequiresConfirmation));
    }

    [Test]
    public void V1V2MixedCatalogIdentityIsDisabled()
    {
        ChoiceExecutionConfig v1 = RandomGrowthSafeProjectionContract.CreateSnapshot(
            RandomGrowthSafeProjectionContract.SemanticCopyDigest).Safe;
        ((RandomGrowthChoiceExecutionData)v1.data).presentationCatalogId =
            RandomGrowthSafeProjectionContract.V2CatalogId;
        Assert.That(ChoiceExecutionRouter.CreateDefault().QueryConfirmable(v1).Kind,
            Is.EqualTo(ConfirmableChoiceDispatchKind.Disabled));

        ChoiceExecutionConfig v2 = RandomGrowthSafeProjectionContract.CreateV2ExecutionConfig(false);
        ((RandomGrowthChoiceExecutionData)v2.data).contentContractVersion =
            ConfirmableChoiceContract.ContentContractVersion;
        Assert.That(ChoiceExecutionRouter.CreateDefault().QueryConfirmable(v2).Kind,
            Is.EqualTo(ConfirmableChoiceDispatchKind.Disabled));
    }

    [Test]
    public void V2CatalogIdentityMismatchIsDisabledWithoutLegacyExecution()
    {
        ChoiceExecutionConfig v2 = RandomGrowthSafeProjectionContract.CreateV2ExecutionConfig(false);
        ((RandomGrowthChoiceExecutionData)v2.data).presentationLocale = "en-US";
        ChoiceExecutionRouter router = ChoiceExecutionRouter.CreateDefault();
        Assert.That(router.QueryConfirmable(v2).Kind,
            Is.EqualTo(ConfirmableChoiceDispatchKind.Disabled));
        Assert.That(router.TryExecute("v2", v2, default, out _),
            Is.EqualTo(ChoiceExecutionResult.UnsupportedType));
    }

    private static ChoiceExecutionConfig CloneConfig(int index)
    {
        PopupEventSO popup = AssetDatabase.LoadAssetAtPath<PopupEventSO>(PopupPath);
        ChoiceExecutionConfig source = popup.choices[index].executionConfig;
        ChoiceExecutionData data = index == 0
            ? (ChoiceExecutionData)UnityEngine.JsonUtility.FromJson<RandomGrowthSafeExecutionData>(
                UnityEngine.JsonUtility.ToJson(source.data))
            : UnityEngine.JsonUtility.FromJson<RandomGrowthDeclineExecutionData>(
                UnityEngine.JsonUtility.ToJson(source.data));
        return new ChoiceExecutionConfig { executionType = source.executionType, data = data };
    }
}
