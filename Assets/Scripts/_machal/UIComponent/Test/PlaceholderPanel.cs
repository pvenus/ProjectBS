using UnityEngine;
using TMPro;
using UnityEngine.UI;

[AutoBindPrefix("Placeholder")]
public class PlaceholderPanel : UIComponent
{
    [AutoBind] [SerializeField] private GameObject panelRoot;
    [AutoBind] [SerializeField] private TMP_Text titleText;
    [AutoBind] [SerializeField] private TMP_Text descText;
    [AutoBind] [SerializeField] private Button closeButton;

    private void Awake()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Hide);
        }
    }

    public void Show(string title, string desc)
    {
        if (titleText != null) titleText.text = title;
        if (descText != null) descText.text = desc;
        
        if (panelRoot != null) panelRoot.SetActive(true);
    }

    public void Hide()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
    }
}
