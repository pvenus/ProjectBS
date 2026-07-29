using System;
using UnityEngine;

namespace Stage
{
    /// <summary>
    /// PopupEventChoice에 직접 직렬화되는 실행 설정.
    /// 별도 ScriptableObject 에셋으로 생성하지 않는다.
    /// </summary>
    [Serializable]
    public sealed class ChoiceExecutionConfig
    {
        public ChoiceExecutionType executionType;

        [SerializeReference]
        public ChoiceExecutionData data;

        public bool HasData => data != null;
    }
}
