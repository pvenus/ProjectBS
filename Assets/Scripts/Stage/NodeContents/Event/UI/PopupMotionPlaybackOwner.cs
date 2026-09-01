using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Stage
{
    public sealed class PopupMotionPlaybackOwner : MonoBehaviour
    {
        private static PopupMotionPlaybackOwner active;
        private static readonly HashSet<string> opened = new(System.StringComparer.Ordinal);

        public static bool ReducedMotion { get; set; }
        public static bool MotionEnabled { get; set; } = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSession()
        {
            active = null;
            opened.Clear();
            ReducedMotion = false;
            MotionEnabled = true;
        }

        private Image target;
        private Sprite fallback;
        private PopupMotionSequenceSO sequence;
        private Sprite[] frames;
        private string sequencePath;
        private int slotIndex;
        private int completedLoops;
        private int loopLimit;
        private float elapsed;

        public static void Bind(Image image, Sprite staticFallback, string resourcePath)
        {
            if (image == null) return;
            PopupMotionPlaybackOwner owner = image.GetComponent<PopupMotionPlaybackOwner>();
            if (owner == null) owner = image.gameObject.AddComponent<PopupMotionPlaybackOwner>();
            if (active != null && active != owner) active.ReleaseToFallback();
            active = owner;
            owner.Begin(image, staticFallback, resourcePath);
        }

        public static void Release(Image image)
        {
            if (active != null && active.target == image) active.ReleaseToFallback();
        }

        private void Begin(Image image, Sprite staticFallback, string resourcePath)
        {
            ReleaseLoaded();
            target = image;
            fallback = staticFallback;
            sequencePath = resourcePath ?? string.Empty;
            target.sprite = fallback;

            if (!MotionEnabled || ReducedMotion || string.IsNullOrWhiteSpace(sequencePath)) return;
            sequence = Resources.Load<PopupMotionSequenceSO>(sequencePath);
            if (sequence == null || !sequence.IsValid())
            {
                ReleaseLoaded();
                return;
            }

            string[] paths = sequence.FrameResourcePaths;
            frames = new Sprite[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                frames[i] = Resources.Load<Sprite>(paths[i]);
                if (frames[i] == null)
                {
                    ReleaseLoaded();
                    target.sprite = fallback;
                    return;
                }
            }

            bool reentry = !opened.Add(sequencePath);
            loopLimit = reentry ? sequence.ReentryLoopLimit : sequence.FirstOpenLoopLimit;
            slotIndex = 0;
            completedLoops = 0;
            elapsed = 0f;
            ApplySlot(0);
        }

        private void Update()
        {
            if (sequence == null || frames == null || target == null) return;
            if (!MotionEnabled || ReducedMotion)
            {
                ReleaseToFallback();
                return;
            }
            if (Time.timeScale <= 0f || !Application.isFocused) return;
            if (loopLimit <= 0) { ReleaseToFallback(); return; }

            elapsed += Time.unscaledDeltaTime;
            while (elapsed >= sequence.SlotDuration)
            {
                elapsed -= sequence.SlotDuration;
                slotIndex++;
                if (slotIndex >= sequence.SlotSequence.Length)
                {
                    slotIndex = 0;
                    completedLoops++;
                    if (completedLoops >= loopLimit)
                    {
                        ReleaseToFallback();
                        return;
                    }
                }
                ApplySlot(slotIndex);
            }
        }

        private void ApplySlot(int index)
        {
            int frameIndex = sequence.SlotSequence[index];
            target.sprite = frames[frameIndex] != null ? frames[frameIndex] : fallback;
        }

        private void OnDisable() => ReleaseToFallback();
        private void OnDestroy() => ReleaseToFallback();

        private void ReleaseToFallback()
        {
            if (target != null) target.sprite = fallback;
            ReleaseLoaded();
            if (active == this) active = null;
        }

        private void ReleaseLoaded()
        {
            if (frames != null)
            {
                for (int i = 0; i < frames.Length; i++)
                    if (frames[i] != null) Resources.UnloadAsset(frames[i]);
            }
            if (sequence != null) Resources.UnloadAsset(sequence);
            frames = null;
            sequence = null;
            sequencePath = string.Empty;
            slotIndex = 0;
            completedLoops = 0;
            elapsed = 0f;
        }
    }
}
