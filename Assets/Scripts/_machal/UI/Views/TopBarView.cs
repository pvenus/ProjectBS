using UnityEngine;
using TMPro;

namespace UIFramework
{
    /// <summary>
    /// 상단바 UI의 최상위 뷰 클래스입니다.
    /// 골드 및 스테이지 텍스트 업데이트를 별도 서브 뷰 없이 직접 관리합니다.
    /// </summary>
    public class TopBarView : AutoBindBehaviour
    {
        // Stage progress fields (previously in StageProgressView)
        [AutoBind] [SerializeField] private UIAutoImage stageIcon;
        [AutoBind] [SerializeField] private TMP_Text stageText;

        // Gold currency fields (previously in CurrencyView)
        [AutoBind] [SerializeField] private UIAutoImage goldIcon;
        [AutoBind] [SerializeField] private TMP_Text goldText;
        [AutoBind] [SerializeField] private UIAutoImage goldField;

        // Subviews (separated views with widgets)
        [AutoBind] [SerializeField] private SystemMenuView menuButton;
        [AutoBind] [SerializeField] private ArtifactListView blessList;

        public SystemMenuView Menu => menuButton;
        public ArtifactListView Artifacts => blessList;

        private void Reset()
        {
            // 인스펙터 기본 프리픽스를 TopBarView에 대해 Top으로 설정
            //AutoBindPrefix = "Top";
        }

        /// <summary>
        /// 골드 보유량을 텍스트로 설정합니다. 세 자리 콤마 포맷을 적용합니다.
        /// </summary>
        public void SetGoldAmount(int amount)
        {
            if (goldText != null)
            {
                goldText.text = amount.ToString("N0");
            }
        }

        /// <summary>
        /// 진행도 텍스트를 설정합니다. (예: "2-4 / 10")
        /// </summary>
        public void SetStageText(string text)
        {
            if (stageText != null)
            {
                stageText.text = text;
            }
        }
    }
}
