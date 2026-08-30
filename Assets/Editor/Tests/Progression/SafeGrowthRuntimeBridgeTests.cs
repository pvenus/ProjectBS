using System;
using System.Collections.Generic;
using System.Reflection;
using Character;
using NUnit.Framework;
using Party;
using Progression;
using Progression.Portfolio;
using Progression.RandomGrowth;
using Session;
using Skill;
using Stage;
using UnityEngine;

public sealed class SafeGrowthRuntimeBridgeTests
{
    [Test]
    public void CompositionPersistsAcrossResetRuntimeAndReentry()
    {
        StageSession stage = Session("run-a", out ProgressionSession progression);
        SafeGrowthRuntimeComposition first = stage.SafeGrowthRuntime;
        stage.ResetRuntime();
        Assert.That(stage.ConfigureSafeGrowthRuntime(progression), Is.True);
        Assert.That(stage.SafeGrowthRuntime, Is.SameAs(first));
        Assert.That(first.IsReady, Is.True);
    }

    [Test]
    public void NewRunReplacesCompositionAndClearSuppresses()
    {
        StageSession stage = Session("run-a", out ProgressionSession progression);
        SafeGrowthRuntimeComposition first = stage.SafeGrowthRuntime;
        progression.ResetForNewRun(new ProgressionRunId("run-b"));
        stage.ResetRandomGrowthForNewRun(progression.RunId);
        Assert.That(stage.ConfigureSafeGrowthRuntime(progression), Is.True);
        Assert.That(stage.SafeGrowthRuntime, Is.Not.SameAs(first));
        stage.Clear();
        Assert.That(stage.SafeGrowthRuntime, Is.Null);
    }

    [Test]
    public void VersionMismatchIsNotReady()
    {
        ProgressionSession progression = new();
        progression.ResetForNewRun(new ProgressionRunId("run-a"));
        var composition = new SafeGrowthRuntimeComposition(progression,
            new SafeGrowthInteractionOwnership(), "wrong");
        Assert.That(composition.IsReady, Is.False);
        Assert.That(composition.Transaction, Is.Null);
    }

    [TestCase(2, 2)]
    [TestCase(1, 1)]
    [TestCase(0, 0)]
    public void EligibilityReturnsActualPartyCandidatesUpToTargetTwo(int members, int expected)
    {
        PartyRuntimeData party = new();
        List<EquipmentSkillSO> catalog = new();
        for (int i = 0; i < members; i++)
        {
            string skillId = "skill-" + i;
            party.Members.Add(Member("owner-" + i, skillId, 1));
            catalog.Add(Skill(skillId, 2));
        }
        SafeGrowthEligibilitySnapshot result =
            new PartyWideSafeGrowthEligibilityQuery().Query(party, catalog);
        Assert.That(result.EligibleCount, Is.EqualTo(expected));
        Assert.That(result.TargetCount, Is.EqualTo(expected));
        Assert.That(result.Status, Is.EqualTo(expected == 0
            ? SafeGrowthEligibilityStatus.NoCandidate : SafeGrowthEligibilityStatus.Eligible));
    }

    [Test]
    public void DuplicateOwnerAndDuplicateTargetFailClosed()
    {
        EquipmentSkillSO skill = Skill("skill", 2);
        PartyRuntimeData duplicateOwner = new();
        duplicateOwner.Members.Add(Member("owner", "skill", 1));
        duplicateOwner.Members.Add(Member("owner", "other", 1));
        Assert.That(new PartyWideSafeGrowthEligibilityQuery().Query(
            duplicateOwner, new[] { skill, Skill("other", 2) }).Status,
            Is.EqualTo(SafeGrowthEligibilityStatus.InvalidRoster));

        CharacterRuntimeData member = Member("unique", "skill", 1);
        member.skillInstances.Add(new EquipmentSkillInstanceData { equipmentId = "skill", currentLevel = 1 });
        PartyRuntimeData duplicateTarget = new(); duplicateTarget.Members.Add(member);
        Assert.That(new PartyWideSafeGrowthEligibilityQuery().Query(
            duplicateTarget, new[] { skill }).Status,
            Is.EqualTo(SafeGrowthEligibilityStatus.InvalidRoster));
    }

    [Test]
    public void InvalidCatalogFailsClosedAndLevelChangeMakesSnapshotStale()
    {
        PartyRuntimeData party = new();
        CharacterRuntimeData member = Member("owner", "skill", 1);
        party.Members.Add(member);
        PartyWideSafeGrowthEligibilityQuery query = new();
        Assert.That(query.Query(party, Array.Empty<EquipmentSkillSO>()).Status,
            Is.EqualTo(SafeGrowthEligibilityStatus.InvalidData));
        EquipmentSkillSO skill = Skill("skill", 2);
        SafeGrowthEligibilitySnapshot snapshot = query.Query(party, new[] { skill });
        member.skillInstances[0].currentLevel = 2;
        Assert.That(query.IsCurrent(snapshot, party, new[] { skill }), Is.False);
    }

