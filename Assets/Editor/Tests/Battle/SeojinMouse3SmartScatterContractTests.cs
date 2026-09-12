#if UNITY_EDITOR
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Skill;
using Npc.Service;
using UnityEditor;
using UnityEngine;

public sealed class SeojinMouse3SmartScatterContractTests
{
    private static readonly string[] JsonPaths =
    {
        "Assets/Contents/Skill/json/skill.character.seojin.1.active_5.command_chain.json",
        "Assets/Contents/Skill/json/skill.character.seojin.1.active_6.thunder_command.json",
        "Assets/Contents/Skill/json/skill.character.seojin.1.active_7.blockade_cut.json"
    };

    [Test]
    public void Exact3Json_UsesCanonicalSlotsAndP0Values()
    {
        string chain = Regex.Replace(File.ReadAllText(JsonPaths[0]), @"\s+", string.Empty);
        string thunder = Regex.Replace(File.ReadAllText(JsonPaths[1]), @"\s+", string.Empty);
        string blockade = Regex.Replace(File.ReadAllText(JsonPaths[2]), @"\s+", string.Empty);
        StringAssert.Contains("\"slotName\":\"active_5\"", chain);
        StringAssert.Contains("\"distance\":2.2", chain);
        StringAssert.Contains("\"duration\":0.32", chain);
        StringAssert.Contains("\"normalRatio\":1", chain);
        StringAssert.Contains("\"eliteRatio\":0.65", chain);
        StringAssert.Contains("\"bossRatio\":0.3", chain);
        StringAssert.Contains("\"bossHardCap\":0.65", chain);
        StringAssert.Contains("\"slotName\":\"active_6\"", thunder);
        StringAssert.Contains("\"castTime\":0", thunder);
        StringAssert.Contains("\"cooldown\":3", thunder);
        StringAssert.Contains("\"contactTime\":0.16", thunder);
        StringAssert.Contains("\"burstInterval\":0.22", thunder);
        StringAssert.Contains("\"tacticalNeed\":\"OffensivePressure\"", thunder);
        StringAssert.DoesNotContain("\"kind\":\"Stun\"", thunder);
        StringAssert.Contains("\"slotName\":\"active_7\"", blockade);
        StringAssert.Contains("\"angle\":60", blockade);
        StringAssert.Contains("\"kind\":\"FanPullToCasterSlowStun\"", blockade);
        StringAssert.Contains("\"rendererScale\":0.6", blockade);
        StringAssert.Contains("\"stopRadius\":0.45", blockade);
        StringAssert.Contains("\"gatherDistance\":2.4", blockade);
        StringAssert.Contains("\"gatherDuration\":0.22", blockade);
        StringAssert.Contains("\"stunNormalDuration\":1.0", blockade);
        StringAssert.Contains("\"stunEliteDuration\":0.6", blockade);
        StringAssert.Contains("\"stunBossDuration\":0.25", blockade);
        StringAssert.Contains("\"projectileColliderRadius\":4.2", blockade);
        StringAssert.Contains("\"range\":4.2", blockade);
        StringAssert.Contains("\"distance\":0.25", blockade);
        StringAssert.Contains("\"gatherDistance\":1.2", blockade);
        StringAssert.Contains("\"tacticalNeed\":\"AreaControl\"", blockade);
        StringAssert.DoesNotContain("encirclement_break", blockade);
    }

    [Test]
    public void Mouse3Profile_SerializesWithoutChangingLegacyDefault()
    {
        EquipmentSkillSO skill = ScriptableObject.CreateInstance<EquipmentSkillSO>();
        Assert.IsFalse(skill.Mouse3Profile.Enabled);
        SerializedObject serialized = new SerializedObject(skill);
        SerializedProperty profile = serialized.FindProperty("mouse3Profile");
        profile.FindPropertyRelative("stableSlotKey").stringValue = SkillPoolSlotKeys.Active5;
        profile.FindPropertyRelative("crowdControlKind").stringValue = "GatherDisplacement";
        profile.FindPropertyRelative("distance").floatValue = 2.2f;
        profile.FindPropertyRelative("duration").floatValue = .32f;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        Assert.IsTrue(skill.Mouse3Profile.Enabled);
        Assert.AreEqual(2.2f, skill.Mouse3Profile.Distance, .0001f);
        Object.DestroyImmediate(skill);
    }

