using UnityEngine;
using UnityEngine.EventSystems;

public class UIHoverScaler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float hoverScale = 1.1f;
    public float duration = 0.1f;

    private Vector3 originalScale = Vector3.one;
    private Vector3 targetScale = Vector3.one;
    private Vector3 currentVelocity = Vector3.zero;

    private void OnEnable()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    private void Update()
    {
        transform.localScale = Vector3.SmoothDamp(transform.localScale, targetScale, ref currentVelocity, duration);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = originalScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
    }
}
