

using System.Collections.Generic;
using UnityEngine;
using Party;

public class PartyAnchorService
{
    public struct PartyAnchorData
    {
        public Vector2 AnchorPosition;
        public Vector2 PartyCenterPosition;
        public Vector2 ZoneCenterPosition;
        public Vector2 ForwardDirection;
        public int MemberCount;
        public float MaxDistanceFromAnchor;
        public bool HasZone;
    }

    public PartyAnchorData BuildAnchor(IReadOnlyList<PartyMovementMono> members)
    {
        PartyAnchorData result = new PartyAnchorData
        {
            AnchorPosition = Vector2.zero,
            PartyCenterPosition = Vector2.zero,
            ZoneCenterPosition = Vector2.zero,
            ForwardDirection = Vector2.right,
            MemberCount = 0,
            MaxDistanceFromAnchor = 0f,
            HasZone = false
        };

        if (members == null || members.Count == 0)
        {
            return result;
        }

        Vector2 center = Vector2.zero;
        int validCount = 0;

        for (int i = 0; i < members.Count; i++)
        {
            PartyMovementMono member = members[i];
            if (member == null)
            {
                continue;
            }

            center += (Vector2)member.transform.position;
            validCount++;
        }

        if (validCount == 0)
        {
            return result;
        }

        center /= validCount;
        result.PartyCenterPosition = center;

        float maxDistance = 0f;
        Vector2 averageForward = Vector2.zero;

        for (int i = 0; i < members.Count; i++)
        {
            PartyMovementMono member = members[i];
            if (member == null)
            {
                continue;
            }

            Vector2 position = member.transform.position;
            float distance = Vector2.Distance(position, center);

            if (distance > maxDistance)
            {
                maxDistance = distance;
            }

            averageForward += Vector2.right;
        }

        Vector2 anchorPosition = center;

        PartyManager partyManager = PartyManager.Instance;

        if (partyManager != null)
        {
            List<Transform> enemies = new();

            for (int i = 0; i < members.Count; i++)
            {
                PartyMovementMono member = members[i];

                if (member == null)
                {
                    continue;
                }

                PerceptionMono perception =
                    member.GetComponent<PerceptionMono>();

                if (perception == null)
                {
                    continue;
                }

                Transform enemy = perception.ClosestEnemy;

                if (enemy != null && !enemies.Contains(enemy))
                {
                    enemies.Add(enemy);
                }
            }

            BattleZoneService.BattleZoneData zone =
                partyManager.GetCurrentBattleZone(enemies);

            if (zone != null)
            {
                result.HasZone = true;
                result.ZoneCenterPosition = zone.Center;

                Vector2 direction = zone.Center - center;

                if (direction.sqrMagnitude > 0.0001f)
                {
                    direction.Normalize();
                    result.ForwardDirection = direction;
                }

                anchorPosition =
                    partyManager.ResolveSafeBattleAnchorPosition(
                        zone,
                        center);
            }
        }

        result.AnchorPosition = anchorPosition;
        result.MemberCount = validCount;
        result.MaxDistanceFromAnchor = maxDistance;

        if (!result.HasZone && averageForward.sqrMagnitude > 0.0001f)
        {
            result.ForwardDirection = averageForward.normalized;
        }

        return result;
    }

    public bool IsOutOfFormation(
        Vector2 currentPosition,
        Vector2 anchorPosition,
        float maxDistance)
    {
        return Vector2.Distance(
                   currentPosition,
                   anchorPosition) > maxDistance;
    }

    public Vector2 ClampToAnchor(
        Vector2 targetPosition,
        Vector2 anchorPosition,
        float maxDistance)
    {
        Vector2 offset = targetPosition - anchorPosition;

        if (offset.magnitude <= maxDistance)
        {
            return targetPosition;
        }

        return anchorPosition + offset.normalized * maxDistance;
    }
}