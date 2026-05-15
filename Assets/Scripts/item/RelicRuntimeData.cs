

using System.Collections.Generic;
using UnityEngine;

namespace Item
{
    [System.Serializable]
    public class RelicRuntimeData
    {
        [SerializeField]
        private List<RelicSO> relics = new();

        public IReadOnlyList<RelicSO> Relics => relics;

        public bool AddRelic(RelicSO relic)
        {
            if (relic == null)
            {
                return false;
            }

            if (relics.Contains(relic))
            {
                return false;
            }

            relics.Add(relic);
            return true;
        }

        public bool RemoveRelic(RelicSO relic)
        {
            if (relic == null)
            {
                return false;
            }

            return relics.Remove(relic);
        }

        public bool HasRelic(RelicSO relic)
        {
            if (relic == null)
            {
                return false;
            }

            return relics.Contains(relic);
        }

        public void Clear()
        {
            relics.Clear();
        }
    }
}