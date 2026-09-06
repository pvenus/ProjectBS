using System.IO;
using System.Linq;
using Character;
using Npc.Service;
using NUnit.Framework;
using Skill;
using UnityEditor;
using UnityEngine;

public sealed class Episode1Npc2AnimationCastingContractTests
{
    private sealed class Spec
    {
        public string Id;
        public string Skill;
        public float Cast;
        public float Contact;
        public float Recovery;
    }

    private static readonly Spec[] Specs =
    {
        new Spec { Id="character.black_cloth_raider.1", Skill="skill.character.black_cloth_raider.1.basic_attack.black_knife_cut", Cast=.90f, Contact=.12f, Recovery=.34f },
        new Spec { Id="character.chain_dragger_raider.1", Skill="skill.character.chain_dragger_raider.1.basic_attack.chain_swing", Cast=1.35f, Contact=.18f, Recovery=.45f }
    };

    [Test]
    public void Complete48AndMaterializedGraphArePresentAndExact()
    {
        foreach (Spec spec in Specs)
        {
            string imageRoot = $"Assets/ImagesGenerated/Character/animation/{spec.Id}/v2";
            Assert.That(Directory.GetFiles(imageRoot, "*.png", SearchOption.AllDirectories).Length, Is.EqualTo(24));
            foreach (string action in new[] { "idle", "move", "basicattack", "death", "stun", "impact_neon_v1" })
                Assert.That(Directory.GetFiles(Path.Combine(imageRoot, action), "*.png").Length,
                    Is.EqualTo(4), $"action must be exact4: {spec.Id}/{action}");

            string safe = spec.Id.Replace('.', '_');
            CharacterSO character = AssetDatabase.LoadAssetAtPath<CharacterSO>($"Assets/Contents/Character/so/{safe}.asset");
            Assert.That(character, Is.Not.Null);
            Assert.That(character.AnimationProfile, Is.Not.Null);
            Assert.That(character.AnimationProfile.SchemaVersion, Is.EqualTo(2));
            foreach (CharacterAnimationSlot slot in new[] { CharacterAnimationSlot.Idle, CharacterAnimationSlot.Move,
                         CharacterAnimationSlot.BasicAttack, CharacterAnimationSlot.Death, CharacterAnimationSlot.AttackDisabledCc })
                Assert.That(character.AnimationProfile.TryGetState(slot, out CharacterAnimationStateEntry entry) && entry != null,
                    Is.True, $"missing state {spec.Id}/{slot}");

            EquipmentSkillSO skill = AssetDatabase.LoadAssetAtPath<EquipmentSkillSO>($"Assets/Contents/Skill/so/{spec.Skill}.asset");
            Assert.That(skill, Is.Not.Null);
            Assert.That(skill.CastSo.CastTime, Is.EqualTo(spec.Cast).Within(.0001f));
            Assert.That(skill.CastSo.BodyActionClip, Is.Not.Null);
            Assert.That(skill.CastSo.SnapshotTargetPointOnCast, Is.True);
            Assert.That(skill.CastSo.BodyActionPlayback.ContactTime, Is.EqualTo(spec.Contact).Within(.0001f));
            Assert.That(skill.CastSo.BodyActionPlayback.Duration(skill.CastSo.BodyActionClip),
                Is.EqualTo(spec.Recovery).Within(.0001f));
            Assert.That(skill.BaseProfileSo.RendererScale, Is.EqualTo(.18f).Within(.0001f));

            Assert.That(skill.BaseVisualSo.ProjectileVisualType,
                Is.EqualTo(ProjectileVisualType.None));
            Assert.That(skill.BaseVisualSo.AnimationClips, Is.Empty);
            Assert.That(skill.BaseVisualSo.AnimationVfxProfile, Is.Null);
        }
    }

