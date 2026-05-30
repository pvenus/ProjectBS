using Character;
using UnityEngine;

namespace Effect
{
    public class TauntEffectRuntime
        : EffectRuntimeData
    {
        private readonly TauntEffectSO effectSO;
        private readonly CharacterManager targetCharacter;
        private readonly Transform tauntTarget;

        private NpcTargeting npcTargeting;
        private bool applied;

        public TauntEffectRuntime(
            TauntEffectSO effectSO,
            CharacterManager targetCharacter,
            Transform tauntTarget)
        {
            this.effectSO = effectSO;
            this.targetCharacter = targetCharacter;
            this.tauntTarget = tauntTarget;

            SourceType = EffectSourceType.Skill;
            SourceId = GetTargetId();

            RuntimeId =
                $"Taunt_{GetEffectId()}_{GetTargetRuntimeId()}_{GetTargetId()}";
        }

        public override void OnApply()
        {
            if (effectSO == null
                || targetCharacter == null
                || tauntTarget == null)
            {
                return;
            }

            npcTargeting = ResolveNpcTargeting(targetCharacter);

            if (npcTargeting == null)
            {
                return;
            }

            npcTargeting.ForceTarget(
                tauntTarget,
                effectSO.duration);

            applied = true;
        }

        public override void OnRemove()
        {
            if (!applied
                || npcTargeting == null
                || tauntTarget == null)
            {
                return;
            }

            npcTargeting.ClearForcedTarget(tauntTarget);
            applied = false;
        }

        private NpcTargeting ResolveNpcTargeting(
            CharacterManager characterManager)
        {
            if (characterManager == null)
            {
                return null;
            }

            NpcTargeting targeting =
                characterManager.GetComponent<NpcTargeting>();

            if (targeting != null)
            {
                return targeting;
            }

            targeting = characterManager.GetComponentInChildren<NpcTargeting>();

            if (targeting != null)
            {
                return targeting;
            }

            return characterManager.GetComponentInParent<NpcTargeting>();
        }

        private string GetEffectId()
        {
            return effectSO != null
                ? effectSO.effectId
                : "NoEffect";
        }

        private string GetTargetRuntimeId()
        {
            return targetCharacter != null
                ? targetCharacter.GetInstanceID().ToString()
                : "NoTarget";
        }

        private string GetTargetId()
        {
            return tauntTarget != null
                ? tauntTarget.GetInstanceID().ToString()
                : "NoTauntTarget";
        }
    }
}