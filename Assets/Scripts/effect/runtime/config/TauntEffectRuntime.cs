using Character;
using UnityEngine;

namespace Effect
{
    public class TauntEffectRuntime
        : EffectRuntimeData
    {
        private readonly EffectSO effectSO;
        private readonly TauntEffectConfig config;
        private readonly CharacterManager targetCharacter;
        private readonly Transform tauntTarget;

        private CharacterStateManager stateManager;
        private bool applied;
        private float applySecond;

        public TauntEffectRuntime(
            EffectSO effectSO,
            TauntEffectConfig config,
            CharacterManager targetCharacter,
            Transform tauntTarget,
            float duration)
        {
            this.effectSO = effectSO;
            this.config = config;
            this.targetCharacter = targetCharacter;
            this.tauntTarget = tauntTarget;


            RuntimeId =
                $"Taunt_{GetEffectId()}_{GetTargetRuntimeId()}_{GetTargetId()}";
            applySecond = duration;
        }

        public override void OnApply()
        {
            if (effectSO == null
                || config == null
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
                applySecond);

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
                ? effectSO.EffectId
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