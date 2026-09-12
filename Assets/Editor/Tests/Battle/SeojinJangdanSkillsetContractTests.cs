using System.IO;
using NUnit.Framework;
using UnityEngine;

public sealed class SeojinJangdanSkillsetContractTests
{
    private const string Root = "Assets/Contents/Skill/json/skill.character.seojin.1.";

    [TestCase("active_5.jangdan_dung", 2)]
    [TestCase("active_6.jangdan_gi", 1)]
    [TestCase("active_7.jangdan_deok", 2)]
    [TestCase("active_8.deoreoreoreo", 4)]
    public void ExactFourJsonAssetsShareRhythmAndExpectedHitRegistry(string suffix, int hits)
    {
        string json = File.ReadAllText(Root + suffix + ".json");
        Assert.That(json, Does.Contain("\"rhythmId\":\"jangdan.seojin.g1.v1\""));
        Assert.That(Count(json, "\"hitId\":"), Is.EqualTo(hits));
        Assert.That(json, Does.Contain("\"entries\":[{\"level\":1"));
        Assert.That(json, Does.Contain("{\"level\":5"));
    }

    [Test]
    public void RuntimeUsesSingleStageMachineAndExactRhythmGrid()
    {
        string source = File.ReadAllText(
            "Assets/Scripts/Actor/Character/service/skill/SeojinJangdanRuntime.cs");
        Assert.That(source, Does.Contain("None, DungArmed, GiArmed, SequenceCommitted"));
        Assert.That(source, Does.Contain("internal const float Bpm = 120f"));
        Assert.That(source, Does.Contain("internal const float Beat = .5f"));
        Assert.That(source, Does.Contain("internal const float Subdivision = .125f"));
        Assert.That(source, Does.Contain("internal const float SequenceInputWindow = 1f"));
        Assert.That(source, Does.Contain("internal const float SequenceContactEnd = .25f"));
        Assert.That(source, Does.Contain("internal const float SequenceRecovery = .10f"));
        Assert.That(source, Does.Contain("SequenceContactEnd + SequenceRecovery"));
        Assert.That(source, Does.Contain("Time.time - state.lastPairCompletedAt <= 1f"));
    }

    [Test]
    public void QSequenceConsumesFiveSubHitsPerKeyDownAndStartsCooldownAfterTwentieth()
    {
        string input = File.ReadAllText(
            "Assets/Scripts/Actor/Character/Control/SeojinInputReader.cs");
        string service = File.ReadAllText(
            "Assets/Scripts/Actor/Character/service/skill/ActiveSkillService.cs");
        string state = File.ReadAllText(
            "Assets/Scripts/Actor/Character/service/skill/SeojinJangdanRuntime.cs");

        Assert.That(input, Does.Contain("ActiveSkillActionDown=Input.GetKeyDown(KeyCode.Q)"));
        Assert.That(service, Does.Not.Contain("float[] receipts = { .125f, .25f, .375f, .625f }"));
        Assert.That(service, Does.Contain("subHitIndex < 5"));
        Assert.That(service, Does.Contain("yield return new WaitForSeconds(.05f)"));
        Assert.That(service, Does.Contain("selectedHitIndex:stepIndex"));
        Assert.That(service, Does.Contain("isFinalSequenceHit ? 3 : Mathf.Min(stepIndex, 2)"));
        Assert.That(service, Does.Contain("suppressVisual:subHitIndex > 0"));
        Assert.That(service, Does.Contain("if (stepIndex >= 3)"));
        Assert.That(service, Does.Contain("if (!UseSkill(manager, runtime)) yield break;"));
        Assert.That(state, Does.Contain("state.sequenceNextStep = completedStep + 1"));
        Assert.That(state, Does.Contain("Time.time > state.sequenceExpiresAt"));
        Assert.That(state, Does.Contain("Time.time + SequenceInputWindow"));
        Assert.That(service, Does.Contain("SeojinJangdanRuntime.SequenceSetDuration"));
        Assert.That(service, Does.Contain("SeojinJangdanRuntime.SequenceRecovery"));
        Assert.That(service, Does.Contain("float.PositiveInfinity"));
        string manager = File.ReadAllText(
            "Assets/Scripts/Actor/Character/CharacterSkillManager.cs");
        Assert.That(manager, Does.Contain("window.ActionKind==-2&&!window.ContactOccurred"));
    }

