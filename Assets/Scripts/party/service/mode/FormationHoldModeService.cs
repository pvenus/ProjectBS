using UnityEngine;

internal class FormationHoldModeService
{
    private const float ForcedMeleeFrontBackOffset = 1.0f;
    private const float ForcedMeleeHorizontalLimit = 1.2f;
    private const float ForcedMeleeVerticalLimit = 0.35f;
    private static readonly Vector2[] WallAvoidanceDirections =
    {
        Vector2.left,
        Vector2.right,
        Vector2.down,
        Vector2.up,
        new Vector2(-1f, -1f).normalized,
        new Vector2(1f, -1f).normalized
    };
    public struct Context
    {
        public Vector2 SelfPosition;
        public Vector2 PartyAnchorPosition;
        public Vector2 PartyForwardDirection;
        public Vector2 JitterDirection;
        public LayerMask ObstacleMask;
        public float FormationRadius;
        public float SlotSpacing;
        public float FrontBackOffset;
        public float HorizontalLimit;
        public float VerticalLimit;
        public int FormationIndex;
        public int MemberCount;
    }

    public Vector2 ResolvePosition(Context context)
    {
        Vector2 anchor = context.PartyAnchorPosition;

        if (context.MemberCount <= 0)
        {
            return context.SelfPosition;
        }

        Vector2 forward = ResolveForward(context.PartyForwardDirection);
        Vector2 horizontal = Vector2.right;
        Vector2 vertical = Vector2.up;

        int index = Mathf.Max(0, context.FormationIndex);
        int count = Mathf.Max(1, context.MemberCount);

        float centeredIndex = index - (count - 1) * 0.5f;
        float spacing = Mathf.Max(0.1f, context.SlotSpacing);

        Vector2 slotOffset = horizontal * centeredIndex * spacing
            + forward * ForcedMeleeFrontBackOffset;
        Vector2 jitterOffset = ResolveJitterOffset(
            context.JitterDirection,
            horizontal,
            vertical,
            context.FormationRadius);

        Vector2 target = anchor + slotOffset + jitterOffset;
        target = ShiftTargetAwayFromWall(
            context,
            target,
            anchor,
            horizontal);

        return ClampAroundAnchor(
            context,
            target,
            anchor,
            context.FormationRadius);
    }

    private Vector2 ResolveForward(Vector2 rawForward)
    {
        if (rawForward.sqrMagnitude <= 0.0001f)
        {
            return Vector2.right;
        }

        return rawForward.normalized;
    }

    private Vector2 ResolveJitterOffset(
        Vector2 jitterDirection,
        Vector2 horizontal,
        Vector2 vertical,
        float formationRadius)
    {
        if (jitterDirection.sqrMagnitude <= 0.0001f)
        {
            return Vector2.zero;
        }

        float radius = Mathf.Max(0f, formationRadius);
        float horizontalAmount = Mathf.Clamp(jitterDirection.x, -1f, 1f);
        float verticalAmount = Mathf.Clamp(jitterDirection.y, -1f, 1f);

        return (horizontal * horizontalAmount * 0.35f
                + vertical * verticalAmount * 0.12f)
            * radius
            * 0.2f;
    }

    private Vector2 ClampAroundAnchor(
        Context context,
        Vector2 target,
        Vector2 anchor,
        float radius)
    {
        float resolvedRadius = Mathf.Max(0.1f, radius);
        Vector2 offset = target - anchor;

        if (offset.magnitude > resolvedRadius)
        {
            target = anchor + offset.normalized * resolvedRadius;
        }

        float maxVerticalDistance = ForcedMeleeVerticalLimit;
        float maxHorizontalDistance = ForcedMeleeHorizontalLimit;

        target.x = Mathf.Clamp(
            target.x,
            anchor.x - maxHorizontalDistance,
            anchor.x + maxHorizontalDistance);

        target.y = Mathf.Clamp(
            target.y,
            anchor.y - maxVerticalDistance,
            anchor.y + maxVerticalDistance);

        return target;
    }

