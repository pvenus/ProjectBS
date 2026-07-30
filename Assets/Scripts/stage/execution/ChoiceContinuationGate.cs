namespace Stage
{
    /// <summary>
    /// 사용자 Confirm과 Reward UI 완료가 모두 끝난 뒤 한 번만 실행을 허용한다.
    /// </summary>
    public sealed class ChoiceContinuationGate
    {
        private string selectionId;
        private bool confirmationRequested;
        private bool rewardPresentationCompleted;
        private bool consumed;

        public bool Begin(string newSelectionId)
        {
            Reset();

            if (string.IsNullOrWhiteSpace(newSelectionId))
            {
                return false;
            }

            selectionId = newSelectionId;
            return true;
        }

        public bool RequestConfirmation()
        {
            if (selectionId == null || consumed)
            {
                return false;
            }

            confirmationRequested = true;
            return TryConsume();
        }

        public bool CompleteRewardPresentation(
            string completedSelectionId)
        {
            if (selectionId == null
                || consumed
                || !string.Equals(
                    selectionId,
                    completedSelectionId,
                    System.StringComparison.Ordinal))
            {
                return false;
            }

            rewardPresentationCompleted = true;
            return TryConsume();
        }

        public void Reset()
        {
            selectionId = null;
            confirmationRequested = false;
            rewardPresentationCompleted = false;
            consumed = false;
        }

        private bool TryConsume()
        {
            if (consumed
                || !confirmationRequested
                || !rewardPresentationCompleted)
            {
                return false;
            }

            consumed = true;
            return true;
        }
    }
}
