using Character;
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
        private const float DefaultSpeed = 12f;
        private const float DefaultRetargetInterval = 0.15f;
        private const float DefaultSearchRadius = 50f;
        private const float DefaultTurnSpeed = 180f;
        private const float DefaultArrivalThreshold = 0.05f;
        private const float DefaultNoTargetLifeTime = 1.5f;
        private const bool EnableDebugLog = true;

        private SkillProjectileMovementContext context;
        private Transform projectileTransform;
        private Transform ownerTransform;
        private Transform targetTransform;

        private Vector2 currentTargetPosition;
        private bool hasTargetPosition;

        private Vector2 position;
        private Vector2 direction;
        private float speed = DefaultSpeed;
        private float arrivalThreshold = DefaultArrivalThreshold;
        private float retargetTimer;
        private float noTargetTimer;
        private bool reachedEnd;
        private int debugInstanceId;
        private float debugMoveLogTimer;

        public bool IsInitialized { get; private set; }

        public void Initialize(object dto)
        {
            HomingMovementDto homingDto = dto as HomingMovementDto;

            speed = homingDto != null
                ? Mathf.Max(0f, homingDto.speed)
                : DefaultSpeed;

            arrivalThreshold = homingDto != null && homingDto.arrivalThreshold > 0f
                ? homingDto.arrivalThreshold
                : DefaultArrivalThreshold;

            projectileTransform = homingDto != null
                ? homingDto.targetTransform
                : null;

            IsInitialized = true;

            LogDebug(
                $"Initialize Speed={speed:F2} ArrivalThreshold={arrivalThreshold:F2} " +
                $"Projectile={GetTransformName(projectileTransform)} " +
                $"Dto={(dto != null ? dto.GetType().Name : "null")}");
        }

        public void SetContext(SkillProjectileMovementContext movementContext)
        {
            context = movementContext;
            debugInstanceId = GetHashCode();
            ownerTransform = context.owner;
            targetTransform = context.targetTransform;

            if (targetTransform != null)
            {
                currentTargetPosition = targetTransform.position;
                hasTargetPosition = true;
            }
            else
            {
                currentTargetPosition = context.spawnPosition;
                hasTargetPosition = false;
            }

            position = projectileTransform != null
                ? projectileTransform.position
                : context.spawnPosition;

            ApplyPositionToProjectile();
            ApplyRotationToProjectile();

            direction = ResolveInitialDirection();

            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                direction = Vector2.right;
            }

            retargetTimer = 0f;
            noTargetTimer = 0f;
            reachedEnd = false;
            IsInitialized = true;

            LogDebug(
                $"SetContext Projectile={GetTransformName(projectileTransform)} " +
                $"Owner={GetTransformName(ownerTransform)} " +
                $"Target={GetTransformName(targetTransform)} " +
                $"TargetPosition={currentTargetPosition} HasTargetPosition={hasTargetPosition} " +
                $"Spawn={position} Direction={direction} Speed={speed:F2} " +
                $"InitialMode=OppositeTargetDirection");
        }

        public void TickMovement(float deltaTime)
        {
            if (reachedEnd)
            {
                LogDebug("Tick skipped because reachedEnd=true");
                return;
            }

            RefreshTarget(deltaTime);

            if (!hasTargetPosition)
            {
                noTargetTimer += deltaTime;

                LogMoveDebug(
                    $"NoTargetPosition waiting Position={position} Direction={direction} " +
                    $"NoTargetTimer={noTargetTimer:F2}/{DefaultNoTargetLifeTime:F2}");

                if (noTargetTimer >= DefaultNoTargetLifeTime)
                {
                    reachedEnd = true;
                    LogDebug("ReachedEnd because no target position timeout");
                }

                return;
            }

            noTargetTimer = 0f;

            Vector2 targetPosition = currentTargetPosition;
            Vector2 toTarget = targetPosition - position;
            float distance = toTarget.magnitude;

            if (distance <= arrivalThreshold)
            {
                LogMoveDebug(
                    $"ArrivalThresholdReached but keep moving for hit detection " +
                    $"Distance={distance:F3} Threshold={arrivalThreshold:F3} " +
                    $"Target={GetTransformName(targetTransform)}");
            }

            Vector2 desiredDirection = toTarget.normalized;
            direction = RotateDirectionToward(
                direction,
                desiredDirection,
                DefaultTurnSpeed * deltaTime);

            float moveDistance = speed * deltaTime;

            if (moveDistance <= Mathf.Epsilon)
            {
                LogMoveDebug(
                    $"Move skipped because moveDistance is zero Speed={speed:F2} DeltaTime={deltaTime:F4}");
                return;
            }

            position += direction * moveDistance;

            ApplyPositionToProjectile();
            ApplyRotationToProjectile();

            LogMoveDebug(
                $"HomingMove Position={position} Target={GetTransformName(targetTransform)} " +
                $"MoveDistance={moveDistance:F3} TurnDelta={DefaultTurnSpeed * deltaTime:F2} " +
                $"TargetPos={targetPosition} HasTargetPosition={hasTargetPosition} Distance={distance:F2} " +
                $"Desired={desiredDirection} Direction={direction} Speed={speed:F2}");
        }

        public bool HasReachedEnd()
        {
            return reachedEnd;
        }

        public void ResetMovement()
        {
            position = projectileTransform != null
                ? projectileTransform.position
                : context.spawnPosition;

            ApplyPositionToProjectile();
            ApplyRotationToProjectile();

            targetTransform = context.targetTransform;
            ownerTransform = context.owner;

            if (targetTransform != null)
            {
                currentTargetPosition = targetTransform.position;
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
            noTargetTimer = 0f;
            reachedEnd = false;
            IsInitialized = true;

            LogDebug(
                $"ResetMovement Position={position} Target={GetTransformName(targetTransform)} " +
                $"Direction={direction}");
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

            if (ownerTransform != null)
            {
                Vector2 fromOwnerToSpawn = position - (Vector2)ownerTransform.position;

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

            if (targetTransform != null && IsTargetValid(targetTransform))
            {
                if (retargetTimer <= 0f)
                {
                    currentTargetPosition = targetTransform.position;
                    hasTargetPosition = true;
                    retargetTimer = DefaultRetargetInterval;

                    LogDebug(
                        $"Refresh target position Target={GetTransformName(targetTransform)} " +
                        $"TargetPosition={currentTargetPosition}");
                }

                return;
            }

            if (targetTransform != null)
            {
                LogDebug($"Target invalid: {GetTransformName(targetTransform)}");
            }

            targetTransform = null;

            if (retargetTimer > 0f)
            {
                return;
            }

            retargetTimer = DefaultRetargetInterval;
            targetTransform = FindNearestTarget();

            if (targetTransform != null)
            {
                currentTargetPosition = targetTransform.position;
                hasTargetPosition = true;
            }

            LogDebug(
                $"Retarget result: {GetTransformName(targetTransform)} " +
                $"TargetPosition={currentTargetPosition} HasTargetPosition={hasTargetPosition}");
        }

        private Transform FindNearestTarget()
        {
            CharacterManager ownerCharacter = ResolveCharacterManager(ownerTransform);

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

            LogDebug(
                $"FindNearestTarget Owner={GetTransformName(ownerTransform)} " +
                $"Nearest={(nearest != null ? nearest.name : "null")} " +
                $"DistanceSqr={nearestDistanceSqr:F2}");

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
            if (projectileTransform == null)
            {
                return;
            }

            projectileTransform.position = position;
        }

        private void ApplyRotationToProjectile()
        {
            if (projectileTransform == null || direction.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            projectileTransform.rotation = Quaternion.Euler(0f, 0f, angle);
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


        private void LogMoveDebug(string message)
        {
            if (!EnableDebugLog)
            {
                return;
            }

            debugMoveLogTimer -= Time.deltaTime;

            if (debugMoveLogTimer > 0f)
            {
                return;
            }

            debugMoveLogTimer = 0.25f;
            LogDebug(message);
        }

        private void LogDebug(string message)
        {
            if (!EnableDebugLog)
            {
                return;
            }

            Debug.Log(
                $"[HomingMovement:{debugInstanceId}] {message}");
        }

        private string GetTransformName(Transform target)
        {
            return target != null ? target.name : "null";
        }
    }
}