

using UnityEngine;
using Skill;
using Skills.Dto.Move;

namespace Skills.Move.Config
{
    [System.Serializable]
    public class HomingMoveConfig : SkillMoveConfig
    {
        public float speed = 8f;
        public float turnSpeed = 180f;

        public override ProjectileMoveType MoveType => ProjectileMoveType.Homing;

        public override SkillMoveRuntimeDto CreateMoveDto(
            Transform targetTransform,
            Vector2 startPosition,
            Vector2 targetPosition)
        {
            return new HomingProjectileMoveDto
            {
                speed = speed,
                turnSpeed = turnSpeed
            };
        }
    }
}