using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Stage
{
    [Serializable]
    public class StageRandomPoolEntry
    {
        [Tooltip("랜덤 노드를 뽑아낼 풀 에셋")]
        public EventPoolSO pool;

        [Range(1, 1000)]
        [Tooltip("이 풀이 선택될 상대적 가중치")]
        public int weight = 100;
    }

    [CreateAssetMenu(
        fileName = "WeightedPoolPlacementRule",
        menuName = "Stage/Weighted Pool Placement Rule")]
    public class WeightedPoolPlacementRuleSO : StageRandomPlacementRuleSO
    {
        [Header("Pool Config")]
        [Tooltip("사용할 노드 풀 및 가중치 목록")]
        public List<StageRandomPoolEntry> pools = new();

        [Header("Duplication Options")]
        [Tooltip("true면 이 Section 내부에서 중복된 RoundNodeSO가 배치되지 않도록 방지합니다.")]
        public bool avoidDuplicateInSection = true;

        [Tooltip("true면 노드 픽에 실패(또는 풀이 비었을 때)하더라도 슬롯을 비워두는 것을 허용합니다. false면 경고를 출력합니다.")]
        public bool allowEmptySlot = false;

        public override void Fill(
            StageRandomPlacementContext context,
            StageRandomSection section,
            IReadOnlyList<StageMapSlot> targetSlots,
            Dictionary<string, RoundNodeSO> resultBySlotId)
        {
            if (pools == null || pools.Count == 0)
            {
                Debug.LogWarning($"[WeightedPoolPlacementRuleSO] Pools list is empty on {name}.");
                return;
            }

            // 이 섹션 내에서 이미 이번 규칙 수행 중 배정된 노드 목록 (중복 체크용)
            var sectionAssignedNodes = new HashSet<RoundNodeSO>();

            foreach (var slot in targetSlots)
            {
                RoundNodeSO selectedNode = PickNodeForSlot(context, slot, sectionAssignedNodes);

                if (selectedNode != null)
                {
                    resultBySlotId[slot.slotId] = selectedNode;
                    if (avoidDuplicateInSection)
                    {
                        sectionAssignedNodes.Add(selectedNode);
                    }
                }
                else
                {
                    if (!allowEmptySlot)
                    {
                        Debug.LogWarning(
                            $"[WeightedPoolPlacementRuleSO] Failed to assign RoundNodeSO to slot '{slot.slotId}' " +
                            $"in section '{section.sectionId}' (Depth: {slot.depth}). No valid node found in pools.");
                    }
                }
            }
        }

        private RoundNodeSO PickNodeForSlot(
            StageRandomPlacementContext context,
            StageMapSlot slot,
            HashSet<RoundNodeSO> sectionAssignedNodes)
        {
            // 1. 유효한 풀 엔트리 필터링
            var validPoolEntries = pools
                .Where(p => p.pool != null && p.weight > 0)
                .ToList();

            if (validPoolEntries.Count == 0)
                return null;

            // 가중치 픽을 위해 풀 복사본 사용 (특정 풀이 실패할 경우 다른 풀 재시도를 위함)
            var remainingPools = new List<StageRandomPoolEntry>(validPoolEntries);

            while (remainingPools.Count > 0)
            {
                // 2. 가중치 기반으로 풀 선택
                int totalPoolWeight = remainingPools.Sum(p => p.weight);
                int poolRandVal = context.random.Next(0, totalPoolWeight);
                int poolAccum = 0;
                StageRandomPoolEntry selectedPoolEntry = null;

                foreach (var p in remainingPools)
                {
                    poolAccum += p.weight;
                    if (poolRandVal < poolAccum)
                    {
                        selectedPoolEntry = p;
                        break;
                    }
                }

                if (selectedPoolEntry == null)
                    break;

                // 3. 선택된 풀에서 뎁스 조건에 맞는 노드 후보 추출
                var availableEntries = selectedPoolEntry.pool.GetAvailableEntries(slot.depth);

                // 중복 방지가 켜져 있으면, 이번 섹션에 이미 배정된 노드는 후보군에서 제외
                if (avoidDuplicateInSection && sectionAssignedNodes.Count > 0)
                {
                    availableEntries = availableEntries
                        .Where(e => !sectionAssignedNodes.Contains(e.node))
                        .ToList();
                }

                if (availableEntries.Count > 0)
                {
                    // 4. 가중치 기반으로 노드 선택
                    int totalNodeWeight = availableEntries.Sum(e => e.weight);
                    int nodeRandVal = context.random.Next(0, totalNodeWeight);
                    int nodeAccum = 0;

                    foreach (var entry in availableEntries)
                    {
                        nodeAccum += entry.weight;
                        if (nodeRandVal < nodeAccum)
                        {
                            return entry.node;
                        }
                    }
                }

                // 이 풀에서 적절한 노드를 찾을 수 없다면, 남은 풀 목록에서 이 풀을 제거하고 다른 풀에서 재시도
                remainingPools.Remove(selectedPoolEntry);
            }

            return null;
        }
    }
}
