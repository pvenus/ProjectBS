using UnityEngine;

namespace Battle.Presentation
{
    [DefaultExecutionOrder(1000)]
    [DisallowMultipleComponent]
    public sealed class BattleCharacterAuraView : MonoBehaviour
    {
        [Header("Visual")]
        [SerializeField] private SpriteRenderer backArcRenderer;
        [SerializeField] private SpriteRenderer frontArcRenderer;
        [SerializeField] private Color defaultColor =
            new(0.8f, 0.22f, 0.08f, 0.42f);
        [Tooltip("Shared local offset for both arcs. Make Y more negative to move the aura down toward the character's feet.")]
        [SerializeField] private Vector3 positionOffset =
            new(0f, -0.12f, 0f);
        [Tooltip("Shared scale for both arcs. Reduce X and Y together to make the entire aura smaller without breaking arc alignment.")]
        [SerializeField] private Vector2 visualScale = Vector2.one;
        [Tooltip("Keeps the aura's world size independent from the parent character scale.")]
        [SerializeField] private bool compensateParentScale = true;

        [Header("Sorting")]
        [SerializeField] private bool followCharacterSorting = true;
        [SerializeField, Min(1)] private int backOrderOffset = 1;
        [SerializeField, Min(1)] private int frontOrderOffset = 1;
        [SerializeField] private string fallbackSortingLayerName = "Default";
        [SerializeField] private int fallbackBackSortingOrder = -1;
        [SerializeField] private int fallbackFrontSortingOrder = 1;

        private SpriteRenderer[] characterRenderers;

        public SpriteRenderer BackArcRenderer => backArcRenderer;
        public SpriteRenderer FrontArcRenderer => frontArcRenderer;
        public Color DefaultColor => defaultColor;
        public Vector3 PositionOffset => positionOffset;
        public Vector2 VisualScale => visualScale;

        private void Awake()
        {
            ResolveRenderers();
            ApplyDefaultColor();
            ApplyVisualTransform();
            ApplyFallbackSorting();
        }

        private void OnValidate()
        {
            ResolveRenderers();
            ApplyDefaultColor();
            ApplyVisualTransform();

            if (!followCharacterSorting)
            {
                ApplyFallbackSorting();
            }
        }

        private void LateUpdate()
        {
            ApplyParentScaleCompensation();
            ApplySorting();
        }

        public void Initialize(SpriteRenderer[] targetRenderers)
        {
            characterRenderers = targetRenderers;

            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            ApplyVisualTransform();
            ApplyParentScaleCompensation();
            ApplySorting();
        }

        public void ConfigureVisual(
            Sprite backSprite,
            Sprite frontSprite,
            Color color,
            Material backMaterial,
            Material frontMaterial,
            Vector2 scale,
            Vector3 offset)
        {
            SetSprites(backSprite, frontSprite);
            SetColor(color);
            SetMaterials(backMaterial, frontMaterial);
            SetVisualScale(scale);
            SetPositionOffset(offset);
        }

        public void SetSprites(
            Sprite backSprite,
            Sprite frontSprite)
        {
            ResolveRenderers();
            backArcRenderer.sprite = backSprite;
            frontArcRenderer.sprite = frontSprite;
        }

        public void SetColor(Color color)
        {
            ResolveRenderers();
            backArcRenderer.color = color;
            frontArcRenderer.color = color;
        }

        public void SetMaterials(
            Material backMaterial,
            Material frontMaterial)
        {
            ResolveRenderers();
            backArcRenderer.sharedMaterial = backMaterial;
            frontArcRenderer.sharedMaterial = frontMaterial;
        }

        public void SetVisualScale(Vector2 scale)
        {
            visualScale = new Vector2(
                Mathf.Max(0.01f, Mathf.Abs(scale.x)),
                Mathf.Max(0.01f, Mathf.Abs(scale.y)));
            ApplyVisualTransform();
        }

        public void SetPositionOffset(Vector3 offset)
        {
            positionOffset = offset;
            ApplyVisualTransform();
        }

        public void SetSortingOrderOffsets(
            int backOffset,
            int frontOffset)
        {
            followCharacterSorting = true;
            backOrderOffset = Mathf.Max(1, backOffset);
            frontOrderOffset = Mathf.Max(1, frontOffset);
            ApplySorting();
        }

        public void UseAbsoluteSorting(
            string sortingLayerName,
            int backSortingOrder,
            int frontSortingOrder)
        {
            followCharacterSorting = false;
            fallbackSortingLayerName = sortingLayerName;
            fallbackBackSortingOrder = backSortingOrder;
            fallbackFrontSortingOrder = frontSortingOrder;
            ApplyFallbackSorting();
        }

