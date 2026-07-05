#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class JsonSpawnPatternSlot
{
    public Vector2 localPosition;
    public float rotation;
}

[Serializable]
public class JsonSpawnPattern
{
    public string patternId;
    public string patternKind;
    public string patternType; // "FixedPosition" or "RangeRandom"
    public string displayName;
    
    public List<JsonSpawnPatternSlot> positions;
    
    public string shape; // "Circle" or "Rectangle"
    public Vector2 areaSize;
    public int quantity; // 레거시 JSON 호환용. 런타임 패턴 데이터에는 저장하지 않음.
    public int count;
    public int rows;
    public int columns;
    public float size;
    public float spacing;

    public float rotation;
    public float scale;
}

public static class SpawnPatternBuilder
{
    public static SpawnPatternData Build(JsonSpawnPattern data)
    {
        if (data == null) return null;

        SpawnPatternKind kind = ResolveKind(data);
        if (kind == SpawnPatternKind.None)
        {
            return null;
        }

        bool isFixedType = kind.IsFixedSlotKind();

        if (isFixedType)
        {
            List<SpawnPatternSlot> finalSlots = BuildFixedSlots(kind, data);

            return new SpawnPatternData(
                data.patternId,
                data.displayName,
                kind,
                finalSlots);
        }
        else
        {
            SpawnAreaShape shapeVal = kind.ResolveAreaShape(ParseShape(data.shape));
            float scaleVal = data.scale <= 0f ? 1f : data.scale;
            Vector2 scaledAreaSize = data.areaSize * scaleVal;

            return new SpawnPatternData(
                data.patternId,
                data.displayName,
                kind,
                shapeVal,
                scaledAreaSize);
        }
    }

    private static SpawnPatternKind ResolveKind(JsonSpawnPattern data)
    {
        if (!string.IsNullOrEmpty(data.patternKind) &&
            Enum.TryParse(data.patternKind, true, out SpawnPatternKind parsedKind))
        {
            return parsedKind;
        }

        bool isRandomType = !string.IsNullOrEmpty(data.patternType) &&
            data.patternType.Equals("RangeRandom", StringComparison.OrdinalIgnoreCase);
        if (isRandomType)
        {
            return ResolveRandomKind(ParseShape(data.shape));
        }

        return ResolveFixedKind(data.patternId, data.positions != null ? data.positions.Count : 0);
    }

    private static SpawnAreaShape ParseShape(string shape)
    {
        if (!string.IsNullOrEmpty(shape) && shape.Equals("Rectangle", StringComparison.OrdinalIgnoreCase))
        {
            return SpawnAreaShape.Rectangle;
        }

        return SpawnAreaShape.Circle;
    }