    [Test]
    public void Exact2BasicAttackImpactPresentationIsIntentionalNoneWithoutSuppressingGameplay()
    {
        foreach (Spec spec in Specs)
        {
            string jsonPath = $"Assets/Contents/Skill/json/{spec.Skill}.json";
            string json = File.ReadAllText(jsonPath);
            Assert.That(json, Does.Contain("\"projectileVisualType\":  \"None\""));
            Assert.That(json, Does.Not.Contain("impact.neon.v1.anim"));

            EquipmentSkillSO skill = AssetDatabase.LoadAssetAtPath<EquipmentSkillSO>(
                $"Assets/Contents/Skill/so/{spec.Skill}.asset");
            Assert.That(skill, Is.Not.Null);
            Assert.That(skill.CastSo.BodyActionClip, Is.Not.Null,
                "body BasicAttack motion must remain bound");
            Assert.That(skill.HitSos, Is.Not.Empty,
                "intentional-none presentation must retain hit gameplay");
            Assert.That(skill.BaseVisualSo.ProjectileVisualType,
                Is.EqualTo(ProjectileVisualType.None));
        }

        string resolver = File.ReadAllText(
            "Assets/Scripts/Ability/Skills/Services/EquipmentSkillResolver.cs");
        Assert.That(resolver, Does.Contain(
            "projectileData.projectileVisualType == ProjectileVisualType.None"));
        Assert.That(resolver, Does.Contain(
            "projectileData.suppressVisual = projectileData.suppressVisual ||"));

        string builder = File.ReadAllText(
            "Assets/Editor/tools/skill/builder/SkillBaseVisualAssetBuilder.cs");
        Assert.That(builder, Does.Contain(
            "projectileVisualType == ProjectileVisualType.None"));
        Assert.That(builder, Does.Contain(
            "visualSo.DisableProjectilePresentationEditor()"));
    }

    [Test]
    public void RuntimeKeepsCastCommitAndImpactDelaySeparated()
    {
        string manager = File.ReadAllText("Assets/Scripts/Actor/Character/CharacterSkillManager.cs");
        string attack = File.ReadAllText("Assets/Scripts/Actor/Character/state/AttackTargetState.cs");
        string service = File.ReadAllText("Assets/Scripts/Actor/Character/service/skill/ActiveSkillService.cs");
        Assert.That(manager, Does.Contain("CastCommitted?.Invoke(runtime)"));
        Assert.That(manager, Does.Contain("BeginCastBodyPose(runtime?.sourceEquipment?.CastSo, caster)"));
        Assert.That(manager, Does.Contain("ReleaseCastBodyPose()"));
        Assert.That(attack, Does.Contain("currentSkillManager.CastCommitted += OnCastCommitted"));
        Assert.That(service, Does.Contain("FireSkillBodyActionTiming"));
        Assert.That(service, Does.Contain("yield return new WaitForSeconds(contactDelay)"));

        string animation = File.ReadAllText("Assets/Scripts/Actor/Party/AnimationMono.cs");
        Assert.That(animation, Does.Contain("HoldSkillBodyActionCastPose"));
        Assert.That(animation, Does.Contain("_holdingSkillCastPose && _currentClip == clip"));
        Assert.That(animation, Does.Contain("clip.SampleAnimation(gameObject, 0f)"));
        Assert.That(manager, Does.Contain("castTargetPointSnapshot = target != null"));
        Assert.That(manager, Does.Contain("castSo.SnapshotTargetPointOnCast"));
        Assert.That(manager, Does.Contain("FireSkillAtSnapshotPoint"));
        Assert.That(service, Does.Contain("StartSkillUseRoutine(\n                skillManager, runtime, caster, null, true, targetPoint)"));
        Assert.That(manager, Does.Contain("!skillService.CanCommitCast(this, runtime, caster)"));
        Assert.That(service, Does.Contain("CanFireRuntime(runtime, caster, skillManager, true)"));
        Assert.That(animation, Does.Contain("if (_holdingSkillCastPose && _currentClip == clip)"));
    }

    [Test]
    public void Exact2CastAndCooldownReadbackDriveGaugeBeforeUnchangedPostCommitContact()
    {
        EquipmentSkillSO blackKnife = AssetDatabase.LoadAssetAtPath<EquipmentSkillSO>(
            "Assets/Contents/Skill/so/skill.character.black_cloth_raider.1.basic_attack.black_knife_cut.asset");
        EquipmentSkillSO chainSwing = AssetDatabase.LoadAssetAtPath<EquipmentSkillSO>(
            "Assets/Contents/Skill/so/skill.character.chain_dragger_raider.1.basic_attack.chain_swing.asset");

        Assert.That(blackKnife.CastSo.CastTime, Is.EqualTo(.90f).Within(.0001f));
        Assert.That(blackKnife.CastSo.Cooldown, Is.EqualTo(2f).Within(.0001f));
        Assert.That(blackKnife.CastSo.BodyActionPlayback.ContactTime, Is.EqualTo(.12f).Within(.0001f));
        Assert.That(blackKnife.CastSo.CastTime + blackKnife.CastSo.BodyActionPlayback.ContactTime,
            Is.EqualTo(1.02f).Within(.0001f));
        Assert.That(chainSwing.CastSo.CastTime, Is.EqualTo(1.35f).Within(.0001f));
        Assert.That(chainSwing.CastSo.Cooldown, Is.EqualTo(2.66f).Within(.0001f));
        Assert.That(chainSwing.CastSo.BodyActionPlayback.ContactTime, Is.EqualTo(.18f).Within(.0001f));
        Assert.That(chainSwing.CastSo.CastTime + chainSwing.CastSo.BodyActionPlayback.ContactTime,
            Is.EqualTo(1.53f).Within(.0001f));

        string manager = File.ReadAllText(
            "Assets/Scripts/Actor/Character/CharacterSkillManager.cs");
        Assert.That(manager, Does.Contain("castPresentation.BeginPresentation(castTime)"));
        Assert.That(manager, Does.Contain("castWorldHud.BeginCast(castTime)"));
        Assert.That(manager, Does.Contain("castClock = new SkillCastPhaseClock(castTime)"));
        Assert.That(manager, Does.Contain("npcCastGroundCone?.SetProgress(castClock.Progress)"));
        Assert.That(manager.IndexOf("CastCommitted?.Invoke(runtime)"),
            Is.GreaterThan(manager.IndexOf("if (!FireSkillImmediate(runtime, caster, target, false))")));
    }

