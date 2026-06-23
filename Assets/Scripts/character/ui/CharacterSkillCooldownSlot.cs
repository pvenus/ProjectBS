using TMPro;
using UnityEngine;

namespace Character.UI
{
    public class CharacterSkillCooldownSlot : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer iconRenderer;
        [SerializeField] private TextMeshPro remainText;
        private float remainingSeconds;

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
        }

        public void Hide()
        {
            remainingSeconds = 0f;
            gameObject.SetActive(false);
        }
    }
}