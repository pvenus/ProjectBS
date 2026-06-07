using System;
using Character;
using Stat;
using UnityEngine;

public class PartyMovementExecutionService
{
    private readonly MonoBehaviour _owner;

    public PartyMovementExecutionService(MonoBehaviour owner = null)
    {
        _owner = owner;
    }
    public struct MoveContext
    {
        public Rigidbody2D Rigidbody;
        public Transform Transform;
        public Transform VisualRoot;
        public Vector2 TargetPosition;
        public Vector2 ManualInput;
        public float MoveSpeed;
        public float ArriveDistance;
        public bool FlipByMoveDirection;
    }

    public bool MoveTo(MoveContext context)
    {
        if (context.Rigidbody == null)
        {
            return false;
        }

        Vector2 currentPosition = context.Rigidbody.position;
        Vector2 toTarget = context.TargetPosition - currentPosition;
        float arriveDistance = Mathf.Max(0.01f, context.ArriveDistance);

        if (toTarget.sqrMagnitude <= arriveDistance * arriveDistance)
        {
            StopMovement(context.Rigidbody);
            return true;
        }

        Vector2 direction = toTarget.normalized;
        MoveInDirection(
            context.Rigidbody,
            direction,
            context.MoveSpeed);

        ApplyFacing(
            context.VisualRoot,
            direction,
            context.FlipByMoveDirection);

        return false;
    }

    public void MoveTowardPoint(
        Rigidbody2D rb,
        Transform visualRoot,
        Vector2 targetPosition,
        float moveSpeed,
        bool flipByMoveDirection)
    {
        if (rb == null)
        {
            return;
        }

        Vector2 currentPosition = rb.position;
        Vector2 direction = targetPosition - currentPosition;

        if (direction.sqrMagnitude <= 0.0001f)
        {
            StopMovement(rb);
            return;
        }

        direction.Normalize();

        MoveInDirection(
            rb,
            direction,
            moveSpeed);

        ApplyFacing(
            visualRoot,
            direction,
            flipByMoveDirection);
    }

    public void HandleManualMovement(MoveContext context)
    {
        if (context.Rigidbody == null)
        {
            return;
        }

        Vector2 input = context.ManualInput;

        if (input.sqrMagnitude <= 0.0001f)
        {
            StopMovement(context.Rigidbody);
            return;
        }

        Vector2 direction = input.normalized;

        MoveInDirection(
            context.Rigidbody,
            direction,
            context.MoveSpeed);

        ApplyFacing(
            context.VisualRoot,
            direction,
            context.FlipByMoveDirection);
    }

    public void MoveInDirection(
        Rigidbody2D rb,
        Vector2 direction,
        float moveSpeed)
    {
        if (rb == null)
        {
            return;
        }

        if (direction.sqrMagnitude <= 0.0001f)
        {
            StopMovement(rb);
            return;
        }

        Vector2 velocity = direction.normalized * Mathf.Max(0f, moveSpeed);
        rb.linearVelocity = velocity;
    }

    public void StopMovement(Rigidbody2D rb)
    {
        if (rb == null)
        {
            return;
        }

        rb.linearVelocity = Vector2.zero;
    }

    public float ResolveMoveSpeed(
        float baseMoveSpeed,
        Func<float> statMoveSpeedGetter = null)
    {
        if (statMoveSpeedGetter == null)
        {
            return Mathf.Max(0f, baseMoveSpeed);
        }

        float statMoveSpeed = statMoveSpeedGetter.Invoke();

        if (statMoveSpeed <= 0f)
        {
            return Mathf.Max(0f, baseMoveSpeed);
        }

        return statMoveSpeed;
    }

    public void ApplyFacing(
        Transform visualRoot,
        Vector2 direction,
        bool flipByMoveDirection)
    {
        if (!flipByMoveDirection || visualRoot == null)
        {
            return;
        }

        if (Mathf.Abs(direction.x) <= 0.0001f)
        {
            return;
        }

        Vector3 scale = visualRoot.localScale;
        float sign = direction.x >= 0f ? 1f : -1f;
        scale.x = Mathf.Abs(scale.x) * sign;
        visualRoot.localScale = scale;
    }

