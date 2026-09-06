using System.IO;
using NUnit.Framework;
using UnityEngine;

public sealed class CharacterKitingRepositionV1ContractTests
{
    private static string Read(string relativePath)
    {
        string root = Directory.GetParent(Application.dataPath).FullName;
        return File.ReadAllText(Path.Combine(root, relativePath));
    }

    [Test]
    public void PilotScopeAndTiming_AreExactAndBattleOnly()
    {
        string state = Read("Assets/Scripts/Actor/Character/state/KitingRepositionState.cs");
        Assert.That(state, Does.Contain("id.IndexOf(\"seojin\""));
        Assert.That(state, Does.Contain("id.IndexOf(\"jihan\""));
        Assert.That(state, Does.Contain("id.IndexOf(\"yujin\""));
        Assert.That(state, Does.Contain("character.CharacterType != CharacterType.Player"));
        Assert.That(state, Does.Contain("if (!IsBattleScene()) return false;"));
        Assert.That(state, Does.Contain("private const float ReadinessCadence = .10f;"));
        Assert.That(state, Does.Contain("private const float ThreatCadence = .20f;"));
        Assert.That(state, Does.Contain("private const float EntryDebounce = .15f;"));
        Assert.That(state, Does.Contain("private const float DestinationLock = .45f;"));
        Assert.That(state, Does.Contain("private const float MaxLeg = 1.20f;"));
        Assert.That(state, Does.Contain("private const float InterLegHold = .20f;"));
    }

    [Test]
    public void ReadinessSnapshot_ReusesAuthoritativeCooldownAndFailureClocks()
    {
        string active = Read("Assets/Scripts/Actor/Character/service/skill/ActiveSkillService.cs");
        Assert.That(active, Does.Contain("public readonly struct OffensiveReadinessSnapshot"));
        Assert.That(active, Does.Contain("data.cooldownEndTimes.TryGetValue"));
        Assert.That(active, Does.Contain("failedSkillRetryEndTimes.TryGetValue"));
        Assert.That(active, Does.Not.Contain("kitingCooldown"));
        Assert.That(active, Does.Not.Contain("kitingReadyTime"));
    }

    [Test]
    public void DecisionBoundary_PreservesReadyAndOutOfRangeSkillFlow()
    {
        string decision = Read("Assets/Scripts/Actor/Character/service/state/CharacterDecisionEngine.cs");
        int kiting = decision.IndexOf("ShouldEnterKiting(context", System.StringComparison.Ordinal);
        int select = decision.IndexOf("return new SelectSkillState();", System.StringComparison.Ordinal);
        int move = decision.IndexOf("return new MoveToTargetState();", System.StringComparison.Ordinal);
        Assert.That(kiting, Is.GreaterThanOrEqualTo(0));
        Assert.That(select, Is.GreaterThan(kiting));
        Assert.That(move, Is.GreaterThan(select));
        Assert.That(decision, Does.Contain("snapshot.hasUsableOffensive"));
        Assert.That(decision, Does.Contain("OffensiveBlockReason.Cooldown"));
        Assert.That(decision, Does.Contain("OffensiveBlockReason.FailureRetry"));
        Assert.That(decision, Does.Contain("OffensiveBlockReason.Cadence"));
    }

    [Test]
    public void MovementOwnership_ManualInputWinsAndStateAlwaysReleases()
    {
        string party = Read("Assets/Scripts/Actor/Party/PartyMovementMono.cs");
        string state = Read("Assets/Scripts/Actor/Character/state/KitingRepositionState.cs");
        Assert.That(party, Does.Contain("TryAcquireExternalMovement(object owner)"));
        Assert.That(party, Does.Contain("HasRecentManualInput()"));
        Assert.That(party, Does.Contain("ReleaseExternalMovement(object owner)"));
        Assert.That(state, Does.Contain("partyMovement.ReleaseExternalMovement(this)"));
        Assert.That(state, Does.Contain("!context.CharacterManager.CanMove"));
        Assert.That(state, Does.Contain("context.AnimationMono.IsPlayingAttack()"));
    }

    [Test]
    public void ExactFamilyProfilesAndSafetyGuards_AreSerializedInCode()
    {
        string state = Read("Assets/Scripts/Actor/Character/state/KitingRepositionState.cs");
        Assert.That(state, Does.Contain("Mathf.Clamp(range * .75f, .45f, .85f)"));
        Assert.That(state, Does.Contain("Mathf.Clamp(range * 1.05f, .45f, .85f)"));
        Assert.That(state, Does.Contain("min = 1.40f; max = 2.20f"));
        Assert.That(state, Does.Contain("Mathf.Clamp(range * .55f, 2.60f, 5.50f)"));
        Assert.That(state, Does.Contain("Mathf.Clamp(range * .70f, 2.60f, 5.50f)"));
        Assert.That(state, Does.Contain("Profile.Seojin, .55f, 3.5f, 5f").Or.Contain("new(Family.Seojin, .55f, 3.5f, 5f)"));
        Assert.That(state, Does.Contain("new(Family.Jihan, 1f, 3f, 4.5f)"));
        Assert.That(state, Does.Contain("new(Family.Yujin, 1.6f, 3.5f, 5.5f)"));
        Assert.That(state, Does.Contain("Physics2D.LinecastAll"));
        Assert.That(state, Does.Contain("ClampToBounds"));
        Assert.That(state, Does.Contain("stuckFailures >= 2"));
    }

    [Test]
    public void ReadySkillHandoff_IsCheckedEveryStateTick()
    {
        string state = Read("Assets/Scripts/Actor/Character/state/KitingRepositionState.cs");
        int snapshot = state.IndexOf("OffensiveReadinessSnapshot readiness = Snapshot(context);", System.StringComparison.Ordinal);
        int cadence = state.IndexOf("if (now >= nextReadiness)", System.StringComparison.Ordinal);
        Assert.That(snapshot, Is.GreaterThanOrEqualTo(0));
        Assert.That(cadence, Is.GreaterThan(snapshot));
        Assert.That(state, Does.Contain("if (readiness.hasUsableOffensive) { Finish(context); return; }"));
    }
}
