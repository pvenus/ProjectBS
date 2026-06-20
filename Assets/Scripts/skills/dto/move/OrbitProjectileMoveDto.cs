using Skill;

namespace Skills.Dto.Move
{
    public class OrbitProjectileMoveDto : SkillMoveRuntimeDto
    {
        public float orbitRadius;
        public float orbitAngularSpeed;
        public bool clockwise;

        public int spawnOrder;
        public int maxProjectileCount;

        public bool resetPhaseWhenLayoutChanges;

        public float radialPulseAmplitude;
        public float radialPulseFrequency;

        public override ProjectileMoveType MoveType => ProjectileMoveType.Orbit;
    }
}