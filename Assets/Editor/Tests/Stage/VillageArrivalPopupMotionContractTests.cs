using System.IO;
using NUnit.Framework;

public sealed class VillageArrivalPopupMotionContractTests
{
    private const string Root = "Assets/Resources/Contents/Stage/popup_motion/node.act1.chapter01.episode01.village_arrival";

    [Test]
    public void Sequence_UsesAcceptedQuietSlotsAndStaticFallback()
    {
        string yaml = File.ReadAllText(Root + "/sequence.asset");
        StringAssert.Contains("slotDuration: 0.5", yaml);
        StringAssert.Contains("firstOpenLoopLimit: 3", yaml);
        StringAssert.Contains("reentryLoopLimit: 1", yaml);
        Assert.That(Count(yaml, "  - 0\n"), Is.EqualTo(10));
        Assert.That(Count(yaml, "  - 1\n"), Is.EqualTo(2));
        Assert.That(Count(yaml, "  - 2\n"), Is.EqualTo(2));
        Assert.That(Count(yaml, "  - 3\n"), Is.EqualTo(2));

        string popup = File.ReadAllText(
            "Assets/Contents/Stage/so/node.act1.chapter01.episode01.village_arrival.asset");
        StringAssert.Contains(
            "mainImage: {fileID: 21300000, guid: 79abfca6304224f468e53b54dbb659cf, type: 3}",
            popup);
        StringAssert.Contains("motionSequenceResourcePath: Contents/Stage/popup_motion/", popup);
    }

    [Test]
    public void Player_IsSingleOwnerAndHasFailClosedLifecycle()
    {
        string source = File.ReadAllText(
            "Assets/Scripts/Stage/NodeContents/Event/UI/PopupMotionPlaybackOwner.cs");
        StringAssert.Contains("private static PopupMotionPlaybackOwner active", source);
        StringAssert.Contains("Resources.Load<PopupMotionSequenceSO>", source);
        StringAssert.Contains("target.sprite = fallback", source);
        StringAssert.Contains("if (!MotionEnabled || ReducedMotion)", source);
        StringAssert.Contains("if (Time.timeScale <= 0f || !Application.isFocused) return", source);
        StringAssert.Contains("Resources.UnloadAsset", source);
        StringAssert.DoesNotContain("material", source.ToLowerInvariant());
        StringAssert.DoesNotContain("shader", source.ToLowerInvariant());
    }

    private static int Count(string value, string token)
    {
        int count = 0;
        int index = 0;
        while ((index = value.IndexOf(token, index, System.StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += token.Length;
        }
        return count;
    }
}
