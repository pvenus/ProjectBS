using UnityEngine;
using ProjectBS.Core;
using UIFramework.Data;
using UIFramework.Page;
using UIFramework.View;
using Stage;
using Shrine;

namespace UIFramework.Test
{
    /// <summary>
    /// 테스트 환경에서 전역 매니저의 이벤트(OnGoldChanged 등)를 구독하고,
    /// 순수 뷰(TopBarPage, RelicListView 등)에 최신 ViewData를 Push해주는 테스트용 Resolver입니다.
    /// (실제 프로덕션에서는 이와 유사한 역할을 하는 정식 Resolver가 필요합니다)
    /// </summary>
    public class TestUIDataResolver : MonoBehaviour
    {
        [Header("Views")]
        public TopBarPage topBarPage;
        public RelicListView relicListView;

        private void Start()
        {
            if (topBarPage == null) topBarPage = FindObjectOfType<TopBarPage>();
            if (relicListView == null) relicListView = FindObjectOfType<RelicListView>();

            SubscribeToManagers();
            RefreshTopBar();
        }

        private void OnDestroy()
        {
            UnsubscribeFromManagers();
        }

        private void SubscribeToManagers()
        {
            if (AppManagers.Currency != null)
            {
                AppManagers.Currency.OnGoldChanged += HandleGoldChanged;
            }

            if (StageManager.Instance != null)
            {
                StageManager.Instance.OnNodeSelected += HandleStageChanged;
                StageManager.Instance.OnStageGenerated += HandleStageGenerated;
            }

            if (AppManagers.Relic != null)
            {
                AppManagers.Relic.OnRelicListChanged += HandleRelicListChanged;
            }
        }

        private void UnsubscribeFromManagers()
        {
            if (AppManagers.Currency != null)
            {
                AppManagers.Currency.OnGoldChanged -= HandleGoldChanged;
            }

            if (StageManager.Instance != null)
            {
                StageManager.Instance.OnNodeSelected -= HandleStageChanged;
                StageManager.Instance.OnStageGenerated -= HandleStageGenerated;
            }

            if (AppManagers.Relic != null)
            {
                AppManagers.Relic.OnRelicListChanged -= HandleRelicListChanged;
            }
        }

        private void HandleGoldChanged(int gold) => RefreshTopBar();
        private void HandleStageChanged(RoundNode node) => RefreshTopBar();
        private void HandleStageGenerated(StageGraph graph) => RefreshTopBar();
        
        private void HandleRelicListChanged(RelicListViewData data)
        {
            RefreshTopBar();
            if (relicListView != null)
            {
                relicListView.Bind(data);
            }
        }

        private void RefreshTopBar()
        {
            if (topBarPage == null) return;

            var viewData = new TopBarViewData
            {
                currentGold = AppManagers.Currency != null ? AppManagers.Currency.Gold : 0,
                currentStageName = (StageManager.Instance != null && StageManager.Instance.StageDefinition != null) 
                    ? StageManager.Instance.StageDefinition.stageName : "Test Stage",
                relicData = AppManagers.Relic != null ? AppManagers.Relic.GetRelicList() : new RelicListViewData()
            };

            // TopBarPage는 뷰이므로 Refresh만 호출
            topBarPage.Refresh(viewData);
        }
    }
}
