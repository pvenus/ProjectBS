using Skill;
using UnityEngine;

namespace Character.Skill
{
    /// <summary>
    /// Decides what the character should do next.
    ///
    /// Current version is intentionally simple.
    /// Later this can be expanded with:
    /// - Traits
    /// - Emotions
    /// - RL policies
    /// - Ontology reasoning
    /// - LLM decisions
    /// </summary>
    public class CharacterDecisionEngine
    {
        private readonly LayerMask _targetMask;

        public CharacterDecisionEngine(LayerMask targetMask)
        {
            _targetMask = targetMask;
        }

        public ICharacterActionState Decide(
            CharacterActionContext context)
        {
            if (context == null)
            {
                return null;
            }

            context.StateManager?.LogStateMessage(
                $"Decision check: Skill={GetSkillName(context.SelectedSkillRuntime?.sourceEquipment)} " +
                $"Target={GetTargetName(context.CurrentTarget)}");

            if (!context.HasSelectedSkill)
            {
                context.StateManager?.LogStateMessage(
                    "Decision selected: SelectSkillState because selected skill is missing");
                return new SelectSkillState();
            }

            if (!RequiresTarget(context))
            {
                context.StateManager?.LogStateMessage(
                    "Decision selected: AttackTargetState because selected skill does not require target");
                return new AttackTargetState();
            }

            if (!HasValidTarget(context))
            {
                LayerMask[] targetMasks = ResolveTargetMask(context);

                context.StateManager?.LogStateMessage(
                    $"Decision selected: FindTargetState because target is missing or invalid TargetMaskCount={targetMasks.Length}");
                return new FindTargetState(targetMasks);
            }

            if (!IsTargetInSelectedSkillRange(context))
            {
                context.StateManager?.LogStateMessage(
                    "Decision selected: MoveToTargetState because target is out of selected skill range");
                return new MoveToTargetState();
            }

            context.StateManager?.LogStateMessage(
                "Decision selected: AttackTargetState because target is in selected skill range");
            return new AttackTargetState();
        }

        private LayerMask[] ResolveTargetMask(CharacterActionContext context)
        {
            if (context?.SelectedSkillRuntime?.sourceEquipment?.HitSos != null &&
                context.SelectedSkillRuntime.sourceEquipment.HitSos.Length > 0)
            {
                LayerMask[] targetMasks = new LayerMask[context.SelectedSkillRuntime.sourceEquipment.HitSos.Length];

                for (int i = 0; i < context.SelectedSkillRuntime.sourceEquipment.HitSos.Length; i++)
                {
                    targetMasks[i] = context.SelectedSkillRuntime.sourceEquipment.HitSos[i].TargetLayerMask;
                }

                return targetMasks;
            }

            return new[] { _targetMask };
        }

        private bool RequiresTarget(CharacterActionContext context)
        {
            if (context?.SelectedSkillRuntime == null || context.SelectedSkillRuntime.sourceEquipment.CastSo == null)
            {
                return true;
            }
            if (context.SelectedSkillRuntime.sourceEquipment.BaseProfileSo != null &&
                context.SelectedSkillRuntime.sourceEquipment.BaseProfileSo.SkillComponentType == SkillComponentType.Spawn)
            {
                return false;
            }

            TargetingType targetingType = context.SelectedSkillRuntime.sourceEquipment.CastSo.TargetingType;

            return targetingType != TargetingType.None &&
                   targetingType != TargetingType.Self;
        }

        private bool HasValidTarget(CharacterActionContext context)
        {
            Transform target = context.CurrentTarget;

            if (target == null)
            {
                return false;
            }

            if (!target.gameObject.activeInHierarchy)
            {
                return false;
            }

            return true;
        }
        private bool IsTargetInSelectedSkillRange(CharacterActionContext context)
        {
            if (context == null)
            {
                return false;
            }

            if (context.OwnerTransform == null || context.CurrentTarget == null)
            {
                return false;
            }

            if (context.SelectedSkillRuntime == null)
            {
                return false;
            }
            if (context.SelectedSkillRuntime.sourceEquipment.BaseProfileSo != null &&
                context.SelectedSkillRuntime.sourceEquipment.BaseProfileSo.SkillComponentType == SkillComponentType.Spawn)
            {
                return true;
            }

            float range = context.SelectedSkillRange;

            if (range <= 0f)
            {
                return false;
            }

            float distance = Vector2.Distance(
                context.OwnerTransform.position,
                context.CurrentTarget.position);

            context.StateManager?.LogStateMessage(
                $"Decision range check: Distance={distance:F2} Range={range:F2}");

            return distance <= range;
        }
        private static string GetTargetName(Transform target)
        {
            return target == null
                ? "null"
                : target.name;
        }

        private static string GetSkillName(ScriptableObject skill)
        {
            return skill == null
                ? "null"
                : skill.name;
        }
    }
}
