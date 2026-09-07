using UnityEngine;
using String;

/// <summary>
/// 장비 = 스킬의 원형 데이터를 정의하는 최상위 허브 SO.
/// 이 객체는 절대 변하지 않는 설계도 역할만 담당하며,
/// 기본 Effect / 룬 / 업그레이드 / 최종 비주얼 / 최종 스탯은 별도 Resolver에서 해석한다.
/// </summary>
namespace Skill
{
    [CreateAssetMenu(fileName = "EquipmentSkillSO", menuName = "Game/Skills/Equipment/EquipmentSkillSO")]
    public class EquipmentSkillSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string equipmentId;
        [SerializeField] private SkillAimMode aimMode;
        [SerializeField] private AimInputSource aimInputSource;
        public AimInputSource AimInputSource => SkillAimPolicy.ResolveInputSource(aimInputSource);
        public void ConfigureAimInputSource(string value) => aimInputSource=SkillAimPolicy.ParseInputSource(value);
        public SkillAimMode AimMode => SkillAimPolicy.Resolve(aimMode,castSo);
        public void ConfigureAimMode(string value) => aimMode=SkillAimPolicy.Parse(value);
        [SerializeField] private Sprite icon;

        [Header("Base Profile")]
        [SerializeField] private EquipmentBaseProfileSO baseProfileSo;

        [Header("Core Profiles")]
        [SerializeField] private SkillCastSO castSo;
        [SerializeField] private SkillHitSO[] hitSos;
        [SerializeField] private SkillMoveSO moveSo;
        [SerializeField] private SpawnSkillSO spawnSkillSo;

        [Header("Upgrade")]
        [SerializeField] private EquipmentUpgradeTableSO upgradeTableSo;

        [Header("Visual")]
        [SerializeField] private BaseVisualSO baseVisualSo;

        [Header("Optional Combo")]
        [SerializeField] private SkillComboProfile comboProfile = new();

        public string EquipmentId => equipmentId;
        public string LocalizationMainKey => equipmentId;
        public Sprite Icon => icon;

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
        public EquipmentUpgradeTableSO UpgradeTableSo => upgradeTableSo;
        public BaseVisualSO BaseVisualSo => baseVisualSo;
        public SkillComboProfile ComboProfile => comboProfile;

