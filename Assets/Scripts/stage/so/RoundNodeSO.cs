using UnityEngine;
using System.Collections.Generic;
using String;

namespace Stage
{
    public enum RoundNodeConditionType
    {
        None,
        HasCharacter,
        HasEquipment,
        HasRelic,
        HasItem,
        HasFaith,
        HasBless
    }

    [System.Serializable]
    public class RoundNodeCondition
    {
        public RoundNodeConditionType conditionType = RoundNodeConditionType.None;

        [Tooltip("조건 체크에 사용할 대상 ID. 예: character.jihan, equipment.xxx, relic.xxx")]
        public string targetId;

        [Tooltip("true면 조건을 반대로 체크한다. 예: 특정 캐릭터를 보유하지 않아야 등장")]
        public bool invert;
    }

    /// <summary>
    /// 노드 템플릿 데이터 (디자인 타임)
    /// 실제 런타임 RoundNode를 생성하기 위한 원본 데이터
    /// </summary>
    [CreateAssetMenu(menuName = "Stage/Round Node")]
    public class RoundNodeSO : ScriptableObject
    {
        [Header("Identity")]
        public string nodeId;

        public string LocalizationMainKey => nodeId;

        public string Title =>
            StringManager.Instance.Get(
                LocalizationMainKey,
                "title");

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

        [Header("Appearance Conditions")]
        [Tooltip("모든 조건을 만족해야 이 노드가 랜덤 풀 후보로 등장한다.")]
        public List<RoundNodeCondition> appearanceConditions = new();
    }
}