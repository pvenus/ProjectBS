

using Skill;
using Skills.Dto.Move;
using UnityEngine;

namespace Skills.Move.Config
{
    [System.Serializable]
    public class OrbitMoveConfig : SkillMoveConfig
    {
        public float orbitRadius = 1.5f;
        public float orbitAngularSpeed = 180f;
        public bool clockwise;

        public int spawnOrder;
        public int maxProjectileCount = 1;

        public bool resetPhaseWhenLayoutChanges = true;

        public float radialPulseAmplitude;
        public float radialPulseFrequency;

        public override ProjectileMoveType MoveType => ProjectileMoveType.Orbit;

        public override SkillMoveRuntimeDto CreateMoveDto(
            Transform targetTransform,
            Vector2 startPosition,
            Vector2 targetPosition)
        {
            return new OrbitProjectileMoveDto
            {
                orbitRadius = orbitRadius,
                orbitAngularSpeed = orbitAngularSpeed,
                clockwise = clockwise,
                spawnOrder = spawnOrder,
                maxProjectileCount = maxProjectileCount,
                resetPhaseWhenLayoutChanges = resetPhaseWhenLayoutChanges,
                radialPulseAmplitude = radialPulseAmplitude,
                radialPulseFrequency = radialPulseFrequency
            };
        }
    }
}