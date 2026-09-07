using System.Collections;
using Skill;
using UnityEngine;
using Character.Movement;

namespace Character.Movement
{
    public enum CharacterMovementPriority
    {
        Party = 0,
        Kiting = 10,
        BasicComboLunge = 20,
        SwiftStep = 30,
        Charge = 40,
        ForcedCrowdControl = 50
    }

    [DisallowMultipleComponent]
    public sealed class CharacterMovementAuthority : MonoBehaviour
    {
        private object owner;
        private CharacterMovementPriority priority;
        public bool TryAcquire(object candidate, CharacterMovementPriority candidatePriority)
        {
            if (candidate == null) return false;
            if (owner == candidate) return true;
            if (owner != null && candidatePriority <= priority) return false;
            owner = candidate;
            priority = candidatePriority;
            return true;
        }
        public void Release(object candidate)
        {
            if (owner != candidate) return;
            owner = null;
            priority = CharacterMovementPriority.Party;
        }
        public void ForceRelease()
        {
            owner = null;
            priority = CharacterMovementPriority.Party;
        }
        private void OnDisable() => ForceRelease();
    }
}

namespace Character.Skill
{
    /// <summary>
    /// 스킬 시전 시 시전자 본체 이동을 처리하는 순수 서비스.
    /// Projectile 이동과 분리된 개념이며, 한 번의 완결적인 이동을 Coroutine으로 수행한다.
    ///
    /// 이동 중 다른 이동을 막는 처리는 추후 캐릭터 이동 제어 계층에서 연결한다.
    /// </summary>
    public class CastMoveService
    {
        public readonly struct TerminalReceipt
        {
            public readonly bool Committed;
            public readonly bool Succeeded;
            public readonly float Distance;

            public TerminalReceipt(bool committed, bool succeeded, float distance)
            {
                Committed = committed;
                Succeeded = succeeded;
                Distance = distance;
            }
        }

        private Coroutine runningRoutine;
        private MonoBehaviour coroutineRunner;
        private CharacterMovementAuthority movementAuthority;
        private PartyMovementMono partyMovement;
        private System.Action<TerminalReceipt> activeTerminal;
        private bool activeCommitted;

        public bool IsMoving => runningRoutine != null;

        public void StopMove()
        {
            if (runningRoutine == null)
            {
                return;
            }

            if (coroutineRunner != null)
            {
                coroutineRunner.StopCoroutine(runningRoutine);
            }

            runningRoutine = null;
            coroutineRunner = null;
            ReleaseMovementOwnership();
            System.Action<TerminalReceipt> terminal = activeTerminal;
            activeTerminal = null;
            bool committed = activeCommitted;
            activeCommitted = false;
            terminal?.Invoke(new TerminalReceipt(committed, false, 0f));
        }

        public bool TryStartMove(
            MonoBehaviour runner,
            Transform casterTransform,
            Transform targetTransform,
            Vector2 castDirection,
            CastMoveProfile castMove)
        {
            return TryStartMove(runner, casterTransform, targetTransform, castDirection,
                castMove, CharacterMovementPriority.Charge, null, null);
        }

        public bool TryStartMove(
            MonoBehaviour runner,
            Transform casterTransform,
            Transform targetTransform,
            Vector2 castDirection,
            CastMoveProfile castMove,
            CharacterMovementPriority priority,
            System.Action onCommitted,
            System.Action<TerminalReceipt> onTerminal,
            System.Func<bool> preparationFrameObserved = null)
        {
            if (runner == null ||
                casterTransform == null ||
                castMove == null ||
                castMove.MoveType == CastMoveType.None)
            {
                return false;
            }

            StopMove();

            movementAuthority = casterTransform.GetComponent<CharacterMovementAuthority>()
                ?? casterTransform.GetComponentInParent<CharacterMovementAuthority>();
            if (movementAuthority == null)
            {
                movementAuthority = casterTransform.gameObject.AddComponent<CharacterMovementAuthority>();
            }
            if (!movementAuthority.TryAcquire(this, priority))
            {
                movementAuthority = null;
                return false;
            }

            partyMovement = casterTransform.GetComponent<PartyMovementMono>()
                ?? casterTransform.GetComponentInParent<PartyMovementMono>();
            if (partyMovement != null &&
                !partyMovement.TryAcquireExternalMovement(
                    this,
                    (priority == CharacterMovementPriority.SwiftStep || (runner is CharacterSkillManager manual && manual.ManualExecutionAuthorized))))
            {
                ReleaseMovementOwnership();
                return false;
            }

            Vector2 direction = ResolveDirection(
                casterTransform,
                targetTransform,
                castDirection,
                castMove.MoveType);

            if (direction.sqrMagnitude <= 0.0001f)
            {
                ReleaseMovementOwnership();
                return false;
            }

            coroutineRunner = runner;
            activeTerminal = onTerminal;
            activeCommitted = false;
            runningRoutine = runner.StartCoroutine(
                MoveRoutine(
                    casterTransform,
                    direction.normalized,
                    castMove.Distance,
                    castMove.Duration,
                    castMove,
                    priority == CharacterMovementPriority.SwiftStep,
                    onCommitted,
                    onTerminal,
                    preparationFrameObserved));

            return true;
        }

