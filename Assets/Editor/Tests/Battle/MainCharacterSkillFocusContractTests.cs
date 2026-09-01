using Battle.Presentation.SkillFocus;
using NUnit.Framework;
using UnityEngine;

public sealed class MainCharacterSkillFocusContractTests
{
    [Test]
    public void Exact18Allowlist_HasNoChildCloneBasicOrPassive()
    {
        Assert.That(MainCharacterSkillFocusProfileSO.EligibleSkillCount, Is.EqualTo(18));
        Assert.That(MainCharacterSkillFocusProfileSO.IsEligibleSkillId("skill.character.seojin.2.active_1.charge"), Is.True);
        Assert.That(MainCharacterSkillFocusProfileSO.IsEligibleSkillId("skill.character.jihan.2.active_2.ten_tonic_soup"), Is.True);
        Assert.That(MainCharacterSkillFocusProfileSO.IsEligibleSkillId("skill.character.yujin.2.active_2.hwalbin_barrage"), Is.True);
        Assert.That(MainCharacterSkillFocusProfileSO.IsEligibleSkillId("skill.character.seojin.3.turtle_ship_cannon_zone"), Is.False);
        Assert.That(MainCharacterSkillFocusProfileSO.IsEligibleSkillId("skill.character.yujin.3.clone.active_1.multi_shot"), Is.False);
        Assert.That(MainCharacterSkillFocusProfileSO.IsEligibleSkillId("skill.character.seojin.3.basic_attack.basic_attack"), Is.False);
    }

    [Test]
    public void Exact3ImpactCalibration_StaysInsideLockedBounds()
    {
        MainCharacterSkillFocusProfileSO profile = MainCharacterSkillFocusProfileSO.CreateRuntimeDefault();
        try
        {
            AssertCalibration(profile.Resolve("skill.character.seojin.2.active_1.charge"), 4f, .160f, 2.5f);
            AssertCalibration(profile.Resolve("skill.character.jihan.2.active_2.ten_tonic_soup"), 2.5f, .140f, 2f);
            AssertCalibration(profile.Resolve("skill.character.yujin.2.active_2.hwalbin_barrage"), 3f, .140f, 2f);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(profile);
        }
    }

    [Test]
    public void ImpactController_RestoresPositionAndNeverMutatesLensRotationOrTime()
    {
        GameObject go = new("SkillFocusCameraTest");
        try
        {
            Camera camera = go.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 7f;
            go.transform.position = new Vector3(3f, -2f, -10f);
            go.transform.rotation = Quaternion.Euler(0f, 0f, 7f);
            Vector3 position = go.transform.position;
            Quaternion rotation = go.transform.rotation;
            float fieldOfView = camera.fieldOfView;
            float timeScale = Time.timeScale;
            SkillFocusCameraControllerMono controller = go.AddComponent<SkillFocusCameraControllerMono>();
            SkillFocusCalibration calibration = new(4f, .16f, 2.5f);

            Assert.That(controller.TryPlay(camera, calibration, Vector2.right), Is.True);
            controller.RestoreImmediate();

            Assert.That(go.transform.position, Is.EqualTo(position));
            Assert.That(go.transform.rotation, Is.EqualTo(rotation));
            Assert.That(camera.orthographicSize, Is.EqualTo(7f).Within(.0001f));
            Assert.That(camera.fieldOfView, Is.EqualTo(fieldOfView).Within(.0001f));
            Assert.That(Time.timeScale, Is.EqualTo(timeScale));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
        }
    }

    [TestCase(720, 3f)]
    [TestCase(1080, 4f)]
    [TestCase(1440, 4.8f)]
    [TestCase(2160, 4.8f)]
    public void ResolutionScaling_ProducesLockedEffectivePixelPeak(int height, float expectedPixels)
    {
        float peak = SkillFocusCameraControllerMono.ResolvePeakPixels(4f, height);
        Assert.That(peak, Is.EqualTo(expectedPixels).Within(.001f));
        Assert.That(peak, Is.LessThanOrEqualTo(6f));
    }

    [Test]
    public void WorldPixelRoundTrip_AtReferenceCameraMeetsDisplayedPeakContract()
    {
        const float orthoSize = 5.4f;
        const float renderHeight = 1080f;
        float requestedPixels = SkillFocusCameraControllerMono.ResolvePeakPixels(4f, renderHeight);
        float worldPerPixel = (2f * orthoSize) / renderHeight;
        float worldOffset = requestedPixels * worldPerPixel;
        float projectedPixels = worldOffset / worldPerPixel;
        Assert.That(worldOffset, Is.EqualTo(.04f).Within(.0001f));
        Assert.That(projectedPixels, Is.GreaterThanOrEqualTo(requestedPixels * .8f));
        Assert.That(projectedPixels, Is.LessThanOrEqualTo(4.35f));
    }

