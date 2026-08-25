using UnityEngine;
using ProjectBS.Core;

namespace UIFramework.Test
{
    public class TestCurrencyManager : MonoBehaviour, ICurrencyManager
    {
        [Header("Initial Gold")]
        public int initialGold = 9999;
        
        private int _gold;
        public int Gold 
        { 
            get => _gold; 
            set
            {
                if (_gold != value)
                {
                    _gold = value;
                    OnGoldChanged?.Invoke(_gold);
                }
            }
        }

        public event System.Action<int> OnGoldChanged;

        private void Awake()
        {
            _gold = initialGold;
            AppManagers.Currency = this;
        }
        
        [ContextMenu("Add 1000 Gold")]
        public void AddGold()
        {
            Gold += 1000;
        }
    }
}
