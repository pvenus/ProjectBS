using Session;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Battle
{
    public class BattleStatusUI : MonoBehaviour
    {
        [Header("Survival UI")]
        [SerializeField]
        private GameObject survivalRoot;

        [SerializeField]
        private TMP_Text survivalTimeText;

        [SerializeField]
        private Slider survivalProgressSlider;

        private BattleRuntime battleRuntime;
        private bool isInitialized;

        private void Start()
        {
            SetSurvivalVisible(false);
        }

        private void Update()
        {
            if (!isInitialized)
            {
                TryInitialize();
            }

            UpdateSurvivalUI();
        }

        private void TryInitialize()
        {
            GameSession gameSession =
                GameSession.Instance;

            if (gameSession == null
                || gameSession.BattleSession == null
                || gameSession.BattleSession.BattleRuntime == null)
            {
                SetSurvivalVisible(false);
                return;
            }

            battleRuntime =
                gameSession.BattleSession.BattleRuntime;

            isInitialized = true;

            bool isSurvivalBattle =
                battleRuntime.victoryRule == BattleVictoryRule.SurviveTime;

            SetSurvivalVisible(isSurvivalBattle);

            if (isSurvivalBattle)
            {
                UpdateSurvivalUI();
            }
        }

        private void UpdateSurvivalUI()
        {
            if (battleRuntime == null
                || battleRuntime.victoryRule != BattleVictoryRule.SurviveTime)
            {
                SetSurvivalVisible(false);
                return;
            }

            float targetSeconds =
                Mathf.Max(0f, battleRuntime.survivalTimeSeconds);

            float elapsedSeconds =
                Mathf.Max(0f, battleRuntime.elapsedTime);

            float remainSeconds =
                Mathf.Max(0f, targetSeconds - elapsedSeconds);

            if (survivalTimeText != null)
            {
                survivalTimeText.text =
                    FormatRemainTime(remainSeconds);
            }

            if (survivalProgressSlider != null)
            {
                survivalProgressSlider.minValue = 0f;
                survivalProgressSlider.maxValue = 1f;
                survivalProgressSlider.value =
                    targetSeconds <= 0f
                        ? 1f
                        : Mathf.Clamp01(elapsedSeconds / targetSeconds);
            }
        }

        private void SetSurvivalVisible(bool isVisible)
        {
            if (survivalRoot != null)
            {
                survivalRoot.SetActive(isVisible);
            }
        }

        private string FormatRemainTime(float remainSeconds)
        {
            int totalSeconds =
                Mathf.CeilToInt(remainSeconds);

            int minutes =
                totalSeconds / 60;

            int seconds =
                totalSeconds % 60;

            return $"생존 {minutes:00}:{seconds:00}";
        }
    }
}