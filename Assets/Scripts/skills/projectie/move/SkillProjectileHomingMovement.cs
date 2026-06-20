using Character;
using Skills.Dto.Move;
using UnityEngine;

namespace Skill
{
    /// <summary>
    /// 실시간 타겟을 추적하는 유도탄 이동 처리.
    /// 세부 튜닝값은 JSON에 노출하지 않고 기본 정책으로 처리한다.
    /// 명중/소멸 판정은 ProjectileHitHandler와 lifetime 처리에 맡기고, 도착만으로 이동을 종료하지 않는다.
    /// </summary>
    public class SkillProjectileHomingMovement : ISkillProjectileMovement
    {
        private const float DefaultRetargetInterval = 0.15f;
        private const float DefaultSearchRadius = 50f;
        private const float DefaultArrivalThreshold = 0.05f;

        private SkillProjectileMovementContext context;

        private Vector2 currentTargetPosition;
        private bool hasTargetPosition;

        private Vector2 position;
        private Vector2 direction;
        private HomingProjectileMoveDto moveDto;
        private float retargetTimer;

        public bool IsInitialized { get; private set; }

        public void Initialize(object dto)
        {
            if (dto is HomingProjectileMoveDto homingDto)
            {
                Initialize(homingDto);
                return;
            }

            Debug.LogError($"SkillProjectileHomingMovement expected HomingProjectileMoveDto but received {(dto != null ? dto.GetType().Name : "null")}");
        }

        public void Initialize(HomingProjectileMoveDto dto)
        {
            if (dto == null)
            {
                Debug.LogError("HomingProjectileMoveDto is null");
                return;
            }

            moveDto = dto;
            IsInitialized = true;

        }

        public void SetContext(SkillProjectileMovementContext movementContext)
        {
            context = movementContext;

            if (context.targetTransform != null)
            {
                currentTargetPosition = context.targetTransform.position;
                hasTargetPosition = true;
            }
            else
            {
                currentTargetPosition = context.spawnPosition;
                hasTargetPosition = false;
            }

            position = context.projectileTransform != null
                ? context.projectileTransform.position
                : context.spawnPosition;

            ApplyPositionToProjectile();
            ApplyRotationToProjectile();

            direction = ResolveInitialDirection();

            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                direction = Vector2.right;
            }

            retargetTimer = 0f;
            IsInitialized = true;

        }

        public void TickMovement(float deltaTime)
        {
            RefreshTarget(deltaTime);

            if (!hasTargetPosition)
            {
                return;
            }

            Vector2 targetPosition = currentTargetPosition;
            Vector2 toTarget = targetPosition - position;
            float distance = toTarget.magnitude;

            if (distance <= DefaultArrivalThreshold)
            {
            }

            Vector2 desiredDirection = toTarget.normalized;
            direction = RotateDirectionToward(
                direction,
                desiredDirection,
                (moveDto != null ? moveDto.turnSpeed : 0f) * deltaTime);

            float moveDistance = (moveDto != null ? moveDto.speed : 0f) * deltaTime;

            if (moveDistance <= Mathf.Epsilon)
            {
                return;
            }

            position += direction * moveDistance;

            ApplyPositionToProjectile();
            ApplyRotationToProjectile();

        }

        public bool HasReachedEnd()
        {
            return false;
        }

        public void ResetMovement()
        {
            position = context.projectileTransform != null
                ? context.projectileTransform.position
                : context.spawnPosition;

            ApplyPositionToProjectile();
            ApplyRotationToProjectile();

            if (context.targetTransform != null)
            {
                currentTargetPosition = context.targetTransform.position;
                hasTargetPosition = true;
            }
            else
            {
                currentTargetPosition = position;
                hasTargetPosition = false;
            }

            direction = ResolveInitialDirection();

            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                direction = Vector2.right;
            }

