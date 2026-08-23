using System;
using UnityEngine;
using UnityEngine.UI;

[AutoBindPrefix("UI")]
public class UISelectableIconButton : UIComponent
{
    [Header("References")]
    [AutoBind] [SerializeField] private Button button;
    [AutoBind] [SerializeField] private Image iconImage;
    [AutoBind] [SerializeField] private Image selectedFrameImage;
    [AutoBind] [SerializeField] private CanvasGroup canvasGroup;
    [AutoBind] [SerializeField] private RectTransform lockedOverlay;

    [Header("Settings")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite selectedSprite;

    private Action onClickAction;

    private void Awake()
    {
        ResolveReferences();

        if (button != null)
        {
            button.onClick.AddListener(OnButtonClicked);
        }

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        ResolveReferences();
    }
#endif

    private void ResolveReferences()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
    }

    public void Bind(Action onClick)
    {
        onClickAction = onClick;
    }

    private void OnButtonClicked()
    {
        onClickAction?.Invoke();
    }

    public void SetIcon(Sprite sprite)
    {
        if (iconImage != null)
        {
            iconImage.sprite = sprite;
            iconImage.enabled = sprite != null;
        }
    }

    public void SetInteractable(bool interactable)
    {
        if (button != null)
        {
            button.interactable = interactable;
        }
    }

    public void SetLocked(bool locked)
    {
        if (lockedOverlay != null)
        {
            lockedOverlay.gameObject.SetActive(locked);
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = locked ? 0.6f : 1f;
        }
    }

    public void SetSelected(bool selected)
    {
        if (selectedFrameImage != null)
        {
            selectedFrameImage.gameObject.SetActive(selected);
        }

        if (button != null && button.targetGraphic is Image targetImg)
        {
            if (selected && selectedSprite != null)
            {
                targetImg.sprite = selectedSprite;
            }
            else if (!selected && normalSprite != null)
            {
                targetImg.sprite = normalSprite;
            }
        }
    }
}
