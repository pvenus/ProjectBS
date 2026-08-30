#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Character;
using NUnit.Framework;
using Party;
using Progression;
using Progression.RandomGrowth;
using ResourceTools.Stage;
using Skill;
using Stage;
using UnityEditor;
using UnityEngine;

public sealed class SafeGrowthPresentationBuilderTests
{
    private const string JsonPath = "Assets/Contents/Stage/json/event/act01/event.act1.random_growth.02.windworn_sword_marks.json";

    [Test]
    public void ProductionJsonProjectsFinalCanonicalThirtyOneFieldCatalog()
    {
        RandomGrowthSafePresentationCatalogBlueprint blueprint =
            ResourceTools.Stage.RandomGrowthSafeProjectionContract.CreateV2CatalogBlueprint();
        SafeGrowthPresentationCopy copy = Copy();
        Assert.That(copy.SemanticDigest, Is.EqualTo(blueprint.Expectation.SemanticCopyDigest));
        Assert.That(copy.DefinitionFingerprint, Is.EqualTo(blueprint.Expectation.DefinitionFingerprint));
        Assert.That(copy.Get("discoveryBody"), Does.Contain("한 걸음 물러서자"));
        Assert.That(copy.Get("candidateZeroReason"), Is.EqualTo("강화 가능한 스킬이 생긴 뒤 다시 확인할 수 있습니다."));
        Assert.That(copy.Get("reminderTemplate"), Does.Contain("{ownerName}").And.Contain("{levelAfter}"));
        Assert.That(copy.Get("candidateTwoStatus"), Is.EqualTo("무작위 성장 후보 2개를 확인합니다."));
        Assert.That(copy.Get("candidateOneStatus"), Is.EqualTo("현재 확인 가능한 무작위 성장 후보는 1개입니다."));
        Assert.That(copy.Get("busyStatus"), Is.EqualTo("관찰의 결과를 확정하고 있습니다."));
    }

    [Test]
    public void OldCopyOrWrongOrderIsRejected()
    {
        Document document = JsonUtility.FromJson<Document>(File.ReadAllText(JsonPath));
        List<SafeGrowthCopyField> fields = Fields(document);
        fields[1] = new SafeGrowthCopyField(fields[1].Name, "교정 전 카피");
        Assert.That(SafeGrowthPresentationCopyResolver.TryResolve(fields,
            document.definitionFingerprint, out _), Is.False);
        fields = Fields(document); (fields[0], fields[1]) = (fields[1], fields[0]);
        Assert.That(SafeGrowthPresentationCopyResolver.TryResolve(fields,
            document.definitionFingerprint, out _), Is.False);
    }

    [TestCase(2, 2)]
    [TestCase(1, 1)]
    [TestCase(0, 0)]
    public void OfferCandidateCountsAndActionsAreTyped(int candidates, int displayed)
    {
        using EligibilityFixture f = EligibilityFixture.Create(candidates);
        SafeGrowthPresentationSnapshot result = Build(SafeGrowthInteractionState.Offerable,
            f.Snapshot, discovered: true);
        Assert.That(result.State, Is.EqualTo(candidates == 0
            ? SafeGrowthPresentationState.DisabledNoCandidate : SafeGrowthPresentationState.Offerable));
        Assert.That(result.TargetCount, Is.EqualTo(2));
        Assert.That(result.DisplayCandidateCount, Is.EqualTo(displayed));
        Assert.That(result.Actions, Does.Contain(candidates == 0
            ? SafeGrowthPresentationActionIntent.RecheckEligibility
            : SafeGrowthPresentationActionIntent.RequestObservePreconfirm));
    }

    [Test]
    public void DiscoveryAndPreconfirmUseCanonicalCopyAndIntent()
    {
        using EligibilityFixture f = EligibilityFixture.Create(2);
        SafeGrowthPresentationSnapshot discovery = Build(SafeGrowthInteractionState.Offerable,
            f.Snapshot, discovered: false);
        SafeGrowthPresentationSnapshot confirm = Build(SafeGrowthInteractionState.Preconfirm,
            f.Snapshot);
        Assert.That(discovery.State, Is.EqualTo(SafeGrowthPresentationState.Discovery));
        Assert.That(discovery.Title, Is.EqualTo("바람에 남은 검식"));
        Assert.That(confirm.State, Is.EqualTo(SafeGrowthPresentationState.Preconfirm));
        Assert.That(confirm.Cta, Is.EqualTo("검식을 익힌다"));
        Assert.That(confirm.Actions, Does.Contain(SafeGrowthPresentationActionIntent.CancelPreconfirm));
    }

