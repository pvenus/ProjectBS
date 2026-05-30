

using UnityEngine;

namespace Npc.Service
{
    /// <summary>
    /// NPC 배회 상태를 관리하는 서비스.
    /// 실제 이동은 수행하지 않고, 이동 방향/정지 여부만 계산한다.
    /// </summary>
    public class NpcWanderService
    {
        public struct Context
        {
            public float deltaTime;
            public float wanderDuration;
            public float pauseDuration;
            public bool canWander;
        }

        public struct Result
        {
            public bool shouldMove;
            public Vector2 moveDirection;
            public bool isPausing;
        }

        private Vector2 wanderDirection = Vector2.right;
        private float wanderTimer;
        private float pauseTimer;
        private bool isPausing;

        public Vector2 WanderDirection => wanderDirection;
        public bool IsPausing => isPausing;

        public void Reset()
        {
            wanderDirection = Vector2.right;
            wanderTimer = 0f;
            pauseTimer = 0f;
            isPausing = false;
        }

        public Result Evaluate(Context context)
        {
            if (!context.canWander)
            {
                return StopResult();
            }

            float deltaTime = Mathf.Max(0f, context.deltaTime);
            float resolvedWanderDuration = Mathf.Max(0.05f, context.wanderDuration);
            float resolvedPauseDuration = Mathf.Max(0f, context.pauseDuration);

            if (isPausing)
            {
                pauseTimer -= deltaTime;

                if (pauseTimer > 0f)
                {
                    return new Result
                    {
                        shouldMove = false,
                        moveDirection = Vector2.zero,
                        isPausing = true
                    };
                }

                isPausing = false;
                PickNewWanderDirection(resolvedWanderDuration);
            }

            if (wanderTimer <= 0f)
            {
                PickNewWanderDirection(resolvedWanderDuration);
            }

            wanderTimer -= deltaTime;

            if (wanderTimer <= 0f && resolvedPauseDuration > 0f)
            {
                isPausing = true;
                pauseTimer = resolvedPauseDuration;

                return new Result
                {
                    shouldMove = false,
                    moveDirection = Vector2.zero,
                    isPausing = true
                };
            }

            return new Result
            {
                shouldMove = true,
                moveDirection = wanderDirection,
                isPausing = false
            };
        }

        private Result StopResult()
        {
            return new Result
            {
                shouldMove = false,
                moveDirection = Vector2.zero,
                isPausing = isPausing
            };
        }

        private void PickNewWanderDirection(float duration)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

            wanderDirection = new Vector2(
                Mathf.Cos(angle),
                Mathf.Sin(angle));

            if (wanderDirection.sqrMagnitude <= 0.0001f)
            {
                wanderDirection = Vector2.right;
            }
            else
            {
                wanderDirection.Normalize();
            }

            wanderTimer = Mathf.Max(0.05f, duration);
        }
    }
}