        private void ResolveRenderers()
        {
            if (backArcRenderer != null
                && frontArcRenderer != null)
            {
                return;
            }

            SpriteRenderer[] renderers =
                GetComponentsInChildren<SpriteRenderer>(true);

            for (int i = 0; i < renderers.Length; i++)
            {
                SpriteRenderer renderer = renderers[i];

                if (renderer == null)
                {
                    continue;
                }

                if (renderer.name == "BackArc")
                {
                    backArcRenderer = renderer;
                }
                else if (renderer.name == "FrontArc")
                {
                    frontArcRenderer = renderer;
                }
            }
        }

        private void ApplyDefaultColor()
        {
            if (backArcRenderer == null
                || frontArcRenderer == null)
            {
                return;
            }

            backArcRenderer.color = defaultColor;
            frontArcRenderer.color = defaultColor;
        }

        private void ApplyVisualTransform()
        {
            if (backArcRenderer == null
                || frontArcRenderer == null)
            {
                return;
            }

            Vector3 rendererScale = new(
                Mathf.Max(0.01f, Mathf.Abs(visualScale.x)),
                Mathf.Max(0.01f, Mathf.Abs(visualScale.y)),
                1f);

            ApplyRendererTransform(backArcRenderer, rendererScale);
            ApplyRendererTransform(frontArcRenderer, rendererScale);
        }

        private void ApplyRendererTransform(
            SpriteRenderer renderer,
            Vector3 rendererScale)
        {
            renderer.transform.localPosition = positionOffset;
            renderer.transform.localRotation = Quaternion.identity;
            renderer.transform.localScale = rendererScale;
        }

        private void ApplyParentScaleCompensation()
        {
            if (!compensateParentScale || transform.parent == null)
            {
                transform.localScale = Vector3.one;
                return;
            }

            Vector3 parentScale = transform.parent.lossyScale;
            transform.localScale = new Vector3(
                GetSafeReciprocal(parentScale.x),
                GetSafeReciprocal(parentScale.y),
                GetSafeReciprocal(parentScale.z));
        }

        private static float GetSafeReciprocal(float value)
        {
            return Mathf.Abs(value) > 0.0001f
                ? 1f / value
                : 1f;
        }

        private void ApplySorting()
        {
            if (backArcRenderer == null
                || frontArcRenderer == null)
            {
                return;
            }

            if (followCharacterSorting
                && TryGetCharacterSortingRange(
                    out int sortingLayerId,
                    out int minimumOrder,
                    out int maximumOrder))
            {
                backArcRenderer.sortingLayerID = sortingLayerId;
                frontArcRenderer.sortingLayerID = sortingLayerId;
                backArcRenderer.sortingOrder =
                    minimumOrder - Mathf.Max(1, backOrderOffset);
                frontArcRenderer.sortingOrder =
                    maximumOrder + Mathf.Max(1, frontOrderOffset);
                return;
            }

            ApplyFallbackSorting();
        }

        private bool TryGetCharacterSortingRange(
            out int sortingLayerId,
            out int minimumOrder,
            out int maximumOrder)
        {
            sortingLayerId = 0;
            minimumOrder = 0;
            maximumOrder = 0;

            if (characterRenderers == null
                || characterRenderers.Length == 0)
            {
                return false;
            }

            bool hasRenderer = false;

            for (int i = 0; i < characterRenderers.Length; i++)
            {
                SpriteRenderer renderer = characterRenderers[i];

                if (renderer == null)
                {
                    continue;
                }

                if (!hasRenderer)
                {
                    sortingLayerId = renderer.sortingLayerID;
                    minimumOrder = renderer.sortingOrder;
                    maximumOrder = renderer.sortingOrder;
                    hasRenderer = true;
                    continue;
                }

                minimumOrder = Mathf.Min(
                    minimumOrder,
                    renderer.sortingOrder);
                maximumOrder = Mathf.Max(
                    maximumOrder,
                    renderer.sortingOrder);
            }

            return hasRenderer;
        }

        private void ApplyFallbackSorting()
        {
            if (backArcRenderer == null
                || frontArcRenderer == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(fallbackSortingLayerName))
            {
                backArcRenderer.sortingLayerName =
                    fallbackSortingLayerName;
                frontArcRenderer.sortingLayerName =
                    fallbackSortingLayerName;
            }

            backArcRenderer.sortingOrder = fallbackBackSortingOrder;
            frontArcRenderer.sortingOrder = fallbackFrontSortingOrder;
        }
    }
}
