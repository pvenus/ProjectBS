#if UNITY_EDITOR
using System.Linq;
using Battle;
using NUnit.Framework;
using UnityEditor;

public sealed class Episode1SoloLargeWaveV2ContractTests
{
    private const string PolicyPath = "Assets/Contents/Battle/large_wave/episode1.rescue_villagers.solo_large_wave.v2.asset";

    [Test]
    public void Exact1Manifest_HasLockedCountsCadenceAndBounds()
    {
        BattleLargeWavePolicySO policy = AssetDatabase.LoadAssetAtPath<BattleLargeWavePolicySO>(PolicyPath);
        Assert.IsNotNull(policy, "Exact1 policy asset must exist.");
        Assert.IsTrue(Episode1SoloLargeWaveManifest.TryCreate(policy, out var rows, out string error), error);
        Assert.AreEqual(28, rows.Count, "Exactly 28 reservations are required.");
        Assert.AreEqual(22, rows.Count(x => x.UnitKey == Episode1SoloLargeWaveManifest.FodderKey));
        Assert.AreEqual(6, rows.Count(x => x.UnitKey == Episode1SoloLargeWaveManifest.FastKey));
        Assert.AreEqual(8, rows.Count(x => x.EmitTime >= .45f && x.EmitTime <= .60f));
        Assert.AreEqual(8, rows.Count(x => x.EmitTime >= .80f && x.EmitTime <= .95f));
        Assert.AreEqual(6, rows.Count(x => x.EmitTime >= 1.15f && x.EmitTime <= 1.25f));
        Assert.AreEqual(6, rows.Count(x => x.EmitTime >= 1.50f && x.EmitTime <= 1.60f));
        Assert.LessOrEqual(rows.Max(x => x.EmitTime), 1.6001f);
        Assert.AreEqual(28, rows.Select(x => x.Token).Distinct().Count(), "Duplicate reservation tokens are forbidden.");
        Assert.IsTrue(rows.All(x => x.Position.magnitude >= 9f && x.Position.magnitude <= 14.001f));
    }

    [Test]
    public void Exact1Policy_IsEnabledForUserRuntimeReview()
    {
        BattleLargeWavePolicySO policy = AssetDatabase.LoadAssetAtPath<BattleLargeWavePolicySO>(PolicyPath);
        Assert.IsNotNull(policy);
        Assert.IsTrue(policy.Enabled, "User explicitly activated the exact1 pilot for runtime feel review.");
        Assert.IsTrue(policy.SoloOriginAtControlHandoff);
        Assert.AreEqual(.25f, policy.ReservationCommitTime, .0001f);
        Assert.AreEqual(.20f, policy.TelegraphDuration, .0001f);
        Assert.AreEqual(28, policy.HardLivingCap);
    }
}
#endif
