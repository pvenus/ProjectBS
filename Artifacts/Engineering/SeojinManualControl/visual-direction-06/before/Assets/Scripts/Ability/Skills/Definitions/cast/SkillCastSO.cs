using System;
using UnityEngine;

namespace Skill
{
    /// <summary>
    /// 스킬이 언제, 어떤 방식으로 발동되는지를 정의하는 SO.
    /// 데미지나 속성, 업그레이드 결과는 포함하지 않고
    /// 순수하게 발동 구조만 담당한다.
    /// </summary>
    [CreateAssetMenu(fileName = "SkillCastSO", menuName = "Game/Skill/SkillCastSO")]
    public class SkillCastSO : ScriptableObject
    {
        [SerializeField] private string castId;
        [Header("Timing")]
        [SerializeField, Min(0f)] private float cooldown = 1f;
        [SerializeField, Min(0f)] private float castTime = 0f;
        [SerializeField, Min(0f)] private float range = 5f;
        [SerializeField] private TargetingType targetingType = TargetingType.AutoTarget;

        [Header("Burst / Repeat")]
        [SerializeField] private BurstProfile burst = new();

        [Header("Cast Settings")]
        [SerializeField] private CastMoveProfile castMove = new();

        [Header("Self Effects")]
        [SerializeField] private Effect.EffectEntrySO[] selfEffects;

        [Header("Mobility Terminal Effects")]
        [SerializeField] private Effect.EffectEntrySO[] postMoveSelfEffects;

        [Header("Mobility Presentation (character body only)")]
        [SerializeField] private AnimationClip mobilityVfxClip;
        [SerializeField] private MobilityBodyPresentationProfile mobilityBodyPresentation = new();

        [Header("Optional Character Body Action")]
        [SerializeField] private AnimationClip bodyActionClip;
        [SerializeField] private SkillBodyActionPlaybackProfile bodyActionPlayback = new();

        [Header("Flags")]
        [SerializeField] private bool skipAttackAnimation;
        [SerializeField] private bool snapshotTargetPointOnCast;
        public string CastId => castId;
        public float Cooldown => cooldown;
        public float CastTime => castTime;
        public float Range => range;
        public int BurstCount => burst.Count;
        public float BurstInterval => burst.Interval;

        public TargetingType TargetingType => targetingType;
        public CastMoveProfile CastMove => castMove;

        public Effect.EffectEntrySO[] SelfEffects => selfEffects;
        public Effect.EffectEntrySO[] PostMoveSelfEffects => postMoveSelfEffects;
        public AnimationClip MobilityVfxClip => mobilityVfxClip;
        public MobilityBodyPresentationProfile MobilityBodyPresentation => mobilityBodyPresentation;
        public AnimationClip BodyActionClip => bodyActionClip;
        public SkillBodyActionPlaybackProfile BodyActionPlayback => bodyActionPlayback;

        public bool SkipAttackAnimation => skipAttackAnimation;
        public bool SnapshotTargetPointOnCast => snapshotTargetPointOnCast;

#if UNITY_EDITOR
        public void ApplyEditorData(
            string castId,
            TargetingType targetingType,
            float castTime,
            float cooldown,
            float range,
            bool skipAttackAnimation,
            Effect.EffectEntrySO[] selfEffects)
        {
            this.castId = castId;
            this.targetingType = targetingType;
            this.castTime = castTime;
            this.cooldown = cooldown;
            this.range = range;
            this.skipAttackAnimation = skipAttackAnimation;
            this.selfEffects = selfEffects;
        }

        public void ApplyEditorMobilityData(
            Effect.EffectEntrySO[] terminalEffects,
            AnimationClip vfxClip,
            MobilityBodyPresentationProfile bodyPresentation = null)
        {
            postMoveSelfEffects = terminalEffects;
            mobilityVfxClip = vfxClip;
            mobilityBodyPresentation = bodyPresentation ?? new MobilityBodyPresentationProfile();
        }

