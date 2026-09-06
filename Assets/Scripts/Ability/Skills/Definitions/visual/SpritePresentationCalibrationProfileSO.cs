using System;
using UnityEngine;

namespace Skill
{
    [CreateAssetMenu(fileName = "SpritePresentationCalibrationProfileSO", menuName = "Game/Skills/Visual/Sprite Presentation Calibration")]
    public sealed class SpritePresentationCalibrationProfileSO : ScriptableObject
    {
        [SerializeField] private string profileId;
        [SerializeField] private SpritePresentationCalibrationEntry[] entries = Array.Empty<SpritePresentationCalibrationEntry>();

        public string ProfileId => profileId;
        public SpritePresentationCalibrationEntry[] Entries => entries;

        public bool TryResolve(Sprite sprite, out Vector2 scale, out Vector2 offset)
        {
            scale = Vector2.one;
            offset = Vector2.zero;
            if (sprite == null || entries == null) return false;
            for (int i = 0; i < entries.Length; i++)
            {
                SpritePresentationCalibrationEntry entry = entries[i];
                if (entry != null && entry.FrameIndex == ResolveFrameIndex(sprite.name) && entry.IsValid)
                {
                    scale = entry.Scale;
                    offset = entry.Offset;
                    return true;
                }
            }
            return false;
        }

        private static int ResolveFrameIndex(string spriteName)
        {
            if (string.IsNullOrWhiteSpace(spriteName)) return -1;
            int separator = spriteName.LastIndexOf('-');
            return separator >= 0 && int.TryParse(spriteName.Substring(separator + 1), out int index) ? index : -1;
        }
    }

    [Serializable]
    public sealed class SpritePresentationCalibrationEntry
    {
        [SerializeField, Range(0, 5)] private int frameIndex;
        [SerializeField] private Vector2 scale = Vector2.one;
        [SerializeField] private Vector2 offset;

        public int FrameIndex => frameIndex;
        public Vector2 Scale => scale;
        public Vector2 Offset => offset;
        public bool IsValid => frameIndex >= 0 && frameIndex <= 5 && IsFinite(scale.x) && IsFinite(scale.y) &&
            scale.x >= (1f / 6.5f) && scale.x <= 6.5f &&
            scale.y >= (1f / 6.5f) && scale.y <= 6.5f &&
            IsFinite(offset.x) && IsFinite(offset.y);

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
