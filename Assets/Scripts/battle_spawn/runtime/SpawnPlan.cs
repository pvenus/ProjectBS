using System;
using System.Collections.Generic;
using UnityEngine;
using Character;

public readonly struct SpawnCommand
{
    public CharacterSO Character { get; }
    public Vector3 Position { get; }
    public float Rotation { get; }
    public float StartTime { get; }

    public SpawnCommand(
        CharacterSO character,
        Vector3 position,
        float rotation,
        float startTime)
    {
        Character = character;
        Position = position;
        Rotation = rotation;
        StartTime = startTime;
    }
}

public sealed class SpawnPlan
{
    private readonly List<SpawnCommand> commands;

    public IReadOnlyList<SpawnCommand> Commands => commands;

    public SpawnPlan(List<SpawnCommand> commands)
    {
        this.commands = commands ?? new List<SpawnCommand>();
    }
}
