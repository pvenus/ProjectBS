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
            return AddRelic(relic, null);
        }

        public bool AddRelic(
            RelicSO relic,
            CharacterManager ownerCharacter)
        {
            if (relic == null || relicService == null)
            {
                return false;
            }

            bool added =
                relicService.Add(relic);

            if (!added)
            {
                return false;
            }

            RelicRuntimeData runtimeData =
                ResolveRelicRuntimeData();

            if (!runtimeData.HasRelic(relic))
            {
                runtimeData.AddRelic(relic);
            }

            RelicEntry entry =
                runtimeData.FindRelic(relic);

            entry?.SetOwner(ownerCharacter);

            if (entry != null
                && entry.isEquipped
                && ownerCharacter != null)
            {
                ApplyRelicEffects(relic);
            }

            return true;
        }

        public bool RemoveRelic(RelicSO relic)
        {
            bool removed =
                relicService != null
                && relicService.Remove(relic);

            if (removed)
            {
                ResolveRelicRuntimeData()
                    .RemoveRelic(relic);
            }

            return removed;
        }

        public bool HasRelic(RelicSO relic)
        {
            return relic != null
                && relicService != null
                && relicService.IsOwned(relic.relicId);
        }

        public bool EquipRelic(RelicSO relic)
        {
            return EquipRelic(relic, null);
        }

        public bool EquipRelic(
            RelicSO relic,
            CharacterManager ownerCharacter)
        {
            if (relicService == null
                || !relicService.Equip(relic))
            {
                return false;
            }

            RelicEntry entry =
                ResolveRelicRuntimeData()
                    .FindRelic(relic);

            entry?.SetOwner(ownerCharacter);

            if (ownerCharacter == null && logDebug)
            {
                Debug.LogWarning(
                    $"[ItemManager] Relic equipped without owner. Runtime effects are blocked until an owner is supplied. relic={relic.relicId}",
                    this);
            }

            ApplyRelicEffects(relic);
            return true;
        }

        public bool UnequipRelic(RelicSO relic)
        {
            return relicService != null
                && relicService.Unequip(relic);
        }

        private void ApplyEffectToOwner(
            EffectEntrySO effectEntry,
            RelicEntry relicEntry)
        {
            if (effectEntry == null || effectEntry.EffectSO == null || relicEntry == null)
            {
                return;
            }

            CharacterManager ownerCharacter =
                relicEntry.ownerCharacter;

            if (ownerCharacter == null)
            {
                Debug.LogWarning(
                    $"[ItemManager] Relic effect skipped because no owner is recorded. relic={relicEntry.relic?.relicId}, effect={effectEntry.EffectSO.EffectId}",
                    this);
                return;
            }

            EffectManager effectManager =
                ResolveEffectManager(ownerCharacter);

            if (effectManager == null)
            {
                Debug.LogWarning(
                    $"[ItemManager] Relic owner has no EffectManager. relic={relicEntry.relic?.relicId}, owner={ownerCharacter.name}",
                    ownerCharacter);
                return;
            }

            EffectEntryRuntime runtimeEntry =
                EffectResolveHelper.CreateRuntimeEntry(
                    effectEntry,
                    ownerCharacter,
                    ownerCharacter,
                    ownerCharacter.transform);

            if (runtimeEntry?.RuntimeData == null)
            {
                return;
            }

            EffectApplyHelper.ApplyEffect(
                effectManager,
                runtimeEntry);
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

        private EffectManager ResolveEffectManager(
            CharacterManager characterManager)
        {
            if (characterManager == null)
            {
                return null;
            }

            EffectManager effectManager =
                characterManager.GetComponent<EffectManager>();

            if (effectManager != null)
            {
                return effectManager;
            }

            effectManager =
                characterManager.GetComponentInChildren<EffectManager>();

            if (effectManager != null)
            {
                return effectManager;
            }

            return characterManager.GetComponentInParent<EffectManager>();
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

            foreach (EffectEntrySO effectEntry in relic.effectEntries)
            {
                if (effectEntry == null || effectEntry.EffectSO == null)
                {
                    continue;
                }
                ApplyEffectToOwner(effectEntry, entry);
            }
        }

        private void RemoveRelicEffects(RelicSO relic)
        {
            if (relic == null || relic.effectEntries == null)
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

            foreach (EffectEntrySO effectEntry in relic.effectEntries)
            {
                if (effectEntry == null || effectEntry.EffectSO == null)
                {
                    continue;
                }
                RemoveRelicEffectFromOwner(effectEntry, entry);
            }
        }

        private void RemoveRelicEffectFromOwner(
            EffectEntrySO effectEntry,
            RelicEntry relicEntry)
        {
            if (effectEntry == null
                || effectEntry.EffectSO == null
                || relicEntry == null
                || relicEntry.ownerCharacter == null)
            {
                return;
            }

            EffectManager effectManager =
                ResolveEffectManager(relicEntry.ownerCharacter);

            if (effectManager == null)
            {
                return;
            }

            EffectEntryRuntime runtimeEntry =
                EffectResolveHelper.CreateRuntimeEntry(
                    effectEntry,
                    relicEntry.ownerCharacter,
                    relicEntry.ownerCharacter,
                    relicEntry.ownerCharacter.transform);

            effectManager.RemoveEffectsBySource(
                runtimeEntry?.RuntimeData?.RuntimeId);
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
