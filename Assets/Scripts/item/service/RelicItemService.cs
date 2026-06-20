

using System;
using System.Collections.Generic;
using System.Linq;

namespace Item.Service
{
    public class RelicItemService
    {
        public const int MaxEquippedRelicCount = 3;

        private readonly List<RelicSO> ownedRelics = new();
        private readonly List<RelicSO> equippedRelics = new();

        public event Action<RelicSO> OnRelicAdded;
        public event Action<RelicSO> OnRelicRemoved;
        public event Action<RelicSO> OnRelicEquipped;
        public event Action<RelicSO> OnRelicUnequipped;

        public IReadOnlyList<RelicSO> OwnedRelics => ownedRelics;
        public IReadOnlyList<RelicSO> EquippedRelics => equippedRelics;

        public bool Add(RelicSO relic)
        {
            if (relic == null)
                return false;

            if (ownedRelics.Contains(relic))
                return false;

            ownedRelics.Add(relic);
            OnRelicAdded?.Invoke(relic);

            if (equippedRelics.Count < MaxEquippedRelicCount)
            {
                Equip(relic);
            }

            return true;
        }

        public bool Remove(RelicSO relic)
        {
            if (relic == null)
                return false;

            Unequip(relic);

            if (!ownedRelics.Remove(relic))
                return false;

            OnRelicRemoved?.Invoke(relic);
            return true;
        }

        public bool Equip(RelicSO relic)
        {
            if (relic == null)
                return false;

            if (!ownedRelics.Contains(relic))
                return false;

            if (equippedRelics.Contains(relic))
                return false;

            if (equippedRelics.Count >= MaxEquippedRelicCount)
                return false;

            equippedRelics.Add(relic);
            OnRelicEquipped?.Invoke(relic);
            return true;
        }

        public bool Unequip(RelicSO relic)
        {
            if (relic == null)
                return false;

            if (!equippedRelics.Remove(relic))
                return false;

            OnRelicUnequipped?.Invoke(relic);
            return true;
        }

        public bool IsOwned(string relicId)
        {
            return ownedRelics.Any(x => x != null && x.relicId == relicId);
        }

        public bool IsEquipped(string relicId)
        {
            return equippedRelics.Any(x => x != null && x.relicId == relicId);
        }

        public void Clear()
        {
            ownedRelics.Clear();
            equippedRelics.Clear();
        }
    }
}