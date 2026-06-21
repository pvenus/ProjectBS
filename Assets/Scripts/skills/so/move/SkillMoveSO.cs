using UnityEngine;
using Skills.Move.Config;
using Skill;
using Skills.Dto.Move;

[CreateAssetMenu(
    fileName = "SkillMove",
    menuName = "BS/Skills/Move/Skill Move SO",
    order = 15)]
public class SkillMoveSO : ScriptableObject
{
    [Header("Move Type")]
    [SerializeField] private ProjectileMoveType moveType = ProjectileMoveType.Linear;

    [Header("Config")]
    [SerializeReference] private SkillMoveConfig config;

    [Header("Rotation")]
    [SerializeField] private bool applyDirectionRotation = true;
    [SerializeField] private float rotationOffset;

    public ProjectileMoveType MoveType => moveType;

    public SkillMoveRuntimeDto CreateMoveRuntimeDto(
        Transform targetTransform,
        Vector2 startPosition,
        Vector2 targetPosition)
    {
        SkillMoveConfig moveConfig = config ?? CreateDefaultConfig(moveType);

        if (moveConfig == null)
        {
            return null;
        }

        SkillMoveRuntimeDto runtimeDto = moveConfig.CreateMoveDto(
            targetTransform,
            startPosition,
            targetPosition);

        if (runtimeDto != null)
        {
            runtimeDto.applyDirectionRotation = applyDirectionRotation;
            runtimeDto.rotationOffset = rotationOffset;
        }

        return runtimeDto;
    }

    private static SkillMoveConfig CreateDefaultConfig(ProjectileMoveType moveType)
    {
        return moveType switch
        {
            ProjectileMoveType.Linear => new LinearMoveConfig(),
            ProjectileMoveType.Hover => new HoverMoveConfig(),
            ProjectileMoveType.Warp => new WarpMoveConfig(),
            ProjectileMoveType.Homing => new HomingMoveConfig(),
            ProjectileMoveType.Orbit => new OrbitMoveConfig(),
            _ => null
        };
    }
}