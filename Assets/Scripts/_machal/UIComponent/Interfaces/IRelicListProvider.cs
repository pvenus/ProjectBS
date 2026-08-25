using System;
using UIFramework.Data;

namespace UIFramework.Interfaces
{
    public interface IRelicListProvider
    {
        RelicListViewData GetRelicList();
        event Action<RelicListViewData> OnRelicListChanged;
    }
}
