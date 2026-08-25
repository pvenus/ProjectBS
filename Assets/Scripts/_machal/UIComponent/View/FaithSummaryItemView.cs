using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UIFramework.Data;

namespace UIFramework.View
{
    [AutoBindPrefix("Bind")]
    public class FaithSummaryItemView : UIComponent
    {
        [AutoBind] [SerializeField] private Image img_faithIcon;
        [AutoBind] [SerializeField] private Slider Bind_LvGuage;
        [AutoBind] [SerializeField] private TMP_Text Bind_NameText;
        [AutoBind] [SerializeField] private TMP_Text Bind_CurLevelText;
        [AutoBind] [SerializeField] private TMP_Text Bind_NextLevelText;
        [AutoBind] [SerializeField] private TMP_Text Bind_ExpText;

        [SerializeField] private Button button;

        private FaithSummaryItemViewData _data;
        private Action<FaithSummaryItemViewData> _onClick;

        private void Awake()
        {
            if (button == null || !button.transform.IsChildOf(transform)) button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(HandleClick);
            }

            // AutoBind 접두사 및 필드명 매칭 실패(더블 접두사 매칭 등) 또는 에디터 상에서 잘못된 오브젝트가 할당된 경우에 대비한 런타임 폴백 바인딩
            if (Bind_NameText == null || !Bind_NameText.transform.IsChildOf(transform)) 
                Bind_NameText = FindChildComponent<TMP_Text>("Bind_NameText");
            
            if (Bind_LvGuage == null || !Bind_LvGuage.transform.IsChildOf(transform)) 
                Bind_LvGuage = FindChildComponent<Slider>("Bind_LvGuage");
            
            if (Bind_CurLevelText == null || !Bind_CurLevelText.transform.IsChildOf(transform)) 
                Bind_CurLevelText = FindChildComponent<TMP_Text>("Bind_CurLevelText");
            
            if (Bind_NextLevelText == null || !Bind_NextLevelText.transform.IsChildOf(transform)) 
                Bind_NextLevelText = FindChildComponent<TMP_Text>("Bind_NextLevelText");
            
            if (Bind_ExpText == null || !Bind_ExpText.transform.IsChildOf(transform)) 
                Bind_ExpText = FindChildComponent<TMP_Text>("Bind_ExpText");
            
            if (img_faithIcon == null || !img_faithIcon.transform.IsChildOf(transform))
            {
                img_faithIcon = FindChildComponent<Image>("Bind_Img_faithIcon");
                if (img_faithIcon == null || !img_faithIcon.transform.IsChildOf(transform)) 
                    img_faithIcon = FindChildComponent<Image>("img_faithIcon");
            }
        }

        private T FindChildComponent<T>(string name) where T : Component
        {
            var children = GetComponentsInChildren<Transform>(true);
            foreach (var child in children)
            {
                if (child.name == name)
                {
                    var comp = child.GetComponent<T>();
                    if (comp != null) return comp;
                }
            }
            return null;
        }

        public void Bind(FaithSummaryItemViewData data, Action<FaithSummaryItemViewData> onClick)
        {
            _data = data;
            _onClick = onClick;

            if (img_faithIcon != null && data.icon != null)
            {
                img_faithIcon.sprite = data.icon;
            }

            if (Bind_NameText != null)
            {
                Bind_NameText.text = data.displayName;
            }

            if (Bind_CurLevelText != null)
            {
                Bind_CurLevelText.text = $"Lv.{data.currentLevel}";
            }

            if (Bind_NextLevelText != null)
            {
                Bind_NextLevelText.text = data.isMaxLevel ? "MAX" : $"Lv.{data.nextLevel}";
            }

            if (Bind_ExpText != null)
            {
                if (data.isMaxLevel)
                {
                    Bind_ExpText.text = "MAX";
                }
                else
                {
                    Bind_ExpText.text = $"{data.currentLevelReputation} / {data.nextLevelRequiredReputation}";
                }
            }

            if (Bind_LvGuage != null)
            {
                Bind_LvGuage.value = data.progress01;
            }
        }

        private void HandleClick()
        {
            if (_data != null)
            {
                _onClick?.Invoke(_data);
            }
        }

        public void SetSelected(bool selected)
        {
            // Can implement visual changes for selected state if needed
        }
    }
}
