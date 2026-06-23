using UnityEngine;
using Skill;

namespace Skills.Dto.Move
{
    public abstract class SkillMoveRuntimeDto
    {
        public bool applyDirectionRotation;
        public float rotationOffset;
        public Vector2 targetPosition;
        public abstract ProjectileMoveType MoveType { get; }

    }
}