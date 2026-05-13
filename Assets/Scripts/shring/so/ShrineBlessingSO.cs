

using System.Collections.Generic;
using UnityEngine;

namespace Shrine
{
    /// <summary>
    /// 신전 축복 정의 데이터.
    /// 현재는 공용 수치 기반 구조로 구성하고,
    /// 이후 실제 스탯/효과 시스템과 연결할 수 있도록 확장 가능한 형태로 유지한다.
    /// </summary>
    [CreateAssetMenu(menuName = "Shrine/Shrine Blessing")]
    public class ShrineBlessingSO : ScriptableObject
    {
        [Header("Identity")]
        public string blessingId;
        public string blessingName;

        [TextArea]
        public string description;

        [Header("Display")]
        public Sprite icon;

        [Header("Blessing")]
        public ShrineBlessingCategory category = ShrineBlessingCategory.None;
        public ShrineBlessingEffectType effectType = ShrineBlessingEffectType.None;

        [Tooltip("효과 수치")]
        public float effectValue = 0f;

        [Tooltip("추가 수치가 필요한 경우 사용")]
        public float secondaryValue = 0f;

        [Tooltip("특정 신 전용 축복 여부")]
        public ShrineGodType godType = ShrineGodType.None;

        [Header("Faith")]
        [Tooltip("등장 최소 신앙 단계")]
        [Range(0, 10)]
        public int requiredFaithLevel = 0;

        [Tooltip("혼합 신앙 상태에서 등장 가능 여부")]
        public bool allowMixedFaith = true;

        [Header("Generation")]
        [Tooltip("랜덤 생성 가중치")]
        [Min(0)]
        public int weight = 100;

        [Tooltip("중복 선택 허용 여부")]
        public bool allowDuplicate = false;

        [Tooltip("특수 이벤트 전용 여부")]
        public bool eventOnly = false;

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

        public bool CanAppear(int faithLevel, bool mixedFaith)
        {
            if (faithLevel < requiredFaithLevel)
            {
                return false;
            }

            if (!allowMixedFaith && mixedFaith)
            {
                return false;
            }

            return true;
        }

        public string GetEffectDescription()
        {
            switch (effectType)
            {
                case ShrineBlessingEffectType.AttackPowerPercent:
                    return $"Attack Power +{effectValue}%";

                case ShrineBlessingEffectType.AttackSpeedPercent:
                    return $"Attack Speed +{effectValue}%";

                case ShrineBlessingEffectType.CriticalChancePercent:
                    return $"Critical Chance +{effectValue}%";

                case ShrineBlessingEffectType.BossDamagePercent:
                    return $"Boss Damage +{effectValue}%";

                case ShrineBlessingEffectType.DamageReductionPercent:
                    return $"Damage Reduction +{effectValue}%";

                case ShrineBlessingEffectType.MaxHpPercent:
                    return $"Max HP +{effectValue}%";

                case ShrineBlessingEffectType.StatusResistancePercent:
                    return $"Status Resistance +{effectValue}%";

                case ShrineBlessingEffectType.LowHpDefensePercent:
                    return $"Low HP Defense +{effectValue}%";

                case ShrineBlessingEffectType.ExpGainPercent:
                    return $"EXP Gain +{effectValue}%";

                case ShrineBlessingEffectType.GoldGainPercent:
                    return $"Gold Gain +{effectValue}%";

                case ShrineBlessingEffectType.RelicDropPercent:
                    return $"Relic Drop Chance +{effectValue}%";

                case ShrineBlessingEffectType.AiReactionSpeedPercent:
                    return $"AI Reaction Speed +{effectValue}%";

                case ShrineBlessingEffectType.CooldownReductionPercent:
                    return $"Cooldown Reduction +{effectValue}%";

                case ShrineBlessingEffectType.ConsumableEffectPercent:
                    return $"Consumable Effect +{effectValue}%";

                case ShrineBlessingEffectType.StartBattleShield:
                    return $"Gain Shield at Battle Start";

                case ShrineBlessingEffectType.Special:
                    return description;
            }

            return description;
        }
    }
}