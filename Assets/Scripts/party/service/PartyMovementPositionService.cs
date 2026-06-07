using UnityEngine;

internal class PartyMovementPositionService
{
    public struct Context
    {
        public PartyMovementMono.PositionMode Mode;
        public Vector2 SelfPos;
        public Vector2 PartyAnchorPosition;
        public float MaxDistanceFromParty;
        public Transform OwnerTransform;
        public string OwnerName;
        public PerceptionMono Perception;
        public Vector2 JitterDir;
        public float Jitter01;
        public float EngageDistance;
        public float PatrolDistance;
        public float PatrolAreaRadius;
        public float PositionRadius;
        public float SafeDistance;
        public float SafeAreaRadius;
        public float ProtectDistance;
        public float ProtectAreaRadius;
        public float AggressiveDistance;
        public float AggressiveAreaRadius;
        public float PositionJitterStrength;
        public float SeparationRadius;
        public float SeparationStrength;
        public LayerMask PartyMask;
        public LayerMask ObstacleMask;
        public bool DebugDraw;
    }

    private readonly MonoBehaviour _owner;
    private Transform _cachedTower;

    public PartyMovementPositionService(MonoBehaviour owner)
    {
        _owner = owner;
    }

    public Vector2 ComputeTargetPosition(Context context)
    {
        Transform closestEnemy = context.Perception != null
            ? context.Perception.ClosestEnemy
            : null;

        bool hasEnemyInRange = false;
        Vector2 threatDir = context.JitterDir.sqrMagnitude > 0.001f
            ? context.JitterDir.normalized
            : Vector2.right;

        if (closestEnemy != null)
        {
            Vector2 toEnemy = (Vector2)closestEnemy.position - context.SelfPos;
            if (toEnemy.sqrMagnitude > 0.0001f)
            {
                float enemyDistance = toEnemy.magnitude;
                hasEnemyInRange = enemyDistance <= context.EngageDistance;
                threatDir = toEnemy.normalized;
            }
        }

        if (context.Mode == PartyMovementMono.PositionMode.PatrolAroundTower)
        {
            return ComputeTowerPatrolPosition(context);
        }

        Vector2 sideDir = new Vector2(-threatDir.y, threatDir.x);

        Vector2 anchor;
        float areaRadius;

        switch (context.Mode)
        {
            case PartyMovementMono.PositionMode.SafePosition:
                anchor = context.SelfPos - threatDir * context.SafeDistance;
                areaRadius = context.SafeAreaRadius;
                break;

            case PartyMovementMono.PositionMode.ProtectPosition:
                anchor = context.SelfPos + threatDir * context.ProtectDistance;
                areaRadius = context.ProtectAreaRadius;
                break;

            case PartyMovementMono.PositionMode.AggressivePosition:
                anchor = context.PartyAnchorPosition;
                areaRadius = 0f;
                break;

            default:
                return ComputeTowerPatrolPosition(context);
        }

        Vector2 target = anchor;

        if (context.Mode != PartyMovementMono.PositionMode.AggressivePosition)
        {
            float jitterScale = Mathf.Max(0f, context.PositionJitterStrength)
                * Mathf.Max(0f, areaRadius)
                * context.Jitter01;
            float sideSign = context.JitterDir.x >= 0f ? 1f : -1f;
            float forwardBias = context.JitterDir.y * 0.35f;

            Vector2 jitterOffset =
                (sideDir * sideSign + threatDir * forwardBias).normalized
                * jitterScale;

            target += jitterOffset;
            target += ComputeSeparationOffset(context, target);
        }

        if (context.Mode == PartyMovementMono.PositionMode.AggressivePosition)
        {
            target = PreventAggressiveBackstep(
                context,
                target,
                threatDir);
        }

        target = ClampByPositionRadius(context, target);
        target = ClampByPartyAnchor(context, target);
        return ApplyWallAvoidance(context, target);
    }

    private Vector2 ComputeAggressiveAnchor(
        Context context,
        Transform closestEnemy,
        Vector2 threatDir)
    {
        if (closestEnemy == null)
        {
            return context.PartyAnchorPosition;
        }

        Vector2 enemyPosition = closestEnemy.position;
        Vector2 fromEnemy = context.SelfPos - enemyPosition;

        if (fromEnemy.sqrMagnitude <= 0.0001f)
        {
            fromEnemy = -threatDir;
        }

        fromEnemy.Normalize();

        float desiredDistance = Mathf.Max(0.1f, context.AggressiveDistance);
        float currentDistance = Vector2.Distance(
            context.SelfPos,
            enemyPosition);
        float tolerance = Mathf.Max(
            0.15f,
            context.AggressiveAreaRadius * 0.35f);

        if (currentDistance < desiredDistance - tolerance)
        {
            return enemyPosition + fromEnemy * desiredDistance;
        }

        if (currentDistance > desiredDistance + tolerance)
        {
            return enemyPosition + fromEnemy * desiredDistance;
        }

        Vector2 sideDir = new Vector2(-fromEnemy.y, fromEnemy.x);
        float sideSign = context.JitterDir.x >= 0f ? 1f : -1f;

        return context.SelfPos
            + sideDir
            * sideSign
            * Mathf.Max(0.05f, context.AggressiveAreaRadius * 0.2f);
    }