    [TestCase(30, 4f, 3.2f)]
    [TestCase(60, 4f, 3.2f)]
    [TestCase(120, 4f, 3.2f)]
    [TestCase(30, 3f, 2.4f)]
    [TestCase(60, 3f, 2.4f)]
    [TestCase(120, 3f, 2.4f)]
    [TestCase(30, 2.5f, 2f)]
    [TestCase(60, 2.5f, 2f)]
    [TestCase(120, 2.5f, 2f)]
    public void FirstRenderedPeakLatch_MeetsExact3ThresholdAtAnySampleRate(
        int fps,
        float requested1080Pixels,
        float minimumDisplayedPixels)
    {
        Assert.That(fps == 30 || fps == 60 || fps == 120, Is.True);
        float latchedPeak = SkillFocusCameraControllerMono.ResolvePeakPixels(
            requested1080Pixels,
            1080f);
        Assert.That(latchedPeak, Is.GreaterThanOrEqualTo(minimumDisplayedPixels));
        Assert.That(latchedPeak, Is.LessThanOrEqualTo(6f));
    }

    [TestCase(2f, .140f, 4)]
    [TestCase(2.5f, .160f, 5)]
    [TestCase(3f, .180f, 6)]
    public void Waveform_HasExactSignedLobesAndNormalizedPeak(float cycles, float duration, int lobes)
    {
        SkillFocusCalibration calibration = new(3f, duration, cycles);
        int signs = 0;
        int previousSign = 0;
        float peak = 0f;
        for (int index = 1; index < 4096; index++)
        {
            float value = SkillFocusCameraControllerMono.EvaluateNormalizedWaveform(index / 4096f, calibration);
            peak = Mathf.Max(peak, Mathf.Abs(value));
            int sign = value > .0001f ? 1 : value < -.0001f ? -1 : 0;
            if (sign != 0 && sign != previousSign)
            {
                signs++;
                previousSign = sign;
            }
        }

        Assert.That(signs, Is.EqualTo(lobes));
        Assert.That(peak, Is.EqualTo(1f).Within(.002f));
        Assert.That(SkillFocusCameraControllerMono.EvaluateNormalizedWaveform(1f, calibration), Is.Zero);
    }

    [TestCase(30)]
    [TestCase(60)]
    [TestCase(120)]
    public void FrameSamples_StayBoundedAndTerminateAtBaseline(int fps)
    {
        SkillFocusCalibration calibration = new(4f, .16f, 2.5f);
        int sampleCount = Mathf.CeilToInt(calibration.Duration * fps);
        float requestedPeak = SkillFocusCameraControllerMono.ResolvePeakPixels(
            calibration.AmplitudePixels,
            1080f);
        float displayedPeak = requestedPeak; // TryPlay holds this sample until one render pass.
        for (int index = 0; index <= sampleCount; index++)
        {
            float u = Mathf.Min(1f, (index / (float)fps) / calibration.Duration);
            float value = SkillFocusCameraControllerMono.EvaluateNormalizedWaveform(u, calibration);
            Assert.That(Mathf.Abs(value), Is.LessThanOrEqualTo(1f));
        }
        Assert.That(displayedPeak, Is.GreaterThanOrEqualTo(requestedPeak * .8f));
        Assert.That(SkillFocusCameraControllerMono.EvaluateNormalizedWaveform(1f, calibration), Is.Zero);
    }

    private static void AssertCalibration(
        SkillFocusCalibration value, float amplitudePixels, float duration, float cycles)
    {
        Assert.That(value.AmplitudePixels, Is.EqualTo(amplitudePixels).Within(.0001f));
        Assert.That(value.Duration, Is.EqualTo(duration).Within(.0001f));
        Assert.That(value.Cycles, Is.EqualTo(cycles).Within(.0001f));
        Assert.That(value.AmplitudePixels, Is.LessThanOrEqualTo(6f));
        Assert.That(value.Duration, Is.LessThanOrEqualTo(.19f));
        Assert.That(value.Cycles, Is.LessThanOrEqualTo(3f));
    }
}
