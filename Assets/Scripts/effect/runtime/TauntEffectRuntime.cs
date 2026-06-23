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

        private CharacterStateManager stateManager;
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

            stateManager = ResolveCharacterStateManager(targetCharacter);

            if (stateManager == null)
            {
                return;
            }

            stateManager.ApplyForcedTarget(
                tauntTarget,
                effectSO.duration);

            applied = true;
        }

        public override void OnRemove()
        {
            if (!applied
                || stateManager == null)
            {
                return;
            }

            stateManager.ClearForcedTarget(tauntTarget);

            applied = false;
        }

        private CharacterStateManager ResolveCharacterStateManager(
            CharacterManager characterManager)
        {
            if (characterManager == null)
            {
                return null;
            }

            CharacterStateManager manager =
                characterManager.GetComponent<CharacterStateManager>();

            if (manager != null)
            {
                return manager;
            }

            manager = characterManager.GetComponentInChildren<CharacterStateManager>();

            if (manager != null)
            {
                return manager;
            }

            return characterManager.GetComponentInParent<CharacterStateManager>();
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