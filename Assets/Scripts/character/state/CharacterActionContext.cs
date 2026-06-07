using UnityEngine;
using Skill;

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

        public SkillExecutorMono SkillExecutor;

        public MovementController MovementController;

        public CharacterMovementExecutionService MovementExecutionService;

        public AnimationMono AnimationMono;

        public EquipmentSkillSO SelectedSkill;

        public float SelectedSkillRange =>
            SelectedSkill?.CastSo?.Range ?? 0f;

        public bool HasSelectedSkill => SelectedSkill != null;

        public Vector2 DestinationPosition;

        public float DeltaTime;
    }
}
