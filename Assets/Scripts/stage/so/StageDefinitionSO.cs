using System.Collections.Generic;
using UnityEngine;

namespace Stage
{
    /// <summary>
    /// 스테이지 생성에 필요한 정의 데이터 (디자인 타임)
    /// </summary>
    [CreateAssetMenu(menuName = "Stage/Stage Definition")]
    public class StageDefinitionSO : ScriptableObject
    {
        [Header("Identity")]
        public string stageId;
        public string stageName;

        [Header("Structure")]
        [Tooltip("스테이지 깊이 (보스 포함)")]
        public int totalDepth = 5;

        [Tooltip("각 depth에서 생성될 노드 수 범위 (간단 버전은 고정으로 사용)")]
        public int minNodesPerDepth = 2;
        public int maxNodesPerDepth = 3;

        [Header("Pools")]
        [Tooltip("일반 전투 풀")]
        public List<RoundNodeSO> battlePool = new();

        [Tooltip("이벤트 풀")]
        public List<RoundNodeSO> eventPool = new();

        [Header("Required")]
        [Tooltip("필수 서브 이벤트 (조건 없이 삽입되는 노드)")]
        public List<RoundNodeSO> requiredSubEvents = new();

        [Header("Boss")]
        public RoundNodeSO bossNode;

        [Header("Start")]
        public RoundNodeSO startNode;

        [Header("Debug")]
        public bool useFixedSeed = false;
        public int seed = 0;
    }
}
