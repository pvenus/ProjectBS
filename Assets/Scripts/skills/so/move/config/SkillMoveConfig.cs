using System;
using UnityEngine;
using Skills.Dto.Move;
using Skill;
namespace Skills.Move.Config
{
    [Serializable]
    public abstract class SkillMoveConfig
    {
        public abstract ProjectileMoveType MoveType { get; }
        public abstract SkillMoveRuntimeDto CreateMoveDto(
            Transform targetTransform,
            Vector2 startPosition,
            Vector2 targetPosition);
    }
}