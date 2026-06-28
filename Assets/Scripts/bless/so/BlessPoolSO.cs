using System;
using System.Collections.Generic;
using UnityEngine;
using Shrine;
using String;

namespace Bless
{
    [CreateAssetMenu(
        fileName = "BlessPool",
        menuName = "BS/Bless/Bless Pool")]
    public class BlessPoolSO : ScriptableObject
    {
        [Serializable]
        public class BlessPoolEntry
        {
            [SerializeField] private BlessSO blessing;

            [SerializeField, Min(1)] private int weight = 1;

            [SerializeField, Min(0)] private int progressionStep = 0;

            public BlessSO Blessing => blessing;
            public int Weight => Mathf.Max(1, weight);
            public int ProgressionStep => Mathf.Max(0, progressionStep);
        }

        [Header("Info")]
        [SerializeField] private string poolId;

        [Header("Visual")]
        [SerializeField] private Sprite icon;

        [Header("Pool")]
        [SerializeField] private List<BlessPoolEntry> blessings = new();

        public string LocalizationMainKey => poolId;

        public string DisplayName =>
            StringManager.Instance.Get(
                LocalizationMainKey,
                "name");

        public string Description =>
            StringManager.Instance.Get(
                LocalizationMainKey,
                "desc");

        public string PoolId => poolId;
        public Sprite Icon => icon;
        public IReadOnlyList<BlessPoolEntry> Blessings => blessings;

        public BlessSO GetRandomBlessing(
            ShrineGodType godType,
            int progressionStep,
            List<BlessSO> excludeList = null)
        {
            if (blessings == null
                || blessings.Count == 0)
            {
                return null;
            }

            List<BlessPoolEntry> candidates = new();

            foreach (BlessPoolEntry entry in blessings)
            {
                if (entry == null
                    || entry.Blessing == null)
                {
                    continue;
                }

                if (entry.Blessing.GodType != ShrineGodType.None
                    && entry.Blessing.GodType != godType)
                {
                    continue;
                }

                if (entry.ProgressionStep != progressionStep)
                {
                    continue;
                }

                if (excludeList != null
                    && excludeList.Contains(entry.Blessing))
                {
                    continue;
                }

                candidates.Add(entry);
            }

            if (candidates.Count == 0)
            {
                return null;
            }

            int totalWeight = 0;

            foreach (BlessPoolEntry entry in candidates)
            {
                totalWeight += entry.Weight;
            }

            int randomValue = UnityEngine.Random.Range(0, totalWeight);
            int currentWeight = 0;

            foreach (BlessPoolEntry entry in candidates)
            {
                currentWeight += entry.Weight;

                if (randomValue < currentWeight)
                {
                    return entry.Blessing;
                }
            }

            return candidates[0].Blessing;
        }

        public BlessSO GetRandomBlessing()
        {
            if (blessings == null
                || blessings.Count == 0)
            {
                return null;
            }

            int totalWeight = 0;

            foreach (BlessPoolEntry entry in blessings)
            {
                if (entry == null
                    || entry.Blessing == null)
                {
                    continue;
                }

                totalWeight += entry.Weight;
            }

            if (totalWeight <= 0)
            {
                return null;
            }

            int randomValue = UnityEngine.Random.Range(0, totalWeight);
            int currentWeight = 0;

            foreach (BlessPoolEntry entry in blessings)
            {
                if (entry == null
                    || entry.Blessing == null)
                {
                    continue;
                }

                currentWeight += entry.Weight;

                if (randomValue < currentWeight)
                {
                    return entry.Blessing;
                }
            }

            return blessings[0] != null
                ? blessings[0].Blessing
                : null;
        }
    }
}