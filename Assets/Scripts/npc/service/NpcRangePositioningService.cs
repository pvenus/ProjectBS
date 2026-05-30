

using UnityEngine;

namespace Npc.Service
{
    /// <summary>
    /// 원거리 NPC가 타겟 주변에서 적정 사거리/분산 위치를 유지하도록
    /// 목표 이동 지점을 계산하는 서비스.
    ///
    /// 이 클래스는 실제 이동을 수행하지 않는다.
    /// NpcPathing은 이 서비스가 계산한 desiredPoint로 이동만 수행한다.
    /// </summary>
    public class NpcRangePositioningService
    {
        public struct Context
        {
            public Vector2 selfPosition;
            public Vector2 targetPosition;
            public Transform targetTransform;

            public float preferredRange;
            public float stopDistance;
            public float minDistance;
            public float maxDistance;

            public float decisionInterval;
            public float repathDistance;
            public float ringMoveAngle;
            public float spacingRadius;
            public float spacingWeight;
            public float stopJitterRadius;

            public bool useRingMovement;
            public bool useLocalSpacing;
            public bool useStopJitter;

            public float time;
        }

        public struct Result
        {
            public bool hasDesiredPoint;
            public Vector2 desiredPoint;
            public float desiredStopDistance;
            public bool shouldStop;
            public bool refreshed;
        }

        private Vector2 currentDesiredPoint;
        private float currentDesiredStopDistance;
        private Transform lastTarget;
        private float nextDecisionTime;
        private bool hasDesiredPoint;
        private bool isHoldingAtRange;

        public void Reset()
        {
            currentDesiredPoint = Vector2.zero;
            currentDesiredStopDistance = 0f;
            lastTarget = null;
            nextDecisionTime = 0f;
            hasDesiredPoint = false;
            isHoldingAtRange = false;
        }

        public void ForceRepath()
        {
            hasDesiredPoint = false;
            isHoldingAtRange = false;
            nextDecisionTime = 0f;
        }

        public Result Evaluate(Context context)
        {
            Result result = new Result
            {
                hasDesiredPoint = false,
                desiredPoint = currentDesiredPoint,
                desiredStopDistance = currentDesiredStopDistance,
                shouldStop = false,
                refreshed = false
            };

            float distanceToTarget = Vector2.Distance(
                context.selfPosition,
                context.targetPosition);

            float stopDistance = ResolveStopDistance(context);

            if (distanceToTarget <= stopDistance)
            {
                isHoldingAtRange = true;
                result.shouldStop = true;
                result.desiredStopDistance = stopDistance;
                return result;
            }

            bool targetChanged = lastTarget != context.targetTransform;
            bool decisionTimeReached = context.time >= nextDecisionTime;
            bool desiredPointInvalid = !hasDesiredPoint;
            bool reachedDesiredPoint = hasDesiredPoint
                && Vector2.Distance(context.selfPosition, currentDesiredPoint) <= stopDistance;
            bool repathDistanceExceeded = hasDesiredPoint
                && context.repathDistance > 0f
                && Vector2.Distance(context.targetPosition, currentDesiredPoint) > context.repathDistance;

            if (targetChanged
                || decisionTimeReached
                || desiredPointInvalid
                || reachedDesiredPoint
                || repathDistanceExceeded)
            {
                currentDesiredPoint = BuildDesiredPoint(context, stopDistance);
                currentDesiredStopDistance = stopDistance;
                lastTarget = context.targetTransform;
                nextDecisionTime = context.time + Mathf.Max(0.05f, context.decisionInterval);
                hasDesiredPoint = true;
                isHoldingAtRange = false;
                result.refreshed = true;
            }

            result.hasDesiredPoint = hasDesiredPoint;
            result.desiredPoint = currentDesiredPoint;
            result.desiredStopDistance = currentDesiredStopDistance;
            result.shouldStop = isHoldingAtRange;

            return result;
        }

        private float ResolveStopDistance(Context context)
        {
            float preferredRange = Mathf.Max(0f, context.preferredRange);
            float stopDistance = Mathf.Max(0f, context.stopDistance);

            if (preferredRange > 0f)
            {
                stopDistance = preferredRange;
            }

            if (context.minDistance > 0f)
            {
                stopDistance = Mathf.Max(stopDistance, context.minDistance);
            }

            if (context.maxDistance > 0f)
            {
                stopDistance = Mathf.Min(stopDistance, context.maxDistance);
            }

            return Mathf.Max(0.05f, stopDistance);
        }

        private Vector2 BuildDesiredPoint(Context context, float stopDistance)
        {
            Vector2 fromTargetToSelf = context.selfPosition - context.targetPosition;

            if (fromTargetToSelf.sqrMagnitude <= 0.0001f)
            {
                fromTargetToSelf = Vector2.right;
            }

            Vector2 direction = fromTargetToSelf.normalized;

            if (context.useRingMovement)
            {
                direction = RotateVector(
                    direction,
                    context.ringMoveAngle);
            }

            Vector2 desiredPoint =
                context.targetPosition + direction * stopDistance;

            if (context.useStopJitter && context.stopJitterRadius > 0f)
            {
                desiredPoint += BuildStableJitter(
                    context.targetTransform,
                    context.stopJitterRadius);
            }

            if (context.useLocalSpacing && context.spacingRadius > 0f && context.spacingWeight > 0f)
            {
                desiredPoint += BuildSpacingOffset(
                    context.selfPosition,
                    context.spacingRadius,
                    context.spacingWeight);
            }

            return desiredPoint;
        }

        private Vector2 BuildStableJitter(Transform target, float radius)
        {
            if (target == null || radius <= 0f)
            {
                return Vector2.zero;
            }

            int hash = target.GetInstanceID();
            float angle = Mathf.Abs(hash % 360) * Mathf.Deg2Rad;

            return new Vector2(
                Mathf.Cos(angle),
                Mathf.Sin(angle)) * radius;
        }

        private Vector2 BuildSpacingOffset(
            Vector2 selfPosition,
            float radius,
            float weight)
        {
            if (radius <= 0f || weight <= 0f)
            {
                return Vector2.zero;
            }

            Collider2D[] colliders = Physics2D.OverlapCircleAll(
                selfPosition,
                radius);

            Vector2 offset = Vector2.zero;

            for (int i = 0; i < colliders.Length; i++)
            {
                Collider2D collider = colliders[i];

                if (collider == null)
                {
                    continue;
                }

                Vector2 otherPosition = collider.transform.position;
                Vector2 away = selfPosition - otherPosition;

                float sqrMagnitude = away.sqrMagnitude;

                if (sqrMagnitude <= 0.0001f)
                {
                    continue;
                }

                float distance = Mathf.Sqrt(sqrMagnitude);
                float strength = 1f - Mathf.Clamp01(distance / radius);

                offset += away.normalized * strength * weight;
            }

            return offset;
        }

        private Vector2 RotateVector(Vector2 vector, float angleDegrees)
        {
            if (Mathf.Abs(angleDegrees) <= 0.0001f)
            {
                return vector;
            }

            float rad = angleDegrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);

            return new Vector2(
                vector.x * cos - vector.y * sin,
                vector.x * sin + vector.y * cos);
        }
    }
}