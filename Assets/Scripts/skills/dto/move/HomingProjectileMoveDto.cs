using Skill;
namespace Skills.Dto.Move
{
    public class HomingProjectileMoveDto : SkillMoveRuntimeDto
    {
        public float speed;
        public float turnSpeed;

        public override ProjectileMoveType MoveType => ProjectileMoveType.Homing;
    }
}