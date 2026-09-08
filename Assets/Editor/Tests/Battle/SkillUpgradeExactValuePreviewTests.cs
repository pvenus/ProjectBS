using System.Collections.Generic;
using System.Reflection;
using Effect;
using NUnit.Framework;
using Skill;
using Stat;
using UnityEngine;
using System.IO;

public sealed class SkillUpgradeExactValuePreviewTests
{
    [Test]
    public void RuntimePopupCard_ReplacesLegacyDescriptionInVisibleTextField()
    {
        string card = File.ReadAllText(
            "Assets/Scripts/_machal/UIComponent/SkillUpgradeView/UISkillUpgradeOptionCard.cs");
        string contentView = File.ReadAllText(
            "Assets/Scripts/Presentation/SharedUI/Content/UIContentInfoView.cs");
        string prefab = File.ReadAllText(
            "Assets/Prefabs/UI/Fixed/Battle/SkillUpgrade/UISkillUpCard.prefab");
        string registry = File.ReadAllText(
            "Assets/Resources/ui/PopupViewRegistrySO.asset");

        Assert.That(registry, Does.Contain("guid: fc4c8e8873c5aa849b405b3875cde303"));
        Assert.That(prefab, Does.Contain("m_Name: Bind_StatComparisonText"));
        Assert.That(prefab, Does.Contain("m_Name: Body"));
        Assert.That(prefab, Does.Contain("m_IsActive: 0"));
        Assert.That(card.IndexOf("SetContent(data?.content)"),
            Is.LessThan(card.IndexOf("SetStatComparison(data?.statComparisonText")));
        Assert.That(card, Does.Contain("SetDescriptionOverride(comparisonText)"));
        Assert.That(contentView, Does.Contain("public void SetDescriptionOverride(string value)"));
        Assert.That(card, Does.Not.Contain("contentInfoView.Bind(content);\n        SetStatComparison"));
    }

    [Test]
    public void Comparison_UsesResolvedBeforeAfterAndIncludesSignedDelta()
    {
        EquipmentSkillSO skill = BuildSkill();
        try
        {
            string text = new EquipmentUpgradeComparisonService()
                .BuildComparisonText(skill, 1, 2);

            Assert.That(text, Does.Contain("8초 → 7.2초  (-0.8초)"));
            Assert.That(text, Does.Contain("0.2초 → 0.5초  (+0.3초)"));
        }
        finally
        {
            DestroyGraph(skill);
        }
    }

    [Test]
    public void SharedEvaluator_MatchesFlatPercentAndOverrideSemantics()
    {
        Assert.That(UpgradeModifierValueEvaluator.Apply(
            100f, SkillStatModifierOperationType.Flat, 15f), Is.EqualTo(115f));
        Assert.That(UpgradeModifierValueEvaluator.Apply(
            100f, SkillStatModifierOperationType.Percent, .15f), Is.EqualTo(115f));
        Assert.That(UpgradeModifierValueEvaluator.Apply(
            100f, SkillStatModifierOperationType.Override, 15f), Is.EqualTo(15f));
    }

    [Test]
    public void EffectName_UsesAuthoredNameBeforeSemanticFallback()
    {
        var config = new StatModifierEffectConfig();
        config.ApplyEditorData(StatType.AttackSpeedPercent, StatModifierType.Percent, 12f);
        var effect = ScriptableObject.CreateInstance<EffectSO>();
        try
        {
            effect.ApplyEditorData(
                "skill.character.seojin.1.active_4.swift_step.effect.self.attack_speed",
                null,
                config,
                "스침걸음 공격속도");
            Assert.That(EffectDisplayNameResolver.Resolve(effect), Is.EqualTo("스침걸음 공격속도"));

            effect.ApplyEditorData("missing.semantic.effect", null, config);
            Assert.That(EffectDisplayNameResolver.Resolve(effect), Is.EqualTo("공격속도 효과"));
        }
        finally
        {
            Object.DestroyImmediate(effect);
        }
    }

    [Test]
    public void CanonicalEffectAssetsPreserveAuthoredNamesAndResolveUpgradeTargets()
    {
        string swift = File.ReadAllText(
            "Assets/Contents/Skill/so/skill.character.seojin.1.active_4.swift_step.effect.self.attack_speed.asset");
        string indomitable = File.ReadAllText(
            "Assets/Contents/Skill/so/skill.character.seojin.1.passive_1.indomitable.effect.self.1.asset");
        Assert.That(swift, Does.Contain("authoredDisplayName: \"스침걸음 공격속도\""));
        Assert.That(indomitable, Does.Contain("authoredDisplayName: \"주변 적 10명 이상 시 공격력(%)\""));
    }

