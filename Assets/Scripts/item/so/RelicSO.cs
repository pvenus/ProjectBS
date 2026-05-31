using UnityEngine;
using System.Collections.Generic;
using Effect;
using String;

namespace Item
{
    [CreateAssetMenu(
        fileName = "RelicSO",
        menuName = "Item/Relic SO")]
    public class RelicSO : ScriptableObject
    {
        [Header("Identity")]
        public string relicId;

        public string LocalizationMainKey => relicId;

        public string DisplayName =>
            StringManager.Instance.Get(
                LocalizationMainKey,
                "name");

        public string Description =>
            StringManager.Instance.Get(
                LocalizationMainKey,
                "desc");

        [Header("UI")]
        public Sprite icon;

        public Color themeColor = Color.white;

        [Header("Gameplay")]
        public RelicRarity rarity = RelicRarity.Common;

        public List<RelicEffectEntry> effects = new();

        [Header("Tags")]
        public string category;

        public string subCategory;

        [Header("Flags")]
        public bool hidden;

        public bool developerOnly;
    }

    [System.Serializable]
    public class RelicEffectEntry
    {
        public EffectSO effect;

        public RelicEffectApplyType applyType =
            RelicEffectApplyType.OnEquip;

        public EffectLifetimeType lifetimeType =
            EffectLifetimeType.Instant;

        [Min(0f)]
        public float duration;

        public EffectCategoryType categoryType =
            EffectCategoryType.Neutral;
    }

    public enum RelicEffectApplyType
    {
        OnEquip = 0,
        OnAttack = 100,
    }

    public enum RelicRarity
    {
        Common = 0,
        Rare = 100,
        Epic = 200,
        Legendary = 300,
    }
}
