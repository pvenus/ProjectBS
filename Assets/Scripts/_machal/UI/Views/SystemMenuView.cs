using UnityEngine;
using UnityEngine.UI;

namespace UIFramework
{
    /// <summary>
    /// 시스템 메뉴 버튼과 드롭다운 리스트의 상태를 관리하는 뷰 컴포넌트입니다.
    /// 구체 위젯 타입에 의존하지 않고 추상 DropdownWidget을 통해 동작합니다.
    /// </summary>
    public class SystemMenuView : AutoBindBehaviour
    {
        [AutoBind] [SerializeField] private Button menuButton;

        [Tooltip("드롭다운 컨테이너의 루트입니다. 드롭다운 위젯이 이곳에 부착됩니다.")]
        [AutoBind]
        [SerializeField] private Transform dropdownRoot;

        private DropdownWidget _dropdownWidget;

        private void Awake()
        {
            if (dropdownRoot != null)
            {
                _dropdownWidget = dropdownRoot.GetComponent<DropdownWidget>();
                if (_dropdownWidget == null)
                {
                    _dropdownWidget = dropdownRoot.gameObject.AddComponent<SimpleDropdownWidget>();
                }
            }

            if (menuButton != null)
            {
                menuButton.onClick.AddListener(ToggleDropdown);
            }
        }

        public void ToggleDropdown()
        {
            if (_dropdownWidget != null)
            {
                _dropdownWidget.Toggle();
            }
        }

        public void ShowDropdown()
        {
            if (_dropdownWidget != null)
            {
                _dropdownWidget.Show();
            }
        }

        public void HideDropdown()
        {
            if (_dropdownWidget != null)
            {
                _dropdownWidget.Hide();
            }
        }

        public Transform DropdownRoot => dropdownRoot;
        public DropdownWidget DropdownWidget => _dropdownWidget;
    }
}
