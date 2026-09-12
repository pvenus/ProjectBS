#if UNITY_EDITOR
using System.IO;
using NUnit.Framework;
using UnityEngine;

public sealed class SeojinActive1ReadyInputContractTests
{
    private const string JsonPath="Assets/Contents/Skill/json/skill.character.seojin.1.active_1.active_1.json";
    [Test] public void ReadyProfile_HasLockedValues()
    {
        string json=File.ReadAllText(JsonPath);
        Assert.That(json,Does.Contain("\"readyDuration\": 2.0"));
        Assert.That(json,Does.Contain("\"actionDuration\": 0.2"));
        Assert.That(json,Does.Contain("\"contactTime\": 0.1"));
        Assert.That(json,Does.Contain("\"maxInputs\": 4"));
        Assert.That(json,Does.Contain("\"bufferCapacity\": 2"));
        Assert.That(json,Does.Contain("\"baseDamage\": 10"));
        Assert.That(json,Does.Contain("\"attackPercentDamage\": 0.2"));
    }
    [Test] public void Q_IsConsumerAction_NotActive8Shortcut()
    {
        string reader=File.ReadAllText("Assets/Scripts/Actor/Character/Control/SeojinInputReader.cs");
        string core=File.ReadAllText("Assets/Scripts/Actor/Character/Control/ManualControlCore.cs");
        Assert.That(reader,Does.Contain("ActiveSkillActionDown=Input.GetKeyDown(KeyCode.Q)"));
        Assert.That(reader,Does.Not.Contain("SequenceSlotKey"));
        Assert.That(core,Does.Contain("game.TryActiveSkillAction(snapshot)"));
    }
}
#endif
