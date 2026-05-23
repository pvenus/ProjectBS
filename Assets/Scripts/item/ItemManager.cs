using System;
using System.Linq;
using Effect;
using UnityEngine;

namespace Item
{
    /// <summary>
    /// 공용 Item 시스템 매니저.
    ///
    /// 현재는 Relic(Runtime) 관리 중심으로 구성되어 있으며,
    /// 이후 Equipment / Consumable / Currency 등으로 확장 가능하다.
    /// </summary>
    public class ItemManager : MonoBehaviour
    {
        public static ItemManager Instance { get; private set; }

        [Header("Runtime Data")]
        [SerializeField]
        private RelicRuntimeData relicRuntimeData = new();

        [SerializeField]
        private ConsumeRuntimeData consumeRuntimeData = new();

        [SerializeField]
        private AIFunctionRuntimeData aiFunctionRuntimeData = new();

        [Header("Debug")]
        [SerializeField]
        private bool logDebug;

        public RelicRuntimeData RelicRuntimeData => relicRuntimeData;

        public ConsumeRuntimeData ConsumeRuntimeData
            => consumeRuntimeData;

        public AIFunctionRuntimeData AIFunctionRuntimeData
            => aiFunctionRuntimeData;

        public event Action<RelicSO> OnRelicAdded;

        public event Action<RelicSO> OnRelicRemoved;

        public event Action<ConsumeSO> OnConsumeAdded;

        public event Action<ConsumeSO> OnConsumeRemoved;

        public event Action<AIFunctionSO> OnAIFunctionAdded;

        public event Action<AIFunctionSO> OnAIFunctionRemoved;

