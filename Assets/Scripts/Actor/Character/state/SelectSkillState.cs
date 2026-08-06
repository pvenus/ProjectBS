using Skill;

namespace Character.Skill
{
    /// <summary>
    /// Selects which skill the character will use for the current target.
    ///
    /// Current simple rule:
    /// - Use CharacterSkillManager as the primary selector.
    /// - Select the first cooldown-ready skill returned by CharacterSkillManager.
    /// - Store the selected EquipmentSkillSO in CharacterActionContext.
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
                    $"SelectSkillState Result: Skill={GetSkillName(context.SelectedSkillRuntime?.sourceEquipment)} Range={context.SelectedSkillRange:F2}");
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
                    context.SelectedSkillRuntime = selectedRuntime;

                    context.StateManager?.LogStateMessage(
                        $"SelectSkillState RuntimeInfo: SkillManager={skillManager.name} " +
                        $"SelectedSkill={GetSkillName(context.SelectedSkillRuntime.sourceEquipment)} " +
                        $"SelectedRange={context.SelectedSkillRange:F2}");

                    return;
                }
            }

            context.SelectedSkillRuntime = null;
        }

        private static string GetSkillName(EquipmentSkillSO skill)
        {
            return skill == null
                ? "null"
                : skill.name;
        }
    }
}