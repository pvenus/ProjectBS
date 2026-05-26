using UnityEngine;

public abstract class UIPage : AutoBindBehaviour
{
    public virtual void Show()
    {
        gameObject.SetActive(true);
    }

    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }

    public virtual void Refresh()
    {
    }
}
