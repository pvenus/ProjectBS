using System;
using System.Linq;
using Effect;
using UnityEngine;
using Session;
using Character;

namespace Item
{
    /// <summary>
    /// 공용 Item 시스템 매니저.
    ///
    /// 현재는 Relic(Runtime) 관리 중심으로 구성되어 있으며,
    /// 이후 Equipment / Strategic Skill Item / Currency 등으로 확장 가능하다.
    /// </summary>
    public class ItemManager : MonoBehaviour
    {
        public static ItemManager Instance { get; private set; }

        [Header("Runtime Data")]
        [SerializeField]
        private RelicRuntimeData relicRuntimeData = new();

        [SerializeField]
        private AIFunctionRuntimeData aiFunctionRuntimeData = new();

        [Header("Debug")]
        [SerializeField]
        private bool logDebug;

        public RelicRuntimeData RelicRuntimeData => relicRuntimeData;

        public StrategicSkillItemRuntimeData StrategicSkillItemRuntimeData
            => ResolveStrategicSkillItemRuntimeData();

        public AIFunctionRuntimeData AIFunctionRuntimeData
            => aiFunctionRuntimeData;

        public event Action<RelicSO> OnRelicAdded;

        public event Action<RelicSO> OnRelicRemoved;

        public event Action<StrategicSkillItemSO> OnStrategicSkillItemAdded;

        public event Action<StrategicSkillItemSO> OnStrategicSkillItemRemoved;

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

            if (aiFunctionRuntimeData == null)
            {
                aiFunctionRuntimeData =
                    new AIFunctionRuntimeData();
            }
        }


        public void InitializeRuntimeData(
            RelicRuntimeData relicRuntimeData,
            AIFunctionRuntimeData aiFunctionRuntimeData)
        {
            this.relicRuntimeData =
                relicRuntimeData ?? new RelicRuntimeData();

            this.aiFunctionRuntimeData =
                aiFunctionRuntimeData ?? new AIFunctionRuntimeData();

            if (logDebug)
            {
                Debug.Log("[ItemManager] Runtime data initialized.");
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

        private void ApplyEffectToCharacters(
            EffectSO effect,
            EffectSourceType sourceType,
            string sourceId,
            RelicEntry relicEntry)
        {
            if (effect == null || relicEntry == null)
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

                CharacterManager targetCharacterManager =
                    ResolveCharacterManager(effectManager);

                Effect.EffectRuntimeData runtimeEffect =
                    CreateRuntimeEffect(
                        effect,
                        sourceType,
                        sourceId,
                        targetCharacterManager);

                if (runtimeEffect == null)
                {
                    continue;
                }

                effectManager.AddEffect(runtimeEffect);
                relicEntry.runtimeEffects.Add(runtimeEffect);
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

        private Effect.EffectRuntimeData CreateRuntimeEffect(
            EffectSO effect,
            EffectSourceType sourceType,
            string sourceId,
            CharacterManager targetCharacterManager)
        {
            if (effect == null)
            {
                return null;
            }

            if (effect is StatModifierEffectSO statModifierEffect)
            {
                if (targetCharacterManager == null)
                {
                    return null;
                }

                return new StatModifierEffectRuntime(
                    statModifierEffect,
                    sourceType,
                    sourceId,
                    targetCharacterManager);
            }

            return null;
        }

        private CharacterManager ResolveCharacterManager(
            EffectManager effectManager)
        {
            if (effectManager == null)
            {
                return null;
            }

            CharacterManager characterManager =
                effectManager.GetComponent<CharacterManager>();

            if (characterManager != null)
            {
                return characterManager;
            }

            characterManager =
                effectManager.GetComponentInParent<CharacterManager>();

            if (characterManager != null)
            {
                return characterManager;
            }

            return effectManager.GetComponentInChildren<CharacterManager>();
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

                ApplyEffectToCharacters(
                    effect,
                    EffectSourceType.Relic,
                    relic.relicId,
                    entry);
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

        private StrategicSkillItemRuntimeData ResolveStrategicSkillItemRuntimeData()
        {
            return GameSession.Instance.StageSession.StrategicSkillItemRuntimeData;
        }

        public bool AddStrategicSkillItem(StrategicSkillItemSO strategicSkillItem)
        {
            if (strategicSkillItem == null)
            {
                return false;
            }

            StrategicSkillItemRuntimeData runtimeData =
                ResolveStrategicSkillItemRuntimeData();

            bool added =
                runtimeData.AddStrategicSkillItem(strategicSkillItem);

            if (!added)
            {
                return false;
            }

            if (logDebug)
            {
                Debug.Log(
                    $"[ItemManager] Strategic skill item added. item={strategicSkillItem.displayName}");
            }

            OnStrategicSkillItemAdded?.Invoke(strategicSkillItem);
            return true;
        }

        public bool RemoveStrategicSkillItem(StrategicSkillItemSO strategicSkillItem)
        {
            if (strategicSkillItem == null)
            {
                return false;
            }

            StrategicSkillItemRuntimeData runtimeData =
                ResolveStrategicSkillItemRuntimeData();

            bool removed =
                runtimeData.RemoveStrategicSkillItem(strategicSkillItem);

            if (!removed)
            {
                return false;
            }

            if (logDebug)
            {
                Debug.Log(
                    $"[ItemManager] Strategic skill item removed. item={strategicSkillItem.displayName}");
            }

            OnStrategicSkillItemRemoved?.Invoke(strategicSkillItem);
            return true;
        }

        public bool HasStrategicSkillItem(StrategicSkillItemSO strategicSkillItem)
        {
            if (strategicSkillItem == null)
            {
                return false;
            }

            return ResolveStrategicSkillItemRuntimeData()
                .HasStrategicSkillItem(strategicSkillItem);
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
            ResolveStrategicSkillItemRuntimeData().Clear();
            aiFunctionRuntimeData.Clear();

            if (logDebug)
            {
                Debug.Log(
                    "[ItemManager] All relics cleared.");
            }
        }
    }
}