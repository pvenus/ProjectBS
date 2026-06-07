using UnityEngine;

namespace Character
{
    /// <summary>
    /// Finds a target for the character and stores it in CharacterActionContext.
    ///
    /// This state does not move or attack.
    /// It only decides CurrentTarget, then finishes.
    /// </summary>
    public class FindTargetState : ICharacterActionState
    {
        private const float SearchRadius = 16f;

        private readonly LayerMask[] _targetMasks;

        public bool IsFinished { get; private set; }

        public FindTargetState(LayerMask[] targetMasks)
        {
            _targetMasks = targetMasks;
        }

        public void Enter(CharacterActionContext context)
        {
            IsFinished = false;

            context?.StateManager?.LogStateMessage(
                "FindTargetState Enter");

            if (context != null)
            {
                context.CurrentTarget = FindClosestTarget(context);

                context.StateManager?.LogStateMessage(
                    $"FindTargetState Result: {GetTargetName(context.CurrentTarget)}");
            }

            IsFinished = true;
        }

        public void Tick(CharacterActionContext context, float deltaTime)
        {
        }

        public void Exit(CharacterActionContext context)
        {
            context?.StateManager?.LogStateMessage(
                "FindTargetState Exit");
        }

        private Transform FindClosestTarget(CharacterActionContext context)
        {
            if (context == null || context.OwnerTransform == null)
            {
                return null;
            }

            Vector2 origin = context.OwnerTransform.position;

            if (_targetMasks == null || _targetMasks.Length == 0)
            {
                return null;
            }

            for (int maskIndex = 0; maskIndex < _targetMasks.Length; maskIndex++)
            {
                Transform closestTarget = null;
                float closestDistanceSqr = float.MaxValue;

                Collider2D[] hits = Physics2D.OverlapCircleAll(
                    origin,
                    SearchRadius,
                    _targetMasks[maskIndex]);

                for (int i = 0; i < hits.Length; i++)
                {
                    Collider2D hit = hits[i];

                    if (hit == null)
                    {
                        continue;
                    }

                    if (hit.transform == context.OwnerTransform)
                    {
                        continue;
                    }

                    float distanceSqr =
                        ((Vector2)hit.transform.position - origin).sqrMagnitude;

                    if (distanceSqr < closestDistanceSqr)
                    {
                        closestDistanceSqr = distanceSqr;
                        closestTarget = hit.transform;
                    }
                }

                if (closestTarget != null)
                {
                    return closestTarget;
                }
            }

            return null;
        }

        private static string GetTargetName(Transform target)
        {
            return target == null
                ? "null"
                : target.name;
        }
    }
}