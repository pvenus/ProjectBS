using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UIFramework.Data;

namespace UIFramework.View
{
    [RequireComponent(typeof(Image))]
    [AutoBindPrefix("UI")]
    public class FaithNodeView : UIComponent, IPointerClickHandler
    {
        private Image _iconImage;
        private FaithNodeViewData _data;
        private Action<FaithNodeViewData> _onClick;

        [Header("Colors")]
        public Color unlockedColor = Color.white;
        public Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 0.4f);

        private void Awake()
        {
            _iconImage = GetComponent<Image>();
        }

        public void Bind(FaithNodeViewData data, Action<FaithNodeViewData> onClick)
        {
            _data = data;
            _onClick = onClick;

            if (_iconImage == null) _iconImage = GetComponent<Image>();

            if (_data.icon != null)
            {
                _iconImage.sprite = _data.icon;
            }

            if (_data.isUnlocked)
            {
                _iconImage.color = unlockedColor;
            }
            else
            {
                _iconImage.color = lockedColor;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _onClick?.Invoke(_data);
        }
    }
}
