using System.Collections.Generic;
using UnityEngine;

using Shrine;
using Effect;

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

        [Header("Effects")]
        public List<EffectSO> effects = new();

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
            if (effects == null || effects.Count == 0)
            {
                return description;
            }

            List<string> lines = new();

            foreach (var effect in effects)
            {
                if (effect == null)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(effect.description))
                {
                    lines.Add(effect.description);
                }
            }

            return string.Join("\n", lines);
        }
    }
}