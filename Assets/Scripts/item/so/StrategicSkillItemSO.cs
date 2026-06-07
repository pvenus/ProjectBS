using UnityEngine;
using String;
using Skill;
namespace Item
{
    [CreateAssetMenu(
        fileName = "StrategicSkillItemSO",
        menuName = "Game/Item/Strategic Skill Item")]
    public class StrategicSkillItemSO : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("전략 스킬 아이템 고유 ID")]
        public string strategicSkillItemId;

        public string LocalizationMainKey => strategicSkillItemId;
        public string DisplayNameSubKey => "name";
        public string DescriptionSubKey => "desc";
        public string DisplayNameLocalizationKey =>
            $"{LocalizationMainKey}.{DisplayNameSubKey}";
        public string DescriptionLocalizationKey =>
            $"{LocalizationMainKey}.{DescriptionSubKey}";

        public string DisplayName =>
            StringManager.Instance.Get(
                LocalizationMainKey,
                DisplayNameSubKey);

        public string Description =>
            StringManager.Instance.Get(
                LocalizationMainKey,
                DescriptionSubKey);

        [Tooltip("UI에 표시할 아이콘")]
        public Sprite icon;

        [Header("Cost")]
        [Tooltip("전략 스킬 게이지 사용량")]
        [Min(0)]
        public int gaugeCost = 10;

        [Tooltip("전투 중 게이지가 충분하면 반복 사용 가능한지 여부")]
        public bool reusable = true;

        [Header("Skill")]
        [Tooltip("전략 스킬 아이템이 실행할 실제 스킬 SO")]
        public EquipmentSkillSO skillSo;

        [Tooltip("전략 스킬에서 사용할 투사체/스킬 런타임 프리팹 오버라이드. 비어 있으면 SkillSO 기본값을 사용")]
        public ProjectileEntity projectilePrefabOverride;

        [Tooltip("전략 스킬 투사체 수명 오버라이드. 0 미만이면 SkillSO 기본값을 사용")]
        public float projectileLifetimeOverride = -1f;

        [Header("Cast")]
        [Tooltip("즉시 발동, 대상 지정, 위치 지정 등 시전 방식")]
        public StrategicSkillCastType castType = StrategicSkillCastType.Position;

        [Tooltip("아군, 적, 위치, 전체 전장 등 대상 방식")]
        public StrategicSkillTargetType targetType = StrategicSkillTargetType.Position;

        [Header("Shop")]
        [Tooltip("상점 또는 보상 풀에서 사용할 기본 가격")]
        [Min(0)]
        public int defaultPrice = 100;


        [Header("Tags")]
        [Tooltip("분류/검색/풀 구성에 사용할 태그")]
        public string[] tags;
    }

    public enum StrategicSkillCastType
    {
        Instant = 0,
        Target = 1,
        Position = 2
    }

    public enum StrategicSkillTargetType
    {
        None = 0,
        Self = 1,
        Ally = 2,
        Enemy = 3,
        Position = 4,
        AllAllies = 5,
        AllEnemies = 6,
        Battlefield = 7
    }
}
