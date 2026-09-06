using System.IO;
using NUnit.Framework;
using Skill;
using UnityEngine;

public sealed class SkillSpecificBodyActionContractTests
{
    [Test]
    public void PlaybackProfileUsesExplicitDurationAndClipFallback()
    {
        AnimationClip clip = new AnimationClip { frameRate = 10f };
        clip.SetCurve(string.Empty, typeof(Transform), "localPosition.x",
            AnimationCurve.Linear(0f, 0f, .6f, 0f));

        SkillBodyActionPlaybackProfile fallback = new SkillBodyActionPlaybackProfile();
        Assert.That(fallback.ContactTime, Is.Zero);
        Assert.That(fallback.Duration(clip), Is.EqualTo(.6f).Within(.001f));

        SkillBodyActionPlaybackProfile configured = new SkillBodyActionPlaybackProfile();
        configured.ApplyEditorData(.2f, .8f);
        Assert.That(configured.ContactTime, Is.EqualTo(.2f).Within(.0001f));
        Assert.That(configured.Duration(clip), Is.EqualTo(.8f).Within(.0001f));
    }

    [Test]
    public void RuntimeBranchIsOptionalAndMobilityPresenterIsMutuallyExclusive()
    {
        string cast = File.ReadAllText(
            "Assets/Scripts/Ability/Skills/Definitions/cast/SkillCastSO.cs");
        string service = File.ReadAllText(
            "Assets/Scripts/Actor/Character/service/skill/ActiveSkillService.cs");
        string manager = File.ReadAllText(
            "Assets/Scripts/Actor/Character/CharacterSkillManager.cs");
        string animation = File.ReadAllText(
            "Assets/Scripts/Actor/Party/AnimationMono.cs");

        Assert.That(cast, Does.Contain("private AnimationClip bodyActionClip"));
        Assert.That(service, Does.Contain("CharacterAnimationResolver.ResolveSkillClip"));
        Assert.That(service, Does.Contain("castSo != null ? castSo.BodyActionClip : null"));
        Assert.That(service, Does.Contain("Body playback is presentation-only"));
        Assert.That(service, Does.Not.Contain("FireSkillAtBodyActionTiming"));
        Assert.That(manager, Does.Contain("!usesBodyActionClip && bodyPresenter.Begin"));
        Assert.That(animation, Does.Contain("PlaySkillBodyActionRoutine"));
        Assert.That(animation, Does.Contain("StopSkillBodyAction"));
    }

    [Test]
    public void BuilderLoadsPersistedClipAndDoesNotInventMissingAssets()
    {
        string builder = File.ReadAllText(
            "Assets/Editor/tools/skill/builder/SkillCastAssetBuilder.cs");
        Assert.That(builder, Does.Contain("AssetDatabase.LoadAssetAtPath<AnimationClip>(json.bodyActionClipPath)"));
        Assert.That(builder, Does.Contain("Body action clip does not exist"));
        Assert.That(builder, Does.Not.Contain("CreateInstance<AnimationClip>"));
        Assert.That(builder, Does.Contain("ImportAssetOptions.ForceSynchronousImport"));
        Assert.That(builder, Does.Contain("return AssetDatabase.LoadAssetAtPath<SkillCastSO>(assetPath)"));
    }

    [Test]
    public void SeojinManualActionsBindOnlyToExistingGradeSlots()
    {
        string g1 = File.ReadAllText("Assets/Contents/Character/so/character_seojin_1.asset");
        Assert.That(g1, Does.Contain("guid: 16567e50dfa6ac9a6c922e6e18102171"));
        Assert.That(g1, Does.Contain("guid: 4d77232be803c944db49a2a1398b8b33"));

        for (int grade = 1; grade <= 3; grade++)
        {
            string swift = File.ReadAllText($"Assets/Contents/Skill/json/skill.character.seojin.{grade}.active_4.swift_step.json");
            Assert.That(swift, Does.Contain(
                "Assets/AnimationClips/Character/character.seojin.1/character.seojin.1.body_action.dash.user-manual.anim"));
        }

        Assert.That(File.Exists("Assets/Contents/Skill/json/skill.character.seojin.1.active_2.crane_wing_formation.json"), Is.False);
        Assert.That(File.Exists("Assets/Contents/Skill/json/skill.character.seojin.2.active_2.crane_wing_formation.json"), Is.True);
        Assert.That(File.Exists("Assets/Contents/Skill/json/skill.character.seojin.3.active_2.crane_wing_formation.json"), Is.True);
        Assert.That(File.Exists("Assets/Contents/Skill/json/skill.character.seojin.1.active_3.turtle_ship_assault.json"), Is.False);
        Assert.That(File.Exists("Assets/Contents/Skill/json/skill.character.seojin.2.active_3.turtle_ship_assault.json"), Is.False);
        Assert.That(File.Exists("Assets/Contents/Skill/json/skill.character.seojin.3.active_3.turtle_ship_assault.json"), Is.True);
    }
}
