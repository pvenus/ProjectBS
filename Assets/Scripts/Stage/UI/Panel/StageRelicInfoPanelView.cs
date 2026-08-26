using System;
using UnityEngine;
using UnityEngine.UI;

namespace Stage.UI
{
    /// <summary>
    /// Panel_RelicInfo 프리팹에 붙는 UIView 래퍼.
    /// UIPopupViewController가 PopupType.StageRelicInfo 타입으로 열고 닫는다.
    /// 실제 유물 목록 표시는 하위 RelicCollectionView가 담당한다.
    /// </summary>
    public class StageRelicInfoPanelView : UIView
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
