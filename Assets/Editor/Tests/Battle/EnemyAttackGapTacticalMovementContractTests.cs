using System.IO;
using Npc.Service;
using NUnit.Framework;
using UnityEngine;

public sealed class EnemyAttackGapTacticalMovementContractTests
{
    [Test]
    public void ExactDefaultsAndMeleeBandAreLocked()
    {
        Assert.That(EnemyCombatRepositionState.MinDwell, Is.EqualTo(.45f));
        Assert.That(EnemyCombatRepositionState.MaxDwell, Is.EqualTo(.75f));
        Assert.That(EnemyCombatRepositionState.RepathInterval, Is.EqualTo(.20f));
        Assert.That(EnemyCombatRepositionState.LateralSpeedMultiplier, Is.EqualTo(.70f));
        Assert.That(EnemyCombatRepositionState.RadialSpeedMultiplier, Is.EqualTo(.55f));
        Assert.That(EnemyCombatRepositionState.CrowdRadius, Is.EqualTo(.65f));
        Assert.That(EnemyCombatRepositionState.MaxCrowdContribution, Is.EqualTo(.35f));
        Assert.That(EnemyCombatRepositionState.MaxCrowdScan, Is.EqualTo(16));

        var state = new EnemyCombatRepositionState();
        state.Enter(42, 0f);
        var tooFar = state.EvaluateMelee(new Vector2(1f, 0f), Vector2.zero, 1f, Vector2.zero, 0f, false);
        Assert.That(Vector2.Dot(tooFar.Direction, Vector2.left), Is.GreaterThan(.99f),
            "distance above .95 range must re-enter, never retreat infinitely");
    }

    [Test]
    public void StableIdProducesStableLateralChoiceAndBlockedFlipIsDelayed()
    {
        var a = new EnemyCombatRepositionState();
        var b = new EnemyCombatRepositionState();
        a.Enter(1234, 0f);
        b.Enter(1234, 0f);
        Vector2 self = new Vector2(.8f, 0f);
        var first = a.EvaluateMelee(self, Vector2.zero, 1f, Vector2.zero, 0f, false);
        var same = b.EvaluateMelee(self, Vector2.zero, 1f, Vector2.zero, 0f, false);
        Assert.That(first.Direction, Is.EqualTo(same.Direction));

        var beforeDelay = a.EvaluateMelee(self, Vector2.zero, 1f, Vector2.zero, .2f, true);
        Assert.That(beforeDelay.Direction, Is.EqualTo(first.Direction));
    }

    [Test]
    public void RecoverySignalIsNonConsumingAndPlayerKitingIsIsolated()
    {
        string executor = File.ReadAllText("Assets/Scripts/Actor/Party/SkillExecutorMono.cs");
        string pathing = File.ReadAllText("Assets/Scripts/Actor/NPC/NpcPathing.cs");
        string state = File.ReadAllText("Assets/Scripts/Actor/NPC/service/EnemyCombatRepositionState.cs");

        Assert.That(executor, Does.Contain("GetEnemyOffensiveRecoverySignal"));
        Assert.That(executor, Does.Contain("GetRemainingCooldown(basic)"));
        string query = executor.Substring(executor.IndexOf("GetEnemyOffensiveRecoverySignal"), 1800);
        Assert.That(query, Does.Not.Contain("ClearRequest("));
        Assert.That(query, Does.Not.Contain("TryExecutePending("));
        Assert.That(pathing, Does.Contain("EnemyOffensiveRecoveryState.Ready && signal.InEffectiveRange"));
        Assert.That(pathing, Does.Contain("EnemyOffensiveRecoveryState.Windup"));
        Assert.That(pathing, Does.Contain("signal.AllowsReposition"));
        Assert.That(pathing, Does.Contain("Existing range service retains retreat/safety priority"));
        Assert.That(pathing, Does.Contain("other.gameObject.layer != gameObject.layer"));
        Assert.That(state, Does.Not.Contain("KitingRepositionState"));
        Assert.That(state, Does.Not.Contain("PartyMovement"));
        Assert.That(state, Does.Not.Contain("CharacterDecisionEngine"));
        Assert.That(pathing, Does.Not.Contain("Character.Skill.KitingRepositionState"));
    }

    [Test]
    public void SignalStateOnlyAllowsRecoveryAndCooldownReposition()
    {
        Assert.That(new EnemyOffensiveRecoverySignal(EnemyOffensiveRecoveryState.Ready, true, 0f).AllowsReposition, Is.False);
        Assert.That(new EnemyOffensiveRecoverySignal(EnemyOffensiveRecoveryState.Windup, true, 0f).AllowsReposition, Is.False);
        Assert.That(new EnemyOffensiveRecoverySignal(EnemyOffensiveRecoveryState.Recovery, true, 1f).AllowsReposition, Is.True);
        Assert.That(new EnemyOffensiveRecoverySignal(EnemyOffensiveRecoveryState.Cooldown, true, 1f).AllowsReposition, Is.True);
        Assert.That(new EnemyOffensiveRecoverySignal(EnemyOffensiveRecoveryState.Disabled, false, 0f).AllowsReposition, Is.False);
    }
}
