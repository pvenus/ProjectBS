using System.Collections.Generic;
using System.Reflection;
using Battle.Progression;
using Effect;
using NUnit.Framework;
using Skill;
using UnityEngine;

public sealed class BattleExperienceProgressionTests
{
    [Test]
    public void Add_PreservesOverflowAndSupportsMultipleLevels()
    {
        var config = new BattleExperienceLevelConfig
        {
            baseRequiredExperience = 10f,
            growthPerLevel = 5f,
            growthExponent = 1f
        };
        var progression = new BattleExperienceProgression(config);

        int gained = progression.Add(40f);

        Assert.That(gained, Is.EqualTo(3));
        Assert.That(progression.Level, Is.EqualTo(4));
        Assert.That(progression.CurrentExperience, Is.EqualTo(0f));
        Assert.That(progression.RequiredExperience, Is.EqualTo(25f));
    }

    [Test]
    public void RequiredExperience_RisesByLevel()
    {
        var config = new BattleExperienceLevelConfig();
        Assert.That(config.RequiredForLevel(2), Is.GreaterThan(config.RequiredForLevel(1)));
        Assert.That(config.RequiredForLevel(3), Is.GreaterThan(config.RequiredForLevel(2)));
    }

    [Test]
    public void RewardQueue_SeparatesSourcesAndRejectsDuplicateEntitlements()
    {
        var queue = new BattleSkillUpgradeRewardQueue();
        var level = new SkillUpgradeRewardRequest(
            SkillUpgradeRewardSource.ExperienceLevel,
            "xp-level:battle:2",
            "experience_level_2");
        var clear = new SkillUpgradeRewardRequest(
            SkillUpgradeRewardSource.BattleClear,
            "battle-clear:battle",
            "battle_clear");

        Assert.That(queue.TryEnqueue(level), Is.True);
        Assert.That(queue.TryEnqueue(level), Is.False);
        Assert.That(queue.TryEnqueue(clear), Is.True);
        Assert.That(queue.PendingCount, Is.EqualTo(2));

        Assert.That(queue.TryPeek(out var first), Is.True);
        Assert.That(first.Source, Is.EqualTo(SkillUpgradeRewardSource.ExperienceLevel));
        Assert.That(queue.TryComplete(first.IdempotencyKey), Is.True);
        Assert.That(queue.TryPeek(out var second), Is.True);
        Assert.That(second.Source, Is.EqualTo(SkillUpgradeRewardSource.BattleClear));
    }

    [Test]
    public void UpgradeEligibility_AcceptsEffectOnlyDashEntryAndUsesTableMax()
    {
        var modifier = new EffectUpgradeModifierData();
        modifier.ApplyEditorData(
            "dash.move_speed",
            EffectModifierFieldType.Value,
            SkillStatModifierOperationType.Flat,
            1f);
        var entry = new EquipmentUpgradeEntry();
        entry.ApplyEditorData(
            2,
            new List<SkillStatModifierData>(),
            new List<EffectUpgradeModifierData> { modifier });
        var table = ScriptableObject.CreateInstance<EquipmentUpgradeTableSO>();
        var skill = ScriptableObject.CreateInstance<EquipmentSkillSO>();
        try
        {
            table.ApplyEditorData("upgrade.dash", new List<EquipmentUpgradeEntry> { entry });
            typeof(EquipmentSkillSO)
                .GetField("upgradeTableSo", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(skill, table);

            Assert.That(SkillUpgradeViewDataBuilder.HasRuntimeApplicableUpgrade(skill, 2), Is.True);
            Assert.That(SkillUpgradeViewDataBuilder.GetRuntimeUpgradeMaxLevel(skill), Is.EqualTo(2));
        }
        finally
        {
            Object.DestroyImmediate(skill);
            Object.DestroyImmediate(table);
        }
    }
}
