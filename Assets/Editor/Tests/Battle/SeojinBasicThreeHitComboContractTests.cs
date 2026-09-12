using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using ResourceTools.Skill;
using Skill;
using UnityEditor;
using UnityEngine;

public sealed class SeojinBasicThreeHitComboContractTests
{
    private const string BasicAttackFrameFolder =
        "Assets/ImagesGenerated/Character/animation/character.seojin.1/basic_attack";

    private static readonly string[] Paths =
    {
        "Assets/Contents/Skill/json/skill.character.seojin.1.basic_attack.basic_attack.json",
        "Assets/Contents/Skill/json/skill.character.seojin.2.basic_attack.basic_attack.json",
        "Assets/Contents/Skill/json/skill.character.seojin.3.basic_attack.basic_attack.json"
    };

    [Test]
    public void GradeOneAppliesPointFiveRootsThenOneSecondGatherFinisherRoot()
    {
        string json = File.ReadAllText(Paths[0]);
        Assert.That(Count(json, "\"statType\": \"RootDuration\""), Is.EqualTo(3));
        Assert.That(Count(json, "\"value\": 0.5"), Is.EqualTo(2));
        Assert.That(Count(json, "\"value\": 1.0"), Is.EqualTo(1));
        Assert.That(json, Does.Not.Contain("\"effectType\": \"Knockback\""));
        Assert.That(json, Does.Contain("\"gatherDistance\": 0.3"));
        Assert.That(json, Does.Contain("\"gatherDuration\": 0.12"));
        Assert.That(json, Does.Contain("\"gatherStopRadius\": 0.35"));
        Assert.That(json, Does.Contain("\"gatherBossHardCap\": 0.15"));

        for (int step = 0; step < 3; step++)
        {
            string effectId = $"skill.character.seojin.1.basic_attack.basic_attack.combo.{step}.effect.debuff.1";
            string effect = File.ReadAllText($"Assets/Contents/Skill/so/{effectId}.asset");
            Assert.That(effect, Does.Contain("type: {class: StatModifierEffectConfig"));
            Assert.That(effect, Does.Contain("targetStat: 1347"));
            Assert.That(effect, Does.Contain(step < 2 ? "value: 0.5" : "value: 1"));
            Assert.That(effect, Does.Contain(step < 2 ? "rootHardCap: 0.5" : "rootHardCap: 1"));
        }

        for (int grade = 1; grade < Paths.Length; grade++)
        {
            string untouchedJson = File.ReadAllText(Paths[grade]);
            Assert.That(untouchedJson, Does.Contain(
                "\"statType\": \"RootDuration\",\n              \"modifierType\": \"Flat\",\n              \"value\": 0.2"),
                $"G{grade + 1} is outside this G1-only correction");
        }
    }

    [Test]
    public void CanonicalBodyChoreographyIsProxyOnlyBoundedAndExact3Enabled()
    {
        string animation = File.ReadAllText("Assets/Scripts/Actor/Party/AnimationMono.cs");
        string service = File.ReadAllText(
            "Assets/Scripts/Actor/Character/service/skill/ActiveSkillService.cs");
        Assert.That(animation, Does.Contain("RestartCanonicalComboChoreography"));
        Assert.That(animation, Does.Contain("attackClip.SampleAnimation(gameObject, 0f)"),
            "canonical attack F0 must be sampled before the proxy hides the idle renderer");
        Assert.That(animation, Does.Contain("BeginComboPresentation(null, true, true)"));
        Assert.That(animation, Does.Contain("private void LateUpdate()"));
        Assert.That(animation, Does.Contain("_comboPresentationRenderer.sprite = targetSpriteRenderer.sprite"),
            "proxy must mirror the currently sampled attack sprite rather than a frozen snapshot");
        Assert.That(animation, Does.Contain("attackClip.SampleAnimation(gameObject, attackClip.length * normalized)"));
        Assert.That(animation, Does.Contain("if (!_comboMirrorsCanonicalAnimation)"),
            "canonical attack recovery must not overwrite the final sampled sprite with the begin snapshot");
        Assert.That(animation, Does.Contain("__ComboBodyPresentationProxy"));
        Assert.That(animation, Does.Contain("new Vector2(.145f, -.02f)"));
        Assert.That(animation, Does.Contain("new Vector2(1.04f, 1.04f)"));
        Assert.That(animation, Does.Contain("new Vector2(1.07f, 1.07f)"));
        Assert.That(animation, Does.Contain("new Vector2(1.08f, 1.08f)"));
        Assert.That(animation, Does.Contain("new Vector2(1.05f, 1.05f)"));
        Assert.That(animation, Does.Contain("new Vector2(1.10f, 1.10f)"));
        Assert.That(animation, Does.Not.Contain("new Vector2(1.20f, .84f)"));
        Assert.That(animation, Does.Not.Contain("new Vector2(1.18f, .86f)"));
        Assert.That(animation, Does.Contain("Vector2.Lerp(recoveryScale, Vector2.one, t)"));
        Assert.That(animation, Does.Contain("Mathf.Lerp(recoveryRotation, 0f, t)"));
        Assert.That(animation, Does.Contain("proxy.localRotation = Quaternion.Euler"));
        Assert.That(animation, Does.Contain("transform.localRotation = Quaternion.identity"));
        Assert.That(animation, Does.Not.Contain("Instantiate(targetSpriteRenderer"));
        Assert.That(service, Does.Contain("UseCanonicalBodyChoreography"));
        Assert.That(service, Does.Contain("step.HitTime - step.StartTime"));
        Assert.That(service, Does.Contain("step.RecoveryEnd - step.HitTime"));

        for (int grade = 0; grade < Paths.Length; grade++)
        {
            string json = File.ReadAllText(Paths[grade]);
            Assert.That(json, Does.Contain("\"useCanonicalBodyChoreography\": true"));
            string yaml = File.ReadAllText(
                $"Assets/Contents/Skill/so/skill.character.seojin.{grade + 1}.basic_attack.basic_attack.asset");
            Assert.That(yaml, Does.Contain("useCanonicalBodyChoreography: 1"));
        }
    }

