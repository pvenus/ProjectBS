using System.Collections;
using Skill;
using UnityEngine;

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
        private Coroutine runningRoutine;
        private MonoBehaviour coroutineRunner;

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
        }

        public bool TryStartMove(
            MonoBehaviour runner,
            Transform casterTransform,
            Transform targetTransform,
            Vector2 castDirection,
            SkillCastMoveSO castMoveSo)
        {
            if (runner == null || casterTransform == null || castMoveSo == null || !castMoveSo.HasMove)
            {
                return false;
            }

            StopMove();

            Vector2 direction = ResolveDirection(
                casterTransform,
                targetTransform,
                castDirection,
                castMoveSo.MoveType);

            if (direction.sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            coroutineRunner = runner;
            runningRoutine = runner.StartCoroutine(
                MoveRoutine(
                    casterTransform,
                    direction.normalized,
                    castMoveSo.Distance,
                    castMoveSo.Duration,
                    castMoveSo.StopOnWall));

            return true;
        }

        private IEnumerator MoveRoutine(
            Transform target,
            Vector2 direction,
            float distance,
            float duration,
            bool stopOnWall)
        {
            if (target == null)
            {
                ClearRunningRoutine();
                yield break;
            }

            Vector3 startPosition = target.position;
            Vector3 endPosition = startPosition + (Vector3)(direction * distance);

            if (duration <= 0f)
            {
                target.position = endPosition;
                ClearRunningRoutine();
                yield break;
            }

            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (target == null)
                {
                    ClearRunningRoutine();
                    yield break;
                }

                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                Vector3 nextPosition = Vector3.Lerp(startPosition, endPosition, t);

                target.position = nextPosition;

                yield return null;
            }

            if (target != null)
            {
                target.position = endPosition;
            }

            ClearRunningRoutine();
        }

        private void ClearRunningRoutine()
        {
            runningRoutine = null;
            coroutineRunner = null;
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