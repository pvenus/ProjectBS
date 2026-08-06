using TMPro;
using UnityEngine;

[AutoBindPrefix("Header")]
public class ShrineHeaderUI : UIComponent
{
    [AutoBind]
    [SerializeField]
    private TMP_Text titleText;

    [AutoBind]
    [SerializeField]
    private TMP_Text descText;

    public void SetTitle(string title)
    {
        titleText.text = title;
    }

    public void SetDescription(string description)
    {
        descText.text = description;
    }
}
