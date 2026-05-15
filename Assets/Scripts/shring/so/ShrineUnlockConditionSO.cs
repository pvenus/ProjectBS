using UnityEngine;
using System.Collections.Generic;

namespace Shrine
{
    public abstract class ShrineUnlockConditionSO : ScriptableObject
    {
        [Header("Condition")]
        [TextArea]
        public string description;

        public abstract bool IsSatisfied(
            ShrineMetaProgressData progressData);
    }

    [System.Serializable]
    public class ShrineMetaProgressData
    {
        [Header("Counters")]
        public List<ShrineProgressCounterEntry> counters = new();

        [Header("Flags")]
        public List<ShrineProgressFlagEntry> flags = new();

        public int GetCounter(ShrineProgressKey key)
        {
            ShrineProgressCounterEntry entry =
                counters.Find(x => x.key == key);

            if (entry == null)
            {
                return 0;
            }

            return entry.value;
        }

        public bool HasFlag(ShrineProgressKey key)
        {
            ShrineProgressFlagEntry entry =
                flags.Find(x => x.key == key);

            if (entry == null)
            {
                return false;
            }

            return entry.value;
        }
    }

    [System.Serializable]
    public class ShrineProgressCounterEntry
    {
        public ShrineProgressKey key;
        public int value;
    }

    [System.Serializable]
    public class ShrineProgressFlagEntry
    {
        public ShrineProgressKey key;
        public bool value;
    }
}
