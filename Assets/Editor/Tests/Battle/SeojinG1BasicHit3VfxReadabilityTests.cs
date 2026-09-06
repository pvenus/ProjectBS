using System.IO;
using NUnit.Framework;

public sealed class SeojinG1BasicHit3VfxReadabilityTests
{
    private const string G1Json =
        "Assets/Contents/Skill/json/skill.character.seojin.1.basic_attack.basic_attack.json";
    private const string G1Skill =
        "Assets/Contents/Skill/so/skill.character.seojin.1.basic_attack.basic_attack.asset";
    private const string Profile =
        "Assets/Contents/Skill/vfx/vfx-seojin-g1-basic-hit3-neon.asset";
    private const string Material =
        "Assets/Contents/Skill/material/skill-animation-vfx-seojin-g1-basic-hit3-neon.mat";
    private const string Shader = "Assets/Shaders/SkillAnimationVfxSourceReadable.shader";

    [Test]
    public void DedicatedProfileIsBoundOnlyToG1ThirdComboStep()
    {
        string json = File.ReadAllText(G1Json);
        string skill = File.ReadAllText(G1Skill);
        Assert.That(Count(json, "\"vfxProfile\""), Is.EqualTo(1));
        Assert.That(json, Does.Contain("\"vfxProfile\": \"skillAnimationVfx.seojinG1BasicHit3Neon.v1\""));
        Assert.That(Count(skill, "vfxProfileOverride: {fileID: 11400000, guid: 9b2c41a6078d49c5a2fc9e78e0d3a5b1"), Is.EqualTo(1));
        Assert.That(File.ReadAllText(
            "Assets/Contents/Skill/json/skill.character.seojin.2.basic_attack.basic_attack.json"),
            Does.Not.Contain("skillAnimationVfx.seojinG1BasicHit3Neon.v1"));
        Assert.That(File.ReadAllText(
            "Assets/Contents/Skill/json/skill.character.seojin.3.basic_attack.basic_attack.json"),
            Does.Not.Contain("skillAnimationVfx.seojinG1BasicHit3Neon.v1"));
    }

    [Test]
    public void SourceReadableProfilePreservesAlphaAndKeepsGlowBounded()
    {
        string profile = File.ReadAllText(Profile);
        string material = File.ReadAllText(Material);
        string shader = File.ReadAllText(Shader);
        Assert.That(profile, Does.Contain("bodyOpacityGain: 0.65"));
        Assert.That(profile, Does.Contain("emissionIntensity: 0.08"));
        Assert.That(profile, Does.Contain("outlineColor: {r: 0.25, g: 0.85, b: 1, a: 0.28}"));
        Assert.That(profile, Does.Contain("tintStrength: 0"));
        Assert.That(profile, Does.Contain("inkDensity: 0"));
        Assert.That(material, Does.Contain("guid: 9b0c41a6078d49c5a2fc9e78e0d3a5b1"));
        Assert.That(shader, Does.Contain("float3 rgb = source.rgb +"));
        Assert.That(shader, Does.Contain("Blend One OneMinusSrcAlpha"));
        Assert.That(shader, Does.Contain("source.a <= _AlphaClip"));
    }

    [Test]
    public void OverrideSurvivesResolverAndPoolCloneAndFallsBackWhenNull()
    {
        string resolver = File.ReadAllText(
            "Assets/Scripts/Ability/Skills/Services/EquipmentSkillResolver.cs");
        string factory = File.ReadAllText(
            "Assets/Scripts/Ability/Skills/Services/ProjectileFactory.cs");
        string visual = File.ReadAllText(
            "Assets/Scripts/Ability/Skills/Projectiles/ProjectileVisual.cs");
        Assert.That(resolver, Does.Contain("projectileData.animationVfxProfileOverride = animationVfxProfileOverride"));
        Assert.That(resolver, Does.Contain("animationVfxProfileOverride = source.animationVfxProfileOverride"));
        Assert.That(factory, Does.Contain("animationVfxProfileOverride = source.animationVfxProfileOverride"));
        Assert.That(visual, Does.Contain("data.animationVfxProfileOverride != null"));
        Assert.That(visual, Does.Contain("baseVisual.AnimationVfxProfile"));
        Assert.That(visual, Does.Contain("animationVfx?.StopImmediate()"));
        Assert.That(visual, Does.Contain("RestoreBaselineMaterial();"));
    }

