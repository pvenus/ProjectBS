using UnityEngine;
using Status.Dto;

namespace Status.Resolver
{
    /// <summary>
    /// Heat 관련 계산 전용 Resolver.
    /// 실제 상태 저장은 DTO / Mono가 담당하고,
    /// 이 클래스는 수치 계산과 판정만 담당한다.
    /// </summary>
    public static class HeatResolver
    {
        /// <summary>
        /// Heat 비율과 기본 데미지 기준의 추가 데미지를 계산한다.
        /// 공식: baseDamage × heatNormalized × coefficient
        /// </summary>
        public static float CalculateBonusDamage(HeatStateDto heatState, float baseDamage, float coefficient)
        {
            if (heatState == null || baseDamage <= 0f || coefficient <= 0f || heatState.currentHeat <= 0f)
            {
                return 0f;
            }

            float heatNormalized = CalculateHeatNormalized(heatState);
            return baseDamage * heatNormalized * coefficient;
        }

        /// <summary>
        /// 평타(기본 공격)용 추가 데미지 계산.
        /// Heat 비율 기반 + 선택적 기본 보너스를 더한다.
        /// </summary>
        public static float CalculateBasicAttackBonusDamage(
            HeatStateDto heatState,
            float baseDamage,
            float coefficient,
            float baseBonus = 0f)
        {
            if (heatState == null)
            {
                return baseBonus;
            }

            float heatBonus = CalculateBonusDamage(heatState, baseDamage, coefficient);
            return baseBonus + heatBonus;
        }

        /// <summary>
        /// 평타 최종 데미지 계산 (기본 데미지 + Heat 비율 기반 추가 데미지)
        /// </summary>
        public static float CalculateBasicAttackTotalDamage(
            float baseDamage,
            HeatStateDto heatState,
            float coefficient,
            float baseBonus = 0f)
        {
            float bonus = CalculateBasicAttackBonusDamage(heatState, baseDamage, coefficient, baseBonus);
            return baseDamage + bonus;
        }

        /// <summary>
        /// Heat 증가량을 계산한다.
        /// 현재는 입력값을 그대로 사용하지만,
        /// 추후 등급/버프/디버프 보정이 들어갈 수 있도록 별도 함수로 분리한다.
        /// </summary>
        public static float CalculateHeatGain(float baseHeatGain, float multiplier = 1f, float flatBonus = 0f)
        {
            if (baseHeatGain <= 0f)
            {
                return 0f;
            }

            float result = (baseHeatGain * Mathf.Max(0f, multiplier)) + flatBonus;
            return Mathf.Max(0f, result);
        }

        /// <summary>
        /// Heat 추가 적용 후 예상 Heat 값을 계산한다.
        /// 상태를 직접 바꾸지 않고 결과만 반환한다.
        /// </summary>
        public static float CalculateNextHeat(HeatStateDto heatState, float heatGain)
        {
            if (heatState == null)
            {
                return 0f;
            }

            return Mathf.Clamp(heatState.currentHeat + Mathf.Max(0f, heatGain), 0f, heatState.maxHeat);
        }

        /// <summary>
        /// Heat가 최대치에 도달하여 Overheat 폭발 조건을 만족하는지 확인한다.
        /// </summary>
        public static bool CanTriggerOverheat(HeatStateDto heatState)
        {
            if (heatState == null)
            {
                return false;
            }

            return heatState.currentHeat >= heatState.maxHeat;
        }

        /// <summary>
        /// Heat 증가 후 Overheat에 도달하는지 미리 확인한다.
        /// 상태를 적용하기 전에 예측 판정할 때 사용한다.
        /// </summary>
        public static bool WillTriggerOverheat(HeatStateDto heatState, float heatGain)
        {
            if (heatState == null)
            {
                return false;
            }

            return CalculateNextHeat(heatState, heatGain) >= heatState.maxHeat;
        }

        /// <summary>
        /// Overheat 폭발 데미지를 계산한다.
        /// 현재는 maxHeat 기준의 단순 계수 방식으로 두고,
        /// 추후 현재 Heat, 스킬 계수, 등급 보너스를 섞어 확장할 수 있다.
        /// </summary>
        public static float CalculateOverheatExplosionDamage(HeatStateDto heatState, float explosionCoefficient)
        {
            if (heatState == null || explosionCoefficient <= 0f)
            {
                return 0f;
            }

            float effectiveHeat = Mathf.Min(heatState.currentHeat, heatState.maxHeat);
            return effectiveHeat * explosionCoefficient;
        }

        /// <summary>
        /// Heat 감소 후 남는 값을 계산한다.
        /// </summary>
        public static float CalculateReducedHeat(HeatStateDto heatState, float amount)
        {
            if (heatState == null)
            {
                return 0f;
            }

            return Mathf.Clamp(heatState.currentHeat - Mathf.Max(0f, amount), 0f, heatState.maxHeat);
        }

        /// <summary>
        /// Heat 정규화 비율을 반환한다. (0~1)
        /// </summary>
        public static float CalculateHeatNormalized(HeatStateDto heatState)
        {
            if (heatState == null || heatState.maxHeat <= 0f)
            {
                return 0f;
            }

            return Mathf.Clamp01(heatState.currentHeat / heatState.maxHeat);
        }
    }
}