using UnityEngine;
using String;
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
        [Tooltip("아이템 사용 시 Resources에서 조회해 실행할 EquipmentSkillSO ID")]
        public string skillId;

        [Header("Shop")]
        [Tooltip("상점 또는 보상 풀에서 사용할 기본 가격")]
        [Min(0)]
        public int defaultPrice = 100;

        [Header("Tags")]
        [Tooltip("분류/검색/풀 구성에 사용할 태그")]
        public string[] tags;
    }
}
