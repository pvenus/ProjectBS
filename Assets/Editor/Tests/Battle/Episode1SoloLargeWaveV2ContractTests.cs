#if UNITY_EDITOR
using System.Linq;
using Battle;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class Episode1SoloLargeWaveV2ContractTests
{
    private const string PolicyPath = "Assets/Contents/Battle/large_wave/episode1.rescue_villagers.solo_large_wave.v2.asset";

    [Test]
    public void Exact1Manifest_HasLockedCountsCadenceAndBounds()
    {
        BattleLargeWavePolicySO policy = AssetDatabase.LoadAssetAtPath<BattleLargeWavePolicySO>(PolicyPath);
        Assert.IsNotNull(policy, "Exact1 policy asset must exist.");
        Assert.AreEqual("seq.act1.chapter01.01.rescue_villagers.large_map.three_stage.v3", policy.PolicyId);
        TextAsset text = Resources.Load<TextAsset>(BattleLargeWavePlacementV3.ResourcePath);
        Assert.IsNotNull(text);
        BattleLargeWavePlacementV3 profile = JsonUtility.FromJson<BattleLargeWavePlacementV3>(text.text);
        Assert.AreEqual(28, profile.slots.Count);
        Assert.AreEqual(28, profile.fallbackAnchors.Count);
        Assert.AreEqual(22, profile.slots.Count(x => x.unitKey == Episode1SoloLargeWaveManifest.FodderKey));
        Assert.AreEqual(6, profile.slots.Count(x => x.unitKey == Episode1SoloLargeWaveManifest.FastKey));
        Assert.AreEqual(19, profile.slots.Count(x => x.waveIndex == 1));
        Assert.AreEqual(5, profile.slots.Count(x => x.waveIndex == 2));
        Assert.AreEqual(4, profile.slots.Count(x => x.waveIndex == 3));
        Assert.AreEqual(.35f, profile.slots.Where(x => x.waveIndex == 1).Min(x => x.dueTime), .0001f);
        Assert.AreEqual(4.60f, profile.slots.Where(x => x.waveIndex == 2).Min(x => x.dueTime), .0001f);
        Assert.AreEqual(8.60f, profile.slots.Where(x => x.waveIndex == 3).Min(x => x.dueTime), .0001f);
        Assert.AreEqual(8.96f, profile.slots.Max(x => x.dueTime), .0001f);
        Assert.AreEqual(32f, profile.map.x); Assert.AreEqual(18f, profile.map.y);
        Assert.AreEqual(.75f, profile.boundary.movementInset); Assert.AreEqual(.5f, profile.boundary.spawnInset);
    }

    [Test]
    public void Exact1Manifest_SpawnsSixTimedAssaultGroupsAroundThePlayer()
    {
        BattleMapBoundsContext.Activate(new Vector2(32f, 18f), .75f);
        Assert.AreEqual(new Vector2(15.25f, 8.25f), BattleMapBoundsContext.ClampActorCenter(new Vector2(99f, 99f)));
        Assert.AreEqual(32f, BattleMapBoundsContext.ResolveTargetSearchRadius(16f));
        BattleMapBoundsContext.Clear();
        Assert.AreEqual(16f, BattleMapBoundsContext.ResolveTargetSearchRadius(16f));
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
        Assert.AreEqual(7.25f, policy.MinimumSpawnRadius);
        Assert.AreEqual(14.25f, policy.MaximumSpawnRadius);
    }

    private static float AngularSpan(float[] angles)
    {
        float largestGap = 0f;
        float[] sorted = angles.OrderBy(angle => angle).ToArray();
        for (int i = 0; i < sorted.Length; i++)
        {
            float next = i == sorted.Length - 1 ? sorted[0] + 360f : sorted[i + 1];
            largestGap = Mathf.Max(largestGap, next - sorted[i]);
        }
        return 360f - largestGap;
    }
}
#endif
