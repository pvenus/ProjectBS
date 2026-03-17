using UnityEngine;

public struct SkillExecutionRequest
{
    public ScriptableObject Skill;
    public Transform Caster;
    public Transform Target;
    public Vector3 TargetPoint;
    public bool UseTarget;
    public bool UsePoint;
}