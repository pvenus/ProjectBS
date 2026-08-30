namespace Stage
{
    /// <summary>
    /// 실행 타입과 SerializeReference 파생 데이터를 한 곳에서 연결한다.
    /// Editor Inspector와 JSON Builder가 동일한 생성 규칙을 사용한다.
    /// </summary>
    public static class ChoiceExecutionDataFactory
    {
        public static ChoiceExecutionData Create(
            ChoiceExecutionType executionType)
        {
            return executionType switch
            {
                ChoiceExecutionType.NextEvent =>
                    new NextEventExecutionData(),
                ChoiceExecutionType.Battle =>
                    new BattleExecutionData(),
                ChoiceExecutionType.Shop =>
                    new ShopExecutionData(),
                ChoiceExecutionType.Shrine =>
                    new ShrineExecutionData(),
                ChoiceExecutionType.CompleteEvent =>
                    new CompleteEventExecutionData(),
                ChoiceExecutionType.RandomGrowthRisk =>
                    new RandomGrowthRiskExecutionData(),
                ChoiceExecutionType.RandomGrowthDecline =>
                    new RandomGrowthDeclineExecutionData(),
                ChoiceExecutionType.RandomGrowthSafe =>
                    new RandomGrowthSafeExecutionData(),
                ChoiceExecutionType.PortfolioOutcome =>
                    new PortfolioOutcomeExecutionData(),
                _ => null
            };
        }

        public static ChoiceExecutionConfig CreateConfig(
            ChoiceExecutionType executionType)
        {
            return new ChoiceExecutionConfig
            {
                executionType = executionType,
                data = Create(executionType)
            };
        }

        public static bool ReplaceDataIfNeeded(
            ChoiceExecutionConfig config)
        {
            if (config == null)
            {
                return false;
            }

            if (ChoiceExecutionConfigValidator.IsTypeMatch(
                    config.executionType,
                    config.data))
            {
                return false;
            }

            config.data = Create(config.executionType);
            return true;
        }
    }
}