    [Test]
    public void AlreadyGrantedCapAndPartyChangedFailClosed()
    {
        using EligibilityFixture f = EligibilityFixture.Create(2);
        Assert.That(Build(SafeGrowthInteractionState.Offerable, f.Snapshot,
            alreadyGranted: true).State, Is.EqualTo(SafeGrowthPresentationState.DisabledAlreadyGranted));
        Assert.That(Build(SafeGrowthInteractionState.Offerable, f.Snapshot,
            capReached: true).State, Is.EqualTo(SafeGrowthPresentationState.DisabledCapReached));
        Assert.That(Build(SafeGrowthInteractionState.Offerable, f.Snapshot,
            runtimeFingerprint: "stale").State, Is.EqualTo(SafeGrowthPresentationState.DisabledPartyChanged));
    }

    [Test]
    public void PendingRetryBusyAndTerminalStatesMapWithoutCommands()
    {
        using EligibilityFixture f = EligibilityFixture.Create(2);
        SafeGrowthPresentationSnapshot retry = Build(
            SafeGrowthInteractionState.ObserveSelectedPendingRetry, f.Snapshot);
        SafeGrowthPresentationSnapshot busy = Build(SafeGrowthInteractionState.Preconfirm,
            f.Snapshot, applying: true);
        SafeGrowthPresentationSnapshot granted = Build(SafeGrowthInteractionState.SafeGrowthGranted, f.Snapshot);
        SafeGrowthPresentationSnapshot declined = Build(SafeGrowthInteractionState.Declined, f.Snapshot);
        Assert.That(retry.State, Is.EqualTo(SafeGrowthPresentationState.PendingRetry));
        Assert.That(retry.Actions, Is.EquivalentTo(new[] { SafeGrowthPresentationActionIntent.RetrySameChoice }));
        Assert.That(busy.State, Is.EqualTo(SafeGrowthPresentationState.BusyApplying));
        Assert.That(granted.State, Is.EqualTo(SafeGrowthPresentationState.TerminalSafeGranted));
        Assert.That(granted.Actions, Does.Contain(SafeGrowthPresentationActionIntent.OpenGrowthOffer));
        Assert.That(declined.State, Is.EqualTo(SafeGrowthPresentationState.TerminalDeclined));
        Assert.That(declined.Actions, Does.Contain(SafeGrowthPresentationActionIntent.ContinueStage));
    }

