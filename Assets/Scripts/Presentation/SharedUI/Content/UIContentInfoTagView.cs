using TMPro;
using UnityEngine;

namespace UI
{
    [AutoBindPrefix("Tag")]
    public sealed class UIContentInfoTagView : UIComponent
    {
        [AutoBind] [SerializeField] private TMP_Text text;

        public void Bind(string value)
        {
            if (text != null)
            {
                text.text = value ?? string.Empty;
            }
        }
    }
}
