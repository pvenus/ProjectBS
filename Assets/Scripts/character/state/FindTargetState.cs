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
                if (context.StateManager != null &&
                    context.StateManager.TryGetForcedTarget(out Transform forcedTarget))
                {
                    context.CurrentTarget = forcedTarget;

                    context.StateManager.LogStateMessage(
                        $"FindTargetState ForcedTarget Result: {GetTargetName(context.CurrentTarget)}");
                }
                else
                {
                    context.CurrentTarget = FindClosestTarget(context);

                    context.StateManager?.LogStateMessage(
                        $"FindTargetState Result: {GetTargetName(context.CurrentTarget)}");
                }
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

                    bool isOwner = IsOwnerTransform(
                        hit.transform,
                        context.OwnerTransform);

                    float distanceSqr =
                        ((Vector2)hit.transform.position - origin).sqrMagnitude;

                    if (isOwner)
                    {
                        distanceSqr += SearchRadius * SearchRadius;
                    }

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

                if (MaskContainsAnyOwnerLayer(
                        _targetMasks[maskIndex],
                        context.OwnerTransform))
                {
                    return context.OwnerTransform;
                }
            }

            return null;
        }

        private static bool IsOwnerTransform(
            Transform target,
            Transform owner)
        {
            if (target == null || owner == null)
            {
                return false;
            }

            return target == owner || target.IsChildOf(owner);
        }

        private static bool MaskContainsAnyOwnerLayer(
            LayerMask mask,
            Transform owner)
        {
            if (owner == null)
            {
                return false;
            }

            if (MaskContainsLayer(mask, owner.gameObject.layer))
            {
                return true;
            }

            Transform[] children = owner.GetComponentsInChildren<Transform>(true);

            for (int i = 0; i < children.Length; i++)
            {
                Transform child = children[i];

                if (child == null)
                {
                    continue;
                }

                if (MaskContainsLayer(mask, child.gameObject.layer))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool MaskContainsLayer(
            LayerMask mask,
            int layer)
        {
            return (mask.value & (1 << layer)) != 0;
        }

        private static string GetTargetName(Transform target)
        {
            return target == null
                ? "null"
                : target.name;
        }
    }
}