    [Test]
    public void Exact2CastUsesSnapshotGroundConeAndSuppressesOverheadHud()
    {
        string manager = File.ReadAllText(
            "Assets/Scripts/Actor/Character/CharacterSkillManager.cs");
        string cone = File.ReadAllText(
            "Assets/Scripts/Actor/Character/presentation/NpcCastGroundConeTelegraphMono.cs");

        Assert.That(cone, Does.Contain(
            "skill.character.black_cloth_raider.1.basic_attack.black_knife_cut"));
        Assert.That(cone, Does.Contain(
            "skill.character.chain_dragger_raider.1.basic_attack.chain_swing"));
        Assert.That(cone, Does.Contain(
            "Battle.BattlePresentationSortingPolicy.GroundTelegraph"));
        Assert.That(cone, Does.Contain("snapshotDirection.normalized"));
        Assert.That(cone, Does.Contain("Color.Lerp(FillStartColor, FillFullColor, progress)"));
        Assert.That(cone, Does.Contain("length = ResolveFootprintLength("));
        Assert.That(cone, Does.Contain("halfAngleDegrees = ResolveHalfAngleDegrees("));
        Assert.That(cone, Does.Contain("True sector: every fill triangle shares the exact caster apex"));
        Assert.That(cone, Does.Not.Contain("nearHalfWidth"));
        Assert.That(cone, Does.Not.Contain("CenterLineThickness"));
        Assert.That(cone, Does.Not.Contain("CenterStartColor"));
        Assert.That(cone, Does.Not.Contain(
            "AddLine(vertices, colors, triangles, origin, origin + direction * length"));

        Assert.That(manager, Does.Contain("NpcCastGroundConeTelegraphMono.Supports(runtime)"));
        Assert.That(manager, Does.Contain("castWorldHud?.HideAndReset()"));
        Assert.That(manager, Does.Contain("npcCastGroundCone.Begin("));
        Assert.That(manager, Does.Contain("castSo.Range,"));
        Assert.That(manager, Does.Contain("npcCastGroundCone?.SetProgress(castClock.Progress)"));
        Assert.That(manager, Does.Contain("npcCastGroundCone?.CompleteAndHide()"));
        Assert.That(manager, Does.Contain("npcCastGroundCone?.CancelAndHide()"));
        Assert.That(manager.IndexOf("BeginCastFacing(runtime, caster, target)"),
            Is.LessThan(manager.IndexOf("BeginCastBodyPose(runtime?.sourceEquipment?.CastSo, caster)")));
        Assert.That(manager, Does.Contain("RefreshCastFacing(caster)"));
        Assert.That(manager, Does.Contain("ScheduleCastFacingRelease()"));
        Assert.That(manager, Does.Contain("ReleaseCastFacingAfterRecovery"));
        Assert.That(manager, Does.Contain("ReleaseCastFacing();"));
    }

