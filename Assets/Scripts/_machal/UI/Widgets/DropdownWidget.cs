using UnityEngine;

namespace UIFramework
{
    /// <summary>
    /// 드롭다운 리스트의 활성화/비활성화 상태를 제어하는 추상 베이스 위젯입니다.
    /// </summary>
    public abstract class DropdownWidget : MonoBehaviour
    {
        public abstract void Show();
        public abstract void Hide();
        public abstract void Toggle();
        public abstract bool IsOpen { get; }
    }
}
