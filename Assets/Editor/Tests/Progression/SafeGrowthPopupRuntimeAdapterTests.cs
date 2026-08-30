#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using Character;
using NUnit.Framework;
using Party;
using Progression;
using Progression.RandomGrowth;
using Session;
using Skill;
using Stage;
using UnityEditor;
using UnityEngine;

public sealed class SafeGrowthPopupRuntimeAdapterTests
{
    [Test]
    public void ActualSafeObserveSelectionRetainsNodeAndCreatesOnlyPendingContext()
    {
        using Fixture f = Fixture.Create(2);
        SafeGrowthPopupAdapterResult result = f.Adapter.Select(f.Popup, f.Node, f.Observe);

        Assert.That(result.Status, Is.EqualTo(SafeGrowthPopupAdapterStatus.RequiresConfirmation));
        Assert.That(f.Session.SafeGrowthPendingConfirm, Is.SameAs(result.Pending));
        Assert.That(f.Node.IsCompleted, Is.False);
        Assert.That(f.Runtime.ProgressionLedger.Count, Is.Zero);
        Assert.That(f.Runtime.ResultLedger.CommittedCount, Is.Zero);
    }

    [Test]
    public void CancelRestoresOfferableWithoutDomainMutation()
    {
        using Fixture f = Fixture.Create(2);
        f.Adapter.Select(f.Popup, f.Node, f.Observe);
        Assert.That(f.Adapter.Cancel().Status, Is.EqualTo(SafeGrowthPopupAdapterStatus.Cancelled));
        Assert.That(f.Session.SafeGrowthPendingConfirm, Is.Null);
        Assert.That(f.Session.SafeGrowthInteraction.State, Is.EqualTo(SafeGrowthInteractionState.Offerable));
        Assert.That(f.Runtime.ProgressionLedger.Count, Is.Zero);
    }

    [Test]
    public void CandidateZeroIsDisabledAndNeverCreatesInteraction()
    {
        using Fixture f = Fixture.Create(0);
        SafeGrowthPopupAdapterResult result = f.Adapter.Select(f.Popup, f.Node, f.Observe);
        Assert.That(result.Status, Is.EqualTo(SafeGrowthPopupAdapterStatus.Disabled));
        Assert.That(f.Session.SafeGrowthPendingConfirm, Is.Null);
        Assert.That(f.Session.SafeGrowthInteraction.Token, Is.Null);
    }

    [Test]
    public void ObserveConfirmCommitsPendingTerminalAndNodeExactlyOnce()
    {
        using Fixture f = Fixture.Create(2);
        SafeGrowthPopupAdapterResult selected = f.Adapter.Select(f.Popup, f.Node, f.Observe);
        SafeGrowthPopupAdapterResult result = f.Adapter.Confirm(f.Popup, f.Node, f.Observe, selected.Pending);

        Assert.That(result.Status, Is.EqualTo(SafeGrowthPopupAdapterStatus.Succeeded));
        Assert.That(f.Node.IsCompleted, Is.True);
        Assert.That(f.Runtime.ProgressionLedger.Count, Is.EqualTo(1));
        Assert.That(f.Runtime.ResultLedger.CommittedCount, Is.EqualTo(1));
        Assert.That(f.Session.SafeGrowthPendingConfirm, Is.Null);
    }

    [Test]
    public void DeclineConfirmCompletesNodeWithoutPendingOrCap()
    {
        using Fixture f = Fixture.Create(0);
        SafeGrowthPopupAdapterResult selected = f.Adapter.Select(f.Popup, f.Node, f.Decline);
        SafeGrowthPopupAdapterResult result = f.Adapter.Confirm(f.Popup, f.Node, f.Decline, selected.Pending);

        Assert.That(result.Status, Is.EqualTo(SafeGrowthPopupAdapterStatus.Declined));
        Assert.That(f.Node.IsCompleted, Is.True);
        Assert.That(f.Runtime.ProgressionLedger.Count, Is.Zero);
        Assert.That(f.Runtime.ResultLedger.CommittedCount, Is.EqualTo(1));
    }

    [Test]
    public void StaleEligibilityRejectsBeforeTransactionAndKeepsPopupPending()
    {
        using Fixture f = Fixture.Create(2);
        SafeGrowthPopupAdapterResult selected = f.Adapter.Select(f.Popup, f.Node, f.Observe);
        f.Party.Members[0].skillInstances[0].currentLevel = 2;

        SafeGrowthPopupAdapterResult result = f.Adapter.Confirm(f.Popup, f.Node, f.Observe, selected.Pending);
        Assert.That(result.Status, Is.EqualTo(SafeGrowthPopupAdapterStatus.Disabled));
        Assert.That(f.Node.IsCompleted, Is.False);
        Assert.That(f.Runtime.ProgressionLedger.Count, Is.Zero);
        Assert.That(f.Session.SafeGrowthPendingConfirm, Is.Not.Null);
    }

