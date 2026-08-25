using System.Collections;
using TMPro;
using UnityEngine;

namespace Character.UI
{
    /// <summary>
    /// 캐릭터가 방금 사용한 스킬의 아이콘을 머리 위에 팝업 연출(확대 후 원복 + 페이드인 + 2초 유지 + 페이드아웃)로 표시하는 뷰 컴포넌트.
    /// 새로운 스킬 사용 시 기존 연출은 즉시 캔슬되고 새 연출이 시작된다.
    /// 스킬 아이콘이 null인 경우 placeholder 없이 즉시 숨긴다.
    /// </summary>
    public class CharacterSkillCooldownSlot : MonoBehaviour
    {
        [Header("Visual Components")]
        [SerializeField] private SpriteRenderer iconRenderer;
        [Tooltip("Legacy cooldown text binding. Retained for prefab compatibility and always disabled at runtime.")]
        [SerializeField] private TextMeshPro remainText;

        [Header("Sorting")]
        [SerializeField] private string sortingLayerName = "Default";
        [SerializeField] private int sortingOrder = 500;

        [Header("Recent Skill Timing")]
        [Tooltip("나타날 때 페이드 인 및 확대에 걸리는 시간")]
        [Min(0f)]
        [SerializeField] private float fadeInDuration = 0.15f;

        [Tooltip("확대된 스케일에서 원래 크기로 부드럽게 돌아오는 시간")]
        [Min(0f)]
        [SerializeField] private float popSettleDuration = 0.10f;

        [Tooltip("스킬 아이콘이 머리 위에 유지되는 시간 (초)")]
        [Min(0f)]
        [SerializeField] private float holdDuration = 2.0f;

        [Tooltip("사라질 때 페이드 아웃에 걸리는 시간")]
        [Min(0f)]
        [SerializeField] private float fadeOutDuration = 0.35f;

        [Header("Recent Skill Scale")]
        [Tooltip("스킬 아이콘 최초 생성 시 시작 스케일")]
        [Min(0f)]
        [SerializeField] private float startScale = 0.6f;

        [Tooltip("뿅 하고 커질 때의 최대 강조 스케일")]
        [Min(0f)]
        [SerializeField] private float popScale = 1.25f;

        [Tooltip("유지될 때의 기본 스케일")]
        [Min(0f)]
        [SerializeField] private float baseScale = 1.0f;

        [Header("Recent Skill Position")]
        [SerializeField] private Vector3 headOffset = new Vector3(0f, 2.2f, 0f);
        [Tooltip("페이드아웃 시 위로 올라가는 거리 (로컬 Y 유닛)")]
        [Min(0f)]
        [SerializeField] private float riseAmount = 0.6f;

        private Coroutine animationRoutine;
        private Color iconBaseColor = Color.white;

        private void Awake()
        {
            EnsureRendererSettings();

            if (iconRenderer != null)
            {
                iconBaseColor = iconRenderer.color;
            }

            if (remainText != null)
            {
                remainText.gameObject.SetActive(false);
            }

            ApplyLayout();
            Hide();
        }

        private void OnDisable()
        {
            StopAnimation();
        }

        private void OnDestroy()
        {
            StopAnimation();
        }

        public void Bind(SpriteRenderer icon, TextMeshPro text)
        {
            iconRenderer = icon;
            remainText = text;

            EnsureRendererSettings();

            if (iconRenderer != null)
            {
                iconBaseColor = iconRenderer.color;
            }

            if (remainText != null)
            {
                remainText.gameObject.SetActive(false);
            }

            ApplyLayout();
            Hide();
        }

