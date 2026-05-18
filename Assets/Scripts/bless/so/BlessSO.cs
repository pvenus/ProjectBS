using System.Collections.Generic;
using UnityEngine;

using Shrine;

namespace Bless
{
    /// <summary>
    /// 신전 축복 정의 데이터.
    /// 현재는 공용 수치 기반 구조로 구성하고,
    /// 이후 실제 스탯/효과 시스템과 연결할 수 있도록 확장 가능한 형태로 유지한다.
    /// </summary>
    [CreateAssetMenu(menuName = "BS/Bless/Bless")]
    public class BlessSO : ScriptableObject
    {
        [Header("Identity")]
        public string blessingId;
        public string groupId;
        public string blessingName;

        [TextArea]
        public string description;

        [Header("Display")]
        public Sprite icon;

        [Header("Blessing")]
        public BlessCategory category = BlessCategory.None;
        public BlessEffectType effectType = BlessEffectType.None;

        [Tooltip("효과 수치")]
        public float effectValue = 0f;

        [Tooltip("추가 수치가 필요한 경우 사용")]
        public float secondaryValue = 0f;


        [Tooltip("특정 신 전용 축복 여부")]
        public ShrineGodType godType = ShrineGodType.None;


        [Header("Duration")]
        public BlessDurationType durationType = BlessDurationType.Permanent;

        [Tooltip("전투 기준 지속 횟수")]
        public int durationBattleCount = -1;



        [Header("Tags")]
        [Tooltip("생성 필터링용 태그")]
        public List<string> tags = new();

        public string DisplayName => string.IsNullOrWhiteSpace(blessingName)
            ? name
            : blessingName;

        public bool HasTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                return false;
            }

            return tags != null && tags.Contains(tag);
        }

        public bool CanAppear(
            ShrineGodType currentGodType,
            int faithLevel)
        {
            if (godType == ShrineGodType.None)
            {
                return true;
            }

            return godType == currentGodType;
        }

        public string GetEffectDescription()
        {
            switch (effectType)
            {
                case BlessEffectType.AttackPowerPercent:
                    return $"Attack Power +{effectValue}%";

                case BlessEffectType.AttackSpeedPercent:
                    return $"Attack Speed +{effectValue}%";

                case BlessEffectType.CriticalChancePercent:
                    return $"Critical Chance +{effectValue}%";

                case BlessEffectType.BossDamagePercent:
                    return $"Boss Damage +{effectValue}%";

                case BlessEffectType.DamageReductionPercent:
                    return $"Damage Reduction +{effectValue}%";

                case BlessEffectType.MaxHpPercent:
                    return $"Max HP +{effectValue}%";

                case BlessEffectType.StatusResistancePercent:
                    return $"Status Resistance +{effectValue}%";

                case BlessEffectType.LowHpDefensePercent:
                    return $"Low HP Defense +{effectValue}%";

                case BlessEffectType.ExpGainPercent:
                    return $"EXP Gain +{effectValue}%";

                case BlessEffectType.GoldGainPercent:
                    return $"Gold Gain +{effectValue}%";

                case BlessEffectType.RelicDropPercent:
                    return $"Relic Drop Chance +{effectValue}%";

                case BlessEffectType.AiReactionSpeedPercent:
                    return $"AI Reaction Speed +{effectValue}%";

                case BlessEffectType.CooldownReductionPercent:
                    return $"Cooldown Reduction +{effectValue}%";

                case BlessEffectType.ConsumableEffectPercent:
                    return $"Consumable Effect +{effectValue}%";

                case BlessEffectType.StartBattleShield:
                    return $"Gain Shield at Battle Start";

                case BlessEffectType.Special:
                    return description;
            }

            return description;
        }
    }
}