using System;
using UIFramework.Data;

namespace UIFramework.Interfaces
{
    public interface IFaithManager
    {
        event Action<FaithSummaryUIViewData> OnFaithSummaryChanged;
        event Action<FaithDetailUIViewData> OnFaithDetailChanged;
        
        FaithSummaryUIViewData GetFaithSummary();
        FaithDetailUIViewData GetFaithDetail();
        
        void AddReputation(string faithId, int amount);
    }
}