    [Test]
    public void ActualRouteEntryIsExactlyOnceAndOffRouteDoesNothing()
    {
        StageSession stage = Session("run-a", out _);
        StoreAssignment(stage, false);
        PartyRuntimeData party = new(); party.Members.Add(Member("owner", "skill", 1));
        EquipmentSkillSO skill = Skill("skill", 2);
        SafeGrowthRouteEntryBridge bridge = new();

        Assert.That(bridge.TryEnter(stage, "wrong", "slot_430_2085", "instance", "stage.act1.random_growth.02.windworn_sword_marks",
            SafeGrowthTransactionIds.EventId, party, new[] { skill }).Status,
            Is.EqualTo(SafeGrowthRouteEntryStatus.Ignored));
        Assert.That(stage.SafeGrowthRouteEncounter, Is.Null);
        Assert.That(bridge.TryEnter(stage, "sec_ep_2_to_ep_3_1", "slot_430_2085", "instance", "wrong-node",
            SafeGrowthTransactionIds.EventId, party, new[] { skill }).Status,
            Is.EqualTo(SafeGrowthRouteEntryStatus.Ignored));
        Assert.That(bridge.TryEnter(stage, "sec_ep_2_to_ep_3_1", "slot_430_2085", "instance", "stage.act1.random_growth.02.windworn_sword_marks",
            SafeGrowthTransactionIds.EventId, party, new[] { skill }).Status,
            Is.EqualTo(SafeGrowthRouteEntryStatus.Entered));
        Assert.That(bridge.TryEnter(stage, "sec_ep_2_to_ep_3_1", "slot_430_2085", "instance", "stage.act1.random_growth.02.windworn_sword_marks",
            SafeGrowthTransactionIds.EventId, party, new[] { skill }).Status,
            Is.EqualTo(SafeGrowthRouteEntryStatus.Existing));
        Assert.That(stage.SafeGrowthInteraction.Token, Is.Null);
    }

    [Test]
    public void OptionalGrantBeforeDisclosureStoresManifestFallbackWithoutReroll()
    {
        StageSession stage = Session("run-cap", out ProgressionSession progression);
        RoundNodeSO fallbackNode = ScriptableObject.CreateInstance<RoundNodeSO>();
        fallbackNode.nodeId = "stage.fallback";
        GrowthCandidateReservation candidate = new(GrowthCandidateKind.Safe,
            SafeGrowthTransactionIds.EventId, SafeGrowthTransactionIds.ReservationId,
            "sec_ep_2_to_ep_3_1", "sec_ep_2_to_ep_3_2", "slot_430_2085", "slot_1370_2085",
            0, true, "event.fallback", 2);
        Chapter1PortfolioManifest manifest = new(PortfolioManifestStatus.Ready,
            progression.RunId.Value, stage.RandomGrowthSession.StageGenerationId,
            new[] { candidate }, Array.Empty<PortfolioEventDescriptor>(), "manifest");
        stage.ConfigureSafeGrowthPlacement(new SafeGrowthPlacementRequest(manifest,
            id => id == "event.fallback" ? fallbackNode : null));
        StoreAssignment(stage, false);
        Assert.That(progression.Ledger.TryEarn(new ProgressionEarnRequest(
            ProgressionSourceRegistry.OptionalRandomGrowthSegment,
            ProgressionSourceCategory.Random, ProgressionSourceType.RandomEventSafe,
            ProgressionSourceRegistry.RandomGrowthSafeSource,
            SafeGrowthTransactionIds.GrantedResultId), out _), Is.EqualTo(ProgressionEarnResult.Earned));

        SafeGrowthRouteEntryResult result = new SafeGrowthRouteEntryBridge().TryEnter(stage,
            "sec_ep_2_to_ep_3_1", "slot_430_2085", "instance",
            "stage.act1.random_growth.02.windworn_sword_marks", SafeGrowthTransactionIds.EventId,
            new PartyRuntimeData(), Array.Empty<EquipmentSkillSO>());
        Assert.That(result.Status, Is.EqualTo(SafeGrowthRouteEntryStatus.Fallback));
        Assert.That(stage.SafeGrowthPlacement.Assignment.DisplayedEventId, Is.EqualTo("event.fallback"));
        Assert.That(stage.SafeGrowthPlacement.Assignment.IsFallback, Is.True);
        Assert.That(stage.SafeGrowthRouteEncounter, Is.Null);
    }

