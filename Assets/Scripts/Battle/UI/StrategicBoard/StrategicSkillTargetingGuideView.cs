using UnityEngine;

namespace Battle.UI.StrategicBoard
{
    /// <summary>
    /// World-space circular targeting guide used while dragging a strategic skill slot.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class StrategicSkillTargetingGuideView : MonoBehaviour
    {
        private const float MinimumCalibrationValue = 0.01f;

        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Gameplay Radius Calibration")]
        [SerializeField, Min(MinimumCalibrationValue)] private float radiusMultiplier = 1f;
        [SerializeField] private float radiusOffset;

        [Header("Sprite Boundary Calibration")]
        [SerializeField] private Vector2 boundaryDiameterRatio = new(0.94921875f, 0.95703125f);
        [SerializeField] private Vector2 scaleMultiplier = Vector2.one;

        [Header("Position And Orientation Calibration")]
        [SerializeField] private Vector2 worldPositionOffset = Vector2.zero;
        [SerializeField] private float rotationDegrees;

        [Header("Visual Adjustment")]
        [SerializeField] private Color guideColor = Color.white;
        [SerializeField, Range(0f, 1f)] private float alpha = 0.75f;

        [Header("Rendering")]
        [SerializeField] private string sortingLayerName = "Default";
        [SerializeField] private int sortingOrder = 100;

        public bool IsVisible => spriteRenderer != null && spriteRenderer.enabled;
        public bool HasSprite => spriteRenderer != null && spriteRenderer.sprite != null;
        public float GameplayRadius { get; private set; }
        public float DisplayRadius { get; private set; }
        public float WorldDiameter { get; private set; }
        public float Radius => GameplayRadius;

        private void Awake()
        {
            ClampCalibration();
            ResolveRenderer();
            ApplyVisualProperties();
            ApplySorting();
            ApplyRotation();
            Hide();
        }

        private void OnDisable()
        {
            Hide();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ClampCalibration();
            ResolveRenderer();
            ApplyVisualProperties();
            ApplySorting();
            ApplyRotation();
        }
#endif

        public bool Show(Vector3 worldPosition, float gameplayRadius)
        {
            ClampCalibration();
            ResolveRenderer();

            GameplayRadius = gameplayRadius;
            DisplayRadius = Mathf.Max(
                MinimumCalibrationValue,
                gameplayRadius * radiusMultiplier + radiusOffset);
            WorldDiameter = DisplayRadius * 2f;

            if (!TryApplyDiameter(WorldDiameter))
            {
                Hide();
                return false;
            }

            Move(worldPosition);
            ApplyVisualProperties();
            ApplySorting();
            spriteRenderer.enabled = true;
            return true;
        }

        public void Move(Vector3 worldPosition)
        {
            transform.position = worldPosition + new Vector3(
                worldPositionOffset.x,
                worldPositionOffset.y,
                0f);
            ApplyRotation();
        }

        public void UpdatePosition(Vector3 worldPosition)
        {
            Move(worldPosition);
        }

        public void Hide()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }

            GameplayRadius = 0f;
            DisplayRadius = 0f;
            WorldDiameter = 0f;
        }

        public void SetSprite(Sprite sprite)
        {
            ResolveRenderer();

            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = sprite;
            }
        }

        public void SetSorting(string layerName, int order)
        {
            sortingLayerName = layerName;
            sortingOrder = order;
            ApplySorting();
        }

        public void ConfigureCalibration(
            float newRadiusMultiplier,
            float newRadiusOffset,
            Vector2 newBoundaryDiameterRatio,
            Vector2 newScaleMultiplier,
            Vector2 newWorldPositionOffset,
            float newRotationDegrees,
            Color newGuideColor,
            float newAlpha)
        {
            radiusMultiplier = newRadiusMultiplier;
            radiusOffset = newRadiusOffset;
            boundaryDiameterRatio = newBoundaryDiameterRatio;
            scaleMultiplier = newScaleMultiplier;
            worldPositionOffset = newWorldPositionOffset;
            rotationDegrees = newRotationDegrees;
            guideColor = newGuideColor;
            alpha = newAlpha;

            ClampCalibration();
            ResolveRenderer();
            ApplyVisualProperties();
            ApplySorting();
            ApplyRotation();
        }

        private bool TryApplyDiameter(float diameter)
        {
            if (spriteRenderer == null || spriteRenderer.sprite == null || diameter <= 0f)
            {
                return false;
            }

            Vector2 spriteSize = spriteRenderer.sprite.bounds.size;

            if (spriteSize.x <= Mathf.Epsilon || spriteSize.y <= Mathf.Epsilon)
            {
                return false;
            }

            Vector3 parentScale = transform.parent != null
                ? transform.parent.lossyScale
                : Vector3.one;
            float parentScaleX = Mathf.Max(Mathf.Abs(parentScale.x), Mathf.Epsilon);
            float parentScaleY = Mathf.Max(Mathf.Abs(parentScale.y), Mathf.Epsilon);

            transform.localScale = new Vector3(
                diameter / (spriteSize.x * boundaryDiameterRatio.x * parentScaleX) * scaleMultiplier.x,
                diameter / (spriteSize.y * boundaryDiameterRatio.y * parentScaleY) * scaleMultiplier.y,
                1f);
            return true;
        }

        private void ClampCalibration()
        {
            radiusMultiplier = Mathf.Max(MinimumCalibrationValue, radiusMultiplier);
            boundaryDiameterRatio = new Vector2(
                Mathf.Max(MinimumCalibrationValue, boundaryDiameterRatio.x),
                Mathf.Max(MinimumCalibrationValue, boundaryDiameterRatio.y));
            scaleMultiplier = new Vector2(
                Mathf.Max(MinimumCalibrationValue, scaleMultiplier.x),
                Mathf.Max(MinimumCalibrationValue, scaleMultiplier.y));
            alpha = Mathf.Clamp01(alpha);
        }

        private void ResolveRenderer()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
        }

        private void ApplySorting()
        {
            if (spriteRenderer == null)
            {
                return;
            }

            spriteRenderer.sortingLayerName = sortingLayerName;
            spriteRenderer.sortingOrder = sortingOrder;
        }

        private void ApplyVisualProperties()
        {
            if (spriteRenderer == null)
            {
                return;
            }

            spriteRenderer.color = new Color(
                guideColor.r,
                guideColor.g,
                guideColor.b,
                alpha);
        }

        private void ApplyRotation()
        {
            transform.rotation = Quaternion.Euler(0f, 0f, rotationDegrees);
        }
    }
}