        public void ApplyEditorBodyActionData(
            AnimationClip clip,
            SkillBodyActionPlaybackProfile playback)
        {
            bodyActionClip = clip;
            bodyActionPlayback = playback ?? new SkillBodyActionPlaybackProfile();
        }

        public void ApplyEditorTargetSnapshotPolicy(bool enabled)
        {
            snapshotTargetPointOnCast = enabled;
        }

        public void ApplyEditorBurst(
            int count,
            float interval)
        {
            burst.ApplyEditorData(count, interval);
        }

        public void ApplyEditorCastMove(
            CastMoveType moveType,
            float distance,
            float duration,
            float anticipation = 0f,
            float wallSkin = 0.1f,
            float targetClearance = 0.35f,
            float minSuccessDistance = 0.4f,
            bool stopOnWall = true)
        {
            castMove.ApplyEditorData(
                moveType,
                distance,
                duration,
                anticipation,
                wallSkin,
                targetClearance,
                minSuccessDistance,
                stopOnWall);
        }
#endif
    }

    [Serializable]
    public sealed class SkillBodyActionPlaybackProfile
    {
        [SerializeField, Min(0f)] private float contactTime;
        [SerializeField, Min(0f)] private float duration;

        public float ContactTime => Mathf.Max(0f, contactTime);
        public float Duration(AnimationClip clip) => duration > 0f
            ? duration
            : clip != null ? Mathf.Max(.01f, clip.length) : 0f;

#if UNITY_EDITOR
        public void ApplyEditorData(float contact, float totalDuration)
        {
            contactTime = Mathf.Max(0f, contact);
            duration = Mathf.Max(0f, totalDuration);
        }
#endif
    }

    [Serializable]
    public sealed class MobilityBodyPresentationProfile
    {
        [SerializeField] private string profileId;
        [SerializeField, Range(1, 3)] private int grade = 1;
        [SerializeField] private bool reducedMotion;
        [SerializeField] private bool disablePresentation;

        public string ProfileId => profileId;
        public int Grade => Mathf.Clamp(grade, 1, 3);
        public bool ReducedMotion => reducedMotion;
        public bool DisablePresentation => disablePresentation;

#if UNITY_EDITOR
        public void ApplyEditorData(string id, int value)
        {
            profileId = id;
            grade = Mathf.Clamp(value, 1, 3);
        }
#endif
    }

    [Serializable]
    public class CastMoveProfile
    {
        [SerializeField] private CastMoveType moveType = CastMoveType.None;
        [SerializeField, Min(0f)] private float distance = 0f;
        [SerializeField, Min(0f)] private float duration = 0f;
        [SerializeField, Min(0f)] private float anticipation = 0f;
        [SerializeField, Min(0f)] private float wallSkin = 0.1f;
        [SerializeField, Min(0f)] private float targetClearance = 0.35f;
        [SerializeField, Min(0f)] private float minSuccessDistance = 0.4f;
        [SerializeField] private bool stopOnWall = true;

        public CastMoveType MoveType => moveType;
        public float Distance => Mathf.Max(0f, distance);
        public float Duration => Mathf.Max(0f, duration);
        public float Anticipation => Mathf.Max(0f, anticipation);
        public float WallSkin => Mathf.Max(0f, wallSkin);
        public float TargetClearance => Mathf.Max(0f, targetClearance);
        public float MinSuccessDistance => Mathf.Max(0f, minSuccessDistance);
        public bool StopOnWall => stopOnWall;

#if UNITY_EDITOR
        public void ApplyEditorData(
            CastMoveType moveType,
            float distance,
            float duration,
            float anticipation = 0f,
            float wallSkin = 0.1f,
            float targetClearance = 0.35f,
            float minSuccessDistance = 0.4f,
            bool stopOnWall = true)
        {
            this.moveType = moveType;
            this.distance = distance;
            this.duration = duration;
            this.anticipation = anticipation;
            this.wallSkin = wallSkin;
            this.targetClearance = targetClearance;
            this.minSuccessDistance = minSuccessDistance;
            this.stopOnWall = stopOnWall;
        }
#endif
    }

