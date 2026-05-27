

using Character;
using UnityEngine;

namespace Effect
{
    public class KnockbackEffectRuntime
        : EffectRuntimeData
    {
        private readonly KnockbackEffectSO effectSO;
        private readonly CharacterManager targetCharacter;
        private readonly Transform sourceTransform;
        private readonly Vector2 projectileDirection;

        public KnockbackEffectRuntime(
            KnockbackEffectSO effectSO,
            CharacterManager targetCharacter,
            Transform sourceTransform = null,
            Vector2 projectileDirection = default)
        {
            this.effectSO = effectSO;
            this.targetCharacter = targetCharacter;
            this.sourceTransform = sourceTransform;
            this.projectileDirection = projectileDirection;

            RuntimeId =
                $"Knockback_{GetEffectId()}_{GetTargetRuntimeId()}";
        }

        public override void OnApply()
        {
            if (effectSO == null || targetCharacter == null)
            {
                return;
            }

            if (effectSO.force <= 0f)
            {
                return;
            }

            MovementMono movementMono =
                ResolveMovementMono(targetCharacter);

            if (movementMono == null)
            {
                return;
            }

            Vector2 direction =
                ResolveDirection();

            if (direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            if (effectSO.normalizeDirection)
            {
                direction.Normalize();
            }

            movementMono.ApplyKnockback(
                direction,
                effectSO.force);
        }

        public override void OnRemove()
        {
            // Instant Knockback Effect
        }

        private Vector2 ResolveDirection()
        {
            switch (effectSO.directionType)
            {
                case KnockbackDirectionType.FromSourceToTarget:
                    return ResolveSourceToTargetDirection();

                case KnockbackDirectionType.FromTargetToSource:
                    return -ResolveSourceToTargetDirection();

                case KnockbackDirectionType.ProjectileDirection:
                    return ResolveProjectileDirection();

                case KnockbackDirectionType.CustomDirection:
                    return effectSO.customDirection;

                default:
                    return ResolveSourceToTargetDirection();
            }
        }

        private Vector2 ResolveSourceToTargetDirection()
        {
            if (sourceTransform != null && targetCharacter != null)
            {
                Vector2 sourcePosition = sourceTransform.position;
                Vector2 targetPosition = targetCharacter.transform.position;

                Vector2 direction = targetPosition - sourcePosition;

                if (direction.sqrMagnitude > 0.0001f)
                {
                    return direction;
                }
            }

            return effectSO.fallbackToProjectileDirection
                ? ResolveProjectileDirection()
                : Vector2.zero;
        }

        private Vector2 ResolveProjectileDirection()
        {
            if (projectileDirection.sqrMagnitude > 0.0001f)
            {
                return projectileDirection;
            }

            return Vector2.zero;
        }

        private MovementMono ResolveMovementMono(
            CharacterManager characterManager)
        {
            if (characterManager == null)
            {
                return null;
            }

            MovementMono movementMono =
                characterManager.GetComponent<MovementMono>();

            if (movementMono != null)
            {
                return movementMono;
            }

            movementMono =
                characterManager.GetComponentInChildren<MovementMono>();

            if (movementMono != null)
            {
                return movementMono;
            }

            return characterManager.GetComponentInParent<MovementMono>();
        }

        private string GetEffectId()
        {
            return effectSO != null
                ? effectSO.effectId
                : "NoEffect";
        }

        private string GetTargetRuntimeId()
        {
            return targetCharacter != null
                ? targetCharacter.GetInstanceID().ToString()
                : "NoTarget";
        }
    }
}