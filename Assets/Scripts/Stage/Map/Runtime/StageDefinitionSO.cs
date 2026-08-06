using System.Collections.Generic;
using UnityEngine;

namespace Stage
{
    /// <summary>
    /// SVG SlotMap 생성에 필요한 스테이지 정의 데이터.
    /// </summary>
    [CreateAssetMenu(menuName = "Stage/Stage Definition")]
    public class StageDefinitionSO : ScriptableObject
    {
        [Header("Identity")]
        public string stageId;
        public string stageName;

        [Header("SVG Slot Map")]
        [Tooltip("SVG에서 추출된 슬롯 목록입니다. depth, orderInDepth, role, label, connections를 포함합니다.")]
        public List<StageMapSlot> svgMapSlots = new();

        [Header("SVG Story Bindings")]
        [Tooltip("Story 슬롯에 고정 배치할 RoundNodeSO 매칭 목록입니다.")]
        public List<StageStorySlotBinding> svgStorySlotBindings = new();

        [Header("SVG Random Sections")]
        [Tooltip("Story 슬롯 사이의 Random 슬롯 묶음과 배치 규칙 목록입니다.")]
        public List<StageRandomSection> svgRandomSections = new();

        [Header("Debug")]
        public bool useFixedSeed;
        public int seed;
    }
}
