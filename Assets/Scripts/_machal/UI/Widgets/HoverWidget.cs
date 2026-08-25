using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIFramework
{
    /// <summary>
    /// 마우스 포인터의 진입(Hover Enter) 및 이탈(Hover Exit) 이벤트를 탐지하여 중계하는 기능형 위젯입니다.
    /// </summary>
    [DisallowMultipleComponent]
    public class HoverWidget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public event Action OnHoverEnter;
        public event Action OnHoverExit;

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnHoverEnter?.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnHoverExit?.Invoke();
        }
    }
}