        private void Awake()
        {
            if (Instance != null
                && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (relicRuntimeData == null)
            {
                relicRuntimeData = new RelicRuntimeData();
            }

            if (consumeRuntimeData == null)
            {
                consumeRuntimeData =
                    new ConsumeRuntimeData();
            }

            if (aiFunctionRuntimeData == null)
            {
                aiFunctionRuntimeData =
                    new AIFunctionRuntimeData();
            }
        }

        public bool AddRelic(RelicSO relic)
        {
            if (relic == null)
            {
                return false;
            }

            bool added =
                relicRuntimeData.AddRelic(relic);

            if (!added)
            {
                return false;
            }

            RelicEntry entry =
                relicRuntimeData.FindRelic(relic);

            if (entry != null
                && entry.isEquipped)
            {
                ApplyRelicEffects(relic);
            }

            if (logDebug)
            {
                Debug.Log(
                    $"[ItemManager] Relic added. relic={relic.displayName}");
            }

            OnRelicAdded?.Invoke(relic);
            return true;
        }

        public bool RemoveRelic(RelicSO relic)
        {
            if (relic == null)
            {
                return false;
            }

            RemoveRelicEffects(relic);

            bool removed =
                relicRuntimeData.RemoveRelic(relic);

            if (!removed)
            {
                return false;
            }

            if (logDebug)
            {
                Debug.Log(
                    $"[ItemManager] Relic removed. relic={relic.displayName}");
            }

            OnRelicRemoved?.Invoke(relic);
            return true;
        }

        public bool HasRelic(RelicSO relic)
        {
            if (relic == null)
            {
                return false;
            }

            return relicRuntimeData.HasRelic(relic);
        }

        public bool EquipRelic(RelicSO relic)
        {
            if (relic == null)
            {
                return false;
            }

            RelicEntry entry =
                relicRuntimeData.FindRelic(relic);

            if (entry == null)
            {
                return false;
            }

            int equippedCount =
                relicRuntimeData.Relics
                    .Count(x => x != null
                        && x.isEquipped);

            if (equippedCount >= relicRuntimeData.MaxRelicCount)
            {
                return false;
            }

            if (entry.isEquipped)
            {
                return false;
            }

            entry.isEquipped = true;

            ApplyRelicEffects(relic);

            return true;
        }

        public bool UnequipRelic(RelicSO relic)
        {
            if (relic == null)
            {
                return false;
            }

            RelicEntry entry =
                relicRuntimeData.FindRelic(relic);

            if (entry == null)
            {
                return false;
            }

            if (!entry.isEquipped)
            {
                return false;
            }

            entry.isEquipped = false;

            RemoveRelicEffects(relic);

            return true;
        }

        private void ApplyRuntimeEffectToCharacters(
            Effect.EffectRuntimeData runtimeEffect)
        {
            if (runtimeEffect == null)
            {
                return;
            }

            EffectManager[] effectManagers =
                FindObjectsByType<EffectManager>(
                    FindObjectsSortMode.None);

            for (int i = 0;
                 i < effectManagers.Length;
                 i++)
            {
                EffectManager effectManager =
                    effectManagers[i];

                if (effectManager == null)
                {
                    continue;
                }

                effectManager.AddEffect(runtimeEffect);
            }
        }

        private void RemoveRuntimeEffectFromCharacters(
            Effect.EffectRuntimeData runtimeEffect)
        {
            if (runtimeEffect == null)
            {
                return;
            }

            EffectManager[] effectManagers =
                FindObjectsByType<EffectManager>(
                    FindObjectsSortMode.None);

            for (int i = 0;
                 i < effectManagers.Length;
                 i++)
            {
                EffectManager effectManager =
                    effectManagers[i];

                if (effectManager == null)
                {
                    continue;
                }

                effectManager.RemoveEffect(runtimeEffect);
            }
        }

        private void ApplyRelicEffects(RelicSO relic)
        {
            if (relic == null)
            {
                return;
            }

            RelicEntry entry =
                relicRuntimeData.FindRelic(relic);

            if (entry == null)
            {
                return;
            }

            if (!entry.isEquipped)
            {
                return;
            }

            foreach (EffectSO effect in relic.effects)
            {
                if (effect == null)
                {
                    continue;
                }

                if (effect is StatModifierEffectSO statModifierEffect)
                {
                    StatModifierEffectRuntime runtime =
                        new(
                            statModifierEffect,
                            EffectSourceType.Relic,
                            relic.relicId);

                    ApplyRuntimeEffectToCharacters(runtime);

                    entry.runtimeEffects.Add(runtime);
                }
            }
        }

        private void RemoveRelicEffects(RelicSO relic)
        {
            if (relic == null)
            {
                return;
            }

            RelicEntry entry =
                relicRuntimeData.FindRelic(relic);

            if (entry == null)
            {
                return;
            }

            foreach (Effect.EffectRuntimeData runtimeEffect
                in entry.runtimeEffects)
            {
                if (runtimeEffect == null)
                {
                    continue;
                }

                RemoveRuntimeEffectFromCharacters(runtimeEffect);
            }

            entry.runtimeEffects.Clear();
        }

        public bool AddConsume(ConsumeSO consume)
        {
            if (consume == null)
            {
                return false;
            }

            bool added =
                consumeRuntimeData.AddConsume(consume);

            if (!added)
            {
                return false;
            }

            if (logDebug)
            {
                Debug.Log(
                    $"[ItemManager] Consume added. consume={consume.displayName}");
            }

            OnConsumeAdded?.Invoke(consume);
            return true;
        }

        public bool RemoveConsume(ConsumeSO consume)
        {
            if (consume == null)
            {
                return false;
            }

            bool removed =
                consumeRuntimeData.RemoveConsume(consume);

            if (!removed)
            {
                return false;
            }

            if (logDebug)
            {
                Debug.Log(
                    $"[ItemManager] Consume removed. consume={consume.displayName}");
            }

            OnConsumeRemoved?.Invoke(consume);
            return true;
        }

        public bool HasConsume(ConsumeSO consume)
        {
            if (consume == null)
            {
                return false;
            }

            return consumeRuntimeData.HasConsume(consume);
        }

        public bool AddAIFunction(AIFunctionSO function)
        {
            if (function == null)
            {
                return false;
            }

            bool added =
                aiFunctionRuntimeData.AddFunction(function);

            if (!added)
            {
                return false;
            }

            if (logDebug)
            {
                Debug.Log(
                    $"[ItemManager] AI Function added. function={function.displayName}");
            }

            OnAIFunctionAdded?.Invoke(function);
            return true;
        }

        public bool RemoveAIFunction(AIFunctionSO function)
        {
            if (function == null)
            {
                return false;
            }

            bool removed =
                aiFunctionRuntimeData.RemoveFunction(function);

            if (!removed)
            {
                return false;
            }

            if (logDebug)
            {
                Debug.Log(
                    $"[ItemManager] AI Function removed. function={function.displayName}");
            }

            OnAIFunctionRemoved?.Invoke(function);
            return true;
        }

        public bool HasAIFunction(AIFunctionSO function)
        {
            if (function == null)
            {
                return false;
            }

            return aiFunctionRuntimeData.HasFunction(function);
        }

        public void ClearRelics()
        {
            relicRuntimeData.Clear();
            consumeRuntimeData.Clear();
            aiFunctionRuntimeData.Clear();

            if (logDebug)
            {
                Debug.Log(
                    "[ItemManager] All relics cleared.");
            }
        }
    }
}