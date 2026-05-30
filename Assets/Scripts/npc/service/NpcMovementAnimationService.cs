

using UnityEngine;

namespace Npc.Service
{
    /// <summary>
    /// NPC 이동 상태 기반 애니메이션 동기화 서비스.
    /// 실제 Animator 제어만 담당하며,
    /// 이동 판단 자체는 수행하지 않는다.
    /// </summary>
    public class NpcMovementAnimationService
    {
        public struct Context
        {
            public Animator animator;
            public Vector2 velocity;
            public bool forceIdle;
            public float moveThreshold;
        }

        private static readonly int IsMovingHash =
            Animator.StringToHash("IsMoving");

        private static readonly int MoveXHash =
            Animator.StringToHash("MoveX");

        private static readonly int MoveYHash =
            Animator.StringToHash("MoveY");

        private static readonly int SpeedHash =
            Animator.StringToHash("Speed");

        private Vector2 lastMoveDirection = Vector2.down;

        public Vector2 LastMoveDirection => lastMoveDirection;

        public void Reset()
        {
            lastMoveDirection = Vector2.down;
        }

        public void UpdateAnimation(Context context)
        {
            if (context.animator == null)
            {
                return;
            }

            float threshold = Mathf.Max(0.001f, context.moveThreshold);

            Vector2 velocity = context.velocity;
            float speed = velocity.magnitude;

            bool isMoving =
                !context.forceIdle
                && speed > threshold;

            if (isMoving)
            {
                lastMoveDirection = velocity.normalized;
            }

            context.animator.SetBool(
                IsMovingHash,
                isMoving);

            context.animator.SetFloat(
                MoveXHash,
                lastMoveDirection.x);

            context.animator.SetFloat(
                MoveYHash,
                lastMoveDirection.y);

            context.animator.SetFloat(
                SpeedHash,
                speed);
        }

        public void ForceIdle(Animator animator)
        {
            if (animator == null)
            {
                return;
            }

            animator.SetBool(IsMovingHash, false);
            animator.SetFloat(SpeedHash, 0f);
        }
    }
}