    [Test]
    public void Complete39_BindsExactBodyAndVfxSetsWithoutLooping()
    {
        string builder = File.ReadAllText(
            "Assets/Editor/tools/skill/builder/SkillBaseVisualAssetBuilder.cs");
        string[] actions = { "command_chain", "thunder_command", "blockade_cut" };
        float[] contacts = { .18f, .24f, .14f };
        float[] durations = { .42f, .5f, .46f };

        for (int actionIndex = 0; actionIndex < actions.Length; actionIndex++)
        {
            string action = actions[actionIndex];
            string json = File.ReadAllText(JsonPaths[actionIndex]);
            StringAssert.Contains(
                $"character.seojin.1.body_action.{action}.anim", json);
            StringAssert.Contains($"\"contactTime\":{contacts[actionIndex]}", json);
            StringAssert.Contains($"\"duration\":{durations[actionIndex]}", json);

            for (int frame = 0; frame < 6; frame++)
            {
                Assert.IsTrue(File.Exists(
                    $"Assets/ImagesGenerated/Character/animation/character.seojin.1/{action}/frame-0{frame}.png"));
                Assert.IsTrue(File.Exists(
                    $"Assets/ImagesGenerated/Skill/animation/skill.character.seojin.1.active_{actionIndex + 5}.{action}/frame-{frame}.png"));
            }
        }

        StringAssert.Contains("CreateOrUpdateMouse3BodyActionClip", builder);
        StringAssert.Contains("settings.loopTime = false", builder);
    }

    [Test]
    public void SmartScatter_IsExact2AndLegacySafe()
    {
        string profile = File.ReadAllText("Assets/Scripts/Actor/NPC/NpcMovementProfile.cs");
        string pathing = File.ReadAllText("Assets/Scripts/Actor/NPC/NpcPathing.cs");
        string reservations = File.ReadAllText(
            "Assets/Scripts/Actor/NPC/service/EnemyApproachReservationService.cs");
        StringAssert.Contains("character.black_cloth_raider.1", profile);
        StringAssert.Contains("character.chain_dragger_raider.1", profile);
        StringAssert.DoesNotContain("oni_soldier", profile + pathing);
        StringAssert.DoesNotContain("restless_spirit", profile + pathing);
        StringAssert.Contains("enableSmartApproachScatter;", profile);
        StringAssert.Contains("StableActorKey", pathing);
        StringAssert.DoesNotContain("targetRoot.GetInstanceID()", reservations);
        StringAssert.Contains("ReportBlocked", reservations);
        StringAssert.Contains("time + .50f", reservations);
        StringAssert.Contains("_blockedAttempts / 5", reservations);
        StringAssert.Contains("_blockedAttempts >= 20", reservations);
        StringAssert.Contains(".28f * side", reservations);
        StringAssert.Contains("GetTargetLoad", reservations);
    }

    [Test]
    public void P0ExplicitlyExcludesSupersededCrossSkillSynergies()
    {
        string all = string.Join("\n", JsonPaths.Select(File.ReadAllText));
        StringAssert.DoesNotContain("CommandMarked", all);
        StringAssert.DoesNotContain("BreakTempo", all);
        StringAssert.DoesNotContain("cooldownReduction", all);
    }

