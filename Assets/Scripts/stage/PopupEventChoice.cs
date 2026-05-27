using System;
using UnityEngine;
using System.Collections.Generic;
using Shrine;

namespace Stage
{
    /// <summary>
    /// 팝업 이벤트에서 플레이어가 선택할 수 있는 선택지 데이터.
    /// 실제 보상/패널티 적용은 이후 Effect 시스템과 연결한다.
    /// </summary>
    [Serializable]
    public class PopupEventChoice
    {
        [Header("Text")]
        public string choiceId;
        public string label;
        [TextArea]
        public string description;

        [Header("Result")]
        [TextArea]
        public string resultText;


        [Header("Rewards")]
        public List<PopupEventRewardData> rewards = new();

        [Header("Flow")]
        public bool completesEvent = true;

        [Tooltip("다음으로 이어질 Popup 이벤트 (선택 시 체이닝)")]
        public PopupEventSO nextEvent;

        public bool HasNextEvent => nextEvent != null;
    }

    [Serializable]
    public class PopupEventRewardData
    {
        public PopupEventRewardType rewardType;

        public int value;

        public ShrineGodType godType;

        public ScriptableObject targetData;


        public string tag;
    }
}
