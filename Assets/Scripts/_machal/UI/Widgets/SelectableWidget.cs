using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIFramework
{
    /// <summary>
    /// 마우스 클릭(Pointer Click)을 감지하고 선택(Selection) 상태를 관리하는 기능형 위젯입니다.
    /// </summary>
    [DisallowMultipleComponent]
    public class SelectableWidget : MonoBehaviour, IPointerClickHandler
    {
        public event Action OnSelected;
        public event Action<bool> OnSelectionChanged;

        [SerializeField] private bool isSelected;

        public bool IsSelected
        {
            get => isSelected;
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    OnSelectionChanged?.Invoke(isSelected);
                }
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnSelected?.Invoke();
        }
    }
}
