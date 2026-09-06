using UnityEngine;
using Skill;

namespace Character.Skill
{
    /// <summary>
    /// Executes the selected skill against the current target.
    ///
    /// This state assumes earlier states already selected:
    /// - CurrentTarget
    /// - SelectedSkillRuntime
    ///
    /// Movement is not handled here.
    /// </summary>
    public class AttackTargetState : ICharacterActionState
    {
        public bool IsFinished { get; private set; }

        private const float skillRangeCheckInset = 0f;


        private AnimationMono currentAnimation;
        private bool waitingForAttackAnimation;
        private bool waitingForCast;
        private CharacterSkillManager currentSkillManager;
        private CharacterActionContext currentContext;
        private EquipmentSkillRuntimeData pendingRuntime;
        private bool successfulUseRecorded;

        public void Enter(CharacterActionContext context)
        {
            IsFinished = false;
            currentAnimation = ResolveAnimation(context);
            waitingForAttackAnimation = false;
            waitingForCast = false;
            currentSkillManager = ResolveSkillManager(context);
            currentContext = context;
            pendingRuntime = context?.SelectedSkillRuntime;
            successfulUseRecorded = false;
            if (currentSkillManager != null)
            {
                currentSkillManager.CastCommitted += OnCastCommitted;
            }

            context?.StateManager?.LogStateMessage(
                "AttackTargetState Enter");

            bool executed = ExecuteAttack(context);

            if (!executed)
            {
                ResolveSkillManager(context)?.MarkSkillExecutionFailed(
                    context.SelectedSkillRuntime);
                ClearSelectedSkill(context);
                ClearCurrentTarget(context);
                IsFinished = true;
                return;
            }

            waitingForCast = currentSkillManager != null && currentSkillManager.IsCasting;
            waitingForAttackAnimation = !waitingForCast &&
                currentAnimation != null && currentAnimation.IsPlayingAttack();

            if (!waitingForCast && !waitingForAttackAnimation)
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
            if (IsFinished)
            {
                return;
            }

            if (waitingForCast)
            {
                if (currentSkillManager != null && currentSkillManager.IsCasting)
                {
                    return;
                }

                waitingForCast = false;
                waitingForAttackAnimation =
                    currentAnimation != null && currentAnimation.IsPlayingAttack();
                if (waitingForAttackAnimation)
                {
                    return;
                }

                ClearSelectedSkill(context);
                ClearCurrentTarget(context);
                IsFinished = true;
                return;
            }

            if (!waitingForAttackAnimation)
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
            if (currentSkillManager != null)
            {
                currentSkillManager.CastCommitted -= OnCastCommitted;
            }
            currentContext = null;
            pendingRuntime = null;
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

            if (context.SelectedSkillRuntime == null)
            {
                context.StateManager?.LogStateMessage(
                    "AttackTargetState Failed: SelectedSkillRuntimeMissing");
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
                    $"AttackTargetState Failed: SkillNotUsable {GetSkillName(context.SelectedSkillRuntime)}");
                return false;
            }

            EquipmentSkillRuntimeData selectedRuntime =
                context.SelectedSkillRuntime;

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

            if (executed && !skillManager.IsCasting)
            {
                RecordSuccessfulUseOnce(selectedRuntime, skillManager);
            }

            context.StateManager?.LogStateMessage(
                $"AttackTargetState Result: Skill={GetSkillName(selectedRuntime)} " +
                $"RuntimeFound={selectedRuntime != null} " +
                $"RequiresTarget={requiresTarget} " +
                $"Target={GetTargetName(target)} " +
                $"Executed={executed}");

            return executed;
        }

        private void OnCastCommitted(EquipmentSkillRuntimeData runtime)
        {
            if (runtime != pendingRuntime || currentSkillManager == null)
            {
                return;
            }

            RecordSuccessfulUseOnce(runtime, currentSkillManager);
        }

        private void RecordSuccessfulUseOnce(
            EquipmentSkillRuntimeData runtime,
            CharacterSkillManager skillManager)
        {
            if (successfulUseRecorded)
            {
                return;
            }

            successfulUseRecorded = true;
            currentContext?.StateService?.RecordSuccessfulSkillUse(runtime, skillManager);
        }

        private bool RequiresTarget(EquipmentSkillRuntimeData runtime)
        {
            if (runtime?.sourceEquipment?.BaseProfileSo != null &&
                runtime.sourceEquipment.BaseProfileSo.SkillComponentType == SkillComponentType.Spawn)
            {
                return false;
            }

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
                $"AttackTargetState ClearSelectedSkill: {GetSkillName(context.SelectedSkillRuntime)}");

            context.SelectedSkillRuntime = null;
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

        private static string GetSkillName(EquipmentSkillRuntimeData runtime)
        {
            EquipmentSkillSO skill = runtime?.sourceEquipment;
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
