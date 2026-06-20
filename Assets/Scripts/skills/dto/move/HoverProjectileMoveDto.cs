

using UnityEngine;

namespace Skills.Dto.Move
{
    public class HoverProjectileMoveDto : SkillMoveRuntimeDto
    {
        public override ProjectileMoveType MoveType => ProjectileMoveType.Hover;

        public Vector2 followOffset = Vector2.zero;
    }
}