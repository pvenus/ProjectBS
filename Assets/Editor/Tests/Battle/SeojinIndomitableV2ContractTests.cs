using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Character.Skill;
using Effect;
using NUnit.Framework;
using ResourceTools.Skill;
using Skill;
using UnityEngine;

public sealed class SeojinIndomitableV2ContractTests
{
    private static readonly string[] Paths =
    {
        "Assets/Contents/Skill/json/skill.character.seojin.1.passive_1.indomitable.json",
        "Assets/Contents/Skill/json/skill.character.seojin.2.passive_1.indomitable.json",
        "Assets/Contents/Skill/json/skill.character.seojin.3.passive_1.indomitable.json"
    };

    private static readonly float[] BaseAttack = { 20f, 23f, 26f };
    private static readonly float[] BaseDr = { 10f, 11f, 12f };
    private static readonly int[] Caps = { 5, 10, 15 };
    private static readonly float[] EndpointAttack = { 25f, 35f, 45f };
    private static readonly float[] EndpointDr = { 11f, 14f, 18f };

    [Test]
    public void JsonDefinitionsUseV2GradeBasesCapsAndSharedIncrementPrefix()
    {
        SkillUpgradeAsssetBuilder.SkillUpgradeTableJson[] tables =
            Paths.Select(ParseTable).ToArray();

        for (int grade = 0; grade < Paths.Length; grade++)
        {
            string json = Read(Paths[grade]);
            Assert.That(json, Does.Contain($"\"value\": {BaseAttack[grade]:0}"), Paths[grade]);
            Assert.That(json, Does.Contain($"\"value\": {BaseDr[grade]:0}"), Paths[grade]);
            Assert.That(tables[grade].entries.Count, Is.EqualTo(Caps[grade]), Paths[grade]);
            Assert.That(tables[grade].entries.Select(entry => entry.level),
                Is.EqualTo(Enumerable.Range(1, Caps[grade]).ToArray()), Paths[grade]);

            (float attack, float dr) = Sum(tables[grade]);
            Assert.That(BaseAttack[grade] + attack, Is.EqualTo(EndpointAttack[grade]));
            Assert.That(BaseDr[grade] + dr, Is.EqualTo(EndpointDr[grade]));
        }

        AssertPrefix(tables[0], tables[1], 5);
        AssertPrefix(tables[1], tables[2], 10);
    }

    [TestCase(1, 5, 25f, 11f)]
    [TestCase(2, 10, 35f, 14f)]
    [TestCase(3, 15, 45f, 18f)]
    public void SnapshotResolverProducesV2Endpoints(
        int grade, int level, float expectedAttack, float expectedDr)
    {
        EquipmentSkillRuntimeData runtime = CreateRuntime(grade, level);
        Assert.That(IndomitablePassiveSnapshotResolver.TryResolve(
            runtime, out ResolvedConditionalPassiveSnapshot snapshot, out string error),
            Is.True, error);
        Assert.That(snapshot.AttackPercent, Is.EqualTo(expectedAttack));
        Assert.That(snapshot.DamageReductionPercent, Is.EqualTo(expectedDr));
        Assert.That(snapshot.Revision, Is.EqualTo("indomitable.v2"));
    }

    [Test]
    public void RuntimeSourceEncodesTriggerCapsAndNoCastHitProjection()
    {
        string status = Read("Assets/Scripts/Actor/Character/service/CharacterStatusTickService.cs");
        string passive = Read("Assets/Scripts/Actor/Character/service/skill/PassiveSkillService.cs");
        Assert.That(status, Does.Contain("2f,"));
        Assert.That(status, Does.Contain("10);"));
        Assert.That(status, Does.Contain("target.RuntimeData.isDead"));
        Assert.That(status, Does.Contain("target.isActiveAndEnabled"));
        Assert.That(status, Does.Contain("EffectiveDefenseCap"));
        Assert.That(passive, Does.Contain("IndomitablePassiveSnapshotResolver.TryResolve"));
        Assert.That(passive, Does.Contain("continue;").And.Contain("ApplyCastSelfEffects"));
    }