    private static SpawnPatternKind ResolveFixedKind(string patternId, int slotCount)
    {
        string id = patternId ?? string.Empty;
        if (id.IndexOf(".circle.", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return SpawnPatternKind.Circle;
        }

        if (id.IndexOf(".grid.", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return SpawnPatternKind.Grid;
        }

        if (id.IndexOf(".triangle.", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return SpawnPatternKind.Triangle;
        }

        if (id.IndexOf(".line.", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return SpawnPatternKind.Line;
        }

        if (id.IndexOf(".origin", StringComparison.OrdinalIgnoreCase) >= 0 || slotCount == 1)
        {
            return SpawnPatternKind.Point;
        }

        return SpawnPatternKind.Fixed;
    }

    private static SpawnPatternKind ResolveRandomKind(SpawnAreaShape shape)
    {
        return shape == SpawnAreaShape.Rectangle
            ? SpawnPatternKind.RandomRectangle
            : SpawnPatternKind.RandomCircle;
    }

    private static List<SpawnPatternSlot> BuildFixedSlots(SpawnPatternKind kind, JsonSpawnPattern data)
    {
        List<SpawnPatternSlot> rawSlots = data.positions != null && data.positions.Count > 0
            ? BuildLegacyPositionSlots(data.positions)
            : GenerateSlots(kind, data);

        return ApplyTransform(rawSlots, data.rotation, data.scale <= 0f ? 1f : data.scale);
    }

    private static List<SpawnPatternSlot> BuildLegacyPositionSlots(List<JsonSpawnPatternSlot> positions)
    {
        List<SpawnPatternSlot> slots = new List<SpawnPatternSlot>();
        foreach (JsonSpawnPatternSlot pos in positions)
        {
            if (pos == null) continue;
            slots.Add(new SpawnPatternSlot(pos.localPosition, pos.rotation));
        }
        return slots;
    }

    private static List<SpawnPatternSlot> GenerateSlots(SpawnPatternKind kind, JsonSpawnPattern data)
    {
        switch (kind)
        {
            case SpawnPatternKind.Point:
                return GeneratePoint();
            case SpawnPatternKind.Line:
                return GenerateLine(data);
            case SpawnPatternKind.Circle:
                return GenerateCircle(data);
            case SpawnPatternKind.Grid:
                return GenerateGrid(data);
            case SpawnPatternKind.Triangle:
                return GenerateTriangle(data);
            default:
                return GeneratePoint();
        }
    }

    private static List<SpawnPatternSlot> GeneratePoint()
    {
        return new List<SpawnPatternSlot> { new SpawnPatternSlot(Vector2.zero, 0f) };
    }

    private static List<SpawnPatternSlot> GenerateLine(JsonSpawnPattern data)
    {
        int count = Mathf.Max(1, data.count > 0 ? data.count : 3);
        float spacing = ResolveSpacing(data, Mathf.Max(1, count - 1), 1f);
        List<SpawnPatternSlot> slots = new List<SpawnPatternSlot>();
        float center = (count - 1) * 0.5f;
        for (int i = 0; i < count; i++)
        {
            slots.Add(new SpawnPatternSlot(new Vector2((i - center) * spacing, 0f), 0f));
        }
        return slots;
    }

    private static List<SpawnPatternSlot> GenerateCircle(JsonSpawnPattern data)
    {
        int count = Mathf.Max(1, data.count > 0 ? data.count : 6);
        float radius = data.size > 0f ? data.size : 1f;
        List<SpawnPatternSlot> slots = new List<SpawnPatternSlot>();
        for (int i = 0; i < count; i++)
        {
            float angleDeg = count == 1 ? 0f : (360f / count) * i;
            float angleRad = angleDeg * Mathf.Deg2Rad;
            Vector2 position = new Vector2(Mathf.Cos(angleRad) * radius, Mathf.Sin(angleRad) * radius);
            slots.Add(new SpawnPatternSlot(position, NormalizeAngle(270f + angleDeg)));
        }
        return slots;
    }

    private static List<SpawnPatternSlot> GenerateGrid(JsonSpawnPattern data)
    {
        int rows = data.rows > 0 ? data.rows : 0;
        int columns = data.columns > 0 ? data.columns : 0;
        int count = data.count > 0 ? data.count : 0;

        if (rows <= 0 && columns <= 0)
        {
            int side = Mathf.Max(1, Mathf.CeilToInt(Mathf.Sqrt(count > 0 ? count : 9)));
            rows = side;
            columns = side;
        }
        else if (rows <= 0)
        {
            rows = Mathf.Max(1, Mathf.CeilToInt((float)Mathf.Max(1, count) / columns));
        }
        else if (columns <= 0)
        {
            columns = Mathf.Max(1, Mathf.CeilToInt((float)Mathf.Max(1, count) / rows));
        }

        count = count > 0 ? Mathf.Min(count, rows * columns) : rows * columns;
        float spacing = ResolveSpacing(data, Mathf.Max(rows, columns) - 1, 1f);
        List<SpawnPatternSlot> slots = new List<SpawnPatternSlot>();
        for (int r = 0; r < rows && slots.Count < count; r++)
        {
            for (int c = 0; c < columns && slots.Count < count; c++)
            {
                float x = (c - (columns - 1) * 0.5f) * spacing;
                float y = ((rows - 1) * 0.5f - r) * spacing;
                slots.Add(new SpawnPatternSlot(new Vector2(x, y), 0f));
            }
        }
        return slots;
    }

    private static List<SpawnPatternSlot> GenerateTriangle(JsonSpawnPattern data)
    {
        int rows = data.rows > 0 ? data.rows : ResolveTriangleRows(data.count > 0 ? data.count : 3);
        int count = data.count > 0 ? data.count : rows * (rows + 1) / 2;
        float spacing = data.spacing > 0f ? data.spacing : (data.size > 0f ? data.size : 1.2f);
        List<SpawnPatternSlot> slots = new List<SpawnPatternSlot>();
        for (int r = 0; r < rows && slots.Count < count; r++)
        {
            int columns = r + 1;
            float y = ((rows - 1) * 0.5f - r) * spacing;
            for (int c = 0; c < columns && slots.Count < count; c++)
            {
                float x = (c - r * 0.5f) * spacing;
                slots.Add(new SpawnPatternSlot(new Vector2(x, y), NormalizeAngle(c * 120f)));
            }
        }
        return slots;
    }

    private static int ResolveTriangleRows(int count)
    {
        int rows = 1;
        while ((rows * (rows + 1)) / 2 < count)
        {
            rows++;
        }
        return rows;
    }

    private static float ResolveSpacing(JsonSpawnPattern data, int segmentCount, float fallback)
    {
        if (data.spacing > 0f)
        {
            return data.spacing;
        }

        if (data.size > 0f && segmentCount > 0)
        {
            return data.size / segmentCount;
        }

        return fallback;
    }

    private static List<SpawnPatternSlot> ApplyTransform(List<SpawnPatternSlot> rawSlots, float rotation, float scale)
    {
        List<SpawnPatternSlot> finalSlots = new List<SpawnPatternSlot>();
        foreach (SpawnPatternSlot slot in rawSlots)
        {
            if (slot == null) continue;
            Vector2 scaled = slot.LocalPosition * scale;
            Vector2 rotated = SpawnCoordinateUtility.Rotate(scaled, rotation);
            finalSlots.Add(new SpawnPatternSlot(rotated, NormalizeAngle(slot.LocalRotation + rotation)));
        }
        return finalSlots;
    }

    private static float NormalizeAngle(float angle)
    {
        return (angle % 360f + 360f) % 360f;
    }
}
#endif
