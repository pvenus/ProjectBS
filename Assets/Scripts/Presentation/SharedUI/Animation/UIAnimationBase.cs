using System;
using UnityEngine;

public abstract class UIAnimationBase : MonoBehaviour
{
    public float duration = 0.3f;
    public float delay = 0f;
    public AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    public bool IsPlaying { get; private set; }

    private float timer = 0f;
    private Action onCompleteCallback;

    public void Play(Action onComplete = null)
    {
        onCompleteCallback = onComplete;
        timer = 0f;
        IsPlaying = true;
        OnPlayStart();
    }

    public void Stop()
    {
        IsPlaying = false;
        onCompleteCallback = null;
    }

    public void SkipToEnd()
    {
        if (IsPlaying)
        {
            UpdateAnimation(1f);
            OnPlayEnd();
            IsPlaying = false;
            
            var callback = onCompleteCallback;
            onCompleteCallback = null;
            callback?.Invoke();
        }
    }

    public void ResetToStart()
    {
        IsPlaying = false;
        timer = 0f;
        OnReset();
    }

    protected virtual void Update()
    {
        if (!IsPlaying) return;

        timer += Time.deltaTime;

        if (timer < delay) return;

        float animTime = timer - delay;
        if (animTime >= duration)
        {
            UpdateAnimation(1f);
            OnPlayEnd();
            IsPlaying = false;
            
            var callback = onCompleteCallback;
            onCompleteCallback = null;
            callback?.Invoke();
        }
        else
        {
            float t = Mathf.Clamp01(animTime / duration);
            float easeT = curve != null ? curve.Evaluate(t) : t;
            UpdateAnimation(easeT);
        }
    }

    protected virtual void OnPlayStart() { }
    protected virtual void OnPlayEnd() { }
    protected abstract void UpdateAnimation(float progress);
    protected virtual void OnReset() { }

    [ContextMenu("Test Play")]
    public void TestPlay() => Play();

    [ContextMenu("Test Stop")]
    public void TestStop() => Stop();

    [ContextMenu("Test Skip")]
    public void TestSkip() => SkipToEnd();

    [ContextMenu("Test Reset")]
    public void TestReset() => ResetToStart();
}