    private Vector2 ComputeTowerPatrolPosition(Context context)
    {
        Transform tower = FindNearestTower(
            context.SelfPos,
            context.DebugDraw,
            context.OwnerName);

        if (tower == null)
        {
            return context.SelfPos;
        }

        Vector2 towerPos = tower.position;
        Vector2 baseDir = context.SelfPos - towerPos;
        if (baseDir.sqrMagnitude <= 0.0001f)
        {
            baseDir = context.JitterDir.sqrMagnitude > 0.001f
                ? context.JitterDir
                : Vector2.right;
        }

        baseDir.Normalize();
        Vector2 sideDir = new Vector2(-baseDir.y, baseDir.x);
        Vector2 anchor = towerPos + baseDir * context.PatrolDistance;

        float jitterScale = Mathf.Max(0f, context.PositionJitterStrength)
            * Mathf.Max(0f, context.PatrolAreaRadius)
            * context.Jitter01;
        float sideSign = context.JitterDir.x >= 0f ? 1f : -1f;
        float forwardBias = context.JitterDir.y * 0.35f;

        Vector2 jitterOffset = (sideDir * sideSign + baseDir * forwardBias).normalized * jitterScale;
        Vector2 target = anchor + jitterOffset;

        target += ComputeSeparationOffset(context, target);
        target = ClampByPositionRadius(context, target);
        target = ClampByPartyAnchor(context, target);
        return ApplyWallAvoidance(context, target);
    }

    public Transform FindNearestTower(
        Vector2 selfPos,
        bool debugDraw,
        string ownerName)
    {
        if (_cachedTower != null)
        {
            return _cachedTower;
        }

        _cachedTower = FindInitialTower(selfPos, debugDraw, ownerName);
        return _cachedTower;
    }

    private Transform FindInitialTower(
        Vector2 selfPos,
        bool debugDraw,
        string ownerName)
    {
        Transform best = null;
        float bestDistance = float.MaxValue;

        TowerPropMono[] towers = Object.FindObjectsByType<TowerPropMono>(FindObjectsSortMode.None);
        for (int i = 0; i < towers.Length; i++)
        {
            TowerPropMono tower = towers[i];
            if (tower == null)
            {
                continue;
            }

            float distance = Vector2.Distance(selfPos, tower.transform.position);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = tower.transform;
            }
        }

        if (best != null)
        {
            return best;
        }

        int towerLayer = LayerMask.NameToLayer("Tower");
        if (towerLayer >= 0)
        {
            Transform[] allTransforms = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
            for (int i = 0; i < allTransforms.Length; i++)
            {
                Transform transform = allTransforms[i];
                if (transform == null || transform.gameObject.layer != towerLayer)
                {
                    continue;
                }

                float distance = Vector2.Distance(selfPos, transform.position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = transform;
                }
            }
        }

        if (best == null && debugDraw && _owner != null)
        {
            Debug.LogWarning($"[PartyMovementPositionService] Could not find tower for {ownerName}. Check TowerPropMono or layer named 'Tower'.", _owner);
        }

