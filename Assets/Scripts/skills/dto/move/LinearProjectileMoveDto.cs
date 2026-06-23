using UnityEngine;
using Skill;

namespace Skills.Dto.Move
{
    public class LinearProjectileMoveDto : SkillMoveRuntimeDto
    {
        public override ProjectileMoveType MoveType => ProjectileMoveType.Linear;
        public Vector2 startPosition;
        public float speed;
    }
}