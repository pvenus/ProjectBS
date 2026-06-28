using System.Collections.Generic;
using System.Linq;
using Shrine;
using UnityEngine;
using Effect;
using Effect.Helper;
using Character;

namespace Bless
{
    public class BlessManager : MonoBehaviour
    {
        public static BlessManager Instance { get; private set; }

        [Header("Config")]
        [SerializeField]
        private BlessConfigSO config;

        [Header("Runtime")]
        [SerializeField]
        private BlessRuntimeData runtimeData = new();

        public IReadOnlyList<BlessRuntimeData.BlessEntry> Blessings =>
            runtimeData.GetBlessings();
        public BlessPoolSO CommonPool =>
            config != null
                ? config.CommonPool
                : null;

        private void Awake()
        {
            if (Instance != null
                && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            Initialize();
        }

        public List<BlessSO> GenerateBlessings(
            ShrineGodType godType,
            int progressionStep)
        {
            List<BlessSO> result = new();

            AddCommonBlessings(
                result,
                progressionStep);

            return result;
        }

        private void AddCommonBlessings(
            List<BlessSO> result,
            int progressionStep)
        {
            BlessPoolSO commonPool =
                config != null
                    ? config.CommonPool
                    : null;

            if (commonPool == null)
            {
                return;
            }

            int commonBlessingCount =
                config != null
                    ? config.CommonBlessingCount
                    : 1;

            for (int i = 0;
                 i < commonBlessingCount;
                 i++)
            {
                BlessSO blessing =
                    commonPool.GetRandomBlessing(
                        ShrineGodType.None,
                        progressionStep,
                        result);

                if (blessing == null)
                {
                    continue;
                }

                result.RemoveAll(x => x != null
                    && x.GodType == ShrineGodType.None);

                result.Add(blessing);
            }
        }

        public void AddBless(
            BlessSO source,
            string generatedFromPoolId = null,
            int slotIndex = -1)
        {
            if (source == null)
            {
                return;
            }

            if (source.GodType == ShrineGodType.None
                && source.DurationType == BlessDurationType.Permanent)
            {
                runtimeData.RemoveBlesses(
                    x => x != null
                         && x.source != null
                         && x.source.GodType == ShrineGodType.None
                         && !x.isTemporary);
            }

            runtimeData.AddBless(
                source,
                generatedFromPoolId,
                slotIndex);

            if (source.EffectEntries == null)
            {
                return;
            }

            foreach (EffectEntrySO effectEntry in source.EffectEntries)
            {
                if (effectEntry == null)
                {
                    continue;
                }

                AddEffectToEffectManagers(effectEntry);
            }
        }

        private void AddEffectToEffectManagers(
            EffectEntrySO effectEntry)
        {
            if (effectEntry == null)
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

                if (targetCharacterManager == null)
                {
                    continue;
                }

                EffectEntryRuntime runtimeEntry =
                    EffectResolveHelper.CreateRuntimeEntry(
                        effectEntry,
                        targetCharacterManager,
                        null,
                        ResolveEffectSourceTransform(effectEntry.EffectSO, targetCharacterManager),
                        Vector2.zero);

                EffectApplyHelper.ApplyEffect(
                    effectManager,
                    runtimeEntry);
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

        private Transform ResolveEffectSourceTransform(
            EffectSO effect,
            CharacterManager targetCharacterManager)
        {
            if (effect != null
                && effect.Config is KnockbackEffectConfig)
            {
                return targetCharacterManager != null
                    ? targetCharacterManager.transform
                    : null;
            }

            return null;
        }

        private void RemoveEffectsFromEffectManagers(
            BlessSO source)
        {
            if (source == null || source.EffectEntries == null)
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

                if (targetCharacterManager == null)
                {
                    continue;
                }

                foreach (EffectEntrySO effectEntry in source.EffectEntries)
                {
                    if (effectEntry == null)
                    {
                        continue;
                    }

                    EffectEntryRuntime runtimeEntry =
                        EffectResolveHelper.CreateRuntimeEntry(
                            effectEntry,
                            targetCharacterManager,
                            null,
                            ResolveEffectSourceTransform(effectEntry.EffectSO, targetCharacterManager),
                            Vector2.zero);

                    if (runtimeEntry?.RuntimeData == null
                        || string.IsNullOrWhiteSpace(runtimeEntry.RuntimeData.RuntimeId))
                    {
                        continue;
                    }

                    effectManager.RemoveEffectsBySource(
                        runtimeEntry.RuntimeData.RuntimeId);
                }
            }
        }

        public void RemoveBlesses(
            System.Predicate<BlessRuntimeData.BlessEntry> match)
        {
            List<BlessRuntimeData.BlessEntry> targets =
                runtimeData.GetBlessings()
                    .Where(x => x != null && match(x))
                    .ToList();

            foreach (BlessRuntimeData.BlessEntry entry in targets)
            {
                if (entry == null || entry.source == null)
                {
                    continue;
                }

                RemoveEffectsFromEffectManagers(entry.source);
            }

            runtimeData.RemoveBlesses(match);
        }

        public void ConsumeBattleBlessings()
        {
            runtimeData.ConsumeBattleBlessings();
        }

        private void Initialize()
        {
            runtimeData = new BlessRuntimeData();
        }

        public void ResetRuntime()
        {
            Initialize();
        }
    }
}