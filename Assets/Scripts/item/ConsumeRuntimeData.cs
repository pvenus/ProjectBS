
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Item
{
    [Serializable]
    public class ConsumeRuntimeData
    {
        [Serializable]
        public class ConsumeEntry
        {
            [SerializeField] private ConsumeSO consume;
            [SerializeField] private bool owned;

            public ConsumeSO Consume => consume;
            public bool Owned => owned;

            public ConsumeEntry(ConsumeSO consume)
            {
                this.consume = consume;
                owned = consume != null;
            }

            public bool CanUseInBattle()
            {
                if (!owned)
                {
                    return false;
                }

                if (consume == null)
                {
                    return false;
                }

                return consume.reusable;
            }

            public bool UseInBattle()
            {
                return CanUseInBattle();
            }

            public void ResetBattleUsage()
            {
            }

            public void SetOwned(bool value)
            {
                owned = value;
            }
        }

        [Header("Consume Inventory")]
        [SerializeField]
        private List<ConsumeEntry> consumes = new();

        public IReadOnlyList<ConsumeEntry> Consumes
            => consumes;

        public bool AddConsume(ConsumeSO consume)
        {
            if (consume == null)
            {
                return false;
            }

            ConsumeEntry exists =
                FindConsume(consume);

            if (exists != null)
            {
                exists.SetOwned(true);
                return true;
            }

            consumes.Add(new ConsumeEntry(consume));
            return true;
        }

        public bool RemoveConsume(ConsumeSO consume)
        {
            ConsumeEntry entry =
                FindConsume(consume);

            if (entry == null)
            {
                return false;
            }

            return consumes.Remove(entry);
        }

        public bool HasConsume(ConsumeSO consume)
        {
            return FindConsume(consume) != null;
        }

        public ConsumeEntry FindConsume(ConsumeSO consume)
        {
            if (consume == null)
            {
                return null;
            }

            return consumes.Find(x => x != null
                && x.Consume == consume);
        }

        public bool UseConsumeInBattle(ConsumeSO consume)
        {
            ConsumeEntry entry =
                FindConsume(consume);

            if (entry == null)
            {
                return false;
            }

            return entry.UseInBattle();
        }

        public void ResetBattleUsage()
        {
            foreach (ConsumeEntry entry in consumes)
            {
                if (entry == null)
                {
                    continue;
                }

                entry.ResetBattleUsage();
            }
        }

        public void Clear()
        {
            consumes.Clear();
        }
    }
}