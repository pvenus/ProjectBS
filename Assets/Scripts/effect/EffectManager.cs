using System.Collections.Generic;
using UnityEngine;
using Character;

namespace Effect
{

    [System.Serializable]
    public class EffectLifetimeData
    {
        public string runtimeId;
        public EffectLifetimeType lifetimeType;
        public EffectCategoryType categoryType;
        public float remainingTime;

        public EffectLifetimeData(
            string runtimeId,
            EffectLifetimeType lifetimeType,
            float duration,
            EffectCategoryType categoryType)
        {
            this.runtimeId = runtimeId;
            this.lifetimeType = lifetimeType;
            this.categoryType = categoryType;
            remainingTime = duration;
        }
    }

    public class EffectManager : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool logDebug = true;

        [Header("Runtime Effects")]
        [SerializeField]
        private List<EffectRuntimeData> activeEffects = new();

        [SerializeField]
        private List<EffectLifetimeData> activeEffectLifetimes = new();

        private bool isBattleActive;

        public event System.Action<EffectRuntimeData> OnEffectAdded;
        public event System.Action<EffectRuntimeData> OnEffectRemoved;

        private void Awake()
        {
        }

        private void OnEnable()
        {
            CharacterManager.OnAnyDamageApplied += HandleAnyDamageApplied;
            CharacterManager.OnAnyHealed += HandleAnyHealed;
        }

        private void OnDisable()
        {
            CharacterManager.OnAnyDamageApplied -= HandleAnyDamageApplied;
            CharacterManager.OnAnyHealed -= HandleAnyHealed;
        }

        public IReadOnlyList<EffectRuntimeData> ActiveEffects
            => activeEffects;
        public void AddEffect(
            EffectRuntimeData runtimeData,
            EffectEntrySO effectEntry)
        {
            if (effectEntry == null)
            {
                return;
            }

            AddEffect(
                runtimeData,
                effectEntry.LifetimeType,
                effectEntry.Duration,
                effectEntry.CategoryType);
        }

        public void AddEffect(
            EffectRuntimeData runtimeData,
            EffectLifetimeType lifetimeType,
            float duration,
            EffectCategoryType categoryType)
        {
            if (runtimeData == null)
            {
                return;
            }

            if (lifetimeType == EffectLifetimeType.Instant)
            {
                runtimeData.IsActive = true;
                runtimeData.OnApply();

                if (!runtimeData.IsActive)
                {
                    return;
                }

                OnEffectAdded?.Invoke(runtimeData);

                if (logDebug)
                {
                    Debug.Log(
                        $"[EffectManager] Instant effect applied. id={runtimeData.RuntimeId}, category={categoryType}");
                }

                return;
            }

            RemoveEffectsByRuntimeId(runtimeData.RuntimeId);

            activeEffects.Add(runtimeData);

            activeEffectLifetimes.Add(
                new EffectLifetimeData(
                    runtimeData.RuntimeId,
                    lifetimeType,
                    duration,
                    categoryType));

            runtimeData.IsActive = true;
            runtimeData.OnApply();

            if (!runtimeData.IsActive)
            {
                activeEffects.Remove(runtimeData);
                RemoveLifetime(runtimeData.RuntimeId);
                return;
            }

            OnEffectAdded?.Invoke(runtimeData);

            if (logDebug)
            {
                Debug.Log(
                    $"[EffectManager] Effect added. id={runtimeData.RuntimeId}, category={categoryType}, lifetime={lifetimeType}, duration={duration}");
            }
        }
        private void RemoveEffectsByRuntimeId(string runtimeId)
        {
            if (string.IsNullOrEmpty(runtimeId))
            {
                return;
            }

            for (int i = activeEffects.Count - 1;
                 i >= 0;
                 i--)
            {
                EffectRuntimeData activeEffect =
                    activeEffects[i];

                if (activeEffect == null)
                {
                    activeEffects.RemoveAt(i);
                    continue;
                }

                if (!string.Equals(
                        activeEffect.RuntimeId,
                        runtimeId,
                        System.StringComparison.Ordinal))
                {
                    continue;
                }

                RemoveEffect(activeEffect);
            }
        }