    public void SyncRigidbodyConfig(
        Rigidbody2D rb,
        float gravityScale = 0f,
        RigidbodyConstraints2D constraints = RigidbodyConstraints2D.FreezeRotation)
    {
        if (rb == null)
        {
            return;
        }

        rb.gravityScale = gravityScale;
        rb.constraints = constraints;
    }
    // MovementController-based helpers

    public void MoveTowardPoint(
        MovementController movementController,
        Vector2 targetPoint,
        float moveSpeed,
        float arriveDistance)
    {
        if (movementController == null)
        {
            return;
        }

        SyncMovementControllerConfig(
            movementController,
            moveSpeed,
            arriveDistance);

        movementController.MoveTo(targetPoint);
    }

    public void MoveTowardPoint(
        MovementController movementController,
        AnimationMono animationMono,
        Vector2 currentPosition,
        Vector2 targetPoint,
        float moveSpeed,
        float arriveDistance)
    {
        if (movementController == null)
        {
            UpdateMovementAnimation(animationMono, Vector2.zero);
            return;
        }

        Vector2 moveDirection = targetPoint - currentPosition;

        if (moveDirection.sqrMagnitude <= 0.0001f)
        {
            StopMovement(
                movementController,
                animationMono);
            return;
        }

        SyncMovementControllerConfig(
            movementController,
            moveSpeed,
            arriveDistance);

        movementController.MoveTo(targetPoint);

        UpdateMovementAnimation(
            animationMono,
            moveDirection);
    }

    public void MoveInDirection(
        MovementController movementController,
        Vector2 direction,
        float moveSpeed,
        float arriveDistance,
        float speedMultiplier = 1f)
    {
        if (movementController == null)
        {
            return;
        }

        if (direction.sqrMagnitude <= 0.0001f)
        {
            StopMovement(movementController);
            return;
        }

        SyncMovementControllerConfig(
            movementController,
            moveSpeed * Mathf.Max(0f, speedMultiplier),
            arriveDistance);

        movementController.MoveByDirection(direction.normalized);
    }

    public void MoveInDirection(
        MovementController movementController,
        AnimationMono animationMono,
        Vector2 direction,
        float moveSpeed,
        float arriveDistance,
        float speedMultiplier = 1f)
    {
        if (movementController == null)
        {
            UpdateMovementAnimation(animationMono, Vector2.zero);
            return;
        }

        if (direction.sqrMagnitude <= 0.0001f)
        {
            StopMovement(
                movementController,
                animationMono);
            return;
        }

        SyncMovementControllerConfig(
            movementController,
            moveSpeed * Mathf.Max(0f, speedMultiplier),
            arriveDistance);

        movementController.MoveByDirection(direction.normalized);

        UpdateMovementAnimation(
            animationMono,
            direction);
    }

    public void StopMovement(MovementController movementController)
    {
        if (movementController == null)
        {
            return;
        }

        movementController.Stop();
    }

    public void StopMovement(
        MovementController movementController,
        AnimationMono animationMono)
    {
        if (movementController != null)
        {
            movementController.Stop();
        }

        UpdateMovementAnimation(
            animationMono,
            Vector2.zero);
    }

    public float ResolveMoveSpeed(
        float fallbackMoveSpeed,
        CharacterManager characterManager)
    {
        if (characterManager == null)
        {
            return Mathf.Max(0f, fallbackMoveSpeed);
        }

        float statMoveSpeed = characterManager.GetStatValue(StatType.MoveSpeed);

        if (statMoveSpeed <= 0f)
        {
            return Mathf.Max(0f, fallbackMoveSpeed);
        }

        return statMoveSpeed;
    }

    public void SyncMovementControllerConfig(
        MovementController movementController,
        float moveSpeed,
        float arriveDistance)
    {
        if (movementController == null)
        {
            return;
        }

        movementController.SetMoveSpeed(Mathf.Max(0f, moveSpeed));
        movementController.SetArriveDistance(Mathf.Max(0.01f, arriveDistance));
    }

    private void UpdateMovementAnimation(
        AnimationMono animationMono,
        Vector2 moveDirection)
    {
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