    [Test]
    public void QSubHitDamageBudgetsAndFinalStunRemainExact()
    {
        string json = File.ReadAllText(Root + "active_8.deoreoreoreo.json");
        string hit = File.ReadAllText(
            "Assets/Scripts/Ability/Skills/Projectiles/ProjectileHitHandler.cs");

        Assert.That(Count(json, "\"baseDamage\":2"), Is.EqualTo(3));
        Assert.That(Count(json, "\"attackPercentDamage\":0.04"), Is.EqualTo(3));
        Assert.That(Count(json, "\"baseDamage\":7"), Is.EqualTo(1));
        Assert.That(Count(json, "\"attackPercentDamage\":0.11"), Is.EqualTo(1));
        Assert.That(2 * 5, Is.EqualTo(10));
        Assert.That(.04f * 5f, Is.EqualTo(.20f).Within(.0001f));
        Assert.That(7 * 5, Is.EqualTo(35));
        Assert.That(.11f * 5f, Is.EqualTo(.55f).Within(.0001f));
        Assert.That(hit, Does.Contain("index >= 3"));
    }

    [Test]
    public void LevelFiveEndpointsAreMachineReadableAndSubdivisionAligned()
    {
        string json = File.ReadAllText("Assets/Contents/Skill/json/jangdan.seojin.g1.v1.json");
        Assert.That(json, Does.Contain("\"kungWindow\":0.875"));
        Assert.That(json, Does.Contain("\"deokWindow\":0.75"));
        Assert.That(json, Does.Contain("\"assistRetention\":1.125"));
        Assert.That(json, Does.Contain("\"pairedStunNormal\":1.20"));
        Assert.That(json, Does.Contain("\"finalStunNormal\":0.75"));
        Assert.That(json, Does.Contain("\"cooldown\":7.25"));
    }

    [Test]
    public void InputAndAtomicLoadoutUseActiveEightWithoutRemovingLegacyAssets()
    {
        string input = File.ReadAllText("Assets/Scripts/Actor/Character/Control/SeojinInputReader.cs");
        string character = File.ReadAllText("Assets/Contents/Character/json/character.seojin.1.json");
        Assert.That(input, Does.Contain("ActiveSkillActionDown=Input.GetKeyDown(KeyCode.Q)"));
        Assert.That(input, Does.Not.Contain("SequenceSlotKey=SkillPoolSlotKeys.Active8"));
        Assert.That(character, Does.Contain("skill.character.seojin.1.active_8.deoreoreoreo"));
        Assert.That(File.Exists(Root + "active_5.command_chain.json"), Is.True);
        Assert.That(File.Exists(Root + "active_6.thunder_command.json"), Is.True);
        Assert.That(File.Exists(Root + "active_7.blockade_cut.json"), Is.True);
    }

    [Test]
    public void DungHoldSeparatesOneShotDamageFromBoundedVisualAndControlLifetime()
    {
        string runtime = File.ReadAllText(
            "Assets/Scripts/Actor/Character/service/skill/SeojinJangdanRuntime.cs");
        string service = File.ReadAllText(
            "Assets/Scripts/Actor/Character/service/skill/ActiveSkillService.cs");
        string hit = File.ReadAllText(
            "Assets/Scripts/Ability/Skills/Projectiles/ProjectileHitHandler.cs");
        string ssot = File.ReadAllText("Assets/Contents/Skill/json/jangdan.seojin.g1.v1.json");
        string dung = File.ReadAllText(Root + "active_5.jangdan_dung.json");

        Assert.That(runtime, Does.Contain("internal const float DungHoldMax = 1.5f"));
        Assert.That(runtime, Does.Contain("internal const float DungHoldBossMax = .375f"));
        Assert.That(runtime, Does.Contain("state.armedAt + DungHoldBossMax"));
        Assert.That(service, Does.Contain("HoldJangdanDungRoutine"));
        Assert.That(service, Does.Contain("minimumVisualLifetime:!dungFollowup"));
        Assert.That(service, Does.Not.Contain("stage == JangdanStage.DungArmed && id != SeojinJangdanRuntime.DungId"));
        Assert.That(service, Does.Not.Contain("bodyClip, SeojinJangdanRuntime.DungHoldMax"));
        Assert.That(service, Does.Contain("id == SeojinJangdanRuntime.DungId ? .30f"));
        Assert.That(service, Does.Contain(": .30f;"));
        Assert.That(hit, Does.Contain("RegisterDungHoldTarget"));
        Assert.That(runtime, Does.Contain("ProjectileEntity.DespawnVisualsFor"));
        Assert.That(runtime, Does.Contain("state.dungHoldArmedAt + DungHoldBossMax"));
        Assert.That(runtime, Does.Contain("Time.time < state.dungHoldExpiresAt"));
        Assert.That(ssot, Does.Contain("\"maxSeconds\": 1.5"));
        Assert.That(ssot, Does.Contain("\"casterActionUnlockSeconds\": 0.3"));
        Assert.That(ssot, Does.Contain("\"recoverySeconds\": 0.05"));
        Assert.That(ssot, Does.Contain("\"stunBossMax\": 0.375"));
        Assert.That(ssot, Does.Contain("\"damageRepeats\": 0"));
        Assert.That(dung, Does.Contain("\"projectileLifetime\":0.25"));
        Assert.That(dung, Does.Contain("\"deactivateAfterFirstHit\":false"));
    }

