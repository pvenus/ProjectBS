using System;
using System.Collections.Generic;
using UnityEngine;

namespace Stat
{
    public class StatManager : MonoBehaviour
    {
        [Serializable]
        public class StatEntry
        {
            public StatType statType;

            public float value;
        }

        public static StatManager Instance { get; private set; }

        [Header("Debug")]
        [SerializeField]
        private bool logDebug = true;

        [Header("Runtime Stats")]
        [SerializeField]
        private List<StatEntry> stats = new();

        public event Action<StatType, float, float> OnAnyStatChanged;

        private readonly Dictionary<
            StatType,
            Action<StatType, float, float>> statListeners = new();

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

        public void RegisterListener(
            StatType statType,
            Action<StatType, float, float> listener)
        {
            if (listener == null)
            {
                return;
            }

            if (!statListeners.ContainsKey(statType))
            {
                statListeners[statType] = null;
            }

            statListeners[statType] += listener;
        }

        public void RegisterListener(
            IEnumerable<StatType> statTypes,
            Action<StatType, float, float> listener)
        {
            if (statTypes == null
                || listener == null)
            {
                return;
            }

            foreach (StatType statType in statTypes)
            {
                RegisterListener(
                    statType,
                    listener);
            }
        }

        public void UnregisterListener(
            StatType statType,
            Action<StatType, float, float> listener)
        {
            if (listener == null)
            {
                return;
            }

            if (!statListeners.ContainsKey(statType))
            {
                return;
            }

            statListeners[statType] -= listener;
        }

        public void UnregisterListener(
            IEnumerable<StatType> statTypes,
            Action<StatType, float, float> listener)
        {
            if (statTypes == null
                || listener == null)
            {
                return;
            }

            foreach (StatType statType in statTypes)
            {
                UnregisterListener(
                    statType,
                    listener);
            }
        }

        public float GetStat(StatType statType)
        {
            StatEntry entry = FindStat(statType);

            if (entry == null)
            {
                return 0f;
            }

            return entry.value;
        }

        public void AddStat(
            StatType statType,
            float value)
        {
            SetStat(
                statType,
                GetStat(statType) + value);
        }

        public void SetStat(
            StatType statType,
            float value)
        {
            StatEntry entry = FindStat(statType);

            float previousValue =
                entry != null
                    ? entry.value
                    : 0f;

            if (entry == null)
            {
                entry = new StatEntry
                {
                    statType = statType,
                    value = value
                };

                stats.Add(entry);
            }
            else
            {
                entry.value = value;
            }

            if (logDebug)
            {
                Debug.Log(
                    $"[StatManager] Stat changed. type={statType} previous={previousValue} current={value}");
            }

            if (statListeners.TryGetValue(
                    statType,
                    out Action<StatType, float, float> listeners))
            {
                listeners?.Invoke(
                    statType,
                    previousValue,
                    value);
            }

            OnAnyStatChanged?.Invoke(
                statType,
                previousValue,
                value);
        }

        public bool HasStat(StatType statType)
        {
            return FindStat(statType) != null;
        }

        public void ResetRuntimeStats()
        {
            stats.Clear();

            if (logDebug)
            {
                Debug.Log("[StatManager] Runtime stats reset.");
            }
        }

        private StatEntry FindStat(StatType statType)
        {
            return stats.Find(x => x != null
                && x.statType == statType);
        }
    }
}