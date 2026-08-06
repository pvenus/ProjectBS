using Character;
using UnityEngine;

namespace Effect
{
    public class OnHitKnockbackDistanceEffectRuntime : EffectRuntimeData
    {
        private readonly EffectSO effectSO;
        private readonly OnHitKnockbackDistanceEffectConfig config;
        private readonly CharacterManager sourceCharacter;

        public OnHitKnockbackDistanceEffectRuntime(
            EffectSO effectSO,
            OnHitKnockbackDistanceEffectConfig config,
            CharacterManager sourceCharacter)
        {
            this.effectSO = effectSO;
            this.config = config;
            this.sourceCharacter = sourceCharacter;
            RuntimeId = $"OnHitKnockbackDistance_{effectSO.EffectId}_{GetSourceRuntimeId()}";
        }

        public override void OnApply()
        {
            IsActive = effectSO != null
                && config != null
                && sourceCharacter != null;
        }

        public void OnHit(
            CharacterDamageRequest request,
            CharacterDamageResult result)
        {
            if (!IsActive
                || request == null
                || request.attacker == null
                || request.target == null
                || request.attacker.GetComponent<CharacterManager>() != sourceCharacter)
            {
                return;
            }

            CharacterManager target =
                request.target.GetComponent<CharacterManager>()
                ?? request.target.GetComponentInParent<CharacterManager>();

            if (target == null || target == sourceCharacter)
            {
                return;
            }

            float chance = Mathf.Clamp(config.ChancePercent, 0f, 100f);
            if (chance <= 0f || Random.Range(0f, 100f) > chance)
            {
                return;
            }

            MovementMono movementMono = ResolveMovementMono(target);
            if (movementMono == null)
            {
                return;
            }

            Vector2 direction = ResolveDirection(target);
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            movementMono.ApplyDisplacement(
                direction,
                config.DistanceMeters);
        }

        private Vector2 ResolveDirection(CharacterManager target)
        {
            Vector2 sourcePosition = sourceCharacter.transform.position;
            Vector2 targetPosition = target.transform.position;
            Vector2 away = targetPosition - sourcePosition;

            return config.DirectionType == KnockbackDirectionType.PullToSource
                ? -away
                : away;
        }

        private static MovementMono ResolveMovementMono(CharacterManager character)
        {
            return character.GetComponent<MovementMono>()
                ?? character.GetComponentInChildren<MovementMono>()
                ?? character.GetComponentInParent<MovementMono>();
        }

        private string GetSourceRuntimeId()
        {
            return sourceCharacter != null
                ? sourceCharacter.GetInstanceID().ToString()
                : "NoSource";
        }
    }
}
