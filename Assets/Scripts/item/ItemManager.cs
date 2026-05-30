using System;
using System.Collections;
using System.Linq;
using Effect;
using Effect.Helper;
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

        private void OnEnable()
        {
            CharacterManager.OnAnyDamageApplied += HandleAnyDamageApplied;
        }

        private void OnDisable()
        {
            CharacterManager.OnAnyDamageApplied -= HandleAnyDamageApplied;
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
            if (relic == null)
            {
                return false;
            }
            RelicRuntimeData runtimeData =
                ResolveRelicRuntimeData();

            bool added =
                runtimeData.AddRelic(relic);

            if (!added)
            {
                return false;
            }

            RelicEntry entry =
                runtimeData.FindRelic(relic);

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
            RelicRuntimeData runtimeData =
                ResolveRelicRuntimeData();

            bool removed =
                runtimeData.RemoveRelic(relic);

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
            return ResolveRelicRuntimeData()
                .HasRelic(relic);
        }

        public bool EquipRelic(RelicSO relic)
        {
            if (relic == null)
            {
                return false;
            }
            RelicRuntimeData runtimeData =
                ResolveRelicRuntimeData();

            RelicEntry entry =
                runtimeData.FindRelic(relic);

            if (entry == null)
            {
                return false;
            }

            int equippedCount =
                runtimeData.Relics
                    .Count(x => x != null
                        && x.isEquipped);

            if (equippedCount >= runtimeData.MaxRelicCount)
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
            RelicRuntimeData runtimeData =
                ResolveRelicRuntimeData();

            RelicEntry entry =
                runtimeData.FindRelic(relic);

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


        private void HandleAnyDamageApplied(
            CharacterDamageRequest request,
            CharacterDamageResult result)
        {
            if (request == null
                || result == null
                || request.attacker == null
                || request.target == null)
            {
                return;
            }

            CharacterManager attackerCharacterManager =
                ResolveCharacterManager(request.attacker);

            if (attackerCharacterManager == null
                || attackerCharacterManager.RuntimeData == null
                || attackerCharacterManager.RuntimeData.characterSO == null
                || attackerCharacterManager.RuntimeData.characterSO.characterType != CharacterType.Player)
            {
                return;
            }
            
            ApplyEquippedRelicEffectsOnDamage(
                request,
                result);
        }

        private void ApplyEquippedRelicEffectsOnDamage(
            CharacterDamageRequest request,
            CharacterDamageResult result)
        {
            RelicRuntimeData runtimeData =
                ResolveRelicRuntimeData();
            if (runtimeData == null
                || runtimeData.Relics == null
                || runtimeData.Relics.Count == 0)
            {
                return;
            }

            EffectManager targetEffectManager =
                request.target.GetComponent<EffectManager>();

            if (targetEffectManager == null)
            {
                targetEffectManager =
                    request.target.GetComponentInChildren<EffectManager>();
            }

            if (targetEffectManager == null)
            {
                return;
            }

            for (int i = 0; i < runtimeData.Relics.Count; i++)
            {
                RelicEntry relicEntry =
                    runtimeData.Relics[i];

                if (relicEntry == null
                    || !relicEntry.isEquipped
                    || relicEntry.relic == null
                    || relicEntry.relic.effects == null)
                {
                    continue;
                }

                ApplyRelicEffectsOnDamageToTarget(
                    relicEntry,
                    request,
                    result,
                    targetEffectManager);
            }
        }

        private void ApplyRelicEffectsOnDamageToTarget(
            RelicEntry relicEntry,
            CharacterDamageRequest request,
            CharacterDamageResult result,
            EffectManager targetEffectManager)
        {
            if (relicEntry == null
                || relicEntry.relic == null
                || request == null
                || result == null
                || targetEffectManager == null)
            {
                return;
            }

            for (int i = 0; i < relicEntry.relic.effects.Count; i++)
            {
                RelicEffectEntry effectEntry =
                    relicEntry.relic.effects[i];

                if (effectEntry == null
                    || effectEntry.effect == null
                    || effectEntry.applyType != RelicEffectApplyType.OnAttack)
                {
                    continue;
                }

                CharacterManager targetCharacterManager =
                    ResolveCharacterManager(request.target);

                if (targetCharacterManager == null)
                {
                    continue;
                }

                CharacterManager sourceCharacterManager =
                    ResolveCharacterManager(request.attacker);

                EffectApplyHelper.ApplyEffect(
                    targetEffectManager,
                    targetCharacterManager,
                    effectEntry.effect,
                    EffectSourceType.Relic,
                    relicEntry.relic.relicId,
                    effectEntry.lifetimeType,
                    effectEntry.duration,
                    effectEntry.categoryType,
                    request.attacker.transform,
                    Vector2.zero,
                    sourceCharacterManager);
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
                    || effectEntry.applyType != RelicEffectApplyType.OnEquip)
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
                    || effectEntry.applyType != RelicEffectApplyType.OnEquip)
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
            ResolveRelicRuntimeData().Clear();
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