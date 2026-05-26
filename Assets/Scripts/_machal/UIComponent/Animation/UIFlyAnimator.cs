using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UIFlyAnimator : MonoBehaviour
{
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private RectTransform rectTransform;
    private Coroutine animCoroutine;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Play(Vector2 startPos, Vector2 targetPos, float duration, float delay = 0f, Action onComplete = null)
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();

        if (animCoroutine != null)
        {
            StopCoroutine(animCoroutine);
        }

        animCoroutine = StartCoroutine(FlyRoutine(startPos, targetPos, duration, delay, onComplete));
    }

    private IEnumerator FlyRoutine(Vector2 startPos, Vector2 targetPos, float duration, float delay, Action onComplete)
    {
        rectTransform.anchoredPosition = startPos;

        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easeT = easeCurve.Evaluate(t);

            rectTransform.anchoredPosition = Vector2.LerpUnclamped(startPos, targetPos, easeT);
            yield return null;
        }

        rectTransform.anchoredPosition = targetPos;
        onComplete?.Invoke();
    }
}