    [Test]
    public void UpgradeTables_AreExactL1ToL5AndPreviewUsesRuntimeResolver()
    {
        string chain = File.ReadAllText(JsonPaths[0]);
        string thunder = File.ReadAllText(JsonPaths[1]);
        string blockade = File.ReadAllText(JsonPaths[2]);

        AssertLevelsOneThroughFiveOnly(chain);
        AssertLevelsOneThroughFiveOnly(thunder);
        AssertLevelsOneThroughFiveOnly(blockade);

        AssertModifier(chain, "BaseDamage", 7f);
        AssertModifier(chain, "AttackPercentDamage", .05f);
        AssertModifier(chain, "Range", .5f);
        AssertModifier(chain, "ProjectileColliderRadius", .25f);
        AssertModifier(chain, "Cooldown", -.9f);
        AssertModifier(chain, "Mouse3Distance", .3f);
        AssertModifier(chain, "Mouse3EliteRatio", .05f);
        AssertModifier(chain, "Mouse3BossRatio", .05f);
        AssertModifier(chain, "Mouse3BossHardCap", .1f);

        AssertModifier(thunder, "BaseDamage", 12f);
        AssertModifier(thunder, "AttackPercentDamage", .1f);
        AssertModifier(thunder, "ProjectileColliderRadius", .2f);
        AssertModifier(thunder, "Mouse3BurstCount", 1f);
        AssertModifier(thunder, "Mouse3NextBasicForwardRatio", .2f);
        AssertModifier(thunder, "Mouse3NextBasicRangeRatio", .15f);
        AssertModifier(thunder, "Mouse3NextBasicInputGrace", .2f);

        AssertModifier(blockade, "AttackPercentDamage", .1f);
        AssertModifier(blockade, "Range", .4f);
        AssertModifier(blockade, "Mouse3FanAngle", 5f);
        AssertModifier(blockade, "Cooldown", -.7f);
        AssertModifier(blockade, "Mouse3Distance", .05f);
        AssertModifier(blockade, "Mouse3CrowdControlDuration", .2f);
        AssertModifier(blockade, "Mouse3GatherDistance", .2f);
        AssertModifier(blockade, "Mouse3StunNormalDuration", .15f);
        AssertModifier(blockade, "Mouse3StunEliteDuration", .1f);
        AssertModifier(blockade, "Mouse3StunBossDuration", .05f);

        string runtime = File.ReadAllText(
            "Assets/Scripts/Ability/Skills/Services/EquipmentSkillResolver.cs");
        string preview = File.ReadAllText(
            "Assets/Scripts/Ability/Skills/Upgrades/Services/EquipmentUpgradeStatComparisonResolver.cs");
        string upgradeUi = File.ReadAllText(
            "Assets/Scripts/Ability/Skills/UI/UIEquipmentUpgradeMono.cs");
        foreach (string modifier in new[]
        {
            "Mouse3Distance", "Mouse3NormalRatio", "Mouse3EliteRatio",
            "Mouse3BossRatio", "Mouse3BossHardCap", "Mouse3FanAngle",
            "Mouse3CrowdControlDuration", "Mouse3BurstCount",
            "Mouse3NextBasicForwardRatio", "Mouse3NextBasicRangeRatio",
            "Mouse3NextBasicInputGrace", "Mouse3GatherDistance",
            "Mouse3GatherDuration"
            ,"Mouse3StunNormalDuration", "Mouse3StunEliteDuration",
            "Mouse3StunBossDuration"
        })
        {
            StringAssert.Contains(modifier, runtime);
            StringAssert.Contains(modifier, preview + upgradeUi);
        }
        StringAssert.Contains("statResolver.ResolveStat", runtime);
    }

    private static void AssertLevelsOneThroughFiveOnly(string json)
    {
        string compact = json.Replace(" ", string.Empty)
            .Replace("\r", string.Empty)
            .Replace("\n", string.Empty);
        for (int level = 1; level <= 5; level++)
        {
            StringAssert.Contains($"\"level\":{level}", compact);
        }
        StringAssert.DoesNotContain("\"level\":6", compact);
    }

    private static void AssertModifier(string json, string type, float value)
    {
        string compact = json.Replace(" ", string.Empty)
            .Replace("\r", string.Empty)
            .Replace("\n", string.Empty);
        StringAssert.Contains(
            $"\"modifierType\":\"{type}\",\"operationType\":\"Flat\",\"value\":{value.ToString(System.Globalization.CultureInfo.InvariantCulture)}",
            compact);
    }

