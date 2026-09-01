using System;
using System.Reflection;
using NUnit.Framework;
using ResourceTools.Effect;

public sealed class SkillHitEffectMaterializationParserTests
{
    private const string CanonicalValueTypeEntry =
        "{\"effect\":{\"effectId\":\"skill.test.defense\",\"effectType\":\"StatModifier\",\"statType\":\"Defense\",\"valueType\":\"Flat\",\"value\":10},\"lifetimeType\":\"Timed\",\"categoryType\":\"Buff\",\"duration\":1.1,\"maxApplyCount\":1}";

    private const string LegacyModifierTypeEntry =
        "{\"effect\":{\"effectId\":\"skill.test.slow\",\"effectType\":\"StatModifier\",\"statType\":\"MoveSpeed\",\"modifierType\":\"Percent\",\"value\":-35},\"lifetimeType\":\"Timed\",\"categoryType\":\"Debuff\",\"duration\":1.1,\"maxApplyCount\":1}";

    private const string NestedConfigHitEntry =
        "{\"effect\":{\"effectId\":\"effect.skill.character.abandoned_shrine_wraith.2.active_1.lost_child_cry.move_speed_down_30\",\"effectType\":\"StatModifier\",\"config\":{\"targetStat\":\"MoveSpeedPercent\",\"modifierType\":\"Percent\",\"value\":-30}},\"lifetimeType\":\"CombatTimed\",\"categoryType\":\"Debuff\",\"duration\":2,\"maxApplyCount\":1}";

    private const string NestedConfigCastEntry =
        "{\"effect\":{\"effectId\":\"effect.skill.character.cage_guard_bandit.2.passive_1.block_the_cage.defense_up\",\"effectType\":\"StatModifier\",\"config\":{\"targetStat\":\"Defense\",\"modifierType\":\"Percent\",\"value\":15}},\"lifetimeType\":\"Manual\",\"categoryType\":\"Buff\",\"duration\":0,\"maxApplyCount\":1}";

    [TestCase(CanonicalValueTypeEntry, "Flat")]
    [TestCase(LegacyModifierTypeEntry, "Percent")]
    public void NestedStatModifier_AcceptsCanonicalAndLegacyModifierKeys(
        string entryJson,
        string expectedModifierType)
    {
        MethodInfo extract = typeof(EffectEntryAssetBuilder).GetMethod(
            "ExtractJsonValue",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.That(extract, Is.Not.Null);

        string effectJson = (string)extract.Invoke(
            null,
            new object[] { entryJson, "effect" });
        Assert.That(effectJson, Is.Not.Null.And.Not.Empty);

        MethodInfo parse = typeof(EffectAssetBuilder).GetMethod(
            "ParseEffectJson",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.That(parse, Is.Not.Null);

        object parsed = parse.Invoke(null, new object[] { effectJson });
        Assert.That(parsed, Is.Not.Null);

        FieldInfo modifier = parsed.GetType().GetField("modifierType");
        Assert.That(modifier, Is.Not.Null);
        Assert.That(modifier.GetValue(parsed), Is.EqualTo(expectedModifierType));
    }

    [TestCase(NestedConfigHitEntry, "MoveSpeedPercent", "Percent")]
    [TestCase(NestedConfigCastEntry, "Defense", "Percent")]
    public void NestedStatModifier_ConfigTargetStatMaterializesForHitAndCast(
        string entryJson,
        string expectedStatType,
        string expectedModifierType)
    {
        MethodInfo extract = typeof(EffectEntryAssetBuilder).GetMethod(
            "ExtractJsonValue",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.That(extract, Is.Not.Null);

        string effectJson = (string)extract.Invoke(
            null,
            new object[] { entryJson, "effect" });
        Assert.That(effectJson, Is.Not.Null.And.Not.Empty);

        MethodInfo parse = typeof(EffectAssetBuilder).GetMethod(
            "ParseEffectJson",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.That(parse, Is.Not.Null);

        object parsed = parse.Invoke(null, new object[] { effectJson });
        Assert.That(parsed, Is.Not.Null);

        FieldInfo statType = parsed.GetType().GetField("statType");
        FieldInfo modifierType = parsed.GetType().GetField("modifierType");
        Assert.That(statType, Is.Not.Null);
        Assert.That(modifierType, Is.Not.Null);
        Assert.That(statType.GetValue(parsed), Is.EqualTo(expectedStatType));
        Assert.That(modifierType.GetValue(parsed), Is.EqualTo(expectedModifierType));
    }
}