        private IEnumerator MoveRoutine(
            Transform target,
            Vector2 direction,
            float distance,
            float duration,
            CastMoveProfile profile,
            bool useCollisionClassifiedPosition,
            System.Action onCommitted,
            System.Action<TerminalReceipt> onTerminal,
            System.Func<bool> preparationFrameObserved)
        {
            if (target == null)
            {
                ClearRunningRoutine();
                onTerminal?.Invoke(new TerminalReceipt(false, false, 0f));
                yield break;
            }

            float anticipationStartedAt = Time.time;
            if (preparationFrameObserved != null)
            {
                // Begin() applies F0 synchronously. Yielding at least once lets the
                // renderer present it before a FixedUpdate can commit movement.
                do
                {
                    yield return null;
                    if (!CanContinueMovement(target))
                    {
                        ClearRunningRoutine();
                        onTerminal?.Invoke(new TerminalReceipt(false, false, 0f));
                        yield break;
                    }
                }
                while (!preparationFrameObserved());
            }

            float remainingAnticipation = profile.Anticipation -
                (Time.time - anticipationStartedAt);
            if (remainingAnticipation > 0f)
            {
                yield return new WaitForSeconds(remainingAnticipation);
            }

            if (!CanContinueMovement(target))
            {
                ClearRunningRoutine();
                onTerminal?.Invoke(new TerminalReceipt(false, false, 0f));
                yield break;
            }

            Rigidbody2D body = target.GetComponent<Rigidbody2D>()
                ?? target.GetComponentInParent<Rigidbody2D>();
            Vector2 startPosition = ResolveCurrentPosition(body, target);
            float allowedDistance = ResolveAllowedDistance(target, direction, distance, profile);
            Vector2 endPosition = startPosition + direction * allowedDistance;
            if (allowedDistance < profile.MinSuccessDistance)
            {
                ClearRunningRoutine();
                onTerminal?.Invoke(new TerminalReceipt(false, false, allowedDistance));
                yield break;
            }
            activeCommitted = true;
            onCommitted?.Invoke();

            if (duration <= 0f)
            {
                ApplyPosition(body, target, endPosition, useCollisionClassifiedPosition);
                ClearRunningRoutine();
                float appliedDistance = Vector2.Distance(
                    startPosition, ResolveCurrentPosition(body, target));
                onTerminal?.Invoke(new TerminalReceipt(appliedDistance > 0f,
                    appliedDistance >= profile.MinSuccessDistance, appliedDistance));
                yield break;
            }

            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (!CanContinueMovement(target))
                {
                    ClearRunningRoutine();
                    float cancelledDistance = target != null
                        ? Vector2.Distance(startPosition, ResolveCurrentPosition(body, target))
                        : 0f;
                    onTerminal?.Invoke(new TerminalReceipt(true, false, cancelledDistance));
                    yield break;
                }

                elapsed += Time.fixedDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                Vector2 nextPosition = Vector2.Lerp(startPosition, endPosition, t);

                ApplyPosition(body, target, nextPosition, useCollisionClassifiedPosition);

                yield return new WaitForFixedUpdate();
            }

            if (target != null)
            {
                ApplyPosition(body, target, endPosition, useCollisionClassifiedPosition);
            }

            float terminalDistance = Vector2.Distance(
                startPosition, ResolveCurrentPosition(body, target));
            ClearRunningRoutine();
            onTerminal?.Invoke(new TerminalReceipt(terminalDistance > 0f,
                terminalDistance >= profile.MinSuccessDistance, terminalDistance));
        }

        private static bool CanContinueMovement(Transform target)
        {
            if (target == null || !target.gameObject.activeInHierarchy) return false;
            CharacterManager character = target.GetComponent<CharacterManager>()
                ?? target.GetComponentInParent<CharacterManager>()
                ?? target.GetComponentInChildren<CharacterManager>();
            return character == null || (character.CanMove && character.CanUseSkill);
        }

        private static void ApplyPosition(
            Rigidbody2D body,
            Transform target,
            Vector2 position,
            bool useCollisionClassifiedPosition)
        {
            if (body == null)
            {
                target.position = new Vector3(position.x, position.y, target.position.z);
                return;
            }

            if (useCollisionClassifiedPosition)
            {
                // ResolveAllowedDistance already clipped the straight path against
                // static environment geometry. Assigning the resolved position keeps
                // actor bodies from being reintroduced as blockers by MovePosition's
                // collision response.
                body.position = position;
                return;
            }

            body.MovePosition(position);
        }

        private static Vector2 ResolveCurrentPosition(
            Rigidbody2D body,
            Transform target)
        {
            return body != null ? body.position : (Vector2)target.position;
        }

