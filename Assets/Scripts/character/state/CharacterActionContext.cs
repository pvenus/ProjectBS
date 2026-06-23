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

        public Vector2 DestinationPosition;

        public float DeltaTime;
    }
}
