

using System;
using UnityEngine;
using Skills.Dto.Move;

namespace Skills.Move.Config
{
    [Serializable]
    public class HoverMoveConfig : SkillMoveConfig
    {
        [Header("Follow")]
        public Vector2 followOffset = Vector2.zero;

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