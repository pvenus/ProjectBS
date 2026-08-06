using System;
using Session;
using UnityEngine;

namespace Currency
{
    /// <summary>
    /// 재화 런타임 데이터를 조작하는 매니저.
    ///
    /// 데이터는 직접 생성/소유하지 않고 StageSession에서 관리하는
    /// CurrencyRutimeData를 Bind해서 사용한다.
    /// </summary>
    public class CurrencyManager : MonoBehaviour
    {
        public static CurrencyManager Instance { get; private set; }

        public event Action<int> OnGoldChanged;
        public event Action<int> OnGoldAdded;
        public event Action<int> OnGoldSpent;

        public CurrencyRutimeData RuntimeData =>
            GameSession.Instance.StageSession.CurrencyRuntimeData;

        public int Gold => RuntimeData.gold;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void AddGold(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            RuntimeData.AddGold(amount);

            OnGoldAdded?.Invoke(amount);
            NotifyGoldChanged();
        }

        public bool TrySpendGold(int amount)
        {
            if (amount <= 0)
            {
                return true;
            }

            bool success = RuntimeData.TrySpendGold(amount);

            if (!success)
            {
                return false;
            }

            OnGoldSpent?.Invoke(amount);
            NotifyGoldChanged();

            return true;
        }

        public bool CanSpendGold(int amount)
        {
            if (amount <= 0)
            {
                return true;
            }

            return RuntimeData.gold >= amount;
        }

        public void SetGold(int amount)
        {
            RuntimeData.gold = Mathf.Max(0, amount);
            NotifyGoldChanged();
        }

        private void NotifyGoldChanged()
        {
            OnGoldChanged?.Invoke(Gold);
        }
    }
}