    [Test]
    public void NpcCastFacingLockMirrorsLeftAndRejectsMovementOverwriteUntilReleased()
    {
        GameObject actor = new("NpcCastFacingLockTest");
        try
        {
            SpriteRenderer renderer = actor.AddComponent<SpriteRenderer>();
            AnimationMono animation = actor.AddComponent<AnimationMono>();
            object owner = new();

            Assert.That(animation.AcquireFacingLock(owner, Vector2.left), Is.True);
            Assert.That(animation.CurrentDirection,
                Is.EqualTo(AnimationMono.DiagonalDirection.DownLeft));
            Assert.That(renderer.flipX, Is.True,
                "right-authored NPC body sprites must mirror for a left cast");

            animation.SetDirectionFromVector(Vector2.right);
            Assert.That(animation.CurrentDirection,
                Is.EqualTo(AnimationMono.DiagonalDirection.DownLeft),
                "movement/AI direction writes must not overwrite an owned cast facing");

            AnimationClip rightAuthoredClip = new();
            rightAuthoredClip.SetCurve(string.Empty, typeof(SpriteRenderer), "m_FlipX",
                AnimationCurve.Constant(0f, .2f, 0f));
            rightAuthoredClip.SampleAnimation(actor, .1f);
            Assert.That(renderer.flipX, Is.False,
                "NPC2 right-authored clips explicitly overwrite flipX while sampled");
            typeof(AnimationMono).GetMethod("LateUpdate",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic)
                ?.Invoke(animation, null);
            Assert.That(renderer.flipX, Is.True,
                "the visible renderer must receive the owned facing after clip sampling");
            Object.DestroyImmediate(rightAuthoredClip);

            Assert.That(animation.RefreshFacingLock(owner, Vector2.right), Is.True,
                "commit may realign after the caster crosses its fixed snapshot point");
            Assert.That(animation.CurrentDirection,
                Is.EqualTo(AnimationMono.DiagonalDirection.DownRight));
            Assert.That(renderer.flipX, Is.False);

            animation.ReleaseFacingLock(owner);
            animation.SetDirectionFromVector(Vector2.left);
            Assert.That(animation.CurrentDirection,
                Is.EqualTo(AnimationMono.DiagonalDirection.DownLeft));
        }
        finally
        {
            Object.DestroyImmediate(actor);
        }
    }

    [Test]
    public void GroundSectorUsesWorldSpaceApexSnapshotDirectionAndFixedFootprint()
    {
        GameObject owner = new("NpcCastGroundSectorWorldSpaceOwner");
        try
        {
            owner.transform.position = new Vector3(3f, -2f, 0f);
            owner.transform.rotation = Quaternion.Euler(0f, 0f, 37f);
            owner.transform.localScale = new Vector3(2.1f, .55f, 1f);
            NpcCastGroundConeTelegraphMono cone =
                owner.AddComponent<NpcCastGroundConeTelegraphMono>();
            Vector2 origin = owner.transform.position;
            Vector2 snapshot = new(.24f, .32f); // exact distance .40

            Assert.That(cone.Begin(origin, snapshot, .70f, .60f, 2f, .26666668f), Is.True);
            Transform presentation = owner.transform.Find("__NpcCastGroundConeTelegraph");
            Assert.That(presentation, Is.Not.Null);
            Assert.That(presentation.localPosition, Is.EqualTo(Vector3.zero));
            Assert.That(presentation.localRotation, Is.EqualTo(Quaternion.identity));
            Assert.That(presentation.localScale, Is.EqualTo(Vector3.one));

            Mesh mesh = presentation.GetComponent<MeshFilter>().sharedMesh;
            Assert.That(mesh, Is.Not.Null);
            Vector2 axis = snapshot.normalized;
            Vector3[] localVertices = mesh.vertices;
            float maxForward = float.MinValue;
            int apexCount = 0;
            foreach (Vector3 localVertex in localVertices)
            {
                Vector2 world = presentation.TransformPoint(localVertex);
                float forward = Vector2.Dot(world - origin, axis);
                maxForward = Mathf.Max(maxForward, forward);
                if (Vector2.Distance(world, origin) <= .0001f) apexCount++;
            }

            Assert.That(maxForward, Is.EqualTo(1.83333336f).Within(.001f),
                "sector endpoint includes spawn offset, targeting range and world hit radius");
            Assert.That(apexCount, Is.GreaterThanOrEqualTo(20),
                "all fill triangles must share the exact caster apex");

            Vector2 fixedAttackPoint = origin + snapshot;
            owner.transform.position += new Vector3(-.30f, .10f, 0f);
            Vector2 movedOrigin = owner.transform.position;
            cone.SetProgress(1f);
            localVertices = mesh.vertices;
            int movedApexCount = 0;
            Vector2 movedAxis = (fixedAttackPoint - movedOrigin).normalized;
            float movedMaxForward = float.MinValue;
            foreach (Vector3 localVertex in localVertices)
            {
                Vector2 world = presentation.TransformPoint(localVertex);
                if (Vector2.Distance(world, movedOrigin) <= .0001f) movedApexCount++;
                movedMaxForward = Mathf.Max(
                    movedMaxForward,
                    Vector2.Dot(world - movedOrigin, movedAxis));
            }

            Assert.That(movedApexCount, Is.GreaterThanOrEqualTo(20),
                "sector apex must follow the current caster root every sampled frame");
            Assert.That(movedMaxForward, Is.EqualTo(1.83333336f).Within(.001f),
                "knockback may rotate the sector but must never stretch cast-start radius");
        }
        finally
        {
            Object.DestroyImmediate(owner);
        }
    }

