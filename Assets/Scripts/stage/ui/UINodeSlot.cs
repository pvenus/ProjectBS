using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Stage.UI
{
    public enum NodeVisualState
    {
        Disabled,
        Normal,
        Hover,
        Click
    }

    /// <summary>
    /// 노드 슬롯 프리팹의 시각적 요소를 관리하는 기본 컴포넌트.
    /// 버튼의 호버 / 클릭 / 활성 / 비활성 상태 및 애니메이션 연출을 담당합니다.
    /// </summary>
    public class UINodeSlot : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] protected Image iconImage;
        [SerializeField] protected Image backgroundImage;
        [SerializeField] protected RectTransform visualTarget;

        [Header("Color Settings")]
        [SerializeField] protected Color normalColor = Color.white;
        [SerializeField] protected Color disabledColor = new Color(0.35f, 0.35f, 0.35f, 1f);
        [SerializeField] protected Color completedColor = new Color(0.6f, 0.6f, 0.6f, 1f);

        [Header("Animation Settings")]
        [SerializeField] private float hoverScale = 1.12f;
        [SerializeField] private float clickPunchScale = 1.25f;
        [SerializeField] private float animationSpeed = 15f;

        protected NodeVisualState currentState = NodeVisualState.Normal;
        protected bool isAvailable = true;
        protected bool isCompleted = false;

        private Vector3 baseScale = Vector3.one;
        private Vector3 targetScale = Vector3.one;
        private Coroutine scaleCoroutine;
        private Coroutine clickAnimCoroutine;

        protected virtual void Awake()
        {
            BindComponents();
            if (visualTarget == null)
            {
                visualTarget = transform as RectTransform;
            }
            if (visualTarget != null)
            {
                baseScale = visualTarget.localScale;
                targetScale = baseScale;
            }
        }

        protected virtual void BindComponents()
        {
            if (iconImage == null)
            {
                var iconTransform = transform.Find("Bind_IconImage") ?? transform.Find("iconImage") ?? transform.Find("Icon");
                if (iconTransform != null)
                {
                    iconImage = iconTransform.GetComponent<Image>();
                }
            }

            if (backgroundImage == null)
            {
                var bgTransform = transform.Find("(img)background") ?? transform.Find("background") ?? transform.Find("Background");
                if (bgTransform != null)
                {
                    backgroundImage = bgTransform.GetComponent<Image>();
                }
                else
                {
                    backgroundImage = GetComponent<Image>();
                }
            }

            DisableChildRaycastTargets();
        }

        private void DisableChildRaycastTargets()
        {
            Image[] images = GetComponentsInChildren<Image>(true);
            foreach (Image img in images)
            {
                if (img != null && img.gameObject != this.gameObject)
                {
                    img.raycastTarget = false;
                }
            }
        }

        public virtual void SetNodeData(RoundNode node)
        {
            if (node == null) return;

            isAvailable = node.IsAvailable;
            isCompleted = node.IsCompleted;

            if (iconImage != null)
            {
                iconImage.enabled = true;
            }

            UpdateState(isAvailable ? NodeVisualState.Normal : NodeVisualState.Disabled);
        }

        public virtual void UpdateState(NodeVisualState newState)
        {
            currentState = newState;
            ApplyColorState(newState);
            ApplyScaleState(newState);
            ApplyShaderState(newState);
        }

        protected virtual void ApplyColorState(NodeVisualState state)
        {
            Color targetColor = normalColor;

            if (isCompleted)
            {
                targetColor = completedColor;
            }
            else if (!isAvailable || state == NodeVisualState.Disabled)
            {
                targetColor = disabledColor;
            }
            else
            {
                targetColor = normalColor;
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = targetColor;
            }

            if (iconImage != null)
            {
                // 아이콘도 어두운 톤 적용
                iconImage.color = targetColor;
            }
        }

        protected virtual void ApplyScaleState(NodeVisualState state)
        {
            if (visualTarget == null) return;

            switch (state)
            {
                case NodeVisualState.Disabled:
                case NodeVisualState.Normal:
                    targetScale = baseScale;
                    StartScaleAnimation(targetScale);
                    break;

                case NodeVisualState.Hover:
                    if (isAvailable && !isCompleted)
                    {
                        targetScale = baseScale * hoverScale;
                        StartScaleAnimation(targetScale);
                    }
                    break;

                case NodeVisualState.Click:
                    if (isAvailable && !isCompleted)
                    {
                        StartClickPulseAnimation();
                    }
                    break;
            }
        }

        protected virtual void ApplyShaderState(NodeVisualState state)
        {
            // 추후 호버/상태별 머티리얼/쉐이더 세팅 적용을 위한 가상 메서드
        }

        public virtual void OnPointerEnter()
        {
            if (!isAvailable || isCompleted) return;
            UpdateState(NodeVisualState.Hover);
        }

        public virtual void OnPointerExit()
        {
            if (!isAvailable || isCompleted)
            {
                UpdateState(NodeVisualState.Disabled);
                return;
            }
            UpdateState(NodeVisualState.Normal);
        }

        public virtual void OnPointerClick()
        {
            if (!isAvailable || isCompleted) return;
            UpdateState(NodeVisualState.Click);
        }

        private void StartScaleAnimation(Vector3 target)
        {
            if (clickAnimCoroutine != null)
            {
                StopCoroutine(clickAnimCoroutine);
                clickAnimCoroutine = null;
            }

            if (scaleCoroutine != null)
            {
                StopCoroutine(scaleCoroutine);
            }

            scaleCoroutine = StartCoroutine(AnimateScaleRoutine(target));
        }

        private IEnumerator AnimateScaleRoutine(Vector3 target)
        {
            if (visualTarget == null) yield break;

            while (Vector3.Distance(visualTarget.localScale, target) > 0.001f)
            {
                visualTarget.localScale = Vector3.Lerp(visualTarget.localScale, target, Time.unscaledDeltaTime * animationSpeed);
                yield return null;
            }

            visualTarget.localScale = target;
        }

        private void StartClickPulseAnimation()
        {
            if (scaleCoroutine != null)
            {
                StopCoroutine(scaleCoroutine);
                scaleCoroutine = null;
            }

            if (clickAnimCoroutine != null)
            {
                StopCoroutine(clickAnimCoroutine);
            }

            clickAnimCoroutine = StartCoroutine(ClickPulseRoutine());
        }

        private IEnumerator ClickPulseRoutine()
        {
            if (visualTarget == null) yield break;

            // 1. 커졌다 (Punch)
            Vector3 punchScale = baseScale * clickPunchScale;
            float elapsed = 0f;
            float duration = 0.08f;

            Vector3 startScale = visualTarget.localScale;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                visualTarget.localScale = Vector3.Lerp(startScale, punchScale, elapsed / duration);
                yield return null;
            }

            // 2. 원래/호버 상태로 복귀
            elapsed = 0f;
            duration = 0.12f;
            Vector3 returnTarget = (currentState == NodeVisualState.Hover) ? baseScale * hoverScale : baseScale;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                visualTarget.localScale = Vector3.Lerp(punchScale, returnTarget, elapsed / duration);
                yield return null;
            }

            visualTarget.localScale = returnTarget;
        }

        public virtual void SetBackgroundColor(Color color)
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = color;
            }
        }
    }
}
