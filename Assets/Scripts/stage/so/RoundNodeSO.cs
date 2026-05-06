

using UnityEngine;

namespace Stage
{
    /// <summary>
    /// 노드 템플릿 데이터 (디자인 타임)
    /// 실제 런타임 RoundNode를 생성하기 위한 원본 데이터
    /// </summary>
    [CreateAssetMenu(menuName = "Stage/Round Node")]
    public class RoundNodeSO : ScriptableObject
    {
        [Header("Identity")]
        public string nodeId;
        public string title;
        [TextArea]
        public string description;

        [Header("Type")]
        public RoundNodeType nodeType = RoundNodeType.None;
        public RoundExecuteMode executeMode = RoundExecuteMode.None;

        [Header("Execute Payload")]
        [Tooltip("씬 전환 시 사용할 씬 이름")]
        public string sceneName;

        [Tooltip("이벤트 ID (팝업/시나리오) ")]
        public string eventId;

        [Tooltip("Popup 이벤트 실행 시 사용할 ScriptableObject (eventId 대신 직접 참조 가능)")]
        public PopupEventSO popupEvent;

        [Tooltip("전투 그룹 ID")]
        public string battleGroupId;

        [Header("Spawn Condition")]
        [Tooltip("이 노드가 등장 가능한 최소 depth")]
        public int minDepth = 0;

        [Tooltip("이 노드가 등장 가능한 최대 depth")]
        public int maxDepth = 999;

        [Header("Weight")]
        [Tooltip("랜덤 선택 가중치")]
        public int weight = 100;

        [Header("Flags")]
        public bool isRequired;

        [Header("Visual")]
        public Sprite icon;
    }
}