    [Test]
    public void Comparison_DoesNotExposeInternalEquipmentIdentity()
    {
        EquipmentSkillSO basic = BuildSkill();
        EquipmentSkillSO dash = BuildSkill();
        EquipmentSkillSO active = BuildSkill();
        try
        {
            Set(basic, "equipmentId", "skill.basic");
            Set(dash, "equipmentId", "skill.dash");
            Set(active, "equipmentId", "skill.active");
            var formatter = new EquipmentUpgradeComparisonService();
            string basicText = formatter.BuildComparisonText(basic, 1, 2);
            string dashText = formatter.BuildComparisonText(dash, 1, 2);
            string activeText = formatter.BuildComparisonText(active, 1, 2);

            Assert.That(basicText, Does.Not.Contain("skill.basic"));
            Assert.That(dashText, Does.Not.Contain("skill.dash"));
            Assert.That(activeText, Does.Not.Contain("skill.active"));
            Assert.That(basicText, Does.Contain("8초 → 7.2초"));
            Assert.That(dashText, Does.Contain("0.2초 → 0.5초"));
        }
        finally
        {
            DestroyGraph(basic);
            DestroyGraph(dash);
            DestroyGraph(active);
        }
    }

    private static EquipmentSkillSO BuildSkill()
    {
        var effectConfig = new StatModifierEffectConfig();
        effectConfig.ApplyEditorData(StatType.MoveSpeed, StatModifierType.Flat, 2f);
        var effect = ScriptableObject.CreateInstance<EffectSO>();
        effect.ApplyEditorData("test.swift_step.move_speed", null, effectConfig);
        var entry = ScriptableObject.CreateInstance<EffectEntrySO>();
        entry.ApplyEditorData(
            effect,
            EffectLifetimeType.Timed,
            EffectCategoryType.Buff,
            .2f,
            1,
            false,
            0f);

        var cast = ScriptableObject.CreateInstance<SkillCastSO>();
        cast.ApplyEditorData(
            "test.cast",
            TargetingType.Self,
            0f,
            8f,
            0f,
            false,
            new[] { entry });

        var cooldown = new SkillStatModifierData();
        cooldown.ApplyEditorData(
            SkillStatModifierType.Cooldown,
            SkillStatModifierOperationType.Flat,
            -.8f);
        var duration = new EffectUpgradeModifierData();
        duration.ApplyEditorData(
            effect.EffectId,
            EffectModifierFieldType.Duration,
            SkillStatModifierOperationType.Flat,
            .3f);
        var levelOne = new EquipmentUpgradeEntry();
        levelOne.ApplyEditorData(1, new List<SkillStatModifierData>(), new List<EffectUpgradeModifierData>());
        var levelTwo = new EquipmentUpgradeEntry();
        levelTwo.ApplyEditorData(
            2,
            new List<SkillStatModifierData> { cooldown },
            new List<EffectUpgradeModifierData> { duration });
        var table = ScriptableObject.CreateInstance<EquipmentUpgradeTableSO>();
        table.ApplyEditorData("test.table", new List<EquipmentUpgradeEntry> { levelOne, levelTwo });

        var skill = ScriptableObject.CreateInstance<EquipmentSkillSO>();
        Set(skill, "equipmentId", "test.skill");
        Set(skill, "castSo", cast);
        Set(skill, "upgradeTableSo", table);
        return skill;
    }

    private static void Set(object target, string field, object value)
    {
        typeof(EquipmentSkillSO)
            .GetField(field, BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(target, value);
    }

    private static void DestroyGraph(EquipmentSkillSO skill)
    {
        if (skill == null) return;
        SkillCastSO cast = skill.CastSo;
        EquipmentUpgradeTableSO table = skill.UpgradeTableSo;
        EffectEntrySO entry = cast?.SelfEffects != null && cast.SelfEffects.Length > 0
            ? cast.SelfEffects[0]
            : null;
        EffectSO effect = entry?.EffectSO;
        Object.DestroyImmediate(skill);
        if (table != null) Object.DestroyImmediate(table);
        if (cast != null) Object.DestroyImmediate(cast);
        if (entry != null) Object.DestroyImmediate(entry);
        if (effect != null) Object.DestroyImmediate(effect);
    }
}
