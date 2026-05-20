using System.Collections.Generic;
using UnityEngine;

namespace Effect
{
    [CreateAssetMenu(
        fileName = "EffectSO",
        menuName = "Effect/Effect SO")]
    public class EffectSO : ScriptableObject
    {
        [Header("Identity")]
        public string effectId;

        public string effectName;

        [TextArea]
        public string description;

        [Header("Visual")]
        public Sprite icon;

        [Header("Runtime")]
        public bool isPermanent;

        public bool allowDuplicate = true;

        public float duration;

        public int maxStack = 1;

        [Header("Tags")]
        public List<string> tags = new();
    }
}
