using Battle;
using UnityEngine;

namespace Battle.UI.StrategicBoard
{
    /// <summary>
    /// Adapts StrategicSkillCostManager gauge changes to the prototype gauge view.
    /// </summary>
    public sealed class StrategicGaugeBinder : MonoBehaviour
    {
        [SerializeField] private StrategicGaugeView gaugeView;
        [SerializeField] private StrategicBoardView boardView;
        [SerializeField] private StrategicSkillCostManager managerOverride;
        [SerializeField] private bool findManagerInScene = true;
        [SerializeField] private float chargePerSecond;

        private StrategicSkillCostManager subscribedManager;

        public StrategicSkillCostManager SubscribedManager => subscribedManager;

        private void OnEnable()
        {
            TrySubscribe();
            ApplyChargePerSecond();
        }

        private void Update()
        {
            if (subscribedManager == null)
            {
                TrySubscribe();
            }
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void SetManager(StrategicSkillCostManager manager)
        {
            if (managerOverride == manager && subscribedManager == manager)
            {
                SynchronizeNow();
                return;
            }

            managerOverride = manager;
            Unsubscribe();

            if (isActiveAndEnabled)
            {
                TrySubscribe();
            }
        }

        public void SetChargePerSecond(float amount)
        {
            chargePerSecond = amount;
            ApplyChargePerSecond();
        }

        public void SynchronizeNow()
        {
            StrategicSkillCostManager manager = subscribedManager != null
                ? subscribedManager
                : ResolveManager();

            if (manager == null)
            {
                return;
            }

            ApplyGauge(manager.CurrentGauge, manager.MaxGauge);
        }

        private StrategicSkillCostManager ResolveManager()
        {
            if (managerOverride != null)
            {
                return managerOverride;
            }

            if (StrategicSkillCostManager.Instance != null)
            {
                return StrategicSkillCostManager.Instance;
            }

            if (!findManagerInScene)
            {
                return null;
            }

#if UNITY_2023_1_OR_NEWER
            return FindFirstObjectByType<StrategicSkillCostManager>();
#else
            return FindObjectOfType<StrategicSkillCostManager>();
#endif
        }

        private void TrySubscribe()
        {
            StrategicSkillCostManager manager = ResolveManager();

            if (manager == null)
            {
                return;
            }

            if (subscribedManager == manager)
            {
                SynchronizeNow();
                return;
            }

            Unsubscribe();
            subscribedManager = manager;
            subscribedManager.OnGaugeChanged += HandleGaugeChanged;
            SynchronizeNow();
        }

        private void Unsubscribe()
        {
            if (subscribedManager == null)
            {
                return;
            }

            subscribedManager.OnGaugeChanged -= HandleGaugeChanged;
            subscribedManager = null;
        }

        private void HandleGaugeChanged(int current, int max)
        {
            ApplyGauge(current, max);
        }

        private void ApplyGauge(int current, int max)
        {
            if (gaugeView != null)
            {
                gaugeView.SetGauge(current, max);
            }

            if (boardView != null)
            {
                boardView.RefreshSlotResourceStates(current);
            }
        }

        private void ApplyChargePerSecond()
        {
            if (gaugeView != null)
            {
                gaugeView.SetChargePerSecond(chargePerSecond);
            }
        }
    }
}
