using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UIFramework;

[AutoBindPrefix("Bind")]
public class UITextButton : UIComponent
{
    [AutoBind] [SerializeField] private Button button;
    [AutoBind] [SerializeField] private TMP_Text text;

    private Action onClickCallback;

    public void Bind(string label, Action onClick)
    {
        onClickCallback = onClick;

        if (text != null)
        {
            text.text = label;
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }
    }

    private void OnClick()
    {
        onClickCallback?.Invoke();
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        if (Application.isPlaying) return;
        base.OnValidate();
        SetupComponents();
    }

    private void Reset()
    {
        SetupComponents();
    }

    private void SetupComponents()
    {
        // 1. Rename root GameObject to Bind_Button for auto-binding if needed
        if (gameObject.name == "UITextButton" || gameObject.name == "UI_Button")
        {
            gameObject.name = "Bind_Button";
        }

        // 2. Setup (img)foreground child
        Transform foreground = transform.Find("(img)foreground");
        if (foreground == null)
        {
            GameObject fgGo = new GameObject("(img)foreground");
            fgGo.transform.SetParent(transform, false);
            RectTransform fgRt = fgGo.AddComponent<RectTransform>();
            fgRt.anchorMin = Vector2.zero;
            fgRt.anchorMax = Vector2.one;
            fgRt.sizeDelta = Vector2.zero;
            
            fgGo.AddComponent<UIAutoImage>();
        }

        // 3. Setup Bind_Text child
        Transform textTrans = transform.Find("Bind_Text");
        if (textTrans == null)
        {
            GameObject txtGo = new GameObject("Bind_Text");
            txtGo.transform.SetParent(transform, false);
            RectTransform txtRt = txtGo.AddComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmp = txtGo.AddComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Center; // Middle-Center alignment
            tmp.text = "Button";
        }
    }
#endif
}