    [Test]
    public void Exact2ConeDimensionsIncludeSpawnOffsetAndWorldColliderWithoutGameplayMutation()
    {
        float blackRadius = .09166667f * 2f;
        float chainRadius = .26666668f * 2f;
        float blackLength = NpcCastGroundConeTelegraphMono.ResolveFootprintLength(
            .5f, .25f, blackRadius);
        float chainLength = NpcCastGroundConeTelegraphMono.ResolveFootprintLength(
            .6f, .7f, chainRadius);
        float blackKnifeHalfAngle =
            NpcCastGroundConeTelegraphMono.ResolveHalfAngleDegrees(blackLength, blackRadius);
        float chainSwingHalfAngle =
            NpcCastGroundConeTelegraphMono.ResolveHalfAngleDegrees(chainLength, chainRadius);

        Assert.That(blackLength, Is.EqualTo(.93333334f).Within(.000001f));
        Assert.That(chainLength, Is.EqualTo(1.83333336f).Within(.000001f));
        Assert.That(2f * blackLength * Mathf.Sin(blackKnifeHalfAngle * Mathf.Deg2Rad),
            Is.EqualTo(blackRadius * 2f).Within(.001f));
        Assert.That(2f * chainLength * Mathf.Sin(chainSwingHalfAngle * Mathf.Deg2Rad),
            Is.EqualTo(chainRadius * 2f).Within(.001f));
        Assert.That(chainLength, Is.GreaterThan(blackLength));
        Assert.That(chainSwingHalfAngle, Is.GreaterThan(blackKnifeHalfAngle));
        Assert.That(.09166667f, Is.EqualTo(.55f / 6f).Within(.000001f));
        Assert.That(.26666668f, Is.EqualTo(.8f / 3f).Within(.000001f));
        Assert.That(.09166667f * 2f, Is.EqualTo(1.10f / 6f).Within(.000001f),
            "projectile root scale 2 keeps the effective world radius at exact old/6");
        Assert.That(.26666668f * 2f, Is.EqualTo(1.60f / 3f).Within(.000001f));

        string blackKnife = File.ReadAllText(
            "Assets/Contents/Skill/json/skill.character.black_cloth_raider.1.basic_attack.black_knife_cut.json");
        string chainSwing = File.ReadAllText(
            "Assets/Contents/Skill/json/skill.character.chain_dragger_raider.1.basic_attack.chain_swing.json");
        Assert.That(blackKnife, Does.Contain("\"projectileColliderRadius\":  0.09166667"));
        Assert.That(blackKnife, Does.Contain("\"range\":  0.25"));
        Assert.That(chainSwing, Does.Contain("\"projectileColliderRadius\":  0.26666668"));
        Assert.That(chainSwing, Does.Contain("\"range\":  0.7"));
    }

    [Test]
    public void GroundConeRepairsPartialOrDestroyedPresentationComponentsIdempotently()
    {
        GameObject owner = new("NpcCastGroundConeTestOwner");
        try
        {
            GameObject partial = new("__NpcCastGroundConeTelegraph");
            partial.transform.SetParent(owner.transform, false);
            NpcCastGroundConeTelegraphMono cone =
                owner.AddComponent<NpcCastGroundConeTelegraphMono>();

            Assert.That(cone.Begin(Vector2.zero, Vector2.right, .5f, .5f, 2f, .18333334f), Is.True);
            Assert.That(partial.GetComponents<MeshFilter>().Length, Is.EqualTo(1));
            Assert.That(partial.GetComponents<MeshRenderer>().Length, Is.EqualTo(1));
            Assert.That(partial.GetComponent<MeshRenderer>().sortingLayerID, Is.Zero);
            Assert.That(partial.GetComponent<MeshRenderer>().sortingOrder,
                Is.EqualTo(Battle.BattlePresentationSortingPolicy.GroundTelegraph));
            Assert.That(Battle.BattlePresentationSortingPolicy.GroundTelegraph,
                Is.GreaterThan(Battle.BattlePresentationSortingPolicy.Background));
            Assert.That(Battle.BattlePresentationSortingPolicy.GroundTelegraph, Is.LessThan(0));

            Object.DestroyImmediate(partial.GetComponent<MeshFilter>());
            Assert.That(cone.Begin(Vector2.zero, Vector2.up, .5f, .5f, 2f, .18333334f), Is.True);
            Assert.That(partial.GetComponents<MeshFilter>().Length, Is.EqualTo(1));

            Object.DestroyImmediate(partial.GetComponent<MeshRenderer>());
            Assert.That(cone.Begin(Vector2.zero, Vector2.left, .5f, .5f, 2f, .18333334f), Is.True);
            Assert.That(partial.GetComponents<MeshRenderer>().Length, Is.EqualTo(1));
            Assert.That(partial.GetComponent<MeshRenderer>().sortingOrder,
                Is.EqualTo(Battle.BattlePresentationSortingPolicy.GroundTelegraph));
            Assert.That(owner.transform.Cast<Transform>().Count(
                child => child.name == "__NpcCastGroundConeTelegraph"), Is.EqualTo(1));

            Assert.That(cone.Begin(Vector2.zero, Vector2.right, .5f, .5f, 2f, .18333334f), Is.True);
            Assert.That(partial.GetComponents<MeshFilter>().Length, Is.EqualTo(1));
            Assert.That(partial.GetComponents<MeshRenderer>().Length, Is.EqualTo(1));
        }
        finally
        {
            Object.DestroyImmediate(owner);
        }
    }