    [Test]
    public void TerminalReplayRetainsTerminalResultAndHasNoExecutorReference()
    {
        using EligibilityFixture f = EligibilityFixture.Create(2);
        SafeGrowthPresentationSnapshot replay = Build(SafeGrowthInteractionState.SafeGrowthGranted,
            f.Snapshot, terminalReplay: true);
        Assert.That(replay.State, Is.EqualTo(SafeGrowthPresentationState.TerminalReplay));
        Assert.That(replay.ResultId, Is.EqualTo(SafeGrowthTransactionIds.GrantedResultId));
        Assert.That(replay.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public), Is.Empty);
    }

    [Test]
    public void InvalidCopyReturnsImmutableDisabledSnapshot()
    {
        using EligibilityFixture f = EligibilityFixture.Create(2);
        SafeGrowthPresentationSnapshot result = new SafeGrowthPresentationBuilder().Build(
            new SafeGrowthPresentationInput(null, SafeGrowthInteractionState.Offerable, false,
                f.Snapshot, false, false, true, false, "instance", "", f.Snapshot.Revision,
                f.Snapshot.Fingerprint, Dispatch(false), Dispatch(true)));
        Assert.That(result.State, Is.EqualTo(SafeGrowthPresentationState.Invalid));
        Assert.That(result.ObserveEnabled, Is.False);
        Assert.That(result.Actions, Is.EquivalentTo(new[] { SafeGrowthPresentationActionIntent.None }));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<SafeGrowthPresentationActionIntent>)result.Actions).Add(SafeGrowthPresentationActionIntent.ContinueStage));
    }

    [Test]
    public void CultureAndReentryProduceIdenticalSnapshotsAndPreserveLfNfc()
    {
        CultureInfo before = CultureInfo.CurrentCulture;
        try
        {
            using EligibilityFixture f = EligibilityFixture.Create(2);
            CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
            SafeGrowthPresentationSnapshot first = Build(SafeGrowthInteractionState.Preconfirm, f.Snapshot);
            CultureInfo.CurrentCulture = new CultureInfo("ko-KR");
            SafeGrowthPresentationSnapshot second = Build(SafeGrowthInteractionState.Preconfirm, f.Snapshot);
            Assert.That(second.State, Is.EqualTo(first.State));
            Assert.That(second.Body, Is.EqualTo(first.Body));
            Assert.That(second.RuntimeFingerprint, Is.EqualTo(first.RuntimeFingerprint));
            Assert.That(second.Body, Does.Contain("\n").And.Not.Contain("\r"));
            Assert.That(second.Body.IsNormalized(), Is.True);
        }
        finally { CultureInfo.CurrentCulture = before; }
    }

    [Test]
    public void V2CatalogResolvesExactThirtyOneFieldsAndRejectsSubset()
    {
        RandomGrowthSafePresentationCatalogBlueprint b =
            ResourceTools.Stage.RandomGrowthSafeProjectionContract.CreateV2CatalogBlueprint();
        RandomGrowthPresentationCopyAsset asset = V2Asset(b, b.Fields);
        Assert.That(SafeGrowthPresentationCopyResolver.TryResolveV2(asset, b.Expectation,
            out SafeGrowthPresentationCopy copy, out RandomGrowthPresentationCopyMismatch mismatch),
            Is.True, $"exact31 mismatch={mismatch}");
        Assert.That(mismatch, Is.EqualTo(RandomGrowthPresentationCopyMismatch.None));
        Assert.That(copy.SchemaVersion, Is.EqualTo(2));
        Assert.That(copy.Get("candidateTwoStatus"), Is.EqualTo("무작위 성장 후보 2개를 확인합니다."));
        UnityEngine.Object.DestroyImmediate(asset);

        asset = V2Asset(b, b.Fields.Take(30).ToArray());
        Assert.That(SafeGrowthPresentationCopyResolver.TryResolveV2(asset, b.Expectation,
            out _, out mismatch), Is.False);
        Assert.That(mismatch, Is.EqualTo(RandomGrowthPresentationCopyMismatch.SubsetOrOrderMismatch));
        UnityEngine.Object.DestroyImmediate(asset);
    }

    [Test]
    public void V2PreconfirmUsesCanonicalCountTwoOrOneStatus()
    {
        SafeGrowthPresentationCopy copy = V2Copy();
        using EligibilityFixture two = EligibilityFixture.Create(2);
        using EligibilityFixture one = EligibilityFixture.Create(1);
        Assert.That(BuildV2(copy, SafeGrowthInteractionState.Preconfirm, two.Snapshot).Status,
            Is.EqualTo("무작위 성장 후보 2개를 확인합니다."));
        Assert.That(BuildV2(copy, SafeGrowthInteractionState.Preconfirm, one.Snapshot).Status,
            Is.EqualTo("현재 확인 가능한 무작위 성장 후보는 1개입니다."));
    }

    [Test]
    public void V2BusyLocksActionsAndUsesCanonicalBusyStatus()
    {
        using EligibilityFixture f = EligibilityFixture.Create(2);
        SafeGrowthPresentationSnapshot x = BuildV2(V2Copy(),
            SafeGrowthInteractionState.Preconfirm, f.Snapshot, applying: true);
        Assert.That(x.State, Is.EqualTo(SafeGrowthPresentationState.BusyApplying));
        Assert.That(x.Status, Is.EqualTo("관찰의 결과를 확정하고 있습니다."));
        Assert.That(x.ObserveEnabled, Is.False);
        Assert.That(x.DeclineEnabled, Is.False);
        Assert.That(x.Actions, Is.EquivalentTo(new[] { SafeGrowthPresentationActionIntent.None }));
    }

    [Test]
    public void V2WrongLocaleAndFingerprintFailClosed()
    {
        RandomGrowthSafePresentationCatalogBlueprint b =
            ResourceTools.Stage.RandomGrowthSafeProjectionContract.CreateV2CatalogBlueprint();
        RandomGrowthPresentationCopyAsset asset = V2Asset(b, b.Fields);
        var wrong = new RandomGrowthPresentationCopyExpectation(2,
            b.Expectation.ContentContractVersion, "en-US", b.Expectation.CatalogId,
            b.Expectation.ProjectionKind, b.Expectation.SemanticDomain,
            b.Expectation.DefinitionDomain, b.Expectation.EventId, b.Expectation.SourcePopupId,
            b.Expectation.SemanticCopyDigest, new string('0', 64), b.Expectation.OrderedFieldNames);
        Assert.That(SafeGrowthPresentationCopyResolver.TryResolveV2(asset, wrong,
            out _, out RandomGrowthPresentationCopyMismatch mismatch), Is.False);
        Assert.That(mismatch, Is.EqualTo(RandomGrowthPresentationCopyMismatch.FingerprintMismatch));
        UnityEngine.Object.DestroyImmediate(asset);
    }

    private static SafeGrowthPresentationSnapshot Build(SafeGrowthInteractionState state,
        SafeGrowthEligibilitySnapshot eligibility, bool discovered = true, bool alreadyGranted = false,
        bool capReached = false, bool applying = false, bool terminalReplay = false,
        string runtimeFingerprint = null) => new SafeGrowthPresentationBuilder().Build(
            new SafeGrowthPresentationInput(Copy(), state, applying, eligibility,
                alreadyGranted, capReached, discovered, terminalReplay, "instance.safe",
                "token.safe", eligibility.Revision, runtimeFingerprint ?? eligibility.Fingerprint,
                Dispatch(false, state), Dispatch(true, state)));

    private static ConfirmableChoiceDispatchResult Dispatch(bool decline,
        SafeGrowthInteractionState state = SafeGrowthInteractionState.Offerable)
    {
        PopupEventSO popup = AssetDatabase.LoadAssetAtPath<PopupEventSO>(
            "Assets/Contents/Stage/so/node.act1.random_growth.02.windworn_sword_marks.intro.asset");
        PopupEventChoice choice = popup.GetChoice(decline
            ? SafeGrowthTransactionIds.DeclineChoiceId : SafeGrowthTransactionIds.ObserveChoiceId);
        ConfirmableChoiceRuntimeState runtime = state switch
        {
            SafeGrowthInteractionState.ObserveSelectedPendingRetry => ConfirmableChoiceRuntimeState.PendingRetry,
            SafeGrowthInteractionState.SafeGrowthGranted or SafeGrowthInteractionState.Declined => ConfirmableChoiceRuntimeState.Terminal,
            _ => ConfirmableChoiceRuntimeState.Offerable
        };
        return ChoiceExecutionRouter.CreateDefault().QueryConfirmable(choice.executionConfig, runtime);
    }

    private static SafeGrowthPresentationCopy Copy() => V2Copy();

    private static SafeGrowthPresentationSnapshot BuildV2(SafeGrowthPresentationCopy copy,
        SafeGrowthInteractionState state, SafeGrowthEligibilitySnapshot eligibility, bool applying = false) =>
        new SafeGrowthPresentationBuilder().Build(new SafeGrowthPresentationInput(copy, state,
            applying, eligibility, false, false, true, false, "instance.safe", "token.safe",
            eligibility.Revision, eligibility.Fingerprint, Dispatch(false, state), Dispatch(true, state)));

    private static SafeGrowthPresentationCopy V2Copy()
    {
        RandomGrowthSafePresentationCatalogBlueprint b =
            ResourceTools.Stage.RandomGrowthSafeProjectionContract.CreateV2CatalogBlueprint();
        RandomGrowthPresentationCopyAsset asset = V2Asset(b, b.Fields);
        Assert.That(SafeGrowthPresentationCopyResolver.TryResolveV2(asset, b.Expectation,
            out SafeGrowthPresentationCopy copy, out RandomGrowthPresentationCopyMismatch mismatch),
            Is.True, $"exact31 mismatch={mismatch}");
        Assert.That(mismatch, Is.EqualTo(RandomGrowthPresentationCopyMismatch.None));
        UnityEngine.Object.DestroyImmediate(asset);
        return copy;
    }

    private static RandomGrowthPresentationCopyAsset V2Asset(
        RandomGrowthSafePresentationCatalogBlueprint b,
        IReadOnlyList<KeyValuePair<string, string>> fields)
    {
        RandomGrowthPresentationCopyAsset asset =
            ScriptableObject.CreateInstance<RandomGrowthPresentationCopyAsset>();
        SerializedObject serialized = new(asset);
        serialized.FindProperty("schemaVersion").intValue = b.Expectation.SchemaVersion;
        Set("contentContractVersion", b.Expectation.ContentContractVersion);
        Set("locale", b.Expectation.Locale);
        Set("catalogId", b.Expectation.CatalogId);
        Set("projectionKind", b.Expectation.ProjectionKind);
        Set("semanticDomain", b.Expectation.SemanticDomain);
        Set("definitionDomain", b.Expectation.DefinitionDomain);
        Set("eventId", b.Expectation.EventId);
        Set("sourcePopupId", b.Expectation.SourcePopupId);
        Set("semanticCopyDigest", b.Expectation.SemanticCopyDigest);
        Set("definitionFingerprint", b.Expectation.DefinitionFingerprint);
        SerializedProperty serializedFields = serialized.FindProperty("fields");
        serializedFields.arraySize = fields.Count;
        for (int i = 0; i < fields.Count; i++)
        {
            SerializedProperty element = serializedFields.GetArrayElementAtIndex(i);
            element.FindPropertyRelative("name").stringValue = fields[i].Key;
            element.FindPropertyRelative("value").stringValue = fields[i].Value;
        }
        serialized.ApplyModifiedPropertiesWithoutUndo();
        Assert.That(asset.Fields, Has.Count.EqualTo(fields.Count), "serialized field count");
        for (int i = 0; i < fields.Count; i++)
            Assert.That(asset.Fields[i].Name, Is.EqualTo(fields[i].Key), $"serialized fields[{i}].name");
        return asset;

        void Set(string propertyName, string value) =>
            serialized.FindProperty(propertyName).stringValue = value;
    }

    private static List<SafeGrowthCopyField> Fields(Document document)
    {
        List<SafeGrowthCopyField> result = new();
        foreach (Field field in document.semanticCopyKo) result.Add(new SafeGrowthCopyField(field.name, field.value));
        return result;
    }

    [Serializable] private sealed class Document
    { public string definitionFingerprint; public List<Field> semanticCopyKo; }
    [Serializable] private sealed class Field { public string name; public string value; }

    private sealed class EligibilityFixture : IDisposable
    {
        private readonly List<UnityEngine.Object> owned = new();
        public SafeGrowthEligibilitySnapshot Snapshot;
        public static EligibilityFixture Create(int count)
        {
            EligibilityFixture f = new(); PartyRuntimeData party = new(); List<EquipmentSkillSO> catalog = new();
            for (int i = 0; i < count; i++)
            {
                string id = "presentation.skill." + i;
                CharacterSO character = ScriptableObject.CreateInstance<CharacterSO>(); f.owned.Add(character);
                character.ApplyEditorData("presentation.owner." + i, default, default, null, null, null);
                party.Members.Add(new CharacterRuntimeData { characterSO = character,
                    skillInstances = new List<EquipmentSkillInstanceData>
                    { new() { equipmentId = id, currentLevel = 1 } } });
                EquipmentSkillSO skill = ScriptableObject.CreateInstance<EquipmentSkillSO>(); f.owned.Add(skill);
                EquipmentUpgradeTableSO table = ScriptableObject.CreateInstance<EquipmentUpgradeTableSO>(); f.owned.Add(table);
                EquipmentUpgradeEntry one = new(); one.ApplyEditorData(1, null, null);
                EquipmentUpgradeEntry two = new(); two.ApplyEditorData(2, null, null);
                table.ApplyEditorData("table." + id, new List<EquipmentUpgradeEntry> { one, two });
                Set(skill, "equipmentId", id); Set(skill, "upgradeTableSo", table); catalog.Add(skill);
            }
            f.Snapshot = new PartyWideSafeGrowthEligibilityQuery().Query(party, catalog);
            return f;
        }
        public void Dispose() { foreach (UnityEngine.Object item in owned) UnityEngine.Object.DestroyImmediate(item); }
        private static void Set(object target, string field, object value) => target.GetType()
            .GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }
}
#endif