            retargetTimer = 0f;
            if (moveDto == null)
            {
                Debug.LogError("HomingProjectileMoveDto is null");
            }
            IsInitialized = true;

        }

        public Vector2 GetDirection()
        {
            return direction;
        }

        public Vector2 GetPosition()
        {
            return position;
        }

        private Vector2 ResolveInitialDirection()
        {
            if (hasTargetPosition)
            {
                Vector2 toTarget = currentTargetPosition - position;

                if (toTarget.sqrMagnitude > Mathf.Epsilon)
                {
                    return -toTarget.normalized;
                }
            }

            if (context.owner != null)
            {
                Vector2 fromOwnerToSpawn = position - (Vector2)context.owner.position;

                if (fromOwnerToSpawn.sqrMagnitude > Mathf.Epsilon)
                {
                    return fromOwnerToSpawn.normalized;
                }
            }

            return Vector2.right;
        }

        private void RefreshTarget(float deltaTime)
        {
            retargetTimer -= deltaTime;

            if (context.targetTransform != null && IsTargetValid(context.targetTransform))
            {
            if (retargetTimer <= 0f)
            {
                currentTargetPosition = context.targetTransform.position;
                hasTargetPosition = true;
                retargetTimer = DefaultRetargetInterval;
            }

                return;
            }

            // if (context.targetTransform != null)
            // {
            //     LogDebug($"Target invalid: {GetTransformName(context.targetTransform)}");
            // }

            context.targetTransform = null;

            if (retargetTimer > 0f)
            {
                return;
            }

            retargetTimer = DefaultRetargetInterval;
            context.targetTransform = FindNearestTarget();

            if (context.targetTransform != null)
            {
                currentTargetPosition = context.targetTransform.position;
                hasTargetPosition = true;
            }

        }

        private Transform FindNearestTarget()
        {
            CharacterManager ownerCharacter = ResolveCharacterManager(context.owner);

            CharacterManager[] characters = Object.FindObjectsByType<CharacterManager>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);

            CharacterManager nearest = null;
            float nearestDistanceSqr = DefaultSearchRadius * DefaultSearchRadius;

            for (int i = 0; i < characters.Length; i++)
            {
                CharacterManager candidate = characters[i];

                if (candidate == null || candidate == ownerCharacter)
                {
                    continue;
                }

                if (!IsTargetValid(candidate.transform))
                {
                    continue;
                }

                if (!IsEnemy(ownerCharacter, candidate))
                {
                    continue;
                }

                float distanceSqr =
                    ((Vector2)candidate.transform.position - position).sqrMagnitude;

                if (distanceSqr > nearestDistanceSqr)
                {
                    continue;
                }

                nearest = candidate;
                nearestDistanceSqr = distanceSqr;
            }


            return nearest != null ? nearest.transform : null;
        }

        private bool IsTargetValid(Transform target)
        {
            if (target == null || !target.gameObject.activeInHierarchy)
            {
                return false;
            }

            CharacterManager characterManager = ResolveCharacterManager(target);

            return characterManager != null;
        }

        private bool IsEnemy(
            CharacterManager ownerCharacter,
            CharacterManager candidate)
        {
            if (ownerCharacter == null
                || candidate == null
                || ownerCharacter.RuntimeData == null
                || candidate.RuntimeData == null
                || ownerCharacter.RuntimeData.characterSO == null
                || candidate.RuntimeData.characterSO == null)
            {
                return true;
            }

            return ownerCharacter.RuntimeData.characterSO.CharacterType
                != candidate.RuntimeData.characterSO.CharacterType;
        }

        private CharacterManager ResolveCharacterManager(Transform target)
        {
            if (target == null)
            {
                return null;
            }

            CharacterManager characterManager = target.GetComponent<CharacterManager>();

            if (characterManager != null)
            {
                return characterManager;
            }

            return target.GetComponentInParent<CharacterManager>();
        }


        private void ApplyPositionToProjectile()
        {
            if (context.projectileTransform == null)
            {
                return;
            }

            context.projectileTransform.position = position;
        }

        private void ApplyRotationToProjectile()
        {
            if (context.projectileTransform == null || direction.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            context.projectileTransform.rotation = Quaternion.Euler(0f, 0f, angle);
        }


        private Vector2 RotateDirectionToward(
            Vector2 currentDirection,
            Vector2 desiredDirection,
            float maxDegreesDelta)
        {
            if (currentDirection.sqrMagnitude <= Mathf.Epsilon)
            {
                return desiredDirection.normalized;
            }

            if (desiredDirection.sqrMagnitude <= Mathf.Epsilon)
            {
                return currentDirection.normalized;
            }

            float currentAngle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;
            float desiredAngle = Mathf.Atan2(desiredDirection.y, desiredDirection.x) * Mathf.Rad2Deg;
            float nextAngle = Mathf.MoveTowardsAngle(
                currentAngle,
                desiredAngle,
                maxDegreesDelta);

            float radian = nextAngle * Mathf.Deg2Rad;

            return new Vector2(
                Mathf.Cos(radian),
                Mathf.Sin(radian)).normalized;
        }


        private string GetTransformName(Transform target)
        {
            return target != null ? target.name : "null";
        }
    }
}