    [Test]
    public void FallbackAndDisclosedCandidateZeroNeverCreateInteraction()
    {
        StageSession fallback = Session("run-a", out _); StoreAssignment(fallback, true);
        SafeGrowthRouteEntryBridge bridge = new();
        Assert.That(bridge.TryEnter(fallback, "sec_ep_2_to_ep_3_1", "slot_430_2085", "i", "stage.fallback",
            "event.fallback", new PartyRuntimeData(), Array.Empty<EquipmentSkillSO>()).Status,
            Is.EqualTo(SafeGrowthRouteEntryStatus.Fallback));

        StageSession disclosed = Session("run-b", out _); StoreAssignment(disclosed, false);
        disclosed.SafeGrowthPlacement.TryMarkDisclosed();
        Assert.That(bridge.TryEnter(disclosed, "sec_ep_2_to_ep_3_1", "slot_430_2085", "i", "stage.act1.random_growth.02.windworn_sword_marks",
            SafeGrowthTransactionIds.EventId, new PartyRuntimeData(), Array.Empty<EquipmentSkillSO>()).Status,
            Is.EqualTo(SafeGrowthRouteEntryStatus.DisabledRecheck));
        Assert.That(disclosed.SafeGrowthPlacement.Assignment.IsFallback, Is.False);
        Assert.That(disclosed.SafeGrowthInteraction.Token, Is.Null);
    }

    [Test]
    public void MissingV2CatalogFallsBackBeforeDisclosureAndBecomesTypedFailureAfterDisclosure()
    {
        StageSession undisclosed = Session("run-v2-a", out _); StoreAssignment(undisclosed, false);
        Assert.That(new SafeGrowthRouteEntryBridge().ResolveV2PresentationBeforePopup(
            undisclosed, null), Is.EqualTo(SafeGrowthRouteEntryStatus.Suppressed));
        Assert.That(undisclosed.SafeGrowthInteraction.Token, Is.Null);

        StageSession disclosed = Session("run-v2-b", out _); StoreAssignment(disclosed, false);
        disclosed.SafeGrowthPlacement.TryMarkDisclosed();
        Assert.That(new SafeGrowthRouteEntryBridge().ResolveV2PresentationBeforePopup(
            disclosed, null), Is.EqualTo(
                SafeGrowthRouteEntryStatus.PresentationContentUnavailableAfterDisclosure));
        Assert.That(disclosed.SafeGrowthPlacement.Assignment.IsFallback, Is.False);
        Assert.That(disclosed.SafeGrowthInteraction.Token, Is.Null);
    }

    private static StageSession Session(string run, out ProgressionSession progression)
    {
        progression = new ProgressionSession();
        progression.ResetForNewRun(new ProgressionRunId(run));
        StageSession stage = new();
        stage.ResetRandomGrowthForNewRun(progression.RunId);
        stage.TryCommitChapter1RandomGrowthGraph(progression.RunId, "stage.chapter1", 5, 5,
            new IdentityFactory(), out _);
        Assert.That(stage.ConfigureSafeGrowthRuntime(progression), Is.True);
        return stage;
    }

    private static void StoreAssignment(StageSession stage, bool fallback)
    {
        RoundNodeSO node = ScriptableObject.CreateInstance<RoundNodeSO>();
        node.nodeId = fallback ? "stage.fallback" : "stage.act1.random_growth.02.windworn_sword_marks";
        Assert.That(stage.SafeGrowthPlacement.TryStore(new SafeGrowthStoredAssignment(
            stage.RandomGrowthSession.RunId.Value, stage.RandomGrowthSession.StageGenerationId,
            "manifest", SafeGrowthTransactionIds.ReservationId,
            "sec_ep_2_to_ep_3_1", "sec_ep_2_to_ep_3_2", "slot_430_2085", "slot_1370_2085",
            fallback ? "event.fallback" : SafeGrowthTransactionIds.EventId, node, fallback)), Is.True);
    }

    private static CharacterRuntimeData Member(string owner, string skill, int level)
    {
        CharacterSO character = ScriptableObject.CreateInstance<CharacterSO>();
        character.ApplyEditorData(owner, default, default, null, null, null);
        return new CharacterRuntimeData
        {
            characterSO = character,
            skillInstances = new List<EquipmentSkillInstanceData>
            { new() { equipmentId = skill, currentLevel = level } }
        };
    }

    private static EquipmentSkillSO Skill(string id, int maxLevel)
    {
        EquipmentSkillSO skill = ScriptableObject.CreateInstance<EquipmentSkillSO>();
        EquipmentUpgradeTableSO table = ScriptableObject.CreateInstance<EquipmentUpgradeTableSO>();
        List<EquipmentUpgradeEntry> entries = new();
        for (int i = 1; i <= maxLevel; i++)
        {
            EquipmentUpgradeEntry entry = new(); entry.ApplyEditorData(i, null, null); entries.Add(entry);
        }
        table.ApplyEditorData("table-" + id, entries);
        Set(skill, "equipmentId", id); Set(skill, "upgradeTableSo", table);
        return skill;
    }

    private static void Set(object target, string field, object value) =>
        target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);

    private sealed class IdentityFactory : IRandomGrowthSessionIdentityFactory
    {
        public ProgressionRunId CreateRunId() => new("unused");
        public string CreateStageGenerationId(ProgressionRunId runId, string chapterId) =>
            "generation-" + runId.Value;
    }
}
