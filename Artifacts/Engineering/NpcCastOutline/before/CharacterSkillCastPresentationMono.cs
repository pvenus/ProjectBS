using System.Collections;
using UnityEngine;

namespace Character
{
    /// <summary>
    /// Optional, renderer-only cast progress presentation. Gameplay never depends
    /// on this component or on shader availability.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CharacterSkillCastPresentationMono : MonoBehaviour
    {
        public const float CancelFadeSeconds = .12f;
        public const float CompleteRestoreSeconds = .04f;
        public const float AccentStartProgress = .88f;
        public const float MinimumPulseHz = .8f;
        public const float MaximumPulseHz = 1.8f;
        public const float MaximumEdgeIntensity = .32f;
        public const float TerminalIntensity = .4f;
        public const float TerminalDurationSeconds = .08f;
        public const float ReducedFlashTerminalIntensity = .2f;
        public const float ReducedFlashTerminalDurationSeconds = .06f;

        private static readonly int CastEnabledId = Shader.PropertyToID("_CastEnabled");
        private static readonly int CastProgressId = Shader.PropertyToID("_CastProgress");
        private static readonly int CastTimeId = Shader.PropertyToID("_CastTime");
        private static readonly int CastBaseColorId = Shader.PropertyToID("_CastBaseColor");
        private static readonly int CastAccentColorId = Shader.PropertyToID("_CastAccentColor");
        private static readonly int CastPulseMinId = Shader.PropertyToID("_CastPulseHzMin");
        private static readonly int CastPulseMaxId = Shader.PropertyToID("_CastPulseHzMax");
        private static readonly int CastEdgeMinId = Shader.PropertyToID("_CastEdgeIntensityMin");
        private static readonly int CastEdgeMaxId = Shader.PropertyToID("_CastEdgeIntensityMax");
        private static readonly int CastInteriorMaxId = Shader.PropertyToID("_CastInteriorLiftMax");
        private static readonly int CastAccentStartId = Shader.PropertyToID("_CastTerminalStart");
        private static readonly int CastTerminalIntensityId = Shader.PropertyToID("_CastTerminalIntensity");
        private static readonly int CastCompleteFlashId = Shader.PropertyToID("_CastCompleteFlash");
        private static readonly int CastReducedFlashId = Shader.PropertyToID("_CastReducedFlash");
        private static readonly int CastReducedMotionId = Shader.PropertyToID("_CastReducedMotion");

        private SpriteRenderer targetRenderer;
        private Material baselineMaterial;
        private Material castMaterial;
        private MaterialPropertyBlock baselineBlock;
        private MaterialPropertyBlock activeBlock;
        private Coroutine restoreRoutine;
        private bool presenting;
        private bool reducedFlash;
        private bool reducedMotion;
        private float presentationTime;
        private float currentProgress;
        private int presentationGeneration;

        public bool IsPresenting => presenting;

        public void ConfigureAccessibility(bool useReducedFlash, bool useReducedMotion)
        {
            reducedFlash = useReducedFlash;
            reducedMotion = useReducedMotion;
            if (presenting) ApplyOwnedProperties();
        }

        public bool BeginPresentation(float duration)
        {
            RestoreImmediate();
            targetRenderer = ResolveRenderer();
            if (targetRenderer == null)
            {
                return false;
            }

            Shader shader = Resources.Load<Shader>("Shaders/CharacterCastProgress")
                ?? Shader.Find("ProjectBS/CharacterCastProgress");
            if (shader == null)
            {
                targetRenderer = null;
                return false;
            }

            baselineMaterial = targetRenderer.sharedMaterial;
            baselineBlock ??= new MaterialPropertyBlock();
            activeBlock ??= new MaterialPropertyBlock();
            targetRenderer.GetPropertyBlock(baselineBlock);
            // A pooled renderer can outlive an interrupted presenter. Cast-owned
            // values are never a valid baseline for the next lease.
            ClearOwnedProperties(baselineBlock);
            targetRenderer.GetPropertyBlock(activeBlock);
            ClearOwnedProperties(activeBlock);

            castMaterial = baselineMaterial != null
                ? new Material(baselineMaterial) { shader = shader }
                : new Material(shader);
            castMaterial.name = "CharacterCastProgress (Runtime)";
            targetRenderer.sharedMaterial = castMaterial;
            presenting = true;
            presentationTime = 0f;
            SetProgress(0f);
            return true;
        }

        public void SetProgress(float progress)
        {
            if (!presenting || targetRenderer == null)
            {
                return;
            }

            currentProgress = Mathf.Clamp01(progress);
            ApplyOwnedProperties();
        }

        private void ApplyOwnedProperties()
        {
            if (!presenting || targetRenderer == null) return;
            targetRenderer.GetPropertyBlock(activeBlock);
            activeBlock.SetFloat(CastEnabledId, 1f);
            activeBlock.SetFloat(CastProgressId, currentProgress);
            activeBlock.SetFloat(CastTimeId, presentationTime);
            activeBlock.SetColor(CastBaseColorId, new Color32(0x86, 0xAF, 0xC8, 0xFF));
            activeBlock.SetColor(CastAccentColorId, new Color32(0xD4, 0x6A, 0x45, 0xFF));
            activeBlock.SetFloat(CastPulseMinId, MinimumPulseHz);
            activeBlock.SetFloat(CastPulseMaxId, MaximumPulseHz);
            activeBlock.SetFloat(CastEdgeMinId, .08f);
            activeBlock.SetFloat(CastEdgeMaxId, MaximumEdgeIntensity);
            activeBlock.SetFloat(CastInteriorMaxId, .08f);
            activeBlock.SetFloat(CastAccentStartId, AccentStartProgress);
            activeBlock.SetFloat(CastTerminalIntensityId,
                reducedFlash ? ReducedFlashTerminalIntensity : TerminalIntensity);
            activeBlock.SetFloat(CastReducedFlashId, reducedFlash ? 1f : 0f);
            activeBlock.SetFloat(CastReducedMotionId, reducedMotion ? 1f : 0f);
            targetRenderer.SetPropertyBlock(activeBlock);
        }

        public void CancelPresentation()
        {
            BeginRestore(CancelFadeSeconds, ++presentationGeneration);
        }

        public void CompletePresentation()
        {
            SetProgress(1f);
            if (restoreRoutine != null) StopCoroutine(restoreRoutine);
            if (!CanRunPresentationCoroutine())
            {
                RestoreImmediate();
                return;
            }
            int generation = presentationGeneration;
            restoreRoutine = StartCoroutine(CompleteRoutine(generation));
        }

        public void RestoreImmediate()
        {
            presentationGeneration++;
            if (restoreRoutine != null)
            {
                StopCoroutine(restoreRoutine);
                restoreRoutine = null;
            }

            if (targetRenderer != null)
            {
                // Do not roll back a newer death/hit/body presentation owner.
                if (targetRenderer.sharedMaterial == castMaterial)
                    targetRenderer.sharedMaterial = baselineMaterial;

                activeBlock ??= new MaterialPropertyBlock();
                targetRenderer.GetPropertyBlock(activeBlock);
                ClearOwnedProperties(activeBlock);
                targetRenderer.SetPropertyBlock(activeBlock);
            }

            if (castMaterial != null)
            {
                Destroy(castMaterial);
            }

            castMaterial = null;
            baselineMaterial = null;
            targetRenderer = null;
            presenting = false;
            presentationTime = 0f;
            currentProgress = 0f;
        }

        private void BeginRestore(float duration, int generation)
        {
            if (!presenting)
            {
                return;
            }

            if (!CanRunPresentationCoroutine() || duration <= 0f)
            {
                RestoreImmediate();
                return;
            }

            if (restoreRoutine != null)
            {
                StopCoroutine(restoreRoutine);
            }
            restoreRoutine = StartCoroutine(RestoreRoutine(duration, generation));
        }

        private bool CanRunPresentationCoroutine()
        {
            return enabled && isActiveAndEnabled && gameObject.activeInHierarchy;
        }

        private IEnumerator RestoreRoutine(float duration, int generation)
        {
            float elapsed = 0f;
            float start = activeBlock != null ? activeBlock.GetFloat(CastProgressId) : 0f;
            while (elapsed < duration)
            {
                if (generation != presentationGeneration)
                    yield break;
                if (!presenting || targetRenderer == null)
                {
                    restoreRoutine = null;
                    RestoreImmediate();
                    yield break;
                }
                elapsed += Time.unscaledDeltaTime;
                SetProgress(Mathf.Lerp(start, 0f, Mathf.Clamp01(elapsed / duration)));
                yield return null;
            }
            if (generation != presentationGeneration)
                yield break;
            restoreRoutine = null;
            RestoreImmediate();
        }

        private IEnumerator CompleteRoutine(int generation)
        {
            if (generation != presentationGeneration || !CanRunPresentationCoroutine() ||
                activeBlock == null || targetRenderer == null)
            {
                restoreRoutine = null;
                if (generation == presentationGeneration)
                    RestoreImmediate();
                yield break;
            }

            float hold = reducedFlash
                ? ReducedFlashTerminalDurationSeconds
                : TerminalDurationSeconds;
            activeBlock.SetFloat(CastCompleteFlashId, 1f);
            targetRenderer.SetPropertyBlock(activeBlock);
            float elapsed = 0f;
            while (elapsed < hold)
            {
                if (generation != presentationGeneration)
                    yield break;
                if (targetRenderer == null)
                {
                    restoreRoutine = null;
                    RestoreImmediate();
                    yield break;
                }
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            if (generation != presentationGeneration || targetRenderer == null)
                yield break;
            activeBlock.SetFloat(CastCompleteFlashId, 0f);
            targetRenderer.SetPropertyBlock(activeBlock);
            restoreRoutine = null;
            BeginRestore(CompleteRestoreSeconds, generation);
        }

        private static void ClearOwnedProperties(MaterialPropertyBlock block)
        {
            if (block == null) return;
            block.SetFloat(CastEnabledId, 0f);
            block.SetFloat(CastProgressId, 0f);
            block.SetFloat(CastTimeId, 0f);
            block.SetFloat(CastCompleteFlashId, 0f);
            block.SetFloat(CastReducedFlashId, 0f);
            block.SetFloat(CastReducedMotionId, 0f);
        }

        private void Update()
        {
            if (!presenting) return;
            presentationTime += Time.unscaledDeltaTime;
            ApplyOwnedProperties();
        }

        private SpriteRenderer ResolveRenderer()
        {
            SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                SpriteRenderer candidate = renderers[i];
                if (candidate != null &&
                    candidate.GetComponent<ComboBodyPresentationProxyMarker>() == null)
                {
                    return candidate;
                }
            }
            return null;
        }

        private void OnDisable()
        {
            RestoreImmediate();
        }

        private void OnDestroy()
        {
            RestoreImmediate();
        }
    }
}
