using UnityEngine;

[CreateAssetMenu(
    fileName = "SkillMove",
    menuName = "BS/Skills/Move/Skill Move SO",
    order = 15)]
public class SkillMoveSO : ScriptableObject
{
    [Header("Move Type")]
    [SerializeField] private SkillProjectileMoveDto.MoveType moveType = SkillProjectileMoveDto.MoveType.Linear;

    [Header("Linear / Common")]
    [SerializeField, Min(0f)] private float speed = 1f;
    [SerializeField, Min(0f)] private float arrivalThreshold = 0.01f;

    [Header("Rotation")]
    [SerializeField] private bool applyDirectionRotation;
    [SerializeField] private float rotationOffset;

    [Header("Hover / Follow")]
    [SerializeField] private Vector2 followOffset = Vector2.zero;
    [SerializeField, Min(0f)] private float followLerpSpeed = 12f;
    [SerializeField] private bool snapOnInitialize = true;

    [Header("Hover Motion")]
    [SerializeField] private bool useHoverMotion = true;
    [SerializeField, Min(0f)] private float hoverAmplitude = 0.15f;
    [SerializeField, Min(0f)] private float hoverFrequency = 2.5f;
    [SerializeField] private Vector2 hoverAxis = Vector2.up;

    [Header("Hover Behavior")]
    [SerializeField] private bool endWhenOwnerMissing = true;

    [Header("Orbit")]
    [SerializeField, Min(0f)] private float orbitRadius = 1.5f;
    [SerializeField] private float orbitAngularSpeed = 180f;
    [SerializeField] private bool clockwise = false;

    [Header("Orbit Layout")]
    [SerializeField] private int spawnOrder = 0;
    [SerializeField, Min(1)] private int maxProjectileCount = 1;
    [SerializeField] private bool resetPhaseWhenLayoutChanges = true;

    [Header("Orbit Pulse")]
    [SerializeField] private bool useRadialPulse = false;
    [SerializeField, Min(0f)] private float radialPulseAmplitude = 0f;
    [SerializeField, Min(0f)] private float radialPulseFrequency = 0f;

    public SkillProjectileMoveDto.MoveType MoveType => moveType;
    public float Speed => speed;
    public float ArrivalThreshold => arrivalThreshold;
    public bool ApplyDirectionRotation => applyDirectionRotation;
    public float RotationOffset => rotationOffset;

    public Vector2 FollowOffset => followOffset;
    public float FollowLerpSpeed => followLerpSpeed;
    public bool SnapOnInitialize => snapOnInitialize;
    public bool UseHoverMotion => useHoverMotion;
    public float HoverAmplitude => hoverAmplitude;
    public float HoverFrequency => hoverFrequency;
    public Vector2 HoverAxis => hoverAxis;
    public bool EndWhenOwnerMissing => endWhenOwnerMissing;

    public float OrbitRadius => orbitRadius;
    public float OrbitAngularSpeed => orbitAngularSpeed;
    public bool Clockwise => clockwise;
    public int SpawnOrder => spawnOrder;
    public int MaxProjectileCount => maxProjectileCount;
    public bool ResetPhaseWhenLayoutChanges => resetPhaseWhenLayoutChanges;
    public bool UseRadialPulse => useRadialPulse;
    public float RadialPulseAmplitude => radialPulseAmplitude;
    public float RadialPulseFrequency => radialPulseFrequency;

    public SkillProjectileMoveDto CreateDto(Transform targetTransform, Vector2 startPosition, Vector2 targetPosition)
    {
        return new SkillProjectileMoveDto
        {
            moveType = moveType,
            targetTransform = targetTransform,
            startPosition = startPosition,
            targetPosition = targetPosition,
            speed = Mathf.Max(0f, speed),
            arrivalThreshold = Mathf.Max(0.0001f, arrivalThreshold),
            applyDirectionRotation = applyDirectionRotation,
            rotationOffset = rotationOffset,

            followOffset = followOffset,
            followLerpSpeed = Mathf.Max(0f, followLerpSpeed),
            snapOnInitialize = snapOnInitialize,
            useHoverMotion = useHoverMotion,
            hoverAmplitude = Mathf.Max(0f, hoverAmplitude),
            hoverFrequency = Mathf.Max(0f, hoverFrequency),
            hoverAxis = hoverAxis,
            endWhenOwnerMissing = endWhenOwnerMissing,

            orbitRadius = Mathf.Max(0f, orbitRadius),
            orbitAngularSpeed = orbitAngularSpeed,
            clockwise = clockwise,
            spawnOrder = Mathf.Max(0, spawnOrder),
            maxProjectileCount = Mathf.Max(1, maxProjectileCount),
            resetPhaseWhenLayoutChanges = resetPhaseWhenLayoutChanges,
            useRadialPulse = useRadialPulse,
            radialPulseAmplitude = Mathf.Max(0f, radialPulseAmplitude),
            radialPulseFrequency = Mathf.Max(0f, radialPulseFrequency)
        };
    }
}