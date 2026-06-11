using UnityEngine;
using Skill;

namespace Character.Skill
{
    /// <summary>
    /// Executes the selected skill against the current target.
    ///
    /// This state assumes earlier states already selected:
    /// - CurrentTarget
    /// - SelectedSkill
    ///
    /// Movement is not handled here.
    /// </summary>
    public class AttackTargetState : ICharacterActionState
    {
        public bool IsFinished { get; private set; }

        private const float skillRangeCheckInset = 0f;


        private AnimationMono currentAnimation;
        private bool waitingForAttackAnimation;

        public void Enter(CharacterActionContext context)
        {
            IsFinished = false;
            currentAnimation = ResolveAnimation(context);
            waitingForAttackAnimation = false;

            context?.StateManager?.LogStateMessage(
                "AttackTargetState Enter");

            bool executed = ExecuteAttack(context);

            if (!executed)
            {
                ClearSelectedSkill(context);
                ClearCurrentTarget(context);
                IsFinished = true;
                return;
            }

            waitingForAttackAnimation =
                currentAnimation != null && currentAnimation.IsPlayingAttack();

            if (!waitingForAttackAnimation)
            {
                ClearSelectedSkill(context);
                ClearCurrentTarget(context);
                IsFinished = true;
            }
        }

        public void Tick(
            CharacterActionContext context,
            float deltaTime)
        {
            if (IsFinished || !waitingForAttackAnimation)
            {
                return;
            }

            if (currentAnimation == null || !currentAnimation.IsPlayingAttack())
            {
                waitingForAttackAnimation = false;
                ClearSelectedSkill(context);
                ClearCurrentTarget(context);
                IsFinished = true;
            }
        }

        public void Exit(CharacterActionContext context)
        {
            context?.StateManager?.LogStateMessage(
                "AttackTargetState Exit");
        }

        private bool ExecuteAttack(CharacterActionContext context)
        {
            if (context == null)
            {
                return false;
            }

            if (currentAnimation != null && currentAnimation.IsPlayingAttack())
            {
                context.StateManager?.LogStateMessage(
                    "AttackTargetState Failed: AttackAnimationPlaying");
                return false;
            }

            CharacterSkillManager skillManager = ResolveSkillManager(context);

            if (skillManager == null)
            {
                context.StateManager?.LogStateMessage(
                    "AttackTargetState Failed: SkillManagerMissing");
                return false;
            }

            if (context.SelectedSkill == null)
            {
                context.StateManager?.LogStateMessage(
                    "AttackTargetState Failed: SelectedSkillMissing");
                return false;
            }

            if (context.CharacterManager == null)
            {
                context.StateManager?.LogStateMessage(
                    "AttackTargetState Failed: CharacterManagerMissing");
                return false;
            }

            if (!context.CharacterManager.CanUseSkill)
            {
                context.StateManager?.LogStateMessage(
                    $"AttackTargetState Failed: SkillNotUsable {GetSkillName(context.SelectedSkill)}");
                return false;
            }

            EquipmentSkillRuntimeData selectedRuntime =
                skillManager.GetRuntimeBySkill(context.SelectedSkill);

            if (selectedRuntime == null)
            {
                context.StateManager?.LogStateMessage(
                    $"AttackTargetState Failed: RuntimeMissing {GetSkillName(context.SelectedSkill)}");
                return false;
            }

            bool requiresTarget = RequiresTarget(selectedRuntime);

            if (requiresTarget && context.CurrentTarget == null)
            {
                context.StateManager?.LogStateMessage(
                    "AttackTargetState Failed: TargetMissing");
                return false;
            }

            Transform target = requiresTarget
                ? context.CurrentTarget
                : null;

            bool executed =
                skillManager.FireSkill(
                    selectedRuntime,
                    context.Owner != null ? context.Owner.transform : null,
                    target);

            context.StateManager?.LogStateMessage(
                $"AttackTargetState Result: Skill={GetSkillName(context.SelectedSkill)} " +
                $"RuntimeFound={selectedRuntime != null} " +
                $"RequiresTarget={requiresTarget} " +
                $"Target={GetTargetName(target)} " +
                $"Executed={executed}");

            return executed;
        }

        private bool RequiresTarget(EquipmentSkillRuntimeData runtime)
        {
            SkillCastSO castSo = ResolveCastSo(runtime);

            if (castSo == null)
            {
                return true;
            }

            TargetingType targetingType = castSo.TargetingType;

            return targetingType != TargetingType.None &&
                   targetingType != TargetingType.Self;
        }

        private SkillCastSO ResolveCastSo(
            EquipmentSkillRuntimeData runtime)
        {
            if (runtime == null || runtime.sourceEquipment == null)
            {
                return null;
            }

            return runtime.sourceEquipment.CastSo;
        }

        private AnimationMono ResolveAnimation(CharacterActionContext context)
        {
            if (context == null || context.Owner == null)
            {
                return null;
            }

            return context.Owner.GetComponent<AnimationMono>()
                ?? context.Owner.GetComponentInChildren<AnimationMono>();
        }

        private void ClearSelectedSkill(CharacterActionContext context)
        {
            if (context == null)
            {
                return;
            }

            context.StateManager?.LogStateMessage(
                $"AttackTargetState ClearSelectedSkill: {GetSkillName(context.SelectedSkill)}");

            context.SelectedSkill = null;
        }

        private void ClearCurrentTarget(CharacterActionContext context)
        {
            if (context == null)
            {
                return;
            }

            context.StateManager?.LogStateMessage(
                $"AttackTargetState ClearCurrentTarget: {GetTargetName(context.CurrentTarget)}");

            context.CurrentTarget = null;
        }

        private CharacterSkillManager ResolveSkillManager(CharacterActionContext context)
        {
            if (context == null || context.Owner == null)
            {
                return null;
            }

            return context.Owner.GetComponent<CharacterSkillManager>()
                ?? context.Owner.GetComponentInChildren<CharacterSkillManager>();
        }

        private static string GetSkillName(EquipmentSkillSO skill)
        {
            return skill == null
                ? "null"
                : skill.name;
        }

        private static string GetTargetName(Transform target)
        {
            return target == null
                ? "null"
                : target.name;
        }
    }
}