    [Test]
    public void DungPrimaryVfxLoopsThreeCompleteCyclesDuringDetachedHold()
    {
        string builder = File.ReadAllText(
            "Assets/Editor/tools/skill/builder/SkillBaseVisualAssetBuilder.cs");
        string clip = File.ReadAllText(
            "Assets/AnimationClips/Skill/skill.character.seojin.1.active_5.jangdan_dung.visual.loop.anim");
        string followup = File.ReadAllText(
            "Assets/AnimationClips/Skill/skill.character.seojin.1.active_5.jangdan_dung.visual.followup.anim");
        Assert.That(builder, Does.Contain("bool loopTime = isJangdanDung"));
        Assert.That(builder, Does.Contain("settings.loopTime = isJangdanDung"));
        Assert.That(clip, Does.Contain("m_StopTime: 0.5"));
        Assert.That(clip, Does.Contain("m_LoopTime: 1"));
        Assert.That(followup, Does.Contain("m_LoopTime: 0"));
        Assert.That(1.5f / .5f, Is.EqualTo(3f));
    }

    [Test]
    public void DungMaintenanceIsBackgroundAndManualBusyEndsAtPointThree()
    {
        string service = File.ReadAllText(
            "Assets/Scripts/Actor/Character/service/skill/ActiveSkillService.cs");
        string manager = File.ReadAllText(
            "Assets/Scripts/Actor/Character/CharacterSkillManager.cs");
        Assert.That(service, Does.Contain(
            "manager.StartBackgroundSkillRoutine(HoldJangdanDungRoutine(caster))"));
        Assert.That(service, Does.Not.Contain(
            "manager.StartOwnedSkillRoutine(HoldJangdanDungRoutine(caster))"));
        Assert.That(service, Does.Contain("yield return new WaitForSeconds(.05f)"));
        Assert.That(manager, Does.Contain("StartBackgroundSkillRoutine"));
        Assert.That(manager, Does.Contain("backgroundRoutineEpoch"));
        Assert.That(manager, Does.Contain(
            "manualRoutines.Count>0||IsCasting||castMoveService.IsMoving"));
        Assert.That(manager, Does.Not.Contain("ManualBusy=>background"));
    }

    [Test]
    public void DungSnapshotsMouseDirectionToAStationaryRangeThreeGroundPoint()
    {
        string service = File.ReadAllText(
            "Assets/Scripts/Actor/Character/service/skill/ActiveSkillService.cs");
        string dung = File.ReadAllText(Root + "active_5.jangdan_dung.json");
        string move = File.ReadAllText(
            "Assets/Contents/Skill/so/skill.character.seojin.1.active_5.jangdan_dung.move.asset");

        Assert.That(dung, Does.Contain("\"range\":3"));
        Assert.That(dung, Does.Contain("\"moveType\":\"Warp\""));
        Assert.That(dung, Does.Not.Contain("\"moveType\":\"Hover\""));
        Assert.That(move, Does.Contain("moveType: 2"));
        Assert.That(move, Does.Contain("type: {class: WarpMoveConfig"));
        Assert.That(move, Does.Not.Contain("HoverMoveConfig"));
        Assert.That(service, Does.Contain(
            "ResolveJangdanDungGroundAim(runtime, caster, aim.Direction)"));
        Assert.That(service, Does.Contain("origin + direction * distance"));
        Assert.That(service, Does.Contain(
            "BattleMapBoundsContext.ClampActorCenter(desiredPoint)"));
        Assert.That(service, Does.Contain("BattleMapBoundsContext.SweepActorStep("));
        Assert.That(service, Does.Contain("manualAim:projectileAim"));
    }


    private static int Count(string value, string needle)
    {
        int count = 0, index = 0;
        while ((index = value.IndexOf(needle, index, System.StringComparison.Ordinal)) >= 0)
        { count++; index += needle.Length; }
        return count;
    }
}
