using UnityEngine;
namespace Party
{
    public class ReturnToDestinationModeService
    {
        public struct Context
        {
            public Vector2 DestinationPosition;
            public float Time;
            public float MoveRange;
        }

        public Vector2 ResolvePosition(Context context)
        {
            float offsetX = Mathf.PingPong(
                context.Time * 0.5f,
                context.MoveRange);

            return context.DestinationPosition
                + new Vector2(offsetX, 0f);
        }
    }
}