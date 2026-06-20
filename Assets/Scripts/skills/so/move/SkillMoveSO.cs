using UnityEngine;
using Skills.Move.Config;
using Skills.Dto.Move;

[CreateAssetMenu(
    fileName = "SkillMove",
    menuName = "BS/Skills/Move/Skill Move SO",
    order = 15)]
public class SkillMoveSO : ScriptableObject
{
    [Header("Move Type")]
    [SerializeField] private SkillProjectileMoveDto.MoveType moveType = SkillProjectileMoveDto.MoveType.Linear;

    [Header("Config")]
    [SerializeReference] private SkillMoveConfig config;

    [Header("Rotation")]
    [SerializeField] private bool applyDirectionRotation;
    [SerializeField] private float rotationOffset;

    [Header("Orbit")]
    [SerializeField, Min(0f)] private float orbitRadius = 1.5f;
    [SerializeField] private float orbitAngularSpeed = 180f;
    [SerializeField] private bool clockwise = false;

    [Header("Orbit Layout")]
    [SerializeField] private int spawnOrder = 0;
    [SerializeField] private bool resetPhaseWhenLayoutChanges = true;

    [Header("Orbit Pulse")]
    [SerializeField] private bool useRadialPulse = false;
    [SerializeField, Min(0f)] private float radialPulseAmplitude = 0f;
    [SerializeField, Min(0f)] private float radialPulseFrequency = 0f;

    public SkillProjectileMoveDto.MoveType MoveType => moveType;
    public bool ApplyDirectionRotation => applyDirectionRotation;
    public float RotationOffset => rotationOffset;

    public float OrbitRadius => orbitRadius;
    public float OrbitAngularSpeed => orbitAngularSpeed;
    public bool Clockwise => clockwise;
    public int SpawnOrder => spawnOrder;
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
            speed = 0f,
            arrivalThreshold = 0.0001f,
            applyDirectionRotation = applyDirectionRotation,
            rotationOffset = rotationOffset,

            orbitRadius = Mathf.Max(0f, orbitRadius),
            orbitAngularSpeed = orbitAngularSpeed,
            clockwise = clockwise,
            spawnOrder = Mathf.Max(0, spawnOrder),
            resetPhaseWhenLayoutChanges = resetPhaseWhenLayoutChanges,
            useRadialPulse = useRadialPulse,
            radialPulseAmplitude = Mathf.Max(0f, radialPulseAmplitude),
            radialPulseFrequency = Mathf.Max(0f, radialPulseFrequency)
        };
    }

    public SkillMoveRuntimeDto CreateMoveRuntimeDto(
        Transform targetTransform,
        Vector2 startPosition,
        Vector2 targetPosition)
    {
        if (config != null)
        {
            return config.CreateMoveDto(
                targetTransform,
                startPosition,
                targetPosition);
        }

        return null;
    }

    public LinearProjectileMoveDto CreateLinearProjectileMoveDto(
        Transform targetTransform,
        Vector2 startPosition,
        Vector2 targetPosition)
    {
        if (config is LinearMoveConfig linearConfig)
            return linearConfig.CreateMoveDto(targetTransform, startPosition, targetPosition) as LinearProjectileMoveDto;

        return null;
    }
}