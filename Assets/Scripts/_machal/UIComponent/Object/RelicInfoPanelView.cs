using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UIFramework.Data;

[AutoBindPrefix("UI")]
public class RelicInfoPanelView : UIComponent
{
    [Header("References")]
    [AutoBind] [SerializeField] private Image iconImage;
    [AutoBind] [SerializeField] private TMP_Text nameText;
    [AutoBind] [SerializeField] private TMP_Text descriptionText;
    [AutoBind] [SerializeField] private CanvasGroup canvasGroup;

    [Header("Animation")]
    [SerializeField] private float fadeDuration = 0.2f;
    [AutoBind] [SerializeField] private LayoutElement layoutElement;
    [SerializeField] private float targetWidth = 400f;

    private UIAnimationSequence sequence;
    private UIFadeAnimation fadeAnim;
    private UILayoutWidthAnimation widthAnim;

    private void Awake()
    {
        sequence = gameObject.GetComponent<UIAnimationSequence>();
        if (sequence == null) sequence = gameObject.AddComponent<UIAnimationSequence>();

        fadeAnim = gameObject.AddComponent<UIFadeAnimation>();
        widthAnim = gameObject.AddComponent<UILayoutWidthAnimation>();

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if (layoutElement == null)
        {
            layoutElement = GetComponent<LayoutElement>();
        }
    }

    public void Show(RelicCollectionItemViewData data)
    {
        Bind(data);
        PlayShow();
    }

    public void Hide()
    {
        PlayHide();
    }

    public void Bind(RelicCollectionItemViewData data)
    {
        if (data == null) return;

        if (iconImage != null)
        {
            iconImage.sprite = data.icon;
            iconImage.enabled = data.icon != null;
        }

        if (nameText != null)
        {
            nameText.text = data.displayName;
        }

        if (descriptionText != null)
        {
            descriptionText.text = data.description;
        }
    }

    private void PlayShow()
    {
        gameObject.SetActive(true);
        if (layoutElement != null)
        {
            layoutElement.preferredWidth = targetWidth;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    private void PlayHide()
    {
        if (layoutElement != null)
        {
            layoutElement.preferredWidth = 0f;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        gameObject.SetActive(false);
    }
}
