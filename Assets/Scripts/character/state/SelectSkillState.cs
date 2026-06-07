using UnityEngine;
using Skill;

namespace Character.Skill
{
    /// <summary>
    /// Selects which skill the character will use for the current target.
    ///
    /// Current simple rule:
    /// - Use CharacterSkillManager as the primary selector.
    /// - Pick the first cooldown-ready skill from the character skill pool.
    /// - Store the selected EquipmentSkillSO in CharacterActionContext.
    ///
    /// Fallback:
    /// - If CharacterSkillManager is missing or no ready runtime exists,
    ///   use SkillExecutorMono basic attack for legacy compatibility.
    /// </summary>
    public class SelectSkillState : ICharacterActionState
    {

        public bool IsFinished { get; private set; }

        public void Enter(CharacterActionContext context)
        {
            IsFinished = false;

            context?.StateManager?.LogStateMessage(
                "SelectSkillState Enter");

            if (context != null)
            {
                SelectSkill(context);

                context.StateManager?.LogStateMessage(
                    $"SelectSkillState Result: Skill={GetSkillName(context.SelectedSkill)} Range={context.SelectedSkillRange:F2}");
            }

            IsFinished = true;
        }

        public void Tick(CharacterActionContext context, float deltaTime)
        {
        }

        public void Exit(CharacterActionContext context)
        {
            context?.StateManager?.LogStateMessage(
                "SelectSkillState Exit");
        }

        private void SelectSkill(CharacterActionContext context)
        {
            if (context == null)
            {
                return;
            }

            CharacterSkillManager skillManager = null;

            if (context.Owner != null)
            {
                skillManager =
                    context.Owner.GetComponent<CharacterSkillManager>()
                    ?? context.Owner.GetComponentInChildren<CharacterSkillManager>();
            }

            if (skillManager != null)
            {
                EquipmentSkillRuntimeData selectedRuntime =
                    skillManager.SelectReadyActiveSkill();

                EquipmentSkillSO selectedSkill =
                    selectedRuntime != null
                        ? selectedRuntime.sourceEquipment
                        : null;

                if (selectedSkill != null)
                {
                    context.SelectedSkill = selectedSkill;

                    context.StateManager?.LogStateMessage(
                        $"SelectSkillState RuntimeInfo: SkillManager={skillManager.name} " +
                        $"SelectedSkill={GetSkillName(context.SelectedSkill)} " +
                        $"SelectedRange={context.SelectedSkillRange:F2}");

                    return;
                }

                context.StateManager?.LogStateMessage(
                    $"SelectSkillState RuntimeInfo: SkillManager={skillManager.name} ReadySkill=null");
            }

            SelectLegacyBasicAttack(context);
        }

        private void SelectLegacyBasicAttack(CharacterActionContext context)
        {
            if (context.SkillExecutor == null && context.Owner != null)
            {
                context.SkillExecutor =
                    context.Owner.GetComponent<SkillExecutorMono>()
                    ?? context.Owner.GetComponentInChildren<SkillExecutorMono>();
            }

            SkillExecutorMono executor = context.SkillExecutor;

            if (executor == null)
            {
                context.SelectedSkill = null;

                context.StateManager?.LogStateMessage(
                    "SelectSkillState RuntimeInfo: SkillManager=null SkillExecutor=null");
                return;
            }

            context.SelectedSkill =
                executor.GetBasicAttackSkill() as EquipmentSkillSO;

            context.StateManager?.LogStateMessage(
                $"SelectSkillState RuntimeInfo: LegacyBasicAttack SkillExecutor={executor.name} " +
                $"SelectedSkill={GetSkillName(context.SelectedSkill)} " +
                $"SelectedRange={context.SelectedSkillRange:F2}");
        }


        private static string GetSkillName(EquipmentSkillSO skill)
        {
            return skill == null
                ? "null"
                : skill.name;
        }
    }
}