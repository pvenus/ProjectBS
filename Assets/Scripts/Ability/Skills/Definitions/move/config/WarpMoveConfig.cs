using System;
using UnityEngine;
using Skills.Dto.Move;
using Skill;
namespace Skills.Move.Config
{
    [Serializable]
    public class WarpMoveConfig : SkillMoveConfig
    {
        public override ProjectileMoveType MoveType => ProjectileMoveType.Warp;

        public override SkillMoveRuntimeDto CreateMoveDto(
            Transform targetTransform,
            Vector2 startPosition,
            Vector2 targetPosition)
        {
            return new WarpProjectileMoveDto
            {
                targetPosition = targetPosition
            };
        }
    }
}