    [Test]
    public void ComboProxyAcquireRepairsMissingRendererAndDisablesDuplicateWithoutThrowing()
    {
        var root = new GameObject("combo-proxy-test");
        try
        {
            SpriteRenderer canonical = root.AddComponent<SpriteRenderer>();
            global::Character.AnimationMono animation = root.AddComponent<global::Character.AnimationMono>();
            var stale = new GameObject("__ComboBodyPresentationProxy");
            stale.transform.SetParent(canonical.transform, false);

            MethodInfo acquire = typeof(global::Character.AnimationMono).GetMethod(
                "AcquireComboPresentationRenderer",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(acquire, Is.Not.Null);
            SpriteRenderer repaired = (SpriteRenderer)acquire.Invoke(animation, null);
            Assert.That(repaired, Is.Not.Null);
            Assert.That(stale.GetComponent<global::Character.ComboBodyPresentationProxyMarker>(), Is.Not.Null);

            UnityEngine.Object.DestroyImmediate(repaired);
            SpriteRenderer repairedAgain = (SpriteRenderer)acquire.Invoke(animation, null);
            Assert.That(repairedAgain, Is.Not.Null, "destroyed component must be reacquired next attack");

            var duplicate = new GameObject("__ComboBodyPresentationProxy");
            duplicate.transform.SetParent(canonical.transform, false);
            duplicate.AddComponent<global::Character.ComboBodyPresentationProxyMarker>();
            duplicate.AddComponent<SpriteRenderer>();
            SpriteRenderer selected = (SpriteRenderer)acquire.Invoke(animation, null);
            Assert.That(selected, Is.SameAs(repairedAgain));
            Assert.That(duplicate.activeSelf, Is.False, "deterministic extra proxy is disabled, not broadly deleted");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
        }
    }

    [Test]
    public void ComboRendererCopyIsNullSafeForCanonicalFallback()
    {
        var root = new GameObject("combo-copy-null-test");
        try
        {
            global::Character.AnimationMono animation = root.AddComponent<global::Character.AnimationMono>();
            MethodInfo copy = typeof(global::Character.AnimationMono).GetMethod(
                "CopyRendererPresentation",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(copy, Is.Not.Null);
            Assert.That(copy.Invoke(animation, new object[] { null, null }), Is.EqualTo(false));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
        }
    }

    [Test]
    public void Continuous18BodyScaffoldIsOptionalAndUsesOnePlaybackOwner()
    {
        string definition = File.ReadAllText(
            "Assets/Scripts/Ability/Skills/Definitions/equipment/EquipmentSkillSO.cs");
        string generator = File.ReadAllText(
            "Assets/Editor/tools/skill/EquipmentSkillJsonGenerator.cs");
        string service = File.ReadAllText(
            "Assets/Scripts/Actor/Character/service/skill/ActiveSkillService.cs");
        string animation = File.ReadAllText(
            "Assets/Scripts/Actor/Party/AnimationMono.cs");

        Assert.That(definition, Does.Contain("segmentedBodyActionClip"));
        Assert.That(definition, Does.Contain("HasBodySegment(0, 5"));
        Assert.That(definition, Does.Contain("HasBodySegment(6, 11"));
        Assert.That(definition, Does.Contain("HasBodySegment(12, 17"));
        Assert.That(generator, Does.Contain("segmentedBodyFrameCount"));
        Assert.That(generator, Does.Contain("bodySegmentStartFrame"));
        Assert.That(service, Does.Contain("continuousAnimation?.RestartContinuousComboAction"));
        Assert.That(service, Does.Contain("step.ComboIndex < 2"));
        Assert.That(service, Does.Contain("continuousBody && step.ComboIndex == 2"));
        Assert.That(service, Does.Contain("SynchronizeContinuousComboContact"));
        Assert.That(animation, Does.Contain("int contactFrame = step.BodySegmentStartFrame + 4"));
        Assert.That(animation, Does.Contain("_continuousComboMinimumNormalizedFrame"));
        Assert.That(service, Does.Contain("SuppressVisual = suppressVisual"));
        string projectileVisual = File.ReadAllText(
            "Assets/Scripts/Ability/Skills/Projectiles/ProjectileVisual.cs");
        string projectileFactory = File.ReadAllText(
            "Assets/Scripts/Ability/Skills/Services/ProjectileFactory.cs");
        Assert.That(projectileVisual, Does.Contain("if (data.suppressVisual)"));
        Assert.That(projectileVisual, Does.Contain("if (spriteRenderer != null) spriteRenderer.enabled = false"));
        Assert.That(projectileVisual, Does.Contain("if (!initialized || IsVisualSuppressed())"));
        Assert.That(projectileVisual, Does.Contain("runtimeData != null && runtimeData.suppressVisual"));
        Assert.That(projectileFactory, Does.Contain("suppressVisual = source.suppressVisual"),
            "factory instance cloning must not drop the hit1/hit2 visual suppression flag");
        Assert.That(projectileFactory, Does.Contain("comboToken = source.comboToken"));
        Assert.That(projectileFactory, Does.Contain("comboIndex = source.comboIndex"));
        Assert.That(service, Does.Contain("bool deferredComboCooldown"));
        Assert.That(service, Does.Contain("cooldownOnThirdStepEntry && step.ComboIndex == 2"));
        Assert.That(service, Does.Contain("thirdStepCooldownCommitted = UseSkill(skillManager, runtime)"));
        Assert.That(service, Does.Contain("anyStepFired && completedSteps == steps.Length"));
        Assert.That(animation, Does.Not.Contain("PlayComboActionSegmentRoutine"));
        Assert.That(service, Does.Contain("if (!continuousBody)"));
        Assert.That(service, Does.Contain("Functional-pilot fallback: art absence never suppresses gameplay."));
        Assert.That(animation, Does.Contain("PlayContinuousComboActionRoutine"));
        Assert.That(animation, Does.Contain("startFrame + 4f"));
        Assert.That(animation, Does.Contain("StopComboAction"));

        string g1 = File.ReadAllText(Paths[0]);
        Assert.That(g1, Does.Contain("\"segmentedBodyActionClip\": \"character.seojin.1.basic_attack.combo.continuous18.body\""));
        Assert.That(g1, Does.Contain("\"segmentedBodyFrameCount\": 18"));
        Assert.That(g1, Does.Contain("\"bodySegmentStartFrame\": 0"));
        Assert.That(g1, Does.Contain("\"bodySegmentEndFrame\": 17"));
        Assert.That(g1, Does.Contain("\"useCanonicalBodyChoreography\": false"));

        for (int grade = 1; grade < Paths.Length; grade++)
        {
            string json = File.ReadAllText(Paths[grade]);
            Assert.That(json, Does.Not.Contain("segmentedBodyActionClip"));
            Assert.That(json, Does.Not.Contain("bodySegmentStartFrame"));
            Assert.That(json, Does.Contain("\"useCanonicalBodyChoreography\": true"));
        }
    }

    [Test]
    public void G1Continuous18Frames_PreserveApproved340PixelCanvas()
    {
        for (int i = 0; i < 18; i++)
        {
            string path = $"{BasicAttackFrameFolder}/frame-{i:00}.png";
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            Assert.That(texture, Is.Not.Null, $"missing frame: {path}");
            Assert.That(texture.width, Is.EqualTo(568), path);
            Assert.That(texture.height, Is.EqualTo(340), path);
        }
    }

    [Serializable]
    private sealed class Combo
    {
        public bool enabled;
        public bool inputDriven;
        public float duration;
        public float totalLungeCap;
        public Step[] steps;
    }

    [Serializable]
    private sealed class Step
    {
        public int comboIndex;
        public string hitId;
        public string bodyActionClip;
        public string vfxClip;
        public string visualClip;
        public string bodyPresentationCalibration;
        public string vfxPresentationCalibration;
        public float startTime;
        public float hitTime;
        public float activeEnd;
        public float recoveryEnd;
        public float postActionHoldTime;
        public float damageWeight;
        public float lungeDistance;
        public float nextComboActivationTime;
        public float gatherDistance;
        public float gatherDuration;
        public float gatherStopRadius;
        public float gatherBossHardCap;
    }

    [Test]
    public void GradeOneComboIsInputDrivenWithOneSecondRecoveryWindowsAndSegmentOnlyPlayback()
    {
        Combo combo = ParseCombo(Paths[0]);
        Assert.That(combo.inputDriven, Is.True);
        Assert.That(combo.steps[0].nextComboActivationTime, Is.EqualTo(1f));
        Assert.That(combo.steps[1].nextComboActivationTime, Is.EqualTo(1f));
        Assert.That(combo.steps[2].nextComboActivationTime, Is.Zero);

        Assert.That(combo.steps[0].postActionHoldTime, Is.EqualTo(.24f).Within(.0001f));
        Assert.That(combo.steps[1].postActionHoldTime, Is.Zero);
        Assert.That(combo.steps[2].postActionHoldTime, Is.Zero);

        for (int grade = 1; grade < Paths.Length; grade++)
        {
            Combo legacyCombo = ParseCombo(Paths[grade]);
            for (int i = 0; i < legacyCombo.steps.Length; i++)
            {
                Assert.That(legacyCombo.steps[i].postActionHoldTime, Is.Zero,
                    $"G{grade + 1} comboIndex{i} must retain the legacy zero-hold default");
            }
        }

        Assert.That(combo.steps[0].hitTime - combo.steps[0].startTime, Is.EqualTo(.16f).Within(.0001f));
        Assert.That(combo.steps[0].recoveryEnd - combo.steps[0].hitTime, Is.EqualTo(.05f).Within(.0001f));
        Assert.That(combo.steps[1].hitTime - combo.steps[1].startTime, Is.EqualTo(.16f).Within(.0001f));
        Assert.That(combo.steps[1].recoveryEnd - combo.steps[1].hitTime, Is.EqualTo(.10f).Within(.0001f));
        Assert.That(combo.steps[2].hitTime - combo.steps[2].startTime, Is.EqualTo(.12f).Within(.0001f));
        Assert.That(combo.steps[2].recoveryEnd - combo.steps[2].hitTime, Is.EqualTo(.20f).Within(.0001f));

        string service = File.ReadAllText(
            "Assets/Scripts/Actor/Character/service/skill/ActiveSkillService.cs");
        Assert.That(service, Does.Contain("combo.InputDriven"));
        Assert.That(service, Does.Contain("Time.time > progress.continuationExpiresAt"));
        Assert.That(service, Does.Contain("Time.time + step.NextComboActivationTime"));
        Assert.That(service, Does.Contain("stepIndex == 2"));
        Assert.That(service, Does.Contain("RestartContinuousComboSegment"));
        Assert.That(service, Does.Contain("step.PostActionHoldTime"));
        Assert.That(service, Does.Contain("bool manualDirectionLunge"));
        Assert.That(service, Does.Contain("manualAim?.Mode == SkillAimMode.Direction"));
        Assert.That(service, Does.Contain("if (manualDirectionLunge || targetedLunge)"));
        Assert.That(service, Does.Contain(
            "ApplyCollisionSafeLunge(caster, target, direction, step.LungeDistance)"),
            "input-driven Basic must retain its current per-input aim direction and collision-safe lunge path");
        Assert.That(service, Does.Contain("int count = body.Cast(direction.normalized, filter, hits, distance)"));
        Assert.That(service, Does.Contain("intendedTarget != null"));
        Assert.That(service, Does.Contain("intendedRoot != null"));
        Assert.That(service, Does.Not.Contain("collider.transform.root == intendedTarget.root"),
            "target-null manual Direction lunges must not dereference the optional target");
        Assert.That(service, Does.Contain(
            "body.MovePosition(body.position + direction.normalized * allowed)"));
        Assert.That(service, Does.Contain("movement?.StopAllMotion(stopKnockback: false)"),
            "the locomotion owner must be stopped before committing the physics-safe lunge");
        int holdWait = service.IndexOf(
            "yield return new WaitForSeconds(step.PostActionHoldTime)",
            StringComparison.Ordinal);
        int windowOpen = service.IndexOf(
            "progress.continuationExpiresAt = Time.time + step.NextComboActivationTime",
            StringComparison.Ordinal);
        Assert.That(holdWait, Is.GreaterThanOrEqualTo(0));
        Assert.That(windowOpen, Is.GreaterThan(holdWait),
            "the one-second continuation window must begin after the post-action hold");

        string input = File.ReadAllText(
            "Assets/Scripts/Actor/Character/Control/ManualControlCore.cs");
        Assert.That(input, Does.Contain("bool newBasicPress=input.AttackDown||(input.AttackHeld&&!wasHeld)"));
        Assert.That(input, Does.Contain("basicPressPending=true"));
        Assert.That(input, Does.Not.Contain("if(AttackHeld&&!basicPressPending)"),
            "the admitting physical hold must not be converted into a pending continuation while busy");
        Assert.That(input, Does.Contain("if(game.Busy)"));
        Assert.That(input, Does.Contain("an uninterrupted hold is sampled only when"));
        Assert.That(input, Does.Contain("if(!basicPressPending&&AttackHeld&&game.Ready(0))"),
            "held input may advance only after recovery/hold ends or restart after cooldown readiness");

        string animation = File.ReadAllText("Assets/Scripts/Actor/Party/AnimationMono.cs");
        Assert.That(animation, Does.Contain("PlayContinuousComboSegmentRoutine"));
        Assert.That(animation, Does.Contain("step.BodySegmentStartFrame / denominator"));
        Assert.That(animation, Does.Contain("step.BodySegmentEndFrame / denominator"));
        Assert.That(animation, Does.Contain("while (holdElapsed < postActionHoldTime)"));
        Assert.That(animation, Does.Contain("clip.SampleAnimation(gameObject, clip.length * end)"),
            "F5 must be re-sampled throughout the hold instead of exposing locomotion/idle");

        string materialized = File.ReadAllText(
            "Assets/Contents/Skill/so/skill.character.seojin.1.basic_attack.basic_attack.asset");
        Assert.That(materialized, Does.Contain("totalLungeCap: 0.72"));
        Assert.That(Count(materialized, "lungeDistance: 0.24"), Is.EqualTo(3));
    }

    [TestCase(0, 1.03f, .72f, .16f, .61f, .83f, .24f)]
    [TestCase(1, .78f, .42f, .14f, .38f, .65f, .14f)]
    [TestCase(2, .70f, .48f, .12f, .34f, .58f, .16f)]
    public void ExactTimingAndFailClosedVisualRolloutAreSerialized(
        int gradeIndex, float end, float cap, float hit0, float hit1, float hit2, float lunge)
    {
        Combo combo = ParseCombo(Paths[gradeIndex]);
        Assert.That(combo.enabled, Is.True, "functional pilot is atomically enabled for all three grades");
        Assert.That(combo.duration, Is.EqualTo(end).Within(.0001f));
        Assert.That(combo.totalLungeCap, Is.EqualTo(cap).Within(.0001f));
        Assert.That(combo.steps.Length, Is.EqualTo(3));
        Assert.That(combo.steps[0].hitTime, Is.EqualTo(hit0).Within(.0001f));
        Assert.That(combo.steps[1].hitTime, Is.EqualTo(hit1).Within(.0001f));
        Assert.That(combo.steps[2].hitTime, Is.EqualTo(hit2).Within(.0001f));
        if (gradeIndex == 0)
        {
            Assert.That(combo.steps[1].startTime - combo.steps[0].recoveryEnd,
                Is.EqualTo(.20f).Within(.0001f));
            Assert.That(combo.steps[2].startTime - combo.steps[1].recoveryEnd,
                Is.EqualTo(.10f).Within(.0001f));
        }
        Assert.That(combo.steps[0].damageWeight, Is.EqualTo(.25f).Within(.0001f));
        Assert.That(combo.steps[1].damageWeight, Is.EqualTo(.30f).Within(.0001f));
        Assert.That(combo.steps[2].damageWeight, Is.EqualTo(.45f).Within(.0001f));
        for (int i = 0; i < combo.steps.Length; i++)
        {
            Assert.That(combo.steps[i].comboIndex, Is.EqualTo(i));
            Assert.That(combo.steps[i].lungeDistance, Is.EqualTo(lunge).Within(.0001f));
            Assert.That(combo.steps[i].startTime, Is.LessThanOrEqualTo(combo.steps[i].hitTime));
            Assert.That(combo.steps[i].hitTime, Is.LessThanOrEqualTo(combo.steps[i].activeEnd));
            Assert.That(combo.steps[i].activeEnd, Is.LessThanOrEqualTo(combo.steps[i].recoveryEnd));
            Assert.That(combo.steps[i].hitId, Does.EndWith($".combo.{i}.hit"));
            Assert.That(combo.steps[i].visualClip, Is.Not.Empty);
            Assert.That(combo.steps[i].bodyActionClip, Is.Not.Empty,
                "COMPLETE54 atomically binds all shared body actions");
            Assert.That(combo.steps[i].vfxClip, Is.Not.Empty);
        }
    }

    [Test]
    public void G1PausesExactlyHalfASecondAfterFirstRecoveryOnly()
    {
        Combo combo = ParseCombo(Paths[0]);
        Assert.That(combo.steps[1].startTime - combo.steps[0].recoveryEnd,
            Is.EqualTo(.5f).Within(.0001f));
        Assert.That(combo.steps[1].hitTime - combo.steps[1].startTime,
            Is.EqualTo(.15f).Within(.0001f));
        Assert.That(combo.steps[2].startTime - combo.steps[1].recoveryEnd,
            Is.Zero.Within(.0001f));
        Assert.That(combo.steps[2].hitTime - combo.steps[1].hitTime,
            Is.EqualTo(.29f).Within(.0001f));

        string animation = File.ReadAllText(
            "Assets/Scripts/Actor/Party/AnimationMono.cs");
        Assert.That(animation, Does.Contain("elapsed < step.StartTime"));
        Assert.That(animation, Does.Contain("steps[i - 1].BodySegmentEndFrame"));
    }

    [Test]
    public void RuntimeUsesSelectedHitVisualOverrideAndOneSharedCriticalDecision()
    {
        string root = Directory.GetParent(Application.dataPath).FullName;
        string active = File.ReadAllText(Path.Combine(root,
            "Assets/Scripts/Actor/Character/service/skill/ActiveSkillService.cs"));
        string resolver = File.ReadAllText(Path.Combine(root,
            "Assets/Scripts/Ability/Skills/Services/EquipmentSkillResolver.cs"));
        Assert.That(active, Does.Contain("bool sharedCritical = RollComboCritical(caster);"));
        Assert.That(active, Does.Contain("step.ComboIndex"));
        Assert.That(active, Does.Contain("step.VfxClip"));
        Assert.That(active, Does.Contain("step.BodyActionClip"));
        Assert.That(active, Does.Contain("RestartComboAction"));
        Assert.That(active, Does.Contain("comboAnimation?.RestartAttack();"));
        Assert.That(resolver, Does.Contain("i != selectedHitIndex"));
        Assert.That(resolver, Does.Contain("criticalOverride.HasValue"));
        Assert.That(resolver, Does.Contain("profile.baseDamage *= resolvedDamageWeight;"));
        Assert.That(resolver, Does.Contain("ResolveHitMaxHitCount"));
        Assert.That(resolver, Does.Not.Contain("hitDto.maxHitCount = 1;"));
        Assert.That(active, Does.Contain("RestartAttack();"));
        Assert.That(active, Does.Contain("Vector2.Distance(caster.position, target.position) > 0.05f"));
        Assert.That(active, Does.Contain("useTriggers = false"));
        Assert.That(active, Does.Contain("movement?.StopAllMotion(stopKnockback: false)"));
        Assert.That(active, Does.Contain("collider.transform.root == intendedTarget.root"));
        Assert.That(active, Does.Contain("targetRange + (i > 0 ? 0.25f : 0f)"));
        Assert.That(active, Does.Contain("step.Hit"));
        Assert.That(active, Does.Contain("target = ResolveComboRetarget(caster, continuationRange, step.Hit);"));
        Assert.That(active, Does.Contain("if (!IsValidComboTarget(caster, target, step.Hit) ||"));
        Assert.That(active, Does.Contain("manager == null || !manager.IsTargetable"));
        Assert.That(active, Does.Contain("leftDistance.CompareTo(rightDistance)"));
        Assert.That(active, Does.Contain("left.transform.root.GetInstanceID().CompareTo"));
        Assert.That(resolver, Does.Contain("hitOverride != null"));

        string statModifier = File.ReadAllText(Path.Combine(root,
            "Assets/Scripts/Ability/Effects/Runtime/config/StatModifierEffectRuntime.cs"));
        Assert.That(statModifier, Does.Contain("ApplyRootAfterDisplacement"));
        Assert.That(statModifier, Does.Contain("movement.IsKnockingBack()"));
        Assert.That(statModifier, Does.Contain("Mathf.Clamp"));
        Assert.That(statModifier, Does.Contain("0.20f"));
        Assert.That(statModifier, Does.Contain("Mathf.Max(current, requested)"));

        string hitHandler = File.ReadAllText(Path.Combine(root,
            "Assets/Scripts/Ability/Skills/Projectiles/ProjectileHitHandler.cs"));
        Assert.That(hitHandler, Does.Contain("HashSet<int> hitTargetRoots"));
        Assert.That(hitHandler, Does.Contain("ResolveTargetRootId(other)"));
        Assert.That(hitHandler, Does.Contain("pendingContactTimes"));
        Assert.That(hitHandler, Does.Contain("contactA.CompareTo(contactB)"));
        Assert.That(hitHandler, Does.Contain("distanceA.CompareTo(distanceB)"));
        Assert.That(hitHandler, Does.Contain("ResolveTargetRootId(a).CompareTo(ResolveTargetRootId(b))"));
        Assert.That(hitHandler, Does.Contain("runtimeData.hit.maxHitCount > 1"));
        Assert.That(hitHandler, Does.Contain("yield return new WaitForFixedUpdate();"));

        string visual = File.ReadAllText(Path.Combine(root,
            "Assets/Scripts/Ability/Skills/Projectiles/ProjectileVisual.cs"));
        Assert.That(visual, Does.Contain("playableGraph.Evaluate(0f);"));
        Assert.That(visual, Does.Contain("PlayClip(clip);\n            animationVfx?.Play();"));
    }

    [Test]
    public void G1FinisherVfxUsesUniformSixFramesAcrossHalfDuration()
    {
        string root = Directory.GetParent(Application.dataPath).FullName;
        string clip = File.ReadAllText(Path.Combine(root,
            "Assets/AnimationClips/Skill/skill.character.seojin.1.basic_attack.basic_attack.combo.2.visual.loop.anim"));
        string json = File.ReadAllText(Path.Combine(root, Paths[0]));
        string skillAsset = File.ReadAllText(Path.Combine(root,
            "Assets/Contents/Skill/so/skill.character.seojin.1.basic_attack.basic_attack.asset"));
        string entity = File.ReadAllText(Path.Combine(root,
            "Assets/Scripts/Ability/Skills/Projectiles/ProjectileEntity.cs"));

        string[] frameTimes = { "time: 0\n", "time: 0.025\n", "time: 0.05\n", "time: 0.075\n", "time: 0.1\n", "time: 0.125\n" };
        foreach (string frameTime in frameTimes)
            Assert.That(Count(clip, frameTime), Is.EqualTo(1), frameTime);
        Assert.That(clip, Does.Contain("m_StopTime: 0.15"));
        Assert.That(clip, Does.Contain("m_LoopTime: 0"));
        Assert.That(json, Does.Contain("\"minimumVisualLifetime\": 0.15"));
        Assert.That(skillAsset, Does.Contain("minimumVisualLifetime: 0.15"));
        Assert.That(entity, Does.Contain("Destroy(gameObject, remainingVisualTime);"));
        Assert.That(entity, Does.Not.Contain("remainingVisualTime + renderSafety"));
    }

    [Test]
    public void DistinctBodyAndVfxRegistryPromotesOnlyAsACompleteExact3Set()
    {
        Combo[] combos = new Combo[Paths.Length];
        int boundBodies = 0;
        for (int grade = 0; grade < Paths.Length; grade++)
        {
            combos[grade] = ParseCombo(Paths[grade]);
            for (int step = 0; step < combos[grade].steps.Length; step++)
                if (!string.IsNullOrWhiteSpace(combos[grade].steps[step].bodyActionClip)) boundBodies++;
        }

        Assert.That(boundBodies == 0 || boundBodies == 9, Is.True,
            "partial grade/hit body registry promotion is forbidden");
        if (boundBodies == 0) return;

        for (int step = 0; step < 3; step++)
        {
            Assert.That(combos[1].steps[step].bodyActionClip,
                Is.EqualTo(combos[0].steps[step].bodyActionClip));
            Assert.That(combos[2].steps[step].bodyActionClip,
                Is.EqualTo(combos[0].steps[step].bodyActionClip));
        }

        var vfxNames = new System.Collections.Generic.HashSet<string>();
        for (int grade = 0; grade < combos.Length; grade++)
            for (int step = 0; step < combos[grade].steps.Length; step++)
                vfxNames.Add(combos[grade].steps[step].vfxClip);
        Assert.That(vfxNames.Count, Is.EqualTo(9));
    }

    [Test]
    public void DeadPrimaryRetargetsBeforeFacingLungeAnimationOrProjectile()
    {
        string root = Directory.GetParent(Application.dataPath).FullName;
        string active = File.ReadAllText(Path.Combine(root,
            "Assets/Scripts/Actor/Character/service/skill/ActiveSkillService.cs"));
        string manager = File.ReadAllText(Path.Combine(root,
            "Assets/Scripts/Actor/Character/CharacterManager.cs"));

        int immediateRetarget = active.IndexOf(
            "target = ResolveComboRetarget(caster, continuationRange, step.Hit);",
            StringComparison.Ordinal);
        int finalGuard = active.IndexOf(
            "if (!IsValidComboTarget(caster, target, step.Hit) ||",
            immediateRetarget + 1,
            StringComparison.Ordinal);
        int facing = active.IndexOf("ApplyAnimationDirection(caster, direction);", StringComparison.Ordinal);
        int animation = active.IndexOf("ResolveAnimation(caster)?.RestartAttack();", StringComparison.Ordinal);
        int projectile = active.IndexOf("bool fired = UseSkillOnce(", StringComparison.Ordinal);

        Assert.That(immediateRetarget, Is.GreaterThanOrEqualTo(0));
        Assert.That(finalGuard, Is.GreaterThan(immediateRetarget));
        Assert.That(facing, Is.GreaterThan(finalGuard));
        Assert.That(animation, Is.GreaterThan(finalGuard));
        Assert.That(projectile, Is.GreaterThan(animation));
        Assert.That(active, Does.Contain("roots.Sort((left, right) =>"));
        Assert.That(active, Does.Contain("mask.value & (1 << manager.gameObject.layer)"));
        Assert.That(Count(active,
            "target = ResolveComboRetarget(caster, continuationRange, step.Hit);"),
            Is.GreaterThanOrEqualTo(3));
        Assert.That(manager, Does.Contain("public bool IsTargetable => isActiveAndEnabled"));
        Assert.That(manager, Does.Contain("!isDying"));
        Assert.That(manager, Does.Contain("!runtimeData.isDead"));
    }

    [Test]
    public void Exact3SerializedAssetsAreAtomicallyEnabledWithGradeLocalFallback()
    {
        string root = Directory.GetParent(Application.dataPath).FullName;
        string[] hitGuids =
        {
            "f4e8d847859f34a05a7ef09379c1fa03",
            "f8afb408e743b4ff9a623d511e522172",
            "0facfa1ae3f7040a1b5ac71e82b70d33"
        };
        string[] clipGuids =
        {
            "11a81539e323648bcbc4d876eac27830",
            "3d6a22eaea28a43589a64b9c77993a6b",
            "7ceeea2d1758f436bb9dbcca9b172322"
        };
        string[] bodyClipGuids =
        {
            "994215e857c7b25fdd9d47b9097014af",
            "a903ea1a4024a5e1eaec3f60e63c2be3",
            "394f5ea110c3b4bc680b9b9c7be429ec"
        };
        string[,] freshVfxClipGuids =
        {
            { "dd7b58f805007239b84f3b9eefacda7c", "f1829639c23dd9280696619f9fd3caff" },
            { "543455762a3f6c672b4355c5436dc99a", "ab91469bee798e9e18112757a933f367" },
            { "c8fbf084613f66fd28be7a54f1affcf5", "b4d27071f6ac7a6e2fd30ef5962c72f7" }
        };
        string[] finisherHitGuids =
        {
            "a1000000000000000000000000000001",
            "b2000000000000000000000000000001",
            "c3000000000000000000000000000001"
        };
        string[,] earlyHitGuids =
        {
            { "a1000000000000000000000000000010", "a1000000000000000000000000000013" },
            { "b2000000000000000000000000000010", "b2000000000000000000000000000013" },
            { "c3000000000000000000000000000010", "c3000000000000000000000000000013" }
        };

        for (int grade = 1; grade <= 3; grade++)
        {
            string yaml = File.ReadAllText(Path.Combine(root,
                $"Assets/Contents/Skill/so/skill.character.seojin.{grade}.basic_attack.basic_attack.asset"));
            Assert.That(yaml, Does.Contain("comboProfile:\n    enabled: 1"));
            Assert.That(Count(yaml, $"hit: {{fileID: 11400000, guid: {hitGuids[grade - 1]}"), Is.EqualTo(0));
            Assert.That(Count(yaml, $"guid: {hitGuids[grade - 1]}, type: 2}}"), Is.EqualTo(1));
            Assert.That(Count(yaml, $"guid: {earlyHitGuids[grade - 1, 0]}, type: 2}}"), Is.EqualTo(2));
            Assert.That(Count(yaml, $"guid: {earlyHitGuids[grade - 1, 1]}, type: 2}}"), Is.EqualTo(2));
            Assert.That(Count(yaml, $"hit: {{fileID: 11400000, guid: {finisherHitGuids[grade - 1]}"), Is.EqualTo(1));
            Assert.That(Count(yaml, $"guid: {finisherHitGuids[grade - 1]}, type: 2}}"), Is.EqualTo(2));
            Assert.That(Count(yaml, $"visualClip: {{fileID: 7400000, guid: {clipGuids[grade - 1]}"), Is.EqualTo(1));
            for (int step = 0; step < 3; step++)
                Assert.That(Count(yaml, $"bodyActionClip: {{fileID: 7400000, guid: {bodyClipGuids[step]}"), Is.EqualTo(1));
            for (int step = 1; step < 3; step++)
                Assert.That(Count(yaml, $"visualClip: {{fileID: 7400000, guid: {freshVfxClipGuids[grade - 1, step - 1]}"), Is.EqualTo(1));

            for (int step = 0; step < 2; step++)
            {
                string assetPath =
                    $"Assets/Contents/Skill/so/skill.character.seojin.{grade}.basic_attack.basic_attack.combo.{step}.hit.asset";
                string meta = File.ReadAllText(Path.Combine(root, assetPath + ".meta"));
                Assert.That(meta, Does.Contain($"guid: {earlyHitGuids[grade - 1, step]}"));
                Assert.That(File.Exists(Path.Combine(root, assetPath)), Is.True);
                Assert.That(File.Exists(Path.Combine(root, assetPath.Replace(".hit.asset", ".hit.hit.asset"))), Is.False);
            }
        }
    }

    private static Combo ParseCombo(string relativePath)
    {
        string root = Directory.GetParent(Application.dataPath).FullName;
        string json = File.ReadAllText(Path.Combine(root, relativePath));
        MethodInfo parser = typeof(EquipmentSkillJsonGenerator).GetMethod(
            "ParseEquipmentSkillJson", BindingFlags.Static | BindingFlags.NonPublic);
        var data = (EquipmentSkillJsonGenerator.EquipmentSkillJson)parser.Invoke(null, new object[] { json });
        return JsonUtility.FromJson<Combo>(data.combo);
    }

    [Test]
    public void PragmaticCalibrationProfilesAreAtomicAndWithinAuthorizedBounds()
    {
        string root = Directory.GetParent(Application.dataPath).FullName;
        for (int grade = 0; grade < Paths.Length; grade++)
        {
            Combo combo = ParseCombo(Paths[grade]);
            for (int step = 0; step < 3; step++)
            {
                if (grade == 0)
                {
                    Assert.That(combo.steps[step].bodyPresentationCalibration, Is.Null.Or.Empty,
                        "G1 input-driven continuous18 uses canonical local scale 1");
                }
                else
                {
                    Assert.That(combo.steps[step].bodyPresentationCalibration,
                        Is.EqualTo($"skill.character.seojin.basic_attack.combo.body.hit{step}.calibration"));
                }
                if (step == 0)
                {
                    Assert.That(combo.steps[step].vfxPresentationCalibration, Is.Null.Or.Empty,
                        "accepted hit0 remains presentation-authority and is not recalibrated");
                }
                else
                {
                    Assert.That(combo.steps[step].vfxPresentationCalibration,
                        Is.EqualTo($"skill.character.seojin.{grade + 1}.basic_attack.combo.vfx.hit{step}.calibration"));
                }
            }
        }

        string[] profilePaths = Directory.GetFiles(
            Path.Combine(root, "Assets/Contents/Skill/so"),
            "*basic_attack.combo.*.calibration.asset");
        Assert.That(profilePaths.Length, Is.EqualTo(9));
        foreach (string profilePath in profilePaths)
        {
            string yaml = File.ReadAllText(profilePath);
            Assert.That(Count(yaml, "frameIndex:"), Is.EqualTo(6));
            Assert.That(yaml, Does.Not.Contain("- {frameIndex:"),
                "Unity-compatible serialized class arrays must use canonical block YAML");
            string assetPath = profilePath.Substring(root.Length + 1).Replace('\\', '/');
            SpritePresentationCalibrationProfileSO loaded =
                AssetDatabase.LoadAssetAtPath<SpritePresentationCalibrationProfileSO>(assetPath);
            Assert.That(loaded, Is.Not.Null, assetPath);
            Assert.That(loaded.Entries, Has.Length.EqualTo(6), assetPath);
            Assert.That(loaded.ProfileId, Is.EqualTo(Path.GetFileNameWithoutExtension(assetPath)));
            bool body = profilePath.Contains(".body.");
            foreach (MatchCapture entry in ParseCalibrationEntries(yaml))
            {
                Assert.That(entry.ScaleX, Is.InRange(body ? .5f : 1f / 6.5f, body ? 2.5f : 6.5f));
                Assert.That(entry.ScaleY, Is.InRange(body ? .5f : 1f / 6.5f, body ? 2.5f : 6.5f));
                if (body) Assert.That(entry.ScaleX, Is.EqualTo(entry.ScaleY).Within(.000001f));
            }
        }
    }

    [Test]
    public void CalibrationRuntimeUsesRenderOnlyTransformsAndRestoresThem()
    {
        string root = Directory.GetParent(Application.dataPath).FullName;
        string body = File.ReadAllText(Path.Combine(root, "Assets/Scripts/Actor/Party/AnimationMono.cs"));
        string projectile = File.ReadAllText(Path.Combine(root,
            "Assets/Scripts/Ability/Skills/Projectiles/ProjectileVisual.cs"));
        Assert.That(body, Does.Contain("__ComboBodyPresentationProxy"));
        Assert.That(body, Does.Contain("targetSpriteRenderer.enabled = false"));
        Assert.That(body, Does.Contain("targetSpriteRenderer.sprite = _comboBaselineSprite"));
        Assert.That(body, Does.Contain("EndComboPresentation();"));
        Assert.That(projectile, Does.Contain("presentationCalibration.TryResolve"));
        Assert.That(projectile, Does.Contain("baselineRendererLocalScale * effectiveRendererScale"));
        Assert.That(projectile, Does.Contain("rendererScaleTransform.localPosition = baselineRendererLocalPosition"));
        Assert.That(projectile, Does.Not.Contain("owner.transform.localScale"));
        Assert.That(body, Does.Not.Contain("readonly MaterialPropertyBlock _comboPropertyBlock = new"));
        Assert.That(projectile, Does.Not.Contain("readonly MaterialPropertyBlock presentationPropertyBlock = new"));
        Assert.That(body, Does.Contain("_comboPropertyBlock ??= new MaterialPropertyBlock()"));
        Assert.That(projectile, Does.Contain("presentationPropertyBlock ??= new MaterialPropertyBlock()"));
    }

    private readonly struct MatchCapture
    {
        public MatchCapture(float scaleX, float scaleY) { ScaleX = scaleX; ScaleY = scaleY; }
        public float ScaleX { get; }
        public float ScaleY { get; }
    }

    private static System.Collections.Generic.IEnumerable<MatchCapture> ParseCalibrationEntries(string yaml)
    {
        var regex = new System.Text.RegularExpressions.Regex(
            @"scale: \{x: (?<x>-?[0-9.]+), y: (?<y>-?[0-9.]+)\}");
        foreach (System.Text.RegularExpressions.Match match in regex.Matches(yaml))
        {
            yield return new MatchCapture(
                float.Parse(match.Groups["x"].Value, System.Globalization.CultureInfo.InvariantCulture),
                float.Parse(match.Groups["y"].Value, System.Globalization.CultureInfo.InvariantCulture));
        }
    }

    private static int Count(string text, string value)
    {
        int count = 0;
        int index = 0;
        while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }
        return count;
    }

    [Test]
    public void HitAssetNamingIsIdempotentForTerminalHitSuffix()
    {
        Assert.That(
            SkillHitAssetBuilder.NormalizeHitAssetName("skill.character.seojin.1.basic_attack.basic_attack.combo.0.hit"),
            Is.EqualTo("skill.character.seojin.1.basic_attack.basic_attack.combo.0.hit"));
        Assert.That(
            SkillHitAssetBuilder.NormalizeHitAssetName("skill.character.seojin.1.basic_attack.basic_attack.combo.0.hit.hit"),
            Is.EqualTo("skill.character.seojin.1.basic_attack.basic_attack.combo.0.hit"));
        Assert.That(
            SkillHitAssetBuilder.NormalizeHitAssetName("skill.character.seojin.1.basic_attack.basic_attack.combo.0.hit.hit.hit"),
            Is.EqualTo("skill.character.seojin.1.basic_attack.basic_attack.combo.0.hit"));
    }

    [Test]
    public void HitBuilderReturnsPersistedCanonicalObjectAndSecondRunKeepsGuid()
    {
        const string folder = "Assets/Editor/Tests/Temp/SeojinHitBuilderIdempotency";
        const string hitId = "skill.character.seojin.test.basic_attack.combo.0.hit";
        const string canonicalPath = folder + "/" + hitId + ".asset";
        const string duplicatePath = folder + "/" + hitId + ".hit.asset";

        try
        {
            var json = new HitJson
            {
                hitId = hitId,
                maxHitCount = 3,
                targetLayerMask = "Enemy"
            };

            SkillHitSO first = SkillHitAssetBuilder.CreateOrUpdate(json, folder) as SkillHitSO;
            Assert.That(first, Is.Not.Null);
            Assert.That(AssetDatabase.GetAssetPath(first), Is.EqualTo(canonicalPath));
            string firstGuid = AssetDatabase.AssetPathToGUID(canonicalPath);
            Assert.That(firstGuid, Is.Not.Empty);
            Assert.That(AssetDatabase.LoadAssetAtPath<SkillHitSO>(canonicalPath), Is.SameAs(first));
            Assert.That(AssetDatabase.LoadMainAssetAtPath(duplicatePath), Is.Null);
            string firstYaml = File.ReadAllText(canonicalPath);

            SkillHitSO second = SkillHitAssetBuilder.CreateOrUpdate(json, folder) as SkillHitSO;
            Assert.That(second, Is.Not.Null);
            Assert.That(AssetDatabase.GetAssetPath(second), Is.EqualTo(canonicalPath));
            Assert.That(AssetDatabase.AssetPathToGUID(canonicalPath), Is.EqualTo(firstGuid));
            Assert.That(AssetDatabase.LoadAssetAtPath<SkillHitSO>(canonicalPath), Is.SameAs(second));
            Assert.That(AssetDatabase.LoadMainAssetAtPath(duplicatePath), Is.Null);
            Assert.That(File.ReadAllText(canonicalPath), Is.EqualTo(firstYaml));
        }
        finally
        {
            AssetDatabase.DeleteAsset(folder);
        }
    }
}