    [Test]
    public void NodePrepareFailureAbortsTransactionAndMovesToPendingRetry()
    {
        using Fixture f = Fixture.Create(2);
        SafeGrowthPopupAdapterResult selected = f.Adapter.Select(f.Popup, f.Node, f.Observe);
        f.Node.state = RoundNodeState.Cleared;

        SafeGrowthPopupAdapterResult result = f.Adapter.Confirm(f.Popup, f.Node, f.Observe, selected.Pending);
        Assert.That(result.Status, Is.EqualTo(SafeGrowthPopupAdapterStatus.Failed));
        Assert.That(f.Runtime.ProgressionLedger.Count, Is.Zero);
        Assert.That(f.Runtime.ResultLedger.CommittedCount, Is.Zero);
        Assert.That(f.Session.SafeGrowthInteraction.State,
            Is.EqualTo(SafeGrowthInteractionState.ObserveSelectedPendingRetry));
        Assert.That(f.Session.SafeGrowthPendingConfirm, Is.Not.Null);
    }

    [Test]
    public void IdentityMismatchCannotConfirmOrMutate()
    {
        using Fixture f = Fixture.Create(2);
        SafeGrowthPopupAdapterResult selected = f.Adapter.Select(f.Popup, f.Node, f.Observe);
        SafeGrowthPendingConfirmContext forged = new(selected.Pending.PopupId, "wrong-node",
            selected.Pending.NodeId, selected.Pending.ChoiceId, selected.Pending.DefinitionFingerprint,
            selected.Pending.InteractionTokenId, selected.Pending.EligibilityRevision,
            selected.Pending.EligibilityFingerprint, selected.Pending.Choice);

        Assert.That(f.Adapter.Confirm(f.Popup, f.Node, f.Observe, forged).Status,
            Is.EqualTo(SafeGrowthPopupAdapterStatus.IdentityMismatch));
        Assert.That(f.Node.IsCompleted, Is.False);
        Assert.That(f.Runtime.ProgressionLedger.Count, Is.Zero);
    }

    [Test]
    public void ReentryAdapterRestoresSamePendingAndTerminalReplayDoesNotExecute()
    {
        using Fixture f = Fixture.Create(2);
        SafeGrowthPopupAdapterResult selected = f.Adapter.Select(f.Popup, f.Node, f.Observe);
        SafeGrowthPopupRuntimeAdapter reentered = f.NewAdapter();
        Assert.That(reentered.Pending, Is.SameAs(selected.Pending));
        Assert.That(reentered.Confirm(f.Popup, f.Node, f.Observe, selected.Pending).Status,
            Is.EqualTo(SafeGrowthPopupAdapterStatus.Succeeded));
        Assert.That(reentered.Confirm(f.Popup, f.Node, f.Observe, selected.Pending).Status,
            Is.EqualTo(SafeGrowthPopupAdapterStatus.TerminalReplay));
        Assert.That(reentered.Select(f.Popup, f.Node, f.Observe).Status,
            Is.EqualTo(SafeGrowthPopupAdapterStatus.TerminalReplay));
        Assert.That(f.Runtime.ProgressionLedger.Count, Is.EqualTo(1));
    }

    [Test]
    public void DeferredPublishFaultKeepsCommittedStateAndIsNotRepublished()
    {
        using Fixture f = Fixture.Create(2);
        SafeGrowthPopupAdapterResult selected = f.Adapter.Select(f.Popup, f.Node, f.Observe);
        int callbacks = 0;
        SafeGrowthPopupAdapterResult result = f.Adapter.Confirm(f.Popup, f.Node, f.Observe,
            selected.Pending, _ => { callbacks++; throw new InvalidOperationException(); });

        Assert.That(result.Status, Is.EqualTo(SafeGrowthPopupAdapterStatus.Succeeded));
        Assert.That(callbacks, Is.EqualTo(1));
        Assert.That(f.Node.IsCompleted, Is.True);
        Assert.That(f.Runtime.ProgressionLedger.Count, Is.EqualTo(1));
    }

    private sealed class Fixture : IDisposable
    {
        private readonly List<UnityEngine.Object> owned = new();
        public StageSession Session;
        public SafeGrowthRuntimeComposition Runtime;
        public PartyRuntimeData Party;
        public EquipmentSkillSO[] Catalog;
        public PopupEventSO Popup;
        public PopupEventChoice Observe;
        public PopupEventChoice Decline;
        public RoundNode Node;
        public SafeGrowthPopupRuntimeAdapter Adapter;

