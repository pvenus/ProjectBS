

namespace Currency
{
    [System.Serializable]
    public class CurrencyRutimeData
    {
        public int gold;

        public void AddGold(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            gold += amount;
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

            gold -= amount;
            return true;
        }
    }
}