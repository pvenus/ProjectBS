using System.Collections.Generic;
using UnityEngine;

namespace Effect
{
    public class EffectManager : MonoBehaviour
    {
        public static EffectManager Instance { get; private set; }

        [Header("Debug")]
        [SerializeField] private bool logDebug = true;

        [Header("Runtime Effects")]
        [SerializeField]
        private List<EffectRuntimeData> activeEffects = new();

        public event System.Action<EffectRuntimeData> OnEffectAdded;
        public event System.Action<EffectRuntimeData> OnEffectRemoved;

        private void Awake()
        {
            if (Instance != null
                && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public IReadOnlyList<EffectRuntimeData> ActiveEffects
            => activeEffects;

        public void AddEffect(
            EffectRuntimeData runtimeData)
        {
            if (runtimeData == null)
            {
                return;
            }

            activeEffects.Add(runtimeData);

            runtimeData.OnApply();

            OnEffectAdded?.Invoke(runtimeData);

            if (logDebug)
            {
                Debug.Log(
                    $"[EffectManager] Effect added. id={runtimeData.RuntimeId}");
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

            runtimeData.OnRemove();

            OnEffectRemoved?.Invoke(runtimeData);

            if (logDebug)
            {
                Debug.Log(
                    $"[EffectManager] Effect removed. id={runtimeData.RuntimeId}");
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