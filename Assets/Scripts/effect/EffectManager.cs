using System.Collections.Generic;
using UnityEngine;

namespace Effect
{
    public enum EffectLifetimeType
    {
        Manual,
        CombatOnly,
        Timed,
        CombatTimed,
        ConsumeOnBattleStart,
        ConsumeOnBattleEnd
    }

    [System.Serializable]
    public class EffectLifetimeData
    {
        public string runtimeId;
        public EffectLifetimeType lifetimeType;
        public float remainingTime;

        public EffectLifetimeData(
            string runtimeId,
            EffectLifetimeType lifetimeType,
            float duration)
        {
            this.runtimeId = runtimeId;
            this.lifetimeType = lifetimeType;
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

        public IReadOnlyList<EffectRuntimeData> ActiveEffects
            => activeEffects;

        public void AddEffect(
            EffectRuntimeData runtimeData)
        {
            AddEffect(
                runtimeData,
                EffectLifetimeType.Manual,
                -1f);
        }

        public void AddEffect(
            EffectRuntimeData runtimeData,
            EffectLifetimeType lifetimeType,
            float duration = -1f)
        {
            if (runtimeData == null)
            {
                return;
            }

            activeEffects.Add(runtimeData);

            activeEffectLifetimes.Add(
                new EffectLifetimeData(
                    runtimeData.RuntimeId,
                    lifetimeType,
                    duration));

            runtimeData.OnApply();

            OnEffectAdded?.Invoke(runtimeData);

            if (logDebug)
            {
                Debug.Log(
                    $"[EffectManager] Effect added. id={runtimeData.RuntimeId}, lifetime={lifetimeType}, duration={duration}");
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
            for (int i = activeEffectLifetimes.Count - 1;
                 i >= 0;
                 i--)
            {
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
                    RemoveEffect(effect);
                }
                else
                {
                    activeEffectLifetimes.RemoveAt(i);
                }
            }
        }

        private bool ShouldTickLifetime(
            EffectLifetimeData lifetimeData)
        {
            if (lifetimeData.remainingTime < 0f)
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
            EffectSourceType sourceType,
            string sourceId)
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

                if (runtimeData.SourceType != sourceType)
                {
                    continue;
                }

                if (!string.Equals(
                        runtimeData.SourceId,
                        sourceId,
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