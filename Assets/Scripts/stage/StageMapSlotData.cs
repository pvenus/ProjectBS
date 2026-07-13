using System;
using System.Collections.Generic;
using UnityEngine;

namespace Stage
{
    /// <summary>
    /// SVG 맵에서 슬롯의 역할을 구분한다.
    /// Story: 에피소드/고정 스토리 노드가 배치되는 자리
    /// Random: 랜덤 이벤트 노드가 배치되는 자리
    /// </summary>
    public enum StageMapSlotRole
    {
        Story,
        Random
    }

    /// <summary>
    /// 하나의 슬롯이 다음 슬롯으로 연결되는 단방향 관계를 나타낸다.
    /// </summary>
    [Serializable]
    public class StageSlotConnection
    {
        [Tooltip("연결 대상 슬롯의 slotId")]
        public string toSlotId;
    }

    /// <summary>
    /// SVG 맵에서 추출된 하나의 슬롯(자리) 데이터.
    /// 슬롯은 어떤 RoundNodeSO가 배치될지 직접 알지 않는다.
    /// 배치 결정은 StageStorySlotBinding 또는 StageRandomSection이 담당한다.
    /// </summary>
    [Serializable]
    public class StageMapSlot
    {
        [Tooltip("슬롯 고유 식별자. SVG 좌표 기반으로 자동 부여된다. 예: ep_1, slot_2085_430")]
        public string slotId;

        [Tooltip("슬롯의 역할. Story = 고정 에피소드 노드, Random = 랜덤 이벤트 노드")]
        public StageMapSlotRole role;

        [Tooltip("맵 진행 방향 기준 깊이 (0이 시작)")]
        public int depth;

        [Tooltip("같은 depth 내에서의 순서 (x 좌표 오름차순)")]
        public int orderInDepth;

        [Tooltip("SVG에서 추출된 대표 텍스트 라벨. 예: Episode 1, ?")]
        public string label;

        [Tooltip("SVG에서 추출된 보조 텍스트. 예: 청운촌의 습격 (스토리 노드 전용)")]
        public string subLabel;

        [Tooltip("다음 슬롯으로의 단방향 연결 목록")]
        public List<StageSlotConnection> connections = new();
    }

    /// <summary>
    /// Story 슬롯에 배치할 RoundNodeSO를 명시적으로 매칭한다.
    /// SVG 파싱 후 에디터 툴이 자동으로 채우며, 수동 교체도 가능하다.
    /// </summary>
    [Serializable]
    public class StageStorySlotBinding
    {
        [Tooltip("매칭 대상 슬롯 ID. StageMapSlot.slotId와 일치해야 한다.")]
        public string slotId;

        [Tooltip("예상 RoundNodeSO의 nodeId. 자동 매칭 기준값으로 사용되며 검증용으로도 활용된다.")]
        public string expectedNodeId;

        [Tooltip("실제 배치될 RoundNodeSO 에셋")]
        public RoundNodeSO node;
    }

    /// <summary>
    /// Story 슬롯과 다음 Story 슬롯 사이에 위치한 Random 슬롯들의 묶음.
    /// 각 섹션은 독립적인 PlacementRule을 가지며, 랜덤 노드 배치를 결정한다.
    /// </summary>
    [Serializable]
    public class StageRandomSection
    {
        [Tooltip("구간 고유 식별자. 예: sec_ep2_to_ep3_1")]
        public string sectionId;

        [Tooltip("이 구간이 시작되는 Story 슬롯 ID")]
        public string fromStorySlotId;

        [Tooltip("이 구간이 끝나는 Story 슬롯 ID")]
        public string toStorySlotId;

        [Tooltip("이 구간에 포함된 모든 Random 슬롯 ID 목록")]
        public List<string> targetSlotIds = new();

        [Tooltip("이 구간에 적용할 랜덤 노드 배치 규칙 ScriptableObject")]
        public StageRandomPlacementRuleSO placementRule;
    }
}
