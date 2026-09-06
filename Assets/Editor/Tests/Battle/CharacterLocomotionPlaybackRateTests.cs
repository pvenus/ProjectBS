using NUnit.Framework;
using Character;

public class CharacterLocomotionPlaybackRateTests
{
    [TestCase(0f, 0f)]
    [TestCase(-1f, 0f)]
    [TestCase(0.5f, 0.5f)]
    [TestCase(1.5f, 0.5f)]
    [TestCase(3f, 1f)]
    [TestCase(4.5f, 1.5f)]
    [TestCase(9f, 1.5f)]
    public void PlaybackRateUsesReferenceSpeedAndSafeClamp(
        float movementSpeed,
        float expected)
    {
        Assert.That(
            AnimationMono.CalculateLocomotionPlaybackRate(movementSpeed),
            Is.EqualTo(expected).Within(0.0001f));
    }

    [Test]
    public void InvalidSpeedFallsBackToNeutralRate()
    {
        Assert.That(
            AnimationMono.CalculateLocomotionPlaybackRate(float.NaN),
            Is.EqualTo(1f));
    }

    [Test]
    public void DefaultsRemainStableForLegacyCharacters()
    {
        Assert.That(AnimationMono.LocomotionReferenceMoveSpeed, Is.EqualTo(3f));
        Assert.That(AnimationMono.LocomotionMinimumPlaybackRate, Is.EqualTo(0.5f));
        Assert.That(AnimationMono.LocomotionMaximumPlaybackRate, Is.EqualTo(1.5f));
    }
}
