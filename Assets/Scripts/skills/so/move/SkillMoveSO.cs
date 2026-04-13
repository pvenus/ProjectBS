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

    public SkillProjectileMoveDto.MoveType MoveType => moveType;
    public float Speed => speed;
    public float ArrivalThreshold => arrivalThreshold;
    public bool ApplyDirectionRotation => applyDirectionRotation;
    public float RotationOffset => rotationOffset;

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
            rotationOffset = rotationOffset
        };
    }
}