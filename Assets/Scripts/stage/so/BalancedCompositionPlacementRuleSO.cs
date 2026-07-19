using System;
using System.Collections.Generic;
using UnityEngine;

namespace Stage
{
    [CreateAssetMenu(
        fileName = "BalancedCompositionPlacementRule",
        menuName = "Stage/Balanced Composition Placement Rule")]
    public class BalancedCompositionPlacementRuleSO : StageRandomPlacementRuleSO
    {
        [Header("Composition Ratios (Example)")]
        [Range(0, 100)] public int battleRatio = 40;
        [Range(0, 100)] public int eventRatio = 30;
        [Range(0, 100)] public int shopRatio = 15;
        [Range(0, 100)] public int restRatio = 15;

        public override void Fill(
            StageRandomPlacementContext context,
            StageRandomSection section,
            IReadOnlyList<StageMapSlot> targetSlots,
            Dictionary<string, RoundNodeSO> resultBySlotId)
        {
            // TODO: Section 내 전체 targetSlotIds의 개수를 분석하고, 비율(battle/event/shop/rest 등)에 맞춰
            // 균형 잡힌 노드 조성을 계산한 뒤 셔플하여 각 슬롯에 고르게 배치하는 연산을 구현해야 합니다.
            // 
            // 힌트:
            // 1. 필요한 노드 타입별 수량을 계산 (예: 슬롯 5개 중 전투 2, 이벤트 2, 상점 1 등)
            // 2. 각 타입 풀에서 적절한 RoundNodeSO를 수집
            // 3. 수집된 노드 목록을 무작위로 섞은(Shuffle) 후 순서대로 targetSlots에 할당
            
            Debug.LogWarning($"[BalancedCompositionPlacementRuleSO] '{name}' Fill is not fully implemented yet (Stub/TODO).");
        }
    }
}
