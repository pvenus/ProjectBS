

using UnityEngine;

namespace Shrine
{
    public delegate void GoldChangedHandler(int currentGold);

    public delegate void HpChangedHandler(
        int currentHp,
        int maxHp);
    /// <summary>
    /// Shrine 시스템에서 사용하는 플레이어 상태 Context.
    /// Gold / HP / 기타 플레이어 자원 접근을 담당한다.
    ///
    /// 현재는 테스트용 Runtime 데이터 기반으로 구성되어 있으며,
    /// 이후 실제 PlayerRuntime / Inventory / Party 시스템과 연결 예정이다.
    /// </summary>
    [System.Serializable]
    public class ShrinePlayerContext
    {
        [Header("Debug Runtime")]
        [SerializeField]
        private int currentGold = 999;

        [SerializeField]
        private int currentHp = 100;

        [SerializeField]
        private int maxHp = 100;

        public int CurrentGold => currentGold;

        public int CurrentHp => currentHp;

        public int MaxHp => maxHp;

        public event GoldChangedHandler OnGoldChanged;

        public event HpChangedHandler OnHpChanged;

        public bool HasEnoughGold(int amount)
        {
            return currentGold >= amount;
        }

        public bool SpendGold(int amount)
        {
            if (amount <= 0)
            {
                return false;
            }

            if (!HasEnoughGold(amount))
            {
                return false;
            }

            currentGold -= amount;
            OnGoldChanged?.Invoke(currentGold);
            return true;
        }

        public void AddGold(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            currentGold += amount;
            OnGoldChanged?.Invoke(currentGold);
        }

        public void HealHp(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            currentHp += amount;

            if (currentHp > maxHp)
            {
                currentHp = maxHp;
            }
            OnHpChanged?.Invoke(
                currentHp,
                maxHp);
        }

        public void HealPercent(int percent)
        {
            if (percent <= 0)
            {
                return;
            }

            int healAmount =
                Mathf.CeilToInt(maxHp * (percent / 100f));

            HealHp(healAmount);
        }

        public void SetGold(int amount)
        {
            currentGold = Mathf.Max(0, amount);
            OnGoldChanged?.Invoke(currentGold);
        }

        public void SetHp(
            int hp,
            int maxHp)
        {
            this.maxHp = Mathf.Max(1, maxHp);
            currentHp = Mathf.Clamp(hp, 0, this.maxHp);
            OnHpChanged?.Invoke(
                currentHp,
                this.maxHp);
        }
    }
}