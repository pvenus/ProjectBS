

using System.Collections.Generic;
using UnityEngine;

public class BattleZoneService
{
    private static readonly Vector2[] AnchorCandidateOffsets =
    {
        Vector2.zero,
        Vector2.up,
        Vector2.down,
        Vector2.left,
        Vector2.right,
        new Vector2(1f, 1f).normalized,
        new Vector2(1f, -1f).normalized,
        new Vector2(-1f, 1f).normalized,
        new Vector2(-1f, -1f).normalized
    };
    public class BattleZoneData
    {
        public Vector2 Center;
        public List<Transform> Enemies = new();

        public float AverageDistanceFromParty;
        public Vector2 ForwardDirection;

        public int EnemyCount => Enemies.Count;
    }

    public List<BattleZoneData> BuildZones(
        IReadOnlyList<Transform> enemies,
        float clusterDistance = 3f)
    {
        List<BattleZoneData> zones = new();

        if (enemies == null || enemies.Count == 0)
        {
            return zones;
        }

        HashSet<Transform> visited = new();

        foreach (Transform enemy in enemies)
        {
            if (enemy == null || visited.Contains(enemy))
            {
                continue;
            }

            BattleZoneData zone = new BattleZoneData();
            Queue<Transform> queue = new();

            queue.Enqueue(enemy);
            visited.Add(enemy);

            while (queue.Count > 0)
            {
                Transform current = queue.Dequeue();
                zone.Enemies.Add(current);

                foreach (Transform candidate in enemies)
                {
                    if (candidate == null || visited.Contains(candidate))
                    {
                        continue;
                    }

                    if (Vector2.Distance(
                            current.position,
                            candidate.position) <= clusterDistance)
                    {
                        visited.Add(candidate);
                        queue.Enqueue(candidate);
                    }
                }
            }

            zone.Center = CalculateCenter(zone.Enemies);
            zones.Add(zone);
        }

        return zones;
    }

    public BattleZoneData SelectBestZone(
        IReadOnlyList<BattleZoneData> zones,
        IReadOnlyList<Transform> partyMembers)
    {
        if (zones == null || zones.Count == 0)
        {
            return null;
        }

        BattleZoneData bestZone = null;
        float bestScore = float.MaxValue;

        foreach (BattleZoneData zone in zones)
        {
            zone.AverageDistanceFromParty =
                CalculateAverageDistance(zone, partyMembers);
            zone.ForwardDirection = CalculateDirectionFromParty(
                zone,
                partyMembers);

            if (zone.AverageDistanceFromParty < bestScore)
            {
                bestScore = zone.AverageDistanceFromParty;
                bestZone = zone;
            }
        }

        return bestZone;
    }

    public Vector2 ResolveSafeAnchorPosition(
        BattleZoneData zone,
        Vector2 partyCenter,
        LayerMask obstacleMask,
        float distanceFromZone = 2.0f,
        float anchorRadius = 1.25f,
        float searchStep = 0.75f,
        int searchRingCount = 3)
    {
        if (zone == null)
        {
            return partyCenter;
        }

        Vector2 direction = zone.ForwardDirection.sqrMagnitude > 0.0001f
            ? zone.ForwardDirection.normalized
            : (zone.Center - partyCenter).normalized;

        if (direction.sqrMagnitude <= 0.0001f)
        {
            direction = Vector2.right;
        }

        Vector2 rawAnchor = zone.Center - direction * Mathf.Max(0.1f, distanceFromZone);

        if (obstacleMask.value == 0)
        {
            return rawAnchor;
        }

        if (IsAnchorAreaFree(rawAnchor, anchorRadius, obstacleMask))
        {
            return rawAnchor;
        }

        Vector2 bestCandidate = rawAnchor;
        float bestScore = float.MaxValue;

        for (int ring = 1; ring <= Mathf.Max(1, searchRingCount); ring++)
        {
            float distance = searchStep * ring;

            for (int i = 0; i < AnchorCandidateOffsets.Length; i++)
            {
                Vector2 candidate = rawAnchor + AnchorCandidateOffsets[i] * distance;

                if (!IsAnchorAreaFree(candidate, anchorRadius, obstacleMask))
                {
                    continue;
                }

                float score = Vector2.Distance(candidate, rawAnchor)
                    + Vector2.Distance(candidate, partyCenter) * 0.25f;

                if (score < bestScore)
                {
                    bestScore = score;
                    bestCandidate = candidate;
                }
            }

            if (bestScore < float.MaxValue)
            {
                return bestCandidate;
            }
        }

        return partyCenter;
    }

    private bool IsAnchorAreaFree(
        Vector2 position,
        float radius,
        LayerMask obstacleMask)
    {
        Collider2D hit = Physics2D.OverlapCircle(
            position,
            Mathf.Max(0.1f, radius),
            obstacleMask);

        return hit == null;
    }

    private Vector2 CalculateDirectionFromParty(
        BattleZoneData zone,
        IReadOnlyList<Transform> partyMembers)
    {
        if (zone == null || partyMembers == null || partyMembers.Count == 0)
        {
            return Vector2.right;
        }

        Vector2 partyCenter = Vector2.zero;
        int count = 0;

        for (int i = 0; i < partyMembers.Count; i++)
        {
            Transform member = partyMembers[i];
            if (member == null)
            {
                continue;
            }

            partyCenter += (Vector2)member.position;
            count++;
        }

        if (count <= 0)
        {
            return Vector2.right;
        }

        partyCenter /= count;

        Vector2 direction = zone.Center - partyCenter;
        if (direction.sqrMagnitude <= 0.0001f)
        {
            return Vector2.right;
        }

        return direction.normalized;
    }

    private Vector2 CalculateCenter(
        IReadOnlyList<Transform> enemies)
    {
        if (enemies == null || enemies.Count == 0)
        {
            return Vector2.zero;
        }

        Vector2 sum = Vector2.zero;

        foreach (Transform enemy in enemies)
        {
            sum += (Vector2)enemy.position;
        }

        return sum / enemies.Count;
    }

    private float CalculateAverageDistance(
        BattleZoneData zone,
        IReadOnlyList<Transform> partyMembers)
    {
        if (partyMembers == null || partyMembers.Count == 0)
        {
            return float.MaxValue;
        }

        float total = 0f;
        int count = 0;

        foreach (Transform member in partyMembers)
        {
            if (member == null)
            {
                continue;
            }

            total += Vector2.Distance(
                member.position,
                zone.Center);

            count++;
        }

        return count == 0
            ? float.MaxValue
            : total / count;
    }
}