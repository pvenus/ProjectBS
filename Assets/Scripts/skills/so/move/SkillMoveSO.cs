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

        return moveConfig.CreateMoveDto(
            targetTransform,
            startPosition,
            targetPosition);
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