        public float EvaluateBrainScore(object context, int roleBias = 0)
        {
            return baseProfileSo.BasePriority + roleBias;
        }
    }

    [System.Serializable]
    public sealed class SkillComboProfile
    {
        [SerializeField] private bool enabled;
        [SerializeField, Min(0f)] private float duration;
        [SerializeField, Min(0f)] private float totalLungeCap;
        [SerializeField] private bool useCanonicalBodyChoreography;
        [SerializeField] private AnimationClip segmentedBodyActionClip;
        [SerializeField, Min(1)] private int segmentedBodyFrameCount = 18;
        [SerializeField] private SkillComboStep[] steps = System.Array.Empty<SkillComboStep>();

        public bool Enabled => enabled;
        public float Duration => Mathf.Max(0f, duration);
        public float TotalLungeCap => Mathf.Max(0f, totalLungeCap);
        public bool UseCanonicalBodyChoreography => useCanonicalBodyChoreography;
        public AnimationClip SegmentedBodyActionClip => segmentedBodyActionClip;
        public int SegmentedBodyFrameCount => Mathf.Max(1, segmentedBodyFrameCount);
        public SkillComboStep[] Steps => steps;

        public bool IsComplete => enabled && steps != null && steps.Length == 3 &&
            steps[0] != null && steps[1] != null && steps[2] != null &&
            steps[0].IsComplete(0) && steps[1].IsComplete(1) && steps[2].IsComplete(2) &&
            Mathf.Abs(steps[0].DamageWeight + steps[1].DamageWeight + steps[2].DamageWeight - 1f) <= 0.0001f &&
            steps[0].LungeDistance + steps[1].LungeDistance + steps[2].LungeDistance <= TotalLungeCap + 0.0001f &&
            Mathf.Abs(steps[2].RecoveryEnd - Duration) <= 0.0001f;

        public bool HasCompleteDistinctVisualRegistry => steps != null && steps.Length == 3 &&
            steps[0] != null && steps[1] != null && steps[2] != null &&
            (HasCompleteSegmentedBodyRegistry ||
             (steps[0].BodyActionClip != null && steps[1].BodyActionClip != null && steps[2].BodyActionClip != null &&
              steps[0].BodyActionClip != steps[1].BodyActionClip &&
              steps[0].BodyActionClip != steps[2].BodyActionClip &&
              steps[1].BodyActionClip != steps[2].BodyActionClip)) &&
            steps[0].VfxClip != null && steps[1].VfxClip != null && steps[2].VfxClip != null &&
            steps[0].VfxClip != steps[1].VfxClip &&
            steps[0].VfxClip != steps[2].VfxClip &&
            steps[1].VfxClip != steps[2].VfxClip;

        public bool HasCompleteSegmentedBodyRegistry => segmentedBodyActionClip != null &&
            segmentedBodyFrameCount == 18 && steps != null && steps.Length == 3 &&
            steps[0] != null && steps[0].HasBodySegment(0, 5, segmentedBodyFrameCount) &&
            steps[1] != null && steps[1].HasBodySegment(6, 11, segmentedBodyFrameCount) &&
            steps[2] != null && steps[2].HasBodySegment(12, 17, segmentedBodyFrameCount);
    }

    [System.Serializable]
    public sealed class SkillComboStep
    {
        [SerializeField] private int comboIndex;
        [SerializeField] private SkillHitSO hit;
        [SerializeField] private AnimationClip bodyActionClip;
        [SerializeField] private int bodySegmentStartFrame = -1;
        [SerializeField] private int bodySegmentEndFrame = -1;
        [SerializeField] private AnimationClip visualClip;
        [SerializeField] private SkillAnimationVfxProfileSO vfxProfileOverride;
        [SerializeField, Min(0f)] private float minimumVisualLifetime;
        [SerializeField] private SpritePresentationCalibrationProfileSO bodyPresentationCalibration;
        [SerializeField] private SpritePresentationCalibrationProfileSO vfxPresentationCalibration;
        [SerializeField, Min(0f)] private float startTime;
        [SerializeField, Min(0f)] private float hitTime;
        [SerializeField, Min(0f)] private float activeEnd;
        [SerializeField, Min(0f)] private float recoveryEnd;
        [SerializeField, Range(0f, 1f)] private float damageWeight;
        [SerializeField, Min(0f)] private float lungeDistance;

        public int ComboIndex => comboIndex;
        public SkillHitSO Hit => hit;
        public AnimationClip BodyActionClip => bodyActionClip;
        public int BodySegmentStartFrame => bodySegmentStartFrame;
        public int BodySegmentEndFrame => bodySegmentEndFrame;
        public AnimationClip VfxClip => visualClip;
        public AnimationClip VisualClip => visualClip;
        public SkillAnimationVfxProfileSO VfxProfileOverride => vfxProfileOverride;
        public float MinimumVisualLifetime => minimumVisualLifetime;
        public SpritePresentationCalibrationProfileSO BodyPresentationCalibration => bodyPresentationCalibration;
        public SpritePresentationCalibrationProfileSO VfxPresentationCalibration => vfxPresentationCalibration;
        public float StartTime => startTime;
        public float HitTime => hitTime;
        public float ActiveEnd => activeEnd;
        public float RecoveryEnd => recoveryEnd;
        public float DamageWeight => damageWeight;
        public float LungeDistance => lungeDistance;
        public bool IsComplete(int expectedIndex) => comboIndex == expectedIndex && hit != null &&
            startTime <= hitTime && hitTime <= activeEnd && activeEnd <= recoveryEnd;

        public bool HasBodySegment(int expectedStart, int expectedEnd, int totalFrames) =>
            bodySegmentStartFrame == expectedStart && bodySegmentEndFrame == expectedEnd &&
            bodySegmentStartFrame >= 0 && bodySegmentEndFrame < totalFrames &&
            bodySegmentEndFrame - bodySegmentStartFrame == 5;
    }
}
