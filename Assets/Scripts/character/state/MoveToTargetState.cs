using Stat;
using UnityEngine;

namespace Character
{
    /// <summary>
    /// Moves toward the current target until the selected skill is in range.
    ///
    /// This state only handles movement.
    /// Actual skill execution is performed by AttackTargetState.
    /// </summary>
    public class MoveToTargetState : ICharacterActionState
    {
        private const float StopBuffer = 0.0f;

        public bool IsFinished { get; private set; }

        public void Enter(CharacterActionContext context)
        {
            IsFinished = false;

            context?.StateManager?.LogStateMessage(
                "MoveToTargetState Enter");
        }

        public void Tick(
            CharacterActionContext context,
            float deltaTime)
        {
            if (context == null)
            {
                IsFinished = true;
                return;
            }

            if (context.AnimationMono != null && context.AnimationMono.IsPlayingAttack())
            {
                StopMovement(context);
                return;
            }

            if (context.CurrentTarget == null)
            {
                context.StateManager?.LogStateMessage(
                    "MoveToTargetState Finish: TargetLost");

                IsFinished = true;
                return;
            }

            if (context.SelectedSkillRuntime == null)
            {
                context.StateManager?.LogStateMessage(
                    "MoveToTargetState Finish: SkillMissing");

                IsFinished = true;
                return;
            }

            Vector2 currentPosition =
                context.OwnerTransform.position;

            Vector2 targetPosition =
                context.CurrentTarget.position;

            float distance =
                Vector2.Distance(
                    currentPosition,
                    targetPosition);

            float skillRange = context.SelectedSkillRange;

            if (distance <= skillRange + StopBuffer)
            {
                StopMovement(context);

                context.StateManager?.LogStateMessage(
                    $"MoveToTargetState Finish: InRange Distance={distance:F2} Range={skillRange:F2}");

                IsFinished = true;
                return;
            }

            CharacterMovementExecutionService movementExecutionService =
                context.MovementExecutionService;

            if (movementExecutionService == null)
            {
                context.StateManager?.LogStateMessage(
                    "MoveToTargetState Finish: MovementServiceMissing");

                IsFinished = true;
                return;
            }

            bool moved = movementExecutionService.MoveTowardPoint(
                context,
                targetPosition,
                GetMoveSpeed(context),
                Mathf.Max(0.1f, skillRange));

            if (!moved)
            {
                context.StateManager?.LogStateMessage(
                    "MoveToTargetState MoveFailed");
            }
        }

        private float GetMoveSpeed(CharacterActionContext context)
        {
            if (context?.CharacterManager == null)
            {
                return 0f;
            }

            return context.CharacterManager.GetStatValue(StatType.MoveSpeed);
        }

        private void StopMovement(CharacterActionContext context)
        {
            context?.MovementExecutionService?.StopMovement(context);
        }

        public void Exit(CharacterActionContext context)
        {
            StopMovement(context);

            context?.StateManager?.LogStateMessage(
                "MoveToTargetState Exit");
        }
    }
}
