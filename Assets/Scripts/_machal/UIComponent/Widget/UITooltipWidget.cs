using UnityEngine;
using TMPro;

[AutoBindPrefix("Tooltip")]
public class UITooltipWidget : UIComponent
{
    [Header("UI Components")]
    [AutoBind] [SerializeField] private TMP_Text contentText;
    [AutoBind] [SerializeField] private RectTransform backgroundRect;

    [Header("Settings")]
    [SerializeField] private Vector2 offset = new Vector2(10f, -10f); // 마우스 커서 등에서 살짝 떨어지게

    private RectTransform rectTransform;
    private Canvas parentCanvas;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();

        // 툴팁이 마우스 레이캐스트를 막아 OnPointerExit를 유발하는 깜빡임 현상 방지
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        Hide();
    }

    /// <summary>
    /// 툴팁을 표시합니다.
    /// </summary>
    /// <param name="content">표시할 내용</param>
    /// <param name="screenPosition">화면 좌표 (보통 Input.mousePosition 사용)</param>
    public void Show(string content, Vector2 screenPosition)
    {
        if (contentText != null)
        {
            contentText.text = content;
        }

        gameObject.SetActive(true);

        // 텍스트 내용이 바뀌었으므로 Layout Rebuild를 강제하여 크기를 맞춤
        if (backgroundRect != null)
        {
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(backgroundRect);
        }

        UpdatePosition(screenPosition);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        // 툴팁이 켜져있을 때는 계속 마우스를 따라다니도록 합니다.
        if (gameObject.activeSelf)
        {
            UpdatePosition(Input.mousePosition);
        }
    }

    private void UpdatePosition(Vector2 screenPosition)
    {
        if (rectTransform == null || parentCanvas == null) return;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform, 
            screenPosition, 
            parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : parentCanvas.worldCamera, 
            out localPoint);

        // 오프셋 적용
        localPoint += offset;

        rectTransform.localPosition = localPoint;

        // 화면 밖으로 툴팁이 나가지 않도록 위치 보정
        AdjustPositionToScreenBounds();
    }

    private void AdjustPositionToScreenBounds()
    {
        if (parentCanvas == null) return;

        Vector3[] canvasCorners = new Vector3[4];
        RectTransform canvasRect = parentCanvas.transform as RectTransform;
        canvasRect.GetWorldCorners(canvasCorners);

        Vector3[] tooltipCorners = new Vector3[4];
        rectTransform.GetWorldCorners(tooltipCorners);

        Vector3 position = rectTransform.position;

        // 왼쪽 화면 밖으로 나감
        if (tooltipCorners[0].x < canvasCorners[0].x)
        {
            position.x += canvasCorners[0].x - tooltipCorners[0].x;
        }
        // 오른쪽 화면 밖으로 나감
        else if (tooltipCorners[2].x > canvasCorners[2].x)
        {
            position.x -= tooltipCorners[2].x - canvasCorners[2].x;
        }

        // 아래쪽 화면 밖으로 나감
        if (tooltipCorners[0].y < canvasCorners[0].y)
        {
            position.y += canvasCorners[0].y - tooltipCorners[0].y;
        }
        // 위쪽 화면 밖으로 나감
        else if (tooltipCorners[1].y > canvasCorners[1].y)
        {
            position.y -= tooltipCorners[1].y - canvasCorners[1].y;
        }

        rectTransform.position = position;
    }
}
