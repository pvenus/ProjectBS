using UnityEngine;

namespace ProjectBS.Core
{
    public class ShopOpenRequest
    {
        public string ShopId;
        public int Floor;
        public string SourceNodeId;
    }

    public interface IShopManager
    {
        void OpenShop(ShopOpenRequest request);
    }

    public class ShrineOpenRequest
    {
        public string ShrineId;
    }

    public interface IShrineManager
    {
        void OpenShrine(ShrineOpenRequest request);
    }

    public class BattleStartRequest
    {
        public string BattleId;
    }

    public interface IBattleManager
    {
        void StartBattle(BattleStartRequest request);
    }

    public class RandomEventRequest
    {
        public string EventId;
    }

    public interface IRandomEventManager
    {
        void OpenRandomEvent(RandomEventRequest request);
        // void ShowCustomEventPopup(EventPopupTestData data, System.Action<EventPopupChoiceData> onChoiceSelected);
    }

    public interface ICurrencyManager
    {
        int Gold { get; }
        event System.Action<int> OnGoldChanged;
    }

    public interface IBeliefManager
    {
        UIFramework.Data.BeliefListViewData GetBeliefList();
        event System.Action<UIFramework.Data.BeliefListViewData> OnBeliefListChanged;
    }
}
