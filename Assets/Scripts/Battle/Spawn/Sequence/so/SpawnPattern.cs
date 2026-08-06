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

[Serializable]
public abstract class SpawnPatternConfig
{
}

[Serializable]
public sealed class FixedSpawnPatternConfig : SpawnPatternConfig
{
    [SerializeField] private List<SpawnPatternSlot> slots = new List<SpawnPatternSlot>();

    public IReadOnlyList<SpawnPatternSlot> Slots => slots;

    public FixedSpawnPatternConfig()
    {
        slots = new List<SpawnPatternSlot>();
    }

    public FixedSpawnPatternConfig(List<SpawnPatternSlot> slotsVal)
    {
        slots = slotsVal ?? new List<SpawnPatternSlot>();
    }
}

[Serializable]
public sealed class RandomSpawnPatternConfig : SpawnPatternConfig
{
    [SerializeField] private SpawnAreaShape shape = SpawnAreaShape.Circle;
    [SerializeField] private Vector2 areaSize = new Vector2(1f, 1f);

    public SpawnAreaShape Shape => shape;
    public Vector2 AreaSize => areaSize;

    public RandomSpawnPatternConfig()
    {
        shape = SpawnAreaShape.Circle;
        areaSize = new Vector2(1f, 1f);
    }

    public RandomSpawnPatternConfig(SpawnAreaShape shapeVal, Vector2 areaSizeVal)
    {
        shape = shapeVal;
        areaSize = areaSizeVal;
    }
}

[Serializable]
public sealed class SpawnPatternData
{
    [SerializeField] private string patternId;
    [SerializeField] private string displayName;
    [SerializeField] private SpawnPatternKind patternKind;
    [SerializeReference] private SpawnPatternConfig config;

    public string PatternId => patternId;
    public string DisplayName => displayName;
    public SpawnPatternKind PatternKind => patternKind;
    public SpawnPatternConfig Config => config;
    public FixedSpawnPatternConfig FixedConfig => patternKind.IsFixedSlotKind() ? config as FixedSpawnPatternConfig : null;
    public RandomSpawnPatternConfig RandomConfig => patternKind.IsRandomAreaKind() ? config as RandomSpawnPatternConfig : null;
    public bool HasPattern => patternKind != SpawnPatternKind.None;

    public TConfig GetConfig<TConfig>() where TConfig : SpawnPatternConfig
    {
        return config as TConfig;
    }

    public SpawnPatternData()
    {
        patternKind = SpawnPatternKind.None;
        config = null;
    }

    public SpawnPatternData(string id, string name, List<SpawnPatternSlot> slotsVal)
        : this(id, name, SpawnPatternKind.Fixed, slotsVal)
    {
    }

    public SpawnPatternData(string id, string name, SpawnPatternKind kind, List<SpawnPatternSlot> slotsVal)
    {
        patternId = id;
        displayName = name;
        patternKind = kind.IsFixedSlotKind() ? kind : SpawnPatternKind.Fixed;
        config = new FixedSpawnPatternConfig(slotsVal);
    }

    public SpawnPatternData(string id, string name, SpawnAreaShape shapeVal, Vector2 areaSizeVal)
        : this(id, name, ShapeToRandomKind(shapeVal), shapeVal, areaSizeVal)
    {
    }

    public SpawnPatternData(string id, string name, SpawnPatternKind kind, SpawnAreaShape shapeVal, Vector2 areaSizeVal)
    {
        patternId = id;
        displayName = name;
        patternKind = kind.IsRandomAreaKind() ? kind : ShapeToRandomKind(shapeVal);
        config = new RandomSpawnPatternConfig(shapeVal, areaSizeVal);
    }

    public static SpawnPatternData None()
    {
        return new SpawnPatternData();
    }

    private static SpawnPatternKind ShapeToRandomKind(SpawnAreaShape shapeVal)
    {
        return shapeVal == SpawnAreaShape.Rectangle
            ? SpawnPatternKind.RandomRectangle
            : SpawnPatternKind.RandomCircle;
    }

}

public enum SpawnPatternKind
{
    None,
    Fixed,
    Random,
    Point,
    Line,
    Circle,
    Grid,
    Triangle,
    RandomCircle,
    RandomRectangle
}

public static class SpawnPatternKindExtensions
{
    public static bool IsFixedSlotKind(this SpawnPatternKind kind)
    {
        switch (kind)
        {
            case SpawnPatternKind.Fixed:
            case SpawnPatternKind.Point:
            case SpawnPatternKind.Line:
            case SpawnPatternKind.Circle:
            case SpawnPatternKind.Grid:
            case SpawnPatternKind.Triangle:
                return true;
            default:
                return false;
        }
    }

    public static bool IsRandomAreaKind(this SpawnPatternKind kind)
    {
        switch (kind)
        {
            case SpawnPatternKind.Random:
            case SpawnPatternKind.RandomCircle:
            case SpawnPatternKind.RandomRectangle:
                return true;
            default:
                return false;
        }
    }

    public static SpawnAreaShape ResolveAreaShape(this SpawnPatternKind kind, SpawnAreaShape configShape)
    {
        if (kind == SpawnPatternKind.RandomCircle)
        {
            return SpawnAreaShape.Circle;
        }

        if (kind == SpawnPatternKind.RandomRectangle)
        {
            return SpawnAreaShape.Rectangle;
        }

        return configShape;
    }
}
