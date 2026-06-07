using UnityEngine;
using String;

/// <summary>
/// 장비 = 스킬의 원형 데이터를 정의하는 최상위 허브 SO.
/// 이 객체는 절대 변하지 않는 설계도 역할만 담당하며,
/// 기본 Effect / 룬 / 업그레이드 / 최종 비주얼 / 최종 스탯은 별도 Resolver에서 해석한다.
/// </summary>
namespace Skill
{
    [CreateAssetMenu(fileName = "EquipmentSkillSO", menuName = "BS/Skills/Equipment/EquipmentSkillSO")]
    public class EquipmentSkillSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string equipmentId;
        [SerializeField] private Sprite icon;

        [Header("Base Profile")]
        [SerializeField] private EquipmentBaseProfileSO baseProfileSo;

        [Header("Core Profiles")]
        [SerializeField] private SkillCastSO castSo;
        [SerializeField] private SkillHitSO[] hitSos;
        [SerializeField] private SkillMoveSO moveSo;
        [SerializeField] private SpawnSkillSO spawnSkillSo;

        [Header("Visual")]
        [SerializeField] private SkillVisualSetSO visualSetSo;

        public string EquipmentId => equipmentId;
        public string LocalizationMainKey => equipmentId;

        public string DisplayName =>
            StringManager.Instance.Get(
                LocalizationMainKey,
                "name");

        public string Description =>
            StringManager.Instance.Get(
                LocalizationMainKey,
                "desc");

        public EquipmentBaseProfileSO BaseProfileSo => baseProfileSo;
        public SkillCastSO CastSo => castSo;
        public SkillHitSO[] HitSos => hitSos;
        public SkillMoveSO MoveSo => moveSo;
        public SpawnSkillSO SpawnSkillSo => spawnSkillSo;
        public SkillVisualSetSO VisualSetSo => visualSetSo;

        public float EvaluateBrainScore(object context, int roleBias = 0)
        {
            return baseProfileSo.BasePriority + roleBias;
        }
    }
}