        public void RemoveEffect(
            EffectRuntimeData runtimeData)
        {
            if (runtimeData == null)
            {
                return;
            }

            if (!activeEffects.Remove(runtimeData))
            {
                return;
            }

            RemoveLifetime(runtimeData.RuntimeId);

            runtimeData.OnRemove();

            OnEffectRemoved?.Invoke(runtimeData);

            if (logDebug)
            {
                Debug.Log(
                    $"[EffectManager] Effect removed. id={runtimeData.RuntimeId}");
            }
        }
        private void Update()
        {
            TickTimedEffects(Time.deltaTime);
        }
        public void OnBattleStarted()
        {
            isBattleActive = true;

            RemoveEffectsByLifetime(
                EffectLifetimeType.ConsumeOnBattleStart);
        }

        public void OnBattleEnded()
        {
            isBattleActive = false;

            RemoveEffectsByLifetime(
                EffectLifetimeType.CombatOnly);

            RemoveEffectsByLifetime(
                EffectLifetimeType.CombatTimed);

            RemoveEffectsByLifetime(
                EffectLifetimeType.ConsumeOnBattleEnd);
        }

        public void OnStageEntered()
        {
            isBattleActive = false;

            RemoveEffectsByLifetime(
                EffectLifetimeType.CombatOnly);

            RemoveEffectsByLifetime(
                EffectLifetimeType.CombatTimed);
        }
        private void TickTimedEffects(float deltaTime)
        {
            List<EffectRuntimeData> expiredEffects =
                new List<EffectRuntimeData>();

            for (int i = activeEffectLifetimes.Count - 1;
                 i >= 0;
                 i--)
            {
                if (i >= activeEffectLifetimes.Count)
                {
                    continue;
                }

                EffectLifetimeData lifetimeData =
                    activeEffectLifetimes[i];

                if (lifetimeData == null)
                {
                    activeEffectLifetimes.RemoveAt(i);
                    continue;
                }

                if (!ShouldTickLifetime(lifetimeData))
                {
                    continue;
                }

                lifetimeData.remainingTime -= deltaTime;

                if (lifetimeData.remainingTime > 0f)
                {
                    continue;
                }

                EffectRuntimeData effect =
                    FindEffect(lifetimeData.runtimeId);

                if (effect != null)
                {
                    expiredEffects.Add(effect);
                }
                else
                {
                    activeEffectLifetimes.RemoveAt(i);
                }
            }

            for (int i = 0; i < expiredEffects.Count; i++)
            {
                RemoveEffect(expiredEffects[i]);
            }
        }

        private bool ShouldTickLifetime(
            EffectLifetimeData lifetimeData)
        {
            if (lifetimeData.remainingTime < 0f)
            {
                return false;
            }

            if (lifetimeData.lifetimeType == EffectLifetimeType.Instant)
            {
                return false;
            }

            if (lifetimeData.lifetimeType == EffectLifetimeType.Timed)
            {
                return true;
            }

            if (lifetimeData.lifetimeType == EffectLifetimeType.CombatTimed)
            {
                return isBattleActive;
            }

            return false;
        }

        private void RemoveEffectsByLifetime(
            EffectLifetimeType lifetimeType)
        {
            for (int i = activeEffectLifetimes.Count - 1;
                 i >= 0;
                 i--)
            {
                EffectLifetimeData lifetimeData =
                    activeEffectLifetimes[i];

                if (lifetimeData == null
                    || lifetimeData.lifetimeType != lifetimeType)
                {
                    continue;
                }

                EffectRuntimeData effect =
                    FindEffect(lifetimeData.runtimeId);

                if (effect != null)
                {
                    RemoveEffect(effect);
                }
                else
                {
                    activeEffectLifetimes.RemoveAt(i);
                }
            }
        }

        public void RemoveBuffs()
        {
            RemoveEffectsByCategory(
                EffectCategoryType.Buff);
        }

        public void RemoveDebuffs()
        {
            RemoveEffectsByCategory(
                EffectCategoryType.Debuff);
        }

        public void RemoveEffectsByCategory(
            EffectCategoryType categoryType)
        {
            for (int i = activeEffectLifetimes.Count - 1;
                 i >= 0;
                 i--)
            {
                EffectLifetimeData lifetimeData =
                    activeEffectLifetimes[i];

                if (lifetimeData == null
                    || lifetimeData.categoryType != categoryType)
                {
                    continue;
                }

                EffectRuntimeData effect =
                    FindEffect(lifetimeData.runtimeId);

                if (effect != null)
                {
                    RemoveEffect(effect);
                }
                else
                {
                    activeEffectLifetimes.RemoveAt(i);
                }
            }
        }