    private static EquipmentSkillRuntimeData CreateRuntime(int grade, int level)
    {
        string path = Paths[grade - 1];
        SkillUpgradeAsssetBuilder.SkillUpgradeTableJson table = ParseTable(path);
        EquipmentSkillSO equipment = ScriptableObject.CreateInstance<EquipmentSkillSO>();
        typeof(EquipmentSkillSO).GetField("equipmentId", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(equipment, $"skill.character.seojin.{grade}.passive_1.indomitable");

        var modifiers = new List<EffectUpgradeModifierData>();
        foreach (SkillUpgradeAsssetBuilder.SkillUpgradeEntryJson entry in table.entries)
        {
            if (entry.level > level) continue;
            foreach (SkillUpgradeAsssetBuilder.EffectUpgradeModifierJson json in entry.effectModifiers)
            {
                var modifier = new EffectUpgradeModifierData();
                modifier.ApplyEditorData(
                    json.targetEffectId,
                    EffectModifierFieldType.Value,
                    SkillStatModifierOperationType.Flat,
                    json.value);
                modifiers.Add(modifier);
            }
        }

        return new EquipmentSkillRuntimeData
        {
            sourceEquipment = equipment,
            instanceData = new EquipmentSkillInstanceData { currentLevel = level },
            resolvedLevel = level,
            upgradeRuntimeData = new EquipmentUpgradeRuntimeData { currentLevel = level, effectModifiers = modifiers }
        };
    }

    private static void AssertPrefix(
        SkillUpgradeAsssetBuilder.SkillUpgradeTableJson left,
        SkillUpgradeAsssetBuilder.SkillUpgradeTableJson right,
        int count)
    {
        for (int i = 0; i < count; i++)
        {
            Assert.That(right.entries[i].level, Is.EqualTo(left.entries[i].level));
            Assert.That(right.entries[i].effectModifiers.Count,
                Is.EqualTo(left.entries[i].effectModifiers.Count));
            for (int j = 0; j < left.entries[i].effectModifiers.Count; j++)
            {
                SkillUpgradeAsssetBuilder.EffectUpgradeModifierJson a = left.entries[i].effectModifiers[j];
                SkillUpgradeAsssetBuilder.EffectUpgradeModifierJson b = right.entries[i].effectModifiers[j];
                Assert.That(b.fieldType, Is.EqualTo(a.fieldType));
                Assert.That(b.operationType, Is.EqualTo(a.operationType));
                Assert.That(b.value, Is.EqualTo(a.value));
                Assert.That(
                    b.targetEffectId[b.targetEffectId.Length - 1],
                    Is.EqualTo(a.targetEffectId[a.targetEffectId.Length - 1]));
            }
        }
    }

    private static (float attack, float dr) Sum(
        SkillUpgradeAsssetBuilder.SkillUpgradeTableJson table)
    {
        float attack = 0f;
        float dr = 0f;
        foreach (SkillUpgradeAsssetBuilder.SkillUpgradeEntryJson entry in table.entries)
        foreach (SkillUpgradeAsssetBuilder.EffectUpgradeModifierJson modifier in entry.effectModifiers)
        {
            if (modifier.targetEffectId.EndsWith(".1", StringComparison.Ordinal)) attack += modifier.value;
            else if (modifier.targetEffectId.EndsWith(".2", StringComparison.Ordinal)) dr += modifier.value;
            else Assert.Fail(modifier.targetEffectId);
        }
        return (attack, dr);
    }

    private static SkillUpgradeAsssetBuilder.SkillUpgradeTableJson ParseTable(string path)
    {
        MethodInfo parser = typeof(EquipmentSkillJsonGenerator).GetMethod(
            "ParseEquipmentSkillJson", BindingFlags.Static | BindingFlags.NonPublic);
        object root = parser.Invoke(null, new object[] { Read(path) });
        string upgrade = (string)root.GetType().GetField("upgrade").GetValue(root);
        return JsonUtility.FromJson<SkillUpgradeAsssetBuilder.SkillUpgradeTableJson>(upgrade);
    }

    private static string Read(string relativePath)
    {
        string root = Directory.GetParent(Application.dataPath).FullName;
        return File.ReadAllText(Path.Combine(root, relativePath));
    }
}
