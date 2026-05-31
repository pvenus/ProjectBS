using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Item.UI
{
    public class ItemStrategicSkillSlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("UI")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text costText;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Drag")]
        [SerializeField] private RectTransform dragVisual;
        [SerializeField, Range(0f, 1f)] private float draggingAlpha = 0.5f;

        private ItemStrategicSkillUI owner;
        private StrategicSkillItemSO strategicSkillItem;
        private RectTransform rectTransform;
        private Canvas rootCanvas;
        private Vector2 originalAnchoredPosition;

        public StrategicSkillItemSO StrategicSkillItem => strategicSkillItem;

        private void Awake()
        {
            rectTransform = transform as RectTransform;
            rootCanvas = GetComponentInParent<Canvas>();

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            if (dragVisual == null)
            {
                dragVisual = rectTransform;
            }
        }

        public void Initialize(
            ItemStrategicSkillUI owner,
            StrategicSkillItemSO strategicSkillItem)
        {
            this.owner = owner;
            this.strategicSkillItem = strategicSkillItem;

            if (iconImage != null)
            {
                iconImage.sprite = strategicSkillItem != null ? strategicSkillItem.icon : null;
                iconImage.enabled = iconImage.sprite != null;
            }

            if (costText != null)
            {
                costText.text = strategicSkillItem != null
                    ? strategicSkillItem.gaugeCost.ToString()
                    : string.Empty;
            }

            if (nameText != null)
            {
                nameText.text = strategicSkillItem != null
                    ? strategicSkillItem.DisplayName
                    : string.Empty;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (strategicSkillItem == null)
            {
                return;
            }

            if (dragVisual != null)
            {
                originalAnchoredPosition = dragVisual.anchoredPosition;
            }

            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = false;
                canvasGroup.alpha = draggingAlpha;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (dragVisual == null)
            {
                return;
            }

            float scaleFactor = rootCanvas != null ? rootCanvas.scaleFactor : 1f;
            dragVisual.anchoredPosition += eventData.delta / Mathf.Max(0.01f, scaleFactor);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (dragVisual != null)
            {
                dragVisual.anchoredPosition = originalAnchoredPosition;
            }

            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = true;
                canvasGroup.alpha = 1f;
            }

            if (owner == null || strategicSkillItem == null)
            {
                return;
            }

            owner.TryUseStrategicSkillItem(
                strategicSkillItem,
                eventData.position);
        }
    }
}