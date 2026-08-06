using System;
using Character;

namespace Battle
{
    /// <summary>
    /// 전략 스킬 코스트의 전투 중 런타임 상태를 가진다.
    /// Config는 기본값을 담당하고, RuntimeData는 버프/디버프/충전 제한 같은 변동값을 담당한다.
    /// </summary>
    [Serializable]
    public class StrategicSkillCostRuntimeData
    {
        private readonly StrategicSkillCostConfigSO config;

        [UnityEngine.SerializeField] private int currentGauge;
        [UnityEngine.SerializeField] private float gainMultiplier = 1f;
        [UnityEngine.SerializeField] private int flatGainBonus;
        [UnityEngine.SerializeField] private bool isGainBlocked;

        public int CurrentGauge => currentGauge;
        public float GainMultiplier => gainMultiplier;
        public int FlatGainBonus => flatGainBonus;
        public bool IsGainBlocked => isGainBlocked;

        public int MaxGauge => config != null ? config.maxGauge : 100;
        public int InitialGauge => config != null ? config.initialGauge : 0;
        public float GaugeRate => MaxGauge <= 0 ? 0f : (float)currentGauge / MaxGauge;

        public StrategicSkillCostRuntimeData(StrategicSkillCostConfigSO config)
        {
            this.config = config;
            currentGauge = ClampGauge(InitialGauge);
        }

        public int GetCharacterKillGain(CharacterType characterType)
        {
            if (config == null || isGainBlocked)
            {
                return 0;
            }

            int baseGain = config.GetCharacterKillGain(characterType);
            return CalculateFinalGain(baseGain);
        }

        public int GetPassiveGain()
        {
            if (config == null || isGainBlocked)
            {
                return 0;
            }

            return CalculateFinalGain(config.passiveGainAmount);
        }

        public bool CanSpend(int cost)
        {
            if (cost <= 0)
            {
                return true;
            }

            return currentGauge >= cost;
        }

        public bool TrySpend(int cost)
        {
            if (!CanSpend(cost))
            {
                return false;
            }

            if (cost <= 0)
            {
                return true;
            }

            currentGauge = ClampGauge(currentGauge - cost);
            return true;
        }

        public bool AddGauge(int amount)
        {
            if (amount <= 0 || isGainBlocked)
            {
                return false;
            }

            int previousGauge = currentGauge;
            currentGauge = ClampGauge(currentGauge + amount);
            return previousGauge != currentGauge;
        }

        public bool ResetGauge()
        {
            int previousGauge = currentGauge;
            currentGauge = ClampGauge(InitialGauge);
            return previousGauge != currentGauge;
        }

        public void SetGainMultiplier(float multiplier)
        {
            gainMultiplier = multiplier < 0f ? 0f : multiplier;
        }

        public void AddFlatGainBonus(int amount)
        {
            flatGainBonus += amount;
        }

        public void SetFlatGainBonus(int amount)
        {
            flatGainBonus = amount;
        }

        public void SetGainBlocked(bool isBlocked)
        {
            isGainBlocked = isBlocked;
        }

        private int CalculateFinalGain(int baseGain)
        {
            if (baseGain <= 0)
            {
                return 0;
            }

            float multipliedGain = baseGain * gainMultiplier;
            int finalGain = UnityEngine.Mathf.RoundToInt(multipliedGain) + flatGainBonus;
            return finalGain < 0 ? 0 : finalGain;
        }

        private int ClampGauge(int value)
        {
            return UnityEngine.Mathf.Clamp(value, 0, MaxGauge);
        }
    }
}