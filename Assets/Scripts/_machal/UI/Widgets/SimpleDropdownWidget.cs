using UnityEngine;

namespace UIFramework
{
    /// <summary>
    /// 대상 컨테이너 오브젝트의 활성화 상태(SetActive)를 직접 토글하는 표준 구체 드롭다운 위젯입니다.
    /// </summary>
    [DisallowMultipleComponent]
    public class SimpleDropdownWidget : DropdownWidget
    {
        [Tooltip("열고 닫을 대상 컨테이너입니다. 지정되지 않으면 위젯이 부착된 GameObject를 사용합니다.")]
        [SerializeField] private GameObject container;

        private void Awake()
        {
            if (container == null)
            {
                container = gameObject;
            }
            Hide(); // 기본 상태는 닫힘
        }

        public override void Show()
        {
            if (container != null)
            {
                container.SetActive(true);
            }
        }

        public override void Hide()
        {
            if (container != null)
            {
                container.SetActive(false);
            }
        }

        public override void Toggle()
        {
            if (container != null)
            {
                if (container.activeSelf)
                {
                    Hide();
                }
                else
                {
                    Show();
                }
            }
        }

        public override bool IsOpen => container != null && container.activeSelf;
    }
}
