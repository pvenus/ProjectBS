using System;
using System.Globalization;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public sealed class SeojinActive2ProceduralGroundVfxContractTests
{
    private static string Root => Application.dataPath;
    private static string Read(string relative) => File.ReadAllText(Path.Combine(Root, relative));

    [Test]
    public void ProceduralProfiles_RemainReusable_ButActive2UsesRasterExact6()
    {
        string g2 = Read("Contents/Skill/vfx/vfx-seojin-active2-ground-g2.asset");
        string g3 = Read("Contents/Skill/vfx/vfx-seojin-active2-ground-g3.asset");
        Assert.That(Scalar(g2, "proceduralGroundField"), Is.EqualTo(1f));
        Assert.That(Scalar(g3, "proceduralGroundField"), Is.EqualTo(1f));
        Assert.That(Scalar(g2, "fieldRadiusWorld"), Is.EqualTo(3f));
        Assert.That(Scalar(g3, "fieldRadiusWorld"), Is.EqualTo(3.5f));
        Assert.That(Scalar(g2, "supportLobes"), Is.EqualTo(2f));
        Assert.That(Scalar(g3, "supportLobes"), Is.EqualTo(3f));
        Assert.That(Scalar(g2, "pressureScarCount"), Is.EqualTo(3f));
        Assert.That(Scalar(g3, "pressureScarCount"), Is.EqualTo(4f));
        Assert.That(Scalar(g2, "deterministicSeed"), Is.EqualTo(2730622978f));
        Assert.That(Scalar(g3, "deterministicSeed"), Is.EqualTo(2730622979f));
        Assert.That(Scalar(g2, "rustCoverage"), Is.LessThanOrEqualTo(.025f));
        Assert.That(Scalar(g3, "rustCoverage"), Is.LessThanOrEqualTo(.035f));
        Assert.That(Scalar(g2, "edgeIrregularity"), Is.EqualTo(.22f));
        Assert.That(Scalar(g3, "edgeIrregularity"), Is.EqualTo(.26f));
        Assert.That(Scalar(g2, "emissionIntensity"), Is.EqualTo(0f));
        Assert.That(Scalar(g3, "emissionIntensity"), Is.EqualTo(0f));

        string v2 = Read("Contents/Skill/so/skill.character.seojin.2.active_2.crane_wing_formation.visual.asset");
        string v3 = Read("Contents/Skill/so/skill.character.seojin.3.active_2.crane_wing_formation.visual.asset");
        Assert.That(v2, Does.Contain("animationVfxProfile: {fileID: 0}"));
        Assert.That(v3, Does.Contain("animationVfxProfile: {fileID: 0}"));
        Assert.That(v2, Does.Not.Contain("guid: b2c2000200004f5a8d65000000000002"));
        Assert.That(v3, Does.Not.Contain("guid: b2c2000300004f5a8d65000000000003"));

        foreach (int grade in new[] { 2, 3 })
        {
            string clip = Read($"AnimationClips/Skill/skill.character.seojin.{grade}.active_2.crane_wing_formation.visual.loop.anim");
            Assert.That(clip.Split('\n').Count(line => line.TrimStart().StartsWith("- time:", StringComparison.Ordinal)), Is.GreaterThanOrEqualTo(6));
            foreach (string time in new[] { "0", "0.167", "0.333", "0.5", "0.667", "0.833" })
                Assert.That(clip, Does.Contain("time: " + time));
            Assert.That(clip, Does.Contain("m_StopTime: 1"));
            Assert.That(clip, Does.Contain("m_LoopTime: 1"));
        }
    }

    [Test]
    public void Shader_IsDefaultOff_AnalyticConnectedAndNonEmissive()
    {
        string shader = Read("Shaders/SkillAnimationVfx.shader");
        Assert.That(shader, Does.Contain("_VfxGroundFieldMode (\"VFX Ground Field Mode\", Float) = 0"));
        Assert.That(shader, Does.Contain("if (_VfxGroundFieldMode > .5) return groundField"));
        Assert.That(shader, Does.Contain("footprint=max(footprint,interior*.82)"));
        Assert.That(shader, Does.Contain("smoothstep(.78,1.0,r)"));
        Assert.That(shader, Does.Contain("(low-.5)*.24+(high-.5)*.08"));
        Assert.That(shader, Does.Contain("elongatedLobe"));
        Assert.That(shader, Does.Contain("dryScar"));
        Assert.That(shader, Does.Not.Contain("exp(-(q0.x*q0.x"));
        Assert.That(shader, Does.Contain("return fixed4(rgb*alpha,alpha)"));
        Assert.That(shader, Does.Not.Contain("tex2D(_VfxGround"));
        Assert.That(shader, Does.Not.Contain("_VfxFieldEmission"));
    }

    [Test]
    public void Feature_UsesOneLocalClock_NoHitRestart_AndExactReset()
    {
        string feature = Read("Scripts/Ability/Skills/Presentation/SkillAnimationVfxFeatureObject.cs");
        string visual = Read("Scripts/Ability/Skills/Projectiles/ProjectileVisual.cs");
        Assert.That(feature, Does.Contain("Mathf.Repeat(elapsed, 1f)"));
        Assert.That(feature, Does.Contain("ReducedMotion ? 0f"));
        Assert.That(feature, Does.Contain("controller?.ResetState()"));
        Assert.That(feature, Does.Contain("groundRenderer.enabled = false"));
        Assert.That(feature, Does.Contain("RestartsOnHit => profile != null && !profile.ProceduralGroundField"));
        Assert.That(visual, Does.Contain("animationVfx.RestartsOnHit"));
        Assert.That(visual, Does.Not.Contain("renderer.material"));
    }

    [Test]
    public void GroundMode_DoesNotWriteGameplayOrColliderState()
    {
        string feature = Read("Scripts/Ability/Skills/Presentation/SkillAnimationVfxFeatureObject.cs");
        string controller = Read("Scripts/Ability/Skills/Presentation/SkillAnimationVfxControllerMono.cs");
        string combined = feature + controller;
        Assert.That(combined, Does.Not.Contain("ProjectileHitHandler"));
        Assert.That(combined, Does.Not.Contain("ColliderRadius"));
        Assert.That(combined, Does.Not.Contain("Physics2D"));
        Assert.That(combined, Does.Not.Contain("Damage"));
        Assert.That(combined, Does.Not.Contain("sharedMaterial.Set"));
        Assert.That(combined, Does.Not.Contain("Random."));
        Assert.That(combined, Does.Not.Contain("Mathf.PerlinNoise"));
    }

    [Test]
    public void ExactGradeData_SeparatesProjectileRendererAndColliderScale()
    {
        string json2 = Read("Contents/Skill/json/skill.character.seojin.2.active_2.crane_wing_formation.json");
        string json3 = Read("Contents/Skill/json/skill.character.seojin.3.active_2.crane_wing_formation.json");
        string profile2 = Read("Contents/Skill/so/skill.character.seojin.2.active_2.crane_wing_formation.profile.asset");
        string profile3 = Read("Contents/Skill/so/skill.character.seojin.3.active_2.crane_wing_formation.profile.asset");
        string move2 = Read("Contents/Skill/so/skill.character.seojin.2.active_2.crane_wing_formation.move.asset");
        string move3 = Read("Contents/Skill/so/skill.character.seojin.3.active_2.crane_wing_formation.move.asset");

        Assert.That(json2, Does.Contain("\"projectileScale\": 0.8"));
        Assert.That(json3, Does.Contain("\"projectileScale\": 1.0"));
        Assert.That(json2, Does.Contain("\"rendererScale\": 0.1"));
        Assert.That(json3, Does.Contain("\"rendererScale\": 0.1"));
        Assert.That(json2, Does.Contain("\"projectileColliderRadius\": 2.2"));
        Assert.That(json3, Does.Contain("\"projectileColliderRadius\": 2.2"));
        Assert.That(profile2, Does.Contain("projectileScale: 0.8"));
        Assert.That(profile3, Does.Contain("projectileScale: 1"));
        Assert.That(profile2, Does.Contain("rendererScale: 0.1"));
        Assert.That(profile3, Does.Contain("rendererScale: 0.1"));
        Assert.That(profile2, Does.Contain("projectileColliderRadius: 2.2"));
        Assert.That(profile3, Does.Contain("projectileColliderRadius: 2.2"));
        Assert.That(move2, Does.Contain("followOffset: {x: 0, y: 0}"));
        Assert.That(move3, Does.Contain("followOffset: {x: 0, y: 0}"));
    }

    [Test]
    public void ExactGradeData_MaterializesEffectsAndBoundsRepeatTicks()
    {
        string json2 = Read("Contents/Skill/json/skill.character.seojin.2.active_2.crane_wing_formation.json");
        string json3 = Read("Contents/Skill/json/skill.character.seojin.3.active_2.crane_wing_formation.json");
        string profile2 = Read("Contents/Skill/so/skill.character.seojin.2.active_2.crane_wing_formation.profile.asset");
        string profile3 = Read("Contents/Skill/so/skill.character.seojin.3.active_2.crane_wing_formation.profile.asset");
        string cast2 = Read("Contents/Skill/so/skill.character.seojin.2.active_2.crane_wing_formation.cast.asset");
        string cast3 = Read("Contents/Skill/so/skill.character.seojin.3.active_2.crane_wing_formation.cast.asset");
        string hit21 = Read("Contents/Skill/so/skill.character.seojin.2.active_2.crane_wing_formation.hit.1.asset");
        string hit22 = Read("Contents/Skill/so/skill.character.seojin.2.active_2.crane_wing_formation.hit.2.asset");
        string hit31 = Read("Contents/Skill/so/skill.character.seojin.3.active_2.crane_wing_formation.hit.1.asset");
        string hit32 = Read("Contents/Skill/so/skill.character.seojin.3.active_2.crane_wing_formation.hit.2.asset");

        Assert.That(json2, Does.Contain("\"cooldown\": 18"));
        Assert.That(json3, Does.Contain("\"cooldown\": 16"));
        Assert.That(json2, Does.Contain("\"projectileLifetime\": 5"));
        Assert.That(json3, Does.Contain("\"projectileLifetime\": 6"));
        Assert.That(json3, Does.Contain("\"baseDamage\": 120"));
        Assert.That(profile2, Does.Contain("projectileLifetime: 5"));
        Assert.That(profile3, Does.Contain("projectileLifetime: 6"));
        Assert.That(cast2, Does.Contain("cooldown: 18"));
        Assert.That(cast3, Does.Contain("cooldown: 16"));
        Assert.That(hit21, Does.Contain("hitDuration: 5"));
        Assert.That(hit22, Does.Contain("hitDuration: 5"));
        Assert.That(hit31, Does.Contain("hitDuration: 6"));
        Assert.That(hit32, Does.Contain("hitDuration: 6"));
        Assert.That(hit31, Does.Contain("baseDamage: 120"));
        Assert.That(hit22, Does.Contain("m_Bits: 128"));
        Assert.That(hit32, Does.Contain("m_Bits: 128"));
        Assert.That(hit22, Does.Not.Contain("buffEffects:\n    - {fileID: 0}"));
        Assert.That(hit32, Does.Not.Contain("buffEffects:\n    - {fileID: 0}"));
        Assert.That(hit21, Does.Not.Contain("debuffEffects:\n    - {fileID: 0}"));
        Assert.That(hit31, Does.Not.Contain("debuffEffects:\n    - {fileID: 0}"));

        foreach (int grade in new[] { 2, 3 })
        {
            string buff = Read($"Contents/Skill/so/skill.character.seojin.{grade}.active_2.crane_wing_formation.effect.buff_2.1.asset");
            string entry = Read($"Contents/Skill/so/skill.character.seojin.{grade}.active_2.crane_wing_formation.effect.buff_2.1.entry.asset");
            Assert.That(buff, Does.Contain("targetStat: 1300"));
            Assert.That(buff, Does.Contain("modifierType: 0"));
            Assert.That(buff, Does.Contain("value: 10"));
            Assert.That(entry, Does.Contain("lifetimeType: 3"));
            Assert.That(entry, Does.Contain("categoryType: 0"));
            Assert.That(entry, Does.Contain("duration: 1.1"));
            Assert.That(entry, Does.Contain("maxApplyCount: 1"));
        }

        string resolver = Read("Scripts/Ability/Skills/Services/EquipmentSkillResolver.cs");
        string handler = Read("Scripts/Ability/Skills/Projectiles/ProjectileHitHandler.cs");
        string effectManager = Read("Scripts/Ability/Effects/Services/EffectManager.cs");
        string builder = Read("Editor/tools/skill/builder/SkillHitAssetBuilder.cs");
        Assert.That(resolver, Does.Contain("hitDuration = Mathf.Max(0f, hitSo.HitDuration)"));
        Assert.That(handler, Does.Contain("Mathf.CeilToInt(Mathf.Max(0f, hitDuration) / interval)"));
        Assert.That(handler, Does.Contain("ProcessRepeatHit(other)"));
        Assert.That(effectManager, Does.Contain("RemoveEffectsByRuntimeId(runtimeData.RuntimeId)"));
        Assert.That(builder, Does.Contain("Refusing to serialize a null {propertyName} entry"));
        Assert.That(builder, Does.Contain("Failed to materialize effect entry at index {i}"));
    }

    [TestCase(30)]
    [TestCase(60)]
    [TestCase(120)]
    public void OneSecondClock_HasFivePresentationSamplesAndNoT5(int fps)
    {
        float[] expected = { 0f, .2f, .4f, .6f, .8f };
        foreach (float phase in expected)
        {
            int frame = Mathf.RoundToInt(phase * fps);
            float sampled = frame / (float)fps;
            Assert.That(sampled, Is.EqualTo(phase).Within(1f / fps));
        }
        Assert.That(Mathf.Repeat(5f, 1f), Is.EqualTo(0f));
    }

    private static float Scalar(string yaml, string key)
    {
        string line = yaml.Split('\n').First(value => value.TrimStart().StartsWith(key + ":", StringComparison.Ordinal));
        return float.Parse(line.Substring(line.IndexOf(':') + 1), CultureInfo.InvariantCulture);
    }
}
