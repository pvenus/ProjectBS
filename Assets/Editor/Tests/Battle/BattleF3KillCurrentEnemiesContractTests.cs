using System.IO;
using NUnit.Framework;

public sealed class BattleF3KillCurrentEnemiesContractTests
{
    [Test]
    public void F3RoutesOnlyThroughBattleEnemyKillSnapshot()
    {
        string source = File.ReadAllText(
            "Assets/Scripts/Core/Session/GameSession.cs");

        Assert.That(source, Does.Contain("killCurrentWaveTestKey = KeyCode.F3"));
        Assert.That(source, Does.Contain("BattleSession.IsBattleActive"));
        Assert.That(source, Does.Contain("HasOpenPopup"));
        Assert.That(source, Does.Contain("KillCurrentSpawnedEnemiesForDebug()"));
        Assert.That(source, Does.Not.Contain("OpenSkillUpgradeWindowForDebug"));
        Assert.That(source, Does.Not.Contain("Input.GetKeyDown(skillUpgradeTestKey)"));
        Assert.That(source, Does.Contain("returnStageTestKey = KeyCode.F2"));
    }

    [Test]
    public void KillOperationSnapshotsRegistryAndUsesLethalDamagePipeline()
    {
        string source = File.ReadAllText(
            "Assets/Scripts/Battle/Core/BattleManager.cs");
        int start = source.IndexOf("public int KillCurrentSpawnedEnemiesForDebug()");
        int end = source.IndexOf("private void HandleDebugSkillUpgradeCompleted()", start);
        Assert.That(start, Is.GreaterThanOrEqualTo(0));
        Assert.That(end, Is.GreaterThan(start));
        string method = source.Substring(start, end - start);

        Assert.That(method, Does.Contain("EnemyRegistry.Instance.ActiveEnemies"));
        Assert.That(method, Does.Contain("List<CharacterManager> snapshot = new()"));
        Assert.That(method, Does.Contain("CharacterType.Npc"));
        Assert.That(method, Does.Contain("CharacterType.Boss"));
        Assert.That(method, Does.Contain("data.isDead"));
        Assert.That(method, Does.Contain("enemy.IsDying"));
        Assert.That(method, Does.Contain("uniqueRoots.Add"));
        Assert.That(method, Does.Contain("enemy.TakeLethalDamage(attacker)"));
        Assert.That(method, Does.Not.Contain("Destroy("));
        Assert.That(method, Does.Not.Contain("SetActive("));
    }

    [Test]
    public void LethalDamageStillTerminatesThroughNormalDeathHandler()
    {
        string source = File.ReadAllText(
            "Assets/Scripts/Actor/Character/service/CharacterDamageService.cs");
        Assert.That(source, Does.Contain("public float TakeLethalDamage("));
        Assert.That(source, Does.Contain("targetManager.RegisterLastHitAttacker(attackerManager)"));
        Assert.That(source, Does.Contain("targetManager.HandleDeath()"));
    }
}
