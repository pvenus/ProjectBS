using UnityEngine;

public class UIFlyAnimation : UIAnimationBase
{
    public RectTransform targetRect;
    public Vector2 fromPosition;
    public Vector2 toPosition;

    protected override void OnPlayStart()
    {
        if (targetRect != null) targetRect.anchoredPosition = fromPosition;
    }

    protected override void UpdateAnimation(float progress)
    {
        if (targetRect != null)
        {
            targetRect.anchoredPosition = Vector2.LerpUnclamped(fromPosition, toPosition, progress);
        }
    }

    protected override void OnReset()
    {
        if (targetRect != null) targetRect.anchoredPosition = fromPosition;
    }
}