    [Test]
    public void GroundConePresentationFailureBoundaryLeavesGameplayPathCallable()
    {
        string cone = File.ReadAllText(
            "Assets/Scripts/Actor/Character/presentation/NpcCastGroundConeTelegraphMono.cs");
        string manager = File.ReadAllText(
            "Assets/Scripts/Actor/Character/CharacterSkillManager.cs");

        Assert.That(cone, Does.Contain("!isActiveAndEnabled || !EnsurePresentationObjects()"));
        Assert.That(cone, Does.Contain("catch (System.Exception exception)"));
        Assert.That(cone, Does.Contain("gameplay continues"));
        Assert.That(manager, Does.Contain("npcCastGroundCone.Begin("));
        Assert.That(manager, Does.Not.Contain("if (!npcCastGroundCone.Begin("));
    }

    [Test]
    public void Exact2VisualScaleUsesRendererProxyWithoutChangingActorRoot()
    {
        Assert.That(NpcVisualScalePresentationMono.ResolveMultiplier(
            "character.black_cloth_raider.1"), Is.EqualTo(1.3982143f).Within(.000001f));
        Assert.That(NpcVisualScalePresentationMono.ResolveMultiplier(
            "character.chain_dragger_raider.1"), Is.EqualTo(1.35f).Within(.000001f));
        Assert.That(2.80f * .20f * 1.3982143f, Is.EqualTo(.783f).Within(.0001f));
        Assert.That(2.90f * .20f * 1.35f, Is.EqualTo(.783f).Within(.0001f));
        Assert.That(NpcVisualScalePresentationMono.ResolveMultiplier(
            "character.seojin.1"), Is.EqualTo(1f));

        GameObject actor = new("NpcVisualScalePresentationTest");
        try
        {
            actor.transform.localScale = Vector3.one * .2f;
            SpriteRenderer source = actor.AddComponent<SpriteRenderer>();
            NpcVisualScalePresentationMono presentation =
                actor.AddComponent<NpcVisualScalePresentationMono>();
            presentation.Configure("character.black_cloth_raider.1");

            Assert.That(actor.transform.localScale, Is.EqualTo(Vector3.one * .2f));
            Transform proxy = actor.transform.Find("__NpcVisualScaleProxy");
            Assert.That(proxy, Is.Not.Null);
            Assert.That(proxy.localScale.x, Is.EqualTo(1.3982143f).Within(.000001f));
            Assert.That(proxy.localScale.y, Is.EqualTo(proxy.localScale.x));
            Assert.That(source.enabled, Is.False);
            Assert.That(proxy.GetComponent<SpriteRenderer>().enabled, Is.True);

            presentation.enabled = false;
            Assert.That(source.enabled, Is.True);
            Assert.That(proxy.GetComponent<SpriteRenderer>().enabled, Is.False);
        }
        finally
        {
            Object.DestroyImmediate(actor);
        }
    }