        private float ResolveAllowedDistance(
            Transform target,
            Vector2 direction,
            float requested,
            CastMoveProfile profile)
        {
            if (!profile.StopOnWall || requested <= 0f) return Mathf.Max(0f, requested);
            Rigidbody2D body = target.GetComponent<Rigidbody2D>()
                ?? target.GetComponentInParent<Rigidbody2D>();
            Collider2D collider = target.GetComponent<Collider2D>()
                ?? target.GetComponentInParent<Collider2D>();
            if (body == null || collider == null) return Mathf.Max(0f, requested);

            ContactFilter2D filter = new ContactFilter2D();
            filter.useTriggers = false;
            RaycastHit2D[] hits = new RaycastHit2D[8];
            int count = collider.Cast(direction, filter, hits, requested + profile.WallSkin);
            float allowed = requested;
            for (int i = 0; i < count; i++)
            {
                Collider2D hit = hits[i].collider;
                if (IsNonBlockingCastCollider(hit, target)) continue;
                allowed = Mathf.Min(allowed, Mathf.Max(0f, hits[i].distance - profile.WallSkin));
            }
            return allowed;
        }

        private static bool IsNonBlockingCastCollider(
            Collider2D hit,
            Transform caster)
        {
            if (hit == null) return true;

            Transform casterRoot = caster != null ? caster.root : null;
            Rigidbody2D hitBody = hit.attachedRigidbody;
            Transform hitBodyTransform = hitBody != null ? hitBody.transform : null;

            if (IsSameRoot(hit.transform, casterRoot) ||
                IsSameRoot(hitBodyTransform, casterRoot))
            {
                return true;
            }

            // Hurtboxes are commonly children of the actor while their Rigidbody2D
            // and CharacterManager live on a parent or canonical root.
            if (HasOwner<CharacterManager>(hit, hitBodyTransform) ||
                HasOwner<ProjectileEntity>(hit, hitBodyTransform))
            {
                return true;
            }

            // Triggers, pickups and other moving runtime entities are presentation /
            // interaction bodies, not terrain. Only static environment geometry is
            // allowed to shorten a cast move.
            if (hit.isTrigger) return true;
            return hitBody != null && hitBody.bodyType != RigidbodyType2D.Static;
        }

        private static bool IsSameRoot(Transform candidate, Transform expectedRoot)
        {
            return candidate != null &&
                   expectedRoot != null &&
                   candidate.root == expectedRoot;
        }

        private static bool HasOwner<T>(
            Collider2D hit,
            Transform attachedBodyTransform)
            where T : Component
        {
            if (hit.GetComponentInParent<T>(true) != null) return true;
            if (attachedBodyTransform != null &&
                attachedBodyTransform.GetComponentInParent<T>(true) != null)
            {
                return true;
            }

            Transform hitRoot = hit.transform.root;
            if (hitRoot != null && hitRoot.GetComponent<T>() != null) return true;

            Transform bodyRoot = attachedBodyTransform != null
                ? attachedBodyTransform.root
                : null;
            return bodyRoot != null && bodyRoot.GetComponent<T>() != null;
        }

        private void ClearRunningRoutine()
        {
            runningRoutine = null;
            coroutineRunner = null;
            activeTerminal = null;
            activeCommitted = false;
            ReleaseMovementOwnership();
        }

        private void ReleaseMovementOwnership()
        {
            partyMovement?.ReleaseExternalMovement(this);
            partyMovement = null;
            movementAuthority?.Release(this);
            movementAuthority = null;
        }

        private Vector2 ResolveDirection(
            Transform casterTransform,
            Transform targetTransform,
            Vector2 castDirection,
            CastMoveType moveType)
        {
            switch (moveType)
            {
                case CastMoveType.DashForward:
                    return ResolveForwardDirection(casterTransform, targetTransform, castDirection);

                case CastMoveType.DashBackward:
                    return -ResolveForwardDirection(casterTransform, targetTransform, castDirection);

                case CastMoveType.MoveToTarget:
                    return ResolveTargetDirection(casterTransform, targetTransform);

                case CastMoveType.MoveAwayFromTarget:
                    return -ResolveTargetDirection(casterTransform, targetTransform);

                case CastMoveType.None:
                default:
                    return Vector2.zero;
            }
        }

        private Vector2 ResolveForwardDirection(
            Transform casterTransform,
            Transform targetTransform,
            Vector2 castDirection)
        {
            if (castDirection.sqrMagnitude > 0.0001f)
            {
                return castDirection.normalized;
            }

            Vector2 targetDirection = ResolveTargetDirection(
                casterTransform,
                targetTransform);

            if (targetDirection.sqrMagnitude > 0.0001f)
            {
                return targetDirection.normalized;
            }

            if (casterTransform != null)
            {
                Vector2 right = casterTransform.right;

                if (right.sqrMagnitude > 0.0001f)
                {
                    return right.normalized;
                }
            }

            return Vector2.right;
        }

        private Vector2 ResolveTargetDirection(
            Transform casterTransform,
            Transform targetTransform)
        {
            if (casterTransform == null || targetTransform == null)
            {
                return Vector2.zero;
            }

            Vector2 direction = targetTransform.position - casterTransform.position;

            if (direction.sqrMagnitude <= 0.0001f)
            {
                return Vector2.zero;
            }

            return direction.normalized;
        }
    }
}
