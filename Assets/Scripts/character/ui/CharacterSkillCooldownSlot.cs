using TMPro;
using UnityEngine;

namespace Character.UI
{
    public class CharacterSkillCooldownSlot : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer iconRenderer;
        [SerializeField] private TextMeshPro remainText;
        [SerializeField] private int textSortingOrderOffset = 1;
        private float remainingSeconds;

        private void Awake()
        {
            ApplySorting();
        }

        private void Update()
        {
            if (!gameObject.activeSelf)
            {
                return;
            }

            if (remainingSeconds > 0f)
            {
                remainingSeconds -= Time.deltaTime;
            }

            if (remainText != null)
            {
                remainText.text = Mathf.Max(
                    0,
                    Mathf.CeilToInt(remainingSeconds)).ToString();
            }
        }

        public void Bind(
            SpriteRenderer icon,
            TextMeshPro text)
        {
            iconRenderer = icon;
            remainText = text;
            ApplySorting();
        }

        public void Show(
            Sprite icon,
            float remainingSeconds)
        {
            gameObject.SetActive(true);

            this.remainingSeconds = remainingSeconds;

            if (iconRenderer != null)
            {
                iconRenderer.sprite = icon;
                iconRenderer.enabled = icon != null;
            }

            if (remainText != null)
            {
                remainText.text = Mathf.CeilToInt(remainingSeconds).ToString();
            }

            ApplySorting();
        }

        public void Hide()
        {
            remainingSeconds = 0f;
            gameObject.SetActive(false);
        }

        private void ApplySorting()
        {
            if (iconRenderer == null || remainText == null)
            {
                return;
            }

            MeshRenderer textRenderer =
                remainText.GetComponent<MeshRenderer>();

            if (textRenderer == null)
            {
                return;
            }

            textRenderer.sortingLayerID = iconRenderer.sortingLayerID;
            textRenderer.sortingOrder =
                iconRenderer.sortingOrder + textSortingOrderOffset;
        }
    }
}
