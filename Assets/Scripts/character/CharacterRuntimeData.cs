using System;
using System.Collections.Generic;
using Stat;
using UnityEngine;

namespace Character
{
    [Serializable]
    public class CharacterRuntimeData
    {
        [Header("Definition")]
        public CharacterSO characterSO;

        [Header("Progression")]
        public bool isDead;

        [Header("Runtime Stats")]
        public List<StatEntry> stats = new();

        [Header("Final Runtime Stats")]
        public List<StatEntry> finalStats = new();

        public float GetStatValue(StatType statType)
        {
            for (int i = 0;
                 i < finalStats.Count;
                 i++)
            {
                if (finalStats[i].statType != statType)
                {
                    continue;
                }

                return finalStats[i].value;
            }

            return 0f;
        }
    }
}