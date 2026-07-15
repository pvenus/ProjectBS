using System;
using System.Collections.Generic;
using UnityEngine;

namespace Stage
{
    /// <summary>
    /// Random Section 배치 연산 시 제공되는 외부 콘텍스트 데이터.
    /// </summary>
    public class StageRandomPlacementContext
    {
        [Tooltip("현재 배치 연산을 수행 중인 스테이지의 정의 SO")]
        public StageDefinitionSO definition;

        [Tooltip("랜덤 노드 배정을 위한 난수 생성기 인스턴스")]
        public System.Random random;

        [Tooltip("이미 특정 슬롯에 고정 배정(Story 등) 또는 먼저 배정된 RoundNodeSO 맵")]
        public IReadOnlyDictionary<string, RoundNodeSO> alreadyAssigned;
    }

    /// <summary>
    /// Random Section의 슬롯들에 RoundNodeSO를 배정하기 위한 규칙의 추상 베이스 SO.
    /// </summary>
    public abstract class StageRandomPlacementRuleSO : ScriptableObject
    {
        /// <summary>
        /// 해당 section의 targetSlots(슬롯 정의 리스트)를 순회하며 적절한 RoundNodeSO를 선택하여 resultBySlotId에 대입한다.
        /// </summary>
        /// <param name="context">난수 생성기 및 이미 배정된 맵 정보를 포함하는 콘텍스트</param>
        /// <param name="section">배정을 수행하려는 대상 Random Section 규칙</param>
        /// <param name="targetSlots">이 Section에 해당하는 실제 StageMapSlot들 (기하 좌표 정보 및 뎁스 정보 포함)</param>
        /// <param name="resultBySlotId">최종 배정 결과를 저장할 Dictionary (SlotId -> RoundNodeSO)</param>
        public abstract void Fill(
            StageRandomPlacementContext context,
            StageRandomSection section,
            IReadOnlyList<StageMapSlot> targetSlots,
            Dictionary<string, RoundNodeSO> resultBySlotId);
    }
}
