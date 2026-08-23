using UnityEngine;
using UnityEngine.UI;

public class UILayoutWidthAnimation : UIAnimationBase
{
    public LayoutElement targetLayoutElement;
    public float fromWidth = 0f;
    public float toWidth = 400f;

    protected override void OnPlayStart()
    {
        if (targetLayoutElement != null)
        {
            targetLayoutElement.preferredWidth = fromWidth;
        }
    }

    protected override void UpdateAnimation(float progress)
    {
        if (targetLayoutElement != null)
        {
            targetLayoutElement.preferredWidth = Mathf.Lerp(fromWidth, toWidth, progress);
        }
    }

    protected override void OnReset()
    {
        if (targetLayoutElement != null)
        {
            targetLayoutElement.preferredWidth = fromWidth;
        }
    }
}