        public static Fixture Create(int candidateCount)
        {
            Fixture f = new();
            f.Popup = AssetDatabase.LoadAssetAtPath<PopupEventSO>(
                "Assets/Contents/Stage/so/node.act1.random_growth.02.windworn_sword_marks.intro.asset");
            RoundNodeSO content = AssetDatabase.LoadAssetAtPath<RoundNodeSO>(
                "Assets/Contents/Stage/so/stage.act1.random_growth.02.windworn_sword_marks.asset");
            Assert.That(f.Popup, Is.Not.Null); Assert.That(content, Is.Not.Null);
            f.Observe = f.Popup.GetChoice(SafeGrowthTransactionIds.ObserveChoiceId);
            f.Decline = f.Popup.GetChoice(SafeGrowthTransactionIds.DeclineChoiceId);
            Assert.That(f.Observe, Is.Not.Null); Assert.That(f.Decline, Is.Not.Null);

            ProgressionSession progression = new();
            ProgressionRunId run = new("run.br3.safe");
            progression.ResetForNewRun(run);
            f.Session = new StageSession();
            f.Node = new RoundNode("node-instance.br3.safe", RoundNodeType.Event, 0, 0)
            { roundNodeSO = content, state = RoundNodeState.Available, isSelected = true };
            RoundNode next = new("node-instance.br3.next", RoundNodeType.Event, 1, 0);
            StageGraph graph = new("stage.chapter1", "Chapter1")
            { progressState = StageProgressState.InProgress, currentNodeId = f.Node.nodeId };
            graph.AddNode(f.Node); graph.AddNode(next); graph.ConnectNodes(f.Node.nodeId, next.nodeId);
            f.Session.Initialize(new StageRuntimeData { currentGraph = graph, currentNode = f.Node });
            f.Session.ResetRandomGrowthForNewRun(run);
            f.Session.TryCommitChapter1RandomGrowthGraph(run, "stage.chapter1", 5, 5,
                new IdentityFactory(), out _);
            Assert.That(f.Session.ConfigureSafeGrowthRuntime(progression), Is.True);
            f.Runtime = f.Session.SafeGrowthRuntime;

            f.Party = new PartyRuntimeData();
            List<EquipmentSkillSO> skills = new();
            for (int i = 0; i < candidateCount; i++)
            {
                EquipmentSkillSO skill = f.CreateSkill("skill.br3." + i, 2);
                skills.Add(skill); f.Party.Members.Add(f.CreateMember("owner.br3." + i, skill.EquipmentId));
            }
            f.Catalog = skills.ToArray();
            Assert.That(f.Session.SafeGrowthPlacement.TryStore(new SafeGrowthStoredAssignment(
                run.Value, f.Session.RandomGrowthSession.StageGenerationId, "manifest.br3",
                SafeGrowthTransactionIds.ReservationId, "sec_ep_2_to_ep_3_1", "sec_ep_2_to_ep_3_2",
                "slot_430_2085", "slot_1370_2085", SafeGrowthTransactionIds.EventId, content, false)), Is.True);
            Assert.That(new SafeGrowthRouteEntryBridge().TryEnter(f.Session,
                "sec_ep_2_to_ep_3_1", "slot_430_2085", f.Node.nodeId, content.nodeId,
                SafeGrowthTransactionIds.EventId, f.Party, f.Catalog).Status,
                Is.EqualTo(candidateCount == 0 ? SafeGrowthRouteEntryStatus.Suppressed : SafeGrowthRouteEntryStatus.Entered));
            if (candidateCount == 0)
            {
                // Candidate-zero after disclosure is a disabled/recheck state, not a replacement.
                f.Session.SafeGrowthPlacement.TryMarkDisclosed();
                f.Session.TryStoreSafeGrowthRouteEncounter(new SafeGrowthRouteEncounterReceipt(
                    SafeGrowthTransactionIds.ReservationId, "sec_ep_2_to_ep_3_1", "slot_430_2085",
                    f.Node.nodeId, SafeGrowthTransactionIds.EventId, string.Empty));
            }
            f.Adapter = f.NewAdapter();
            return f;
        }

        public SafeGrowthPopupRuntimeAdapter NewAdapter() => new(Session, Party, Catalog,
            ChoiceExecutionRouter.CreateDefault());

        private CharacterRuntimeData CreateMember(string owner, string skill)
        {
            CharacterSO character = ScriptableObject.CreateInstance<CharacterSO>(); owned.Add(character);
            character.ApplyEditorData(owner, default, default, null, null, null);
            return new CharacterRuntimeData { characterSO = character,
                skillInstances = new List<EquipmentSkillInstanceData>
                { new() { equipmentId = skill, currentLevel = 1 } } };
        }

        private EquipmentSkillSO CreateSkill(string id, int maxLevel)
        {
            EquipmentSkillSO skill = ScriptableObject.CreateInstance<EquipmentSkillSO>(); owned.Add(skill);
            EquipmentUpgradeTableSO table = ScriptableObject.CreateInstance<EquipmentUpgradeTableSO>(); owned.Add(table);
            List<EquipmentUpgradeEntry> entries = new();
            for (int i = 1; i <= maxLevel; i++)
            { EquipmentUpgradeEntry entry = new(); entry.ApplyEditorData(i, null, null); entries.Add(entry); }
            table.ApplyEditorData("table." + id, entries);
            Set(skill, "equipmentId", id); Set(skill, "upgradeTableSo", table);
            return skill;
        }

        public void Dispose()
        {
            foreach (UnityEngine.Object item in owned) UnityEngine.Object.DestroyImmediate(item);
        }

        private static void Set(object target, string field, object value) => target.GetType()
            .GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }

    private sealed class IdentityFactory : IRandomGrowthSessionIdentityFactory
    {
        public ProgressionRunId CreateRunId() => new("unused");
        public string CreateStageGenerationId(ProgressionRunId runId, string chapterId) =>
            "generation-" + runId.Value;
    }
}
#endif
