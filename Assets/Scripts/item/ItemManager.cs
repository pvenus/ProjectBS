using System;
using System.Collections;
using System.Linq;
using Effect;
using Effect.Helper;
using UnityEngine;
using Session;
using Character;
using Item.Service;

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

        private RelicItemService relicService;
        private StrategicSkillItemService strategicSkillItemService;
        private AIFunctionItemService aiFunctionItemService;

        [Header("Debug")]
        [SerializeField]
        private bool logDebug;

        public RelicRuntimeData RelicRuntimeData => ResolveRelicRuntimeData();

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
            relicService = new RelicItemService();
            relicService.OnRelicAdded += HandleRelicAdded;
            relicService.OnRelicRemoved += HandleRelicRemoved;
            relicService.OnRelicEquipped += ApplyRelicEffects;
            relicService.OnRelicUnequipped += RemoveRelicEffects;

            strategicSkillItemService = new StrategicSkillItemService();
            strategicSkillItemService.OnStrategicSkillItemAdded += HandleStrategicSkillItemAdded;
            strategicSkillItemService.OnStrategicSkillItemRemoved += HandleStrategicSkillItemRemoved;

            aiFunctionItemService = new AIFunctionItemService();
            aiFunctionItemService.OnAIFunctionAdded += HandleAIFunctionAdded;
            aiFunctionItemService.OnAIFunctionRemoved += HandleAIFunctionRemoved;

            if (relicRuntimeData == null)
            {
                relicRuntimeData = new RelicRuntimeData();
            }
            relicRuntimeData =
                ResolveRelicRuntimeData();

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
                relicRuntimeData ?? ResolveRelicRuntimeData();

            PushRelicRuntimeDataToSession(this.relicRuntimeData);

            this.aiFunctionRuntimeData =
                aiFunctionRuntimeData ?? new AIFunctionRuntimeData();

            if (logDebug)
            {
                Debug.Log("[ItemManager] Runtime data initialized.");
            }
        }

        public bool AddRelic(RelicSO relic)
        {
            return relicService != null
                && relicService.Add(relic);
        }

        public bool RemoveRelic(RelicSO relic)
        {
            return relicService != null
                && relicService.Remove(relic);
        }

        public bool HasRelic(RelicSO relic)
        {
            return relic != null
                && relicService != null
                && relicService.IsOwned(relic.relicId);
        }

        public bool EquipRelic(RelicSO relic)
        {
            return relicService != null
                && relicService.Equip(relic);
        }

        public bool UnequipRelic(RelicSO relic)
        {
            return relicService != null
                && relicService.Unequip(relic);
        }

        private void ApplyEffectToCharacters(
            RelicEffectEntry effectEntry,
            EffectSourceType sourceType,
            string sourceId,
            RelicEntry relicEntry)
        {
            if (effectEntry == null || effectEntry.effect == null || relicEntry == null)
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
                    EffectResolveHelper.CreateRuntimeData(
                        effectEntry.effect,
                        sourceType,
                        sourceId,
                        targetCharacterManager);

                if (runtimeEffect == null)
                {
                    continue;
                }

                bool applied =
                    EffectApplyHelper.ApplyRuntimeData(
                        effectManager,
                        runtimeEffect,
                        effectEntry.lifetimeType,
                        effectEntry.duration,
                        effectEntry.categoryType);

                if (!applied)
                {
                    continue;
                }
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

        private CharacterManager ResolveCharacterManager(
            GameObject gameObject)
        {
            if (gameObject == null)
            {
                return null;
            }

            CharacterManager characterManager =
                gameObject.GetComponent<CharacterManager>();

            if (characterManager != null)
            {
                return characterManager;
            }

            characterManager =
                gameObject.GetComponentInParent<CharacterManager>();

            if (characterManager != null)
            {
                return characterManager;
            }

            return gameObject.GetComponentInChildren<CharacterManager>();
        }

        private bool ShouldApplyRelicEffectOnEquip(
            RelicEffectApplyType applyType)
        {
            return applyType == RelicEffectApplyType.OnEquip
                || applyType == RelicEffectApplyType.OnAttack;
        }

        private void ApplyRelicEffects(RelicSO relic)
        {
            if (relic == null)
            {
                return;
            }
            RelicEntry entry =
                ResolveRelicRuntimeData()
                    .FindRelic(relic);

            if (entry == null)
            {
                return;
            }

            if (!entry.isEquipped)
            {
                return;
            }

            foreach (RelicEffectEntry effectEntry in relic.effects)
            {
                if (effectEntry == null
                    || effectEntry.effect == null
                    || !ShouldApplyRelicEffectOnEquip(effectEntry.applyType))
                {
                    continue;
                }

                ApplyEffectToCharacters(
                    effectEntry,
                    EffectSourceType.Relic,
                    relic.relicId,
                    entry);
            }
        }

        private void RemoveRelicEffects(RelicSO relic)
        {
            if (relic == null || relic.effects == null)
            {
                return;
            }

            RelicEntry entry =
                ResolveRelicRuntimeData()
                    .FindRelic(relic);

            if (entry == null)
            {
                return;
            }

            foreach (RelicEffectEntry effectEntry in relic.effects)
            {
                if (effectEntry == null
                    || effectEntry.effect == null
                    || !ShouldApplyRelicEffectOnEquip(effectEntry.applyType))
                {
                    continue;
                }

                RemoveRelicEffectFromCharacters(
                    effectEntry,
                    EffectSourceType.Relic,
                    relic.relicId);
            }
        }

        private void RemoveRelicEffectFromCharacters(
            RelicEffectEntry effectEntry,
            EffectSourceType sourceType,
            string sourceId)
        {
            if (effectEntry == null || effectEntry.effect == null)
            {
                return;
            }

            EffectManager[] effectManagers =
                FindObjectsByType<EffectManager>(
                    FindObjectsSortMode.None);

            for (int i = 0; i < effectManagers.Length; i++)
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
                    EffectResolveHelper.CreateRuntimeData(
                        effectEntry.effect,
                        sourceType,
                        sourceId,
                        targetCharacterManager);

                if (runtimeEffect == null)
                {
                    continue;
                }

                RemoveRuntimeEffectFromCharacters(runtimeEffect);
            }
        }

        private void HandleRelicAdded(RelicSO relic)
        {
            if (logDebug)
            {
                Debug.Log($"[ItemManager] Relic added. relic={relic.DisplayName}");
            }

            OnRelicAdded?.Invoke(relic);
        }

        private void HandleRelicRemoved(RelicSO relic)
        {
            if (logDebug)
            {
                Debug.Log($"[ItemManager] Relic removed. relic={relic.DisplayName}");
            }

            OnRelicRemoved?.Invoke(relic);
        }

        private RelicRuntimeData ResolveRelicRuntimeData()
        {
            if (GameSession.Instance != null
                && GameSession.Instance.StageSession != null)
            {
                if (GameSession.Instance.StageSession.RelicRuntimeData == null)
                {
                    GameSession.Instance.StageSession.RelicRuntimeData =
                        relicRuntimeData ?? new RelicRuntimeData();
                }

                relicRuntimeData =
                    GameSession.Instance.StageSession.RelicRuntimeData;

                return relicRuntimeData;
            }

            if (relicRuntimeData == null)
            {
                relicRuntimeData = new RelicRuntimeData();
            }

            return relicRuntimeData;
        }

        private void PushRelicRuntimeDataToSession(
            RelicRuntimeData runtimeData)
        {
            if (runtimeData == null
                || GameSession.Instance == null
                || GameSession.Instance.StageSession == null)
            {
                return;
            }

            GameSession.Instance.StageSession.RelicRuntimeData = runtimeData;
        }

        private StrategicSkillItemRuntimeData ResolveStrategicSkillItemRuntimeData()
        {
            return GameSession.Instance.StageSession.StrategicSkillItemRuntimeData;
        }

        public bool AddStrategicSkillItem(StrategicSkillItemSO strategicSkillItem)
        {
            return strategicSkillItemService != null
                && strategicSkillItemService.Add(strategicSkillItem);
        }

        public bool RemoveStrategicSkillItem(StrategicSkillItemSO strategicSkillItem)
        {
            return strategicSkillItemService != null
                && strategicSkillItemService.Remove(strategicSkillItem);
        }

        public bool HasStrategicSkillItem(StrategicSkillItemSO strategicSkillItem)
        {
            return strategicSkillItem != null
                && strategicSkillItemService != null
                && strategicSkillItemService.Has(strategicSkillItem);
        }

        public bool TryUseStrategicSkillItemFromScreenPosition(
            StrategicSkillItemSO strategicSkillItem,
            Vector2 screenPosition,
            Camera worldCamera,
            bool logDebugOverride = false,
            UnityEngine.Object logContext = null)
        {
            strategicSkillItemService ??= new StrategicSkillItemService();

            return strategicSkillItemService.TryUseFromScreenPosition(
                strategicSkillItem,
                screenPosition,
                worldCamera,
                logDebug || logDebugOverride,
                logContext != null ? logContext : this);
        }

        private void HandleStrategicSkillItemAdded(
            StrategicSkillItemSO strategicSkillItem)
        {
            if (logDebug)
            {
                Debug.Log(
                    $"[ItemManager] Strategic skill item added. item={strategicSkillItem.DisplayName}");
            }

            OnStrategicSkillItemAdded?.Invoke(strategicSkillItem);
        }

        private void HandleStrategicSkillItemRemoved(
            StrategicSkillItemSO strategicSkillItem)
        {
            if (logDebug)
            {
                Debug.Log(
                    $"[ItemManager] Strategic skill item removed. item={strategicSkillItem.DisplayName}");
            }

            OnStrategicSkillItemRemoved?.Invoke(strategicSkillItem);
        }

        public bool AddAIFunction(AIFunctionSO function)
        {
            return aiFunctionItemService != null
                && aiFunctionItemService.Add(function);
        }

        public bool RemoveAIFunction(AIFunctionSO function)
        {
            return aiFunctionItemService != null
                && aiFunctionItemService.Remove(function);
        }

        public bool HasAIFunction(AIFunctionSO function)
        {
            return function != null
                && aiFunctionItemService != null
                && aiFunctionItemService.Has(function);
        }

        public void ClearRelics()
        {
            ResolveRelicRuntimeData().Clear();
            ResolveStrategicSkillItemRuntimeData().Clear();
            aiFunctionRuntimeData.Clear();

            if (logDebug)
            {
                Debug.Log(
                    "[ItemManager] All relics cleared.");
            }
        }

        private void HandleAIFunctionAdded(AIFunctionSO function)
        {
            if (logDebug)
            {
                Debug.Log(
                    $"[ItemManager] AI Function added. function={function.displayName}");
            }

            OnAIFunctionAdded?.Invoke(function);
        }

        private void HandleAIFunctionRemoved(AIFunctionSO function)
        {
            if (logDebug)
            {
                Debug.Log(
                    $"[ItemManager] AI Function removed. function={function.displayName}");
            }

            OnAIFunctionRemoved?.Invoke(function);
        }
    }
}