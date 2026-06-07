

using UnityEngine;

namespace Party
{
    public class ProtectPositionModeService
    {
        public struct Context
        {
            public Vector2 SelfPosition;
            public Vector2 ThreatDirection;
            public float ProtectDistance;
        }

        public Vector2 ResolvePosition(Context context)
        {
            Vector2 threatDirection = context.ThreatDirection;

            if (threatDirection.sqrMagnitude <= 0.0001f)
            {
                return context.SelfPosition;
            }

            return context.SelfPosition
                + threatDirection.normalized * Mathf.Max(0f, context.ProtectDistance);
        }
    }
}