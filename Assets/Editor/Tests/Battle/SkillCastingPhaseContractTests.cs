using System.IO;
using Character;
using NUnit.Framework;
using UnityEngine;

public class SkillCastingPhaseContractTests
{
    [TestCase(-1f, 0f)]
    [TestCase(0f, 0f)]
    [TestCase(.25f, .25f)]
    public void CastTimeNormalizationPreservesLegacyInstantPath(float input, float expected)
    {
        Assert.That(CharacterSkillManager.NormalizeCastTime(input), Is.EqualTo(expected));
    }

    [Test]
    public void InvalidCastTimesFailSafelyToInstant()
    {
        Assert.That(CharacterSkillManager.NormalizeCastTime(float.NaN), Is.Zero);
        Assert.That(CharacterSkillManager.NormalizeCastTime(float.PositiveInfinity), Is.Zero);
        Assert.That(CharacterSkillManager.NormalizeCastTime(float.NegativeInfinity), Is.Zero);
    }

    [Test]
    public void ScaledClockDoesNotFireEarlyAndCommitsExactlyOnce()
    {
        CharacterSkillManager.SkillCastPhaseClock clock = new(.25f);
        Assert.That(clock.Tick(.1f), Is.False);
        Assert.That(clock.Progress, Is.EqualTo(.4f).Within(.0001f));
        Assert.That(clock.Tick(0f), Is.False, "paused scaled time must not advance");
        Assert.That(clock.Tick(.149f), Is.False);
        Assert.That(clock.Tick(.001f), Is.True);
        Assert.That(clock.Tick(1f), Is.False, "commit receipt must be exactly once");
    }

    [Test]
    public void RuntimeGateCommitsOnlyAfterScaledProgressAndRevalidation()
    {
        string source = File.ReadAllText(
            "Assets/Scripts/Actor/Character/CharacterSkillManager.cs");
        Assert.That(source, Does.Contain("!castClock.Tick(Time.deltaTime)"));
        Assert.That(source, Does.Contain("!CanContinueCasting(runtime, caster, target)"));
        Assert.That(source, Does.Contain("!skillService.CanCommitCast(this, runtime, caster)"));
        Assert.That(source, Does.Contain("FireSkillImmediate(runtime, caster, target, false)"));
        Assert.That(source, Does.Contain("castRoutine != null"));

        string attackState = File.ReadAllText(
            "Assets/Scripts/Actor/Character/state/AttackTargetState.cs");
        Assert.That(attackState, Does.Contain("waitingForCast = currentSkillManager != null && currentSkillManager.IsCasting"));
        Assert.That(attackState, Does.Contain("currentSkillManager.IsCasting"));
        Assert.That(source, Does.Contain("CastCommitted?.Invoke(runtime)"));
        Assert.That(attackState, Does.Contain("currentSkillManager.CastCommitted += OnCastCommitted"));
        Assert.That(attackState, Does.Contain("if (executed && !skillManager.IsCasting)"));
        Assert.That(attackState, Does.Not.Contain(
            "if (executed)\n            {\n                context.StateService?.RecordSuccessfulSkillUse"));

        string legacy = File.ReadAllText(
            "Assets/Scripts/Actor/Party/SkillExecutorMono.cs");
        Assert.That(legacy, Does.Contain("Positive castTime requires CharacterSkillManager"));
        Assert.That(legacy, Does.Contain("NormalizeCastTime(castEquipment.CastSo.CastTime) > 0f"));
    }

    [Test]
    public void CommitAllowsOnlyTheCurrentCastOwnedPose()
    {
        string manager = File.ReadAllText(
            "Assets/Scripts/Actor/Character/CharacterSkillManager.cs");
        string service = File.ReadAllText(
            "Assets/Scripts/Actor/Character/service/skill/ActiveSkillService.cs");

        Assert.That(manager, Does.Contain("internal bool OwnsHeldCastPose("));
        Assert.That(manager, Does.Contain("ReferenceEquals(castingRuntime, runtime)"));
        Assert.That(manager, Does.Contain("castingCaster == caster"));
        Assert.That(manager, Does.Contain("castPoseClip == castSo.BodyActionClip"));
        Assert.That(manager, Does.Contain("animation.IsHoldingSkillBodyActionCastPose(castPoseClip)"));
        Assert.That(service, Does.Contain("public bool CanCommitCast("));
        Assert.That(service, Does.Contain("castOwner.OwnsHeldCastPose(runtime, caster, animationMono)"));
        Assert.That(service, Does.Contain("animationMono.IsPlayingAttack() && !ownsCurrentCastPose"));
        Assert.That(service, Does.Not.Contain("ShouldSkipAttackAnimation(runtime) || ownsCurrentCastPose"));
    }

