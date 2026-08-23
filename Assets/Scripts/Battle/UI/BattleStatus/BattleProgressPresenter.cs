using Battle.UI.BattleStatus;
using Session;
using UnityEngine;

namespace Battle.UI.BattleStatus
{
    /// <summary>
    /// BattleManager / BattleRuntime 데이터를 실시간으로 관찰하여 BattleProgressView에 표시하는 Presenter.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BattleProgressPresenter : MonoBehaviour
    {
        [Header("View Reference")]
        [SerializeField] private BattleProgressView progressView;

        [Header("Update Interval")]
        [SerializeField] private float updateInterval = 0.1f;

        [Header("Debug")]
        [SerializeField] private bool logDebug;

        private float elapsedSinceLastUpdate;

        public BattleProgressView ProgressView => progressView;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();
            Refresh();
        }

        private void LateUpdate()
        {
            if (updateInterval <= 0f)
            {
                Refresh();
                return;
            }

            elapsedSinceLastUpdate += Time.deltaTime;
            if (elapsedSinceLastUpdate >= updateInterval)
            {
                elapsedSinceLastUpdate = 0f;
                Refresh();
            }
        }

        private void ResolveReferences()
        {
            if (progressView == null)
            {
                progressView = GetComponent<BattleProgressView>();
            }
        }

        [ContextMenu("Refresh")]
        public void Refresh()
        {
            if (progressView == null)
            {
                return;
            }

            BattleManager battleManager = BattleManager.Instance;
            BattleRuntime battleRuntime = battleManager?.BattleSession?.BattleRuntime;

            if (battleRuntime == null)
            {
                // Try get BattleSO from Session if runtime not initialized yet
                BattleSO battleSO = GameSession.Instance?.BattleSession?.BattleSO;
                if (battleSO != null)
                {
                    BattleProgressViewData fallbackData = new BattleProgressViewData(
                        battleSO.BattleName,
                        1,
                        1,
                        battleSO.SurvivalTimeSeconds,
                        0f,
                        0);
                    progressView.Render(fallbackData);
                }
                return;
            }

            float remainingTime = Mathf.Max(0f, battleRuntime.survivalTimeSeconds - battleRuntime.elapsedTime);
            float elapsedTime = battleRuntime.elapsedTime;
            int remainingEnemyCount = battleRuntime.remainingEnemyCount;
            string battleName = battleRuntime.battleName;

            BattleProgressViewData viewData = new BattleProgressViewData(
                battleName,
                1,
                1,
                remainingTime,
                elapsedTime,
                remainingEnemyCount);

            progressView.Render(viewData);
        }
    }
}
