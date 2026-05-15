using UnityEngine;

namespace Item
{
    [CreateAssetMenu(
        fileName = "RelicSO",
        menuName = "Item/Relic SO")]
    public class RelicSO : ScriptableObject
    {
        [Header("Identity")]
        public string relicId;

        public string displayName;

        [TextArea]
        public string description;

        [Header("UI")]
        public Sprite icon;

        public Color themeColor = Color.white;

        [Header("Tags")]
        public string category;

        public string subCategory;

        [Header("Flags")]
        public bool hidden;

        public bool developerOnly;
    }
}
