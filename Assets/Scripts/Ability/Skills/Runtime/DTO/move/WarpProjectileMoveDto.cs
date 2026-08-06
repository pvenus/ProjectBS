using UnityEngine;
using Skill;

namespace Skills.Dto.Move
{
    public class WarpProjectileMoveDto : SkillMoveRuntimeDto
    {
        public Vector2 targetPosition;

        public override ProjectileMoveType MoveType => ProjectileMoveType.Warp;
    }
}