    [Test]
    public void ReservationHarness_CapsLoadAndEscalatesBlockedPathToLane()
    {
        GameObject target = new GameObject("StableTarget");
        GameObject actor = new GameObject("StableActor");
        try
        {
            target.transform.position = Vector3.zero;
            actor.transform.position = new Vector3(3f, 0f, 0f);
            EnemyApproachReservationService service = new EnemyApproachReservationService();
            int stable = EnemyApproachReservationService.StableActorKey(actor.transform);
            EnemyApproachReservationService.Result result = default;
            float time = 1f;
            for (int attempt = 0; attempt < 20; attempt++)
            {
                result = service.Resolve(target.transform, actor.transform.position, .6f, stable, time);
                Assert.IsTrue(result.Valid, $"attempt {attempt}");
                service.ReportBlocked(time);
                time += .51f;
            }
            result = service.Resolve(target.transform, actor.transform.position, .6f, stable, time);
            Assert.IsTrue(result.Valid);
            Assert.IsTrue(result.LaneMode);
            Assert.AreEqual(1, EnemyApproachReservationService.GetTargetLoad(target.transform, time));
            service.Release();
        }
        finally
        {
            Object.DestroyImmediate(actor);
            Object.DestroyImmediate(target);
        }
    }

    [Test]
    public void ComboRev2_LinkAndSetupContractsRemainAdditive()
    {
        string thunder = File.ReadAllText(JsonPaths[1]);
        string blockade = File.ReadAllText(JsonPaths[2]);
        string service = File.ReadAllText(
            "Assets/Scripts/Actor/Character/service/skill/ActiveSkillService.cs");
        string hit = File.ReadAllText(
            "Assets/Scripts/Ability/Skills/Projectiles/ProjectileHitHandler.cs");
        string bridge = File.ReadAllText(
            "Assets/Scripts/Ability/Skills/Runtime/Mouse3BasicBridgeState.cs");

        StringAssert.Contains("\"kind\":\"Linker\"", thunder);
        StringAssert.Contains("\"burstCount\":1", thunder);
        StringAssert.Contains("\"burstInterval\":0.22", thunder);
        StringAssert.Contains("\"modifierType\":\"Mouse3BurstCount\"", thunder);
        StringAssert.Contains("comboIndex: burstIndex", service);
        StringAssert.Contains("Mouse3BasicBridgeState.TryConsume", service);
        StringAssert.Contains("Mouse3BasicBridgeState.Arm", hit);
        StringAssert.Contains("runtimeData.comboIndex != 1", hit);
        StringAssert.Contains("Pending.Remove", bridge);

        StringAssert.Contains("\"kind\":\"FanPullToCasterSlowStun\"", blockade);
        StringAssert.Contains("\"gatherDistance\":2.4", blockade);
        StringAssert.Contains("\"gatherDuration\":0.22", blockade);
        StringAssert.Contains("\"bossRatio\":0.3", blockade);
        StringAssert.Contains("\"bossHardCap\":0.3", blockade);
        StringAssert.Contains("runtimeData.spawnPosition + runtimeData.NormalizedDirection * .90f", hit);
        StringAssert.Contains(": runtimeData.controlPoint", hit);
        StringAssert.Contains("crowdControlKind, \"FanPullToCasterSlow\"", hit);
        StringAssert.Contains("slow.Apply(slowRatio, durationValue)", hit);
        for (int frame = 0; frame < 12; frame++)
        {
            Assert.IsTrue(File.Exists(
                $"Assets/ImagesGenerated/Character/animation/character.seojin.1/thunder_command/frame-{frame:00}.png"));
            Assert.IsTrue(File.Exists(
                $"Assets/ImagesGenerated/Skill/animation/skill.character.seojin.1.active_6.thunder_command/frame-{frame}.png"));
        }
        StringAssert.Contains("followupBodyClipPath", thunder);
        StringAssert.Contains("followupVfxClipPath", thunder);
        StringAssert.Contains("ThunderFollowupDurations", File.ReadAllText(
            "Assets/Editor/tools/skill/builder/SkillBaseVisualAssetBuilder.cs"));
    }
}
#endif
