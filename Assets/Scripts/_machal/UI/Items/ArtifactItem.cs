using System;
using UnityEngine;

namespace UIFramework
{
    /// <summary>
    /// 보유 유물 리스트 내에 배치되는 개별 유물 아이템 컴포넌트입니다.
    /// </summary>
    [AutoBindPrefix("UI")]
    public class ArtifactItem : AutoBindBehaviour
    {
        [AutoBind] [SerializeField] private UIAutoImage icon;

        [AutoBind]
        [SerializeField] private UIAutoImage img_Frame;

        [AutoBind]
        [SerializeField] private UIAutoImage img_TypeFrame;

        [AutoBind]
        [SerializeField] private Transform hoverRoot;

        private HoverWidget _hoverWidget;
        private SelectableWidget _selectableWidget;

        private int _index;
        private Action<int> _onHover;
        private Action<int> _onExit;
        private Action<int> _onSelected;

        public enum ArtifactState
        {
            Normal,
            Hover,
            Selected,
            Locked
        }

        private void Awake()
        {
            _selectableWidget = GetComponent<SelectableWidget>();
            if (_selectableWidget == null)
            {
                _selectableWidget = gameObject.AddComponent<SelectableWidget>();
            }

            if (hoverRoot != null)
            {
                _hoverWidget = hoverRoot.GetComponent<HoverWidget>();
                if (_hoverWidget == null)
                {
                    _hoverWidget = hoverRoot.gameObject.AddComponent<HoverWidget>();
                }
            }

            if (_hoverWidget != null)
            {
                _hoverWidget.OnHoverEnter += HandleHoverEnter;
                _hoverWidget.OnHoverExit += HandleHoverExit;
            }

            if (_selectableWidget != null)
            {
                _selectableWidget.OnSelected += HandleSelected;
                _selectableWidget.OnSelectionChanged += HandleSelectionChanged;
            }
        }

        /// <summary>
        /// 아이템의 인덱스, 스프라이트 리소스 및 인터랙션 액션을 바인딩합니다.
        /// </summary>
        public void Bind(int index, Sprite iconSprite, Action<int> onHover, Action<int> onExit, Action<int> onSelected)
        {
            _index = index;
            _onHover = onHover;
            _onExit = onExit;
            _onSelected = onSelected;

            if (icon != null && icon.ImageComponent != null)
            {
                icon.ImageComponent.sprite = iconSprite;
                icon.ImageComponent.enabled = (iconSprite != null);
            }

            // 기본 상태로 리셋
            if (_selectableWidget != null)
            {
                _selectableWidget.IsSelected = false;
            }
            SetState(ArtifactState.Normal);
        }

        /// <summary>
        /// 유물의 렌더링 상태(Normal, Hover, Selected, Locked)를 변경하고 외형을 적용합니다.
        /// </summary>
        public void SetState(ArtifactState state)
        {
            if (img_Frame != null && img_Frame.ImageComponent != null)
            {
                switch (state)
                {
                    case ArtifactState.Normal:
                        img_Frame.ImageComponent.color = Color.white;
                        break;
                    case ArtifactState.Hover:
                        img_Frame.ImageComponent.color = Color.cyan; // 호버 시 청록색 프레임
                        break;
                    case ArtifactState.Selected:
                        img_Frame.ImageComponent.color = Color.yellow; // 선택 시 노란색 프레임
                        break;
                    case ArtifactState.Locked:
                        img_Frame.ImageComponent.color = Color.gray; // 잠금 시 회색 프레임
                        break;
                }
            }
        }

        private void HandleHoverEnter()
        {
            _onHover?.Invoke(_index);
            if (_selectableWidget == null || !_selectableWidget.IsSelected)
            {
                SetState(ArtifactState.Hover);
            }
        }

        private void HandleHoverExit()
        {
            _onExit?.Invoke(_index);
            SetState(_selectableWidget != null && _selectableWidget.IsSelected ? ArtifactState.Selected : ArtifactState.Normal);
        }

        private void HandleSelected()
        {
            _onSelected?.Invoke(_index);
        }

        private void HandleSelectionChanged(bool isSelected)
        {
            SetState(isSelected ? ArtifactState.Selected : ArtifactState.Normal);
        }
    }
}
