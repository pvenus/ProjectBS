using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class LayeredProjectilePresentationContractTests
{
    [Test]
    public void ProfileIsAdditiveAndRequiredGroundFieldControlsLegacyFallback()
    {
        string profile = File.ReadAllText(
            "Assets/Scripts/Ability/Skills/Definitions/animation/LayeredProjectilePresentationProfileSO.cs");
        string visual = File.ReadAllText(
            "Assets/Scripts/Ability/Skills/Definitions/animation/BaseVisualSO.cs");

        Assert.That(profile, Does.Contain("projectile_layers_v1"));
        Assert.That(profile, Does.Contain("contactTime = .25f"));
        Assert.That(profile, Does.Contain("sweetOpen = .8f"));
        Assert.That(profile, Does.Contain("sweetClose = 1.1f"));
        Assert.That(profile, Does.Contain("sustainEnd = 1.5f"));
        Assert.That(profile, Does.Contain("HasRequiredGroundField"));
        Assert.That(profile, Does.Contain("entry.Required && entry.HasPresentation"));
        Assert.That(visual, Does.Contain("LayeredProjectilePresentationProfileSO layeredPresentationProfile"));
        Assert.That(visual, Does.Contain("LayeredPresentationProfile => layeredPresentationProfile"));
    }

    [Test]
    public void LayerChildrenArePresentationOnlyAndUseSnapshotWorldAnchor()
    {
        string controller = File.ReadAllText(
            "Assets/Scripts/Ability/Skills/Projectiles/LayeredProjectilePresentationController.cs");

        Assert.That(controller, Does.Contain("worldAnchor = runtimeData.spawnPosition"));
        Assert.That(controller, Does.Contain("layer.root.transform.position = worldAnchor"));
        Assert.That(controller, Does.Contain("root.AddComponent<SpriteRenderer>()"));
        Assert.That(controller, Does.Not.Contain("AddComponent<Collider"));
        Assert.That(controller, Does.Not.Contain("AddComponent<Rigidbody"));
        Assert.That(controller, Does.Contain("layer.entry.FollowCaster"));
        Assert.That(controller, Does.Contain("layer.entry.FollowTarget"));
    }

    [Test]
    public void IndependentLayerEndExtendsOnlyVisualLifetimeAndCleanupIsIdempotent()
    {
        string entity = File.ReadAllText(
            "Assets/Scripts/Ability/Skills/Projectiles/ProjectileEntity.cs");
        string controller = File.ReadAllText(
            "Assets/Scripts/Ability/Skills/Projectiles/LayeredProjectilePresentationController.cs");
        string visual = File.ReadAllText(
            "Assets/Scripts/Ability/Skills/Projectiles/ProjectileVisual.cs");

        Assert.That(entity, Does.Contain("layeredProfile.ResolveMaximumLayerEnd()"));
        Assert.That(entity, Does.Contain("data.minimumVisualLifetime = Mathf.Max("));
        Assert.That(controller, Does.Contain("layer.entry.StartTime + layer.entry.Lifetime"));
        Assert.That(controller, Does.Contain("private void OnDisable() => StopImmediate()"));
        Assert.That(controller, Does.Contain("private void OnDestroy() => StopImmediate()"));
        Assert.That(controller, Does.Contain("layers.Clear()"));
        Assert.That(visual, Does.Contain("if (layeredPresentationActive) return;"));
    }

    [Test]
    public void JangdanDungExact17GraphHasCanonicalTimingAndPersistedReferences()
    {
        const string profilePath =
            "Assets/Contents/Skill/so/skill.character.seojin.1.active_5.jangdan_dung.layered.v1.asset";
        const string visualPath =
            "Assets/Contents/Skill/so/skill.character.seojin.1.active_5.jangdan_dung.visual.asset";
        LayeredProjectilePresentationProfileSO profile =
            AssetDatabase.LoadAssetAtPath<LayeredProjectilePresentationProfileSO>(profilePath);
        BaseVisualSO visual = AssetDatabase.LoadAssetAtPath<BaseVisualSO>(visualPath);

        Assert.That(profile, Is.Not.Null);
        Assert.That(visual, Is.Not.Null);
        Assert.That(visual.LayeredPresentationProfile, Is.SameAs(profile));
        Assert.That(profile.ProfileId,
            Is.EqualTo("skill.character.seojin.1.active_5.jangdan_dung.layered.v1"));
        Assert.That(profile.Layers.Length, Is.EqualTo(4));
        Assert.That(profile.HasRequiredGroundField(), Is.True);
        Assert.That(profile.ResolveMaximumLayerEnd(), Is.EqualTo(1.5f).Within(.0001f));

        AssertLayer(profile.Layers[0], "ground-field", 0f, 1.5f, true, 4, .5f);
        AssertLayer(profile.Layers[1], "pull-flow", .25f, .75f, false, 6, .75f);
        AssertLayer(profile.Layers[2], "setup-pulse", .8f, .3f, false, 3, .3f);
        AssertLayer(profile.Layers[3], "finish-residue", 1.1f, .4f, false, 4, .4f);

        string json = File.ReadAllText(
            "Assets/Contents/Skill/json/skill.character.seojin.1.active_5.jangdan_dung.json");
        Assert.That(json, Does.Contain(
            "\"layeredPresentationProfile\":\"skill.character.seojin.1.active_5.jangdan_dung.layered.v1\""));
    }

    private static void AssertLayer(ProjectilePresentationLayerEntry layer,
        string id, float start, float lifetime, bool loop, int frameCount, float stop)
    {
        Assert.That(layer.LayerId, Is.EqualTo(id));
        Assert.That(layer.StartTime, Is.EqualTo(start).Within(.0001f));
        Assert.That(layer.Lifetime, Is.EqualTo(lifetime).Within(.0001f));
        Assert.That(layer.Loop, Is.EqualTo(loop));
        Assert.That(layer.WorldFixed, Is.True);
        Assert.That(layer.Clip, Is.Not.Null);
        Assert.That(AnimationUtility.GetObjectReferenceCurve(
            layer.Clip, EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite")).Length,
            Is.EqualTo(frameCount));
        Assert.That(AnimationUtility.GetAnimationClipSettings(layer.Clip).stopTime,
            Is.EqualTo(stop).Within(.0001f));
    }
}
