using System;
using UnityEngine;
using UnityEngine.UI;

namespace Stage.UI
{
    /// <summary>
    /// Panel_CharacterInfo 프리팹에 붙는 UIView 래퍼.
    /// UIPopupViewController가 PopupType.StageCharacterInfo 타입으로 열고 닫는다.
    /// 실제 캐릭터 정보 표시는 하위 CharacterSkillContentInfoPresenter가 담당한다.
    /// </summary>
    public class StageCharacterInfoPanelView : UIView
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
