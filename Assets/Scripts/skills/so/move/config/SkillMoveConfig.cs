using System;
using UnityEngine;
using Skills.Dto.Move;

namespace Skills.Move.Config
{
    [Serializable]
    public abstract class SkillMoveConfig
    {
        public abstract SkillMoveRuntimeDto CreateMoveDto(
            Transform targetTransform,
            Vector2 startPosition,
            Vector2 targetPosition);
    }
}