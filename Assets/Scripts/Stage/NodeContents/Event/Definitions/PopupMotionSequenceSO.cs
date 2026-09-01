using UnityEngine;

namespace Stage
{
    [CreateAssetMenu(menuName = "Stage/Popup Motion Sequence")]
    public sealed class PopupMotionSequenceSO : ScriptableObject
    {
        [SerializeField] private string[] frameResourcePaths = System.Array.Empty<string>();
        [SerializeField] private int[] slotSequence = System.Array.Empty<int>();
        [SerializeField, Min(0.01f)] private float slotDuration = 0.5f;
        [SerializeField, Min(0)] private int firstOpenLoopLimit = 3;
        [SerializeField, Min(0)] private int reentryLoopLimit = 1;

        public string[] FrameResourcePaths => frameResourcePaths;
        public int[] SlotSequence => slotSequence;
        public float SlotDuration => slotDuration > 0f ? slotDuration : 0.5f;
        public int FirstOpenLoopLimit => Mathf.Max(0, firstOpenLoopLimit);
        public int ReentryLoopLimit => Mathf.Max(0, reentryLoopLimit);

        public bool IsValid()
        {
            if (frameResourcePaths == null || frameResourcePaths.Length == 0
                || slotSequence == null || slotSequence.Length == 0)
                return false;

            for (int i = 0; i < frameResourcePaths.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(frameResourcePaths[i])) return false;
            }
            for (int i = 0; i < slotSequence.Length; i++)
            {
                if (slotSequence[i] < 0 || slotSequence[i] >= frameResourcePaths.Length)
                    return false;
            }
            return true;
        }
    }
}