    [Serializable]
    public class BurstProfile
    {
        [SerializeField, Min(1)] private int count = 1;
        [SerializeField, Min(0f)] private float interval = 0f;

        public int Count => Mathf.Max(1, count);
        public float Interval => Mathf.Max(0f, interval);

#if UNITY_EDITOR
        public void ApplyEditorData(
            int count,
            float interval)
        {
            this.count = count;
            this.interval = interval;
        }
#endif
    }

    /// <summary>
    /// Presentation-only owner for Mobility VFX. It never targets the character
    /// SpriteRenderer and owns no Transform movement, collider, hit or gameplay state.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MobilityCastPresentationControllerMono : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer presentationRenderer;
        private Coroutine routine;

        public bool Play(AnimationClip clip)
        {
            if (clip == null || presentationRenderer == null) return false;
            StopPresentation();
            routine = StartCoroutine(PlayRoutine(clip));
            return true;
        }

        public void StopPresentation()
        {
            if (routine != null) StopCoroutine(routine);
            routine = null;
            if (presentationRenderer != null) presentationRenderer.sprite = null;
        }

        private System.Collections.IEnumerator PlayRoutine(AnimationClip clip)
        {
            float elapsed = 0f;
            while (elapsed < .42f)
            {
                clip.SampleAnimation(gameObject, elapsed);
                elapsed += Time.deltaTime;
                yield return null;
            }
            StopPresentation();
        }