        return best;
    }

    private Vector2 ComputeSeparationOffset(Context context, Vector2 desiredTarget)
    {
        float radius = Mathf.Max(0.01f, context.SeparationRadius);
        float strength = Mathf.Max(0f, context.SeparationStrength);
        if (strength <= 0f)
        {
            return Vector2.zero;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            context.OwnerTransform.position,
            radius,
            context.PartyMask);

        if (hits == null || hits.Length == 0)
        {
            return Vector2.zero;
        }

        Vector2 push = Vector2.zero;
        int count = 0;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D collider = hits[i];
            if (collider == null || collider.isTrigger)
            {
                continue;
            }

            Transform otherTransform = collider.transform;
            if (otherTransform == context.OwnerTransform || otherTransform.IsChildOf(context.OwnerTransform))
            {
                continue;
            }

            PartyMovementMono other = otherTransform.GetComponentInParent<PartyMovementMono>();
            if (other == null || other.transform == context.OwnerTransform)
            {
                continue;
            }

            Vector2 otherPos = other.transform.position;
            Vector2 away = desiredTarget - otherPos;
            float distance = away.magnitude;
            if (distance <= 0.0001f)
            {
                away = context.JitterDir.sqrMagnitude > 0.001f
                    ? context.JitterDir
                    : Vector2.right;
                distance = 0.0001f;
            }

            if (distance > radius)
            {
                continue;
            }

            float weight = 1f - Mathf.Clamp01(distance / radius);
            push += away.normalized * weight;
            count++;
        }

        if (count <= 0 || push.sqrMagnitude <= 0.0001f)
        {
            return Vector2.zero;
        }

        return push.normalized * strength;
    }

    private Vector2 PreventAggressiveBackstep(
        Context context,
        Vector2 target,
        Vector2 threatDir)
    {
        if (threatDir.sqrMagnitude <= 0.0001f)
        {
            return target;
        }

        Vector2 selfPosition = context.SelfPos;
        Vector2 toTarget = target - selfPosition;
        Vector2 forward = threatDir.normalized;

        float forwardAmount = Vector2.Dot(
            toTarget,
            forward);

        if (forwardAmount >= 0f)
        {
            return target;
        }

        return selfPosition
            + forward * Mathf.Max(0.25f, context.AggressiveDistance * 0.25f);
    }

    private Vector2 ApplyWallAvoidance(Context context, Vector2 target)
    {
        if (context.ObstacleMask.value == 0)
        {
            return target;
        }

        Vector2 currentPosition = context.SelfPos;
        float bodyRadius = 0.35f;

        Collider2D targetBlocked = Physics2D.OverlapCircle(
            target,
            bodyRadius,
            context.ObstacleMask);

        if (context.Mode == PartyMovementMono.PositionMode.AggressivePosition && targetBlocked == null)
        {
            return target;
        }

        Vector2 moveDirection = target - currentPosition;
        RaycastHit2D pathHit = default;

        if (moveDirection.sqrMagnitude > 0.0001f)
        {
            float moveDistance = moveDirection.magnitude;
            moveDirection.Normalize();

            pathHit = Physics2D.CircleCast(
                currentPosition,
                bodyRadius,
                moveDirection,
                Mathf.Max(0.75f, moveDistance),
                context.ObstacleMask);
        }

        if (targetBlocked == null && !pathHit.collider)
        {
            return target;
        }

        Vector2 alternative = FindReachableWallAvoidanceTarget(
            context,
            currentPosition,
            pathHit.collider ? pathHit.normal : Vector2.zero,
            bodyRadius);

        if (alternative != Vector2.zero)
        {
            return alternative;
        }

        return currentPosition;
    }

    private Vector2 FindReachableWallAvoidanceTarget(
        Context context,
        Vector2 currentPosition,
        Vector2 wallNormal,
        float bodyRadius)
    {
        Vector2 toAnchor = context.PartyAnchorPosition - currentPosition;
        Vector2 anchorDirection = toAnchor.sqrMagnitude > 0.0001f
            ? toAnchor.normalized
            : Vector2.zero;

        Vector2 normalDirection = wallNormal.sqrMagnitude > 0.0001f
            ? wallNormal.normalized
            : Vector2.zero;

        Vector2 tangentA = normalDirection.sqrMagnitude > 0.0001f
            ? new Vector2(-normalDirection.y, normalDirection.x)
            : Vector2.zero;
        Vector2 tangentB = -tangentA;

        Vector2[] directions =
        {
            normalDirection,
            anchorDirection,
            tangentA,
            tangentB,
            Vector2.down,
            Vector2.up,
            Vector2.left,
            Vector2.right
        };

        float searchDistance = Mathf.Max(0.75f, context.PositionRadius);

        for (int i = 0; i < directions.Length; i++)
        {
            Vector2 dir = directions[i];
            if (dir.sqrMagnitude <= 0.0001f)
            {
                continue;
            }

            dir.Normalize();
            Vector2 candidate = currentPosition + dir * searchDistance;

            if (IsReachableAndFree(
                    currentPosition,
                    candidate,
                    bodyRadius,
                    context.ObstacleMask))
            {
                return candidate;
            }
        }

        return Vector2.zero;
    }

    private bool IsReachableAndFree(
        Vector2 from,
        Vector2 target,
        float bodyRadius,
        LayerMask obstacleMask)
    {
        Collider2D blocked = Physics2D.OverlapCircle(
            target,
            bodyRadius,
            obstacleMask);

        if (blocked != null)
        {
            return false;
        }

        Vector2 path = target - from;

        if (path.sqrMagnitude <= 0.0001f)
        {
            return false;
        }

        RaycastHit2D hit = Physics2D.CircleCast(
            from,
            bodyRadius,
            path.normalized,
            path.magnitude,
            obstacleMask);

        return !hit.collider;
    }

    private Vector2 ClampByPartyAnchor(Context context, Vector2 target)
    {
        if (context.MaxDistanceFromParty <= 0f)
        {
            return target;
        }

        Vector2 anchor = context.PartyAnchorPosition;

        if (anchor.sqrMagnitude <= 0.0001f)
        {
            return target;
        }

        float maxDistance = Mathf.Max(0.05f, context.MaxDistanceFromParty);
        Vector2 offset = target - anchor;

        if (offset.magnitude > maxDistance)
        {
            target = anchor + offset.normalized * maxDistance;
        }

        float maxVerticalDistance = Mathf.Max(0.35f, maxDistance * 0.3f);
        target.y = Mathf.Clamp(
            target.y,
            anchor.y - maxVerticalDistance,
            anchor.y + maxVerticalDistance);

        return target;
    }

    private Vector2 ClampByPositionRadius(Context context, Vector2 target)
    {
        Vector2 offset = target - context.SelfPos;
        float magnitude = offset.magnitude;
        float positionRadius = Mathf.Max(0.05f, context.PositionRadius);

        if (magnitude > positionRadius)
        {
            target = context.SelfPos + offset.normalized * positionRadius;
        }

        return target;
    }
}