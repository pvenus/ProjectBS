using System.Collections.Generic;
using System.Linq;
using Shrine;
using UnityEngine;
using Effect;
using Effect.Helper;
using Character;
using Session;

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
            ResolveBlessRuntimeData().GetBlessings();
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
                ResolveBlessRuntimeData().RemoveBlesses(
                    x => x != null
                         && x.source != null
                         && x.source.GodType == ShrineGodType.None
                         && !x.isTemporary);
            }

            ResolveBlessRuntimeData().AddBless(
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
                ResolveBlessRuntimeData().GetBlessings()
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

            ResolveBlessRuntimeData().RemoveBlesses(match);
        }

        public void ConsumeBattleBlessings()
        {
            ResolveBlessRuntimeData().ConsumeBattleBlessings();
        }

        private void Initialize()
        {
            // 세션이 있으면 세션 데이터를 참조한다 (씬 재진입 시 초기화 방지).
            // 세션이 없을 때만 로컬 runtimeData를 사용한다.
            runtimeData = ResolveBlessRuntimeData();
        }

        public void ResetRuntime()
        {
            // 새 게임 시작이나 명시적 세션 초기화 시에만 호출한다.
            runtimeData = new BlessRuntimeData();
            PushBlessRuntimeDataToSession(runtimeData);
        }

        /// <summary>
        /// StageSession이 있으면 세션의 BlessRuntimeData를 반환한다.
        /// 없으면 로컬 runtimeData를 반환한다.
        /// ItemManager.ResolveRelicRuntimeData()와 동일한 패턴.
        /// </summary>
        private BlessRuntimeData ResolveBlessRuntimeData()
        {
            if (GameSession.Instance != null
                && GameSession.Instance.StageSession != null)
            {
                if (GameSession.Instance.StageSession.BlessRuntimeData == null)
                {
                    GameSession.Instance.StageSession.BlessRuntimeData =
                        runtimeData ?? new BlessRuntimeData();
                }

                runtimeData = GameSession.Instance.StageSession.BlessRuntimeData;
                return runtimeData;
            }

            if (runtimeData == null)
            {
                runtimeData = new BlessRuntimeData();
            }

            return runtimeData;
        }

        private void PushBlessRuntimeDataToSession(BlessRuntimeData data)
        {
            if (data == null
                || GameSession.Instance == null
                || GameSession.Instance.StageSession == null)
            {
                return;
            }

            GameSession.Instance.StageSession.BlessRuntimeData = data;
        }
    }
}