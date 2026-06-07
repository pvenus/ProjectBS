

using UnityEngine;

namespace Party
{
    public class AggressivePositionModeService
    {
        public struct Context
        {
            public Vector2 SelfPosition;
            public Vector2 PartyAnchorPosition;
            public Vector2 ThreatDirection;
            public float AggressiveDistance;
        }

        public Vector2 ResolvePosition(Context context)
        {
            Vector2 target = context.PartyAnchorPosition;

            return PreventBackstep(
                context,
                target);
        }

        private Vector2 PreventBackstep(
            Context context,
            Vector2 target)
        {
            Vector2 threatDirection = context.ThreatDirection;

            if (threatDirection.sqrMagnitude <= 0.0001f)
            {
                return target;
            }

            Vector2 selfPosition = context.SelfPosition;
            Vector2 toTarget = target - selfPosition;
            Vector2 forward = threatDirection.normalized;

            float forwardAmount = Vector2.Dot(
                toTarget,
                forward);

            if (forwardAmount >= 0f)
            {
                return target;
            }

            return selfPosition;
        }
    }
}