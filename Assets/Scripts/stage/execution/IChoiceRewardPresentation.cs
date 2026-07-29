using System;
using System.Collections.Generic;

namespace Stage
{
    /// <summary>
    /// Reward 지급 이후 별도 Reward UI가 실행 흐름을 지연시킬 수 있는 경계.
    /// 이벤트 Popup 자체는 Reward 정보를 표시하지 않는다.
    /// </summary>
    public interface IChoiceRewardPresentation
    {
        void Present(
            IReadOnlyList<PopupEventRewardData> rewards,
            Action onCompleted);
    }

    public sealed class ImmediateChoiceRewardPresentation
        : IChoiceRewardPresentation
    {
        public void Present(
            IReadOnlyList<PopupEventRewardData> rewards,
            Action onCompleted)
        {
            onCompleted?.Invoke();
        }
    }
}
