using System.Collections;
using TMPro;
using UnityEngine;

namespace Character.UI
{
    /// <summary>
    /// Sprite-only View for the most recently used character skill.
    /// The legacy class name is retained to preserve the existing prefab GUID.
    /// </summary>
    public class CharacterSkillCooldownSlot : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer iconRenderer;
        [Tooltip("Legacy cooldown text binding. Retained for prefab compatibility and always disabled at runtime.")]
        [SerializeField] private TextMeshPro remainText;

        [Header("Recent Skill Timing (Battle Time)")]
        [Min(0f)]
        [SerializeField] private float fadeInDuration = 0.12f;
        [Min(0f)]
        [SerializeField] private float holdDuration = 0.8f;
        [Min(0f)]
        [SerializeField] private float fadeOutDuration = 0.25f;

        [Header("Recent Skill Scale")]
        [Min(0f)]
        [SerializeField] private float startScale = 0.85f;
        [Min(0f)]
        [SerializeField] private float emphasisScale = 1.08f;

        [Header("Recent Skill Position")]
        [SerializeField] private Vector3 headOffset =
            new Vector3(0f, 2.2f, 0f);

        private Coroutine animationRoutine;
        private Color iconColor = Color.white;

        private void Awake()
        {
            if (iconRenderer != null)
            {
                iconColor = iconRenderer.color;
            }

            if (remainText != null)
            {
                remainText.gameObject.SetActive(false);
            }

            ApplyLayout();
            Hide();
        }

        private void OnDestroy()
        {
            StopAnimation();
        }

        public void Bind(
            SpriteRenderer icon,
            TextMeshPro text)
        {
            iconRenderer = icon;
            remainText = text;

            if (iconRenderer != null)
            {
                iconColor = iconRenderer.color;
            }

            if (remainText != null)
            {
                remainText.gameObject.SetActive(false);
            }

            ApplyLayout();
            Hide();
        }

        public void ShowRecentSkill(Sprite icon)
        {
            StopAnimation();

            if (icon == null || iconRenderer == null)
            {
                Hide();
                return;
            }

            gameObject.SetActive(true);
            ApplyLayout();
            iconRenderer.sprite = icon;
            iconRenderer.enabled = true;
            SetAlpha(0f);
            transform.localScale = Vector3.one * Mathf.Max(0f, startScale);
            animationRoutine = StartCoroutine(PlayAnimation());
        }

        public void Hide()
        {
            StopAnimation();

            if (iconRenderer != null)
            {
                iconRenderer.sprite = null;
                iconRenderer.enabled = false;
                SetAlpha(0f);
            }

            gameObject.SetActive(false);
        }

        private IEnumerator PlayAnimation()
        {
            yield return Animate(
                fadeInDuration,
                0f,
                1f,
                startScale,
                emphasisScale);

            float settleDuration = Mathf.Min(fadeInDuration, holdDuration);
            yield return Animate(
                settleDuration,
                1f,
                1f,
                emphasisScale,
                1f);

            float remainingHold = Mathf.Max(0f, holdDuration - settleDuration);
            float elapsed = 0f;

            while (elapsed < remainingHold)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            yield return Animate(
                fadeOutDuration,
                1f,
                0f,
                1f,
                1f);

            animationRoutine = null;
            Hide();
        }

        private IEnumerator Animate(
            float duration,
            float fromAlpha,
            float toAlpha,
            float fromScale,
            float toScale)
        {
            if (duration <= 0f)
            {
                SetVisual(toAlpha, toScale);
                yield break;
            }

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float easedT = t * t * (3f - (2f * t));

                SetVisual(
                    Mathf.Lerp(fromAlpha, toAlpha, easedT),
                    Mathf.Lerp(fromScale, toScale, easedT));

                yield return null;
            }

            SetVisual(toAlpha, toScale);
        }

        private void SetVisual(float alpha, float scale)
        {
            SetAlpha(alpha);
            transform.localScale = Vector3.one * Mathf.Max(0f, scale);
        }

        private void SetAlpha(float alpha)
        {
            if (iconRenderer == null)
            {
                return;
            }

            Color color = iconColor;
            color.a *= Mathf.Clamp01(alpha);
            iconRenderer.color = color;
        }

        private void StopAnimation()
        {
            if (animationRoutine == null)
            {
                return;
            }

            StopCoroutine(animationRoutine);
            animationRoutine = null;
        }

        private void ApplyLayout()
        {
            transform.localPosition = headOffset;
        }

        private void OnValidate()
        {
            fadeInDuration = Mathf.Max(0f, fadeInDuration);
            holdDuration = Mathf.Max(0f, holdDuration);
            fadeOutDuration = Mathf.Max(0f, fadeOutDuration);
            startScale = Mathf.Max(0f, startScale);
            emphasisScale = Mathf.Max(0f, emphasisScale);

            ApplyLayout();
        }
    }
}