    [Test]
    public void Exact2ImpactUsesIsolatedSourceReadableProfile()
    {
        const string profilePath =
            "Assets/Contents/Skill/vfx/vfx-npc2-impact-source-readable.asset";
        const string materialPath =
            "Assets/Contents/Skill/material/skill-animation-vfx-npc2-impact-source-readable.mat";
        SkillAnimationVfxProfileSO profile =
            AssetDatabase.LoadAssetAtPath<SkillAnimationVfxProfileSO>(profilePath);
        Assert.That(profile, Is.Not.Null);
        Assert.That(profile.ProfileId, Is.EqualTo("skillAnimationVfx.npc2ImpactSourceReadable.v1"));
        Assert.That(profile.Material, Is.Not.Null);
        Assert.That(AssetDatabase.GetAssetPath(profile.Material), Is.EqualTo(materialPath));
        Assert.That(profile.Material.shader.name, Is.EqualTo(
            SkillAnimationVfxMaterialAuthority.SourceReadableShaderName));
        Assert.That(profile.TintStrength, Is.Zero);
        Assert.That(profile.ColorShiftStrength, Is.Zero);
        Assert.That(profile.Desaturate, Is.Zero);
        Assert.That(profile.BodyOpacityGain, Is.Zero);
        Assert.That(profile.EmissionIntensity, Is.EqualTo(.06f).Within(.0001f));
        Assert.That(profile.FadeIn, Is.EqualTo(.01f).Within(.0001f));
        Assert.That(profile.Hold, Is.EqualTo(.20f).Within(.0001f));
        Assert.That(profile.FadeOut, Is.EqualTo(.03f).Within(.0001f));

        foreach (Spec spec in Specs)
        {
            EquipmentSkillSO skill = AssetDatabase.LoadAssetAtPath<EquipmentSkillSO>(
                $"Assets/Contents/Skill/so/{spec.Skill}.asset");
            Assert.That(skill.BaseVisualSo.AnimationVfxProfile, Is.SameAs(profile));
            Assert.That(skill.BaseProfileSo.RendererScale, Is.EqualTo(.18f).Within(.0001f));
        }

        string sharedProfile = File.ReadAllText(
            "Assets/Contents/Skill/vfx/vfx-impact-attack.asset");
        Assert.That(sharedProfile, Does.Contain("profileId: skillAnimationVfx.impactAttack.v1"));
        Assert.That(sharedProfile, Does.Contain("tintStrength: 0.1"));
        Assert.That(sharedProfile, Does.Contain("colorShiftStrength: 0.16"));
    }

    [Test]
    public void GeneratorNormalizesCanonicalAndLegacyBodyClipNames()
    {
        string dto = File.ReadAllText("Assets/Editor/tools/skill/builder/SkillCastAssetBuilder.cs");
        string generator = File.ReadAllText("Assets/Editor/tools/skill/EquipmentSkillJsonGenerator.cs");
        Assert.That(dto, Does.Contain("public string bodyActionClip;"));
        Assert.That(dto, Does.Contain("public string bodyActionClipPath;"));
        Assert.That(dto, Does.Contain("public string mobilityVfxClip;"));
        Assert.That(dto, Does.Contain("public string mobilityVfxClipPath;"));
        Assert.That(generator, Does.Contain("cast.bodyActionClipPath = cast.bodyActionClip"));
        Assert.That(generator, Does.Contain("cast.mobilityVfxClipPath = cast.mobilityVfxClip"));

        string npcMaterializer = File.ReadAllText(
            "Assets/Editor/tools/character/Episode1Npc2Complete48Materializer.cs");
        Assert.That(npcMaterializer, Does.Contain(
            "/Users/pvenus/imageforge-mcp/generated_image"));
        Assert.That(npcMaterializer, Does.Contain("frame_{i + 1:00}.png"));
        Assert.That(npcMaterializer, Does.Contain("File.Copy(source, target, true)"));
        Assert.That(npcMaterializer, Does.Contain("SnapshotTree("));
    }

    [Test]
    public void Exact2DeathSequenceFinishesAnimationBeforeDissolveAndDespawn()
    {
        string manager = File.ReadAllText("Assets/Scripts/Actor/Character/CharacterManager.cs");
        string animation = File.ReadAllText("Assets/Scripts/Actor/Party/AnimationMono.cs");
        string death = File.ReadAllText("Assets/Scripts/Actor/Character/service/CharacterDeathService.cs");

        Assert.That(manager, Does.Contain("character.black_cloth_raider.1"));
        Assert.That(manager, Does.Contain("character.chain_dragger_raider.1"));
        Assert.That(manager, Does.Contain("animationMono.PlayDeath(1f)"));
        Assert.That(manager, Does.Contain("yield return new WaitForSeconds(1f)"));
        Assert.That(manager, Does.Contain("return 1f + dissolveDuration"));
        Assert.That(manager.IndexOf("yield return new WaitForSeconds(1f)"),
            Is.LessThan(manager.IndexOf("shader.PlayDeathDissolve()")));
        Assert.That(animation, Does.Contain("PlayDeathDurationRoutine"));
        Assert.That(animation, Does.Contain("clip.length * Mathf.Clamp01(elapsed / duration)"));
        Assert.That(death, Does.Contain("if (isHandlingDeath)"));
        Assert.That(death, Does.Contain("DisablePhysics(context.rigidbody2D)"));
        Assert.That(death, Does.Contain("DisableColliders(context.colliders)"));
    }

