

using System.Collections.Generic;
using UnityEngine;

namespace Stage
{
    /// <summary>
    /// 팝업 기반 이벤트 라운드 데이터.
    /// RoundNode.eventId와 이 eventId를 매칭해서 팝업을 실행한다.
    /// </summary>
    [CreateAssetMenu(menuName = "Stage/Popup Event")]
    public class PopupEventSO : ScriptableObject
    {
        [Header("Identity")]
        public string eventId;
        public string title;

        [Header("Content")]
        [TextArea(3, 8)]
        public string body;

        [Header("Visual")]
        public Sprite mainImage;
        public Sprite icon;

        [Header("Choices")]
        public List<PopupEventChoice> choices = new();

        public PopupEventChoice GetChoice(string choiceId)
        {
            if (string.IsNullOrWhiteSpace(choiceId))
            {
                return null;
            }

            foreach (PopupEventChoice choice in choices)
            {
                if (choice != null && choice.choiceId == choiceId)
                {
                    return choice;
                }
            }

            return null;
        }

        public bool HasChoices()
        {
            return choices != null && choices.Count > 0;
        }
    }
}