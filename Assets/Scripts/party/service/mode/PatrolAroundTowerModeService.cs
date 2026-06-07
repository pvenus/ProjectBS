

using UnityEngine;

namespace Party
{
    public class PatrolAroundTowerModeService
    {
        public struct Context
        {
            public Vector2 SelfPosition;
            public Transform Tower;
            public Vector2 JitterDirection;
            public float PatrolDistance;
        }

        public Vector2 ResolvePosition(Context context)
        {
            if (context.Tower == null)
            {
                return context.SelfPosition;
            }

            Vector2 towerPosition = context.Tower.position;
            Vector2 baseDirection = context.SelfPosition - towerPosition;

            if (baseDirection.sqrMagnitude <= 0.0001f)
            {
                baseDirection = context.JitterDirection.sqrMagnitude > 0.0001f
                    ? context.JitterDirection
                    : Vector2.right;
            }

            baseDirection.Normalize();

            return towerPosition
                + baseDirection * Mathf.Max(0f, context.PatrolDistance);
        }
    }
}