using System;
using System.Collections.Generic;
using UnityEngine;

namespace Stage
{
    [Serializable]
    public class StageSegmentPoolRule
    {
        [Tooltip("이 구간에서 사용할 이벤트 풀")]
        public EventPoolSO pool;

        [Tooltip("true면 이 풀에서 최소 1개 이상 등장하도록 우선 배치한다.")]
        public bool required;

        [Tooltip("이 구간 전체에서 이 풀을 최대 몇 번 사용할지. 0 이하이면 제한 없음.")]
        public int maxAppearCount = 1;

        [Tooltip("필수 풀 배치 순서. 낮을수록 먼저 배치한다.")]
        public int priority;
    }

    [Serializable]
    public class StageSegmentRule
    {
        [Header("Connection")]
        [Tooltip("구간 시작 핵심 노드의 fixedDepth")]
        public int fromDepth;

        [Tooltip("구간 시작 핵심 노드의 fixedColumn")]
        public int fromColumn;

        [Tooltip("구간 종료 핵심 노드의 fixedDepth")]
        public int toDepth;

        [Tooltip("구간 종료 핵심 노드의 fixedColumn")]
        public int toColumn;

        [Header("Total Weight")]
        public int minTotalWeight = 90;
        public int maxTotalWeight = 110;

        [Header("Layers")]
        public int minLayerCount = 2;
        public int maxLayerCount = 4;

        public int minLayerWeight = 15;
        public int maxLayerWeight = 60;

        [Header("Nodes Per Layer")]
        public int minNodesPerLayer = 1;
        public int maxNodesPerLayer = 3;

        [Header("Pools")]
        public List<StageSegmentPoolRule> pools = new();
    }

    [Serializable]
    public class StageRandomRouteRule
    {
        public int startDepth;
        public int endDepth;

        public int minBranchesPerRoute = 1;
        public int maxBranchesPerRoute = 1;

        public List<StageSegmentPoolRule> pools = new();

        public int minTotalWeight;
        public int maxTotalWeight;
    }
}
