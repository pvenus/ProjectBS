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
                if (ShouldEnterKiting(context, out KitingRepositionState kiting))
                {
                    context.StateManager?.LogStateMessage(
                        "Decision selected: KitingRepositionState because offensive skills are temporarily unavailable");
                    return kiting;
                }
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

        private static bool ShouldEnterKiting(
            CharacterActionContext context,
            out KitingRepositionState state)
        {
            state = null;
            if (!KitingRepositionState.TryCreate(context, out KitingRepositionState candidate) ||
                context?.SkillManager == null || context.CharacterManager == null ||
                !context.CharacterManager.CanMove || !context.CharacterManager.CanUseSkill)
            {
                return false;
            }

            PartyMovementMono partyMovement = context.Owner != null
                ? context.Owner.GetComponent<PartyMovementMono>() ??
                  context.Owner.GetComponentInChildren<PartyMovementMono>()
                : null;
            if (partyMovement != null && partyMovement.HasRecentManualInput())
            {
                return false;
            }

            OffensiveReadinessSnapshot snapshot = context.SkillManager.QueryOffensiveReadiness(
                context.StateService == null || context.StateService.CanUseActiveSkill,
                context.CharacterManager.CanUseSkill,
                context.CurrentTarget != null);
            if (snapshot.hasUsableOffensive || snapshot.blockReason == OffensiveBlockReason.CrowdControl)
            {
                return false;
            }

            state = candidate;
            return snapshot.blockReason == OffensiveBlockReason.Cooldown ||
                   snapshot.blockReason == OffensiveBlockReason.FailureRetry ||
                   snapshot.blockReason == OffensiveBlockReason.Cadence ||
                   snapshot.blockReason == OffensiveBlockReason.NoOffensiveRuntime;
        }

        private LayerMask[] ResolveTargetMask(CharacterActionContext context)
        {
            return new[] { ResolvePrimaryTargetMask(context) };
        }

        private LayerMask ResolvePrimaryTargetMask(CharacterActionContext context)
        {
            SkillHitSO[] hitSos =
                context?.SelectedSkillRuntime?.sourceEquipment?.HitSos;

            if (hitSos != null && hitSos.Length > 0 && hitSos[0] != null)
            {
                // A skill can contain secondary hit definitions for auxiliary
                // effects such as healing allies. Those masks control effect
                // application only; they must not become AI target candidates.
                return hitSos[0].TargetLayerMask;
            }

            return _targetMask;
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

            LayerMask primaryTargetMask = ResolvePrimaryTargetMask(context);
            return (primaryTargetMask.value & (1 << target.gameObject.layer)) != 0;
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
