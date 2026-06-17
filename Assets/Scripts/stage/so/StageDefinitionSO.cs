using System.Collections.Generic;
using UnityEngine;

namespace Stage
{
    [System.Serializable]
    public class StageRequiredNode
    {
        public RoundNodeSO node;
        public int depth;
        public int column;
        public bool hiddenByDefault;
    }

    /// <summary>
    /// 스테이지 생성에 필요한 정의 데이터 (디자인 타임)
    /// </summary>
    [CreateAssetMenu(menuName = "Stage/Stage Definition")]
    public class StageDefinitionSO : ScriptableObject
    {
        [Header("Identity")]
        public string stageId;
        public string stageName;

        [Header("Segments")]
        [Tooltip("핵심 노드 사이에 생성할 랜덤 구간 규칙")]
        public List<StageSegmentRule> segmentRules = new();


        [Header("Required / Main Story")]
        [Tooltip("무조건 지나가야 하는 메인 스토리 노드. StageDefinitionSO에서 depth/column으로 배치한다.")]
        public List<StageRequiredNode> requiredSubEvents = new();


        [Header("Debug")]
        public bool useFixedSeed = false;
        public int seed = 0;
    }
}
