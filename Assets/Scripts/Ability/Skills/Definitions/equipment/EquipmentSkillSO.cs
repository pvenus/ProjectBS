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

        [Header("Optional Mouse3 Control")]
        [SerializeField] private Mouse3SkillProfile mouse3Profile = new();

        [Header("Optional Active Input Mode")]
        [SerializeField] private ActiveInputModeProfile activeInputMode = new();

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
        public Mouse3SkillProfile Mouse3Profile => mouse3Profile;
        public ActiveInputModeProfile ActiveInputMode => activeInputMode;

        public float EvaluateBrainScore(object context, int roleBias = 0)
        {
            return baseProfileSo.BasePriority + roleBias;
        }
    }

    [System.Serializable]
    public sealed class ActiveInputModeProfile
    {
        [SerializeField] private bool enabled;
        [SerializeField] private string inputAction;
        [SerializeField, Min(0f)] private float readyDuration;
        [SerializeField, Min(0f)] private float actionDuration;
        [SerializeField, Min(0f)] private float contactTime;
        [SerializeField, Min(1)] private int maxInputs = 1;
        [SerializeField, Min(0f)] private float minInterval;
        [SerializeField, Min(0)] private int bufferCapacity;
        [SerializeField] private string cooldownCommit;
        [SerializeField] private string aimSnapshot;
        [SerializeField] private SkillHitSO actionHit;

        public bool Enabled => enabled && actionHit != null;
        public string InputAction => inputAction;
        public float ReadyDuration => Mathf.Max(0f, readyDuration);
        public float ActionDuration => Mathf.Max(0f, actionDuration);
        public float ContactTime => Mathf.Clamp(contactTime, 0f, ActionDuration);
        public int MaxInputs => Mathf.Max(1, maxInputs);
        public float MinInterval => Mathf.Max(0f, minInterval);
        public int BufferCapacity => Mathf.Max(0, bufferCapacity);
        public string CooldownCommit => cooldownCommit;
        public string AimSnapshot => aimSnapshot;
        public SkillHitSO ActionHit => actionHit;
    }

    [System.Serializable]
    public sealed class Mouse3SkillProfile
    {
        [SerializeField] private string inputAction;
        [SerializeField] private string stableSlotKey;
        [SerializeField] private bool edgeOnly;
        [SerializeField] private string crowdControlKind;
        [SerializeField, Min(0f)] private float distance;
        [SerializeField, Min(0f)] private float duration;
        [SerializeField, Min(0f)] private float stopRadius;
        [SerializeField] private bool collisionSafe;
        [SerializeField, Min(0f)] private float normalDurationOrRatio;
        [SerializeField, Min(0f)] private float eliteDurationOrRatio;
        [SerializeField, Min(0f)] private float bossDurationOrRatio;
        [SerializeField, Min(0f)] private float bossHardCap;
        [SerializeField, Min(0f)] private float fanAngle;
        [SerializeField, Min(1)] private int burstCount = 1;
        [SerializeField, Min(0f)] private float burstInterval;
        [SerializeField, Min(0f)] private float nextBasicForwardRatio;
        [SerializeField, Min(0f)] private float nextBasicRangeRatio;
        [SerializeField, Min(0f)] private float nextBasicInputGrace;
        [SerializeField, Min(0f)] private float gatherDistance;
        [SerializeField, Min(0f)] private float gatherDuration;
        [SerializeField, Min(0f)] private float stunNormalDuration;
        [SerializeField, Min(0f)] private float stunEliteDuration;
        [SerializeField, Min(0f)] private float stunBossDuration;
        [SerializeField] private AnimationClip followupBodyClip;
        [SerializeField] private AnimationClip followupVfxClip;

        public string InputAction => inputAction;
        public string StableSlotKey => stableSlotKey;
        public bool EdgeOnly => edgeOnly;
        public string CrowdControlKind => crowdControlKind;
        public float Distance => distance;
        public float Duration => duration;
        public float StopRadius => stopRadius;
        public bool CollisionSafe => collisionSafe;
        public float NormalDurationOrRatio => normalDurationOrRatio;
        public float EliteDurationOrRatio => eliteDurationOrRatio;
        public float BossDurationOrRatio => bossDurationOrRatio;
        public float BossHardCap => bossHardCap;
        public float FanAngle => fanAngle;
        public int BurstCount => Mathf.Max(1, burstCount);
        public float BurstInterval => Mathf.Max(0f, burstInterval);
        public float NextBasicForwardRatio => Mathf.Max(0f, nextBasicForwardRatio);
        public float NextBasicRangeRatio => Mathf.Max(0f, nextBasicRangeRatio);
        public float NextBasicInputGrace => Mathf.Max(0f, nextBasicInputGrace);
        public float GatherDistance => Mathf.Max(0f, gatherDistance);
        public float GatherDuration => Mathf.Max(0f, gatherDuration);
        public float StunNormalDuration => Mathf.Max(0f, stunNormalDuration);
        public float StunEliteDuration => Mathf.Max(0f, stunEliteDuration);
        public float StunBossDuration => Mathf.Max(0f, stunBossDuration);
        public AnimationClip FollowupBodyClip => followupBodyClip;
        public AnimationClip FollowupVfxClip => followupVfxClip;
        public bool Enabled => !string.IsNullOrWhiteSpace(stableSlotKey);
    }

    [System.Serializable]
    public sealed class SkillComboProfile
    {
        [SerializeField] private bool enabled;
        [SerializeField] private bool inputDriven;
        [SerializeField, Min(0f)] private float duration;
        [SerializeField, Min(0f)] private float totalLungeCap;
        [SerializeField] private bool useCanonicalBodyChoreography;
        [SerializeField] private AnimationClip segmentedBodyActionClip;
        [SerializeField, Min(1)] private int segmentedBodyFrameCount = 18;
        [SerializeField] private SkillComboStep[] steps = System.Array.Empty<SkillComboStep>();

        public bool Enabled => enabled;
        public bool InputDriven => inputDriven;
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
        [SerializeField, Min(0f)] private float postActionHoldTime;
        [SerializeField, Range(0f, 1f)] private float damageWeight;
        [SerializeField, Min(0f)] private float lungeDistance;
        [SerializeField, Min(0f)] private float nextComboActivationTime;
        [SerializeField, Min(0f)] private float gatherDistance;
        [SerializeField, Min(0f)] private float gatherDuration;
        [SerializeField, Min(0f)] private float gatherStopRadius;
        [SerializeField, Min(0f)] private float gatherBossHardCap;

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
        public float PostActionHoldTime => Mathf.Max(0f, postActionHoldTime);
        public float DamageWeight => damageWeight;
        public float LungeDistance => lungeDistance;
        public float NextComboActivationTime => Mathf.Max(0f, nextComboActivationTime);
        public float GatherDistance => Mathf.Max(0f, gatherDistance);
        public float GatherDuration => Mathf.Max(0f, gatherDuration);
        public float GatherStopRadius => Mathf.Max(0f, gatherStopRadius);
        public float GatherBossHardCap => Mathf.Max(0f, gatherBossHardCap);
        public bool IsComplete(int expectedIndex) => comboIndex == expectedIndex && hit != null &&
            startTime <= hitTime && hitTime <= activeEnd && activeEnd <= recoveryEnd;

        public bool HasBodySegment(int expectedStart, int expectedEnd, int totalFrames) =>
            bodySegmentStartFrame == expectedStart && bodySegmentEndFrame == expectedEnd &&
            bodySegmentStartFrame >= 0 && bodySegmentEndFrame < totalFrames &&
            bodySegmentEndFrame - bodySegmentStartFrame == 5;
    }
}
