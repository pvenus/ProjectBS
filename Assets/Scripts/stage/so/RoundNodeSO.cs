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

        [Header("Icon")]
        [Tooltip(
            "true일 때만 iconType 값을 직접 사용한다.\n" +
            "false(기본값)이면 popupEvent의 Choice Execution Config를 분석해\n" +
            "아이콘 타입을 자동으로 결정한다.")]
        public bool overrideIconType = false;

        [Tooltip(
            "overrideIconType이 true일 때만 이 값이 적용된다.\n" +
            "overrideIconType이 false이면 popupEvent 분석 결과가 우선한다.")]
        public NodeIconType iconType = NodeIconType.None;

        /// <summary>
        /// 이 RoundNodeSO의 실제 NodeIconType을 반환한다.
        /// overrideIconType == true이고 iconType != None이면 iconType을 반환한다.
        /// 그 외에는 popupEvent의 Choice Execution Config를 분석해 자동으로 결정한다.
        /// popupEvent가 없거나 분석에 실패하면 iconType을 fallback으로 사용한다.
        /// </summary>
        public NodeIconType GetResolvedIconType()
        {
            if (overrideIconType && iconType != NodeIconType.None)
            {
                return iconType;
            }

            if (popupEvent != null)
            {
                NodeIconType autoResolved =
                    ChoiceExecutionIconResolver.ResolveIconType(
                        popupEvent,
                        fallback: NodeIconType.None);

                if (autoResolved != NodeIconType.None)
                {
                    return autoResolved;
                }
            }

            // fallback: 인스펙터에 지정된 iconType 또는 None
            return iconType;
        }

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