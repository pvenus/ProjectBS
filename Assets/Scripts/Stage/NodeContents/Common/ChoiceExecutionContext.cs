using System;
using Battle;

namespace Stage
{
    /// <summary>
    /// 실행 데이터가 구체 Manager를 직접 참조하지 않도록 런타임 동작을 전달한다.
    /// </summary>
    public sealed class ChoiceExecutionContext
    {
        public Action<PopupEventSO> OpenNextEvent { get; }
        public TryOpenNextEventTransaction OpenNextEventTransaction { get; }
        public Func<bool> CompleteEvent { get; }
        public Func<BattleSO, bool> BeginBattle { get; }
        public Func<ShopExecutionData, bool> OpenShop { get; }
        public Func<ShrineExecutionData, bool> OpenShrine { get; }
        public TryApplyPortfolioOutcome ApplyPortfolioOutcome { get; }

        public delegate bool TryApplyPortfolioOutcome(
            PortfolioOutcomeExecutionData data, out string error);
        public delegate bool TryOpenNextEventTransaction(
            NextEventExecutionData data, out string error);

        public ChoiceExecutionContext(
            Action<PopupEventSO> openNextEvent = null,
            Func<bool> completeEvent = null,
            Func<BattleSO, bool> beginBattle = null,
            Func<ShopExecutionData, bool> openShop = null,
            Func<ShrineExecutionData, bool> openShrine = null,
            TryApplyPortfolioOutcome applyPortfolioOutcome = null,
            TryOpenNextEventTransaction openNextEventTransaction = null)
        {
            OpenNextEvent = openNextEvent;
            CompleteEvent = completeEvent;
            BeginBattle = beginBattle;
            OpenShop = openShop;
            OpenShrine = openShrine;
            ApplyPortfolioOutcome = applyPortfolioOutcome;
            OpenNextEventTransaction = openNextEventTransaction;
        }
    }
}
