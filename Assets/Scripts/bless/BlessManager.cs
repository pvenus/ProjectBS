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
                ? config.commonPool
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
                    ? config.commonPool
                    : null;

            if (commonPool == null)
            {
                return;
            }

            int commonBlessingCount =
                config != null
                    ? config.commonBlessingCount
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
                    && x.godType == ShrineGodType.None);

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

            if (source.godType == ShrineGodType.None
                && source.durationType == BlessDurationType.Permanent)
            {
                runtimeData.RemoveBlesses(
                    x => x != null
                         && x.source != null
                         && x.source.godType == ShrineGodType.None
                         && !x.isTemporary);
            }

            runtimeData.AddBless(
                source,
                generatedFromPoolId,
                slotIndex);

            if (source.effects == null)
            {
                return;
            }

            foreach (EffectSO effect in source.effects)
            {
                if (effect == null)
                {
                    continue;
                }

                AddEffectToEffectManagers(
                    effect,
                    EffectSourceType.Bless,
                    source.blessingId);
            }
        }

        private void AddEffectToEffectManagers(
            EffectSO effect,
            EffectSourceType sourceType,
            string sourceId)
        {
            if (effect == null)
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

                EffectApplyHelper.ApplyEffect(
                    effectManager,
                    targetCharacterManager,
                    effect,
                    sourceType,
                    sourceId,
                    EffectLifetimeType.Manual,
                    -1f,
                    EffectCategoryType.Buff,
                    ResolveEffectSourceTransform(effect, targetCharacterManager),
                    Vector2.zero);
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
            if (effect is KnockbackEffectSO)
            {
                return targetCharacterManager != null
                    ? targetCharacterManager.transform
                    : null;
            }

            return null;
        }

        private void RemoveEffectsFromEffectManagers(
            EffectSourceType sourceType,
            string sourceId)
        {
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

                effectManager.RemoveEffectsBySource(
                    sourceType,
                    sourceId);
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

                RemoveEffectsFromEffectManagers(
                    EffectSourceType.Bless,
                    entry.source.blessingId);
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