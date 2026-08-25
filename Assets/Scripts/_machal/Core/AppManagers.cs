using UnityEngine;

namespace ProjectBS.Core
{
    /// <summary>
    /// 실제 게임의 싱글톤과 테스트 환경의 Mock 매니저 사이를 스위칭해주는 전역 프록시 클래스입니다.
    /// UI 등 클라이언트 코드는 이 클래스만 바라보며, 런타임/테스트 여부를 모르게 됩니다.
    /// </summary>
    public static class AppManagers
    {
        public static ICurrencyManager Currency { get; set; }
        public static UIFramework.Interfaces.IRelicListProvider Relic { get; set; }
        public static IShopManager Shop { get; set; }
        public static IBeliefManager Belief { get; set; }
        public static UIFramework.Interfaces.IFaithManager Faith { get; set; }
        public static IShrineManager Shrine { get; set; }
        public static IBattleManager Battle { get; set; }
        public static IRandomEventManager RandomEvent { get; set; }
    }
}
