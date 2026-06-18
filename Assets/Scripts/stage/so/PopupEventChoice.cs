using System;
using UnityEngine;
using System.Collections.Generic;
using Shrine;
using String;

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

        public string LocalizationMainKey => choiceId;

        public string Label =>
            StringManager.Instance.Get(
                LocalizationMainKey,
                "label");

        public string Description =>
            StringManager.Instance.Get(
                LocalizationMainKey,
                "description");

        [Header("Result")]
        public string ResultText =>
            StringManager.Instance.Get(
                LocalizationMainKey,
                "result", true);

        [Header("Requirements")]
        public List<PopupEventRequirementData> requirements = new();

        [Header("Conditions")]
        [Tooltip("모든 조건을 만족해야 선택지가 표시된다.")]
        public List<PopupEventChoiceConditionData> visibleConditions = new();

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

        public string rewardId;

        [Tooltip("보상 적용 대상 ID. 예: 전직 대상 CharacterJob enum 이름, characterId")]
        public string targetId;

        public int value;

        public ShrineGodType godType;

        public ScriptableObject targetData;


        public string tag;
    }

    [Serializable]
    public class PopupEventChoiceConditionData
    {
        public PopupEventChoiceConditionType conditionType;

        [Tooltip("조건 체크에 사용할 대상 ID. 예: CharacterJob enum 이름, characterId, tag")]
        public string targetId;

        public int value;

        public string tag;

        [Tooltip("true면 조건 결과를 반대로 사용한다.")]
        public bool invert;
    }

    [Serializable]
    public class PopupEventRequirementData
    {
        public PopupEventRequirementType requirementType;

        public ScriptableObject targetData;

        public int value;

        public string tag;
    }

    public enum PopupEventChoiceConditionType
    {
        None = 0,

        HasCharacter = 100,
        HasCharacterJob = 110,
        HasCharacterJobFamily = 120,
        HasCharacterJobTier = 130,

        HasTag = 200,
        HasRelic = 300,
        HasBless = 400,
        HasItem = 500,
    }

    public enum PopupEventRequirementType
    {
        None = 0,

        StoryItem = 1,

        Relic = 2,

        Bless = 3,

        Tag = 4
    }
}
