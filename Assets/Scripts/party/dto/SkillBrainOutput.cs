using UnityEngine;

[System.Serializable]
public struct SkillBrainOutput
{
    public ScriptableObject skill;
    public SkillOutputTargetMode targetMode;
    public Transform target;
    public Vector3 point;
    public float score;
    public string reason;

    public static SkillBrainOutput None => new SkillBrainOutput
    {
        skill = null,
        targetMode = SkillOutputTargetMode.None,
        target = null,
        point = Vector3.zero,
        score = float.NegativeInfinity,
        reason = string.Empty
    };

    public bool HasSkill => skill != null;
}