    [Test]
    public void Exact2AcceptedAttackQueuesRecoveryBoundedTacticalRepositionOnly()
    {
        string pathing = File.ReadAllText("Assets/Scripts/Actor/NPC/NpcPathing.cs");
        Assert.That(pathing, Does.Contain("characterSkillManager.SkillUseSucceeded += HandleSkillUseSucceeded"));
        Assert.That(pathing, Does.Contain("NpcPostAttackTacticalRepositionState.Supports(equipmentId)"));
        Assert.That(pathing, Does.Contain("_postAttackPending = true"));
        Assert.That(pathing, Does.Contain("if (recovery.State == EnemyOffensiveRecoveryState.Ready)"));
        Assert.That(pathing, Does.Contain("if (!recovery.AllowsReposition)"));
        Assert.That(pathing, Does.Not.Contain("!characterManager.CanMove || !characterManager.CanUseSkill"));
        Assert.That(pathing.IndexOf("if (IsMovementLocked())"),
            Is.LessThan(pathing.IndexOf("TryHandleExact2PostAttackTacticalReposition(target)")));
        Assert.That(pathing, Does.Contain("Physics2D.CircleCastAll"));
        Assert.That(pathing, Does.Contain("if (owner != null) continue"));
        Assert.That(pathing, Does.Contain("_postAttackTacticalState.Exit()"));
        Assert.That(pathing, Does.Not.Contain("KitingRepositionState"));
    }

    [Test]
    public void Exact2TacticalChoiceIsDeterministicAntiRepeatAndSpawnLeashed()
    {
        Assert.That(NpcPostAttackTacticalRepositionState.Supports(Specs[0].Skill), Is.True);
        Assert.That(NpcPostAttackTacticalRepositionState.Supports(Specs[1].Skill), Is.True);
        Assert.That(NpcPostAttackTacticalRepositionState.Supports(
            "skill.character.other.1.basic_attack"), Is.False);

        for (uint seed = 0; seed < 64; seed++)
        {
            var first = NpcPostAttackTacticalRepositionState.SelectPattern(Specs[0].Skill, seed,
                NpcPostAttackTacticalRepositionState.Pattern.Hold,
                NpcPostAttackTacticalRepositionState.Pattern.Orbit);
            var repeated = NpcPostAttackTacticalRepositionState.SelectPattern(Specs[0].Skill, seed,
                NpcPostAttackTacticalRepositionState.Pattern.Hold,
                NpcPostAttackTacticalRepositionState.Pattern.Orbit);
            Assert.That(repeated, Is.EqualTo(first));
        }

        foreach (NpcPostAttackTacticalRepositionState.Pattern pattern in
                 System.Enum.GetValues(typeof(NpcPostAttackTacticalRepositionState.Pattern)))
        {
            Vector2 destination = NpcPostAttackTacticalRepositionState.ResolveDestination(
                Specs[0].Skill, pattern, 2, Vector2.zero, new Vector2(.4f, 0f), .7f, Vector2.zero);
            Assert.That(destination.magnitude,
                Is.LessThanOrEqualTo(NpcPostAttackTacticalRepositionState.SpawnLeashRadius + .0001f));
        }

        for (uint seed = 0; seed < 64; seed++)
        {
            var noThirdRepeat = NpcPostAttackTacticalRepositionState.SelectPattern(
                Specs[1].Skill, seed,
                NpcPostAttackTacticalRepositionState.Pattern.Orbit,
                NpcPostAttackTacticalRepositionState.Pattern.Orbit);
            Assert.That(noThirdRepeat,
                Is.Not.EqualTo(NpcPostAttackTacticalRepositionState.Pattern.Orbit));
        }

        Assert.That(NpcPostAttackTacticalRepositionState.ResolveSpeedMultiplier(
            Specs[0].Skill, NpcPostAttackTacticalRepositionState.Pattern.Retreat), Is.EqualTo(1.20f));
        Assert.That(NpcPostAttackTacticalRepositionState.ResolveSpeedMultiplier(
            Specs[1].Skill, NpcPostAttackTacticalRepositionState.Pattern.ForwardPressure), Is.EqualTo(1.25f));

        string blackJson = File.ReadAllText($"Assets/Contents/Skill/json/{Specs[0].Skill}.json");
        string chainJson = File.ReadAllText($"Assets/Contents/Skill/json/{Specs[1].Skill}.json");
        Assert.That(blackJson, Does.Contain("\"cooldown\":  2"));
        Assert.That(blackJson, Does.Contain("\"castTime\":  0.9"));
        Assert.That(chainJson, Does.Contain("\"cooldown\":  2.66"));
        Assert.That(chainJson, Does.Contain("\"castTime\":  1.35"));
    }
}
