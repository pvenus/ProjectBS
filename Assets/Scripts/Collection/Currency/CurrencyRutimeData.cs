

namespace Currency
{
    [System.Serializable]
    public readonly struct CurrencyRuntimeSnapshot
    {
        public CurrencyRuntimeSnapshot(int gold, int revision)
        {
            Gold = gold;
            Revision = revision;
        }

        public int Gold { get; }
        public int Revision { get; }
    }

    [System.Serializable]
    public class CurrencyRutimeData
    {
        public int gold;
        public int revision;

        public int Revision => revision;

        public CurrencyRuntimeSnapshot CaptureSnapshot() => new(gold, revision);

        public void AddGold(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            if (!TryAddGoldExact(amount)) return;
        }

        public bool TrySpendGold(int amount)
        {
            if (amount <= 0)
            {
                return true;
            }

            if (gold < amount)
            {
                return false;
            }

            return TryApplyGold(gold - amount);
        }

        public bool TryAddGoldExact(int amount)
        {
            if (amount <= 0) return false;
            long next = (long)gold + amount;
            return next <= int.MaxValue && TryApplyGold((int)next);
        }

        public bool TryRestoreSnapshot(CurrencyRuntimeSnapshot snapshot)
        {
            if (revision != snapshot.Revision + 1) return false;
            gold = snapshot.Gold;
            revision = snapshot.Revision;
            return true;
        }

        private bool TryApplyGold(int next)
        {
            if (next < 0 || revision == int.MaxValue) return false;
            gold = next;
            revision++;
            return true;
        }
    }
}
