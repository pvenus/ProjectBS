

using UnityEngine;

namespace Character
{
    /// <summary>
    /// Character movement execution service for the new state-based AI flow.
    ///
    /// Responsibilities:
    /// - Execute movement through MovementController
    /// - Stop movement when movement is unavailable
    /// - Update movement/idle animation
    /// - Avoid overwriting attack animation
    /// </summary>
    public class CharacterMovementExecutionService
    {
        public bool MoveTowardPoint(
            CharacterActionContext context,
            Vector2 targetPoint,
            float moveSpeed,
            float arriveDistance)
        {
            if (context == null)
            {
                return false;
            }

            if (!CanMove(context))
            {
                StopMovement(context);
                return false;
            }

            MovementController movementController = context.MovementController;

            if (movementController == null)
            {
                StopAnimation(context);
                return false;
            }

            Vector2 currentPosition = context.OwnerTransform != null
                ? (Vector2)context.OwnerTransform.position
                : targetPoint;

            Vector2 moveDirection = targetPoint - currentPosition;

            if (moveDirection.sqrMagnitude <= 0.0001f)
            {
                StopMovement(context);
                return true;
            }

            SyncMovementControllerConfig(
                movementController,
                moveSpeed,
                arriveDistance);

            movementController.MoveTo(targetPoint);

            UpdateMovementAnimation(
                context,
                moveDirection);

            return true;
        }

        public bool MoveInDirection(
            CharacterActionContext context,
            Vector2 direction,
            float moveSpeed,
            float arriveDistance,
            float speedMultiplier = 1f)
        {
            if (context == null)
            {
                return false;
            }

            if (!CanMove(context))
            {
                StopMovement(context);
                return false;
            }

            MovementController movementController = context.MovementController;

            if (movementController == null)
            {
                StopAnimation(context);
                return false;
            }

            if (direction.sqrMagnitude <= 0.0001f)
            {
                StopMovement(context);
                return true;
            }

            SyncMovementControllerConfig(
                movementController,
                moveSpeed * Mathf.Max(0f, speedMultiplier),
                arriveDistance);

            movementController.MoveByDirection(direction.normalized);

            UpdateMovementAnimation(
                context,
                direction);

            return true;
        }

        public void StopMovement(CharacterActionContext context)
        {
            if (context?.MovementController != null)
            {
                context.MovementController.Stop();
            }

            StopAnimation(context);
        }

        private bool CanMove(CharacterActionContext context)
        {
            return context.CharacterManager == null
                || context.CharacterManager.CanMove;
        }

        private void SyncMovementControllerConfig(
            MovementController movementController,
            float moveSpeed,
            float arriveDistance)
        {
            movementController.SetMoveSpeed(Mathf.Max(0f, moveSpeed));
            movementController.SetArriveDistance(Mathf.Max(0.01f, arriveDistance));
        }

        private void StopAnimation(CharacterActionContext context)
        {
            UpdateMovementAnimation(
                context,
                Vector2.zero);
        }

        private void UpdateMovementAnimation(
            CharacterActionContext context,
            Vector2 moveDirection)
        {
            AnimationMono animationMono = context?.AnimationMono;

            if (animationMono == null)
            {
                return;
            }

            if (animationMono.IsPlayingAttack())
            {
                return;
            }

            if (moveDirection.sqrMagnitude <= 0.0001f)
            {
                animationMono.PlayIdle();
                return;
            }

            animationMono.SetDirectionFromVector(moveDirection.normalized);
            animationMono.PlayMove();
        }
    }
}