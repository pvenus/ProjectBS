using System;
using UnityEngine;
using UnityEngine.UI;

namespace Stage.UI
{
    /// <summary>
    /// Panel_OwnedEffects 프리팹에 붙는 UIView 래퍼.
    /// UIPopupViewController가 PopupType.StageOwnedEffects 타입으로 열고 닫는다.
    /// 실제 보유효과 표시는 하위 OwnedEffectInventoryPresenter가 담당한다.
    /// </summary>
    public class StageOwnedEffectsPanelView : UIView
    {
        [Header("Close Button")]
        [SerializeField] private Button closeButton;

        public event Action OnCloseRequested;

        private void Awake()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(HandleCloseClicked);
            }
        }

        private void OnDestroy()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(HandleCloseClicked);
            }
        }

        public override void ClearCallbacks()
        {
            OnCloseRequested = null;
        }

        private void HandleCloseClicked()
        {
            OnCloseRequested?.Invoke();
        }
    }
}
