using Skill;

namespace Skills.Dto.Move
{
    public abstract class SkillMoveRuntimeDto
    {
        public bool applyDirectionRotation;
        public float rotationOffset;
        public abstract ProjectileMoveType MoveType { get; }

    }
}