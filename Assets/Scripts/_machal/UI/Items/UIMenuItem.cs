using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UIFramework
{
    /// <summary>
    /// 시스템 메뉴 드롭다운 내부에 배치되는 개별 메뉴 항목 컴포넌트입니다.
    /// </summary>
    [AutoBindPrefix("UI")]
    public class UIMenuItem : AutoBindBehaviour
    {
        [AutoBind] [SerializeField] private TMP_Text labelText;
        [AutoBind] [SerializeField] private Button button;

        /// <summary>
        /// 메뉴 항목의 이름과 클릭 시 호출될 액션을 바인딩합니다.
        /// </summary>
        public void Bind(string label, Action onClick)
        {
            if (labelText != null)
            {
                labelText.text = label;
            }

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                if (onClick != null)
                {
                    button.onClick.AddListener(() => onClick.Invoke());
                }
            }
        }
    }
}
