using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UIFramework.Data;

namespace UIFramework.View
{
    /// <summary>
    /// 개별 신앙(평판) 정보를 표시하는 UI 컴포넌트
    /// 아이콘, 이름, 현재 레벨, 그리고 경험치 바(혹은 텍스트)를 렌더링합니다.
    /// </summary>
    [AutoBindPrefix("UI")]
    public class BeliefIconView : UIComponent
    {
        [AutoBind]
        [SerializeField] private Image godIcon;

        [AutoBind]
        [SerializeField] private TMP_Text godNameText;

        [AutoBind]
        [SerializeField] private TMP_Text levelText;

        [AutoBind]
        [SerializeField] private TMP_Text expText;

        [AutoBind]
        [SerializeField] private Slider expSlider; // 선택사항: 경험치 슬라이더 (없으면 무시)

        public void Bind(BeliefItemViewData data)
        {
            if (data == null) return;

            if (godIcon != null)
            {
                godIcon.sprite = data.godIcon;
                godIcon.enabled = data.godIcon != null;
            }

            if (godNameText != null)
                godNameText.text = data.godName;

            if (levelText != null)
                levelText.text = $"Lv.{data.currentLevel}";

            if (expText != null)
                expText.text = $"{data.currentExp} / {data.maxExpForNextLevel}";

            if (expSlider != null && data.maxExpForNextLevel > 0)
            {
                expSlider.maxValue = data.maxExpForNextLevel;
                expSlider.value = data.currentExp;
            }
        }
    }
}