        /// <summary>
        /// 방금 사용한 스킬의 아이콘을 팝업 애니메이션과 함께 표시한다.
        /// 이미 연출이 진행 중인 경우 기존 효과를 캔슬하고 새로운 스킬의 연출을 즉시 시작한다.
        /// 아이콘이 null인 경우 placeholder 없이 즉시 Hide 처리한다.
        /// </summary>
        public void ShowRecentSkill(Sprite icon)
        {
            // 진행 중인 기존 연출 즉시 캔슬
            StopAnimation();

            if (icon == null || iconRenderer == null)
            {
                Hide();
                return;
            }

            gameObject.SetActive(true);
            ApplyLayout();
            EnsureRendererSettings();

            iconRenderer.sprite = icon;
            iconRenderer.enabled = true;

            // 초기 상태: 알파 0, 시작 스케일
            SetVisual(0f, startScale);

            animationRoutine = StartCoroutine(PlayRecentSkillAnimationRoutine());
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

        private IEnumerator PlayRecentSkillAnimationRoutine()
        {
            // 1. 나타날 때: 스케일 확대 (startScale -> popScale) + 페이드 인 (Alpha 0 -> 1)
            yield return AnimateVisual(
                fadeInDuration,
                0f,
                1f,
                startScale,
                popScale);

            // 2. 뿅 하고 커진 뒤 원래 크기로 안착 (popScale -> baseScale)
            yield return AnimateVisual(
                popSettleDuration,
                1f,
                1f,
                popScale,
                baseScale);

            // 3. 인스펙터에서 설정한 시간(holdDuration) 동안 고정 위치에서 유지
            float elapsed = 0f;
            while (elapsed < holdDuration)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            // 4. 위로 올라가며 페이드 아웃 (데미지 텍스트 스타일)
            yield return AnimateVisualWithRise(
                fadeOutDuration,
                1f,
                0f,
                baseScale,
                riseAmount);

            animationRoutine = null;
            Hide();
        }

        private IEnumerator AnimateVisual(
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

                // 부드러운 스무스 스텝 이징 (Ease-In-Out)
                float easedT = t * t * (3f - (2f * t));

                SetVisual(
                    Mathf.Lerp(fromAlpha, toAlpha, easedT),
                    Mathf.Lerp(fromScale, toScale, easedT));

                yield return null;
            }

            SetVisual(toAlpha, toScale);
        }

        private IEnumerator AnimateVisualWithRise(
            float duration,
            float fromAlpha,
            float toAlpha,
            float scale,
            float totalRise)
        {
            Vector3 startPos = headOffset;

            if (duration <= 0f)
            {
                SetVisual(toAlpha, scale);
                transform.localPosition = startPos + Vector3.up * totalRise;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // 알파는 선형 감소, 위치는 Ease-Out (처음엔 빠르게, 끝엔 느리게)
                float alphaT = t;
                float riseT = 1f - (1f - t) * (1f - t);

                SetVisual(Mathf.Lerp(fromAlpha, toAlpha, alphaT), scale);
                transform.localPosition = startPos + Vector3.up * (totalRise * riseT);

                yield return null;
            }

            SetVisual(toAlpha, scale);
            transform.localPosition = startPos + Vector3.up * totalRise;
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

            Color color = iconBaseColor;
            color.a = iconBaseColor.a * Mathf.Clamp01(alpha);
            iconRenderer.color = color;
        }

        private void StopAnimation()
        {
            if (animationRoutine != null)
            {
                StopCoroutine(animationRoutine);
                animationRoutine = null;
            }
        }

        private void EnsureRendererSettings()
        {
            if (iconRenderer == null)
            {
                iconRenderer = GetComponentInChildren<SpriteRenderer>(true);
            }

            if (iconRenderer != null)
            {
                if (!string.IsNullOrEmpty(sortingLayerName))
                {
                    iconRenderer.sortingLayerName = sortingLayerName;
                }
                iconRenderer.sortingOrder = sortingOrder;
            }
        }

        private void ApplyLayout()
        {
            transform.localPosition = headOffset;
        }

        private void OnValidate()
        {
            fadeInDuration = Mathf.Max(0f, fadeInDuration);
            popSettleDuration = Mathf.Max(0f, popSettleDuration);
            holdDuration = Mathf.Max(0f, holdDuration);
            fadeOutDuration = Mathf.Max(0f, fadeOutDuration);
            startScale = Mathf.Max(0f, startScale);
            popScale = Mathf.Max(0f, popScale);
            baseScale = Mathf.Max(0f, baseScale);
            riseAmount = Mathf.Max(0f, riseAmount);

            ApplyLayout();
            EnsureRendererSettings();
        }
    }
}
