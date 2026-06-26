using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class SpawnPatternSlot
{
    [SerializeField] private Vector2 localPosition;
    [SerializeField] private float localRotation;

    public SpawnPatternSlot(Vector2 localPosition, float localRotation)
    {
        this.localPosition = localPosition;
        this.localRotation = localRotation;
    }

    public Vector2 LocalPosition => localPosition;
    public float LocalRotation => localRotation;
}

public enum SpawnAreaShape
{
    Circle,
    Rectangle
}

public enum LookDirectionType
{
    AxisX,
    AxisY,
    Center
}

public enum PatternTargetType
{
    Squad,
    Formation
}

public abstract class SpawnPattern : ScriptableObject
{
    [SerializeField] private string patternId;
    [SerializeField] private string displayName;

    public string PatternId => patternId;
    public string DisplayName => displayName;

    public void InitializeBase(string id, string name)
    {
        patternId = id;
        displayName = name;
    }

    // 공통 추상 API
    public abstract List<SpawnPatternSlot> GetSlots();
    public abstract void ScaleAreaSize(float scale);
}
