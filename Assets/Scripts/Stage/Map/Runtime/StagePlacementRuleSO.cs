using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Stage
{
    public enum StagePlacementRuleMode
    {
        WeightedPool = 0,
        BalancedComposition = 1
    }

    [Serializable]
    public class StagePlacementPoolEntry
    {
        [Tooltip("랜덤 노드를 선택할 이벤트 풀")]
        public EventPoolSO pool;

        [Range(1, 1000)]
        [Tooltip("이 풀을 선택할 상대 가중치")]
        public int weight = 100;
    }

    [Serializable]
    public class WeightedPoolPlacementConfig
    {
        [Tooltip("사용할 이벤트 풀과 가중치 목록")]
        public List<StagePlacementPoolEntry> pools = new();

        [Tooltip("같은 Random Section 안에서 동일한 RoundNodeSO의 중복 배치를 방지합니다.")]
        public bool avoidDuplicateInSection = true;

        [Tooltip("노드 선택 실패 시 슬롯을 비워 두는 것을 허용합니다.")]
        public bool allowEmptySlot;
    }

    [Serializable]
    public class BalancedCompositionPlacementConfig
    {
        [Range(0, 100)] public int battleRatio = 40;
        [Range(0, 100)] public int eventRatio = 30;
        [Range(0, 100)] public int shopRatio = 15;
        [Range(0, 100)] public int restRatio = 15;
    }

    /// <summary>
    /// SVG Random Section의 슬롯 배치 전략을 한 에셋 타입으로 관리한다.
    /// mode에 따라 대응하는 config와 실행 로직을 선택한다.
    /// </summary>
    [CreateAssetMenu(fileName = "StagePlacementRule", menuName = "Stage/Placement Rule")]
    public class StagePlacementRuleSO : ScriptableObject
    {
        [Header("Rule Mode")]
        public StagePlacementRuleMode mode = StagePlacementRuleMode.WeightedPool;

        [Header("Weighted Pool Config")]
        public WeightedPoolPlacementConfig weightedPool = new();

        [Header("Balanced Composition Config")]
        public BalancedCompositionPlacementConfig balancedComposition = new();

        public void Fill(
            StageRandomSection section,
            IReadOnlyList<StageMapSlot> targetSlots,
            Dictionary<string, RoundNodeSO> resultBySlotId,
            System.Random random)
        {
            switch (mode)
            {
                case StagePlacementRuleMode.WeightedPool:
                    FillWeightedPool(section, targetSlots, resultBySlotId, random);
                    break;

                case StagePlacementRuleMode.BalancedComposition:
                    FillBalancedComposition(section, targetSlots, resultBySlotId, random);
                    break;

                default:
                    Debug.LogError($"[StagePlacementRuleSO:{name}] Unsupported placement mode: {mode}.");
                    break;
            }
        }

        private void FillWeightedPool(
            StageRandomSection section,
            IReadOnlyList<StageMapSlot> targetSlots,
            Dictionary<string, RoundNodeSO> resultBySlotId,
            System.Random random)
        {
            if (section == null)
            {
                Debug.LogError($"[StagePlacementRuleSO:{name}] section is null.");
                return;
            }

            if (targetSlots == null || targetSlots.Count == 0)
            {
                Debug.LogWarning($"[StagePlacementRuleSO:{name}] targetSlots is empty for section '{section.sectionId}'.");
                return;
            }

            if (resultBySlotId == null)
            {
                Debug.LogError($"[StagePlacementRuleSO:{name}] resultBySlotId is null.");
                return;
            }

            if (random == null)
            {
                Debug.LogError($"[StagePlacementRuleSO:{name}] random is null.");
                return;
            }

            WeightedPoolPlacementConfig config = weightedPool ?? new WeightedPoolPlacementConfig();
            List<StagePlacementPoolEntry> validPools = config.pools?
                .Where(entry => entry != null && entry.pool != null && entry.weight > 0)
                .ToList() ?? new List<StagePlacementPoolEntry>();

            if (validPools.Count == 0)
            {
                Debug.LogWarning($"[StagePlacementRuleSO:{name}] No valid pool entries for section '{section.sectionId}'.");
                return;
            }

            var sectionAssigned = new HashSet<RoundNodeSO>();
            foreach (StageMapSlot slot in targetSlots)
            {
                if (slot == null || string.IsNullOrWhiteSpace(slot.slotId))
                {
                    Debug.LogWarning($"[StagePlacementRuleSO:{name}] Invalid target slot in section '{section.sectionId}'.");
                    continue;
                }

                if (resultBySlotId.ContainsKey(slot.slotId))
                {
                    continue;
                }

                RoundNodeSO selectedNode = PickWeightedNode(
                    slot,
                    validPools,
                    sectionAssigned,
                    config.avoidDuplicateInSection,
                    random);

                if (selectedNode != null)
                {
                    resultBySlotId[slot.slotId] = selectedNode;
                    if (config.avoidDuplicateInSection)
                    {
                        sectionAssigned.Add(selectedNode);
                    }
                }
                else if (!config.allowEmptySlot)
                {
                    Debug.LogWarning(
                        $"[StagePlacementRuleSO:{name}] Failed to assign slot '{slot.slotId}' " +
                        $"in section '{section.sectionId}' at depth {slot.depth}.");
                }
            }
        }

        private void FillBalancedComposition(
            StageRandomSection section,
            IReadOnlyList<StageMapSlot> targetSlots,
            Dictionary<string, RoundNodeSO> resultBySlotId,
            System.Random random)
        {
            Debug.LogWarning(
                $"[StagePlacementRuleSO:{name}] BalancedComposition mode is not implemented yet. " +
                $"Section '{section?.sectionId ?? "<null>"}' was not filled.");
        }

        private static RoundNodeSO PickWeightedNode(
            StageMapSlot slot,
            IReadOnlyList<StagePlacementPoolEntry> validPools,
            HashSet<RoundNodeSO> sectionAssigned,
            bool avoidDuplicateInSection,
            System.Random random)
        {
            var remainingPools = new List<StagePlacementPoolEntry>(validPools);
            while (remainingPools.Count > 0)
            {
                int totalPoolWeight = remainingPools.Sum(entry => entry.weight);
                int poolRoll = random.Next(totalPoolWeight);
                int accumulatedPoolWeight = 0;
                StagePlacementPoolEntry chosenPool = null;

                foreach (StagePlacementPoolEntry entry in remainingPools)
                {
                    accumulatedPoolWeight += entry.weight;
                    if (poolRoll < accumulatedPoolWeight)
                    {
                        chosenPool = entry;
                        break;
                    }
                }

                if (chosenPool == null)
                {
                    break;
                }

                List<EventPoolEntry> candidates = chosenPool.pool
                    .GetAvailableEntries(slot.depth)
                    .Where(entry => entry.node != null && entry.weight > 0)
                    .Where(entry => !avoidDuplicateInSection || !sectionAssigned.Contains(entry.node))
                    .ToList();

                if (candidates.Count > 0)
                {
                    int totalNodeWeight = candidates.Sum(entry => entry.weight);
                    int nodeRoll = random.Next(totalNodeWeight);
                    int accumulatedNodeWeight = 0;

                    foreach (EventPoolEntry entry in candidates)
                    {
                        accumulatedNodeWeight += entry.weight;
                        if (nodeRoll < accumulatedNodeWeight)
                        {
                            return entry.node;
                        }
                    }
                }

                remainingPools.Remove(chosenPool);
            }

            return null;
        }
    }
}
