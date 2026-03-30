

using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpawnCirclePattern
{
    public enum AngleSpaceMode
    {
        Even,
        Random
    }

    [Header("Circle Shape")]
    [SerializeField] private Vector2 centerOffset = Vector2.zero;
    [SerializeField, Min(0.1f)] private float radius = 30f;
    [SerializeField, Min(1)] private int spawnCount = 30;

    [Header("Angle Control")]
    [SerializeField] private AngleSpaceMode angleSpaceMode = AngleSpaceMode.Even;
    [SerializeField, Range(0f, 360f)] private float startAngleDeg = 0f;
    [SerializeField, Range(0f, 180f)] private float angleJitterDeg = 0f;
    [SerializeField] private bool randomizePatternRotationPerUse;

    [Header("Optional Random Radius")]
    [SerializeField] private bool useRadiusJitter;
    [SerializeField, Min(0f)] private float minRadius = 5f;
    [SerializeField, Min(0f)] private float maxRadius = 7f;

    [Header("Optional Center Spawn")]
    [SerializeField] private bool includeCenterSpawn;

    public Vector2 CenterOffset => centerOffset;
    public float Radius => radius;
    public int SpawnCount => spawnCount;
    public AngleSpaceMode SpaceMode => angleSpaceMode;
    public float StartAngleDeg => startAngleDeg;
    public float AngleJitterDeg => angleJitterDeg;
    public bool RandomizePatternRotationPerUse => randomizePatternRotationPerUse;
    public bool UseRadiusJitter => useRadiusJitter;
    public float MinRadius => minRadius;
    public float MaxRadius => maxRadius;
    public bool IncludeCenterSpawn => includeCenterSpawn;

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

        float rotationOffset = randomizePatternRotationPerUse ? Random.Range(0f, 360f) : 0f;

        if (angleSpaceMode == AngleSpaceMode.Random)
        {
            for (int i = 0; i < spawnCount; i++)
            {
                float angle = startAngleDeg + rotationOffset + Random.Range(0f, 360f);
                angle += Random.Range(-angleJitterDeg, angleJitterDeg);

                float targetRadius = ResolveRadius();
                positions.Add(center + AngleToDirection(angle) * targetRadius);
            }

            return positions;
        }

        float step = spawnCount <= 0 ? 0f : 360f / spawnCount;
        for (int i = 0; i < spawnCount; i++)
        {
            float angle = startAngleDeg + rotationOffset + (step * i);
            angle += Random.Range(-angleJitterDeg, angleJitterDeg);

            float targetRadius = ResolveRadius();
            positions.Add(center + AngleToDirection(angle) * targetRadius);
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
        Vector2 center = origin + centerOffset;

        Gizmos.color = lineColor;
        Gizmos.DrawWireSphere(center, useRadiusJitter ? Mathf.Max(minRadius, maxRadius) : radius);

        List<Vector2> positions = BuildPreviewPositions(origin);
        Gizmos.color = pointColor;
        for (int i = 0; i < positions.Count; i++)
        {
            Gizmos.DrawSphere(positions[i], pointRadius);
        }
    }

    private float ResolveRadius()
    {
        if (!useRadiusJitter)
            return radius;

        float resolvedMin = Mathf.Min(minRadius, maxRadius);
        float resolvedMax = Mathf.Max(minRadius, maxRadius);
        return Random.Range(resolvedMin, resolvedMax);
    }

    private static Vector2 AngleToDirection(float angleDeg)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }

    private List<Vector2> BuildPreviewPositions(Vector2 origin)
    {
        List<Vector2> positions = new List<Vector2>(GetTotalSpawnCount());
        Vector2 center = origin + centerOffset;

        if (includeCenterSpawn)
            positions.Add(center);

        if (spawnCount <= 0)
            return positions;

        if (angleSpaceMode == AngleSpaceMode.Random)
        {
            float previewRadius = useRadiusJitter ? (minRadius + maxRadius) * 0.5f : radius;
            float step = 360f / spawnCount;

            for (int i = 0; i < spawnCount; i++)
            {
                float angle = startAngleDeg + (step * i);
                positions.Add(center + AngleToDirection(angle) * previewRadius);
            }

            return positions;
        }

        float evenStep = 360f / spawnCount;
        float finalRadius = useRadiusJitter ? (minRadius + maxRadius) * 0.5f : radius;
        for (int i = 0; i < spawnCount; i++)
        {
            float angle = startAngleDeg + (evenStep * i);
            positions.Add(center + AngleToDirection(angle) * finalRadius);
        }

        return positions;
    }
}