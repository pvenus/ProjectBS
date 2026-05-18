using System;
using System.Collections.Generic;
using UnityEngine;
using Shrine;

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
            public BlessSO blessing;

            [Min(1)]
            public int weight = 1;

            [Min(0)]
            public int progressionStep = 0;
        }

        [Header("Info")]
        public string poolId;

        public string displayName;

        [TextArea]
        public string description;


        [Header("Visual")]
        public Sprite icon;

        [Header("Pool")]
        public List<BlessPoolEntry> blessings = new();

        public BlessSO GetRandomBlessing(
            Shrine.ShrineGodType godType,
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
                    || entry.blessing == null)
                {
                    continue;
                }

                if (entry.blessing.godType != Shrine.ShrineGodType.None
                    && entry.blessing.godType != godType)
                {
                    continue;
                }

                if (entry.progressionStep != progressionStep)
                {
                    continue;
                }

                if (excludeList != null
                    && excludeList.Contains(entry.blessing))
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
                totalWeight += Mathf.Max(1, entry.weight);
            }

            int randomValue = UnityEngine.Random.Range(0, totalWeight);
            int currentWeight = 0;

            foreach (BlessPoolEntry entry in candidates)
            {
                currentWeight += Mathf.Max(1, entry.weight);

                if (randomValue < currentWeight)
                {
                    return entry.blessing;
                }
            }

            return candidates[0].blessing;
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
                    || entry.blessing == null)
                {
                    continue;
                }

                totalWeight += Mathf.Max(1, entry.weight);
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
                    || entry.blessing == null)
                {
                    continue;
                }

                currentWeight += Mathf.Max(1, entry.weight);

                if (randomValue < currentWeight)
                {
                    return entry.blessing;
                }
            }

            return blessings[0] != null
                ? blessings[0].blessing
                : null;
        }
    }
}