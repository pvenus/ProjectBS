using System.Collections.Generic;
using Character;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class CharacterIdleLoopTimingContractTests
{
    private const float Duration = 2f;

    [Test]
    public void EveryCharacterIdleConsumerUsesUniformTwoSecondLoop()
    {
        HashSet<AnimationClip> idleClips = new();
        string[] characterGuids = AssetDatabase.FindAssets(
            "t:CharacterSO",
            new[] { "Assets/Contents/Character/so" });

        foreach (string guid in characterGuids)
        {
            CharacterSO character = AssetDatabase.LoadAssetAtPath<CharacterSO>(
                AssetDatabase.GUIDToAssetPath(guid));
            if (character == null)
            {
                continue;
            }

            foreach (CharacterAnimationClipEntry entry in character.AnimationClips)
            {
                int clipType = entry != null ? (int)entry.clipType : -1;
                if (entry != null && entry.clip != null &&
                    clipType >= (int)CharacterAnimationClipType.IdleUpRight &&
                    clipType <= (int)CharacterAnimationClipType.IdleDownLeft)
                {
                    idleClips.Add(entry.clip);
                }
            }

            if (character.AnimationProfile != null &&
                character.AnimationProfile.TryGetState(
                    CharacterAnimationSlot.Idle,
                    out CharacterAnimationStateEntry idleState))
            {
                foreach (CharacterAnimationClipEntry entry in idleState.DirectionalClips)
                {
                    if (entry != null && entry.clip != null)
                    {
                        idleClips.Add(entry.clip);
                    }
                }
            }
        }

        Assert.That(idleClips.Count, Is.EqualTo(68),
            "The exact current CharacterSO primary+legacy-fallback idle inventory changed.");

        EditorCurveBinding spriteBinding = EditorCurveBinding.PPtrCurve(
            string.Empty,
            typeof(SpriteRenderer),
            "m_Sprite");
        foreach (AnimationClip clip in idleClips)
        {
            ObjectReferenceKeyframe[] keys =
                AnimationUtility.GetObjectReferenceCurve(clip, spriteBinding);
            Assert.That(keys, Is.Not.Null, clip.name);
            Assert.That(keys.Length, Is.GreaterThan(0), clip.name);
            Assert.That(AnimationUtility.GetAnimationClipSettings(clip).loopTime,
                Is.True, clip.name);
            Assert.That(clip.length, Is.EqualTo(Duration).Within(.0001f), clip.name);

            float expectedFrameDuration = Duration / keys.Length;
            for (int i = 0; i < keys.Length; i++)
            {
                Assert.That(keys[i].time,
                    Is.EqualTo(i * expectedFrameDuration).Within(.0001f),
                    $"{clip.name} frame {i}");
            }
        }
    }

    [Test]
    public void IdlePlaybackIsNotScaledByMovementSpeed()
    {
        string source = System.IO.File.ReadAllText(
            "Assets/Scripts/Actor/Party/AnimationMono.cs");
        Assert.That(source, Does.Contain(
            "_currentState == AnimationState.Move\n                    ? ResolveLocomotionPlaybackRate()\n                    : 1f;"));
    }

    [Test]
    public void GeneratorsUseTheSharedTwoSecondIdlePolicy()
    {
        string helper = System.IO.File.ReadAllText(
            "Assets/Editor/tools/helper/AnimationClipAssetHelper.cs");
        string genericBuilder = System.IO.File.ReadAllText(
            "Assets/Editor/tools/character/CharacterClipBuilder.cs");
        string npc2Builder = System.IO.File.ReadAllText(
            "Assets/Editor/tools/character/Episode1Npc2Complete48Materializer.cs");
        string seojinBuilder = System.IO.File.ReadAllText(
            "Assets/Editor/tools/character/SeojinG1AnimationV2PilotMaterializer.cs");

        Assert.That(helper, Does.Contain("CharacterIdleLoopDuration = 2f"));
        Assert.That(helper, Does.Contain("CreateOrUpdateCharacterIdleLoop"));
        Assert.That(genericBuilder, Does.Contain("CreateOrUpdateCharacterIdleLoop"));
        Assert.That(npc2Builder, Does.Contain("CreateOrUpdateCharacterIdleLoop"));
        Assert.That(seojinBuilder, Does.Contain("CreateOrUpdateCharacterIdleLoop"));
    }
}