    [Test]
    public void ThirdHitUsesFullSixFrameVisualLifetimeWithoutChangingGameplayLifetime()
    {
        string json = File.ReadAllText(G1Json);
        string skill = File.ReadAllText(G1Skill);
        string lifetime = File.ReadAllText(
            "Assets/Scripts/Ability/Skills/Projectiles/ProjectileLifetime.cs");
        string entity = File.ReadAllText(
            "Assets/Scripts/Ability/Skills/Projectiles/ProjectileEntity.cs");
        string visual = File.ReadAllText(
            "Assets/Scripts/Ability/Skills/Projectiles/ProjectileVisual.cs");
        Assert.That(json, Does.Contain("\"projectileLifetime\": 0.2"));
        Assert.That(Count(json, "\"minimumVisualLifetime\": 0.15"), Is.EqualTo(1));
        Assert.That(Count(skill, "minimumVisualLifetime: 0.15"), Is.EqualTo(1));
        Assert.That(json, Does.Not.Contain(
            "\"vfxPresentationCalibration\": \"skill.character.seojin.1.basic_attack.combo.vfx.hit2.calibration\""));
        Assert.That(skill, Does.Contain(
            "vfxPresentationCalibration: {fileID: 0}"));
        Assert.That(lifetime, Does.Contain("runtimeData.minimumVisualLifetime > lifetime"));
        Assert.That(lifetime, Does.Contain("owner.CompleteCollisionAndDespawnAfterVisual();"));
        Assert.That(visual, Does.Contain("Mathf.Max(runtimeData.lifetime, runtimeData.minimumVisualLifetime)"));
        Assert.That(entity, Does.Contain("1f / 60f"));
        Assert.That(entity, Does.Contain("runtimeData.minimumVisualLifetime - elapsedSinceInitialize"));
        Assert.That(entity, Does.Contain("Mathf.Max(remainingVisualTime, remainingMinimumTime)"));
        string clip = File.ReadAllText(
            "Assets/AnimationClips/Skill/skill.character.seojin.1.basic_attack.basic_attack.combo.2.visual.loop.anim");
        Assert.That(clip, Does.Contain("m_StopTime: 0.15"));
        Assert.That(clip, Does.Contain("- time: 0.025"));
        Assert.That(clip, Does.Contain("- time: 0.05"));
        Assert.That(clip, Does.Contain("- time: 0.075"));
        Assert.That(clip, Does.Contain("- time: 0.1"));
        Assert.That(clip, Does.Contain("- time: 0.125"));
    }

    [Test]
    public void CooldownCommitsAtThirdStepEntryAndDoesNotWaitForHitOrVisual()
    {
        string service = File.ReadAllText(
            "Assets/Scripts/Actor/Character/service/skill/ActiveSkillService.cs");
        string json = File.ReadAllText(G1Json);
        Assert.That(json, Does.Contain("\"startTime\": 0.78"));
        Assert.That(json, Does.Contain("\"hitTime\": 0.94"));
        Assert.That(json, Does.Contain("\"duration\": 1.06"));
        Assert.That(service, Does.Contain("skill.character.seojin.1.basic_attack.basic_attack"));
        Assert.That(service, Does.Contain("cooldownOnThirdStepEntry && step.ComboIndex == 2"));
        Assert.That(service, Does.Contain("thirdStepCooldownCommitted = UseSkill(skillManager, runtime)"));
        Assert.That(service, Does.Contain("if (cooldownOnThirdStepEntry)"));
        Assert.That(service, Does.Contain("anyStepFired && completedSteps == steps.Length"));
    }

    [Test]
    public void AboveOwnerSkillPresentationUsesAbsoluteSkillTopOrder()
    {
        string projectileVisual = File.ReadAllText(
            "Assets/Scripts/Ability/Skills/Projectiles/ProjectileVisual.cs");
        string spawnSequence = File.ReadAllText(
            "Assets/Scripts/Ability/Skills/Spawn/CharacterSpawnSequence.cs");
        Assert.That(projectileVisual, Does.Contain(
            "runtimeData.sortingRelation == SkillSortingRelation.AboveOwner"));
        Assert.That(projectileVisual, Does.Contain(
            "ApplyResolvedSortingOrder((int)SkillSortingRelation.AbsoluteTop)"));
        Assert.That(spawnSequence, Does.Contain(
            "sortingRelation == SkillSortingRelation.AboveOwner"));
        Assert.That(spawnSequence, Does.Contain(
            "visualRenderer.sortingOrder = (int)SkillSortingRelation.AbsoluteTop"));
    }

    private static int Count(string text, string value)
    {
        int count = 0;
        for (int index = 0; (index = text.IndexOf(value, index, System.StringComparison.Ordinal)) >= 0;
             index += value.Length) count++;
        return count;
    }
}
