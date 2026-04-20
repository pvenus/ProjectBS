

using System;

namespace Status.Dto
{
    /// <summary>
    /// Heat 상태를 직렬화/전달하기 위한 DTO
    /// MonoBehaviour와 분리된 순수 데이터 객체
    /// </summary>
    [Serializable]
    public class HeatStateDto
    {
        /// <summary>현재 Heat 값</summary>
        public float currentHeat;

        /// <summary>최대 Heat 값</summary>
        public float maxHeat;

        /// <summary>Heat 유지 시간 (남은 시간)</summary>
        public float remainingDuration;

        /// <summary>초당 감소량 (선택)</summary>
        public float decayPerSecond;

        /// <summary>Overheat 상태 여부</summary>
        public bool isOverheated;

        public HeatStateDto() { }

        public HeatStateDto(float currentHeat, float maxHeat, float duration, float decay)
        {
            this.currentHeat = currentHeat;
            this.maxHeat = maxHeat;
            this.remainingDuration = duration;
            this.decayPerSecond = decay;
            this.isOverheated = currentHeat >= maxHeat;
        }

        /// <summary>
        /// Heat 비율 (0~1)
        /// </summary>
        public float GetNormalized()
        {
            if (maxHeat <= 0f) return 0f;
            return Math.Clamp(currentHeat / maxHeat, 0f, 1f);
        }

        /// <summary>
        /// Heat 초기화
        /// </summary>
        public void Reset()
        {
            currentHeat = 0f;
            isOverheated = false;
        }

        /// <summary>
        /// Heat 갱신 (외부에서 값 세팅 시 사용)
        /// </summary>
        public void UpdateHeat(float value)
        {
            currentHeat = Math.Clamp(value, 0f, maxHeat);
            isOverheated = currentHeat >= maxHeat;
        }
    }
}