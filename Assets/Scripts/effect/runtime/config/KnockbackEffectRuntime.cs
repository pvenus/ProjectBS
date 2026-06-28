using Character;
using UnityEngine;

namespace Effect
{
    public class KnockbackEffectRuntime
        : EffectRuntimeData
    {
        private readonly EffectSO effectSO;
        private readonly KnockbackEffectConfig config;
        private readonly CharacterManager targetCharacter;
        private readonly Transform sourceTransform;
        private readonly Vector2 projectileDirection;

        public KnockbackEffectRuntime(
            EffectSO effectSO,
            KnockbackEffectConfig config,
            CharacterManager targetCharacter,
            Transform sourceTransform = null,
            Vector2 projectileDirection = default)
        {
            this.effectSO = effectSO;
            this.config = config;
            this.targetCharacter = targetCharacter;
            this.sourceTransform = sourceTransform;
            this.projectileDirection = projectileDirection;

            RuntimeId =
                $"Knockback_{GetEffectId()}_{GetTargetRuntimeId()}";
        }

        public override void OnApply()
        {
            if (effectSO == null || config == null || targetCharacter == null)
            {
                return;
            }

            if (config.Force <= 0f)
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

            if (config.NormalizeDirection)
            {
                direction.Normalize();
            }

            movementMono.ApplyKnockback(
                direction,
                config.Force);
        }

        public override void OnRemove()
        {
            // Instant Knockback Effect
        }

        private Vector2 ResolveDirection()
        {
            switch (config.DirectionType)
            {
                case KnockbackDirectionType.PushAwayFromSource:
                    return ResolveSourceToTargetDirection();

                case KnockbackDirectionType.PullToSource:
                    return -ResolveSourceToTargetDirection();

                case KnockbackDirectionType.ProjectileDirection:
                    return ResolveProjectileDirection();

                case KnockbackDirectionType.CustomDirection:
                    return config.CustomDirection;

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

            return config.FallbackToProjectileDirection
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
                ? effectSO.EffectId
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