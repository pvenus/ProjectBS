using System;
using UnityEngine;
using Skills.Dto.Move;

namespace Skills.Move.Config
{
    [Serializable]
    public class LinearMoveConfig : SkillMoveConfig
    {
        [Header("Movement")]
        public float speed = 1f;

        public override SkillMoveRuntimeDto CreateMoveDto(
            Transform targetTransform,
            Vector2 startPosition,
            Vector2 targetPosition)
        {
            return new LinearProjectileMoveDto
            {
                startPosition = startPosition,
                targetPosition = targetPosition,
                speed = Mathf.Max(0f, speed)
            };
        }
    }
}