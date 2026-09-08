using String;
using UnityEngine;

namespace Effect
{
    public static class EffectDisplayNameResolver
    {
        public static string Resolve(EffectSO effect)
        {
            if (effect == null) return "알 수 없는 효과";

            string localized = StringManager.Instance != null
                ? StringManager.Instance.Get(effect.EffectId, "name", true)
                : null;
            if (!string.IsNullOrWhiteSpace(localized)
                && !string.Equals(localized, effect.EffectId, System.StringComparison.Ordinal)
                && !string.Equals(localized, $"{effect.EffectId}.name", System.StringComparison.Ordinal))
                return localized;

            if (!string.IsNullOrWhiteSpace(effect.AuthoredDisplayName))
                return effect.AuthoredDisplayName;

            string semantic = ResolveSemantic(effect.Config);
            if (!string.IsNullOrWhiteSpace(semantic)) return semantic;

            Debug.LogWarning($"[EffectDisplayName] Missing localization/authored/semantic name: {effect.EffectId}", effect);
            return string.IsNullOrWhiteSpace(effect.EffectId) ? "알 수 없는 효과" : effect.EffectId;
        }

        private static string ResolveSemantic(EffectConfig config)
        {
            if (config is StatModifierEffectConfig stat)
                return $"{ResolveStat(stat.TargetStat)} 효과";
            if (config is KnockbackEffectConfig) return "밀쳐내기 효과";
            if (config is HealEffectConfig) return "회복 효과";
            if (config is CooldownReduceEffectConfig) return "재사용 대기시간 감소 효과";
            if (config is TauntEffectConfig) return "도발 효과";
            return null;
        }

        private static string ResolveStat(Stat.StatType stat)
        {
            switch (stat)
            {
                case Stat.StatType.Attack:
                case Stat.StatType.AttackPercent:
                case Stat.StatType.SurroundedAttackPercent: return "공격력";
                case Stat.StatType.Defense:
                case Stat.StatType.LowHpDefenseBonus: return "방어력";
                case Stat.StatType.MoveSpeed:
                case Stat.StatType.MoveSpeedPercent: return "이동속도";
                case Stat.StatType.AttackSpeed:
                case Stat.StatType.AttackSpeedPercent: return "공격속도";
                case Stat.StatType.SurroundedDamageReductionPercent: return "피해 감소";
                case Stat.StatType.MissingHpFinalDamageAmplify:
                case Stat.StatType.FinalDamageAmplify: return "최종 피해";
                case Stat.StatType.Hp: return "체력";
                default: return stat == Stat.StatType.None ? null : stat.ToString();
            }
        }
    }

    [CreateAssetMenu(
        fileName = "EffectSO",
        menuName = "Effect/Effect SO")]
    public class EffectSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string effectId;
        [SerializeField] private string authoredDisplayName;

        [Header("Visual")]
        [SerializeField] private Sprite icon;
        
        [Header("Config")]
        [SerializeReference] private EffectConfig config;

        public EffectConfig Config => config;

        public string LocalizationMainKey => effectId;

        public string AuthoredDisplayName => authoredDisplayName;

        public string DisplayName => EffectDisplayNameResolver.Resolve(this);

        public string Description =>
            StringManager.Instance.Get(
                LocalizationMainKey,
                "desc");

        public string EffectId => effectId;
        public Sprite Icon => icon;

#if UNITY_EDITOR
        public void ApplyEditorData(
            string effectId,
            Sprite icon,
            EffectConfig config,
            string authoredDisplayName = null)
        {
            this.effectId = effectId;
            this.authoredDisplayName = authoredDisplayName ?? string.Empty;
            this.icon = icon;
            this.config = config;
        }
#endif
    }
}
