#if UNITY_EDITOR
using System.IO;
using NUnit.Framework;

public sealed class SeojinBasicSkillWeaveCancelContractTests
{
    [Test]
    public void GameplaySpawnReceipt_LocksFirstTwoRecoveriesAndOpensFinisherBridge()
    {
        string service = File.ReadAllText(
            "Assets/Scripts/Actor/Character/service/skill/ActiveSkillService.cs");
        int spawn = service.IndexOf("bool fired = UseSkillOnce(", System.StringComparison.Ordinal);
        int receipt = service.IndexOf("skillManager.MarkManualActionContact(",
            spawn, System.StringComparison.Ordinal);
        Assert.That(spawn, Is.GreaterThanOrEqualTo(0));
        Assert.That(receipt, Is.GreaterThan(spawn));
        Assert.That(service, Does.Contain(
            "stepIndex < 2 ? float.PositiveInfinity : 0f"));
        Assert.That(service, Does.Contain("stepIndex < 2"));
    }

    [Test]
    public void SnapshotAndCancellation_PreserveReceiptIdentityAndLinkerException()
    {
        string manager = File.ReadAllText(
            "Assets/Scripts/Actor/Character/CharacterSkillManager.cs");
        string control = File.ReadAllText(
            "Assets/Scripts/Actor/Character/Control/SeojinManualControl.cs");
        string core = File.ReadAllText(
            "Assets/Scripts/Actor/Character/Control/ManualControlCore.cs");
        Assert.That(manager, Does.Contain("GameplayAttackCommitted"));
        Assert.That(manager, Does.Contain("ComboIndex=kind>0?kind-1:-1"));
        Assert.That(control, Does.Contain("CancelManualExecution(slot==6)"));
        Assert.That(core, Does.Contain("game.CancelForReservedSkill(requested)"));
        Assert.That(core, Does.Contain("game.Busy&&basicPressPending&&game.CanCancelForReservedSkill"));
        Assert.That(core, Does.Contain("game.CancelForReservedSkill(0)"));
        Assert.That(core, Does.Contain("ClearPending();"));
    }

    [Test]
    public void FinisherGather_IsCollisionSafeResistanceAwareAndBossCapped()
    {
        string hit = File.ReadAllText(
            "Assets/Scripts/Ability/Skills/Projectiles/ProjectileHitHandler.cs");
        string movement = File.ReadAllText(
            "Assets/Scripts/Ability/Skills/Runtime/Mouse3GatherDisplacementMono.cs");
        string root = File.ReadAllText(
            "Assets/Scripts/Ability/Effects/Runtime/config/StatModifierEffectRuntime.cs");
        Assert.That(hit, Does.Contain("ApplyComboGather(targetCharacter)"));
        Assert.That(hit, Does.Contain("Mouse3CrowdControlPolicy.ResolveDistance"));
        Assert.That(hit, Does.Contain("runtimeData.comboGatherBossHardCap"));
        Assert.That(movement, Does.Contain("movement.ApplyDisplacement(delta, step)"));
        Assert.That(root, Does.Contain("config.RootHardCap"));
        Assert.That(root, Does.Contain("Mouse3CrowdControlPolicy.ResolveDuration"));
    }

    [TestCase("skill.character.seojin.1.basic_attack.basic_attack.json", .16f, .61f, .83f)]
    [TestCase("skill.character.seojin.2.basic_attack.basic_attack.json", .14f, .38f, .65f)]
    [TestCase("skill.character.seojin.3.basic_attack.basic_attack.json", .12f, .34f, .58f)]
    public void GradeTimingSource_RemainsUnchanged(string file, float hit0, float hit1, float hit2)
    {
        string json = File.ReadAllText("Assets/Contents/Skill/json/" + file)
            .Replace(" ", string.Empty).Replace("\r", string.Empty).Replace("\n", string.Empty);
        Assert.That(json, Does.Contain($"\"hitTime\":{hit0}"));
        Assert.That(json, Does.Contain($"\"hitTime\":{hit1}"));
        Assert.That(json, Does.Contain($"\"hitTime\":{hit2}"));
    }
}
#endif