        private void OnDisable() => StopPresentation();
    }

    [DisallowMultipleComponent]
    public sealed class CharacterMobilityBodyPresentationController : MonoBehaviour
    {
        private const float Lifecycle = .42f;
        private SpriteRenderer primary;
        private SpriteRenderer presentationRenderer;
        private SpriteRenderer proxy;
        private SpriteRenderer echo;
        private Coroutine routine;
        private Vector3 position;
        private Quaternion rotation;
        private Vector3 scale;
        private bool flipX;
        private bool flipY;
        private Sprite sprite;
        private Color color;
        private Material material;
        private MaterialPropertyBlock propertyBlock;
        private string sortingLayer;
        private int sortingOrder;
        private bool rendererEnabled;
        private float echoPeakAlpha;
        private bool usesRootProxy;
        private bool preparationFrameObserved;

        public bool PreparationFrameObserved => preparationFrameObserved;

        public bool Begin(MobilityBodyPresentationProfile profile)
        {
            Restore();
            if (profile == null || profile.DisablePresentation) return false;
            primary = ResolvePrimary();
            if (primary == null || primary.sprite == null) return false;
            Snapshot();
            PreparePresentationRenderer();
            ApplyPose(Evaluate(0f), profile.ReducedMotion ? .5f : 1f);
            preparationFrameObserved = false;
            routine = StartCoroutine(Present(profile));
            return true;
        }

        public void Restore()
        {
            if (routine != null) StopCoroutine(routine);
            routine = null;
            if (primary != null)
            {
                // The canonical renderer can live on the Rigidbody/actor root. Never
                // restore that Transform: doing so would undo the completed dash.
                if (!usesRootProxy && presentationRenderer == primary)
                {
                    primary.transform.localPosition = position;
                    primary.transform.localRotation = rotation;
                    primary.transform.localScale = scale;
                }
                primary.flipX = flipX;
                primary.flipY = flipY;
                primary.sprite = sprite;
                primary.color = color;
                primary.sharedMaterial = material;
                if (propertyBlock != null) primary.SetPropertyBlock(propertyBlock);
                primary.sortingLayerName = sortingLayer;
                primary.sortingOrder = sortingOrder;
                primary.enabled = rendererEnabled;
            }
            ClearEcho();
            ClearProxy();
            presentationRenderer = null;
            primary = null;
            usesRootProxy = false;
            preparationFrameObserved = false;
        }

        private void LateUpdate()
        {
            if (routine != null && presentationRenderer != null &&
                presentationRenderer.enabled)
            {
                preparationFrameObserved = true;
            }
        }

        private void Snapshot()
        {
            position = primary.transform.localPosition;
            rotation = primary.transform.localRotation;
            scale = primary.transform.localScale;
            flipX = primary.flipX;
            flipY = primary.flipY;
            sprite = primary.sprite;
            color = primary.color;
            material = primary.sharedMaterial;
            propertyBlock = new MaterialPropertyBlock();
            primary.GetPropertyBlock(propertyBlock);
            sortingLayer = primary.sortingLayerName;
            sortingOrder = primary.sortingOrder;
            rendererEnabled = primary.enabled;
        }

        private void PreparePresentationRenderer()
        {
            Rigidbody2D actorBody = GetComponent<Rigidbody2D>() ?? GetComponentInParent<Rigidbody2D>();
            Transform actorRoot = actorBody != null ? actorBody.transform : transform;
            usesRootProxy = primary.transform == actorRoot;
            if (!usesRootProxy)
            {
                // A dedicated child renderer owns its local pose and is safe to restore.
                presentationRenderer = primary;
                return;
            }

            if (proxy == null)
            {
                Transform existing = actorRoot.Find("SwiftStepBodyProxy_RenderOnly");
                if (existing != null) proxy = existing.GetComponent<SpriteRenderer>();
                if (proxy == null)
                {
                    GameObject child = new GameObject("SwiftStepBodyProxy_RenderOnly");
                    child.transform.SetParent(actorRoot, false);
                    proxy = child.AddComponent<SpriteRenderer>();
                }
            }

            proxy.transform.localPosition = Vector3.zero;
            proxy.transform.localRotation = Quaternion.identity;
            proxy.transform.localScale = Vector3.one;
            CopyCanonicalRenderer(proxy);
            proxy.enabled = rendererEnabled;
            presentationRenderer = proxy;
            position = Vector3.zero;
            rotation = Quaternion.identity;
            scale = Vector3.one;
            primary.enabled = false;
        }

        private void CopyCanonicalRenderer(SpriteRenderer target)
        {
            target.flipX = flipX;
            target.flipY = flipY;
            target.sprite = sprite;
            target.color = color;
            target.sharedMaterial = material;
            if (propertyBlock != null) target.SetPropertyBlock(propertyBlock);
            target.sortingLayerName = sortingLayer;
            target.sortingOrder = sortingOrder;
        }

        private System.Collections.IEnumerator Present(MobilityBodyPresentationProfile profile)
        {
            float elapsed = 0f;
            bool echoSpawned = false;
            while (elapsed < Lifecycle && primary != null && presentationRenderer != null)
            {
                float multiplier = profile.ReducedMotion ? .5f : 1f;
                Pose pose = Evaluate(elapsed);
                ApplyPose(pose, multiplier);
                if (elapsed >= .08f && elapsed <= .28f) presentationRenderer.sprite = sprite;
                if (!profile.ReducedMotion && !echoSpawned && elapsed >= .20f)
                {
                    CreateEcho(profile.Grade);
                    echoSpawned = true;
                }
                UpdateEcho(elapsed);
                elapsed += Time.deltaTime;
                yield return null;
            }
            Restore();
        }

        private void ApplyPose(Pose pose, float multiplier)
        {
            if (presentationRenderer == null) return;
            presentationRenderer.transform.localPosition = position +
                new Vector3(0f, pose.Y * multiplier, 0f);
            presentationRenderer.transform.localRotation = rotation *
                Quaternion.Euler(0f, 0f, pose.Rotation * multiplier);
            presentationRenderer.transform.localScale = Vector3.Scale(scale,
                new Vector3(1f + (pose.ScaleX - 1f) * multiplier,
                    1f + (pose.ScaleY - 1f) * multiplier, 1f));
        }

        private void CreateEcho(int grade)
        {
            if (echo == null)
            {
                GameObject child = new GameObject("SwiftStepBodyEcho_RenderOnly");
                child.transform.SetParent(presentationRenderer.transform.parent, false);
                echo = child.AddComponent<SpriteRenderer>();
            }
            echo.sprite = sprite;
            echo.flipX = flipX;
            echo.flipY = flipY;
            echo.sharedMaterial = material;
            echo.sortingLayerName = sortingLayer;
            echo.sortingOrder = Mathf.Min(sortingOrder, sortingOrder - 1);
            echo.transform.localPosition = position + new Vector3(flipX ? .18f : -.18f, -.045f, 0f);
            echo.transform.localRotation = rotation * Quaternion.Euler(0f, 0f, -10f);
            echo.transform.localScale = Vector3.Scale(scale, new Vector3(1.10f, .82f, 1f));
            float alpha = grade == 1 ? .12f : grade == 2 ? .18f : .25f;
            echoPeakAlpha = alpha;
            echo.color = new Color(.30f, .36f, .46f, alpha);
            echo.enabled = true;
        }

        private void UpdateEcho(float elapsed)
        {
            if (echo == null || !echo.enabled) return;
            float t = Mathf.InverseLerp(.20f, .36f, elapsed);
            Color value = echo.color;
            value.a = echoPeakAlpha * (1f - t);
            echo.color = value;
            if (elapsed >= .36f) ClearEcho();
        }

        private void ClearEcho()
        {
            if (echo == null) return;
            echo.sprite = null;
            echo.color = Color.clear;
            echo.enabled = false;
        }

        private void ClearProxy()
        {
            if (proxy == null) return;
            proxy.sprite = null;
            proxy.color = Color.clear;
            proxy.SetPropertyBlock(null);
            proxy.enabled = false;
        }

        private SpriteRenderer ResolvePrimary()
        {
            global::Character.AnimationMono animation = GetComponent<global::Character.AnimationMono>()
                ?? GetComponentInChildren<global::Character.AnimationMono>();
            SpriteRenderer[] renderers = animation != null
                ? animation.GetComponentsInChildren<SpriteRenderer>(true)
                : GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && renderers[i] != echo && renderers[i] != proxy
                    && renderers[i].sprite != null)
                    return renderers[i];
            }
            return null;
        }

        private static Pose Evaluate(float time)
        {
            float[] times = { 0f, .08f, .13f, .20f, .28f, .36f, .42f };
            Pose[] poses = {
                new Pose(1f,.96f,-2f,-.01f), new Pose(1.03f,.91f,-6f,-.025f),
                new Pose(1.07f,.86f,-8f,-.035f), new Pose(1.10f,.82f,-10f,-.045f),
                new Pose(1.05f,.90f,-5f,-.025f), new Pose(1.01f,.98f,-1f,-.005f),
                new Pose(1f,1f,0f,0f) };
            for (int i = 0; i < times.Length - 1; i++)
            {
                if (time > times[i + 1]) continue;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(times[i], times[i + 1], time));
                return Pose.Lerp(poses[i], poses[i + 1], t);
            }
            return poses[poses.Length - 1];
        }

        private readonly struct Pose
        {
            public readonly float ScaleX, ScaleY, Rotation, Y;
            public Pose(float x, float y, float rotation, float localY)
            { ScaleX = x; ScaleY = y; Rotation = rotation; Y = localY; }
            public static Pose Lerp(Pose a, Pose b, float t) => new Pose(
                Mathf.Lerp(a.ScaleX,b.ScaleX,t), Mathf.Lerp(a.ScaleY,b.ScaleY,t),
                Mathf.Lerp(a.Rotation,b.Rotation,t), Mathf.Lerp(a.Y,b.Y,t));
        }

        private void OnDisable() => Restore();
        private void OnDestroy() => Restore();
    }
}
