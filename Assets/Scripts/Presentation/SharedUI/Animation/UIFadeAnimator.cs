using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UIFadeAnimator : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private Coroutine animCoroutine;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Play(float startAlpha, float targetAlpha, float duration, float delay = 0f, Action onComplete = null)
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

        if (animCoroutine != null)
        {
            StopCoroutine(animCoroutine);
        }

        animCoroutine = StartCoroutine(FadeRoutine(startAlpha, targetAlpha, duration, delay, onComplete));
    }

    private IEnumerator FadeRoutine(float startAlpha, float targetAlpha, float duration, float delay, Action onComplete)
    {
        canvasGroup.alpha = startAlpha;

        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        onComplete?.Invoke();
    }
}
