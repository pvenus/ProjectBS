using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpawnLinePattern
{
    public enum LineDirectionMode
    {
        AutoPerpendicularToOffset,
        Horizontal,
        Vertical,
        CustomAngle
    }

    [Header("Line Shape")]
    [SerializeField] private Vector2 centerOffset = Vector2.zero;
    [SerializeField, Min(1)] private int spawnCount =30;
    [SerializeField, Min(0.1f)] private float spacing = 1f;

    [Header("Direction")]
    [SerializeField] private LineDirectionMode directionMode = LineDirectionMode.AutoPerpendicularToOffset;
    [SerializeField, Range(0f, 360f)] private float customAngleDeg = 0f;
    [SerializeField, Range(0f, 180f)] private float lineRotationDeg = 0f;

    [Header("Randomization")]
    [SerializeField] private bool useSpacingJitter;
    [SerializeField, Min(0f)] private float spacingJitter = 0.25f;
    [SerializeField] private bool randomizeLineRotationPerUse;
    [SerializeField, Range(0f, 180f)] private float randomRotationRangeDeg = 20f;

    [Header("Optional Center Spawn")]
    [SerializeField] private bool includeCenterSpawn;

    public Vector2 CenterOffset => centerOffset;
    public int SpawnCount => spawnCount;
    public float Spacing => spacing;
    public LineDirectionMode DirectionMode => directionMode;
    public float CustomAngleDeg => customAngleDeg;
    public float LineRotationDeg => lineRotationDeg;
    public bool UseSpacingJitter => useSpacingJitter;
    public float SpacingJitter => spacingJitter;
    public bool RandomizeLineRotationPerUse => randomizeLineRotationPerUse;
    public float RandomRotationRangeDeg => randomRotationRangeDeg;
    public bool IncludeCenterSpawn => includeCenterSpawn;

    public SpawnLinePattern SetCenterOffset(Vector2 value)
    {
        centerOffset = value;
        return this;
    }

    public SpawnLinePattern SetSpawnCount(int value)
    {
        spawnCount = Mathf.Max(1, value);
        return this;
    }

    public SpawnLinePattern SetSpacing(float value)
    {
        spacing = Mathf.Max(0.1f, value);
        return this;
    }

    public SpawnLinePattern SetDirectionMode(LineDirectionMode value)
    {
        directionMode = value;
        return this;
    }

    public SpawnLinePattern SetCustomAngle(float value)
    {
        customAngleDeg = value;
        return this;
    }

    public SpawnLinePattern SetLineRotation(float value)
    {
        lineRotationDeg = value;
        return this;
    }

    public SpawnLinePattern SetIncludeCenterSpawn(bool value)
    {
        includeCenterSpawn = value;
        return this;
    }

    public int GetTotalSpawnCount()
    {
        return spawnCount + (includeCenterSpawn ? 1 : 0);
    }

    public List<Vector2> BuildPositions(Vector2 origin)
    {
        List<Vector2> positions = new List<Vector2>(GetTotalSpawnCount());
        Vector2 center = origin + centerOffset;

        if (includeCenterSpawn)
            positions.Add(center);

        if (spawnCount <= 0)
            return positions;

        Vector2 direction = ResolveDirection(origin);
        float rotationOffset = randomizeLineRotationPerUse
            ? Random.Range(-randomRotationRangeDeg, randomRotationRangeDeg)
            : 0f;

        if (Mathf.Abs(rotationOffset) > 0.0001f || Mathf.Abs(lineRotationDeg) > 0.0001f)
            direction = Rotate(direction, lineRotationDeg + rotationOffset).normalized;

        float half = (spawnCount - 1) * 0.5f;
        for (int i = 0; i < spawnCount; i++)
        {
            float offsetIndex = i - half;
            float finalSpacing = ResolveSpacing();
            Vector2 pos = center + (direction * (offsetIndex * finalSpacing));
            positions.Add(pos);
        }

        return positions;
    }

    public Vector2 GetPosition(Vector2 origin, int index)
    {
        List<Vector2> positions = BuildPositions(origin);
        if (positions.Count == 0)
            return origin;

        index = Mathf.Clamp(index, 0, positions.Count - 1);
        return positions[index];
    }

    public void DrawGizmos(Vector2 origin, Color lineColor, Color pointColor, float pointRadius = 0.15f)
    {
        List<Vector2> positions = BuildPreviewPositions(origin);
        if (positions.Count == 0)
            return;

        Gizmos.color = lineColor;
        for (int i = 0; i < positions.Count - 1; i++)
        {
            Gizmos.DrawLine(positions[i], positions[i + 1]);
        }

        Gizmos.color = pointColor;
        for (int i = 0; i < positions.Count; i++)
        {
            Gizmos.DrawSphere(positions[i], pointRadius);
        }
    }

    private Vector2 ResolveDirection(Vector2 origin)
    {
        switch (directionMode)
        {
            case LineDirectionMode.AutoPerpendicularToOffset:
            {
                Vector2 center = origin + centerOffset;
                Vector2 offset = center - origin;

                if (offset.sqrMagnitude <= 0.0001f)
                    return Vector2.right;

                // If the spawn center is above/below the player, make a horizontal line.
                if (Mathf.Abs(offset.y) >= Mathf.Abs(offset.x))
                    return Vector2.right;

                // If the spawn center is left/right of the player, make a vertical line.
                return Vector2.up;
            }

            case LineDirectionMode.Vertical:
                return Vector2.up;

            case LineDirectionMode.CustomAngle:
                return AngleToDirection(customAngleDeg);

            case LineDirectionMode.Horizontal:
            default:
                return Vector2.right;
        }
    }

    private float ResolveSpacing()
    {
        if (!useSpacingJitter)
            return spacing;

        float delta = Random.Range(-spacingJitter, spacingJitter);
        return Mathf.Max(0.01f, spacing + delta);
    }

    private List<Vector2> BuildPreviewPositions(Vector2 origin)
    {
        List<Vector2> positions = new List<Vector2>(GetTotalSpawnCount());
        Vector2 center = origin + centerOffset;

        if (includeCenterSpawn)
            positions.Add(center);

        if (spawnCount <= 0)
            return positions;

        Vector2 direction = ResolveDirection(origin);
        if (Mathf.Abs(lineRotationDeg) > 0.0001f)
            direction = Rotate(direction, lineRotationDeg).normalized;

        float half = (spawnCount - 1) * 0.5f;
        for (int i = 0; i < spawnCount; i++)
        {
            float offsetIndex = i - half;
            Vector2 pos = center + (direction * (offsetIndex * spacing));
            positions.Add(pos);
        }

        return positions;
    }

    private static Vector2 Rotate(Vector2 v, float angleDeg)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(
            v.x * cos - v.y * sin,
            v.x * sin + v.y * cos);
    }

    private static Vector2 AngleToDirection(float angleDeg)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }
}