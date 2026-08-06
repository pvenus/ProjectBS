using System.Collections;
using Session;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Loading
{
    public class LoadingSceneController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Slider progressSlider;

        [SerializeField] private string fallbackSceneName = "StageScene";

        [SerializeField] private float minimumLoadingDuration = 1.5f;

        [SerializeField] private float sliderSmoothSpeed = 2f;

        private IEnumerator Start()
        {
            yield return null;

            GameSession gameSession =
                GameSession.Instance;

            if (gameSession == null)
            {
                Debug.LogError(
                    "[LoadingSceneController] GameSession not found.");

                yield break;
            }

            BattleSession battleSession =
                gameSession.BattleSession;

            if (battleSession == null)
            {
                Debug.LogError(
                    "[LoadingSceneController] BattleSession not found.");

                yield break;
            }

            string nextSceneName =
                battleSession.BattleSceneName;

            if (string.IsNullOrEmpty(nextSceneName))
            {
                Debug.LogWarning(
                    "[LoadingSceneController] BattleSceneName is empty. Using fallback scene.");

                nextSceneName = fallbackSceneName;
            }

            float elapsedTime = 0f;
            AsyncOperation operation =
                SceneManager.LoadSceneAsync(nextSceneName);

            operation.allowSceneActivation = false;

            float displayedProgress = 0f;

            while (!operation.isDone)
            {
                float targetProgress =
                    Mathf.Clamp01(operation.progress / 0.9f);

                displayedProgress =
                    Mathf.MoveTowards(
                        displayedProgress,
                        targetProgress,
                        Time.deltaTime * sliderSmoothSpeed);

                if (progressSlider != null)
                {
                    progressSlider.value = displayedProgress;
                }

                elapsedTime += Time.deltaTime;

                if (operation.progress >= 0.9f
                    && elapsedTime >= minimumLoadingDuration
                    && displayedProgress >= 1f)
                {
                    if (progressSlider != null)
                    {
                        progressSlider.value = 1f;
                    }

                    operation.allowSceneActivation = true;
                }

                yield return null;
            }

            if (!battleSession.IsBattleActive)
            {
                battleSession.Clear();
            }
        }
    }
}
