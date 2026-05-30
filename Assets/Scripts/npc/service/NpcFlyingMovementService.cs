

using UnityEngine;

namespace Npc.Service
{
    /// <summary>
    /// Flying NPC 전용 직선 이동/자동 제거 판단 서비스.
    ///
    /// 이 서비스는 실제 Rigidbody 이동이나 GameObject 제거를 직접 수행하지 않는다.
    /// NpcPathing은 Evaluate 결과를 보고 이동/제거를 실행한다.
    /// </summary>
    public class NpcFlyingMovementService
    {
        public struct Context
        {
            public Vector2 selfPosition;
            public Vector2 spawnPosition;
            public Vector2 configuredDirection;
            public Transform target;

            public bool useTargetDirectionOnInitialize;
            public bool autoDespawnByDistance;
            public float autoDespawnDistance;
        }

        public struct Result
        {
            public bool hasMoveDirection;
            public Vector2 moveDirection;
            public bool shouldDespawn;
        }

        private bool hasMoveDirection;
        private Vector2 moveDirection;
        private Vector2 spawnPosition;

        public bool HasMoveDirection => hasMoveDirection;
        public Vector2 MoveDirection => moveDirection;

        public void Reset()
        {
            hasMoveDirection = false;
            moveDirection = Vector2.zero;
            spawnPosition = Vector2.zero;
        }

        public void Initialize(Context context)
        {
            spawnPosition = context.spawnPosition;
            moveDirection = ResolveInitialDirection(context);
            hasMoveDirection = moveDirection.sqrMagnitude > 0.0001f;

            if (hasMoveDirection)
            {
                moveDirection.Normalize();
            }
        }

        public Result Evaluate(Context context)
        {
            if (!hasMoveDirection)
            {
                Initialize(context);
            }

            Result result = new Result
            {
                hasMoveDirection = hasMoveDirection,
                moveDirection = moveDirection,
                shouldDespawn = ShouldDespawn(context)
            };

            return result;
        }

        public void SetMoveDirection(Vector2 direction)
        {
            if (direction.sqrMagnitude <= 0.0001f)
            {
                hasMoveDirection = false;
                moveDirection = Vector2.zero;
                return;
            }

            moveDirection = direction.normalized;
            hasMoveDirection = true;
        }

        public void SetSpawnPosition(Vector2 position)
        {
            spawnPosition = position;
        }

        private Vector2 ResolveInitialDirection(Context context)
        {
            if (context.useTargetDirectionOnInitialize && context.target != null)
            {
                Vector2 directionToTarget =
                    (Vector2)context.target.position - context.selfPosition;

                if (directionToTarget.sqrMagnitude > 0.0001f)
                {
                    return directionToTarget.normalized;
                }
            }

            if (context.configuredDirection.sqrMagnitude > 0.0001f)
            {
                return context.configuredDirection.normalized;
            }

            return Vector2.right;
        }

        private bool ShouldDespawn(Context context)
        {
            if (!context.autoDespawnByDistance)
            {
                return false;
            }

            if (context.autoDespawnDistance <= 0f)
            {
                return false;
            }

            Vector2 resolvedSpawnPosition = spawnPosition;

            if (resolvedSpawnPosition.sqrMagnitude <= 0.0001f)
            {
                resolvedSpawnPosition = context.spawnPosition;
            }

            float distance = Vector2.Distance(
                context.selfPosition,
                resolvedSpawnPosition);

            return distance >= context.autoDespawnDistance;
        }
    }
}