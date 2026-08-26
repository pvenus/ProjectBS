namespace Stage
{
    /// <summary>
    /// 스테이지 노드의 아이콘 및 UI 프리팹 타입.
    /// ChoiceExecutionType과 독립된 아이콘 전용 분류 키.
    /// 신규 노드 작성 시 직접 지정, 기존 노드는 Choice 분석 결과를 기반으로 채운다.
    /// 확장 시 값을 추가해도 기존 에셋에 영향 없음.
    /// </summary>
    public enum NodeIconType
    {
        None    = 0,

        Battle  = 100,

        Shop    = 200,

        Shrine  = 300,

        /// <summary>
        /// 스토리 분기 / CompleteEvent 로 마무리되는 일반 이벤트 노드.
        /// </summary>
        Story   = 400,

        /// <summary>
        /// 전직
        /// </summary>
        Up = 500
    }
}
