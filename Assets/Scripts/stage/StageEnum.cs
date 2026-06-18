using System;

namespace Stage
{
    /// <summary>
    /// 스테이지 노드 타입
    /// </summary>
    public enum RoundNodeType
    {
        None = 0,

        // 시작
        Start,

        // 전투
        Battle,
        EliteBattle,
        Boss,

        // 이벤트
        Event,
        RequiredSubEvent,

        // 시스템
        Shop,
        Rest
    }

    /// <summary>
    /// 노드 실행 방식
    /// </summary>
    public enum RoundExecuteMode
    {
        None = 0,

        // 씬 전환 (전투 / 이벤트 씬)
        Scene,

        // 팝업 UI
        Popup,

        // 즉시 처리 (버프, 회복 등)
        Immediate
    }

    /// <summary>
    /// 노드 상태
    /// </summary>
    public enum RoundNodeState
    {
        Locked = 0,
        Available,
        Cleared
    }

    /// <summary>
    /// 스테이지 진행 상태
    /// </summary>
    public enum StageProgressState
    {
        NotStarted = 0,
        InProgress,
        Completed,
        Failed
    }

    /// <summary>
    /// 노드 연결 타입 (확장 대비)
    /// </summary>
    public enum NodeConnectionType
    {
        Normal = 0,
        Hidden,
        Conditional
    }

    /// <summary>
    /// 팝업 이벤트 보상 타입
    /// </summary>
    public enum PopupEventRewardType
    {
        None = 0,

        Gold = 100,
        Hp = 200,
        HpPercent = 300,

        Reputation = 1000,
        Faith = 1100,

        Relic = 2000,
        RelicPool = 2050,
        StrategicSkillItem = 2100,
        StrategicSkillItemPool = 2150,
        Blessing = 2200,
        BlessingPool = 2250,
        AIFunction = 2300,

        FirstJobChange = 2400,
        SecondJobChange = 2450,

        SpecialBattle = 3000,
        BossBattle = 3100,

        UnlockRoute = 4000,
        RevealHiddenNode = 4050,
        NextEvent = 4100,

        NextBattleAttackSpeed = 5000,
        NextBattleMoveSpeed = 5100,
        NextBattleDefense = 5200,
    }
}