    private Vector2 ShiftTargetAwayFromWall(
        Context context,
        Vector2 target,
        Vector2 anchor,
        Vector2 horizontal)
    {
        if (context.ObstacleMask.value == 0)
        {
            return target;
        }

        Vector2 currentPosition = context.SelfPosition;
        Vector2 toTarget = target - currentPosition;

        if (toTarget.sqrMagnitude <= 0.0001f)
        {
            return target;
        }

        Vector2 direction = toTarget.normalized;
        float checkDistance = Mathf.Max(0.5f, toTarget.magnitude + 0.45f);
        float bodyRadius = 0.35f;

        RaycastHit2D hit = Physics2D.CircleCast(
            currentPosition,
            bodyRadius,
            direction,
            checkDistance,
            context.ObstacleMask);

        if (!hit.collider)
        {
            return target;
        }

        Vector2 alternativeTarget = FindAlternativeTarget(
            context,
            anchor,
            currentPosition);

        if (alternativeTarget != Vector2.zero)
        {
            return alternativeTarget;
        }

        Vector2 wallNormal = hit.normal;
        if (wallNormal.sqrMagnitude <= 0.0001f)
        {
            wallNormal = anchor - currentPosition;
        }

        if (wallNormal.sqrMagnitude <= 0.0001f)
        {
            wallNormal = Vector2.down;
        }

        wallNormal.Normalize();

        Vector2 toAnchor = anchor - currentPosition;
        Vector2 anchorDirection = toAnchor.sqrMagnitude > 0.0001f
            ? toAnchor.normalized
            : Vector2.zero;

        Vector2 escapeDirection = wallNormal + anchorDirection * 0.45f;

        if (Mathf.Abs(wallNormal.y) > Mathf.Abs(wallNormal.x))
        {
            escapeDirection.x *= 0.35f;
        }

        if (escapeDirection.sqrMagnitude <= 0.0001f)
        {
            escapeDirection = wallNormal;
        }

        escapeDirection.Normalize();

        float escapeDistance = Mathf.Max(
            0.75f,
            Mathf.Min(context.FormationRadius, context.SlotSpacing * 1.25f));

        Vector2 shiftedTarget = currentPosition + escapeDirection * escapeDistance;

        float maxAnchorEscapeDistance = Mathf.Max(
            0.75f,
            context.FormationRadius);

        Vector2 anchorOffset = shiftedTarget - anchor;

        if (anchorOffset.magnitude > maxAnchorEscapeDistance)
        {
            shiftedTarget =
                anchor
                + anchorOffset.normalized * maxAnchorEscapeDistance;
        }

        float maxVerticalDistance = ForcedMeleeVerticalLimit;
        float maxHorizontalDistance = ForcedMeleeHorizontalLimit;

        shiftedTarget.x = Mathf.Clamp(
            shiftedTarget.x,
            anchor.x - maxHorizontalDistance,
            anchor.x + maxHorizontalDistance);

        shiftedTarget.y = Mathf.Clamp(
            shiftedTarget.y,
            anchor.y - maxVerticalDistance,
            anchor.y + maxVerticalDistance);

        return shiftedTarget;
    }

    private Vector2 FindAlternativeTarget(
        Context context,
        Vector2 anchor,
        Vector2 currentPosition)
    {
        float searchDistance = Mathf.Max(
            0.75f,
            context.FormationRadius);

        for (int i = 0; i < WallAvoidanceDirections.Length; i++)
        {
            Vector2 candidate =
                anchor + WallAvoidanceDirections[i] * searchDistance;

            Collider2D blocked = Physics2D.OverlapCircle(
                candidate,
                0.35f,
                context.ObstacleMask);

            if (blocked != null)
            {
                continue;
            }

            Vector2 path = candidate - currentPosition;

            if (path.sqrMagnitude <= 0.0001f)
            {
                continue;
            }

            RaycastHit2D hit = Physics2D.CircleCast(
                currentPosition,
                0.35f,
                path.normalized,
                path.magnitude,
                context.ObstacleMask);

            if (!hit.collider)
            {
                return candidate;
            }
        }

        return Vector2.zero;
    }
}