    [Test]
    public void PresentationIsOptionalIsolatedAndRestored()
    {
        Assert.That(CharacterSkillCastPresentationMono.MaximumPulseHz, Is.EqualTo(1.8f));
        Assert.That(CharacterSkillCastPresentationMono.MaximumEdgeIntensity, Is.EqualTo(.32f));
        Assert.That(CharacterSkillCastPresentationMono.AccentStartProgress, Is.EqualTo(.88f));
        Assert.That(CharacterSkillCastPresentationMono.CancelFadeSeconds, Is.EqualTo(.12f));
        Assert.That(CharacterSkillCastPresentationMono.CompleteRestoreSeconds, Is.EqualTo(.04f));
        Assert.That(CharacterSkillCastPresentationMono.TerminalIntensity, Is.EqualTo(.4f));
        Assert.That(CharacterSkillCastPresentationMono.TerminalDurationSeconds, Is.EqualTo(.08f));
        Assert.That(CharacterSkillCastPresentationMono.ReducedFlashTerminalIntensity, Is.EqualTo(.2f));
        Assert.That(CharacterSkillCastPresentationMono.ReducedFlashTerminalDurationSeconds, Is.EqualTo(.06f));

        string source = File.ReadAllText(
            "Assets/Scripts/Actor/Character/presentation/CharacterSkillCastPresentationMono.cs");
        Assert.That(source, Does.Contain("new Material(baselineMaterial)"));
        Assert.That(source, Does.Not.Contain("baselineMaterial.Set"));
        Assert.That(source, Does.Contain("ClearOwnedProperties(baselineBlock)"));
        Assert.That(source, Does.Contain("targetRenderer.SetPropertyBlock(activeBlock)"));
        Assert.That(source, Does.Contain("ConfigureAccessibility"));
        Assert.That(source, Does.Contain("private void OnDisable()"));
        Assert.That(source, Does.Contain("RestoreImmediate();"));
        Assert.That(source, Does.Contain(
            "enabled && isActiveAndEnabled && gameObject.activeInHierarchy"));
        Assert.That(source, Does.Contain(
            "if (!CanRunPresentationCoroutine())"));
        Assert.That(source, Does.Contain(
            "if (!CanRunPresentationCoroutine() || duration <= 0f)"));
        Assert.That(source, Does.Contain("presentationGeneration"));
        Assert.That(source, Does.Contain("generation != presentationGeneration"));
        Assert.That(source, Does.Contain("ClearOwnedProperties(baselineBlock)"));
        Assert.That(source, Does.Contain("ClearOwnedProperties(activeBlock)"));
        Assert.That(source, Does.Contain("targetRenderer.sharedMaterial == castMaterial"));
    }

    [Test]
    public void CastPresentationClearsPooledCastResidueAndRestoresImmediatelyWhenDisabled()
    {
        GameObject owner = new("CastPresentationLifecycleTest");
        try
        {
            SpriteRenderer renderer = owner.AddComponent<SpriteRenderer>();
            MaterialPropertyBlock poisoned = new();
            poisoned.SetFloat(Shader.PropertyToID("_CastEnabled"), 1f);
            poisoned.SetFloat(Shader.PropertyToID("_CastCompleteFlash"), 1f);
            renderer.SetPropertyBlock(poisoned);
            CharacterSkillCastPresentationMono presentation =
                owner.AddComponent<CharacterSkillCastPresentationMono>();

            Assert.That(presentation.BeginPresentation(.25f), Is.True);
            MaterialPropertyBlock active = new();
            renderer.GetPropertyBlock(active);
            Assert.That(active.GetFloat(Shader.PropertyToID("_CastEnabled")), Is.EqualTo(1f));
            Assert.That(active.GetFloat(Shader.PropertyToID("_CastCompleteFlash")), Is.Zero);

            owner.SetActive(false);
            MaterialPropertyBlock restored = new();
            renderer.GetPropertyBlock(restored);
            Assert.That(presentation.IsPresenting, Is.False);
            Assert.That(restored.GetFloat(Shader.PropertyToID("_CastEnabled")), Is.Zero);
            Assert.That(restored.GetFloat(Shader.PropertyToID("_CastCompleteFlash")), Is.Zero);
        }
        finally
        {
            Object.DestroyImmediate(owner);
        }
    }

    [Test]
    public void CastWorldHudIsRuntimeOwnedAndUsesTheGameplayCastClock()
    {
        CharacterSkillManager.SkillCastPhaseClock clock = new(.45f);
        Assert.That(clock.RemainingSeconds, Is.EqualTo(.45f).Within(.0001f));
        Assert.That(clock.Tick(.2f), Is.False);
        Assert.That(clock.RemainingSeconds, Is.EqualTo(.25f).Within(.0001f));
        Assert.That(CharacterCastWorldHudMono.FormatRemainingSeconds(.249f), Is.EqualTo("0.2s"));

        string source = File.ReadAllText(
            "Assets/Scripts/Actor/Character/CharacterSkillManager.cs");
        Assert.That(source, Does.Contain("caster.gameObject.AddComponent<CharacterCastWorldHudMono>()"));
        Assert.That(source, Does.Contain("castWorldHud.BeginCast(castTime)"));
        Assert.That(source, Does.Contain("castClock.RemainingSeconds"));
        Assert.That(source, Does.Contain("canvas.renderMode = RenderMode.WorldSpace"));
        Assert.That(source, Does.Contain("canvas.overrideSorting = true"));
        Assert.That(source, Does.Contain("castWorldHud?.HideAndReset();"));
        Assert.That(source, Does.Contain("private void OnDisable() => HideAndReset()"));
        Assert.That(source, Does.Contain("private void OnDestroy() => HideAndReset()"));
    }
}
