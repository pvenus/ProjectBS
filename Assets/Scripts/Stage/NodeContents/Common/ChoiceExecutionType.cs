namespace Stage
{
    /// <summary>
    /// Choice 확정 후 수행할 흐름의 종류.
    /// JSON discriminator와 직렬화된 실행 데이터 타입을 연결한다.
    /// </summary>
    public enum ChoiceExecutionType
    {
        None = 0,

        NextEvent = 50,
        Battle = 100,
        Shop = 200,
        Shrine = 300,

        CompleteEvent = 900,

        RandomGrowthRisk = 1000,
        RandomGrowthDecline = 1010,
        RandomGrowthSafe = 1020,

        PortfolioOutcome = 1100
    }
}
