using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIFramework.Data
{
    [Serializable]
    public struct EventPopupViewData
    {
        public string Title;
        public string Description;
        public Sprite Illustration;
        public IReadOnlyList<EventChoiceViewData> Choices;
    }

    [Serializable]
    public struct EventChoiceViewData
    {
        public string Id;
        public string Text;
        public string ResultText;
        public bool HasResult;
    }
}
