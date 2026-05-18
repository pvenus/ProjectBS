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

        public event Action<StatType, float> OnStatChanged;

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
                    $"[StatManager] Stat changed. type={statType} value={value}");
            }

            OnStatChanged?.Invoke(statType, value);
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

    public enum StatType
    {
        None = 0,

        Reputation = 100,
        Fame = 200,
        Notoriety = 300,

        LifeFaith = 1000,
        WarFaith = 1100,
        GreedFaith = 1200,
        DarkFaith = 1300,
    }
}