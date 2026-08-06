using UnityEngine;

public class UIFadeAnimation : UIAnimationBase
{
    public CanvasGroup targetGroup;
    public float fromAlpha = 0f;
    public float toAlpha = 1f;

    protected override void OnPlayStart()
    {
        if (targetGroup != null) targetGroup.alpha = fromAlpha;
    }

    protected override void UpdateAnimation(float progress)
    {
        if (targetGroup != null)
        {
            targetGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, progress);
        }
    }

    protected override void OnReset()
    {
        if (targetGroup != null) targetGroup.alpha = fromAlpha;
    }
}
