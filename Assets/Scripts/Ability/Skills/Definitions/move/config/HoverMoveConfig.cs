

using System;
using UnityEngine;
using Skills.Dto.Move;
using Skill;
namespace Skills.Move.Config
{
    [Serializable]
    public class HoverMoveConfig : SkillMoveConfig
    {
        [Header("Follow")]
        public Vector2 followOffset = Vector2.zero;

        public override ProjectileMoveType MoveType => ProjectileMoveType.Hover;

        public override SkillMoveRuntimeDto CreateMoveDto(
            Transform targetTransform,
            Vector2 startPosition,
            Vector2 targetPosition)
        {
            return new HoverProjectileMoveDto
            {
                followOffset = followOffset
            };
        }
    }
}