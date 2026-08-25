using UnityEngine;
using ProjectBS.Core;
using UIFramework.Data;

namespace UIFramework.View
{
    /// <summary>
    /// AppManagers.Belief 데이터를 읽어와 전체 신앙 목록을 렌더링하는 순수 뷰 클래스입니다.
    /// </summary>
    [AutoBindPrefix("UI")]
    public class BeliefListView : UIComponent
    {
        [AutoBind]
        [SerializeField] private Transform contentRoot;

        [SerializeField] private BeliefIconView iconPrefab;

        private bool isSubscribed = false;

        private void OnEnable()
        {
            SubscribeToBeliefList();
        }

        private void OnDisable()
        {
            UnsubscribeFromBeliefList();
        }

        private void Update()
        {
            if (!isSubscribed && AppManagers.Belief != null)
            {
                SubscribeToBeliefList();
            }
        }

        private void SubscribeToBeliefList()
        {
            if (AppManagers.Belief != null && !isSubscribed)
            {
                AppManagers.Belief.OnBeliefListChanged += HandleBeliefListChanged;
                HandleBeliefListChanged(AppManagers.Belief.GetBeliefList());
                isSubscribed = true;
            }
        }

        private void UnsubscribeFromBeliefList()
        {
            if (AppManagers.Belief != null && isSubscribed)
            {
                AppManagers.Belief.OnBeliefListChanged -= HandleBeliefListChanged;
                isSubscribed = false;
            }
        }

        private void HandleBeliefListChanged(BeliefListViewData data)
        {
            if (contentRoot == null || iconPrefab == null || data == null) return;

            // 기존 아이콘 초기화
            foreach (Transform child in contentRoot)
            {
                Destroy(child.gameObject);
            }

            // 신앙 리스트 생성
            foreach (var belief in data.beliefs)
            {
                BeliefIconView iconView = Instantiate(iconPrefab, contentRoot);
                iconView.Bind(belief);
            }
        }
    }
}
