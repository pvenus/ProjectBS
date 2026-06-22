using UnityEngine;
using Skill;
using Character.Skill;

namespace Character
{
    /// <summary>
    /// Shared runtime context used by character states and decision logic.
    /// Start small and expand only when a state actually needs data.
    /// </summary>
    public class CharacterActionContext
    {
        public GameObject Owner;

        public Transform OwnerTransform;

        public CharacterManager CharacterManager;

        public CharacterStateManager StateManager;

        public Transform CurrentTarget;

        public Transform ForcedTarget;

        public float ForcedTargetEndTime;

        public bool HasLurePoint;

        public Vector2 LurePoint;

        public float LureEndTime;

        public float LureRadius;

        public float LureMoveSpeedMultiplier = 1f;

        public SkillExecutorMono SkillExecutor;

        public CharacterSkillManager SkillManager;

        public MovementController MovementController;

        public CharacterMovementExecutionService MovementExecutionService;

        public AnimationMono AnimationMono;

        public EquipmentSkillRuntimeData SelectedSkillRuntime;

        public float SelectedSkillRange =>
            SelectedSkillRuntime?.resolvedRange ?? 0f;

        public bool HasSelectedSkill => SelectedSkillRuntime != null;

        public void ApplyForcedTarget(
            Transform target,
            float duration)
        {
            ForcedTarget = target;
            ForcedTargetEndTime = Time.time + Mathf.Max(0f, duration);
        }

        public void ClearForcedTarget()
        {
            ForcedTarget = null;
            ForcedTargetEndTime = 0f;
        }

        public bool TryGetForcedTarget(out Transform target)
        {
            if (ForcedTarget == null || Time.time > ForcedTargetEndTime)
            {
                ClearForcedTarget();
                target = null;
                return false;
            }

            target = ForcedTarget;
            return true;
        }

        public void ApplyLurePoint(
            Vector2 lurePoint,
            float duration,
            float lureRadius,
            float lureMoveSpeedMultiplier)
        {
            HasLurePoint = true;
            LurePoint = lurePoint;
            LureEndTime = Time.time + Mathf.Max(0f, duration);
            LureRadius = Mathf.Max(0f, lureRadius);
            LureMoveSpeedMultiplier = Mathf.Max(0f, lureMoveSpeedMultiplier);
        }

        public void ClearLurePoint()
        {
            HasLurePoint = false;
            LurePoint = Vector2.zero;
            LureEndTime = 0f;
            LureRadius = 0f;
            LureMoveSpeedMultiplier = 1f;
        }

        public bool TryGetLurePoint(out Vector2 lurePoint)
        {
            if (!HasLurePoint || Time.time > LureEndTime)
            {
                ClearLurePoint();
                lurePoint = Vector2.zero;
                return false;
            }

            lurePoint = LurePoint;
            return true;
        }

        public Vector2 DestinationPosition;

        public float DeltaTime;
    }
}