        public bool HasBuff(string runtimeId)
        {
            return HasEffectByCategory(
                runtimeId,
                EffectCategoryType.Buff);
        }

        public bool HasDebuff(string runtimeId)
        {
            return HasEffectByCategory(
                runtimeId,
                EffectCategoryType.Debuff);
        }

        public bool HasEffectByCategory(
            string runtimeId,
            EffectCategoryType categoryType)
        {
            EffectLifetimeData lifetimeData =
                FindLifetime(runtimeId);

            return lifetimeData != null
                && lifetimeData.categoryType == categoryType
                && FindEffect(runtimeId) != null;
        }

        private EffectLifetimeData FindLifetime(string runtimeId)
        {
            return activeEffectLifetimes.Find(x => x != null
                && string.Equals(
                    x.runtimeId,
                    runtimeId,
                    System.StringComparison.Ordinal));
        }

        private void RemoveLifetime(string runtimeId)
        {
            for (int i = activeEffectLifetimes.Count - 1;
                 i >= 0;
                 i--)
            {
                EffectLifetimeData lifetimeData =
                    activeEffectLifetimes[i];

                if (lifetimeData == null
                    || string.Equals(
                        lifetimeData.runtimeId,
                        runtimeId,
                        System.StringComparison.Ordinal))
                {
                    activeEffectLifetimes.RemoveAt(i);
                }
            }
        }

        public void RemoveEffectsBySource(
            string runtimeId)
        {
            for (int i = activeEffects.Count - 1;
                 i >= 0;
                 i--)
            {
                EffectRuntimeData runtimeData =
                    activeEffects[i];

                if (runtimeData == null)
                {
                    continue;
                }

                if (!string.Equals(
                        runtimeData.RuntimeId,
                        runtimeId,
                        System.StringComparison.Ordinal))
                {
                    continue;
                }

                RemoveEffect(runtimeData);
            }
        }

        public bool HasEffect(string runtimeId)
        {
            return FindEffect(runtimeId) != null;
        }

        public EffectRuntimeData FindEffect(string runtimeId)
        {
            return activeEffects.Find(x => x != null
                && string.Equals(
                    x.RuntimeId,
                    runtimeId,
                    System.StringComparison.Ordinal));
        }

        private void HandleAnyHealed(
            CharacterManager healTarget,
            float healAmount)
        {
            if (activeEffects == null || activeEffects.Count <= 0)
            {
                return;
            }

            for (int i = 0; i < activeEffects.Count; i++)
            {
                EffectRuntimeData runtimeData = activeEffects[i];

                if (runtimeData == null || !runtimeData.IsActive)
                {
                    continue;
                }

                if (runtimeData is ChanceOnHealStatModifierEffectRuntime chanceOnHealStatModifierEffect)
                {
                    chanceOnHealStatModifierEffect.OnHeal(
                        null,
                        healTarget,
                        healAmount);
                }
            }
        }

        private void HandleAnyDamageApplied(
            CharacterDamageRequest request,
            CharacterDamageResult result)
        {
            if (activeEffects == null || activeEffects.Count <= 0)
            {
                return;
            }

            for (int i = 0; i < activeEffects.Count; i++)
            {
                EffectRuntimeData runtimeData = activeEffects[i];

                if (runtimeData == null || !runtimeData.IsActive)
                {
                    continue;
                }

                if (runtimeData is ChanceOnHitSkillEffectRuntime chanceOnHitSkillEffect)
                {
                    chanceOnHitSkillEffect.OnHit(
                        request.attacker.GetComponent<CharacterManager>(),
                        request.target.GetComponent<CharacterManager>(),
                        true);
                    continue;
                }

                if (runtimeData is ChanceOnHitStatModifierEffectRuntime chanceOnHitStatModifierEffect)
                {
                    chanceOnHitStatModifierEffect.OnHit(
                        request,
                        result);
                }
            }
        }

        public void Clear()
        {
            for (int i = activeEffects.Count - 1;
                 i >= 0;
                 i--)
            {
                RemoveEffect(activeEffects[i]);
            }
        }
    }
}