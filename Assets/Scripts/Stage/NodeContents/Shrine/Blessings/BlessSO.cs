using System.Collections.Generic;
using UnityEngine;

using Shrine;
using Effect;
using String;

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
        [SerializeField] private string blessingId;
        [SerializeField] private string groupId;

        [Header("Display")]
        [SerializeField] private Sprite icon;

        [Header("Blessing")]
        [SerializeField] private BlessCategory category = BlessCategory.None;

        [Header("Effects")]
        [SerializeField] private List<EffectEntrySO> effectEntries = new();

        [Tooltip("특정 신 전용 축복 여부")]
        [SerializeField] private ShrineGodType godType = ShrineGodType.None;


        [Header("Duration")]
        [SerializeField] private BlessDurationType durationType = BlessDurationType.Permanent;

        [Tooltip("전투 기준 지속 횟수")]
        [SerializeField] private int durationBattleCount = -1;



        [Header("Tags")]
        [Tooltip("생성 필터링용 태그")]
        [SerializeField] private List<string> tags = new();

        public string LocalizationMainKey => blessingId;

        public string DisplayName =>
            StringManager.Instance.Get(
                LocalizationMainKey,
                "name");

        public string Description =>
            StringManager.Instance.Get(
                LocalizationMainKey,
                "desc");

        public string BlessingId => blessingId;
        public string GroupId => groupId;
        public Sprite Icon => icon;
        public BlessCategory Category => category;
        public IReadOnlyList<EffectEntrySO> EffectEntries => effectEntries;
        public ShrineGodType GodType => godType;
        public BlessDurationType DurationType => durationType;
        public int DurationBattleCount => durationBattleCount;
        public IReadOnlyList<string> Tags => tags;

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
            if (effectEntries == null || effectEntries.Count == 0)
            {
                return Description;
            }

            List<string> lines = new();

            foreach (var effectEntry in effectEntries)
            {
                if (effectEntry?.EffectSO == null)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(effectEntry.EffectSO.Description))
                {
                    lines.Add(effectEntry.EffectSO.Description);
                }
            }

            return string.Join("\n", lines);
        }
    }
}