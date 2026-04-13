using UnityEngine;
public class SkillProjectileWarpMovementDto
{
    public Transform targetTransform;
    public Vector2 startPosition;
    public Vector2 targetPosition;
    public Vector2 direction;
    public float arrivalThreshold = 0.01f;
}