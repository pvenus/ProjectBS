using UnityEngine;
using TMPro;
using ProjectBS.Core;

namespace UIFramework.View
{
    /// <summary>
    /// 오직 골드(재화) 정보만 받아와서 텍스트로 렌더링하는 순수 뷰(View) 클래스입니다.
    /// 어느 프리팹이나 씬에 던져놓아도 스스로 AppManagers.Currency를 찾아 갱신됩니다.
    /// </summary>
    [AutoBindPrefix("UI")]
    public class GoldView : UIComponent
    {
        [AutoBind]
        [SerializeField] private TMP_Text goldText;
        [SerializeField] private string prefix = ""; // 예: "Gold: " 
        [SerializeField] private string suffix = " G"; // 예: " G"

        private bool isSubscribed = false;

        private void OnEnable()
        {
            SubscribeToCurrency();
        }

        private void OnDisable()
        {
            UnsubscribeFromCurrency();
        }

        private void Update()
        {
            // 아직 구독되지 않았다면 계속해서 AppManagers.Currency가 할당되었는지 체크합니다.
            // (씬 로딩 순서상 매니저가 나중에 생성될 수 있으므로)
            if (!isSubscribed && AppManagers.Currency != null)
            {
                SubscribeToCurrency();
            }
        }

        private void SubscribeToCurrency()
        {
            if (AppManagers.Currency != null && !isSubscribed)
            {
                AppManagers.Currency.OnGoldChanged += HandleGoldChanged;
                // 구독 즉시 현재 골드 값을 UI에 반영
                HandleGoldChanged(AppManagers.Currency.Gold);
                isSubscribed = true;
            }
        }

        private void UnsubscribeFromCurrency()
        {
            if (AppManagers.Currency != null && isSubscribed)
            {
                AppManagers.Currency.OnGoldChanged -= HandleGoldChanged;
                isSubscribed = false;
            }
        }

        private void HandleGoldChanged(int currentGold)
        {
            if (goldText != null)
            {
				goldText.text = $"{prefix}{currentGold}{suffix}";
            }
        }
    }
}
