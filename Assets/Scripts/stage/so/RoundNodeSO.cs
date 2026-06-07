using UnityEngine;
using System.Collections.Generic;

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

        [Header("Type")]
        public RoundNodeType nodeType = RoundNodeType.None;

        [Header("Execute Payload")]

        [Tooltip("Popup 이벤트 실행 시 사용할 ScriptableObject (eventId 대신 직접 참조 가능)")]
        public PopupEventSO popupEvent;


        [Header("Flags")]
        public bool isRequired;

        [Tooltip("기본적으로 숨겨진 노드 여부")]
        public bool hiddenByDefault;

        [Header("Tags")]
        public List<string> tags = new();
    }
}