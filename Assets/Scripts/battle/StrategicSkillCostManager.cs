using System;
using Character;
using UnityEngine;

namespace Battle
{
    /// <summary>
    /// 플레이어 전략 스킬의 공용 코스트/게이지를 관리한다.
    /// 몬스터 처치, 시간 보너스, 골드 소모 등으로 게이지를 획득하고,
    /// 전략 스킬 사용 시 게이지를 차감한다.
    /// </summary>
    public class StrategicSkillCostManager : MonoBehaviour
    {
        public static StrategicSkillCostManager Instance { get; private set; }

        [Header("Config")]
        [SerializeField] private StrategicSkillCostConfigSO config;
        [Header("Runtime")]
        [SerializeField] private StrategicSkillCostRuntimeData runtimeData;
        private float passiveGainTimer;
        private int lastNotifiedGauge = -1;
        private int lastNotifiedMaxGauge = -1;
        private bool isCharacterDeathEventRegistered;
        public int CurrentGauge => runtimeData != null ? runtimeData.CurrentGauge : 0;
        public int MaxGauge => runtimeData != null ? runtimeData.MaxGauge : config != null ? config.maxGauge : 100;
        public float GaugeRate => runtimeData != null ? runtimeData.GaugeRate : 0f;

        public event Action<int, int> OnGaugeChanged = delegate { };

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            runtimeData = new StrategicSkillCostRuntimeData(config);
            NotifyGaugeChanged();
        }
        private void OnEnable()
        {
            RegisterCharacterDeathEvent();
        }

        private void OnDisable()
        {
            UnregisterCharacterDeathEvent();
        }

        private void OnDestroy()
        {
            UnregisterCharacterDeathEvent();

            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            UpdatePassiveGain();
            NotifyGaugeChangedIfNeeded();
        }

        private void UpdatePassiveGain()
        {
            if (config == null || !config.usePassiveGain)
            {
                return;
            }

            if (config.passiveGainInterval <= 0f)
            {
                return;
            }

            passiveGainTimer += Time.deltaTime;

            while (passiveGainTimer >= config.passiveGainInterval)
            {
                passiveGainTimer -= config.passiveGainInterval;
                AddGauge(runtimeData != null ? runtimeData.GetPassiveGain() : 0);
            }
        }
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            NotifyGaugeChanged();
        }
#endif

        private void RegisterCharacterDeathEvent()
        {
            if (isCharacterDeathEventRegistered)
            {
                return;
            }

            CharacterManager.OnAnyCharacterDied += HandleCharacterDied;
            isCharacterDeathEventRegistered = true;
        }

        private void UnregisterCharacterDeathEvent()
        {
            if (!isCharacterDeathEventRegistered)
            {
                return;
            }

            CharacterManager.OnAnyCharacterDied -= HandleCharacterDied;
            isCharacterDeathEventRegistered = false;
        }

        private void HandleCharacterDied(CharacterManager characterManager)
        {
            if (characterManager == null)
            {
                return;
            }
            CharacterRuntimeData characterRuntimeData = characterManager.RuntimeData;

            if (characterRuntimeData == null || characterRuntimeData.characterSO == null)
            {
                return;
            }

            AddGaugeByCharacterType(characterRuntimeData.characterSO.characterType);
        }

        public void AddGaugeByCharacterType(CharacterType characterType)
        {
            int gainAmount = GetCharacterKillGain(characterType);
            AddGauge(gainAmount);
        }

        public void AddGauge(int amount)
        {
            if (runtimeData == null)
            {
                return;
            }

            if (runtimeData.AddGauge(amount))
            {
                NotifyGaugeChanged();
            }
        }

        public bool CanSpend(int cost)
        {
            return runtimeData != null && runtimeData.CanSpend(cost);
        }

        public bool TrySpend(int cost)
        {
            if (runtimeData == null)
            {
                return false;
            }

            int previousGauge = runtimeData.CurrentGauge;
            bool result = runtimeData.TrySpend(cost);

            if (result && previousGauge != runtimeData.CurrentGauge)
            {
                NotifyGaugeChanged();
            }

            return result;
        }

        public void ResetGauge()
        {
            if (runtimeData == null)
            {
                runtimeData = new StrategicSkillCostRuntimeData(config);
                NotifyGaugeChanged();
                return;
            }

            if (runtimeData.ResetGauge())
            {
                NotifyGaugeChanged();
            }
        }

        private int GetCharacterKillGain(CharacterType characterType)
        {
            if (runtimeData == null)
            {
                return 0;
            }

            return runtimeData.GetCharacterKillGain(characterType);
        }

        public void SetGainMultiplier(float multiplier)
        {
            if (runtimeData == null)
            {
                return;
            }

            runtimeData.SetGainMultiplier(multiplier);
            NotifyGaugeChangedIfNeeded();
        }

        public void AddFlatGainBonus(int amount)
        {
            if (runtimeData == null)
            {
                return;
            }

            runtimeData.AddFlatGainBonus(amount);
            NotifyGaugeChangedIfNeeded();
        }

        public void SetFlatGainBonus(int amount)
        {
            if (runtimeData == null)
            {
                return;
            }

            runtimeData.SetFlatGainBonus(amount);
            NotifyGaugeChangedIfNeeded();
        }

        public void SetGainBlocked(bool isBlocked)
        {
            if (runtimeData == null)
            {
                return;
            }

            runtimeData.SetGainBlocked(isBlocked);
            NotifyGaugeChangedIfNeeded();
        }

        public void ForceNotifyGaugeChanged()
        {
            NotifyGaugeChanged();
        }

        private void NotifyGaugeChanged()
        {
            lastNotifiedGauge = CurrentGauge;
            lastNotifiedMaxGauge = MaxGauge;
            OnGaugeChanged?.Invoke(CurrentGauge, MaxGauge);
        }

        private void NotifyGaugeChangedIfNeeded()
        {
            if (lastNotifiedGauge == CurrentGauge && lastNotifiedMaxGauge == MaxGauge)
            {
                return;
            }

            NotifyGaugeChanged();
        }
    }
}