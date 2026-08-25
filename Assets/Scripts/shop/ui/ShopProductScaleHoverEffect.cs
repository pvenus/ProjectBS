using UnityEngine;

namespace Shop
{
    /// <summary>
    /// 호버 중 상품의 크기를 부드럽게 확대한다.
    /// </summary>
    public class ShopProductScaleHoverEffect : ShopProductHoverEffectBase
    {
        [SerializeField] private Transform target;
        [SerializeField, Min(1f)] private float hoverScale = 1.06f;
        [SerializeField, Min(0f)] private float smoothTime = 0.08f;

        private Vector3 baseScale;
        private Vector3 targetScale;
        private Vector3 scaleVelocity;
        private bool isInitialized;

        public Transform Target => target;
        public float HoverScale => hoverScale;
        public float SmoothTime => smoothTime;

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            Initialize();
            ResetScaleImmediately();
        }

        private void Update()
        {
            if (!isInitialized || target == null)
            {
                return;
            }

            if (smoothTime <= 0f)
            {
                target.localScale = targetScale;
                return;
            }

            target.localScale = Vector3.SmoothDamp(
                target.localScale,
                targetScale,
                ref scaleVelocity,
                smoothTime,
                Mathf.Infinity,
                Time.unscaledDeltaTime);

            if ((target.localScale - targetScale).sqrMagnitude <= 0.000001f)
            {
                target.localScale = targetScale;
                scaleVelocity = Vector3.zero;
            }
        }

        public override void SetHovered(bool hovered)
        {
            ApplyScaleEffect(hovered);
        }

        protected virtual void ApplyScaleEffect(bool hovered)
        {
            Initialize();

            if (target == null)
            {
                return;
            }

            targetScale = hovered
                ? baseScale * hoverScale
                : baseScale;

            if (smoothTime <= 0f || !isActiveAndEnabled)
            {
                target.localScale = targetScale;
                scaleVelocity = Vector3.zero;
            }
        }

        private void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            if (target == null)
            {
                target = transform;
            }

            baseScale = target.localScale;
            targetScale = baseScale;
            isInitialized = true;
        }

        private void ResetScaleImmediately()
        {
            if (target == null)
            {
                return;
            }

            targetScale = baseScale;
            target.localScale = baseScale;
            scaleVelocity = Vector3.zero;
        }
    }
}
