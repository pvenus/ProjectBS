using System;
using System.Collections.Generic;
using UnityEngine;

public readonly struct SpawnCommand
{
    public string UnitKey { get; }
    public SpawnUnitRole Role { get; }
    public Vector3 Position { get; }
    public float Rotation { get; }
    public float StartTime { get; }

    public SpawnCommand(
        string unitKey,
        SpawnUnitRole role,
        Vector3 position,
        float rotation,
        float startTime)
    {
        UnitKey = unitKey;
        Role = role;
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
