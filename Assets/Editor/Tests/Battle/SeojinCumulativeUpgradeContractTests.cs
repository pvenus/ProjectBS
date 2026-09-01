using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ResourceTools.Skill;
using UnityEngine;

public sealed class SeojinCumulativeUpgradeContractTests
{
    private static readonly string[] Paths =
    {
        "Assets/Contents/Skill/json/skill.character.seojin.1.basic_attack.basic_attack.json",
        "Assets/Contents/Skill/json/skill.character.seojin.2.basic_attack.basic_attack.json",
        "Assets/Contents/Skill/json/skill.character.seojin.3.basic_attack.basic_attack.json",
        "Assets/Contents/Skill/json/skill.character.seojin.1.active_1.active_1.json",
        "Assets/Contents/Skill/json/skill.character.seojin.2.active_1.charge.json",
        "Assets/Contents/Skill/json/skill.character.seojin.3.active_1.charge.json"
    };

    [Test]
    public void ExactCapsPrefixesAndModifierWhitelistAreStable()
    {
        SkillUpgradeAsssetBuilder.SkillUpgradeTableJson[] tables = Paths.Select(ParseTable).ToArray();
        int[] caps = { 5, 10, 15, 5, 10, 15 };

        for (int i = 0; i < tables.Length; i++)
        {
            Assert.That(tables[i].entries.Count, Is.EqualTo(caps[i]), Paths[i]);
            Assert.That(tables[i].entries.Select(e => e.level),
                Is.EqualTo(Enumerable.Range(1, caps[i]).ToArray()), Paths[i]);
            foreach (SkillUpgradeAsssetBuilder.SkillUpgradeEntryJson entry in tables[i].entries)
            {
                Assert.That(entry.effectModifiers, Is.Empty, Paths[i]);
                foreach (SkillUpgradeAsssetBuilder.SkillStatModifierJson modifier in entry.statModifiers)
                {
                    Assert.That(modifier.modifierType,
                        Is.EqualTo("BaseDamage")
                            .Or.EqualTo("AttackPercentDamage")
                            .Or.EqualTo("Range")
                            .Or.EqualTo("Cooldown"), Paths[i]);
                    Assert.That(modifier.operationType, Is.EqualTo("Flat"), Paths[i]);
                }
            }
        }

        AssertPrefix(tables[0], tables[1], 5);
        AssertPrefix(tables[1], tables[2], 10);
        AssertPrefix(tables[3], tables[4], 5);
        AssertPrefix(tables[4], tables[5], 10);
    }

    [TestCase(0, 15f, 1f, .6f, 3f, 26f, 1.23f, .8f, 2.87f)]
    [TestCase(1, 30f, 1f, .7f, 2.5f, 76f, 1.75f, 1.45f, 1.98f)]
    [TestCase(2, 45f, 1f, .8f, 2f, 152f, 2.57f, 2.05f, 1.16f)]
    [TestCase(3, 50f, 1.8f, 2f, 10f, 80f, 2.10f, 2.35f, 9.50f)]
    [TestCase(4, 80f, 1.8f, 2f, 9f, 187f, 2.90f, 3.05f, 6.75f)]
    [TestCase(5, 120f, 1.8f, 2f, 8f, 302f, 3.57f, 3.75f, 4.65f)]
    public void CumulativeCapEndpointsMatchGoldenResolverValues(
        int index, float baseDamage, float apd, float range, float cooldown,
        float expectedDamage, float expectedApd, float expectedRange, float expectedCooldown)
    {
        SkillUpgradeAsssetBuilder.SkillUpgradeTableJson table = ParseTable(Paths[index]);
        Dictionary<string, float> sums = Sum(table.entries);
        Assert.That(baseDamage + Get(sums, "BaseDamage"), Is.EqualTo(expectedDamage).Within(.0001f));
        Assert.That(apd + Get(sums, "AttackPercentDamage"), Is.EqualTo(expectedApd).Within(.0001f));
        Assert.That(range + Get(sums, "Range"), Is.EqualTo(expectedRange).Within(.0001f));
        Assert.That(cooldown + Get(sums, "Cooldown"), Is.EqualTo(expectedCooldown).Within(.0001f));
    }

    [Test]
    public void RuntimeResolverClearsThenAccumulatesEntriesUpToCurrentLevel()
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string runtime = File.ReadAllText(Path.Combine(projectRoot,
            "Assets/Scripts/Ability/Skills/Upgrades/Runtime/EquipmentUpgradeRuntimeData.cs"));
        Assert.That(runtime, Does.Contain("statModifiers.Clear();"));
        Assert.That(runtime, Does.Contain("effectModifiers.Clear();"));
        Assert.That(runtime, Does.Contain("if (entryLevel > currentLevel)"));
        Assert.That(runtime, Does.Contain("statModifiers.AddRange(copiedStatModifiers);"));
    }

    private static SkillUpgradeAsssetBuilder.SkillUpgradeTableJson ParseTable(string relativePath)
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string json = File.ReadAllText(Path.Combine(projectRoot, relativePath));
        MethodInfo parser = typeof(EquipmentSkillJsonGenerator).GetMethod(
            "ParseEquipmentSkillJson", BindingFlags.Static | BindingFlags.NonPublic);
        EquipmentSkillJsonGenerator.EquipmentSkillJson root =
            (EquipmentSkillJsonGenerator.EquipmentSkillJson)parser.Invoke(null, new object[] { json });
        return JsonUtility.FromJson<SkillUpgradeAsssetBuilder.SkillUpgradeTableJson>(root.upgrade);
    }

    private static void AssertPrefix(
        SkillUpgradeAsssetBuilder.SkillUpgradeTableJson expected,
        SkillUpgradeAsssetBuilder.SkillUpgradeTableJson actual,
        int count)
    {
        for (int i = 0; i < count; i++)
        {
            string left = JsonUtility.ToJson(expected.entries[i]);
            string right = JsonUtility.ToJson(actual.entries[i]);
            Assert.That(right, Is.EqualTo(left), $"shared level {i + 1}");
        }
    }

    private static Dictionary<string, float> Sum(
        IEnumerable<SkillUpgradeAsssetBuilder.SkillUpgradeEntryJson> entries)
    {
        var result = new Dictionary<string, float>();
        foreach (SkillUpgradeAsssetBuilder.SkillUpgradeEntryJson entry in entries)
        foreach (SkillUpgradeAsssetBuilder.SkillStatModifierJson modifier in entry.statModifiers)
        {
            result[modifier.modifierType] = Get(result, modifier.modifierType) + modifier.value;
        }
        return result;
    }

    private static float Get(IReadOnlyDictionary<string, float> values, string key) =>
        values.TryGetValue(key, out float value) ? value : 0f;
}
